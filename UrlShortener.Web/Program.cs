using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using UrlShortener.Domain.Entities;
using UrlShortener.Infrastructure;
using UrlShortener.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? throw new InvalidOperationException("Connection string 'ConnectionStrings:Default' is not configured.");

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

    // A redirect to the login page is useless for Angular requests: the SPA needs a status code,
    // otherwise it receives the login page HTML with status 200 instead of an error.
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
