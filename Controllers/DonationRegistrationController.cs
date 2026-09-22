using HeThongHienMauTichHopAI.Data;
using HeThongHienMauTichHopAI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HeThongHienMauTichHopAI.Controllers
{
    public class DonationRegistrationController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DonationRegistrationController(
            ApplicationDbContext context)
        {
            _context = context;
        }

        // ==============================
        // DANH SÁCH ĐĂNG KÝ
        // ==============================

        public async Task<IActionResult> Index()
        {
            var registrations =
                await _context.DonationRegistrations
                    .OrderByDescending(x => x.RegisteredAt)
                    .ToListAsync();

            return View(registrations);
        }


        // ==============================
        // FORM ĐĂNG KÝ
        // ==============================

        [HttpGet]
        public async Task<IActionResult> Register(int? campaignId)
        {
            var campaigns =
                await _context.Campaigns
                    .OrderBy(x => x.Date)
                    .ToListAsync();

            ViewBag.Campaigns = campaigns;

            var registration =
                new DonationRegistration();

            if (campaignId.HasValue)
            {
                registration.CampaignId =
                    campaignId.Value;
            }

            return View(registration);
        }


        // ==============================
        // XỬ LÝ ĐĂNG KÝ
        // ==============================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(
            DonationRegistration registration)
        {
            var campaigns =
                await _context.Campaigns
                    .OrderBy(x => x.Date)
                    .ToListAsync();

            ViewBag.Campaigns = campaigns;


            if (!ModelState.IsValid)
            {
                return View(registration);
            }


            // Kiểm tra chiến dịch
            var campaign =
                await _context.Campaigns
                    .FirstOrDefaultAsync(
                        x => x.Id == registration.CampaignId);

            if (campaign == null)
            {
                ModelState.AddModelError(
                    "CampaignId",
                    "Chiến dịch không tồn tại.");

                return View(registration);
            }


            // Kiểm tra số lượng đăng ký
            var totalRegistrations =
                await _context.DonationRegistrations
                    .CountAsync(
                        x => x.CampaignId ==
                             registration.CampaignId);


            if (totalRegistrations >=
                campaign.MaxParticipants)
            {
                ModelState.AddModelError(
                    "CampaignId",
                    "Chiến dịch đã đủ số lượng đăng ký.");

                return View(registration);
            }


            // Giá trị mặc định
            registration.Status = "Chờ duyệt";

            registration.RegisteredAt =
                DateTime.Now;


            // Lưu
            _context.DonationRegistrations.Add(
                registration);

            await _context.SaveChangesAsync();


            return RedirectToAction(
                nameof(Success));
        }


        // ==============================
        // THÀNH CÔNG
        // ==============================

        public IActionResult Success()
        {
            return View();
        }
    }
}