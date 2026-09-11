using Microsoft.AspNetCore.Mvc;

namespace UrlShortener.Web.Controllers;

public class HomeController : Controller
{
    /// <summary>The table page. The data and all work with it live in the Angular application.</summary>
    public IActionResult Index() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() => View();
}
