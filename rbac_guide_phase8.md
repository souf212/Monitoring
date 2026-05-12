# Phase 8 — Guide RBAC Complet : KTC Web


Étape 1 : Créer l'utilisateur
Ouvrir dsa.msc (Utilisateurs et ordinateurs Active Directory) sur le serveur.

Naviguer vers votre unité d'organisation (OU) : ATM.LOCAL > Users.

Clic droit dans l'espace vide → Nouveau → Utilisateur.

Remplir les champs :

Prénom / Nom d'affichage : Support User

Nom d'ouverture de session : support.user (C'est le login à utiliser dans l'application)

Cliquer sur Suivant.

Définir un mot de passe robuste.

⚠️ Étape cruciale : Décocher la case "L'utilisateur doit changer le mot de passe à la prochaine ouverture de session" (Sinon, l'API renverra une erreur 401 Unauthorized).

Cocher "Le mot de passe n'expire jamais" (recommandé pour les comptes de service ou de test).

Cliquer sur Terminer.

Étape 2 : Créer les groupes de sécurité
Toujours dans ATM.LOCAL > Users, faire un Clic droit → Nouveau → Groupe.

Nom du groupe : Support_FullAccess (Ce nom doit correspondre exactement à ce qui est attendu par le backend).

Conserver "Globale" et "Sécurité" cochés, puis valider par OK.

(Optionnel) Répéter l'opération pour créer le groupe Admin_ReadOnly.

Étape 3 : Affecter l'utilisateur au groupe
Double-cliquer sur l'utilisateur Support User fraîchement créé.

Aller dans l'onglet Membre de (Member Of).

Cliquer sur le bouton Ajouter....

Taper Support_FullAccess dans la zone de saisie.

Cliquer sur Vérifier les noms (le nom va se souligner).

Cliquer sur OK, puis sur Appliquer et fermer la fenêtre.


-----

## PARTIE 2 — Backend ASP.NET Core 8

### `Program.cs` — Policies RBAC (déjà configuré ✅)

```csharp
// ─── RBAC Policies ─────────────────────────────────────────────────────────
builder.Services.AddAuthorization(options =>
{
    // RequireReadOnly : Admin_ReadOnly OU Support_FullAccess (les deux peuvent lire)
    options.AddPolicy("RequireReadOnly", policy =>
        policy.RequireAssertion(ctx =>
            ctx.User.IsInRole("Admin_ReadOnly") ||
            ctx.User.IsInRole("Support_FullAccess")));

    // RequireWrite : UNIQUEMENT Support_FullAccess
    options.AddPolicy("RequireWrite", policy =>
        policy.RequireRole("Support_FullAccess"));
});
```

### `ActiveDirectoryService.cs` — JWT avec rôles (déjà configuré ✅)

Les claims sont émis en **double** pour compatibilité avec les décodeurs JWT Angular :

```csharp
// Chaque rôle est ajouté avec les deux clés :
claims.Add(new Claim(ClaimTypes.Role, role));  // URI long .NET
claims.Add(new Claim("role", role));           // Clé courte lisible par Angular
```

### `AtmController.cs` — Pattern de sécurisation (déjà configuré ✅)

```csharp
[ApiController]
[Route("api/atm")]
[Authorize(Policy = "RequireReadOnly")]   // ← Protection lecture : classe entière
public class AtmController : ControllerBase
{
    // GET → hérité de la classe → RequireReadOnly ✅
    [HttpGet("clients")]
    public async Task<ActionResult<List<ClientAtmDto>>> GetAllClients() { ... }

    // POST/PUT/DELETE → protection écriture supplémentaire
    [HttpPost("clients")]
    [Authorize(Policy = "RequireWrite")]   // ← Support_FullAccess uniquement
    public async Task<IActionResult> CreateClient(...) { ... }

    [HttpPut("clients/{id}")]
    [Authorize(Policy = "RequireWrite")]
    public async Task<IActionResult> UpdateClient(...) { ... }

    [HttpDelete("clients/{id}")]
    [Authorize(Policy = "RequireWrite")]
    public async Task<IActionResult> DeleteClient(...) { ... }
}
```

---

## PARTIE 3 — Frontend Angular 18

### `auth.service.ts` — Signals RBAC (déjà configuré ✅)

| Signal/Méthode | Description |
|---|---|
| `isSupport()` | `true` si l'utilisateur a le rôle `Support_FullAccess` |
| `isReadOnly()` | `true` si `Admin_ReadOnly` SANS `Support_FullAccess` |
| `canRead()` | `true` pour les deux rôles (lecture autorisée) |
| `hasRole(['role1'])` | Vérifie la présence d'au moins un rôle |
| `currentUserRoles()` | Tableau de tous les rôles du JWT |

### `has-role.directive.ts` — Directive structurelle (déjà configurée ✅)

```html
<!-- Masque le bouton pour Admin_ReadOnly -->
<button *appHasRole="['Support_FullAccess']" (click)="delete()">
  🗑 Supprimer
</button>

<!-- Visible pour les deux rôles -->
<div *appHasRole="['Admin_ReadOnly', 'Support_FullAccess']">
  Contenu visible en lecture
</div>

<!-- Équivalent avec le nouveau Control Flow Angular 18 -->
@if (authService.isSupport()) {
  <button (click)="save()">Sauvegarder</button>
}
```

### `role.guard.ts` — Route Guard (déjà configuré ✅)

```typescript
// Dans app.routes.ts
{ 
  path: 'atms/create',
  component: AtmFormComponent,
  canActivate: [roleGuard],          // ← bloque Admin_ReadOnly
  data: { roles: ['Support_FullAccess'] }
}
```

**Comportement :**
- `Admin_ReadOnly` tente `/admin/atms/create` → redirigé vers `/access-denied`
- `Support_FullAccess` → accès autorisé ✅

---

## PARTIE 4 — Matrice des droits

| Fonctionnalité | `Admin_ReadOnly` | `Support_FullAccess` |
|---|:---:|:---:|
| Voir toutes les pages | ✅ | ✅ |
| Voir les tableaux de bord | ✅ | ✅ |
| Voir la liste des ATMs | ✅ | ✅ |
| Voir les détails ATM | ✅ | ✅ |
| Bouton **Nouvel ATM** | ❌ masqué | ✅ visible |
| Bouton **Modifier** ATM | ❌ masqué | ✅ visible |
| Bouton **Supprimer** ATM | ❌ masqué | ✅ visible |
| Accès route `/atms/create` | ❌ → 403 | ✅ |
| Accès route `/:id/edit` | ❌ → 403 | ✅ |
| **Actions distantes** (Reboot, etc.) | ❌ masqué | ✅ visible |
| API GET (lecture) | ✅ | ✅ |
| API POST/PUT/DELETE | ❌ 403 | ✅ |

---

## PARTIE 5 — Récapitulatif des fichiers modifiés

### Backend (aucune modification nécessaire — tout déjà en place)
- [Program.cs](file:///c:/Users/souf2/Desktop/monitoring-main/Backend/Program.cs) — Policies `RequireReadOnly` / `RequireWrite`
- [ActiveDirectoryService.cs](file:///c:/Users/souf2/Desktop/monitoring-main/Backend/Services/Implementations/ActiveDirectoryService.cs) — JWT avec double-claim
- [AtmController.cs](file:///c:/Users/souf2/Desktop/monitoring-main/Backend/Controllers/AtmController.cs) — `[Authorize(Policy)]` sur tous les endpoints

### Frontend (modifications appliquées dans cette session)
- [atm-list.component.ts](file:///c:/Users/souf2/Desktop/monitoring-main/ktc-frontend/src/app/features/atm/components/atm-list.component.ts) — Import `AuthService` + `HasRoleDirective`
- [atm-list.component.html](file:///c:/Users/souf2/Desktop/monitoring-main/ktc-frontend/src/app/features/atm/components/atm-list.component.html) — `*appHasRole` sur Nouvel ATM / Modifier / Supprimer
- [atm-list.component.css](file:///c:/Users/souf2/Desktop/monitoring-main/ktc-frontend/src/app/features/atm/components/atm-list.component.css) — Style `.badge-readonly`
- [atm-remote-commands-menu.component.ts](file:///c:/Users/souf2/Desktop/monitoring-main/ktc-frontend/src/app/features/atm/components/atm-remote-commands-menu.component.ts) — Import `AuthService` + `HasRoleDirective`
- [atm-remote-commands-menu.component.html](file:///c:/Users/souf2/Desktop/monitoring-main/ktc-frontend/src/app/features/atm/components/atm-remote-commands-menu.component.html) — `*appHasRole` sur le bouton "Actions distantes"

### Déjà en place (sessions précédentes)
- [auth.service.ts](file:///c:/Users/souf2/Desktop/monitoring-main/ktc-frontend/src/app/core/services/auth.service.ts) — Signals RBAC (`isSupport`, `isReadOnly`, `canRead`, `hasRole`)
- [has-role.directive.ts](file:///c:/Users/souf2/Desktop/monitoring-main/ktc-frontend/src/app/shared/directives/has-role.directive.ts) — Directive `*appHasRole`
- [role.guard.ts](file:///c:/Users/souf2/Desktop/monitoring-main/ktc-frontend/src/app/core/guards/role.guard.ts) — Route Guard RBAC
- [auth.guard.ts](file:///c:/Users/souf2/Desktop/monitoring-main/ktc-frontend/src/app/core/guards/auth.guard.ts) — Guard d'authentification + expiration JWT
- [access-denied.component.ts](file:///c:/Users/souf2/Desktop/monitoring-main/ktc-frontend/src/app/features/auth/components/access-denied.component.ts) — Page 403
- [app.routes.ts](file:///c:/Users/souf2/Desktop/monitoring-main/ktc-frontend/src/app/app.routes.ts) — Routes `/create` et `/:id/edit` protégées par `roleGuard`

---

## PARTIE 6 — Comment appliquer `*appHasRole` sur un nouveau composant

```typescript
// 1. Importer dans le composant TS
import { HasRoleDirective } from '../../../shared/directives/has-role.directive';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  imports: [..., HasRoleDirective],  // 2. Ajouter dans imports[]
})
export class MonComponent {
  readonly auth = inject(AuthService);  // 3. Injecter pour @if Control Flow
}
```

```html
<!-- 4a. Via directive structurelle (masquage DOM complet) -->
<button *appHasRole="['Support_FullAccess']">Action sensible</button>

<!-- 4b. Via Control Flow Angular 18 (lisibilité) -->
@if (auth.isSupport()) {
  <button>Action sensible</button>
}

<!-- 4c. Désactiver sans masquer (accessibility) -->
<button [disabled]="!auth.isSupport()">Action sensible</button>
```

> [!WARNING]
> La directive `*appHasRole` masque l'élément côté **DOM client**. Le backend reste la **barrière de sécurité réelle** via `[Authorize(Policy = "RequireWrite")]`. Ne jamais compter uniquement sur le masquage UI.
