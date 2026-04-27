import { Component, OnInit, effect, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { AtmService, AtmTicketDto } from '../services/atm.service';

@Component({
  selector: 'app-atm-tickets',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './atm-tickets.component.html',
  styleUrls: ['./atm-tickets.component.css']
})
export class AtmTicketsComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private atmService = inject(AtmService);

  isLoading = signal(true);
  error = signal<string | null>(null);
  tickets = signal<AtmTicketDto[]>([]);

  daysFilter = signal(14);
  statusFilter = signal('Open/Dispatched');
  atmId = signal<number | null>(null);

  constructor() {
    effect(() => {
      const id = this.atmId();
      const days = this.daysFilter();
      const status = this.statusFilter();

      if (!id) {
        return;
      }

      this.fetchTickets(id, days, status);
    });
  }

  ngOnInit(): void {
    const idStr = this.route.snapshot.paramMap.get('id')
      ?? this.route.parent?.snapshot.paramMap.get('id');

    const id = idStr ? Number(idStr) : null;
    if (!id) {
      this.error.set("Aucun identifiant d'ATM fourni.");
      this.isLoading.set(false);
      return;
    }

    this.atmId.set(id);
  }

  setDays(event: Event): void {
    const value = Number((event.target as HTMLInputElement).value);
    this.daysFilter.set(value > 0 ? value : 14);
  }

  setStatus(event: Event): void {
    this.statusFilter.set((event.target as HTMLSelectElement).value);
  }

  private fetchTickets(id: number, days: number, status: string): void {
    this.isLoading.set(true);
    this.error.set(null);

    this.atmService.getAtmTickets(id, days, status).subscribe({
      next: (data) => {
        this.tickets.set(data);
        this.isLoading.set(false);
      },
      error: () => {
        this.error.set("Erreur lors de la récupération des tickets.");
        this.isLoading.set(false);
      }
    });
  }
}
