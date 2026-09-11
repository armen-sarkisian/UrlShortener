using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using UrlShortener.Domain.Entities;
using UrlShortener.Web.Models;

namespace UrlShortener.Web.Controllers.Api;

/// <summary>
/// Who is signed in right now. Angular asks for this on start-up to decide whether to show
/// the add form and the delete buttons; the antiforgery token for modifying requests
/// is handed out here as well.
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
