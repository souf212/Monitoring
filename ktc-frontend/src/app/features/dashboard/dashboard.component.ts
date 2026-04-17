import { Component, inject } from '@angular/core';
import { AuthService } from '../../core/services/auth.service';
import { NgFor } from '@angular/common';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [NgFor],
  template: `
    <div class="min-h-screen bg-gray-50">
      <nav class="bg-white shadow">
        <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div class="flex justify-between h-16">
            <div class="flex">
              <div class="flex-shrink-0 flex items-center">
                <span class="font-bold text-xl text-green-600">KTC Dashboard</span>
              </div>
            </div>
            <div class="flex items-center">
              <button (click)="logout()" class="ml-4 px-4 py-2 border border-transparent text-sm font-medium rounded-md text-white bg-red-600 hover:bg-red-700">
                Déconnexion
              </button>
            </div>
          </div>
        </div>
      </nav>

      <main class="max-w-7xl mx-auto py-6 sm:px-6 lg:px-8">
        <div class="px-4 py-6 sm:px-0">
          <div class="border-4 border-dashed border-gray-200 rounded-lg h-96 p-8 bg-white flex flex-col justify-center items-center">
            
            <svg class="h-20 w-20 text-green-500 mb-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z"></path>
            </svg>
            
            <h1 class="text-3xl font-bold text-gray-900 mb-2">Bienvenue !</h1>
            <p class="text-gray-500 mb-6">L'authentification Active Directory a réussi de bout en bout.</p>
            
            <div class="bg-blue-50 w-full max-w-md p-4 rounded text-left border border-blue-200">
              <h3 class="text-sm font-bold text-blue-900 mb-2">Vos Rôles Active Directory :</h3>
              <ul class="list-disc pl-5 text-blue-800">
                <li *ngFor="let role of roles()" class="font-mono text-sm py-1">{{ role }}</li>
              </ul>
            </div>

          </div>
        </div>
      </main>
    </div>
  `
})
export class DashboardComponent {
  private authService = inject(AuthService);
  roles = this.authService.currentUserRoles;

  logout() {
    this.authService.logout();
  }
}

