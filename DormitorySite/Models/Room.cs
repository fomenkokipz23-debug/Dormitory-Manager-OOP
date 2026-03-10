using System.Collections.Generic;
using DormitorySite.Services;

namespace DormitorySite.Models;

public class Room
{
    // Інкапсулюємо максимальну кількість місць у константу (позбуваємося "магічного числа")
    public const int MaxCapacity = 4;

    public int Number { get; set; }
    
    // Одразу ініціалізуємо список, щоб уникнути NullReferenceException
    public List<Student> Students { get; set; } = new List<Student>();
    
    public bool IsUnderRepair { get; set; }

    // Патерн State (Стан) — обчислюється динамічно на основі бізнес-правил
    public IRoomState State
    {
        get
        {
            if (IsUnderRepair) return new MaintenanceState();
            if (Students.Count >= MaxCapacity) return new FullState();
            return new FreeState();
        }
    }
}