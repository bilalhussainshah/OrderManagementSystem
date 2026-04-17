using Newtonsoft.Json;
using Serilog;
using System;
using System.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace OrderManagementSystem.Services
{
    public class ApiClientService : IDisposable
    {
        private readonly HttpClient _client;
        private bool _disposed = false;

        public ApiClientService()
        {
            // Read from Web.config <appSettings> key="ApiBaseUrl
            var baseUrl = ConfigurationManager.AppSettings["ApiBaseUrl"]
                          ?? "https://localhost:44345/";

            _client = new HttpClient
            {
                BaseAddress = new Uri(baseUrl),
                Timeout = TimeSpan.FromSeconds(30)
            };

            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
        }

        // GET
        public async Task<T> GetAsync<T>(string url)
        {
            try
            {
                Log.Debug("API GET: {Url}", url);
                var response = await _client.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    Log.Warning("GET {Url} returned {Status}", url, response.StatusCode);
                    return default(T);
                }

                return await response.Content.ReadAsAsync<T>();
            }
            catch (TaskCanceledException ex)
            {
                Log.Error(ex, "GET timeout: {Url}", url);
                return default(T);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "GET error: {Url}", url);
                return default(T);
            }
        }

        // POST 
        public async Task<(bool Success, string Error)> PostAsyncWithError<T>(string url, T data)
        {
            try
            {
                Log.Debug("API POST: {Url}", url);
                var response = await _client.PostAsJsonAsync(url, data);
                var body = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    Log.Warning("POST {Url} failed ({Status}): {Body}", url, response.StatusCode, body);
                    return (false, ExtractMessage(body));
                }

                return (true, null);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "POST error: {Url}", url);
                return (false, "Unexpected error");
            }
        }

        // POST 
        public async Task<bool> PostAsync<T>(string url, T data)
        {
            var (success, _) = await PostAsyncWithError(url, data);
            return success;
        }

        // PUT 
        public async Task<(bool Success, string Error)> PutAsyncWithError<T>(string url, T data)
        {
            try
            {
                Log.Debug("API PUT: {Url}", url);
                var response = await _client.PutAsJsonAsync(url, data);
                var body = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    Log.Warning("PUT {Url} failed ({Status}): {Body}", url, response.StatusCode, body);
                    return (false, ExtractMessage(body));
                }

                return (true, null);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "PUT error: {Url}", url);
                return (false, "Unexpected error");
            }
        }

        public async Task<bool> PutAsync<T>(string url, T data)
        {
            var (success, _) = await PutAsyncWithError(url, data);
            return success;
        }

        // DELETE 
        public async Task<(bool Success, string Error)> DeleteAsyncWithError(string url)
        {
            try
            {
                Log.Debug("API DELETE: {Url}", url);
                var response = await _client.DeleteAsync(url);
                var body = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    Log.Warning("DELETE {Url} failed ({Status}): {Body}", url, response.StatusCode, body);
                    return (false, ExtractMessage(body));
                }

                return (true, null);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "DELETE error: {Url}", url);
                return (false, "Unexpected error");
            }
        }

        public async Task<bool> DeleteAsync(string url)
        {
            var (success, _) = await DeleteAsyncWithError(url);
            return success;
        }

        private string ExtractMessage(string body)
        {
            if (string.IsNullOrWhiteSpace(body)) return "Request failed";
            try
            {
                dynamic obj = JsonConvert.DeserializeObject(body);
                return obj?.message ?? obj?.Message ?? body;
            }
            catch
            {
                return body.Length > 200 ? body.Substring(0, 200) : body;
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _client?.Dispose();
                _disposed = true;
            }
        }
    }
}