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
        Task<NlpResponse> AnalyzeCodeAsync(string code);
        Task<FormalResponse> VerifyCodeAsync(string code, string functionName);
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
            _httpClient.DefaultRequestHeaders.Accept.Add(
                new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
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
                    var debugResponse = JsonConvert.DeserializeObject<DebugResponse>(responseContent);
                    return debugResponse ?? new DebugResponse { Status = "Error parsing response" };
                }
                else
                {
                    return new DebugResponse
                    {
                        Status = $"Error: {response.StatusCode}",
                        CompilationAnalysis = new CompilationAnalysis
                        {
                            Errors = responseContent
                        }
                    };
                }
            }
            catch (Exception ex)
            {
                return new DebugResponse
                {
                    Status = "Error",
                    CompilationAnalysis = new CompilationAnalysis
                    {
                        Errors = $"Failed to debug code: {ex.Message}"
                    }
                };
            }
        }

        public async Task<NlpResponse> AnalyzeCodeAsync(string code)
        {
            try
            {
                var endpoint = _configuration["ApiSettings:Endpoints:NlpAnalyze"];
                var request = new NlpRequest
                {
                    Code = code
                };

                var json = JsonConvert.SerializeObject(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{_baseUrl}{endpoint}", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var nlpResponse = JsonConvert.DeserializeObject<NlpResponse>(responseContent);
                    return nlpResponse ?? new NlpResponse { Status = "Error parsing response" };
                }
                else
                {
                    return new NlpResponse
                    {
                        Status = $"Error: {response.StatusCode} - {responseContent}"
                    };
                }
            }
            catch (Exception ex)
            {
                return new NlpResponse
                {
                    Status = $"Failed to analyze code: {ex.Message}"
                };
            }
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
                    var formalResponse = JsonConvert.DeserializeObject<FormalResponse>(responseContent);
                    return formalResponse ?? new FormalResponse { FunctionName = functionName };
                }
                else
                {
                    return new FormalResponse
                    {
                        FunctionName = functionName,
                        Validation = new Validation
                        {
                            Valid = false,
                            Errors = new System.Collections.Generic.List<string>
                            {
                                $"Error: {response.StatusCode} - {responseContent}"
                            }
                        }
                    };
                }
            }
            catch (Exception ex)
            {
                return new FormalResponse
                {
                    FunctionName = functionName,
                    Validation = new Validation
                    {
                        Valid = false,
                        Errors = new System.Collections.Generic.List<string>
                        {
                            $"Failed to verify code: {ex.Message}"
                        }
                    }
                };
            }
        }
    }
}