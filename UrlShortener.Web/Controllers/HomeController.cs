using Microsoft.AspNetCore.Mvc;

namespace UrlShortener.Web.Controllers;

public class HomeController : Controller
{
    /// <summary>Страница таблицы. Данные и вся работа с ними живут в Angular-приложении.</summary>
    public IActionResult Index() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() => View();
}
