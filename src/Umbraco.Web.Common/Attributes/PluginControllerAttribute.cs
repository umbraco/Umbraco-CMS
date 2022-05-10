using Microsoft.AspNetCore.Mvc;

namespace Umbraco.Cms.Web.Common.Attributes;

/// <summary>
///     Indicates that a controller is a plugin controller and will be routed to its own area.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class PluginControllerAttribute : AreaAttribute
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="PluginControllerAttribute" /> class.
    /// </summary>
    /// <param name="areaName"></param>
    public PluginControllerAttribute(string areaName)
        : base(areaName)
    {
        // validate this, only letters and digits allowed.
        if (areaName.Any(c => !char.IsLetterOrDigit(c)))
        {
            throw new FormatException(
                $"Invalid area name \"{areaName}\": the area name can only contains letters and digits.");
        }

        AreaName = areaName;
    }

    /// <summary>
    ///     Gets the name of the area.
    /// </summary>
    public string AreaName { get; }
}
