using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


using System.Xml.Linq;

namespace KtcWeb.API.Controllers
{
    [ApiController]
    [Route("api/atm")]
    public class AtmController : ControllerBase
    {
        private readonly KtcDbContext _context;
        private readonly IAtmRepository _atmRepository;

        public AtmController(KtcDbContext context, IAtmRepository atmRepository)
        {
            _context = context;
            _atmRepository = atmRepository;
        }

        // ====================== TEST CONNEXION ======================
        [HttpGet("test-connection")]
        public async Task<IActionResult> TestConnection()
        {
            try
            {
                bool canConnect = await _context.Database.CanConnectAsync();
                int count = await _context.Clients.CountAsync();

                return Ok(new
                {
                    status = "✅ Connexion réussie à KALKTCDB",
                    message = "La base est bien liée",
                    nombreDAtm = count,
                    database = "KALKTCDB"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { status = "❌ Erreur de connexion", error = ex.Message });
            }
        }

        // ====================== REGION ======================
[HttpGet("regions")]
public async Task<ActionResult<List<RegionListDto>>> GetAllRegions()
{
    try
    {
        var regions = await _context.Database.SqlQueryRaw<RegionListDto>(@"
            SELECT
                r.region_id              AS RegionId,
                r.regionname             AS RegionName,
                ISNULL(r.displayID,'')   AS DisplayId,
                r.region_level           AS RegionLevel,
                r.parent_region_id       AS ParentRegionId,
                ISNULL(b.businessname,'—') AS BusinessName
            FROM dbo.Regions r
            LEFT JOIN dbo.Businesses b ON b.business_id = r.business_id
            ORDER BY r.regionname").ToListAsync();

        return Ok(regions);
    }
    catch (Exception ex)
    {
        return BadRequest(new { message = ex.Message });
    }
}
        [HttpGet("regions/{id}")]
        public async Task<ActionResult<RegionDetailsDto>> GetRegionById(short id)
        {
            try
            {
                var items = await _context.Database.SqlQueryRaw<RegionDetailsDto>(@"
                    SELECT
                        region_id AS RegionId,
                        regionname AS RegionName,
                        ISNULL(displayID, '') AS DisplayId,
                        business_id AS BusinessId,
                        region_level AS RegionLevel,
                        parent_region_id AS ParentRegionId,
                        CAST(additionalinfo AS nvarchar(max)) AS AdditionalInfo
                    FROM dbo.Regions
                    WHERE region_id = {0}", id).ToListAsync();

                var region = items.FirstOrDefault();
                if (region == null) return NotFound(new { message = "Région introuvable" });

                return Ok(region);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("regions")]
        public async Task<IActionResult> CreateRegion([FromBody] CreateRegionRequest req)
        {
            try
            {
                var additionalInfoXml = BuildSimpleXml("PreConfigInfo", req.AdditionalInfo);

                await _context.Database.ExecuteSqlRawAsync(@"
                    INSERT INTO dbo.Regions (displayID, regionname, business_id, region_level, parent_region_id, additionalinfo)
                    VALUES (@displayId, @regionName, @businessId, @regionLevel, @parentRegionId, CONVERT(xml, @additionalInfoXml))",
                    new[]
                    {
                        new Microsoft.Data.SqlClient.SqlParameter("@displayId", (object?)req.DisplayId ?? DBNull.Value),
                        new Microsoft.Data.SqlClient.SqlParameter("@regionName", req.RegionName),
                        new Microsoft.Data.SqlClient.SqlParameter("@businessId", req.BusinessId),
                        new Microsoft.Data.SqlClient.SqlParameter("@regionLevel", req.RegionLevel),
                        new Microsoft.Data.SqlClient.SqlParameter("@parentRegionId", req.ParentRegionId),
                        new Microsoft.Data.SqlClient.SqlParameter("@additionalInfoXml", additionalInfoXml),
                    });

                return Ok(new { message = "Région créée avec succès" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("regions/{id}")]
        public async Task<IActionResult> UpdateRegion(short id, [FromBody] UpdateRegionRequest req)
        {
            try
            {
                var additionalInfoXml = BuildSimpleXml("PreConfigInfo", req.AdditionalInfo);

                var rows = await _context.Database.ExecuteSqlRawAsync(@"
                    UPDATE dbo.Regions
                    SET displayID = @displayId,
                        regionname = @regionName,
                        business_id = @businessId,
                        region_level = @regionLevel,
                        parent_region_id = @parentRegionId,
                        additionalinfo = CONVERT(xml, @additionalInfoXml)
                    WHERE region_id = @id",
                    new[]
                    {
                        new Microsoft.Data.SqlClient.SqlParameter("@displayId", (object?)req.DisplayId ?? DBNull.Value),
                        new Microsoft.Data.SqlClient.SqlParameter("@regionName", req.RegionName),
                        new Microsoft.Data.SqlClient.SqlParameter("@businessId", req.BusinessId),
                        new Microsoft.Data.SqlClient.SqlParameter("@regionLevel", req.RegionLevel),
                        new Microsoft.Data.SqlClient.SqlParameter("@parentRegionId", req.ParentRegionId),
                        new Microsoft.Data.SqlClient.SqlParameter("@additionalInfoXml", additionalInfoXml),
                        new Microsoft.Data.SqlClient.SqlParameter("@id", id),
                    });

                if (rows == 0) return NotFound(new { message = "Région introuvable" });
                return Ok(new { message = "Région modifiée avec succès" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("regions/{id}")]
        public async Task<IActionResult> DeleteRegion(short id)
        {
            try
            {
                var rows = await _context.Database.ExecuteSqlRawAsync(
                    "DELETE FROM dbo.Regions WHERE region_id = {0}", id);

                if (rows == 0) return NotFound(new { message = "Région introuvable" });
                return Ok(new { message = "Région supprimée avec succès" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ====================== BUSINESS ======================
        [HttpGet("businesses")]
        public async Task<ActionResult<List<BusinessDto>>> GetAllBusinesses()
        {
            try
            {
                var businesses = await _context.Database.SqlQueryRaw<BusinessDto>(@"
                    SELECT business_id AS BusinessId, businessname AS BusinessName, displayID AS DisplayId 
                    FROM dbo.Businesses ORDER BY businessname").ToListAsync();

                return Ok(businesses);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("businesses/{id}")]
        public async Task<ActionResult<BusinessDetailsDto>> GetBusinessById(short id)
        {
            try
            {
                var items = await _context.Database.SqlQueryRaw<BusinessDetailsDto>(@"
                    SELECT 
                        business_id AS BusinessId,
                        businessname AS BusinessName,
                        ISNULL(displayID, '') AS DisplayId,
                        CAST(additionalinfo AS nvarchar(max)) AS AdditionalInfo
                    FROM dbo.Businesses
                    WHERE business_id = {0}", id).ToListAsync();

                var business = items.FirstOrDefault();
                if (business == null) return NotFound(new { message = "Business introuvable" });

                return Ok(business);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("businesses")]
        public async Task<IActionResult> CreateBusiness([FromBody] CreateBusinessRequest req)
        {
            try
            {
                var additionalInfoXml = BuildSimpleXml("PreConfigInfo", req.AdditionalInfo);

                await _context.Database.ExecuteSqlRawAsync(@"
                    INSERT INTO dbo.Businesses (businessname, displayID, additionalinfo) 
                    VALUES (@businessName, @displayId, CONVERT(xml, @additionalInfoXml))",
                    new[]
                    {
                        new Microsoft.Data.SqlClient.SqlParameter("@businessName", req.BusinessName),
                        new Microsoft.Data.SqlClient.SqlParameter("@displayId", (object?)req.DisplayId ?? DBNull.Value),
                        new Microsoft.Data.SqlClient.SqlParameter("@additionalInfoXml", additionalInfoXml)
                    });

                return Ok(new { message = "Business créée avec succès" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("businesses/{id}")]
        public async Task<IActionResult> UpdateBusiness(short id, [FromBody] UpdateBusinessRequest req)
        {
            try
            {
                var additionalInfoXml = BuildSimpleXml("PreConfigInfo", req.AdditionalInfo);

                var rows = await _context.Database.ExecuteSqlRawAsync(@"
                    UPDATE dbo.Businesses
                    SET businessname = @businessName,
                        displayID = @displayId,
                        additionalinfo = CONVERT(xml, @additionalInfoXml)
                    WHERE business_id = @id",
                    new[]
                    {
                        new Microsoft.Data.SqlClient.SqlParameter("@businessName", req.BusinessName),
                        new Microsoft.Data.SqlClient.SqlParameter("@displayId", (object?)req.DisplayId ?? DBNull.Value),
                        new Microsoft.Data.SqlClient.SqlParameter("@additionalInfoXml", additionalInfoXml),
                        new Microsoft.Data.SqlClient.SqlParameter("@id", id)
                    });

                if (rows == 0) return NotFound(new { message = "Business introuvable" });
                return Ok(new { message = "Business modifiée avec succès" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("businesses/{id}")]
        public async Task<IActionResult> DeleteBusiness(short id)
        {
            try
            {
                var rows = await _context.Database.ExecuteSqlRawAsync(
                    "DELETE FROM dbo.Businesses WHERE business_id = {0}", id);

                if (rows == 0) return NotFound(new { message = "Business introuvable" });
                return Ok(new { message = "Business supprimée avec succès" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ====================== BRANCH ======================
        [HttpGet("branches")]
        public async Task<ActionResult<List<BranchDto>>> GetAllBranches()
        {
            try
            {
                var branches = await _context.Database.SqlQueryRaw<BranchDto>(@"
                    SELECT 
                        b.branch_id AS BranchId, 
                        b.branchname AS BranchName, 
                        ISNULL(b.displayID, '') AS DisplayId,
                        CAST(b.additionalinfo AS nvarchar(max)) AS AdditionalInfo,
                        b.business_id AS BusinessId,
                        b.level1_region_id AS Level1RegionId,
                        b.level2_region_id AS Level2RegionId,
                        b.level3_region_id AS Level3RegionId,
                        b.level4_region_id AS Level4RegionId,
                        b.level5_region_id AS Level5RegionId
                    FROM dbo.Branches b
                    ORDER BY b.branchname")
                    .ToListAsync();

                return Ok(branches);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("branches/{id}")]
        public async Task<ActionResult<BranchDto>> GetBranchById(short id)
        {
            try
            {
                var items = await _context.Database.SqlQueryRaw<BranchDto>(@"
                    SELECT 
                        b.branch_id AS BranchId, 
                        b.branchname AS BranchName, 
                        ISNULL(b.displayID, '') AS DisplayId,
                        CAST(b.additionalinfo AS nvarchar(max)) AS AdditionalInfo,
                        b.business_id AS BusinessId,
                        b.level1_region_id AS Level1RegionId,
                        b.level2_region_id AS Level2RegionId,
                        b.level3_region_id AS Level3RegionId,
                        b.level4_region_id AS Level4RegionId,
                        b.level5_region_id AS Level5RegionId
                    FROM dbo.Branches b
                    WHERE b.branch_id = {0}", id).ToListAsync();

                var branch = items.FirstOrDefault();
                if (branch == null) return NotFound(new { message = "Branche introuvable" });

                return Ok(branch);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        private static string BuildSimpleXml(string rootName, string? freeText)
        {
            // Si on reçoit déjà une string XML valide, on l'accepte tel quel.
            if (!string.IsNullOrWhiteSpace(freeText))
            {
                var trimmed = freeText.Trim();
                if (trimmed.StartsWith("<") && trimmed.EndsWith(">"))
                {
                    try
                    {
                        _ = XDocument.Parse(trimmed);
                        return trimmed;
                    }
                    catch
                    {
                        // fallback: encapsulation
                    }
                }
            }

            var doc = new XDocument(
                new XElement(rootName,
                    string.IsNullOrWhiteSpace(freeText) ? null : new XElement("note", freeText.Trim())
                )
            );

            return doc.ToString(SaveOptions.DisableFormatting);
        }

[HttpPost("branches")]
public async Task<IActionResult> CreateBranch([FromBody] CreateBranchRequest req)
{
    try
    {
        var additionalInfoXml = BuildSimpleXml("PreConfigInfo", req.AdditionalInfo);

        await _context.Database.ExecuteSqlRawAsync(@"
            INSERT INTO dbo.Branches 
                (branchname, displayID, additionalinfo, business_id, 
                 level1_region_id, level2_region_id, level3_region_id, 
                 level4_region_id, level5_region_id)
            VALUES 
                (@branchName, @displayId, CONVERT(xml, @additionalInfoXml), @businessId,
                 @level1, @level2, @level3, @level4, @level5)",
            new[]
            {
                new Microsoft.Data.SqlClient.SqlParameter("@branchName", req.BranchName),
                new Microsoft.Data.SqlClient.SqlParameter("@displayId", (object?)req.DisplayId ?? DBNull.Value),
                new Microsoft.Data.SqlClient.SqlParameter("@additionalInfoXml", additionalInfoXml),
                new Microsoft.Data.SqlClient.SqlParameter("@businessId", req.BusinessId),
                new Microsoft.Data.SqlClient.SqlParameter("@level1", req.Level1RegionId),
                new Microsoft.Data.SqlClient.SqlParameter("@level2", req.Level2RegionId),
                new Microsoft.Data.SqlClient.SqlParameter("@level3", req.Level3RegionId),
                new Microsoft.Data.SqlClient.SqlParameter("@level4", req.Level4RegionId),
                new Microsoft.Data.SqlClient.SqlParameter("@level5", req.Level5RegionId)
            });

        return Ok(new { message = "Branche créée avec succès" });
    }
    catch (Exception ex)
    {
        return BadRequest(new { message = ex.Message });
    }
}

[HttpDelete("branches/{id}")]
public async Task<IActionResult> DeleteBranch(short id)
{
    try
    {
        var rows = await _context.Database.ExecuteSqlRawAsync(
            "DELETE FROM dbo.Branches WHERE branch_id = {0}", id);

        if (rows == 0) return NotFound(new { message = "Branche introuvable" });
        return Ok(new { message = "Branche supprimée avec succès" });
    }
    catch (Exception ex)
    {
        return BadRequest(new { message = ex.Message });
    }
}
[HttpPut("branches/{id}")]
public async Task<IActionResult> UpdateBranch(short id, [FromBody] UpdateBranchRequest req)
{
    try
    {
        var additionalInfoXml = BuildSimpleXml("PreConfigInfo", req.AdditionalInfo);

        var rows = await _context.Database.ExecuteSqlRawAsync(@"
            UPDATE dbo.Branches
            SET branchname          = @branchName,
                displayID           = @displayId,
                additionalinfo      = CONVERT(xml, @additionalInfoXml),
                business_id         = @businessId,
                level1_region_id    = @level1,
                level2_region_id    = @level2,
                level3_region_id    = @level3,
                level4_region_id    = @level4,
                level5_region_id    = @level5
            WHERE branch_id = @id",
            new[]
            {
                new Microsoft.Data.SqlClient.SqlParameter("@branchName", req.BranchName),
                new Microsoft.Data.SqlClient.SqlParameter("@displayId", (object?)req.DisplayId ?? DBNull.Value),
                new Microsoft.Data.SqlClient.SqlParameter("@additionalInfoXml", additionalInfoXml),
                new Microsoft.Data.SqlClient.SqlParameter("@businessId", req.BusinessId),
                new Microsoft.Data.SqlClient.SqlParameter("@level1", req.Level1RegionId),
                new Microsoft.Data.SqlClient.SqlParameter("@level2", req.Level2RegionId),
                new Microsoft.Data.SqlClient.SqlParameter("@level3", req.Level3RegionId),
                new Microsoft.Data.SqlClient.SqlParameter("@level4", req.Level4RegionId),
                new Microsoft.Data.SqlClient.SqlParameter("@level5", req.Level5RegionId),
                new Microsoft.Data.SqlClient.SqlParameter("@id", id)
            });

        if (rows == 0) return NotFound(new { message = "Branche introuvable" });
        return Ok(new { message = "Branche modifiée avec succès" });
    }
    catch (Exception ex)
    {
        return BadRequest(new { message = ex.Message });
    }
}
        // ====================== CLIENT / ATM ======================
        [HttpGet("clients")]
        public async Task<ActionResult<List<ClientAtmDto>>> GetAllClients()
        {
            try
            {
                var clients = await _context.Clients
                    .FromSqlRaw(@"
                        SELECT 
                            c.client_id AS ClientId,
                            c.ktcguid AS KtcGuid,
                            c.clientname AS ClientName,
                            c.networkaddress AS NetworkAddress,
                            c.connectable AS Connectable,
                            c.detailsunknown AS DetailsUnknown,
                            c.latitude AS Latitude,
                            c.longitude AS Longitude,
                            c.timezone AS Timezone,
                            CAST(c.comments AS nvarchar(max)) AS Comments,
                            c.business_id AS BusinessId,
                            c.branch_id AS BranchId,
                            c.hardwaretype_id AS HardwareTypeId,
                            ht.name AS HardwareTypeName,
                            c.active AS Active,
                            c.clienttype AS ClientType
                        FROM dbo.Clients c
                        LEFT JOIN dbo.HardwareTypes ht ON c.hardwaretype_id = ht.hardwaretype_id
                        ORDER BY c.clientname")
                    .ToListAsync();

                return Ok(clients);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("clients")]
        public async Task<IActionResult> CreateClient([FromBody] CreateOrUpdateAtmRequest req)
        {
            try
            {
                var commentsXml = BuildSimpleXml("comments", req.Comments);

                await _context.Database.ExecuteSqlRawAsync(@"
                    INSERT INTO dbo.Clients 
                        (ktcguid, connectable, networkaddress, clientname, detailsunknown,
                         latitude, longitude, timezone, comments, clienttype, gridposition,
                         business_id, branch_id, hardwaretype_id, owner_id, deletelater, active,
                         subnet, level1_region_id, level2_region_id, level3_region_id, level4_region_id, level5_region_id,
                         salt, authhash, hypervisor_active, mergeto_client_id, feature_flags)
                    VALUES 
                        (CONVERT(nvarchar(36), NEWID()), @connectable, @networkAddress, @clientName, @detailsUnknown,
                         @latitude, @longitude, @timezone, CONVERT(xml, @commentsXml), @clientType, @gridPosition,
                         @businessId, @branchId, @hardwareTypeId, @ownerId, @deleteLater, @active,
                         @subnet, @level1RegionId, @level2RegionId, @level3RegionId, @level4RegionId, @level5RegionId,
                         @salt, @authHash, @hypervisorActive, @mergeToClientId, @featureFlags)",
                    new[]
                    {
                        new Microsoft.Data.SqlClient.SqlParameter("@connectable", req.Connectable),
                        new Microsoft.Data.SqlClient.SqlParameter("@networkAddress", req.NetworkAddress),
                        new Microsoft.Data.SqlClient.SqlParameter("@clientName", req.ClientName),
                        new Microsoft.Data.SqlClient.SqlParameter("@detailsUnknown", req.DetailsUnknown),
                        new Microsoft.Data.SqlClient.SqlParameter("@latitude", req.Latitude),
                        new Microsoft.Data.SqlClient.SqlParameter("@longitude", req.Longitude),
                        new Microsoft.Data.SqlClient.SqlParameter("@timezone", req.Timezone),
                        new Microsoft.Data.SqlClient.SqlParameter("@commentsXml", commentsXml),
                        new Microsoft.Data.SqlClient.SqlParameter("@clientType", req.ClientType),
                        new Microsoft.Data.SqlClient.SqlParameter("@gridPosition", req.GridPosition),
                        new Microsoft.Data.SqlClient.SqlParameter("@businessId", req.BusinessId),
                        new Microsoft.Data.SqlClient.SqlParameter("@branchId", req.BranchId),
                        new Microsoft.Data.SqlClient.SqlParameter("@hardwareTypeId", req.HardwareTypeId),
                        new Microsoft.Data.SqlClient.SqlParameter("@ownerId", req.OwnerId),
                        new Microsoft.Data.SqlClient.SqlParameter("@deleteLater", req.DeleteLater),
                        new Microsoft.Data.SqlClient.SqlParameter("@active", req.Active),
                        new Microsoft.Data.SqlClient.SqlParameter("@subnet", req.Subnet),
                        new Microsoft.Data.SqlClient.SqlParameter("@level1RegionId", req.Level1RegionId),
                        new Microsoft.Data.SqlClient.SqlParameter("@level2RegionId", req.Level2RegionId),
                        new Microsoft.Data.SqlClient.SqlParameter("@level3RegionId", req.Level3RegionId),
                        new Microsoft.Data.SqlClient.SqlParameter("@level4RegionId", req.Level4RegionId),
                        new Microsoft.Data.SqlClient.SqlParameter("@level5RegionId", req.Level5RegionId),
                        new Microsoft.Data.SqlClient.SqlParameter("@salt", req.Salt),
                        new Microsoft.Data.SqlClient.SqlParameter("@authHash", req.AuthHash),
                        new Microsoft.Data.SqlClient.SqlParameter("@hypervisorActive", req.HypervisorActive),
                        new Microsoft.Data.SqlClient.SqlParameter("@mergeToClientId", req.MergeToClientId),
                        new Microsoft.Data.SqlClient.SqlParameter("@featureFlags", req.FeatureFlags)
                    });

                return Ok(new { message = "ATM créé avec succès" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("clients/{id}")]
        public async Task<ActionResult<ClientAtmDto>> GetClientById(int id)
        {
            try
            {
                var items = await _context.Database.SqlQueryRaw<ClientAtmDto>(@"
                    SELECT 
                        c.client_id AS ClientId,
                        c.ktcguid AS KtcGuid,
                        c.clientname AS ClientName,
                        c.networkaddress AS NetworkAddress,
                        c.connectable AS Connectable,
                        c.detailsunknown AS DetailsUnknown,
                        c.latitude AS Latitude,
                        c.longitude AS Longitude,
                        c.timezone AS Timezone,
                        CAST(c.comments AS nvarchar(max)) AS Comments,
                        c.business_id AS BusinessId,
                        c.branch_id AS BranchId,
                        c.hardwaretype_id AS HardwareTypeId,
                        ht.name AS HardwareTypeName,
                        c.active AS Active,
                        c.clienttype AS ClientType
                    FROM dbo.Clients c
                    LEFT JOIN dbo.HardwareTypes ht ON c.hardwaretype_id = ht.hardwaretype_id
                    WHERE c.client_id = {0}", id).ToListAsync();

                var client = items.FirstOrDefault();
                if (client == null) return NotFound(new { message = "ATM introuvable" });
                return Ok(client);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("clients/{id}/status")]
        public async Task<ActionResult<List<AtmComponentStatusDto>>> GetAtmStatus(int id)
        {
            try
            {
                var status = await _atmRepository.GetAtmStatusAsync(id);
                return Ok(status);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("clients/{id}/assethistory")]
        public async Task<ActionResult<List<AtmAssetHistoryDto>>> GetAtmAssetHistory(int id)
        {
            try
            {
                var history = await _atmRepository.GetAtmAssetHistoryAsync(id);
                return Ok(history);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("clients/{id}/lastcontact")]
        public async Task<ActionResult<LastClientContactDto>> GetLastClientContact(int id)
        {
            try
            {
                var lastContact = await _atmRepository.GetLastClientContactAsync(id);
                if (lastContact == null)
                {
                    return NotFound(new { message = "Aucun dernier contact trouvé pour cet ATM." });
                }
                return Ok(lastContact);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("clients/{id}/softwareinfo")]
        public async Task<ActionResult<List<AtmSoftwareInfoDto>>> GetAtmSoftwareInfo(int id)
        {
            try
            {
                var softwareInfo = await _atmRepository.GetAtmSoftwareInfoAsync(id);
                return Ok(softwareInfo);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("clients/{id}/certificates")]
        public async Task<ActionResult<List<AtmCertificateDto>>> GetAtmCertificates(int id)
        {
            try
            {
                var certificates = await _atmRepository.GetAtmCertificatesAsync(id);
                return Ok(certificates);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("clients/{id}/tickets")]
        public async Task<ActionResult<List<AtmTicketDto>>> GetAtmTickets(int id, [FromQuery] int days = 14, [FromQuery] string statusFilter = "All")
        {
            try
            {
                // Validate parameters
                if (days < 1 || days > 365)
                {
                    days = 14;
                }

                if (!new[] { "All", "Open/Dispatched", "Closed" }.Contains(statusFilter))
                {
                    statusFilter = "All";
                }

                var tickets = await _atmRepository.GetAtmTicketsAsync(id, days, statusFilter);
                return Ok(tickets);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Erreur: {ex.Message}", details = ex.InnerException?.Message });
            }
        }

        [HttpGet("clients/{id}/tickets-debug")]
        public async Task<ActionResult> GetAtmTicketsDebug(int id)
        {
            try
            {
                // Test 1: Check if TroubleTickets table exists
                var tableCheck = await _context.Database.SqlQueryRaw<dynamic>(@"
                    SELECT TABLE_NAME 
                    FROM INFORMATION_SCHEMA.TABLES 
                    WHERE TABLE_NAME LIKE '%Ticket%' OR TABLE_NAME LIKE '%ticket%'").ToListAsync();

                // Test 3: Get first few columns from TroubleTickets
                var sampleColumns = await _context.Database.SqlQueryRaw<dynamic>(@"
                    SELECT COLUMN_NAME 
                    FROM INFORMATION_SCHEMA.COLUMNS 
                    WHERE TABLE_NAME = 'TroubleTickets' 
                    ORDER BY ORDINAL_POSITION").ToListAsync();

                // Test 2: Try a simple first ticket query
                var sampleTicket = await _context.Database.SqlQueryRaw<dynamic>(@"
                    SELECT TOP 1 * FROM dbo.TroubleTickets WHERE client_id = {0}", id).FirstOrDefaultAsync();

                return Ok(new
                {
                    tablesFound = tableCheck.Count,
                    tables = tableCheck,
                    columnCount = sampleColumns.Count,
                    sampleColumns = sampleColumns.Take(10),
                    firstTicketForClient = sampleTicket ?? "Aucun ticket trouvé"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message, innerError = ex.InnerException?.Message });
            }
        }

        [HttpPut("clients/{id}")]
        public async Task<IActionResult> UpdateClient(int id, [FromBody] CreateOrUpdateAtmRequest req)
        {
            try
            {
                var commentsXml = BuildSimpleXml("comments", req.Comments);

                var rows = await _context.Database.ExecuteSqlRawAsync(@"
                    UPDATE dbo.Clients
                    SET connectable = @connectable,
                        networkaddress = @networkAddress,
                        clientname = @clientName,
                        detailsunknown = @detailsUnknown,
                        latitude = @latitude,
                        longitude = @longitude,
                        timezone = @timezone,
                        comments = CONVERT(xml, @commentsXml),
                        clienttype = @clientType,
                        gridposition = @gridPosition,
                        business_id = @businessId,
                        branch_id = @branchId,
                        hardwaretype_id = @hardwareTypeId,
                        owner_id = @ownerId,
                        deletelater = @deleteLater,
                        active = @active,
                        subnet = @subnet,
                        level1_region_id = @level1RegionId,
                        level2_region_id = @level2RegionId,
                        level3_region_id = @level3RegionId,
                        level4_region_id = @level4RegionId,
                        level5_region_id = @level5RegionId,
                        salt = @salt,
                        authhash = @authHash,
                        hypervisor_active = @hypervisorActive,
                        mergeto_client_id = @mergeToClientId,
                        feature_flags = @featureFlags
                    WHERE client_id = @id",
                    new[]
                    {
                        new Microsoft.Data.SqlClient.SqlParameter("@connectable", req.Connectable),
                        new Microsoft.Data.SqlClient.SqlParameter("@networkAddress", req.NetworkAddress),
                        new Microsoft.Data.SqlClient.SqlParameter("@clientName", req.ClientName),
                        new Microsoft.Data.SqlClient.SqlParameter("@detailsUnknown", req.DetailsUnknown),
                        new Microsoft.Data.SqlClient.SqlParameter("@latitude", req.Latitude),
                        new Microsoft.Data.SqlClient.SqlParameter("@longitude", req.Longitude),
                        new Microsoft.Data.SqlClient.SqlParameter("@timezone", req.Timezone),
                        new Microsoft.Data.SqlClient.SqlParameter("@commentsXml", commentsXml),
                        new Microsoft.Data.SqlClient.SqlParameter("@clientType", req.ClientType),
                        new Microsoft.Data.SqlClient.SqlParameter("@gridPosition", req.GridPosition),
                        new Microsoft.Data.SqlClient.SqlParameter("@businessId", req.BusinessId),
                        new Microsoft.Data.SqlClient.SqlParameter("@branchId", req.BranchId),
                        new Microsoft.Data.SqlClient.SqlParameter("@hardwareTypeId", req.HardwareTypeId),
                        new Microsoft.Data.SqlClient.SqlParameter("@ownerId", req.OwnerId),
                        new Microsoft.Data.SqlClient.SqlParameter("@deleteLater", req.DeleteLater),
                        new Microsoft.Data.SqlClient.SqlParameter("@active", req.Active),
                        new Microsoft.Data.SqlClient.SqlParameter("@subnet", req.Subnet),
                        new Microsoft.Data.SqlClient.SqlParameter("@level1RegionId", req.Level1RegionId),
                        new Microsoft.Data.SqlClient.SqlParameter("@level2RegionId", req.Level2RegionId),
                        new Microsoft.Data.SqlClient.SqlParameter("@level3RegionId", req.Level3RegionId),
                        new Microsoft.Data.SqlClient.SqlParameter("@level4RegionId", req.Level4RegionId),
                        new Microsoft.Data.SqlClient.SqlParameter("@level5RegionId", req.Level5RegionId),
                        new Microsoft.Data.SqlClient.SqlParameter("@salt", req.Salt),
                        new Microsoft.Data.SqlClient.SqlParameter("@authHash", req.AuthHash),
                        new Microsoft.Data.SqlClient.SqlParameter("@hypervisorActive", req.HypervisorActive),
                        new Microsoft.Data.SqlClient.SqlParameter("@mergeToClientId", req.MergeToClientId),
                        new Microsoft.Data.SqlClient.SqlParameter("@featureFlags", req.FeatureFlags),
                        new Microsoft.Data.SqlClient.SqlParameter("@id", id),
                    });

                if (rows == 0) return NotFound(new { message = "ATM introuvable" });
                return Ok(new { message = "ATM modifié avec succès" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("clients/{id}")]
        public async Task<IActionResult> DeleteClient(int id)
        {
            try
            {
                var rows = await _context.Database.ExecuteSqlRawAsync(
                    "DELETE FROM dbo.Clients WHERE client_id = {0}", id);

                if (rows == 0) return NotFound(new { message = "ATM introuvable" });
                return Ok(new { message = "ATM supprimé avec succès" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("hardwaretypes")]
        public async Task<ActionResult<List<HardwareTypeDto>>> GetHardwareTypes()
        {
            try
            {
                var types = await _context.Database.SqlQueryRaw<HardwareTypeDto>(@"
                    SELECT
                        hardwaretype_id AS HardwareTypeId,
                        name AS Name,
                        description AS Description,
                        typegroup AS TypeGroup,
                        canbeconfigured AS CanBeConfigured,
                        canbemonitored AS CanBeMonitored
                    FROM dbo.HardwareTypes
                    ORDER BY name").ToListAsync();

                return Ok(types);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("businesses/{businessId}/hardwaretypes")]
        public async Task<ActionResult<List<HardwareTypeDto>>> GetHardwareTypesByBusiness(short businessId)
        {
            try
            {
                var types = await _context.Database.SqlQueryRaw<HardwareTypeDto>(@"
                    SELECT
                        ht.hardwaretype_id AS HardwareTypeId,
                        ht.name AS Name,
                        ht.description AS Description,
                        ht.typegroup AS TypeGroup,
                        ht.canbeconfigured AS CanBeConfigured,
                        ht.canbemonitored AS CanBeMonitored
                    FROM dbo.BusinessHardwareTypes bht
                    INNER JOIN dbo.HardwareTypes ht ON ht.hardwaretype_id = bht.hardwaretype_id
                    WHERE bht.business_id = {0}
                    ORDER BY ht.name", businessId).ToListAsync();

                return Ok(types);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}

