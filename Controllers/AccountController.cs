using System.Security.Claims;
using HeThongHienMauTichHopAI.Data;
using HeThongHienMauTichHopAI.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HeThongHienMauTichHopAI.Controllers;

public class AccountController : Controller
{
    private readonly ApplicationDbContext _context;

    public AccountController(ApplicationDbContext context)
    {
        _context = context;
    }

    // =========================
    // ĐĂNG NHẬP
    // =========================

    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(string email, string password)
    {
        if (string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(password))
        {
            ViewBag.Error = "Vui lòng nhập đầy đủ thông tin.";
            return View();
        }

        var user = await _context.Users
            .FirstOrDefaultAsync(x =>
                x.Email == email &&
                x.Password == password);

        if (user == null)
        {
            ViewBag.Error = "Email hoặc mật khẩu không đúng.";
            return View();
        }

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var identity = new ClaimsIdentity(
            claims,
            CookieAuthenticationDefaults.AuthenticationScheme);

        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal);

        return RedirectToAction("Index", "Home");
    }


    // =========================
    // ĐĂNG KÝ
    // =========================

    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Register(
        string fullName,
        string email,
        string password,
        string confirmPassword)
    {
        if (string.IsNullOrWhiteSpace(fullName) ||
            string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(password))
        {
            ViewBag.Error = "Vui lòng nhập đầy đủ thông tin.";
            return View();
        }

        if (password != confirmPassword)
        {
            ViewBag.Error = "Mật khẩu xác nhận không trùng khớp.";
            return View();
        }

        var exists = await _context.Users
            .AnyAsync(x => x.Email == email);

        if (exists)
        {
            ViewBag.Error = "Email này đã được đăng ký.";
            return View();
        }

        var user = new User
        {
            FullName = fullName,
            Email = email,
            Password = password,

            // Người đăng ký mặc định là tình nguyện viên
            Role = "Volunteer"
        };

        _context.Users.Add(user);

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Login));
    }


    // =========================
    // ĐĂNG XUẤT
    // =========================

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(
            CookieAuthenticationDefaults.AuthenticationScheme);

        return RedirectToAction("Index", "Home");
    }


    // =========================
    // KHÔNG CÓ QUYỀN
    // =========================

    public IActionResult AccessDenied()
    {
        return View();
    }
}