using System.Text;
using System.Text.Json;

namespace GymApp.Services
{
    public class GeminiImageService : IGeminiImageService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly ILogger<GeminiImageService> _logger;

        private const string Endpoint =
            "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash-image:generateContent";

        public GeminiImageService(
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory,
            ILogger<GeminiImageService> logger)
        {
            _httpClient = httpClientFactory.CreateClient();
            _apiKey = configuration["Gemini:ApiKey"] ?? "";
            _logger = logger;
        }

        public async Task<(string? dataUrl, string? error)> GenerateFutureImageAsync(
            byte[] userPhoto,
            string mimeType,
            string prompt)
        {
            if (string.IsNullOrWhiteSpace(_apiKey))
                return (null, "Gemini API anahtarý tanýmlý deðil.");

            if (userPhoto is null || userPhoto.Length == 0)
                return (null, "Fotoðraf bulunamadý.");

            // mimeType normalize
            mimeType = (mimeType ?? "").ToLowerInvariant();
            if (mimeType is not ("image/jpeg" or "image/png" or "image/webp"))
                mimeType = "image/jpeg";

            try
            {
                // Input base64 (echo kontrolü için)
                var inputBase64 = Convert.ToBase64String(userPhoto);

                // Gemini request
                var requestBody = new
                {
                    contents = new[]
                    {
                        new
                        {
                            role = "user",
                            parts = new object[]
                            {
                                new { text = prompt },
                                new { inline_data = new { mime_type = mimeType, data = inputBase64 } }
                            }
                        }
                    },
                    generationConfig = new
                    {
                        // bazý örneklerde responseModalities kullanýlýyor; image için zorlamaya yardýmcý olabilir
                        responseModalities = new[] { "Image" }
                    }
                };

                var json = JsonSerializer.Serialize(requestBody);
                using var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

                using var req = new HttpRequestMessage(HttpMethod.Post, Endpoint);
                req.Headers.Add("x-goog-api-key", _apiKey);
                req.Content = httpContent;

                using var resp = await _httpClient.SendAsync(req);
                var respText = await resp.Content.ReadAsStringAsync();

                if (!resp.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Gemini image error: {Status} - {Body}", resp.StatusCode, respText);

                    if ((int)resp.StatusCode == 429)
                        return (null, "Ýstek limiti aþýldý. Biraz bekleyip tekrar dene.");

                    return (null, "Görsel üretilemedi. (Gemini hatasý)");
                }

                using var doc = JsonDocument.Parse(respText);

                if (!doc.RootElement.TryGetProperty("candidates", out var candidates) || candidates.GetArrayLength() == 0)
                    return (null, "Görsel üretilemedi (candidates boþ).");

                if (!candidates[0].TryGetProperty("content", out var contentEl) ||
                    !contentEl.TryGetProperty("parts", out var partsEl) ||
                    partsEl.ValueKind != JsonValueKind.Array)
                    return (null, "Görsel üretilemedi (content.parts yok).");

                // 1) En kritik fix:
                // Bazý cevaplarda input foto da parts içinde geri dönebiliyor (echo).
                // Bu yüzden "ilk görsel" deðil, "son görsel" alýnýr.
                string? lastImageBase64 = null;
                string outMime = "image/png";

                foreach (var part in partsEl.EnumerateArray())
                {
                    // snake_case: inline_data
                    if (part.TryGetProperty("inline_data", out var inlineDataSnake) &&
                        inlineDataSnake.ValueKind == JsonValueKind.Object &&
                        inlineDataSnake.TryGetProperty("data", out var dataPropSnake))
                    {
                        var data = dataPropSnake.GetString();
                        if (!string.IsNullOrEmpty(data))
                        {
                            lastImageBase64 = data;
                            if (inlineDataSnake.TryGetProperty("mime_type", out var mt))
                                outMime = mt.GetString() ?? outMime;
                        }
                    }

                    // camelCase: inlineData
                    if (part.TryGetProperty("inlineData", out var inlineDataCamel) &&
                        inlineDataCamel.ValueKind == JsonValueKind.Object &&
                        inlineDataCamel.TryGetProperty("data", out var dataPropCamel))
                    {
                        var data = dataPropCamel.GetString();
                        if (!string.IsNullOrEmpty(data))
                        {
                            lastImageBase64 = data;
                            if (inlineDataCamel.TryGetProperty("mimeType", out var mt))
                                outMime = mt.GetString() ?? outMime;
                        }
                    }
                }

                if (string.IsNullOrEmpty(lastImageBase64))
                    return (null, "Yanýtta görsel bulunamadý.");

                // 2) Echo kontrolü:
                // Eðer model input görseli aynen geri yolladýysa, kullanýcý "ayný foto" görür.
                if (lastImageBase64 == inputBase64)
                {
                    return (null,
                        "Model ayný fotoðrafý geri döndürdü (echo). " +
                        "Prompt’u daha belirgin edit isteyecek þekilde güçlendir veya dönüþümü daha ‘noticeable’ yap.");
                }

                return ($"data:{outMime};base64,{lastImageBase64}", null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Gemini image exception");
                return (null, "Görsel üretiminde beklenmeyen hata oluþtu.");
            }
        }
    }
}
