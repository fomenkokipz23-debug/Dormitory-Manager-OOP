using System;
using System.Text;
using System.Linq;
using DormitorySite.Models;

namespace DormitorySite.Services
{
    public static class ReceiptGenerator
    {
        public static byte[] GenerateReceipt(Student student, Floor floor)
        {
            // Розрахунок загальної суми через твій патерн Strategy
            decimal totalAmount = floor.Strategy.CalculateTotal(student.PaidMonths);
            decimal monthlyRate = floor.Strategy.GetDailyRate() * 30;

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("========================================");
            sb.AppendLine("       КВИТАНЦІЯ ПРО ОПЛАТУ ПРОЖИВАННЯ  ");
            sb.AppendLine("========================================");
            sb.AppendLine($"Дата формування: {DateTime.Now:dd.MM.yyyy HH:mm}");
            sb.AppendLine("Організація: \"Dormitory Manager 2.0\"");
            sb.AppendLine("----------------------------------------");
            sb.AppendLine($"ПІБ платника: {student.FullName}");
            sb.AppendLine($"Кімната: №{student.RoomNumber}");
            sb.AppendLine($"Категорія: {floor.Type} (Поверх {floor.Number})");
            sb.AppendLine("----------------------------------------");
            sb.AppendLine($"Оплачений період: {student.PaidMonths} міс.");
            sb.AppendLine($"Тариф за місяць: {monthlyRate} грн.");
            sb.AppendLine("----------------------------------------");
            sb.AppendLine($"СУМА ОПЛАТИ: {totalAmount} грн.");
            sb.AppendLine("========================================");
            sb.AppendLine("Дякуємо за своєчасну оплату!");
            sb.AppendLine("Документ згенеровано автоматично.");

            // Важливо: додаємо BOM (Byte Order Mark) для UTF-8, 
            // щоб кирилиця коректно відображалася у стандартному Блокноті Windows
            byte[] preamble = Encoding.UTF8.GetPreamble();
            byte[] content = Encoding.UTF8.GetBytes(sb.ToString());
            
            return preamble.Concat(content).ToArray();
        }
    }
}