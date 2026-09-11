using System.ComponentModel.DataAnnotations;

namespace UrlShortener.Web.Models;

public sealed class AboutViewModel
{
    [Required(ErrorMessage = "Описание не может быть пустым")]
    [Display(Name = "Описание алгоритма")]
    public string Content { get; set; } = string.Empty;

    public DateTime UpdatedAtUtc { get; set; }

    public string? UpdatedBy { get; set; }

    public bool CanEdit { get; set; }
}
