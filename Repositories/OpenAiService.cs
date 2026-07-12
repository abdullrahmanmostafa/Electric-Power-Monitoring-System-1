using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Electric_Power_Monitoring_System.Services
{
    public class OpenAiService : IAiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _model;
        private readonly ILogger<OpenAiService> _logger;

        public OpenAiService(IConfiguration config, ILogger<OpenAiService> logger)
        {
            _httpClient = new HttpClient();
            _apiKey = config["OpenAI:ApiKey"] ?? throw new Exception("OpenAI API key missing");
            _model = config["OpenAI:Model"] ?? "gpt-3.5-turbo";
            _logger = logger;
        }

        public async Task<List<string>> GenerateTipsAsync(decimal remainingKWh, decimal nextTierPrice)
        {
            try
            {
                var prompt = $"أنت خبير في ترشيد استهلاك الكهرباء في مصر. المستخدم لديه {remainingKWh} كيلووات/ساعة متبقية قبل الانتقال إلى شريحة أعلى بسعر {nextTierPrice} جنيه للكيلووات. قدم 3 نصائح عملية وسهلة بالعامية المصرية لمساعدته على توفير هذه الكمية لأطول فترة ممكنة. ركز على نصائح يمكن تطبيقها فوراً في المنزل. أجب فقط بقائمة من 3 نقاط مرقمة، بدون مقدمات.";

                var requestBody = new
                {
                    model = _model,
                    messages = new[]
                    {
                        new { role = "user", content = prompt }
                    },
                    max_tokens = 300,
                    temperature = 0.7
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");

                var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("OpenAI API error: {StatusCode}", response.StatusCode);
                    return null;
                }

                var responseString = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<OpenAiResponse>(responseString);
                var tipsText = result?.Choices?.FirstOrDefault()?.Message?.Content;
                if (string.IsNullOrEmpty(tipsText))
                    return null;

                // استخراج النقاط المرقمة
                var tips = tipsText.Split('\n')
                                   .Where(line => !string.IsNullOrWhiteSpace(line) && (char.IsDigit(line.Trim()[0]) || line.Trim().StartsWith("-")))
                                   .Select(line => line.Trim().TrimStart('0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '.', '-', ' '))
                                   .Where(t => !string.IsNullOrEmpty(t))
                                   .Take(3)
                                   .ToList();

                if (tips.Count < 3) // إذا لم تكن القائمة كاملة، نضيف نصائح احتياطية
                {
                    var fallback = await GetFallbackTips();
                    while (tips.Count < 3 && fallback.Any())
                    {
                        tips.Add(fallback.First());
                        fallback = fallback.Skip(1).ToList();
                    }
                }

                return tips;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling OpenAI");
                return null;
            }
        }

        private async Task<List<string>> GetFallbackTips()
        {
            // سيتم حقن AppDbContext لسحب النصائح الاحتياطية
            // لكننا سنستخدم خدمة منفصلة بدلاً من ذلك، أو نمرر DbContext.
            // سننفذ هذا في TierService.
            return new List<string>(); // placeholder
        }

        // فئات مساعدة لـ JSON
        private class OpenAiResponse
        {
            public Choice[] Choices { get; set; }
        }
        private class Choice
        {
            public Message Message { get; set; }
        }
        private class Message
        {
            public string Content { get; set; }
        }
    }
}