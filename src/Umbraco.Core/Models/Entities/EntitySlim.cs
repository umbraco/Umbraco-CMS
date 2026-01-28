using System.ComponentModel;
using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Models.Entities;

/// <summary>
///     Implementation of <see cref="IEntitySlim" /> for internal use.
/// </summary>
/// <remarks>
///     <para>
///         Although it implements <see cref="IEntitySlim" />, this class does not
///         implement <see cref="IRememberBeingDirty" /> and everything this interface defines, throws.
///     </para>
///     <para>
///         Although it implements <see cref="IEntitySlim" />, this class does not
///         implement <see cref="IDeepCloneable" /> and deep-cloning throws.
///     </para>
/// </remarks>
public class EntitySlim : IEntitySlim
{
    /// <summary>
    ///     Gets an entity representing "root".
    /// </summary>
    public static readonly IEntitySlim Root = new EntitySlim { Path = "-1", Name = "root", HasChildren = true };

    // implement IEntity

    /// <inheritdoc />
    [DataMember]
    public int Id { get; set; }

    /// <inheritdoc />
    [DataMember]
    public Guid Key { get; set; }

    /// <inheritdoc />
    [DataMember]
    public DateTime CreateDate { get; set; }

    /// <inheritdoc />
    [DataMember]
    public DateTime UpdateDate { get; set; }

    /// <inheritdoc />
    [DataMember]
    public DateTime? DeleteDate { get; set; }

    /// <inheritdoc />
    [DataMember]
    public bool HasIdentity => Id != 0;

    // implement ITreeEntity

    /// <inheritdoc />
    [DataMember]
    public string? Name { get; set; }

    /// <inheritdoc />
    [DataMember]
    public int CreatorId { get; set; }

    /// <inheritdoc />
    [DataMember]
    public int ParentId { get; set; }

    /// <inheritdoc />
    [DataMember]
    public int Level { get; set; }

    /// <inheritdoc />
    public void SetParent(ITreeEntity? parent) =>
        throw new InvalidOperationException("This property won't be implemented.");

    /// <inheritdoc />
    [DataMember]
    public string Path { get; set; } = string.Empty;

    /// <inheritdoc />
    [DataMember]
    public int SortOrder { get; set; }

    /// <inheritdoc />
    [DataMember]
    public bool Trashed { get; set; }

    // implement IEntitySlim

    /// <inheritdoc />
    [DataMember]
    public Guid NodeObjectType { get; set; }

    /// <inheritdoc />
    [DataMember]
    public bool HasChildren { get; set; }

    /// <inheritdoc />
    [DataMember]
    public virtual bool IsContainer { get; set; }

    /// <summary>
    ///     Occurs when a property value changes.
    /// </summary>
    /// <remarks>
    ///     This event is declared to satisfy the <see cref="ICanBeDirty" /> interface but is never raised
    ///     since <see cref="EntitySlim" /> does not track changes.
    /// </remarks>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    ///     Not implemented. Always throws <see cref="InvalidOperationException" />.
    /// </summary>
    /// <exception cref="InvalidOperationException">Always thrown as this method is not implemented.</exception>
    public void DisableChangeTracking() => throw new InvalidOperationException("This method won't be implemented.");

    /// <summary>
    ///     Not implemented. Always throws <see cref="InvalidOperationException" />.
    /// </summary>
    /// <exception cref="InvalidOperationException">Always thrown as this method is not implemented.</exception>
    public void EnableChangeTracking() => throw new InvalidOperationException("This method won't be implemented.");

    #region IDeepCloneable

    /// <summary>
    ///     Not implemented. Always throws <see cref="InvalidOperationException" />.
    /// </summary>
    /// <exception cref="InvalidOperationException">Always thrown as this method is not implemented.</exception>
    public object DeepClone() => throw new InvalidOperationException("This method won't be implemented.");

    #endregion

    /// <inheritdoc />
    public void ResetIdentity()
    {
        Id = default;
        Key = Guid.Empty;
    }

    #region IRememberBeingDirty

    // IEntitySlim does *not* track changes, but since it indirectly implements IUmbracoEntity,
    // and therefore IRememberBeingDirty, we have to have those methods - which all throw.

    /// <summary>
    ///     Not implemented. Always throws <see cref="InvalidOperationException" />.
    /// </summary>
    /// <exception cref="InvalidOperationException">Always thrown as this method is not implemented.</exception>
    public bool IsDirty() => throw new InvalidOperationException("This method won't be implemented.");

    /// <summary>
    ///     Not implemented. Always throws <see cref="InvalidOperationException" />.
    /// </summary>
    /// <param name="propName">The property name.</param>
    /// <exception cref="InvalidOperationException">Always thrown as this method is not implemented.</exception>
    public bool IsPropertyDirty(string propName) =>
        throw new InvalidOperationException("This method won't be implemented.");

    /// <summary>
    ///     Not implemented. Always throws <see cref="InvalidOperationException" />.
    /// </summary>
    /// <exception cref="InvalidOperationException">Always thrown as this method is not implemented.</exception>
    public IEnumerable<string> GetDirtyProperties() =>
        throw new InvalidOperationException("This method won't be implemented.");

    /// <summary>
    ///     Not implemented. Always throws <see cref="InvalidOperationException" />.
    /// </summary>
    /// <exception cref="InvalidOperationException">Always thrown as this method is not implemented.</exception>
    public void ResetDirtyProperties() => throw new InvalidOperationException("This method won't be implemented.");

    /// <summary>
    ///     Not implemented. Always throws <see cref="InvalidOperationException" />.
    /// </summary>
    /// <exception cref="InvalidOperationException">Always thrown as this method is not implemented.</exception>
    public bool WasDirty() => throw new InvalidOperationException("This method won't be implemented.");

    /// <summary>
    ///     Not implemented. Always throws <see cref="InvalidOperationException" />.
    /// </summary>
    /// <param name="propertyName">The property name.</param>
    /// <exception cref="InvalidOperationException">Always thrown as this method is not implemented.</exception>
    public bool WasPropertyDirty(string propertyName) =>
        throw new InvalidOperationException("This method won't be implemented.");

    /// <summary>
    ///     Not implemented. Always throws <see cref="InvalidOperationException" />.
    /// </summary>
    /// <exception cref="InvalidOperationException">Always thrown as this method is not implemented.</exception>
    public void ResetWereDirtyProperties() => throw new InvalidOperationException("This method won't be implemented.");

    /// <summary>
    ///     Not implemented. Always throws <see cref="InvalidOperationException" />.
    /// </summary>
    /// <param name="rememberDirty">A value indicating whether to remember dirty properties.</param>
    /// <exception cref="InvalidOperationException">Always thrown as this method is not implemented.</exception>
    public void ResetDirtyProperties(bool rememberDirty) =>
        throw new InvalidOperationException("This method won't be implemented.");

    /// <summary>
    ///     Not implemented. Always throws <see cref="InvalidOperationException" />.
    /// </summary>
    /// <exception cref="InvalidOperationException">Always thrown as this method is not implemented.</exception>
    public IEnumerable<string> GetWereDirtyProperties() =>
        throw new InvalidOperationException("This method won't be implemented.");

    #endregion
}
