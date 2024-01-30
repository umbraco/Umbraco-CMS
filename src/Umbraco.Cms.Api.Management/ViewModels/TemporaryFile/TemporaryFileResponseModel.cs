using System.ComponentModel.DataAnnotations;

namespace Umbraco.Cms.Api.Management.ViewModels.TemporaryFile;

public class TemporaryFileResponseModel
{
    public Guid Id { get; set; }

    public DateTime? AvailableUntil { get; set; }

    [Required]
    public string FileName { get; set; } = string.Empty;
}
