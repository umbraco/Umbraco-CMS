namespace Umbraco.Cms.Core.PropertyEditors;

public interface IBlockConfiguration
{
    public Guid ContentElementTypeKey { get; set; }

    public Guid? SettingsElementTypeKey { get; set; }
}
