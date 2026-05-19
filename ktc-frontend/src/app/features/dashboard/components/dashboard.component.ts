import { Component, ChangeDetectionStrategy, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';

interface DateRange { label: string; from: string; to: string; }

@Component({
  selector: 'app-dashboard',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [CommonModule],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.css']
})
export class DashboardComponent {
  private sanitizer = inject(DomSanitizer);

  // ── Remplacez par l'UID obtenu après import dans Grafana ──────────────────
  private readonly DASHBOARD_UID  = 'adrwjhz';
  private readonly DASHBOARD_SLUG = 'dashboard-ktc';
  // Via proxy Angular (/grafana → localhost:3000) qui supprime X-Frame-Options
  private readonly GRAFANA_BASE   = '/grafana';

  readonly ranges: DateRange[] = [
    { label: '24h',  from: 'now-24h', to: 'now' },
    { label: '7j',   from: 'now-7d',  to: 'now' },
    { label: '30j',  from: 'now-30d', to: 'now' },
    { label: '1 an', from: 'now-1y',  to: 'now' },
    { label: 'Tout', from: '2021-05-15T00:00:00.000Z', to: 'now' },
  ];

  activeRange = signal<DateRange>(this.ranges[2]);

  readonly grafanaUrl = computed<SafeResourceUrl>(() => {
    const r = this.activeRange();
    const url =
      `${this.GRAFANA_BASE}/d/${this.DASHBOARD_UID}/${this.DASHBOARD_SLUG}` +
      `?orgId=1` +
      `&from=${encodeURIComponent(r.from)}` +
      `&to=${encodeURIComponent(r.to)}` +
      `&timezone=browser` +
      `&refresh=30s` +
      `&kiosk`;
    return this.sanitizer.bypassSecurityTrustResourceUrl(url);
  });

  setRange(range: DateRange): void {
    this.activeRange.set(range);
  }
}
