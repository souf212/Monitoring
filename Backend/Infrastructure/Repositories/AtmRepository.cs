

using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace KtcWeb.Infrastructure.Repositories
{
    public class AtmRepository : IAtmRepository
    {
        private readonly KtcDbContext _context;

        public AtmRepository(KtcDbContext context)
        {
            _context = context;
        }

        private class AtmTicketRaw
        {
            public int TicketId { get; set; }
            public string TicketType { get; set; } = string.Empty;
            public string ClientName { get; set; } = string.Empty;
            public DateTime Created { get; set; }
            public bool IsClosed { get; set; }
            public string Duration { get; set; } = string.Empty;
            public string Status { get; set; } = string.Empty;
            public int ErrorId { get; set; }
            public string Code { get; set; } = string.Empty;
            public string ErrorText { get; set; } = string.Empty;
            public string Owner { get; set; } = string.Empty;
            public string LastChangeBy { get; set; } = string.Empty;
            public DateTime LastChangeDate { get; set; }
            public string SlaSummary { get; set; } = string.Empty;
            public string DispatchedTo { get; set; } = string.Empty;
            public string CommentsXml { get; set; } = string.Empty;
        }

        private async Task<bool> ColumnExistsAsync(string tableName, string columnName)
        {
            var exists = await _context.Database.SqlQueryRaw<int>(@"
                SELECT CASE
                    WHEN EXISTS (
                        SELECT 1
                        FROM INFORMATION_SCHEMA.COLUMNS
                        WHERE TABLE_SCHEMA = 'dbo'
                          AND TABLE_NAME = {0}
                          AND COLUMN_NAME = {1}
                    ) THEN 1
                    ELSE 0
                END AS [Value]", tableName, columnName).FirstOrDefaultAsync();

            return exists == 1;
        }

        private async Task<string?> FirstExistingColumnAsync(string tableName, params string[] candidates)
        {
            foreach (var candidate in candidates)
            {
                if (await ColumnExistsAsync(tableName, candidate))
                {
                    return candidate;
                }
            }

            return null;
        }

        public async Task<List<AtmComponentStatusDto>> GetAtmStatusAsync(int clientId)
        {
            var query = from status in _context.CurrentStatus.AsNoTracking()
                        where status.ClientId == clientId
                        join component in _context.ComponentList.AsNoTracking()
                            on status.ComponentId equals component.ComponentId
                        join property in _context.PropertyList.AsNoTracking()
                            on status.PropertyId equals property.PropertyId
                        join val in _context.ValueList.AsNoTracking()
                            on status.ValueId equals val.ValueId into valueGroup
                        from val in valueGroup.DefaultIfEmpty()
                        select new AtmComponentStatusDto
                        {
                            ComponentName = component.ComponentName,
                            PropertyCategory = property.Category,
                            PropertyName = property.PropertyName,
                            Value = status.ValueId == 0 ? status.NumericValue.ToString() : (val != null ? val.ValueName : status.NumericValue.ToString()),
                            Severity = "UNKNOWN" // We'll compute this after retrieving or let it be UNKNOWN for now
                        };

            var result = await query.ToListAsync();

            // Compute Severity based on text value (OK, WARNING, CRITICAL, UNKNOWN)
            foreach (var item in result)
            {
                var valLower = item.Value?.ToLower() ?? "";
                if (valLower.Contains("ok") || valLower.Contains("normal") || valLower.Contains("good") || valLower.Contains("active"))
                    item.Severity = "OK";
                else if (valLower.Contains("warning") || valLower.Contains("low") || valLower.Contains("near") || valLower.Contains("attention"))
                    item.Severity = "WARNING";
                else if (valLower.Contains("critical") || valLower.Contains("error") || valLower.Contains("fault") || valLower.Contains("empty") || valLower.Contains("jam") || valLower.Contains("out") || valLower.Contains("offline") || valLower.Contains("fatal"))
                    item.Severity = "CRITICAL";
                else
                    item.Severity = "UNKNOWN";
            }

            return result;
        }

        public async Task<List<AtmAssetHistoryDto>> GetAtmAssetHistoryAsync(int clientId)
        {
            var query = from history in _context.AssetHistory.AsNoTracking()
                        where history.ClientId == clientId
                        join component in _context.ComponentList.AsNoTracking()
                            on history.ComponentId equals component.ComponentId into compGroup
                        from component in compGroup.DefaultIfEmpty()
                        join property in _context.PropertyList.AsNoTracking()
                            on history.PropertyId equals property.PropertyId into propGroup
                        from property in propGroup.DefaultIfEmpty()
                        join valOld in _context.ValueList.AsNoTracking()
                            on history.OldValueId equals valOld.ValueId into valOldGroup
                        from valOld in valOldGroup.DefaultIfEmpty()
                        join valNew in _context.ValueList.AsNoTracking()
                            on history.NewValueId equals valNew.ValueId into valNewGroup
                        from valNew in valNewGroup.DefaultIfEmpty()
                        orderby history.Date descending
                        select new
                        {
                            history.Date,
                            ComponentName = component != null ? component.ComponentName : "Unknown",
                            PropertyName = property != null ? property.PropertyName : "Unknown",
                            OldValueName = valOld != null ? valOld.ValueName : null,
                            NewValueName = valNew != null ? valNew.ValueName : null,
                            history.OldValueId,
                            history.NewValueId,
                            history.OldNumericValue,
                            history.NewNumericValue,
                            history.Comments
                        };

            var dbResult = await query.ToListAsync();

            var result = new List<AtmAssetHistoryDto>();

            foreach (var item in dbResult)
            {
                var dto = new AtmAssetHistoryDto
                {
                    Timestamp = item.Date,
                    ComponentName = item.ComponentName,
                    PropertyName = item.PropertyName,
                    OldValue = item.OldValueId == 0 ? item.OldNumericValue.ToString() : (item.OldValueName ?? item.OldNumericValue.ToString()),
                    NewValue = item.NewValueId == 0 ? item.NewNumericValue.ToString() : (item.NewValueName ?? item.NewNumericValue.ToString()),
                    User = "System",
                    Comment = item.Comments ?? ""
                };

                if (!string.IsNullOrWhiteSpace(item.Comments))
                {
                    try
                    {
                        var xdoc = XDocument.Parse(item.Comments);
                        var root = xdoc.Root;
                        if (root != null)
                        {
                            var userAttr = root.Attribute("User");
                            if (userAttr != null)
                            {
                                dto.User = userAttr.Value;
                            }
                            dto.Comment = root.Value;
                        }
                    }
                    catch
                    {
                        // Fallback to raw string if XML parsing fails
                        dto.Comment = item.Comments;
                    }
                }

                result.Add(dto);
            }

            return result;
        }

        public async Task<LastClientContactDto?> GetLastClientContactAsync(int clientId)
        {
            return await _context.Database.SqlQueryRaw<LastClientContactDto>(@"
                SELECT TOP (1)
                    client_id AS ClientId,
                    timestmp AS Timestmp,
                    ISNULL(timeoffset, 0) AS Timeoffset,
                    ISNULL(lastmsgid, 0) AS LastMsgId,
                    ISNULL(CONVERT(varchar(50), lastmsgreply), 'N/A') AS LastMsgReply,
                    nextmessageexpected AS NextMessageExpected,
                    msgrejectedinfo AS MsgRejectedInfo,
                    ISNULL(msgqueuesize, 0) AS MsgQueueSize,
                    msgcreatedts AS MsgCreatedTs,
                    CAST(ISNULL(replayflag, 0) AS bit) AS ReplayFlag,
                    CAST(ISNULL(mutual_auth, 0) AS bit) AS MutualAuth
                FROM dbo.LastClientContact
                WHERE client_id = {0}
                ORDER BY timestmp DESC", clientId).FirstOrDefaultAsync();
        }

        public async Task<List<AtmSoftwareInfoDto>> GetAtmSoftwareInfoAsync(int clientId)
        {
            return await _context.Database.SqlQueryRaw<AtmSoftwareInfoDto>(@"
                SELECT
                    si.sw_id AS SwId,
                    si.sw_name AS SoftwareName,
                    si.version AS Version,
                    ins.install_type AS InstallType,
                    CASE ins.install_type
                        WHEN -1 THEN 'Unknown'
                        WHEN 0 THEN 'KTC software package'
                        WHEN 1 THEN 'Windows Hotfix'
                        WHEN 2 THEN 'MSI installed program'
                        WHEN 3 THEN 'Operating system'
                        WHEN 4 THEN 'KTC marketing package'
                        WHEN 5 THEN 'Other KAL software'
                        WHEN 6 THEN 'Hypervisor'
                        ELSE 'Other'
                    END AS InstallTypeLabel,
                    ins.install_date AS InstallDate,
                    ISNULL(r.rules_count, 0) AS ComplianceRulesCount
                FROM dbo.InstalledSoftware ins
                INNER JOIN dbo.SoftwareInfo si ON si.sw_id = ins.sw_id
                LEFT JOIN (
                    SELECT sw_id, COUNT(*) AS rules_count
                    FROM dbo.SoftwareInfoComplianceRules
                    GROUP BY sw_id
                ) r ON r.sw_id = si.sw_id
                WHERE ins.client_id = {0}
                ORDER BY ins.install_date DESC, si.sw_name ASC", clientId).ToListAsync();
        }

        public async Task<List<AtmCertificateDto>> GetAtmCertificatesAsync(int clientId)
        {
            return await _context.Database.SqlQueryRaw<AtmCertificateDto>(@"
                SELECT
                    CASE cl.certificate_store
                        WHEN 1 THEN 'LMRoot'
                        WHEN 2 THEN 'LMPersonal'
                        WHEN 3 THEN 'CURoot'
                        WHEN 4 THEN 'CUPersonal'
                        WHEN 5 THEN 'LMIntermediate'
                        WHEN 6 THEN 'LMTrustedPeople'
                        WHEN 7 THEN 'CUIntermediate'
                        WHEN 8 THEN 'CUTrustedPeople'
                        ELSE 'Unknown'
                    END AS CertificateStore,
                    cl.subject_name AS SubjectName,
                    cl.issuer AS Issuer,
                    cl.friendly_name AS FriendlyName,
                    cl.not_before AS NotBefore,
                    cl.not_after AS NotAfter,
                    CAST(cl.is_private AS bit) AS IsPrivate,
                    cl.addedtime AS FirstSeen,
                    UPPER(CONVERT(varchar(128), cl.serial, 2)) AS SerialNumber
                FROM dbo.ClientCertificates cc
                INNER JOIN dbo.CertificateList cl ON cl.certificate_id = cc.certificate_id
                WHERE cc.client_id = {0}
                ORDER BY
                    cl.certificate_store ASC,
                    cl.not_after ASC,
                    cl.subject_name ASC", clientId).ToListAsync();
        }

        public async Task<List<AtmTicketDto>> GetAtmTicketsAsync(int clientId, int days, string statusFilter)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-days);

            var ticketIdCol = await FirstExistingColumnAsync("TroubleTickets",
                "trouble_ticket_id", "troubleticket_id", "ticket_id", "ticketid", "id");
            var lastChangeCol = await FirstExistingColumnAsync("TroubleTickets",
                "lastchangedate", "lastchange_date", "lastupdatedate", "updatedtime", "modifieddate");
            var userUpdatedCol = await FirstExistingColumnAsync("TroubleTickets",
                "user_updated", "userupdated", "lastchangeby", "updatedby");
            var slaSummaryCol = await FirstExistingColumnAsync("TroubleTickets",
                "sla_summary", "slasummary", "sla");
            var ttCreatedCol = await FirstExistingColumnAsync("TroubleTickets",
                "creationtime", "created", "createdate", "created_at");
            var tttErrorCodeCol = await FirstExistingColumnAsync("TroubleTicketTypes",
                "errorcode_id", "errorcodeid", "error_code_id");
            var tttTypeNameCol = await FirstExistingColumnAsync("TroubleTicketTypes",
                "tickettype_name", "tickettypename", "ticket_type", "tickettype", "name", "description");
            var ecErrorCodeCol = await FirstExistingColumnAsync("ErrorCodeTypes",
                "errorcode_id", "errorcodeid", "error_code_id");
            var ecCodeCol = await FirstExistingColumnAsync("ErrorCodeTypes", "code", "error_code");
            var ecErrorTextCol = await FirstExistingColumnAsync("ErrorCodeTypes", "error_text", "errortext", "description");
            var userNameCol = await FirstExistingColumnAsync("KTCUsers",
                "user_name", "username", "name", "display_name");
            var clientNameCol = await FirstExistingColumnAsync("Clients",
                "clientname", "client_name", "name");

            var ticketIdExpr = string.IsNullOrWhiteSpace(ticketIdCol) ? "0" : $"CAST(tt.[{ticketIdCol}] AS int)";
            var lastChangeExpr = string.IsNullOrWhiteSpace(lastChangeCol) ? "tt.creationtime" : $"tt.[{lastChangeCol}]";
            var lastChangeByExpr = string.IsNullOrWhiteSpace(userUpdatedCol) ? "'N/A'" : $"tt.[{userUpdatedCol}]";
            var slaSummaryExpr = string.IsNullOrWhiteSpace(slaSummaryCol) ? "''" : $"tt.[{slaSummaryCol}]";
            var ticketTypeExpr = string.IsNullOrWhiteSpace(tttTypeNameCol) ? "''" : $"ttt.[{tttTypeNameCol}]";
            var clientNameExpr = string.IsNullOrWhiteSpace(clientNameCol) ? "''" : $"c.[{clientNameCol}]";
            var createdExpr = string.IsNullOrWhiteSpace(ttCreatedCol) ? "GETUTCDATE()" : $"tt.[{ttCreatedCol}]";
            var canJoinErrorCode = !string.IsNullOrWhiteSpace(tttErrorCodeCol) && !string.IsNullOrWhiteSpace(ecErrorCodeCol);
            var errorIdExpr = canJoinErrorCode ? $"CAST(ISNULL(ec.[{ecErrorCodeCol}], 0) AS int)" : "0";
            var codeExpr = !string.IsNullOrWhiteSpace(ecCodeCol) ? $"ISNULL(ec.[{ecCodeCol}], '')" : "''";
            var errorTextExpr = !string.IsNullOrWhiteSpace(ecErrorTextCol) ? $"ISNULL(ec.[{ecErrorTextCol}], '')" : "''";
            var errorCodeJoin = canJoinErrorCode
                ? $"LEFT JOIN dbo.ErrorCodeTypes ec ON ec.[{ecErrorCodeCol}] = ttt.[{tttErrorCodeCol}]"
                : "LEFT JOIN dbo.ErrorCodeTypes ec ON 1 = 0";
            var ownerExpr = string.IsNullOrWhiteSpace(userNameCol) ? "'N/A'" : $"u.[{userNameCol}]";

            var query = @"
                SELECT
                    " + ticketIdExpr + @" AS TicketId,
                    ISNULL(" + ticketTypeExpr + @", 'N/A') AS TicketType,
                    ISNULL(" + clientNameExpr + @", 'N/A') AS ClientName,
                    " + createdExpr + @" AS Created,
                    CASE 
                        WHEN tt.ticketstatus_id IN (3, 4, 5) THEN CAST(1 AS bit)
                        ELSE CAST(0 AS bit)
                    END AS IsClosed,
                    ISNULL(CONVERT(varchar(50), DATEDIFF(HOUR, tt.creationtime, ISNULL(" + lastChangeExpr + @", GETUTCDATE()))), '0') + ' hours' AS Duration,
                    CASE 
                        WHEN tt.ticketstatus_id = 1 THEN 'Open'
                        WHEN tt.ticketstatus_id = 2 THEN 'Dispatched'
                        WHEN tt.ticketstatus_id = 3 THEN 'Closed'
                        WHEN tt.ticketstatus_id = 4 THEN 'AutoClosed'
                        WHEN tt.ticketstatus_id = 5 THEN 'Suspended'
                        ELSE 'Unknown'
                    END AS Status,
                    " + errorIdExpr + @" AS ErrorId,
                    " + codeExpr + @" AS Code,
                    " + errorTextExpr + @" AS ErrorText,
                    ISNULL(" + ownerExpr + @", 'N/A') AS Owner,
                    ISNULL(" + lastChangeByExpr + @", 'N/A') AS LastChangeBy,
                    ISNULL(" + lastChangeExpr + @", tt.creationtime) AS LastChangeDate,
                    ISNULL(" + slaSummaryExpr + @", '') AS SlaSummary,
                    ISNULL(dl.dispatchname, '') AS DispatchedTo,
                    ISNULL(tt.comments, '') AS CommentsXml
                FROM dbo.TroubleTickets tt
                LEFT JOIN dbo.TroubleTicketTypes ttt ON ttt.tickettype_id = tt.tickettype_id
                " + errorCodeJoin + @"
                LEFT JOIN dbo.KTCUsers u ON u.user_id = tt.owner_id
                LEFT JOIN dbo.DispatchList dl ON dl.dispatch_id = tt.dispatch_id
                LEFT JOIN dbo.Clients c ON c.client_id = tt.client_id
                WHERE tt.client_id = {0}
                  AND tt.creationtime >= {1}";

            // Add status filter
            if (statusFilter == "Open/Dispatched")
            {
                query += " AND tt.ticketstatus_id IN (1, 2)";
            }
            else if (statusFilter == "Closed")
            {
                query += " AND tt.ticketstatus_id IN (3, 4, 5)";
            }
            // If "All", no additional filter

            if (lastChangeExpr.Equals("tt.creationtime", StringComparison.OrdinalIgnoreCase))
            {
                query += " ORDER BY tt.creationtime DESC";
            }
            else
            {
                query += " ORDER BY " + lastChangeExpr + " DESC, tt.creationtime DESC";
            }

            var dbResults = await _context.Database.SqlQueryRaw<AtmTicketRaw>(query, clientId, cutoffDate).ToListAsync();

            var tickets = new List<AtmTicketDto>();

            foreach (var row in dbResults)
            {
                var lastComment = "";
                var lastChangeBy = row.LastChangeBy;

                // Try to parse XML comments
                if (!string.IsNullOrWhiteSpace(row.CommentsXml))
                {
                    try
                    {
                        var xdoc = XDocument.Parse(row.CommentsXml);
                        var root = xdoc.Root;
                        if (root != null)
                        {
                            var lastCommentElement = root.Elements("Comment").LastOrDefault();
                            if (lastCommentElement != null)
                            {
                                lastComment = lastCommentElement.Value;
                                var userAttr = lastCommentElement.Attribute("User");
                                if (userAttr != null)
                                {
                                    lastChangeBy = userAttr.Value;
                                }
                            }
                        }
                    }
                    catch
                    {
                        lastComment = row.CommentsXml;
                    }
                }

                tickets.Add(new AtmTicketDto
                {
                    TicketId = row.TicketId,
                    TicketType = string.IsNullOrWhiteSpace(row.TicketType) ? "N/A" : row.TicketType,
                    ClientName = string.IsNullOrWhiteSpace(row.ClientName) ? "N/A" : row.ClientName,
                    Created = row.Created,
                    IsClosed = row.IsClosed,
                    Duration = string.IsNullOrWhiteSpace(row.Duration) ? "-" : row.Duration,
                    Status = string.IsNullOrWhiteSpace(row.Status) ? "Unknown" : row.Status,
                    ErrorId = row.ErrorId,
                    Code = row.Code ?? string.Empty,
                    ErrorText = row.ErrorText ?? string.Empty,
                    Owner = row.Owner ?? "N/A",
                    LastChangeBy = lastChangeBy ?? string.Empty,
                    LastChangeDate = row.LastChangeDate,
                    LastComment = lastComment,
                    SlaSummary = row.SlaSummary ?? string.Empty,
                    DispatchedTo = row.DispatchedTo ?? string.Empty
                });
            }

            return tickets;
        }
    }
}


