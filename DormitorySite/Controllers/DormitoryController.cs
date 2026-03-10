using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using DormitorySite.Models;
using DormitorySite.Services;
using System.Linq;

namespace DormitorySite.Controllers
{
    public class DormitoryController : Controller
    {
        private bool IsAuthorized() => HttpContext.Session.GetString("IsLoggedIn") == "true";

        public IActionResult Index(int floor = 1)
        {
            if (!IsAuthorized()) return RedirectToAction("Login", "Account");
            
            var floorData = DormitoryManager.Instance.Floors.FirstOrDefault(f => f.Number == floor) 
                            ?? DormitoryManager.Instance.Floors.First();
            
            ViewBag.CurrentFloor = floor;
            return View(floorData);
        }

        public IActionResult ToggleRepair(int roomNumber, int floor)
        {
            if (!IsAuthorized()) return RedirectToAction("Login", "Account");

            var room = DormitoryManager.Instance.Floors.SelectMany(f => f.Rooms).FirstOrDefault(r => r.Number == roomNumber);
            
            if (room != null)
            {
                room.IsUnderRepair = !room.IsUnderRepair;
                string msg = room.IsUnderRepair ? "закрита на ремонт" : "відкрита";
                DormitoryManager.Instance.AddLog($"Кімната №{roomNumber} {msg}.");
            }
            
            return RedirectToAction("Index", new { floor = floor });
        }

        public IActionResult Logs()
        {
            if (!IsAuthorized()) return RedirectToAction("Login", "Account");
            return View(DormitoryManager.Instance.Logs);
        }

        [HttpGet]
        public IActionResult DownloadReceipt(string fullName, int roomNumber)
        {
            if (!IsAuthorized()) return RedirectToAction("Login", "Account");

            var floor = DormitoryManager.Instance.Floors.FirstOrDefault(f => f.Rooms.Any(r => r.Number == roomNumber));
            var room = floor?.Rooms.FirstOrDefault(r => r.Number == roomNumber);
            var student = room?.Students.FirstOrDefault(s => s.FullName == fullName);

            if (student == null || floor == null || student.PaidMonths == 0)
            {
                return NotFound("Студента не знайдено, або оплата відсутня.");
            }

            byte[] fileBytes = ReceiptGenerator.GenerateReceipt(student, floor);
            string fileName = $"Receipt_{student.FullName?.Replace(" ", "_")}.txt";

            return File(fileBytes, "text/plain", fileName);
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

            var floor = DormitoryManager.Instance.Floors.FirstOrDefault(f => f.Rooms.Any(r => r.Number == student.RoomNumber));
            var room = floor?.Rooms.First(r => r.Number == student.RoomNumber);

            if (room != null)
            {
                // 1. ПЕРЕВІРКА СТАТІ
                if (room.Students.Any() && room.Students.First().Gender != student.Gender)
                {
                    string existingGender = room.Students.First().Gender ?? "інша стать";
                    ModelState.AddModelError(string.Empty, $"Неможливо поселити: у кімнаті №{room.Number} проживають тільки {existingGender.ToLower()}!");
                    
                    ViewBag.Rooms = DormitoryManager.Instance.Floors.SelectMany(f => f.Rooms).OrderBy(r => r.Number).ToList();
                    return View(student);
                }

                // 2. ПЕРЕВІРКА ВІЛЬНИХ МІСЦЬ (Делегування логіки патерну State)
                if (room.State.CanAssignStudent(room.Students.Count))
                {
                    room.Students.Add(student);

                    if (student.PaidMonths == 0)
                    {
                        DormitoryManager.Instance.AddLog($"⚠️ УВАГА: {student.FullName} (кім. №{student.RoomNumber}) НЕ ОПЛАТИВ проживання!");
                    }
                    else
                    {
                        decimal totalAmount = floor.Strategy.CalculateTotal(student.PaidMonths);
                        DormitoryManager.Instance.AddLog($"✅ Заселено: {student.FullName} (кім. №{student.RoomNumber}). Оплачено за {student.PaidMonths} міс. Сума: {totalAmount} грн.");
                    }

                    return RedirectToAction("Index", new { floor = floor.Number });
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "У цій кімнаті немає вільних місць або вона на ремонті.");
                }
            }

            ViewBag.Rooms = DormitoryManager.Instance.Floors.SelectMany(f => f.Rooms).OrderBy(r => r.Number).ToList();
            return View(student);
        }

        public IActionResult Students(string search, string gender, int? course)
        {
            if (!IsAuthorized()) return RedirectToAction("Login", "Account");
            
            var allStudents = DormitoryManager.Instance.Floors.SelectMany(f => f.Rooms.SelectMany(r => r.Students)).AsQueryable();
            
            if (!string.IsNullOrEmpty(search)) 
                allStudents = allStudents.Where(s => s.FullName != null && s.FullName.Contains(search, System.StringComparison.OrdinalIgnoreCase));
            
            if (!string.IsNullOrEmpty(gender)) 
                allStudents = allStudents.Where(s => s.Gender == gender);
            
            if (course.HasValue) 
                allStudents = allStudents.Where(s => s.Course == course.Value);
            
            return View(allStudents.ToList());
        }

        public IActionResult Calendar() => IsAuthorized() ? View() : RedirectToAction("Login", "Account");

        [HttpGet]
        public JsonResult GetCalendarEvents()
        {
            var students = DormitoryManager.Instance.Floors.SelectMany(f => f.Rooms.SelectMany(r => r.Students));
            var events = students.SelectMany(s => new[] {
                new { title = $"⬇ Вхід: {s.FullName}", start = s.SettlementDate.ToString("yyyy-MM-dd"), color = "#28a745" },
                new { title = $"⬆ Вихід: {s.FullName}", start = s.DepartureDate.ToString("yyyy-MM-dd"), color = "#dc3545" }
            });
            
            return Json(events);
        }
    }
}