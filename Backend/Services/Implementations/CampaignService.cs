using KtcWeb.Application.DTOs;
using KtcWeb.Application.Interfaces;
using KtcWeb.Domain.Interfaces;

namespace KtcWeb.Application.Services
{
    public class CampaignService : ICampaignService
    {
        private readonly ICampaignRepository _repo;

        public CampaignService(ICampaignRepository repo)
        {
            _repo = repo;
        }

        public Task<List<CampaignDto>> GetAllCampaignsAsync() =>
            _repo.GetAllCampaignsAsync();

        public Task<CampaignDto?> GetCampaignByIdAsync(int campaignId) =>
            _repo.GetCampaignByIdAsync(campaignId);

        public async Task<CampaignDto?> CreateCampaignAsync(CreateCampaignRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                throw new InvalidOperationException("Le nom de la campagne est obligatoire");

            await _repo.CreateCampaignAsync(request);
            return await _repo.GetCampaignByNameLatestAsync(request.Name);
        }

        public async Task<CampaignDto?> UpdateCampaignAsync(int campaignId, CreateCampaignRequest request)
        {
            await _repo.UpdateCampaignAsync(campaignId, request);
            return await _repo.GetCampaignByIdAsync(campaignId);
        }

        public async Task<bool> DeleteCampaignAsync(int campaignId)
        {
            var rows = await _repo.DeleteCampaignAsync(campaignId);
            return rows > 0;
        }

        public Task<List<CampaignBusinessDto>> GetCampaignBusinessesAsync(int campaignId) =>
            _repo.GetCampaignBusinessesAsync(campaignId);

        public Task<List<CampaignGroupDto>> GetCampaignGroupsAsync(int campaignId) =>
            _repo.GetCampaignGroupsAsync(campaignId);

        public Task<List<CampaignBINRangeDto>> GetCampaignBINRangesAsync(int campaignId) =>
            _repo.GetCampaignBINRangesAsync(campaignId);

        public Task<List<CampaignShownCountDto>> GetCampaignShownCountsAsync(int campaignId) =>
            _repo.GetCampaignShownCountsAsync(campaignId);
    }
}
