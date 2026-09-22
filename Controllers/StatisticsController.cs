using HeThongHienMauTichHopAI.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HeThongHienMauTichHopAI.Controllers;

[Authorize(Roles = "Coordinator,Admin")]
public class StatisticsController : Controller
{
    private readonly ApplicationDbContext _context;

    public StatisticsController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var registrations =
            await _context.DonationRegistrations
                .ToListAsync();

        ViewBag.Total =
            registrations.Count;

        ViewBag.Pending =
            registrations.Count(
                x => x.Status == "Chờ duyệt");

        ViewBag.Approved =
            registrations.Count(
                x => x.Status == "Đã duyệt");

        ViewBag.Rejected =
            registrations.Count(
                x => x.Status == "Từ chối");

        ViewBag.APlus =
            registrations.Count(
                x => x.BloodType == "A+");

        ViewBag.AMinus =
            registrations.Count(
                x => x.BloodType == "A-");

        ViewBag.BPlus =
            registrations.Count(
                x => x.BloodType == "B+");

        ViewBag.BMinus =
            registrations.Count(
                x => x.BloodType == "B-");

        ViewBag.ABPlus =
            registrations.Count(
                x => x.BloodType == "AB+");

        ViewBag.ABMinus =
            registrations.Count(
                x => x.BloodType == "AB-");

        ViewBag.OPlus =
            registrations.Count(
                x => x.BloodType == "O+");

        ViewBag.OMinus =
            registrations.Count(
                x => x.BloodType == "O-");

        return View();
    }
}