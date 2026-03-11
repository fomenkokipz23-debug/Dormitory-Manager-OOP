using DormitorySite.Models;
using System.Linq;

namespace DormitorySite.Services
{
    public class DormitoryFacade
    {
        private readonly ApplicationDbContext _context;

        public DormitoryFacade(ApplicationDbContext context)
        {
            _context = context;
        }

        // Це єдиний метод ("пульт"), який знає, як правильно заселити студента
        public string SettleStudent(Student student)
        {
            // 1. Знаходимо кімнату та поверх через Singleton
            var floor = DormitoryManager.Instance.Floors.FirstOrDefault(f => f.Rooms.Any(r => r.Number == student.RoomNumber));
            var room = floor?.Rooms.FirstOrDefault(r => r.Number == student.RoomNumber);

            if (room == null) return "Помилка: Кімнату не знайдено.";

            // 2. Перевіряємо стать (Бізнес-правило)
            var currentStudents = _context.Students.Where(s => s.RoomNumber == room.Number).ToList();
            if (currentStudents.Any() && currentStudents.First().Gender != student.Gender)
            {
                return $"Помилка: у кімнаті №{room.Number} живуть тільки {currentStudents.First().Gender}!";
            }

            // 3. Використовуємо патерн STATE (питаємо у стану кімнати, чи можна заселити)
            if (!room.State.CanAssignStudent(currentStudents.Count))
            {
                return "Помилка: Кімната переповнена або на ремонті.";
            }

            // 4. Зберігаємо в базу даних
            _context.Students.Add(student);
            _context.SaveChanges();

            // 5. Логуємо подію через Singleton
            DormitoryManager.Instance.AddLog($"✅ ФАСАД: Успішно заселено {student.FullName} у кімнату {student.RoomNumber}.");

            return "Success";
        }
    }
}