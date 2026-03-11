using Microsoft.EntityFrameworkCore;

namespace DormitorySite.Models;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) 
        : base(options) { }

    // Таблиця студентів у базі даних
    public DbSet<Student> Students { get; set; }
}