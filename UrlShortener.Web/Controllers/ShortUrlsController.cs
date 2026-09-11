using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using UrlShortener.Domain.Entities;
using UrlShortener.Domain.Services;
using UrlShortener.Web.Models;

namespace UrlShortener.Web.Controllers;

/// <summary>The Short URL Info page. Closed to anonymous users, as the task requires.</summary>
[Authorize]
public class ShortUrlsController(
    IShortUrlService shortUrlService,
    UserManager<ApplicationUser> userManager) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var shortUrl = await shortUrlService.GetByIdAsync(id, cancellationToken);

        if (shortUrl is null)
        {
            return NotFound();
        }

        var model = ShortUrlDto.From(
            shortUrl,
            $"{Request.Scheme}://{Request.Host}",
            userManager.GetUserId(User),
            User.IsInRole(Roles.Admin));

        return View(model);
    }
}
