using KtcWeb.Application.DTOs;

namespace KtcWeb.Domain.Interfaces
{
    public interface ICampaignRepository
    {
        Task<List<CampaignDto>> GetAllCampaignsAsync();
        Task<CampaignDto?> GetCampaignByIdAsync(int campaignId);
        Task CreateCampaignAsync(CreateCampaignRequest request);
        Task<CampaignDto?> GetCampaignByNameLatestAsync(string name);
        Task UpdateCampaignAsync(int campaignId, CreateCampaignRequest request);
        Task<int> DeleteCampaignAsync(int campaignId);
        Task<List<CampaignBusinessDto>> GetCampaignBusinessesAsync(int campaignId);
        Task<List<CampaignGroupDto>> GetCampaignGroupsAsync(int campaignId);
        Task<List<CampaignBINRangeDto>> GetCampaignBINRangesAsync(int campaignId);
        Task<List<CampaignShownCountDto>> GetCampaignShownCountsAsync(int campaignId);
    }
}
