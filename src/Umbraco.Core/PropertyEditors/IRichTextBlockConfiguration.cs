namespace Umbraco.Cms.Core.PropertyEditors;

public interface IRichTextBlockConfiguration : IBlockConfiguration
{
    public bool? DisplayInline { get; set; }
}
