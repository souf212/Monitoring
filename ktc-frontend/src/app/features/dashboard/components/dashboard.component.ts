import {
  Component, OnInit, OnDestroy, inject, signal, computed, ChangeDetectionStrategy
} from '@angular/core';
import { CommonModule, DecimalPipe, DatePipe, NgClass } from '@angular/common';
import { RouterModule } from '@angular/router';
import { interval, Subscription } from 'rxjs';
import { switchMap, startWith } from 'rxjs/operators';

import { NocDashboardService, NocSummary, AtmStatusRow } from '../services/noc-dashboard.service';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [CommonModule, RouterModule, DecimalPipe, DatePipe, NgClass],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.css']
})
export class DashboardComponent implements OnInit, OnDestroy {
  private nocService  = inject(NocDashboardService);
  private authService = inject(AuthService);

  user  = this.authService.currentUser;

  // ── State ────────────────────────────────────────────────────────────────
  data       = signal<NocSummary | null>(null);
  loading    = signal(true);
  error      = signal<string | null>(null);
  lastUpdate = signal<Date | null>(null);

  // Selected time range (default: last 7 days)
  selectedRange = signal<'1d' | '7d' | '30d'>('7d');

  private sub?: Subscription;

  // ── Computed helpers ─────────────────────────────────────────────────────
  fleet   = computed(() => this.data()?.fleetHealth);
  cash    = computed(() => this.data()?.cashSummary);
  sla     = computed(() => this.data()?.sla);
  statuses = computed(() => this.data()?.atmStatuses ?? []);

  // Donut chart segments for SVG (online/warning/offline/unknown)
  donutSegments = computed(() => {
    const f = this.fleet();
    if (!f || f.totalAtms === 0) return [];
    const total = f.totalAtms;
    const items = [
      { label: 'En ligne',   count: f.onlineCount,  color: '#22c55e', cssClass: 'online' },
      { label: 'Avertiss.',  count: f.warningCount, color: '#f97316', cssClass: 'warning' },
      { label: 'Hors ligne', count: f.offlineCount, color: '#ef4444', cssClass: 'offline' },
      { label: 'Inconnu',    count: f.unknownCount, color: '#64748b', cssClass: 'unknown' }
    ].filter(i => i.count > 0);

    // Build SVG arc path data for each segment
    const radius = 80, cx = 100, cy = 100;
    const circumference = 2 * Math.PI * radius;
    let offset = 0;
    return items.map(item => {
      const pct    = item.count / total;
      const dash   = pct * circumference;
      const gap    = circumference - dash;
      const result = { ...item, pct: Math.round(pct * 100), strokeDasharray: `${dash} ${gap}`, strokeDashoffset: -offset };
      offset += dash;
      return result;
    });
  });

  // SLA gauge degree (0–180)
  slaGaugeDeg = computed(() => {
    const pct = this.sla()?.availabilityPercent ?? 0;
    return Math.min(180, (Number(pct) / 100) * 180);
  });

  slaColor = computed(() => {
    const pct = Number(this.sla()?.availabilityPercent ?? 0);
    if (pct >= 99) return '#22c55e';
    if (pct >= 95) return '#f97316';
    return '#ef4444';
  });

  // Filtered & sorted ATM status table
  statusFilter = signal<'ALL' | 'ONLINE' | 'WARNING' | 'OFFLINE' | 'UNKNOWN'>('ALL');
  searchTerm   = signal('');

  filteredStatuses = computed(() => {
    const rows  = this.statuses();
    const flt   = this.statusFilter();
    const term  = this.searchTerm().toLowerCase();
    return rows
      .filter(r => flt === 'ALL' || r.status === flt)
      .filter(r => !term || r.clientName.toLowerCase().includes(term) || r.networkAddress?.includes(term))
      .slice(0, 50);
  });

  // ── Lifecycle ────────────────────────────────────────────────────────────
  ngOnInit() {
    this.startPolling();
  }

  ngOnDestroy() {
    this.sub?.unsubscribe();
  }

  private startPolling() {
    this.sub?.unsubscribe();
    this.loading.set(true);

    this.sub = interval(30_000).pipe(
      startWith(0),
      switchMap(() => {
        const { from, to } = this.dateRange();
        return this.nocService.getSummary(from, to);
      })
    ).subscribe({
      next: (res) => {
        this.data.set(res);
        this.loading.set(false);
        this.error.set(null);
        this.lastUpdate.set(new Date());
      },
      error: (err) => {
        this.error.set('Impossible de joindre le backend. Vérifiez que le serveur tourne sur le port 5239.');
        this.loading.set(false);
      }
    });
  }

  private dateRange(): { from: Date; to: Date } {
    const to   = new Date();
    const from = new Date();
    const days = this.selectedRange() === '1d' ? 1 : this.selectedRange() === '7d' ? 7 : 30;
    from.setDate(from.getDate() - days);
    return { from, to };
  }

  setRange(r: '1d' | '7d' | '30d') {
    this.selectedRange.set(r);
    this.startPolling();
  }

  setFilter(f: 'ALL' | 'ONLINE' | 'WARNING' | 'OFFLINE' | 'UNKNOWN') {
    this.statusFilter.set(f);
  }

  onSearch(ev: Event) {
    this.searchTerm.set((ev.target as HTMLInputElement).value);
  }

  refresh() { this.startPolling(); }

  statusClass(status: string): string {
    switch (status.toUpperCase()) {
      case 'ONLINE':  return 'badge--ok';
      case 'WARNING': return 'badge--warn';
      case 'OFFLINE': return 'badge--down';
      default:        return 'badge--muted';
    }
  }

  statusIcon(status: string): string {
    switch (status.toUpperCase()) {
      case 'ONLINE':  return '●';
      case 'WARNING': return '▲';
      case 'OFFLINE': return '✕';
      default:        return '?';
    }
  }

  formatCash(val: number | undefined): string {
    if (val == null) return '—';
    if (val >= 1_000_000) return (val / 1_000_000).toFixed(1) + ' M';
    if (val >= 1_000)     return (val / 1_000).toFixed(0) + ' K';
    return val.toFixed(0);
  }

  formatSeconds(sec: number | undefined): string {
    if (!sec) return '—';
    const h = Math.floor(sec / 3600);
    const m = Math.floor((sec % 3600) / 60);
    return `${h}h ${m}m`;
  }

  trackById(_: number, row: AtmStatusRow) { return row.clientId; }
}
