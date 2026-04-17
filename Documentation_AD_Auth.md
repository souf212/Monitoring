# Documentation du Projet : KTC Web (AD-Auth)

Ce document décrit le fonctionnement complet de l'architecture d'authentification mise en place pour le projet **KTC Web**, intégrant un backend ASP.NET Core lié à un serveur Active Directory (AD DS) et un frontend Angular moderne (Zoneless).

---

## 1. Vue d'Ensemble de l'Architecture

### Le Flux d'Authentification (Comment ça marche) :
1. **L'utilisateur** ouvre l'application Angular (Frontend) et entre ses identifiants.
2. Angular envoie une requête HTTP POST au **Backend ASP.NET Core** (`/api/auth/login`).
3. Le Backend utilise `PrincipalContext.ValidateCredentials` pour interroger directement le **Contrôleur de Domaine (Active Directory)** hébergé sur une Machine Virtuelle (Hyper-V).
4. **Si valide :** Le Backend génère un token cryptographique sécurisé (JWT - Json Web Token) incluant le nom de l'utilisateur et tous les **Rôles** extrés depuis les "Groupes" AD de l'utilisateur.
5. Angular reçoit et stocke ce JWT dans le `localStorage`.
6. L'application Angular **décode** nativement ce Token pour lire les rôles de l'utilisateur et adapter l'interface.
7. Pour chaque appel futur au Backend (ex: obtenir des données métiers), un **Interceptor Angular** ajoutera automatiquement ce Token dans les requêtes (`Authorization: Bearer <TKN>`). Le Backend acceptera ou bloquera l'accès grâce à `[Authorize]`.

---

## 2. Infrastructure et Prérequis

- **Hyper-V (Virtualisation) :** Un commutateur réseau "Interne" permet au Laptop (Hôte) de communiquer avec la VM Windows Server.
- **Windows Server (Serveur) :**
  - IP Fixe : `192.168.100.11` (Le Serveur DNS Préféré est `127.0.0.1`).
  - Active Directory Domain Services (AD DS) installé.
  - Domaine Racine créé : `ATM.LOCAL`.
  - Utilisateur de test créé (ex: `testuser` sans partie de mdp dans l'identifiant pour respecter les politiques complexes AD).
- **Laptop (Hôte) :**
  - IP Fixe sur la carte vEthernet : `192.168.100.15`.
  - DNS Pointeur : L'IP du DNS de la carte vEthernet pointe vers le Serveur (`192.168.100.11`), ET/OU le fichier `C:\Windows\System32\drivers\etc\hosts` lie manuellement l'IP à `ATM.LOCAL`.

---

## 3. Ce que contient le projet (Le Code)

### A. Le Backend (`/AD-Auth/Backend`)
Développé en **C# (.NET 8)**, ce composant gère la sécurité.
* **`ActiveDirectoryService.cs`** : Classe métier vitale. Elle se connecte à Active Directory via `System.DirectoryServices.AccountManagement`. Elle valide le mot de passe (en tentant `Kerberos Negotiate` d'abord, puis du repli si l'hôte n'est pas dans le domaine) et extrait les groupes de l'utilisateur pour les convertir en Rôles métiers.
* **`appsettings.Development.json`** : Contient le `DomainName` ("ATM.LOCAL") qui définit où l'AD doit être interrogé. Contient la clé secrète du JWT.
* **`Program.cs`** : Injecte le service AD, configure le Bearer JWT (Validation de l'Issuer et clé d'authentification), et ajoute la politique **CORS** qui autorise `http://localhost:4200` (Angular) à discuter avec le backend sans être bloqué par les navigateurs web.

### B. Le Frontend (`/ktc-frontend`)
Développé en **Angular 21 (v19/v21.1)**, configuré de manière extrêmement moderne :
* **Standalone Components :** Plus de `app.module.ts`. 
* **Zoneless (`app.config.ts`) :** Utilisation de `provideZonelessChangeDetection()` avec les `Signals`. Cela rend l'application deux fois plus rapide en évitant l'usage de Zone.js.
* **Architecture :**
  - **`core/services/auth.service.ts`** : Le service de communication. Il utilise le typage réactif (`signal`) pour savoir instantanément, n'importe où dans l'application, si l'utilisateur est connecté, plutôt que d'attendre l'état.
  - **`core/interceptors/auth.interceptor.ts`** : Assure la sécurisation transparente de toutes les requêtes futures d'un coup.
  - **`core/guards/auth.guard.ts`** : L'agent de sécurité d'Angular. Si on essaie de taper l'URL `/dashboard` manuellement dans le navigateur sans être connecté, il renvoie à la page Login.
  - **`features/auth/login.component.ts`** : Interface de connexion avec *Angular Reactive Forms* (Vérification et sécurité côté utilisateur, ex: afficher un message si le mot de passe est vide). Design via *TailwindCSS*.
  - **`features/dashboard/dashboard.component.ts`** : Tableau de bord qui utilise la logique `authService.currentUserRoles()` pour lire le JWT décodé et lister à l'utilisateur ce qu'il a le droit de faire en fonction d'Active Directory.

---

## 4. Comment démarrer et travailler sur ce projet

1. **Allumer le Serveur AD :** Dans Hyper-V, démarrez la machine `Windows Server` contenant le domaine.
2. **Lancer le Backend (.NET) :**
   - Ouvrez la solution `.sln` et cliquez sur "Start" (Ou exécutez `dotnet run` dans le dossier `Backend`).
3. **Lancer le Frontend (Angular) :**
   - Ouvrez un terminal dans le dossier `/ktc-frontend`.
   - Exécutez `npm start` ou `ng serve`.
4. **Utilisation :**
   - Naviguez vers `http://localhost:4200/`.
   - Vous verrez la page Login. Entrez vos identifiants AD (ex: `testuser@ATM.LOCAL`).

*(Note: Concernant votre question sur les erreurs : les fichiers générés ont tous été vérifiés, les dépendances d'Angular 21 concernant l'anomalie `Zone.js` ont été fixées (Zoneless activé). Il n'y a plus aucune erreur de compilation ou de Runtime).*
