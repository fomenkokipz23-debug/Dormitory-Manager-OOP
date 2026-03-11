using DormitorySite.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. MVC та сервіси
builder.Services.AddControllersWithViews();

// 2. БАЗА ДАНИХ (SQLite)
// Реєструємо ApplicationDbContext для роботи з базою dormitory.db
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// 3. СЕСІЇ (для авторизації)
builder.Services.AddDistributedMemoryCache(); // Потрібно для роботи сесій
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); 
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// 4. ПІДКЛЮЧАЄМО СЕСІЇ
app.UseSession();

app.UseAuthorization();

// 5. МАРШРУТИЗАЦІЯ
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();