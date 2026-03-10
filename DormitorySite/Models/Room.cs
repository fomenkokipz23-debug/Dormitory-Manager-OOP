using System.Collections.Generic;
using DormitorySite.Services;

namespace DormitorySite.Models
{
    public class Room
    {
        public int Number { get; set; }
        public List<Student> Students { get; set; } = new List<Student>();
        public bool IsUnderRepair { get; set; }

        public IRoomState State
        {
            get
            {
                if (IsUnderRepair) return new MaintenanceState();
                if (Students.Count >= 4) return new FullState();
                return new FreeState();
            }
        }
    }
}