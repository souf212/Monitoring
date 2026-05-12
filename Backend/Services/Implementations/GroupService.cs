using KtcWeb.Application.DTOs;
using KtcWeb.Application.Interfaces;
using KtcWeb.Domain.Interfaces;

namespace KtcWeb.Application.Services
{
    public class GroupService : IGroupService
    {
        private readonly IGroupRepository _repo;

        public GroupService(IGroupRepository repo)
        {
            _repo = repo;
        }

        public Task<List<GroupDto>> GetAllGroupsAsync() =>
            _repo.GetAllGroupsAsync();

        public async Task<List<ClientSimpleDto>> GetGroupClientsAsync(int groupId)
        {
            var group = await _repo.GetGroupByIdAsync(groupId)
                ?? throw new KeyNotFoundException($"Groupe {groupId} non trouvé");

            return group.GroupTypeId == 1
                ? await _repo.GetAllClientsAsync()
                : await _repo.GetClientsByGroupAsync(groupId);
        }

        public async Task<GroupDetailsDto?> GetGroupDetailsAsync(int groupId)
        {
            var group = await _repo.GetGroupByIdAsync(groupId);
            if (group == null)
                return null;

            var clients = group.GroupTypeId == 1
                ? await _repo.GetAllClientsAsync()
                : await _repo.GetClientsByGroupAsync(groupId);

            return new GroupDetailsDto
            {
                GroupId              = group.GroupId,
                GroupName            = group.GroupName,
                GroupTypeId          = group.GroupTypeId,
                GroupQuery           = group.GroupQuery,
                GroupDescription     = group.GroupDescription,
                IncludeMothballed    = group.IncludeMothballed,
                EvaluationInterval   = group.EvaluationInterval,
                LastChangedTimestamp = group.LastChangedTimestamp,
                Clients              = clients
            };
        }

        public Task CreateGroupAsync(CreateGroupRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.GroupName))
                throw new InvalidOperationException("Le nom du groupe est requis");

            return _repo.CreateGroupAsync(request);
        }

        public Task UpdateGroupAsync(UpdateGroupRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.GroupName))
                throw new InvalidOperationException("Le nom du groupe est requis");

            return _repo.UpdateGroupAsync(request);
        }

        public Task AddClientToGroupAsync(int groupId, int clientId) =>
            _repo.AddClientToGroupAsync(groupId, clientId);

        public Task RemoveClientFromGroupAsync(int groupId, int clientId) =>
            _repo.RemoveClientFromGroupAsync(groupId, clientId);

        public async Task DeleteGroupAsync(int groupId)
        {
            await _repo.DeleteClientGroupsAsync(groupId);
            await _repo.DeleteGroupAsync(groupId);
        }
    }
}
