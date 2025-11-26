namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters;

public interface IRichTextBlockConfiguration : IBlockConfiguration
{
    public bool? DisplayInline { get; set; }
}
