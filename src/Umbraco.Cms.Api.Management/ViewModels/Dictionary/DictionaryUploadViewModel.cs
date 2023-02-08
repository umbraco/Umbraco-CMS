﻿namespace Umbraco.Cms.Api.Management.ViewModels.Dictionary;

public class DictionaryUploadViewModel
{
    public IEnumerable<DictionaryItemsImportViewModel> DictionaryItems { get; set; } = Array.Empty<DictionaryItemsImportViewModel>();

    public string? FileName { get; set; }
}
