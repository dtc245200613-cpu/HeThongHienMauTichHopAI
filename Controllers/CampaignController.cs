using HeThongHienMauTichHopAI.Data;
using HeThongHienMauTichHopAI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HeThongHienMauTichHopAI.Controllers
{
    public class CampaignController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CampaignController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Danh sách chiến dịch
        public async Task<IActionResult> Index()
        {
            var campaigns = await _context.Campaigns
                .OrderByDescending(x => x.Date)
                .ToListAsync();

            return View(campaigns);
        }

        // Trang tạo chiến dịch
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // Lưu chiến dịch
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Campaign campaign)
        {
            if (ModelState.IsValid)
            {
                _context.Campaigns.Add(campaign);

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(campaign);
        }
    }
}