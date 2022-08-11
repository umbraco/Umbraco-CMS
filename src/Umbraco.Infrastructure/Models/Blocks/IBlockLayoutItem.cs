namespace Umbraco.Cms.Core.Models.Blocks;

public interface IBlockLayoutItem
{
    public Udi? ContentUdi { get; set; }

    public Udi? SettingsUdi { get; set; }
}
