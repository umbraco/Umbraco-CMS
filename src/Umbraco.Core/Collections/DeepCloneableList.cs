using System.ComponentModel;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;

namespace Umbraco.Cms.Core.Collections;

/// <summary>
///     A List that can be deep cloned with deep cloned elements and can reset the collection's items dirty flags
/// </summary>
/// <typeparam name="T"></typeparam>
public class DeepCloneableList<T> : List<T>, IDeepCloneable, IRememberBeingDirty
{
    private readonly ListCloneBehavior _listCloneBehavior;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DeepCloneableList{T}" /> class with the specified clone behavior.
    /// </summary>
    /// <param name="listCloneBehavior">The clone behavior for the list.</param>
    public DeepCloneableList(ListCloneBehavior listCloneBehavior) => _listCloneBehavior = listCloneBehavior;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DeepCloneableList{T}" /> class with a collection and the specified clone behavior.
    /// </summary>
    /// <param name="collection">The collection whose elements are copied to the new list.</param>
    /// <param name="listCloneBehavior">The clone behavior for the list.</param>
    public DeepCloneableList(IEnumerable<T> collection, ListCloneBehavior listCloneBehavior)
        : base(collection) =>
        _listCloneBehavior = listCloneBehavior;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DeepCloneableList{T}" /> class with a collection.
    /// </summary>
    /// <param name="collection">The collection whose elements are copied to the new list.</param>
    /// <remarks>Default behavior is <see cref="ListCloneBehavior.CloneOnce" />.</remarks>
    public DeepCloneableList(IEnumerable<T> collection)
        : this(collection, ListCloneBehavior.CloneOnce)
    {
    }

    /// <inheritdoc />
    /// <remarks>This event is not used by the list but is required by the interface.</remarks>
    public event PropertyChangedEventHandler? PropertyChanged; // noop

    /// <summary>
    ///     Creates a new list and adds each element as a deep cloned element if it is of type IDeepCloneable
    /// </summary>
    /// <returns></returns>
    public object DeepClone()
    {
        switch (_listCloneBehavior)
        {
            case ListCloneBehavior.CloneOnce:
                // we are cloning once, so create a new list in none mode
                // and deep clone all items into it
                var newList = new DeepCloneableList<T>(ListCloneBehavior.None);
                DeepCloneHelper.CloneListItems<DeepCloneableList<T>, T>(this, newList);

                return newList;
            case ListCloneBehavior.None:
                // we are in none mode, so just return a new list with the same items
                return new DeepCloneableList<T>(this, ListCloneBehavior.None);
            case ListCloneBehavior.Always:
                // always clone to new list
                var newList2 = new DeepCloneableList<T>(ListCloneBehavior.Always);
                DeepCloneHelper.CloneListItems<DeepCloneableList<T>, T>(this, newList2);

                return newList2;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    #region IRememberBeingDirty

    /// <inheritdoc />
    public bool IsDirty() => this.OfType<IRememberBeingDirty>().Any(x => x.IsDirty());

    /// <inheritdoc />
    public bool WasDirty() => this.OfType<IRememberBeingDirty>().Any(x => x.WasDirty());

    /// <inheritdoc />
    /// <remarks>Always return false, the list has no properties that can be dirty.</remarks>
    public bool IsPropertyDirty(string propName) => false;

    /// <inheritdoc />
    /// <remarks>Always return false, the list has no properties that can be dirty.</remarks>
    public bool WasPropertyDirty(string propertyName) => false;

    /// <inheritdoc />
    /// <remarks>Always return an empty enumerable, the list has no properties that can be dirty.</remarks>
    public IEnumerable<string> GetDirtyProperties() => Enumerable.Empty<string>();

    /// <inheritdoc />
    public void ResetDirtyProperties()
    {
        foreach (IRememberBeingDirty dc in this.OfType<IRememberBeingDirty>())
        {
            dc.ResetDirtyProperties();
        }
    }

    /// <inheritdoc />
    public void DisableChangeTracking()
    {
        // noop
    }

    /// <inheritdoc />
    public void EnableChangeTracking()
    {
        // noop
    }

    /// <inheritdoc />
    public void ResetWereDirtyProperties()
    {
        foreach (IRememberBeingDirty dc in this.OfType<IRememberBeingDirty>())
        {
            dc.ResetWereDirtyProperties();
        }
    }

    /// <inheritdoc />
    public void ResetDirtyProperties(bool rememberDirty)
    {
        foreach (IRememberBeingDirty dc in this.OfType<IRememberBeingDirty>())
        {
            dc.ResetDirtyProperties(rememberDirty);
        }
    }

    /// <remarks>Always return an empty enumerable, the list has no properties that can be dirty.</remarks>
    public IEnumerable<string> GetWereDirtyProperties() => Enumerable.Empty<string>();

    #endregion
}
