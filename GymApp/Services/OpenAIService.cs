using GymApp.ViewModels;
using System.Text;
using System.Text.Json;

namespace GymApp.Services
{
    public class OpenAIService : IAIService
    {
      private readonly HttpClient _httpClient;
   private readonly string _apiKey;
        private readonly ILogger<OpenAIService> _logger;
 private readonly IWebHostEnvironment _environment;

  public OpenAIService(IConfiguration configuration, IHttpClientFactory httpClientFactory, ILogger<OpenAIService> logger, IWebHostEnvironment environment)
  {
     _httpClient = httpClientFactory.CreateClient();
    _apiKey = configuration["OpenAI:ApiKey"] ?? "";
   _logger = logger;
   _environment = environment;
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

       // BMR ve TDEE hesaplama (Mifflin-St Jeor Formülü)
if (model.Height.HasValue && model.Weight.HasValue && model.Age.HasValue && !string.IsNullOrEmpty(model.Gender))
         {
 double bmr;
     if (model.Gender.ToLower() == "erkek" || model.Gender.ToLower() == "male")
       {
    bmr = 10 * model.Weight.Value + 6.25 * model.Height.Value - 5 * model.Age.Value + 5;
      }
      else
     {
   bmr = 10 * model.Weight.Value + 6.25 * model.Height.Value - 5 * model.Age.Value - 161;
 }
      model.BMR = Math.Round(bmr, 0);

     // TDEE hesaplama (aktivite seviyesine göre)
    double activityMultiplier = model.ActivityLevel switch
            {
   "Sedanter" => 1.2,
       "Hafif Aktif" => 1.375,
     "Aktif" => 1.55,
        "Çok Aktif" => 1.725,
      _ => 1.375
     };
     model.TDEE = Math.Round(bmr * activityMultiplier, 0);

     // Hedefe göre kalori
      model.TargetCalories = model.Goal?.ToLower() switch
  {
        var g when g?.Contains("kilo ver") == true || g?.Contains("zayýfla") == true => Math.Round(model.TDEE.Value - 500, 0),
    var g when g?.Contains("kas") == true || g?.Contains("kilo al") == true => Math.Round(model.TDEE.Value + 300, 0),
_ => model.TDEE
        };
          }

          // Vücut tipi tahmini (boy/kilo oranýna ve BMI'ye göre basit tahmin)
        if (string.IsNullOrEmpty(model.BodyType) && model.BMI.HasValue)
     {
     model.BodyType = model.BMI.Value switch
      {
      < 20 => "Ektomorf",
   >= 20 and < 25 => "Mezomorf",
        >= 25 => "Endomorf"
  };
    }

      // API key yoksa veya boþsa, kural tabanlý öneri döndür
         if (string.IsNullOrEmpty(_apiKey))
  {
      return GetRuleBasedRecommendations(model);
            }

   try
            {
 var prompt = BuildPrompt(model);
     var response = await CallOpenAIAsync(prompt);

              if (!string.IsNullOrEmpty(response))
     {
      ParseAIResponse(response, model);
      }
             else
   {
         return GetRuleBasedRecommendations(model);
     }
   }
    catch (Exception ex)
     {
      _logger.LogError(ex, "OpenAI API çaðrýsýnda hata oluþtu");
    return GetRuleBasedRecommendations(model);
      }

   return model;
        }

        public async Task<string?> SavePhotoAsync(IFormFile photo)
 {
    if (photo == null || photo.Length == 0)
        return null;

          var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "photos");
  Directory.CreateDirectory(uploadsFolder);

   var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(photo.FileName);
    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

 using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
    await photo.CopyToAsync(fileStream);
     }

     return "/uploads/photos/" + uniqueFileName;
        }

     private string BuildPrompt(AIRecommendationViewModel model)
        {
            var sb = new StringBuilder();
    sb.AppendLine("Sen bir fitness ve beslenme uzmanýsýn. Aþaðýdaki bilgilere göre Türkçe öneriler ver:");
          sb.AppendLine();

            if (model.Height.HasValue)
     sb.AppendLine($"Boy: {model.Height} cm");

  if (model.Weight.HasValue)
      sb.AppendLine($"Kilo: {model.Weight} kg");

   if (model.Age.HasValue)
    sb.AppendLine($"Yaþ: {model.Age}");

         if (!string.IsNullOrEmpty(model.Gender))
      sb.AppendLine($"Cinsiyet: {model.Gender}");

  if (model.BMI.HasValue)
          sb.AppendLine($"BMI: {model.BMI} ({model.BMICategory})");

 if (!string.IsNullOrEmpty(model.BodyType))
    sb.AppendLine($"Vücut Tipi: {model.BodyType}");

       if (!string.IsNullOrEmpty(model.ActivityLevel))
   sb.AppendLine($"Aktivite Seviyesi: {model.ActivityLevel}");

         if (!string.IsNullOrEmpty(model.Goal))
  sb.AppendLine($"Hedef: {model.Goal}");

            if (!string.IsNullOrEmpty(model.HealthConditions))
     sb.AppendLine($"Saðlýk durumu/kýsýtlamalar: {model.HealthConditions}");

        if (model.TDEE.HasValue)
    sb.AppendLine($"Günlük Kalori Ýhtiyacý: {model.TDEE} kcal");

  sb.AppendLine();
            sb.AppendLine("Lütfen þu formatta yanýt ver:");
            sb.AppendLine("EGZERSÝZ ÖNERÝLERÝ:");
   sb.AppendLine("[egzersiz önerileri]");
            sb.AppendLine("DÝYET ÖNERÝLERÝ:");
            sb.AppendLine("[diyet önerileri]");
       sb.AppendLine("GENEL TAVSÝYELER:");
   sb.AppendLine("[genel tavsiyeler]");

   return sb.ToString();
        }

        private async Task<string?> CallOpenAIAsync(string prompt)
  {
   var requestBody = new
     {
        model = "gpt-3.5-turbo",
             messages = new[]
              {
           new { role = "system", content = "Sen yardýmcý bir fitness ve beslenme asistanýsýn." },
       new { role = "user", content = prompt }
            },
 max_tokens = 1000,
      temperature = 0.7
      };

 var json = JsonSerializer.Serialize(requestBody);
    var content = new StringContent(json, Encoding.UTF8, "application/json");

  _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");

          var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);

  if (response.IsSuccessStatusCode)
      {
    var responseContent = await response.Content.ReadAsStringAsync();
          using var doc = JsonDocument.Parse(responseContent);
   return doc.RootElement
      .GetProperty("choices")[0]
          .GetProperty("message")
      .GetProperty("content")
 .GetString();
 }

  return null;
        }

     private void ParseAIResponse(string response, AIRecommendationViewModel model)
    {
            var lines = response.Split('\n');
     var currentSection = "";
   var exerciseBuilder = new StringBuilder();
  var dietBuilder = new StringBuilder();
       var generalBuilder = new StringBuilder();

            foreach (var line in lines)
         {
    var trimmedLine = line.Trim();

    if (trimmedLine.Contains("EGZERSÝZ ÖNERÝLERÝ"))
     {
         currentSection = "exercise";
          continue;
     }
       else if (trimmedLine.Contains("DÝYET ÖNERÝLERÝ"))
    {
       currentSection = "diet";
                    continue;
   }
   else if (trimmedLine.Contains("GENEL TAVSÝYELER"))
            {
            currentSection = "general";
           continue;
                }

        switch (currentSection)
     {
         case "exercise":
           exerciseBuilder.AppendLine(trimmedLine);
 break;
   case "diet":
          dietBuilder.AppendLine(trimmedLine);
    break;
   case "general":
           generalBuilder.AppendLine(trimmedLine);
  break;
     }
    }

            model.ExerciseRecommendation = exerciseBuilder.ToString().Trim();
            model.DietRecommendation = dietBuilder.ToString().Trim();
            model.GeneralAdvice = generalBuilder.ToString().Trim();
        }

        private AIRecommendationViewModel GetRuleBasedRecommendations(AIRecommendationViewModel model)
        {
   var exerciseBuilder = new StringBuilder();
    var dietBuilder = new StringBuilder();
        var generalBuilder = new StringBuilder();

 // BMI bazlý öneriler
 if (model.BMI.HasValue)
            {
     var bmi = model.BMI.Value;

    if (bmi < 18.5)
             {
        exerciseBuilder.AppendLine("• Kas geliþtirme egzersizlerine (aðýrlýk antrenmaný) odaklanýn");
           exerciseBuilder.AppendLine("• Haftada 3-4 gün direnç antrenmaný yapýn");
     exerciseBuilder.AppendLine("• Kardiyo egzersizlerini sýnýrlý tutun (haftada max 2 gün)");
      exerciseBuilder.AppendLine("• Compound hareketlere (squat, deadlift, bench press) öncelik verin");

         dietBuilder.AppendLine("• Kalori alýmýnýzý artýrýn (günlük +300-500 kcal fazla)");
       dietBuilder.AppendLine("• Protein açýsýndan zengin besinler tüketin (tavuk, balýk, yumurta)");
  dietBuilder.AppendLine("• Saðlýklý karbonhidratlar ekleyin (yulaf, esmer pirinç)");
            dietBuilder.AppendLine("• Her öðünde protein tüketmeye özen gösterin");
         }
           else if (bmi >= 18.5 && bmi < 25)
   {
           exerciseBuilder.AppendLine("• Dengeli bir egzersiz programý uygulayýn");
 exerciseBuilder.AppendLine("• Haftada 3-4 gün kardiyo ve aðýrlýk antrenmaný karýþýmý yapýn");
       exerciseBuilder.AppendLine("• Esneklik çalýþmalarýný (yoga, pilates) ekleyin");
           exerciseBuilder.AppendLine("• HIIT antrenmanlarý metabolizmanýzý hýzlandýrýr");

        dietBuilder.AppendLine("• Dengeli beslenmeye devam edin");
       dietBuilder.AppendLine("• Günlük protein ihtiyacýnýzý karþýlayýn (vücut aðýrlýðýnýn kg baþýna 1.6-2g)");
         dietBuilder.AppendLine("• Bol sebze ve meyve tüketin (günde 5 porsiyon)");
         dietBuilder.AppendLine("• Ýþlenmiþ gýdalardan uzak durun");
                }
      else if (bmi >= 25 && bmi < 30)
   {
           exerciseBuilder.AppendLine("• Kardiyo egzersizlerine aðýrlýk verin (yürüyüþ, koþu, bisiklet)");
      exerciseBuilder.AppendLine("• Haftada en az 5 gün 30-45 dakika egzersiz yapýn");
   exerciseBuilder.AppendLine("• Yüzme veya su egzersizleri eklem dostu seçeneklerdir");
      exerciseBuilder.AppendLine("• Direnç antrenmanlarý da kas kaybýný önler");

              dietBuilder.AppendLine("• Kalori alýmýnýzý kontrol edin (günlük -500 kcal açýk)");
          dietBuilder.AppendLine("• Ýþlenmiþ gýdalardan ve þekerden kaçýnýn");
   dietBuilder.AppendLine("• Porsiyonlarý küçültün ve yavaþ yiyin");
    dietBuilder.AppendLine("• Lifli besinlere (sebze, tam tahýllar) aðýrlýk verin");
         }
          else
      {
             exerciseBuilder.AppendLine("• Düþük etkili egzersizlerle baþlayýn (yürüyüþ, yüzme)");
   exerciseBuilder.AppendLine("• Bir uzmandan destek alarak baþlayýn");
          exerciseBuilder.AppendLine("• Günlük adým sayýnýzý kademeli olarak artýrýn (hedef: 10.000 adým)");
         exerciseBuilder.AppendLine("• Sandalye egzersizleri ile baþlayabilirsiniz");

        dietBuilder.AppendLine("• Bir diyetisyenle görüþmenizi öneririz");
     dietBuilder.AppendLine("• Þekerli içecekleri tamamen býrakýn");
           dietBuilder.AppendLine("• Öðün atlamayýn, düzenli beslenin");
     dietBuilder.AppendLine("• Porsiyonlarý kontrol altýnda tutun");
       }
            }

            // Vücut tipine göre öneriler
     if (!string.IsNullOrEmpty(model.BodyType))
       {
   generalBuilder.AppendLine($"\n?? Vücut Tipiniz: {model.BodyType}");
              switch (model.BodyType.ToLower())
      {
        case "ektomorf":
               generalBuilder.AppendLine("• Hýzlý metabolizmanýz var, daha fazla kalori almalýsýnýz");
    generalBuilder.AppendLine("• Aðýrlýk antrenmanlarýna odaklanýn, kardiyo sýnýrlý tutun");
          generalBuilder.AppendLine("• Yüksek karbonhidrat, yüksek protein diyeti uygundur");
                break;
      case "mezomorf":
        generalBuilder.AppendLine("• Kas yapýmýna yatkýnsýnýz, dengeli antrenman uygulayýn");
  generalBuilder.AppendLine("• Hem kardiyo hem aðýrlýk antrenmaný yapabilirsiniz");
          generalBuilder.AppendLine("• Dengeli makro besin daðýlýmý idealdir");
       break;
   case "endomorf":
            generalBuilder.AppendLine("• Yað depolamaya yatkýnsýnýz, kardiyo önemli");
        generalBuilder.AppendLine("• HIIT antrenmanlarý metabolizmayý hýzlandýrýr");
            generalBuilder.AppendLine("• Düþük karbonhidrat diyeti size uygun olabilir");
    break;
 }
            }

        // Hedefe göre öneriler
  if (!string.IsNullOrEmpty(model.Goal))
     {
         var goal = model.Goal.ToLower();
            generalBuilder.AppendLine($"\n?? Hedefiniz: {model.Goal}");

        if (goal.Contains("kilo ver") || goal.Contains("zayýfla"))
   {
              generalBuilder.AppendLine("• Haftalýk 0.5-1 kg kaybetmeyi hedefleyin");
   generalBuilder.AppendLine("• Kalori açýðý oluþturun ama aþýrýya kaçmayýn");
           generalBuilder.AppendLine("• Sabýrlý olun, saðlýklý kilo verme zaman alýr");
   }
 else if (goal.Contains("kas") || goal.Contains("güçlen"))
      {
          generalBuilder.AppendLine("• Protein alýmýnýzý artýrýn (vücut aðýrlýðýnýn kg baþýna 1.6-2g)");
         generalBuilder.AppendLine("• Yeterli uyku alýn (7-9 saat)");
   generalBuilder.AppendLine("• Progressive overload uygulayýn");
         }
      else if (goal.Contains("fit") || goal.Contains("saðlýk"))
         {
             generalBuilder.AppendLine("• Düzenli egzersiz rutini oluþturun");
          generalBuilder.AppendLine("• Stres yönetimi tekniklerini öðrenin");
         generalBuilder.AppendLine("• Yeterli uyku ve su tüketimine dikkat edin");
           }
        }

         // Kalori bilgisi
            if (model.TDEE.HasValue)
            {
   generalBuilder.AppendLine($"\n?? Günlük Kalori Ýhtiyacýnýz: {model.TDEE} kcal");
           if (model.TargetCalories.HasValue && model.TargetCalories != model.TDEE)
    {
       generalBuilder.AppendLine($"?? Hedefinize Göre Alýnmasý Gereken: {model.TargetCalories} kcal");
          }
      }

  // Genel tavsiyeler
        generalBuilder.AppendLine("\n?? Genel Tavsiyeler:");
generalBuilder.AppendLine("• Günde en az 2-3 litre su için");
            generalBuilder.AppendLine("• Düzenli uyku düzenine sahip olun (7-9 saat)");
      generalBuilder.AppendLine("• Egzersiz öncesi ýsýnma, sonrasý soðuma yapýn");
  generalBuilder.AppendLine("• Hedeflerinizi küçük parçalara bölün");

            model.ExerciseRecommendation = exerciseBuilder.ToString().Trim();
            model.DietRecommendation = dietBuilder.ToString().Trim();
            model.GeneralAdvice = generalBuilder.ToString().Trim();

  return model;
        }

        private string GetBMICategory(double bmi)
        {
     return bmi switch
   {
         < 18.5 => "Zayýf",
              >= 18.5 and < 25 => "Normal",
                >= 25 and < 30 => "Fazla Kilolu",
     >= 30 => "Obez"
            };
        }
    }
}
