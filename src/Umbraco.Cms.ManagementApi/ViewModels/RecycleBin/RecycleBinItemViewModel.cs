namespace Umbraco.Cms.ManagementApi.ViewModels.RecycleBin;

public class RecycleBinItemViewModel
{
    public Guid Key { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;

    public string Icon { get; set; } = string.Empty;

    public bool HasChildren { get; set; }

    public bool IsContainer { get; set; }

    public Guid? ParentKey { get; set; }
}

