using HeThongHienMauTichHopAI.Services;
using Microsoft.AspNetCore.Mvc;

namespace HeThongHienMauTichHopAI.Controllers
{
    public class AIController : Controller
    {
        private readonly AIService _aiService;

        public AIController(AIService aiService)
        {
            _aiService = aiService;
        }

        // GET: /AI
        public IActionResult Index()
        {
            return View();
        }

        // POST: /AI/Ask
        [HttpPost]
        public async Task<IActionResult> Ask(string question)
        {
            if (string.IsNullOrWhiteSpace(question))
            {
                ViewBag.Answer = "Vui lòng nhập câu hỏi.";
                return View("Index");
            }

            var answer = await _aiService.GetAnswerAsync(question);

            ViewBag.Question = question;
            ViewBag.Answer = answer;

            return View("Index");
        }
    }
}