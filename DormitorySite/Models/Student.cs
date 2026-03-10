namespace DormitorySite.Models
{
    public class Student
    {
        public string? FullName { get; set; }
        public string? Gender { get; set; }
        public int Course { get; set; }
        public int RoomNumber { get; set; }
        public DateTime SettlementDate { get; set; }
        public DateTime DepartureDate { get; set; }
        public int PaidMonths { get; set; }
    }
}