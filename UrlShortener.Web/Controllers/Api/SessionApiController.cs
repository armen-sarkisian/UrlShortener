using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using UrlShortener.Domain.Entities;
using UrlShortener.Web.Models;

namespace UrlShortener.Web.Controllers.Api;

/// <summary>
/// Кто сейчас в системе. Angular спрашивает это при старте, чтобы решить,
/// показывать ли форму добавления и кнопки удаления; здесь же выдаётся
/// antiforgery-токен для изменяющих запросов.
/// </summary>
[ApiController]
[Route("api/session")]
public class SessionApiController(IAntiforgery antiforgery) : ControllerBase
{
    [HttpGet]
    public ActionResult<SessionDto> Get()
    {
        var tokens = antiforgery.GetAndStoreTokens(HttpContext);

        return Ok(new SessionDto(
            User.Identity?.IsAuthenticated == true,
            User.Identity?.Name,
            User.IsInRole(Roles.Admin),
            tokens.RequestToken ?? string.Empty));
    }
}
