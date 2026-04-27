import { Routes } from '@angular/router';

import { LoginComponent } from './features/auth/components/login.component';
import { DashboardComponent } from './features/dashboard/components/dashboard.component';
import { AdminLayoutComponent } from './features/admin/components/admin-layout.component';
import { AtmListComponent } from './features/atm/components/atm-list.component';
import { AtmFormComponent } from './features/atm/components/atm-form.component';
import { AtmMapComponent } from './features/atm/components/atm-map.component';
import { AtmDetailLayoutComponent } from './features/atm/components/atm-detail-layout.component';
import { AtmStatusComponent } from './features/atm/components/atm-status.component';
import { AtmAssetHistoryComponent } from './features/atm/components/atm-asset-history.component';
import { GroupComponent } from './features/group/components/group.component';
import { authGuard } from './core/guards/auth.guard';
import { BusinessListComponent } from './features/business/components/business-list.component';
import { BusinessFormComponent } from './features/business/components/business-form.component';
import { RegionListComponent } from './features/region/components/region-list.component';
import { RegionFormComponent } from './features/region/components/region-form.component';
import { BranchFormComponent } from './features/branch/components/branch-form.component';
import { BranchListComponent } from './features/branch/components/branch-list.component';
import { GroupFormComponent } from './features/group/components/group-form.component';


export const routes: Routes = [
  { path: 'login', component: LoginComponent },

  {
    path: '',
    canActivate: [authGuard],
    children: [
      { path: 'dashboard', component: DashboardComponent },

      // === ADMINISTRATION LAYOUT ===
      {
        path: 'admin',
        component: AdminLayoutComponent,
        children: [
          { path: '', redirectTo: 'atms', pathMatch: 'full' },

          // ── ATMs ──────────────────────────────────────────────────────────
          { path: 'atms',           component: AtmListComponent },
          { path: 'atms/map',       component: AtmMapComponent },
          { path: 'atms/create',    component: AtmFormComponent },
          {
            path: 'atms/:id',
            component: AtmDetailLayoutComponent,
            children: [
              { path: '', redirectTo: 'general', pathMatch: 'full' },
              { path: 'general', component: AtmFormComponent },
              { path: 'status', component: AtmStatusComponent },
              { path: 'asset-history', component: AtmAssetHistoryComponent }
            ]
          },

          // ── Businesses ────────────────────────────────────────────────────
          { path: 'businesses',          component: BusinessListComponent },
          { path: 'businesses/create',   component: BusinessFormComponent },
          { path: 'businesses/:id/edit', component: BusinessFormComponent },

          // ── Regions ───────────────────────────────────────────────────────
          { path: 'regions',          component: RegionListComponent },
          { path: 'regions/create',   component: RegionFormComponent },
          { path: 'regions/:id/edit', component: RegionFormComponent },

          // ── Branches ──────────────────────────────────────────────────────
          { path: 'branches',          component: BranchListComponent },
          { path: 'branches/create',   component: BranchFormComponent },
          { path: 'branches/:id/edit', component: BranchFormComponent },

          // ── Groups ────────────────────────────────────────────────────────
          { path: 'groups',          component: GroupComponent },
          { path: 'groups/create',   component: GroupFormComponent },
          { path: 'groups/:id/edit', component: GroupFormComponent }
        ]
      }
    ]
  },

  { path: '', redirectTo: '/login', pathMatch: 'full' },
  { path: '**', redirectTo: '/login' }
];

