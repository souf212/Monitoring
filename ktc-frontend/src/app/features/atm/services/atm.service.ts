export * from '../models/atm.models';
import { ClientAtm, BusinessDto, BusinessDetailsDto, BranchDto, RegionDto, RegionDetailsDto, HardwareTypeDto, CreateOrUpdateAtmRequest, CreateBranchRequest, CreateBusinessRequest, CreateRegionRequest, AtmComponentStatusDto, AtmAssetHistoryDto, RegionListDto, LastClientContactDto, AtmSoftwareInfoDto, AtmCertificateDto, AtmTicketDto } from '../models/atm.models';
import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

// ── Models calqués sur vos DTOs C# ──────────────────────────────────────────

// ── Service ──────────────────────────────────────────────────────────────────

@Injectable({ providedIn: 'root' })
export class AtmService {
  private readonly http = inject(HttpClient);
  private readonly BASE = 'http://localhost:5239/api/atm';

  // ── Clients / ATMs ────────────────────────────────────────────────────────

  getClients(): Observable<ClientAtm[]> {
    return this.http.get<ClientAtm[]>(`${this.BASE}/clients`);
  }

  getClientById(id: number): Observable<ClientAtm> {
    return this.http.get<ClientAtm>(`${this.BASE}/clients/${id}`);
  }

  createClient(req: CreateOrUpdateAtmRequest): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${this.BASE}/clients`, req);
  }

  updateClient(id: number, req: CreateOrUpdateAtmRequest): Observable<{ message: string }> {
    return this.http.put<{ message: string }>(`${this.BASE}/clients/${id}`, req);
  }

  deleteClient(id: number): Observable<{ message: string }> {
    return this.http.delete<{ message: string }>(`${this.BASE}/clients/${id}`);
  }

  getAtmStatus(clientId: number): Observable<AtmComponentStatusDto[]> {
    return this.http.get<AtmComponentStatusDto[]>(`${this.BASE}/clients/${clientId}/status`);
  }

  getAtmAssetHistory(clientId: number): Observable<AtmAssetHistoryDto[]> {
    return this.http.get<AtmAssetHistoryDto[]>(`${this.BASE}/clients/${clientId}/assethistory`);
  }

  // ── Businesses ────────────────────────────────────────────────────────────

  getBusinesses(): Observable<BusinessDto[]> {
    return this.http.get<BusinessDto[]>(`${this.BASE}/businesses`);
  }

  getBusinessById(id: number): Observable<BusinessDetailsDto> {
    return this.http.get<BusinessDetailsDto>(`${this.BASE}/businesses/${id}`);
  }

  createBusiness(req: CreateBusinessRequest): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${this.BASE}/businesses`, req);
  }

  updateBusiness(id: number, req: Partial<CreateBusinessRequest>): Observable<{ message: string }> {
    return this.http.put<{ message: string }>(`${this.BASE}/businesses/${id}`, req);
  }

  deleteBusiness(id: number): Observable<{ message: string }> {
    return this.http.delete<{ message: string }>(`${this.BASE}/businesses/${id}`);
  }

  // ── Branches ──────────────────────────────────────────────────────────────

  getBranches(): Observable<BranchDto[]> {
    return this.http.get<BranchDto[]>(`${this.BASE}/branches`);
  }

  getBranchById(id: number): Observable<BranchDto> {
    return this.http.get<BranchDto>(`${this.BASE}/branches/${id}`);
  }

  createBranch(req: CreateBranchRequest): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${this.BASE}/branches`, req);
  }

  updateBranch(id: number, req: CreateBranchRequest): Observable<{ message: string }> {
    return this.http.put<{ message: string }>(`${this.BASE}/branches/${id}`, req);
  }

  deleteBranch(id: number): Observable<{ message: string }> {
    return this.http.delete<{ message: string }>(`${this.BASE}/branches/${id}`);
  }

  // ── Regions ───────────────────────────────────────────────────────────────

getRegions(): Observable<RegionListDto[]> {
  return this.http.get<RegionListDto[]>(`${this.BASE}/regions`);
}
  getRegionById(id: number): Observable<RegionDetailsDto> {
    return this.http.get<RegionDetailsDto>(`${this.BASE}/regions/${id}`);
  }

  createRegion(req: CreateRegionRequest): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${this.BASE}/regions`, req);
  }

  updateRegion(id: number, req: CreateRegionRequest): Observable<{ message: string }> {
    return this.http.put<{ message: string }>(`${this.BASE}/regions/${id}`, req);
  }

  deleteRegion(id: number): Observable<{ message: string }> {
    return this.http.delete<{ message: string }>(`${this.BASE}/regions/${id}`);
  }

  // ── Hardware Types ────────────────────────────────────────────────────────

  getHardwareTypes(): Observable<HardwareTypeDto[]> {
    return this.http.get<HardwareTypeDto[]>(`${this.BASE}/hardwaretypes`);
  }

  getHardwareTypesByBusiness(businessId: number): Observable<HardwareTypeDto[]> {
    return this.http.get<HardwareTypeDto[]>(`${this.BASE}/businesses/${businessId}/hardwaretypes`);
  }

  // ── Last Client Contact ───────────────────────────────────────────────────

  getLastClientContact(clientId: number): Observable<LastClientContactDto> {
    return this.http.get<LastClientContactDto>(`${this.BASE}/clients/${clientId}/lastcontact`);
  }

  getAtmSoftwareInfo(clientId: number): Observable<AtmSoftwareInfoDto[]> {
    return this.http.get<AtmSoftwareInfoDto[]>(`${this.BASE}/clients/${clientId}/softwareinfo`);
  }

  getAtmCertificates(clientId: number): Observable<AtmCertificateDto[]> {
    return this.http.get<AtmCertificateDto[]>(`${this.BASE}/clients/${clientId}/certificates`);
  }

  getAtmTickets(clientId: number, days: number = 14, statusFilter: string = 'All'): Observable<AtmTicketDto[]> {
    return this.http.get<AtmTicketDto[]>(`${this.BASE}/clients/${clientId}/tickets`, {
      params: {
        days: days.toString(),
        statusFilter: statusFilter
      }
    });
  }
}

