namespace Umbraco.Cms.Core.PropertyEditors;

public interface IBlockGridConfiguration : IBlockConfiguration
{
    public bool? AllowAtRoot { get; set; }

    public bool? AllowInAreas { get; set; }

    public bool? HideContentEditor { get; set; }

    public int? RowMinSpan { get; set; }

    public int? RowMaxSpan { get; set; }

    public int? AreaGridColumns { get; set; }
}
