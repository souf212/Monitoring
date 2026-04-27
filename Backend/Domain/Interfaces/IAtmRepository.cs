
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KtcWeb.Domain.Interfaces
{
    public interface IAtmRepository
    {
        Task<List<AtmComponentStatusDto>> GetAtmStatusAsync(int clientId);
        Task<List<AtmAssetHistoryDto>> GetAtmAssetHistoryAsync(int clientId);
    }
}


