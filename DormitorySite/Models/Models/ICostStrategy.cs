using System;

namespace DormitorySite.Models
{
    // Оголошуємо інтерфейс стратегії
    public interface ICostCalculator 
    { 
        decimal GetDailyRate(); 
        decimal CalculateTotal(int months); 
    }

    // Реалізація для стандартних поверхів (1 поверх)
    public class StandardRate : ICostCalculator 
    { 
        public decimal GetDailyRate() => 200; 
        public decimal CalculateTotal(int months) => 200 * 30 * months;
    }

    // Реалізація для пільгових поверхів (2 поверх)
    public class DiscountRate : ICostCalculator 
    { 
        public decimal GetDailyRate() => 80; 
        public decimal CalculateTotal(int months) => 80 * 30 * months;
    }

    // Реалізація для люкс поверхів (3 поверх)
    public class LuxuryRate : ICostCalculator 
    { 
        public decimal GetDailyRate() => 500; 
        public decimal CalculateTotal(int months) => 500 * 30 * months;
    }
}