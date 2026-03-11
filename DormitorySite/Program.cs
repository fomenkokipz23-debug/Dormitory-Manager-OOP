using DormitorySite.Models;
using DormitorySite.Services; // Додано простір імен для сервісів
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. MVC та сервіси
builder.Services.AddControllersWithViews();

// 2. БАЗА ДАНИХ (SQLite)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// --- ДОДАНО ПАТЕРН FACADE ---
// Реєструємо фасад як Scoped сервіс
builder.Services.AddScoped<DormitoryFacade>();
// ----------------------------

// 3. СЕСІЇ (для авторизації)
builder.Services.AddDistributedMemoryCache();
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