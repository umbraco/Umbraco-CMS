using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.PropertyEditors;

namespace Umbraco.Cms.Infrastructure.PropertyEditors;

internal sealed class SingleBlockConfigurationEditor : ConfigurationEditor<SingleBlockConfiguration>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SingleBlockConfigurationEditor"/> class.
    /// </summary>
    /// <param name="ioHelper">The IO helper used for file and path operations.</param>
    public SingleBlockConfigurationEditor(IIOHelper ioHelper)
        : base(ioHelper)
    {
    }
}
