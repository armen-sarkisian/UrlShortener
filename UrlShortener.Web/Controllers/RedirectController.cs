using Microsoft.AspNetCore.Mvc;
using UrlShortener.Domain.Services;

namespace UrlShortener.Web.Controllers;

/// <summary>
/// Переход по короткой ссылке. Префикс /s/ выбран намеренно: код на корне
/// (/{code}) конфликтовал бы с маршрутами /About и /Account/Login.
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

        // Временный редирект, а не постоянный: иначе браузер закеширует переход
        // и счётчик кликов перестанет расти.
        return Redirect(originalUrl);
    }
}
