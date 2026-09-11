using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using UrlShortener.Domain.Entities;
using UrlShortener.Domain.Models;
using UrlShortener.Domain.Services;
using UrlShortener.Web.Models;

namespace UrlShortener.Web.Controllers.Api;

/// <summary>API of the short links table: the Angular application talks to it.</summary>
[ApiController]
[Route("api/shorturls")]
[AutoValidateAntiforgeryToken]
public class ShortUrlsApiController(
    IShortUrlService shortUrlService,
    UserManager<ApplicationUser> userManager) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IReadOnlyList<ShortUrlDto>>> GetAll(CancellationToken cancellationToken)
    {
        var items = await shortUrlService.GetAllAsync(cancellationToken);

        return Ok(items.Select(ToDto).ToList());
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<ShortUrlDto>> Create(
        [FromBody] CreateShortUrlRequest request,
        CancellationToken cancellationToken)
    {
        var result = await shortUrlService.CreateAsync(request.Url, userManager.GetUserId(User)!, cancellationToken);

        return result.Status switch
        {
            CreateStatus.Created => CreatedAtAction(nameof(GetAll), ToDto(result.ShortUrl!)),
            CreateStatus.Duplicate => Conflict(new ProblemDetails { Detail = result.Error }),
            _ => BadRequest(new ProblemDetails { Detail = result.Error }),
        };
    }

    [HttpDelete("{id:int}")]
    [Authorize]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var status = await shortUrlService.DeleteAsync(
            id,
            userManager.GetUserId(User)!,
            User.IsInRole(Roles.Admin),
            cancellationToken);

        return status switch
        {
            DeleteStatus.Deleted => NoContent(),
            DeleteStatus.NotFound => NotFound(),
            _ => Forbid(),
        };
    }

    private ShortUrlDto ToDto(ShortUrl source) => ShortUrlDto.From(
        source,
        $"{Request.Scheme}://{Request.Host}",
        userManager.GetUserId(User),
        User.IsInRole(Roles.Admin));
}
