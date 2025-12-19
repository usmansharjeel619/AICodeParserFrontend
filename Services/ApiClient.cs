using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using AICodeParser.Models;
using Microsoft.Extensions.Configuration;

namespace AICodeParser.Services
{
    public interface IApiClient
    {
        Task<bool> CheckHealthAsync();
        Task<DebugResponse> DebugCodeAsync(string code, string filename);
        Task<StatusResponse> GetDebugStatusAsync();
        Task<NlpResponse> GenerateTestsAsync(string specifications);
        Task<FunctionsResponse> GetFunctionsAsync();
        Task<StatusResponse> GetNlpStatusAsync();
        Task<FormalResponse> VerifyCodeAsync(string code, string functionName);
        Task<StatusResponse> GetFormalStatusAsync();
    }

    public class ApiClient : IApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly string _baseUrl;

        public ApiClient(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _baseUrl = _configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5000";

            var timeout = _configuration.GetValue<int>("ApiSettings:Timeout", 300);
            _httpClient.Timeout = TimeSpan.FromSeconds(timeout);
        }

        public async Task<bool> CheckHealthAsync()
        {
            try
            {
                var endpoint = _configuration["ApiSettings:Endpoints:Health"];
                var response = await _httpClient.GetAsync($"{_baseUrl}{endpoint}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var healthResponse = JsonConvert.DeserializeObject<HealthResponse>(content);
                    return healthResponse?.Status == "running";
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public async Task<DebugResponse> DebugCodeAsync(string code, string filename)
        {
            try
            {
                var endpoint = _configuration["ApiSettings:Endpoints:Debug"];
                var request = new DebugRequest
                {
                    Code = code,
                    Filename = filename
                };

                var json = JsonConvert.SerializeObject(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{_baseUrl}{endpoint}", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return JsonConvert.DeserializeObject<DebugResponse>(responseContent)
                           ?? new DebugResponse();
                }
                else
                {
                    var errorResponse = JsonConvert.DeserializeObject<ApiResponse<object>>(responseContent);
                    throw new Exception(errorResponse?.Error ?? "Unknown error occurred");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to debug code: {ex.Message}", ex);
            }
        }

        public async Task<StatusResponse> GetDebugStatusAsync()
        {
            return await GetStatusAsync(_configuration["ApiSettings:Endpoints:DebugStatus"]);
        }

        public async Task<NlpResponse> GenerateTestsAsync(string specifications)
        {
            try
            {
                var endpoint = _configuration["ApiSettings:Endpoints:NlpGenerate"];
                var request = new NlpRequest
                {
                    Specifications = specifications
                };

                var json = JsonConvert.SerializeObject(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{_baseUrl}{endpoint}", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return JsonConvert.DeserializeObject<NlpResponse>(responseContent)
                           ?? new NlpResponse();
                }
                else
                {
                    var errorResponse = JsonConvert.DeserializeObject<ApiResponse<object>>(responseContent);
                    throw new Exception(errorResponse?.Error ?? "Unknown error occurred");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to generate tests: {ex.Message}", ex);
            }
        }

        public async Task<FunctionsResponse> GetFunctionsAsync()
        {
            try
            {
                var endpoint = _configuration["ApiSettings:Endpoints:NlpFunctions"];
                var response = await _httpClient.GetAsync($"{_baseUrl}{endpoint}");
                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return JsonConvert.DeserializeObject<FunctionsResponse>(content)
                           ?? new FunctionsResponse();
                }
                else
                {
                    var errorResponse = JsonConvert.DeserializeObject<ApiResponse<object>>(content);
                    throw new Exception(errorResponse?.Error ?? "Unknown error occurred");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get functions: {ex.Message}", ex);
            }
        }

        public async Task<StatusResponse> GetNlpStatusAsync()
        {
            return await GetStatusAsync(_configuration["ApiSettings:Endpoints:NlpStatus"]);
        }

        public async Task<FormalResponse> VerifyCodeAsync(string code, string functionName)
        {
            try
            {
                var endpoint = _configuration["ApiSettings:Endpoints:FormalVerify"];
                var request = new FormalRequest
                {
                    Code = code,
                    FunctionName = functionName
                };

                var json = JsonConvert.SerializeObject(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{_baseUrl}{endpoint}", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return JsonConvert.DeserializeObject<FormalResponse>(responseContent)
                           ?? new FormalResponse();
                }
                else
                {
                    var errorResponse = JsonConvert.DeserializeObject<ApiResponse<object>>(responseContent);
                    throw new Exception(errorResponse?.Error ?? "Unknown error occurred");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to verify code: {ex.Message}", ex);
            }
        }

        public async Task<StatusResponse> GetFormalStatusAsync()
        {
            return await GetStatusAsync(_configuration["ApiSettings:Endpoints:FormalStatus"]);
        }

        private async Task<StatusResponse> GetStatusAsync(string endpoint)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}{endpoint}");
                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return JsonConvert.DeserializeObject<StatusResponse>(content)
                           ?? new StatusResponse();
                }
                else
                {
                    var errorResponse = JsonConvert.DeserializeObject<ApiResponse<object>>(content);
                    throw new Exception(errorResponse?.Error ?? "Unknown error occurred");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get status: {ex.Message}", ex);
            }
        }
    }
}