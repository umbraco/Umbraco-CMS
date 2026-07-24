using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.IO;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     The configuration editor for the decimal property editor.
/// </summary>
public class DecimalConfigurationEditor : ConfigurationEditor<DecimalConfiguration>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DecimalConfigurationEditor"/> class.
    /// </summary>
    /// <param name="ioHelper">The IO helper.</param>
    public DecimalConfigurationEditor(IIOHelper ioHelper)
        : base(ioHelper)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DecimalConfigurationEditor"/> class.
    /// </summary>
    [Obsolete("Please use the constructor taking all parameters. Scheduled for removal in Umbraco 21.")]
    public DecimalConfigurationEditor()
        : this(StaticServiceProvider.Instance.GetRequiredService<IIOHelper>())
    {
    }
}
