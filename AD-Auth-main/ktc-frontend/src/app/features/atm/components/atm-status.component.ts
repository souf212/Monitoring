import { Component, Input, OnInit, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { AtmService, AtmComponentStatusDto } from '../services/atm.service';

@Component({
  selector: 'app-atm-status',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './atm-status.component.html',
  styleUrls: ['./atm-status.component.css']
})
export class AtmStatusComponent implements OnInit {
  @Input() clientId?: number;
  
  private route = inject(ActivatedRoute);
  private atmService = inject(AtmService);

  isLoading = signal(true);
  error = signal<string | null>(null);

  rawStatus = signal<AtmComponentStatusDto[]>([]);
  selectedComponent = signal<string | null>(null);

  // Compute unique components and their worst severity
  uniqueComponents = computed(() => {
    const statusList = this.rawStatus();
    const compMap = new Map<string, string>();

    for (const item of statusList) {
      const currentWorst = compMap.get(item.componentName) || 'OK';
      compMap.set(item.componentName, this.getWorstSeverity(currentWorst, item.severity));
    }

    return Array.from(compMap.entries()).map(([name, globalSeverity]) => ({
      name,
      displayName: this.formatComponentName(name),
      globalSeverity
    })).sort((a, b) => a.displayName.localeCompare(b.displayName));
  });

  formatComponentName(name: string): string {
    if (!name) return '';
    let formatted = name;
    
    // Supprimer les préfixes techniques
    formatted = formatted.replace(/^KXDevice\.KX/, '');
    formatted = formatted.replace(/^KXDevice\./, '');
    formatted = formatted.replace(/^Win32_/, '');
    
    // Remplacer les points restants par des tirets
    formatted = formatted.replace(/\./g, ' - ');
    
    // Ajouter des espaces pour le CamelCase (ex: CardReader -> Card Reader, ICCard -> IC Card)
    formatted = formatted.replace(/([a-z])([A-Z])/g, '$1 $2').replace(/([A-Z])([A-Z][a-z])/g, '$1 $2');
    
    return formatted.trim();
  }

  // Group properties by category for the selected component
  selectedComponentPropertiesGrouped = computed(() => {
    const selected = this.selectedComponent();
    if (!selected) return [];
    
    const props = this.rawStatus().filter(s => s.componentName === selected);
    
    const groups = new Map<string, AtmComponentStatusDto[]>();
    for (const prop of props) {
      const cat = prop.propertyCategory || 'Général';
      if (!groups.has(cat)) {
        groups.set(cat, []);
      }
      groups.get(cat)!.push(prop);
    }
    
    return Array.from(groups.entries()).map(([category, items]) => ({
      category,
      items
    })).sort((a, b) => a.category.localeCompare(b.category));
  });

  ngOnInit(): void {
    // Try to get from Input, then current route, then parent route
    let idStr = this.route.snapshot.paramMap.get('id');
    if (!idStr && this.route.parent) {
      idStr = this.route.parent.snapshot.paramMap.get('id');
    }

    const finalId = this.clientId ?? (idStr ? Number(idStr) : null);

    if (finalId) {
      this.loadStatus(finalId);
    } else {
      this.error.set("Aucun identifiant d'ATM fourni.");
      this.isLoading.set(false);
    }
  }

  loadStatus(id: number): void {
    this.isLoading.set(true);
    this.atmService.getAtmStatus(id).subscribe({
      next: (data) => {
        this.rawStatus.set(data);
        if (data.length > 0) {
           this.selectedComponent.set(this.uniqueComponents()[0].name);
        }
        this.isLoading.set(false);
      },
      error: () => {
        this.error.set("Erreur lors de la récupération du statut.");
        this.isLoading.set(false);
      }
    });
  }

  selectComponent(name: string): void {
    this.selectedComponent.set(name);
  }

  getSeverityClass(severity: string | undefined | null): string {
    if (!severity) return 'status-gray';
    switch (severity.toUpperCase()) {
      case 'OK': return 'status-green';
      case 'WARNING': return 'status-orange';
      case 'CRITICAL': return 'status-red';
      default: return 'status-gray';
    }
  }

  private getWorstSeverity(sev1: string, sev2: string): string {
    const levels: Record<string, number> = { 'OK': 0, 'WARNING': 1, 'CRITICAL': 2, 'UNKNOWN': -1 };
    const val1 = levels[sev1?.toUpperCase() || 'UNKNOWN'] ?? -1;
    const val2 = levels[sev2?.toUpperCase() || 'UNKNOWN'] ?? -1;
    
    if (val1 >= val2) return sev1?.toUpperCase() || 'UNKNOWN';
    return sev2?.toUpperCase() || 'UNKNOWN';
  }
}

