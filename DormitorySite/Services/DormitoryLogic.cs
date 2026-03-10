using System;
using System.Collections.Generic;
using DormitorySite.Models;

namespace DormitorySite.Services;

// --- ПАТЕРН STATE (Стан) ---
public interface IRoomState 
{
    string GetStatusName();
    string GetColorClass();
    bool CanAssignStudent(int currentCount);
}

public class FreeState : IRoomState 
{
    public string GetStatusName() => "Вільна";
    public string GetColorClass() => "success";
    
    // БЕРЕМО КОНСТАНТУ З КЛАСУ ROOM (Принцип DRY)
    public bool CanAssignStudent(int count) => count < Room.MaxCapacity;
}

public class FullState : IRoomState 
{
    public string GetStatusName() => "Зайнята";
    public string GetColorClass() => "danger";
    public bool CanAssignStudent(int count) => false;
}

public class MaintenanceState : IRoomState 
{
    public string GetStatusName() => "Ремонт";
    public string GetColorClass() => "secondary";
    public bool CanAssignStudent(int count) => false;
}

// --- ПАТЕРН SINGLETON (Одинак) + Журналювання ---
public class DormitoryManager
{
    // СУЧАСНИЙ ПОТОКОБЕЗПЕЧНИЙ SINGLETON (через Lazy<T>)
    private static readonly Lazy<DormitoryManager> _lazyInstance = 
        new Lazy<DormitoryManager>(() => new DormitoryManager());

    public static DormitoryManager Instance => _lazyInstance.Value;

    public List<Floor> Floors { get; set; }
    public List<string> Logs { get; set; } = new List<string>();

    private DormitoryManager()
    {
        Floors = new List<Floor>
        {
            new Floor(1, "Стандарт", new StandardRate()),
            new Floor(2, "Пільговий", new DiscountRate()),
            new Floor(3, "Люкс", new LuxuryRate())
        };
    }
    
    public void AddLog(string msg) => Logs.Add($"[{DateTime.Now:HH:mm}] {msg}");
}

// --- КЛАС ПОВЕРХУ ---
public class Floor 
{
    public int Number { get; set; }
    public string Type { get; set; }
    public ICostCalculator Strategy { get; set; } // Використання патерну Strategy
    public List<Room> Rooms { get; set; }

    public Floor(int number, string type, ICostCalculator strategy) 
    {
        Number = number; 
        Type = type; 
        Strategy = strategy;
        Rooms = new List<Room>();
        
        for (int i = 1; i <= 10; i++) 
        {
            Rooms.Add(new Room { Number = Number * 100 + i, Students = new List<Student>() });
        }
    }
}