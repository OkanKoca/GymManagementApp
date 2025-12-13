using GymApp.ViewModels;
using System.Text;
using System.Text.Json;

namespace GymApp.Services
{
    public class GeminiService : IAIService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly ILogger<GeminiService> _logger;

        public GeminiService(IConfiguration configuration, IHttpClientFactory httpClientFactory, ILogger<GeminiService> logger)
        {
            _httpClient = httpClientFactory.CreateClient();
            _apiKey = configuration["Gemini:ApiKey"] ?? "";
            _logger = logger;
        }

        public async Task<AIRecommendationViewModel> GetRecommendationsAsync(AIRecommendationViewModel model)
        {
            // BMI hesaplama
            if (model.Height.HasValue && model.Weight.HasValue && model.Height > 0)
            {
                var heightInMeters = model.Height.Value / 100;
                model.BMI = Math.Round(model.Weight.Value / (heightInMeters * heightInMeters), 2);
                model.BMICategory = GetBMICategory(model.BMI.Value);
            }

            // BMR ve TDEE hesaplama
            if (model.Height.HasValue && model.Weight.HasValue && model.Age.HasValue && !string.IsNullOrEmpty(model.Gender))
            {
                double bmr;
                if (model.Gender.ToLower() == "erkek" || model.Gender.ToLower() == "male")
                    bmr = 10 * model.Weight.Value + 6.25 * model.Height.Value - 5 * model.Age.Value + 5;
                else
                    bmr = 10 * model.Weight.Value + 6.25 * model.Height.Value - 5 * model.Age.Value - 161;

                model.BMR = Math.Round(bmr, 0);

                double activityMultiplier = model.ActivityLevel switch
                {
                    "Sedanter" => 1.2,
                    "Hafif Aktif" => 1.375,
                    "Aktif" => 1.55,
                    "Çok Aktif" => 1.725,
                    _ => 1.375
                };
                model.TDEE = Math.Round(bmr * activityMultiplier, 0);

                model.TargetCalories = model.Goal?.ToLower() switch
                {
                    var g when g?.Contains("kilo ver") == true => Math.Round(model.TDEE.Value - 500, 0),
                    var g when g?.Contains("kas") == true || g?.Contains("kilo al") == true => Math.Round(model.TDEE.Value + 300, 0),
                    _ => model.TDEE
                };
            }

            // Vücut tipi tahmini
            if (string.IsNullOrEmpty(model.BodyType) && model.BMI.HasValue)
            {
                model.BodyType = model.BMI.Value switch
                {
                    < 20 => "Ektomorf",
                    >= 20 and < 25 => "Mezomorf",
                    >= 25 => "Endomorf"
                };
            }

            if (string.IsNullOrEmpty(_apiKey))
            {
                model.ExerciseRecommendation = "?? Gemini API anahtarý tanýmlý deðil.";
                model.DietRecommendation = "appsettings.json dosyasýna Gemini:ApiKey ekleyin.";
                model.GeneralAdvice = "https://aistudio.google.com/app/apikey adresinden ücretsiz anahtar alabilirsiniz.";
                return model;
            }

            try
            {
                var prompt = BuildPrompt(model);
                var response = await CallGeminiAsync(prompt);

                if (!string.IsNullOrEmpty(response))
                    ParseAIResponse(response, model);
                else
                {
                    model.ExerciseRecommendation = "?? Yanýt alýnamadý.";
                    model.DietRecommendation = "Lütfen tekrar deneyin.";
                    model.GeneralAdvice = "";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Gemini API hatasý");
                model.ExerciseRecommendation = "?? AI servisi hatasý.";
                model.DietRecommendation = ex.Message;
                model.GeneralAdvice = "";
            }

            return model;
        }

        private string BuildPrompt(AIRecommendationViewModel model)
        {
            var sb = new StringBuilder();

            sb.AppendLine("Fitness koçu olarak kýsa ve öz Türkçe öneriler ver.");
            sb.AppendLine();
            sb.AppendLine($"Kiþi: {model.Age} yaþ, {model.Gender}, {model.Height}cm, {model.Weight}kg");
            sb.AppendLine($"BMI: {model.BMI} ({model.BMICategory}), Vücut: {model.BodyType}");
            sb.AppendLine($"Hedef: {model.Goal ?? "Genel fitness"}");
            sb.AppendLine($"Aktivite: {model.ActivityLevel ?? "Orta"}");

            if (!string.IsNullOrEmpty(model.HealthConditions))
                sb.AppendLine($"Saðlýk: {model.HealthConditions}");

            if (model.TDEE.HasValue)
                sb.AppendLine($"Günlük kalori: {model.TDEE} kcal, Hedef: {model.TargetCalories} kcal");

            sb.AppendLine();
            sb.AppendLine("TAM OLARAK bu formatta yanýt ver (her bölüm 3-5 madde):");
            sb.AppendLine();
            sb.AppendLine("[EGZERSIZ]");
            sb.AppendLine("• Madde 1");
            sb.AppendLine("• Madde 2");
            sb.AppendLine();
            sb.AppendLine("[BESLENME]");
            sb.AppendLine("• Madde 1");
            sb.AppendLine("• Madde 2");
            sb.AppendLine();
            sb.AppendLine("[TAVSIYE]");
            sb.AppendLine("• Madde 1");
            sb.AppendLine("• Madde 2");

            return sb.ToString();
        }

        private async Task<string?> CallGeminiAsync(string prompt)
        {
            var models = new[] { "gemini-2.5-flash-lite", "gemini-2.5-flash", "gemma-3-27b" };

            foreach (var modelName in models)
            {
                try
                {
                    var url = $"https://generativelanguage.googleapis.com/v1beta/models/{modelName}:generateContent?key={_apiKey}";

                    var requestBody = new
                    {
                        contents = new[]
                        {
                            new
                            {
                                parts = new[] { new { text = prompt } }
                            }
                        },
                        generationConfig = new
                        {
                            temperature = 0.5,
                            maxOutputTokens = 1024
                        }
                    };

                    var json = JsonSerializer.Serialize(requestBody);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    var response = await _httpClient.PostAsync(url, content);

                    if (response.IsSuccessStatusCode)
                    {
                        var responseContent = await response.Content.ReadAsStringAsync();
                        using var doc = JsonDocument.Parse(responseContent);

                        if (doc.RootElement.TryGetProperty("candidates", out var candidates) &&
                        candidates.GetArrayLength() > 0 &&
                              candidates[0].TryGetProperty("content", out var contentProp) &&
                           contentProp.TryGetProperty("parts", out var parts) &&
                              parts.GetArrayLength() > 0)
                        {
                            _logger.LogInformation("Gemini baþarýlý: {Model}", modelName);
                            return parts[0].GetProperty("text").GetString();
                        }
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        continue;
                    }
                    else
                    {
                        var error = await response.Content.ReadAsStringAsync();
                        _logger.LogWarning("Gemini hatasý ({Model}): {Error}", modelName, error);

                        if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                            throw new Exception("Ýstek limiti aþýldý. Biraz bekleyin.");
                    }
                }
                catch (Exception ex) when (ex.Message.Contains("limit"))
                {
                    throw;
                }
                catch
                {
                    continue;
                }
            }

            throw new Exception("AI modeli yanýt vermedi.");
        }

        private void ParseAIResponse(string response, AIRecommendationViewModel model)
        {
            // Markdown temizle
            response = response.Replace("**", "").Replace("##", "").Replace("###", "").Replace("*", "•");

            var exerciseBuilder = new StringBuilder();
            var dietBuilder = new StringBuilder();
            var generalBuilder = new StringBuilder();

            var currentSection = "";
            var lines = response.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                var upper = trimmed.ToUpper();

                // Bölüm tespiti
                if (upper.Contains("[EGZERSIZ]") || upper.Contains("EGZERSIZ") || upper.Contains("EGZERSÝZ"))
                {
                    currentSection = "exercise";
                    continue;
                }
                if (upper.Contains("[BESLENME]") || upper.Contains("BESLENME") || upper.Contains("DÝYET") || upper.Contains("DIYET"))
                {
                    currentSection = "diet";
                    continue;
                }
                if (upper.Contains("[TAVSIYE]") || upper.Contains("TAVSÝYE") || upper.Contains("GENEL"))
                {
                    currentSection = "general";
                    continue;
                }

                // Ýçerik ekleme
                if (!string.IsNullOrWhiteSpace(trimmed) && !trimmed.StartsWith("[") && !trimmed.StartsWith("==="))
                {
                    // Bullet ekle yoksa
                    if (!trimmed.StartsWith("•") && !trimmed.StartsWith("-") && !trimmed.StartsWith("*"))
                        trimmed = "• " + trimmed;

                    trimmed = trimmed.Replace("- ", "• ").Replace("* ", "• ");

                    switch (currentSection)
                    {
                        case "exercise":
                            exerciseBuilder.AppendLine(trimmed);
                            break;
                        case "diet":
                            dietBuilder.AppendLine(trimmed);
                            break;
                        case "general":
                            generalBuilder.AppendLine(trimmed);
                            break;
                    }
                }
            }

            model.ExerciseRecommendation = exerciseBuilder.ToString().Trim();
            model.DietRecommendation = dietBuilder.ToString().Trim();
            model.GeneralAdvice = generalBuilder.ToString().Trim();

            // Hiçbir bölüm parse edilmediyse tamamýný göster
            if (string.IsNullOrEmpty(model.ExerciseRecommendation) && string.IsNullOrEmpty(model.DietRecommendation) && string.IsNullOrEmpty(model.GeneralAdvice))
            {
                var parts = response.Split(new[] { "EGZERSIZ", "EGZERSÝZ", "BESLENME", "DÝYET", "TAVSIYE", "GENEL" },
                        StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length >= 3)
                {
                    model.ExerciseRecommendation = CleanText(parts[0]);
                    model.DietRecommendation = CleanText(parts[1]);
                    model.GeneralAdvice = CleanText(parts.Length > 2 ? parts[2] : "");
                }
                else
                {
                    model.GeneralAdvice = CleanText(response);
                }
            }
        }

        private string CleanText(string text)
        {
            return text.Replace("**", "")
            .Replace("##", "")
            .Replace("###", "")
            .Replace("[", "")
            .Replace("]", "")
            .Replace("ÖNERÝLERÝ:", "")
            .Replace("ONERILERI:", "")
            .Trim();
        }

        private string GetBMICategory(double bmi)
        {
            if (bmi < 18.5) return "Zayýf";
            if (bmi < 25) return "Normal";
            if (bmi < 30) return "Fazla Kilolu";
            return "Obez";
        }
    }
}
