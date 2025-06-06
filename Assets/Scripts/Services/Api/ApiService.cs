using System.Threading.Tasks;
using UnityEngine.Networking;
using UnityEngine;
using ChaosCosmos.Core.Services;

namespace ChaosCosmos.Services.Api
{
    public class ApiService : IApiService
    {
        private readonly string _baseUrl;

        public ApiService(string baseUrl)
        {
            _baseUrl = baseUrl.TrimEnd('/');
        }

        public async Task<string> GetAsync(string endpoint)
        {
            string url = $"{_baseUrl}/{endpoint.TrimStart('/')}";
            using UnityWebRequest request = UnityWebRequest.Get(url);
            var operation = request.SendWebRequest();
            while (!operation.isDone)
            {
                await Task.Yield();
            }
            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"ApiService GET error: {request.error}");
                return null;
            }
            return request.downloadHandler.text;
        }

        public async Task<string> PostAsync(string endpoint, string jsonPayload)
        {
            string url = $"{_baseUrl}/{endpoint.TrimStart('/')}";
            using UnityWebRequest request = new UnityWebRequest(url, "POST");
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonPayload ?? "");
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            var operation = request.SendWebRequest();
            while (!operation.isDone)
            {
                await Task.Yield();
            }
            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"ApiService POST error: {request.error}");
                return null;
            }
            return request.downloadHandler.text;
        }
    }
}
