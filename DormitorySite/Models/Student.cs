using System;
using System.ComponentModel.DataAnnotations;

namespace DormitorySite.Models;

public class Student
{
    [Key] // Позначаємо, що це первинний ключ
    public int Id { get; set; }
    
    // Робимо поле обов'язковим
    [Required(ErrorMessage = "ПІБ є обов'язковим")]
    public string? FullName { get; set; }

    [Required(ErrorMessage = "Стать є обов'язковою")]
    public string? Gender { get; set; }

    // Обмежуємо курс від 1 до 6 (магістратура)
    [Range(1, 6, ErrorMessage = "Курс має бути від 1 до 6")]
    public int Course { get; set; }

    public int RoomNumber { get; set; }

    // Задаємо значення за замовчуванням (сьогоднішній день)
    [Required]
    public DateTime SettlementDate { get; set; } = DateTime.Today;

    // За замовчуванням виселення через 10 місяців (навчальний рік)
    [Required]
    public DateTime DepartureDate { get; set; } = DateTime.Today.AddMonths(10);

    // Захист від введення від'ємних чисел в оплаті
    [Range(0, 12, ErrorMessage = "Некоректна кількість місяців")]
    public int PaidMonths { get; set; }
}