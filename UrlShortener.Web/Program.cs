using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using UrlShortener.Domain.Entities;
using UrlShortener.Infrastructure;
using UrlShortener.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? throw new InvalidOperationException("Не задана строка подключения 'ConnectionStrings:Default'.");

builder.Services.AddInfrastructure(connectionString);

builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.User.RequireUniqueEmail = true;
        options.SignIn.RequireConfirmedAccount = false;
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";

    // Для запросов Angular редирект на страницу логина бесполезен: SPA нужен код ответа,
    // иначе вместо ошибки она получит HTML страницы входа со статусом 200.
    options.Events.OnRedirectToLogin = context => WriteStatusForApi(context, StatusCodes.Status401Unauthorized);
    options.Events.OnRedirectToAccessDenied = context => WriteStatusForApi(context, StatusCodes.Status403Forbidden);
});

builder.Services.AddAntiforgery(options => options.HeaderName = "X-CSRF-TOKEN");
builder.Services.AddControllersWithViews();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");

await DatabaseSeeder.SeedAsync(app.Services);

app.Run();

static Task WriteStatusForApi(
    Microsoft.AspNetCore.Authentication.RedirectContext<CookieAuthenticationOptions> context,
    int statusCode)
{
    if (context.Request.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase))
    {
        context.Response.StatusCode = statusCode;
        return Task.CompletedTask;
    }

    context.Response.Redirect(context.RedirectUri);
    return Task.CompletedTask;
}

/// <summary>Точка входа объявлена явно, чтобы тесты могли ссылаться на сборку приложения.</summary>
public partial class Program;
