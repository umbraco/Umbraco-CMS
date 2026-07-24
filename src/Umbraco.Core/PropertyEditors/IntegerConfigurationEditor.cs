using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.IO;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     The configuration editor for the integer (numeric) property editor.
/// </summary>
public class IntegerConfigurationEditor : ConfigurationEditor<IntegerConfiguration>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="IntegerConfigurationEditor"/> class.
    /// </summary>
    /// <param name="ioHelper">The IO helper.</param>
    public IntegerConfigurationEditor(IIOHelper ioHelper)
        : base(ioHelper)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="IntegerConfigurationEditor"/> class.
    /// </summary>
    [Obsolete("Please use the constructor taking all parameters. Scheduled for removal in Umbraco 21.")]
    public IntegerConfigurationEditor()
        : this(StaticServiceProvider.Instance.GetRequiredService<IIOHelper>())
    {
    }
}
