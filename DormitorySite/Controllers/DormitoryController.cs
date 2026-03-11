using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using DormitorySite.Models;
using DormitorySite.Services;
using System.Linq;

namespace DormitorySite.Controllers
{
    public class DormitoryController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly DormitoryFacade _facade; // Додано: посилання на фасад

        // Оновлений конструктор
        public DormitoryController(ApplicationDbContext context, DormitoryFacade facade)
        {
            _context = context;
            _facade = facade;
        }

        private bool IsAuthorized() => HttpContext.Session.GetString("IsLoggedIn") == "true";

        public IActionResult Index(int floor = 1)
        {
            if (!IsAuthorized()) return RedirectToAction("Login", "Account");
            
            var floorData = DormitoryManager.Instance.Floors.FirstOrDefault(f => f.Number == floor) 
                            ?? DormitoryManager.Instance.Floors.First();
            
            foreach (var room in floorData.Rooms)
            {
                room.Students = _context.Students.Where(s => s.RoomNumber == room.Number).ToList();
            }

            ViewBag.CurrentFloor = floor;
            return View(floorData);
        }

        public IActionResult ToggleRepair(int roomNumber, int floor)
        {
            if (!IsAuthorized()) return RedirectToAction("Login", "Account");

            var room = DormitoryManager.Instance.Floors
                .SelectMany(f => f.Rooms)
                .FirstOrDefault(r => r.Number == roomNumber);
            
            if (room != null)
            {
                room.IsUnderRepair = !room.IsUnderRepair;
                string msg = room.IsUnderRepair ? "закрита на ремонт" : "відкрита";
                DormitoryManager.Instance.AddLog($"Кімната №{roomNumber} {msg}.");
            }
            
            return RedirectToAction("Index", new { floor = floor });
        }
        
        [HttpGet]
        public IActionResult AddStudent(int? roomNumber)
        {
            if (!IsAuthorized()) return RedirectToAction("Login", "Account");
            ViewBag.SelectedRoom = roomNumber;
            ViewBag.Rooms = DormitoryManager.Instance.Floors.SelectMany(f => f.Rooms).OrderBy(r => r.Number).ToList();
            return View(new Student());
        }

        // Цей метод приймає дані з форми (POST)
        [HttpPost]
        public IActionResult AddStudent(Student student)
        {
            if (!IsAuthorized()) return RedirectToAction("Login", "Account");
            if (ModelState.IsValid)
            {
                var result = _facade.SettleStudent(student);
                if (result == "Success") return RedirectToAction("Students");
                ModelState.AddModelError(string.Empty, result);
            }
            ViewBag.Rooms = DormitoryManager.Instance.Floors.SelectMany(f => f.Rooms).OrderBy(r => r.Number).ToList();
            return View(student);
        }

        // ... інші методи (DownloadReceipt, Students, DeleteStudent, Calendar, Logs) залишаються без змін ...
        
        [HttpGet]
        public IActionResult DownloadReceipt(string fullName, int roomNumber)
        {
            if (!IsAuthorized()) return RedirectToAction("Login", "Account");
            var student = _context.Students.FirstOrDefault(s => s.FullName == fullName && s.RoomNumber == roomNumber);
            var floor = DormitoryManager.Instance.Floors.FirstOrDefault(f => f.Rooms.Any(r => r.Number == roomNumber));
            if (student == null || floor == null) return NotFound();
            byte[] pdfBytes = ReceiptGenerator.GeneratePdfReceipt(student, floor);
            return File(pdfBytes, "application/pdf", $"Receipt_{student.FullName?.Replace(" ", "_")}.pdf");
        }

        public IActionResult Students(string search, string gender, int? course)
        {
            if (!IsAuthorized()) return RedirectToAction("Login", "Account");
            var query = _context.Students.AsQueryable();
            if (!string.IsNullOrEmpty(search)) query = query.Where(s => s.FullName.Contains(search));
            if (!string.IsNullOrEmpty(gender)) query = query.Where(s => s.Gender == gender);
            if (course.HasValue) query = query.Where(s => s.Course == course.Value);
            return View(query.ToList());
        }

        [HttpPost]
        public IActionResult DeleteStudent(int id)
        {
            var student = _context.Students.Find(id);
            if (student != null)
            {
                _context.Students.Remove(student);
                _context.SaveChanges();
                DormitoryManager.Instance.AddLog($"❌ Виселено: {student.FullName}.");
            }
            return RedirectToAction("Students");
        }

        public IActionResult Calendar() => IsAuthorized() ? View() : RedirectToAction("Login", "Account");

        [HttpGet]
        public JsonResult GetCalendarEvents()
        {
            var events = _context.Students.ToList().SelectMany(s => new[] {
                new { title = $"⬇ Вхід: {s.FullName}", start = s.SettlementDate.ToString("yyyy-MM-dd"), color = "#28a745" },
                new { title = $"⬆ Вихід: {s.FullName}", start = s.DepartureDate.ToString("yyyy-MM-dd"), color = "#dc3545" }
            }).ToList();
            return Json(events);
        }

        public IActionResult Logs() => IsAuthorized() ? View(DormitoryManager.Instance.Logs) : RedirectToAction("Login", "Account");
    }
}