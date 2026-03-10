using DormitorySite.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DormitorySite.Services
{
    // --- ПАТЕРН STATE ---
    // Керує станом кімнати: Вільна, Зайнята або Ремонт
    public interface IRoomState {
        string GetStatusName();
        string GetColorClass();
        bool CanAssignStudent(int currentCount);
    }

    public class FreeState : IRoomState {
        public string GetStatusName() => "Вільна";
        public string GetColorClass() => "success";
        public bool CanAssignStudent(int count) => count < 4;
    }

    public class FullState : IRoomState {
        public string GetStatusName() => "Зайнята";
        public string GetColorClass() => "danger";
        public bool CanAssignStudent(int count) => false;
    }

    public class MaintenanceState : IRoomState {
        public string GetStatusName() => "Ремонт";
        public string GetColorClass() => "secondary";
        public bool CanAssignStudent(int count) => false;
    }

    // ПРИМІТКА: Інтерфейс ICostCalculator та його реалізації (StandardRate і т.д.) 
    // тепер знаходяться у файлі Models/ICostStrategy.cs, тому тут ми їх НЕ дублюємо.

    // --- ПАТЕРН SINGLETON + LOGGER ---
    // Гарантує існування лише одного екземпляру менеджера даних
    public class DormitoryManager
    {
        private static DormitoryManager? _instance; // Додано ? для усунення попередження
        public List<Floor> Floors { get; set; }
        public List<string> Logs { get; set; } = new List<string>();

        private DormitoryManager()
        {
            Floors = new List<Floor>();
            // Поверхи ініціалізуються різними стратегіями оплати з простору імен Models
            Floors.Add(new Floor(1, "Стандарт", new StandardRate()));
            Floors.Add(new Floor(2, "Пільговий", new DiscountRate()));
            Floors.Add(new Floor(3, "Люкс", new LuxuryRate()));
        }

        public static DormitoryManager Instance => _instance ??= new DormitoryManager();
        
        public void AddLog(string msg) => Logs.Add($"[{DateTime.Now:HH:mm}] {msg}");
    }

    public class Floor {
        public int Number { get; set; }
        public string Type { get; set; }
        public ICostCalculator Strategy { get; set; } // Використання патерну Strategy
        public List<Room> Rooms { get; set; }

        public Floor(int n, string t, ICostCalculator s) {
            Number = n; Type = t; Strategy = s;
            Rooms = new List<Room>();
            for (int i = 1; i <= 10; i++) 
                Rooms.Add(new Room { Number = n * 100 + i, Students = new List<Student>() });
        }
    }
}