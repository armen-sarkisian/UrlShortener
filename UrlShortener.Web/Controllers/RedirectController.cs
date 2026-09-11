using Microsoft.AspNetCore.Mvc;
using UrlShortener.Domain.Services;

namespace UrlShortener.Web.Controllers;

/// <summary>
/// Following a short link. The /s/ prefix is deliberate: a code at the root (/{code})
/// would collide with the /About and /Account/Login routes.
/// </summary>
[Route("s")]
public class RedirectController(IShortUrlService shortUrlService) : Controller
{
    [HttpGet("{code}")]
    public async Task<IActionResult> Go(string code, CancellationToken cancellationToken)
    {
        var originalUrl = await shortUrlService.ResolveAsync(code, cancellationToken);

        if (originalUrl is null)
        {
            return NotFound();
        }

        // A temporary redirect rather than a permanent one: otherwise the browser caches the jump
        // and the click counter stops growing.
        return Redirect(originalUrl);
    }
}
