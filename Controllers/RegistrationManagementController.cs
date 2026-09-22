using HeThongHienMauTichHopAI.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HeThongHienMauTichHopAI.Controllers;

[Authorize(Roles = "Coordinator,Admin")]
public class RegistrationManagementController : Controller
{
    private readonly ApplicationDbContext _context;

    public RegistrationManagementController(
        ApplicationDbContext context)
    {
        _context = context;
    }

    // Danh sách đăng ký
    public async Task<IActionResult> Index()
    {
        var registrations =
            await _context.DonationRegistrations
                .OrderByDescending(x => x.RegisteredAt)
                .ToListAsync();

        return View(registrations);
    }

    // Duyệt đăng ký
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(int id)
    {
        var registration =
            await _context.DonationRegistrations
                .FirstOrDefaultAsync(x => x.Id == id);

        if (registration == null)
        {
            return NotFound();
        }

        registration.Status = "Đã duyệt";

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    // Từ chối đăng ký
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(int id)
    {
        var registration =
            await _context.DonationRegistrations
                .FirstOrDefaultAsync(x => x.Id == id);

        if (registration == null)
        {
            return NotFound();
        }

        registration.Status = "Từ chối";

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }
}