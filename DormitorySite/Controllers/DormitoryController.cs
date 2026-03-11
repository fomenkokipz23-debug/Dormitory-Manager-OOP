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

        public DormitoryController(ApplicationDbContext context)
        {
            _context = context;
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

        [HttpGet]
        public IActionResult DownloadReceipt(string fullName, int roomNumber)
        {
            if (!IsAuthorized()) return RedirectToAction("Login", "Account");

            var student = _context.Students.FirstOrDefault(s => s.FullName == fullName && s.RoomNumber == roomNumber);
            var floor = DormitoryManager.Instance.Floors.FirstOrDefault(f => f.Rooms.Any(r => r.Number == roomNumber));

            if (student == null || floor == null) return NotFound();

            // Викликаємо новий метод генерації PDF
            byte[] pdfBytes = ReceiptGenerator.GeneratePdfReceipt(student, floor);
            
            string fileName = $"Receipt_{student.FullName?.Replace(" ", "_")}.pdf";

            // ПОВЕРТАЄМО PDF (application/pdf)
            return File(pdfBytes, "application/pdf", fileName);
        }

        [HttpGet]
        public IActionResult AddStudent(int? roomNumber)
        {
            if (!IsAuthorized()) return RedirectToAction("Login", "Account");
            
            ViewBag.SelectedRoom = roomNumber;
            ViewBag.Rooms = DormitoryManager.Instance.Floors.SelectMany(f => f.Rooms).OrderBy(r => r.Number).ToList();
            
            return View(new Student());
        }

        [HttpPost]
        public IActionResult AddStudent(Student student)
        {
            if (!IsAuthorized()) return RedirectToAction("Login", "Account");

            if (ModelState.IsValid)
            {
                var floor = DormitoryManager.Instance.Floors.FirstOrDefault(f => f.Rooms.Any(r => r.Number == student.RoomNumber));
                var room = floor?.Rooms.First(r => r.Number == student.RoomNumber);

                if (room != null)
                {
                    var currentStudents = _context.Students.Where(s => s.RoomNumber == room.Number).ToList();

                    if (currentStudents.Any() && currentStudents.First().Gender != student.Gender)
                    {
                        ModelState.AddModelError(string.Empty, $"Помилка: у кімнаті №{room.Number} живуть тільки {currentStudents.First().Gender}!");
                    }
                    else if (room.State.CanAssignStudent(currentStudents.Count))
                    {
                        _context.Students.Add(student);
                        _context.SaveChanges();

                        DormitoryManager.Instance.AddLog($"✅ БАЗА ДАНИХ: Заселено {student.FullName} (кім. №{student.RoomNumber}).");
                        return RedirectToAction("Students");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Кімната переповнена або закрита.");
                    }
                }
            }

            ViewBag.Rooms = DormitoryManager.Instance.Floors.SelectMany(f => f.Rooms).OrderBy(r => r.Number).ToList();
            return View(student);
        }

        public IActionResult Students(string search, string gender, int? course)
        {
            if (!IsAuthorized()) return RedirectToAction("Login", "Account");
            
            var query = _context.Students.AsQueryable();
            
            if (!string.IsNullOrEmpty(search)) 
                query = query.Where(s => s.FullName.Contains(search));
            
            if (!string.IsNullOrEmpty(gender)) 
                query = query.Where(s => s.Gender == gender);
            
            if (course.HasValue) 
                query = query.Where(s => s.Course == course.Value);
            
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
            });
            return Json(events);
        }

        public IActionResult Logs() => IsAuthorized() ? View(DormitoryManager.Instance.Logs) : RedirectToAction("Login", "Account");
    }
}