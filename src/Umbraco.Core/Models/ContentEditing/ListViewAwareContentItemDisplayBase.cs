using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Models.ContentEditing;

/// <summary>
///     An abstract model representing a content item that can be contained in a list view
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class ListViewAwareContentItemDisplayBase<T> : ContentItemDisplayBase<T>
    where T : ContentPropertyBasic
{
    /// <summary>
    ///     Property indicating if this item is part of a list view parent
    /// </summary>
    [DataMember(Name = "isChildOfListView")]
    public bool IsChildOfListView { get; set; }

    /// <summary>
    ///     Property for the entity's individual tree node URL
    /// </summary>
    /// <remarks>
    ///     This is required if the item is a child of a list view since the tree won't actually be loaded,
    ///     so the app will need to go fetch the individual tree node in order to be able to load it's action list (menu)
    /// </remarks>
    [DataMember(Name = "treeNodeUrl")]
    public string? TreeNodeUrl { get; set; }
}
