namespace DormitorySite.Models
{
    // Оголошуємо інтерфейс стратегії (Абстракція)
    public interface ICostCalculator 
    { 
        decimal GetDailyRate(); 
        decimal CalculateTotal(int months); 
    }

    // Реалізація для стандартних поверхів (1 поверх)
    public class StandardRate : ICostCalculator 
    { 
        public decimal GetDailyRate() => 200m; 
        
        // Використовуємо принцип DRY: викликаємо свій же метод
        public decimal CalculateTotal(int months) => GetDailyRate() * 30 * months; 
    }

    // Реалізація для пільгових поверхів (2 поверх)
    public class DiscountRate : ICostCalculator 
    { 
        public decimal GetDailyRate() => 80m; 
        public decimal CalculateTotal(int months) => GetDailyRate() * 30 * months;
    }

    // Реалізація для люкс поверхів (3 поверх)
    public class LuxuryRate : ICostCalculator 
    { 
        public decimal GetDailyRate() => 500m; 
        public decimal CalculateTotal(int months) => GetDailyRate() * 30 * months;
    }
}