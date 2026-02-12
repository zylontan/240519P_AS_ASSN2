using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Text.Json;

namespace _240519P_AS_ASSN2.Security
{
    public class ReCaptchaService
    {
        private readonly IConfiguration _config;
        private readonly HttpClient _httpClient;

        public ReCaptchaService(IConfiguration config)
        {
            _config = config;
            _httpClient = new HttpClient();
        }

        public async Task<bool> VerifyTokenAsync(string token)
        {
            var secret = _config["GoogleReCaptcha:SecretKey"];

            var response = await _httpClient.PostAsync(
                $"https://www.google.com/recaptcha/api/siteverify?secret={secret}&response={token}",
                null);

            var json = await response.Content.ReadAsStringAsync();

            var result = JsonSerializer.Deserialize<ReCaptchaResponse>(json);

            if (result == null)
                return false;

            // Accept only if score >= 0.5
            return result.success && result.score >= 0.5;
        }

        private class ReCaptchaResponse
        {
            public bool success { get; set; }
            public double score { get; set; }
            public string action { get; set; } = "";
        }
    }
}
