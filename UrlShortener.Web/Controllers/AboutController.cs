using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UrlShortener.Domain.Abstractions;
using UrlShortener.Domain.Entities;
using UrlShortener.Infrastructure.Data;
using UrlShortener.Web.Models;

namespace UrlShortener.Web.Controllers;

/// <summary>
/// Обычная Razor-страница с submit-действием: читать может кто угодно,
/// править — только администратор.
/// </summary>
public class AboutController(
    AppDbContext context,
    UserManager<ApplicationUser> userManager,
    IClock clock) : Controller
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var page = await GetOrCreateAsync(cancellationToken);

        return View(new AboutViewModel
        {
            Content = page.Content,
            UpdatedAtUtc = page.UpdatedAtUtc,
            UpdatedBy = page.UpdatedBy?.UserName,
            CanEdit = User.IsInRole(Roles.Admin),
        });
    }

    [HttpPost]
    [Authorize(Roles = Roles.Admin)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(AboutViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            model.CanEdit = true;
            return View(model);
        }

        var page = await GetOrCreateAsync(cancellationToken);

        page.Content = model.Content;
        page.UpdatedAtUtc = clock.UtcNow;
        page.UpdatedById = userManager.GetUserId(User);

        await context.SaveChangesAsync(cancellationToken);

        TempData["StatusMessage"] = "Описание сохранено.";

        return RedirectToAction(nameof(Index));
    }

    private async Task<AboutPage> GetOrCreateAsync(CancellationToken cancellationToken)
    {
        var page = await context.AboutPages
            .Include(x => x.UpdatedBy)
            .FirstOrDefaultAsync(cancellationToken);

        if (page is not null)
        {
            return page;
        }

        page = new AboutPage
        {
            Id = AboutPage.SingletonId,
            Content = string.Empty,
            UpdatedAtUtc = clock.UtcNow,
        };

        context.AboutPages.Add(page);
        await context.SaveChangesAsync(cancellationToken);

        return page;
    }
}
