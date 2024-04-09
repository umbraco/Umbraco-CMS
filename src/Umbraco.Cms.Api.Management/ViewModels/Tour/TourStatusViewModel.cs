namespace Umbraco.Cms.Api.Management.ViewModels.Tour;

public class TourStatusViewModel
{
    public required string Alias { get; set; }

    public bool Completed { get; set; }

    public bool Disabled { get; set; }
}
