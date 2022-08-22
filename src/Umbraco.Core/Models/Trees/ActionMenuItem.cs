using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Models.Trees;

/// <inheritdoc />
/// <summary>
///     A menu item that represents some JS that needs to execute when the menu item is clicked.
/// </summary>
/// <remarks>
///     These types of menu items are rare but they do exist. Things like refresh node simply execute
///     JS and don't launch a dialog.
///     Each action menu item describes what angular service that it's method exists in and what the method name is.
///     An action menu item must describe the angular service name for which it's method exists. It may also define what
///     the
///     method name is that will be called in this service but if one is not specified then we will assume the method name
///     is the
///     same as the Type name of the current action menu class.
/// </remarks>
public abstract class ActionMenuItem : MenuItem
{
    protected ActionMenuItem(string alias, string name)
        : base(alias, name) => Initialize();

    protected ActionMenuItem(string alias, ILocalizedTextService textService)
        : base(alias, textService) =>
        Initialize();

    /// <summary>
    ///     The angular service name containing the <see cref="AngularServiceMethodName" />
    /// </summary>
    public abstract string AngularServiceName { get; }

    /// <summary>
    ///     The angular service method name to call for this menu item
    /// </summary>
    public virtual string? AngularServiceMethodName { get; } = null;

    private void Initialize()
    {
        // add the current type to the metadata
        if (AngularServiceMethodName.IsNullOrWhiteSpace())
        {
            // if no method name is supplied we will assume that the menu action is the type name of the current menu class
            ExecuteJsMethod($"{AngularServiceName}.{GetType().Name}");
        }
        else
        {
            ExecuteJsMethod($"{AngularServiceName}.{AngularServiceMethodName}");
        }
    }
}
