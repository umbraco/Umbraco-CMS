﻿using Umbraco.Cms.Api.Management.ViewModels.Tree;

namespace Umbraco.Cms.Api.Management.ViewModels.DataType.Item;

public class DataTypeTreeItemResponseModel : FolderTreeItemResponseModel
{
    public string? EditorUiAlias { get; set; }

    public bool IsDeletable { get; set; }
}
