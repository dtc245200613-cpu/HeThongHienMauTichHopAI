using System.Net.Http.Json;
using System.Text.Json;

namespace HeThongHienMauTichHopAI.Services
{
    public class AIService
    {
        private readonly HttpClient _httpClient;

        public AIService()
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri("http://127.0.0.1:8000/")
            };

            _httpClient.Timeout = TimeSpan.FromMinutes(5);
        }

        // =====================================================
        // GỌI RAG PYTHON API
        // =====================================================

        public async Task<string> GetAnswerAsync(string question)
        {
            if (string.IsNullOrWhiteSpace(question))
            {
                return "Vui lòng nhập câu hỏi.";
            }

            try
            {
                var request = new QuestionRequest
                {
                    question = question
                };

                var response = await _httpClient.PostAsJsonAsync(
                    "ask",
                    request
                );

                var json = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return $"Lỗi kết nối RAG: HTTP {(int)response.StatusCode}\n{json}";
                }

                var result = JsonSerializer.Deserialize<RagResponse>(
                    json,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }
                );

                if (result == null)
                {
                    return "RAG không trả về dữ liệu.";
                }

                if (!result.success)
                {
                    return result.answer ?? "RAG xảy ra lỗi.";
                }

                return result.answer ?? "RAG không trả về câu trả lời.";
            }
            catch (HttpRequestException)
            {
                return
                    "Không thể kết nối RAG Python.\n\n" +
                    "Hãy kiểm tra API Python đang chạy tại:\n" +
                    "http://127.0.0.1:8000";
            }
            catch (TaskCanceledException)
            {
                return
                    "RAG xử lý quá lâu.\n\n" +
                    "Gemini hoặc Embedding Model có thể đang xử lý.";
            }
            catch (Exception ex)
            {
                return
                    "Lỗi AI:\n\n" +
                    ex.Message;
            }
        }

        // =====================================================
        // REQUEST
        // =====================================================

        private class QuestionRequest
        {
            public string question { get; set; } = "";
        }

        // =====================================================
        // RESPONSE
        // =====================================================

        private class RagResponse
        {
            public bool success { get; set; }

            public string? answer { get; set; }
        }
    }
}