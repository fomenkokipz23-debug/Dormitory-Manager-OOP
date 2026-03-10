using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

public class AccountController : Controller
{
    [HttpGet]
    public IActionResult Login() => View();

    [HttpPost]
    public IActionResult Login(string password)
    {
        if (password == "12") // Твій пароль
        {
            HttpContext.Session.SetString("IsLoggedIn", "true");
            return RedirectToAction("Index", "Dormitory");
        }
        ViewBag.Error = "Невірний пароль!";
        return View();
    }
} 