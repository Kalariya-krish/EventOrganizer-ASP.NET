using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace EventOrganizer_ASP.NET.Services
{

    public class UnsplashService
    {
        private readonly HttpClient _httpClient;
        private readonly string accessKey = "7PFAWTpIRoearColK-Nm7E6kjS1foBVMgXdgtIiezx0";

        public UnsplashService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> GetEventImage(string query)
        {
            var url = $"https://api.unsplash.com/search/photos?query={query}&client_id={accessKey}&per_page=1";

            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                return null;

            var content = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(content);
            var imageUrl = doc.RootElement
                              .GetProperty("results")[0]
                              .GetProperty("urls")
                              .GetProperty("regular")
                              .GetString();

            return imageUrl;
        }
    }
}
