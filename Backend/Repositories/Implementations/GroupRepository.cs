using KtcWeb.Application.DTOs;
using KtcWeb.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace KtcWeb.Infrastructure.Repositories
{
    public class GroupRepository : IGroupRepository
    {
        private const string SelectGroupColumns = @"
            SELECT CAST(group_id AS INT) AS GroupId,
                   groupname AS GroupName,
                   CAST(ISNULL(grouptype_id, 0) AS INT) AS GroupTypeId,
                   groupquery AS GroupQuery,
                   groupdescription AS GroupDescription,
                   ISNULL(include_mothballed, 0) AS IncludeMothballed,
                   CAST(ISNULL(evaluation_interval, 0) AS INT) AS EvaluationInterval,
                   last_changed_timestamp AS LastChangedTimestamp
            FROM [KALKTCDB].[dbo].[Groups]";

        private readonly KtcDbContext _context;

        public GroupRepository(KtcDbContext context)
        {
            _context = context;
        }

        public Task<List<GroupDto>> GetAllGroupsAsync() =>
            _context.Database
                .SqlQueryRaw<GroupDto>(SelectGroupColumns + " ORDER BY groupname")
                .ToListAsync();

        public Task<GroupDto?> GetGroupByIdAsync(int groupId) =>
            _context.Database
                .SqlQueryRaw<GroupDto>(SelectGroupColumns + " WHERE group_id = {0}", groupId)
                .FirstOrDefaultAsync();

        public Task<List<ClientSimpleDto>> GetAllClientsAsync() =>
            _context.Database.SqlQueryRaw<ClientSimpleDto>(@"
                SELECT c.client_id AS ClientId, c.clientname AS ClientName,
                       c.networkaddress AS NetworkAddress, c.active AS Active
                FROM [KALKTCDB].[dbo].[Clients] c WHERE c.client_id > 0
                ORDER BY c.clientname").ToListAsync();

        public Task<List<ClientSimpleDto>> GetClientsByGroupAsync(int groupId) =>
            _context.Database.SqlQueryRaw<ClientSimpleDto>(@"
                SELECT c.client_id AS ClientId, c.clientname AS ClientName,
                       c.networkaddress AS NetworkAddress, c.active AS Active
                FROM [KALKTCDB].[dbo].[Clients] c
                INNER JOIN [KALKTCDB].[dbo].[ClientGroups] cg ON c.client_id = cg.client_id
                WHERE cg.group_id = {0}
                ORDER BY c.clientname", groupId).ToListAsync();

        public Task CreateGroupAsync(CreateGroupRequest r) =>
            _context.Database.ExecuteSqlInterpolatedAsync($@"
                INSERT INTO [KALKTCDB].[dbo].[Groups]
                (groupname, grouptype_id, groupquery, groupdescription, include_mothballed, evaluation_interval, last_changed_timestamp)
                VALUES ({r.GroupName}, {r.GroupTypeId}, {r.GroupQuery},
                        {r.GroupDescription}, {r.IncludeMothballed ?? false},
                        {r.EvaluationInterval}, {DateTime.Now})");

        public Task UpdateGroupAsync(UpdateGroupRequest r) =>
            _context.Database.ExecuteSqlInterpolatedAsync($@"
                UPDATE [KALKTCDB].[dbo].[Groups]
                SET groupname = {r.GroupName},
                    grouptype_id = {r.GroupTypeId},
                    groupquery = {r.GroupQuery},
                    groupdescription = {r.GroupDescription},
                    include_mothballed = {r.IncludeMothballed ?? false},
                    evaluation_interval = {r.EvaluationInterval},
                    last_changed_timestamp = {DateTime.Now}
                WHERE group_id = {r.GroupId}");

        public Task AddClientToGroupAsync(int groupId, int clientId) =>
            _context.Database.ExecuteSqlInterpolatedAsync($@"
                INSERT INTO [KALKTCDB].[dbo].[ClientGroups] (group_id, client_id)
                VALUES ({groupId}, {clientId})");

        public Task RemoveClientFromGroupAsync(int groupId, int clientId) =>
            _context.Database.ExecuteSqlInterpolatedAsync($@"
                DELETE FROM [KALKTCDB].[dbo].[ClientGroups]
                WHERE group_id = {groupId} AND client_id = {clientId}");

        public Task DeleteClientGroupsAsync(int groupId) =>
            _context.Database.ExecuteSqlInterpolatedAsync($@"
                DELETE FROM [KALKTCDB].[dbo].[ClientGroups] WHERE group_id = {groupId}");

        public Task DeleteGroupAsync(int groupId) =>
            _context.Database.ExecuteSqlInterpolatedAsync($@"
                DELETE FROM [KALKTCDB].[dbo].[Groups] WHERE group_id = {groupId}");
    }
}
