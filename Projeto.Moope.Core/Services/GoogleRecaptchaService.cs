using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Projeto.Moope.Core.DTOs;
using Projeto.Moope.Core.Interfaces.Services;

namespace Projeto.Moope.Core.Services
{
    public class GoogleRecaptchaService: IGoogleRecaptchaService
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public GoogleRecaptchaService(IConfiguration configuration, HttpClient httpClient)
        {
            _configuration = configuration;
            _httpClient = httpClient;
        }

        public async Task<bool> VerifyTokenAsync(string token)
        {
            var secretKey = _configuration["Recaptcha:SecretKey"];
            if (secretKey != null && token != null)
            {
                var content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    {"secret", secretKey },
                    {"response", token }
                });
                var response = await _httpClient.PostAsync("https://www.google.com/recaptcha/api/siteverify", content);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<GoogleRecaptchaResponse>(json);
                    return result?.Success ?? false;
                }
            }
            return false;
        }
    }
}
