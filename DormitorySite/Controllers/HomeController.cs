using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using DormitorySite.Models;

namespace DormitorySite.Controllers;

public class HomeController : Controller
{
    // Головна сторінка (якщо вона у тебе є)
    public IActionResult Index()
    {
        return View();
    }

    // Сторінка політики конфіденційності (стандартна заглушка)
    public IActionResult Privacy()
    {
        return View();
    }

    // Метод для обробки та красивого відображення системних помилок
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        // Передаємо у View модель помилки з унікальним ID запиту для пошуку в логах
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}