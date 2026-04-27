
using System.Collections.Generic;
using System.Threading.Tasks;
using KtcWeb.Application.DTOs;

namespace KtcWeb.Domain.Interfaces
{
    public interface IAtmRepository
    {
        Task<List<AtmComponentStatusDto>> GetAtmStatusAsync(int clientId);
        Task<List<AtmAssetHistoryDto>> GetAtmAssetHistoryAsync(int clientId);
        Task<LastClientContactDto?> GetLastClientContactAsync(int clientId);
        Task<List<AtmSoftwareInfoDto>> GetAtmSoftwareInfoAsync(int clientId);
        Task<List<AtmCertificateDto>> GetAtmCertificatesAsync(int clientId);
        Task<List<AtmTicketDto>> GetAtmTicketsAsync(int clientId, int days, string statusFilter);
    }
}


