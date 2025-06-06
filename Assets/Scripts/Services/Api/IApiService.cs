using System.Threading.Tasks;
using ChaosCosmos.Core.Services;

namespace ChaosCosmos.Services.Api
{
    public interface IApiService : IService
    {
        Task<string> GetAsync(string endpoint);
        Task<string> PostAsync(string endpoint, string jsonPayload);
    }
}
