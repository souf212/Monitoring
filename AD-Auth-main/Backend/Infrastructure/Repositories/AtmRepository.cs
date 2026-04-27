

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
    }
}


