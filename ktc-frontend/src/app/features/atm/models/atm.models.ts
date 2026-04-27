export interface ClientAtm {
  clientId: number;
  ktcGuid: string;
  clientName: string;
  networkAddress: string;
  connectable: number;          // 1=Non connectable | 2=IP Statique | 3=IP Dynamique
  detailsUnknown: boolean;
  latitude: number;
  longitude: number;
  timezone: string;
  comments?: string;
  businessId: number;
  branchId: number;
  hardwareTypeId: number;
  hardwareTypeName?: string;
  active: boolean;
  clientType: number;
}

export interface BusinessDto {
  businessId: number;
  businessName: string;
  displayId: string;
}

export interface BusinessDetailsDto extends BusinessDto {
  additionalInfo?: string;
}

export interface BranchDto {
  branchId: number;
  branchName: string;
  displayId: string;
  additionalInfo?: string;
  businessId: number;
  level1RegionId: number;
  level2RegionId: number;
  level3RegionId: number;
  level4RegionId: number;
  level5RegionId: number;
}

export interface RegionDto {
  regionId: number;
  regionName: string;
  displayId: string;
}

export interface RegionDetailsDto extends RegionDto {
  businessId: number;
  regionLevel: number;
  parentRegionId: number;
  additionalInfo?: string;
}

export interface HardwareTypeDto {
  hardwareTypeId: number;
  name: string;
  description: string;
  typeGroup: string;
  canBeConfigured: boolean;
  canBeMonitored: boolean;
}
export interface RegionListDto {
  regionId: number;
  regionName: string;
  displayId: string;
  regionLevel: number;
  parentRegionId: number;
  businessName: string;
}

export interface CreateOrUpdateAtmRequest {
  clientName: string;
  networkAddress: string;
  connectable: number;
  detailsUnknown: boolean;
  latitude: number;
  longitude: number;
  timezone: string;
  comments?: string;
  clientType: number;
  gridPosition: number;
  businessId: number;
  branchId: number;
  hardwareTypeId: number;
  ownerId: number;
  deleteLater: boolean;
  active: boolean;
  subnet: string;
  level1RegionId: number;
  level2RegionId: number;
  level3RegionId: number;
  level4RegionId: number;
  level5RegionId: number;
  salt: string;
  authHash: string;
  hypervisorActive: boolean;
  mergeToClientId: number;
  featureFlags: string;
}

export interface CreateBranchRequest {
  branchName: string;
  displayId?: string;
  businessId: number;
  level1RegionId: number;
  level2RegionId: number;
  level3RegionId: number;
  level4RegionId: number;
  level5RegionId: number;
  additionalInfo?: string;
}

export interface CreateBusinessRequest {
  businessName: string;
  displayId?: string;
  additionalInfo?: string;
}

export interface CreateRegionRequest {
  regionName: string;
  displayId?: string;
  businessId: number;
  regionLevel: number;
  parentRegionId: number;
  additionalInfo?: string;
}

export interface AtmComponentStatusDto {
  componentName: string;
  propertyCategory: string;
  propertyName: string;
  value: string;
  severity: string;
}

export interface AtmAssetHistoryDto {
  user: string;
  timestamp: string;
  componentName: string;
  propertyName: string;
  oldValue: string;
  newValue: string;
  comment: string;
}

export interface LastClientContactDto {
  clientId: number;
  timestmp?: string | null;           // ISO datetime
  timeoffset: number;
  lastMsgId: number;
  lastMsgReply?: string | null;
  nextMessageExpected?: string | null; // ISO datetime
  msgRejectedInfo?: string | null;
  msgQueueSize: number;
  msgCreatedTs?: string | null;        // ISO datetime
  replayFlag: boolean;
  mutualAuth: boolean;
}

export interface AtmSoftwareInfoDto {
  swId: number;
  softwareName: string;
  version: string;
  installType: number;
  installTypeLabel: string;
  installDate: string;
  complianceRulesCount: number;
}

export interface AtmCertificateDto {
  certificateStore: string;
  subjectName: string;
  issuer: string;
  friendlyName: string;
  notBefore: string;
  notAfter: string;
  isPrivate: boolean;
  firstSeen: string;
  serialNumber: string;
}

export interface AtmTicketDto {
  ticketId: number;
  ticketType: string;
  clientName: string;
  created: string;
  isClosed: boolean;
  duration: string;
  status: string;
  errorId: number;
  code: string;
  errorText: string;
  owner: string;
  lastChangeBy: string;
  lastChangeDate: string;
  lastComment: string;
  slaSummary: string;
  dispatchedTo: string;
}
