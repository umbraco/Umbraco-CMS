using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Api.Management.ViewModels.ModelsBuilderDashboard;

/// <summary>
/// Represents the status indicating whether the Models Builder is out of date.
/// </summary>
public class OutOfDateStatusResponseModel
{
    /// <summary>
    /// Gets or sets the status that specifies the type of out-of-date condition.
    /// </summary>
    public OutOfDateType Status { get; set; }
}
