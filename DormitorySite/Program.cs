var builder = WebApplication.CreateBuilder(args);

// 1. Додаємо MVC
builder.Services.AddControllersWithViews();

// 2. ДОДАНО: Реєстрація сервісу сесій
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Сесія завершиться через 30 хв бездії
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Стандартні middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// 3. ДОДАНО: Дозволяємо використання сесій (важливо ставити ПІСЛЯ UseRouting)
app.UseSession();

app.UseAuthorization();

// 4. ЗМІНЕНО: Тепер за замовчуванням відкривається сторінка входу (Account/Login)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();