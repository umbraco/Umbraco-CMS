using System.ComponentModel;
using Microsoft.AspNetCore.Identity;
using Umbraco.Cms.Core.Models.Entities;

namespace Umbraco.Cms.Core.Security;

/// <summary>
/// Represents an identity role used for authorization within the Umbraco CMS.
/// </summary>
public class UmbracoIdentityRole : IdentityRole, IRememberBeingDirty
{
    private string _id = string.Empty;
    private string _name = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="UmbracoIdentityRole"/> class using the specified role name.
    /// </summary>
    /// <param name="roleName">The name to assign to the role.</param>
    public UmbracoIdentityRole(string roleName)
        : base(roleName)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="UmbracoIdentityRole"/> class with default values.
    /// </summary>
    public UmbracoIdentityRole()
    {
    }

    /// <summary>
    /// Occurs when a property value of the <see cref="UmbracoIdentityRole"/> changes.
    /// </summary>
    public event PropertyChangedEventHandler PropertyChanged
    {
        add => BeingDirty.PropertyChanged += value;

        remove => BeingDirty.PropertyChanged -= value;
    }

    /// <inheritdoc />
    public override string Id
    {
        get => _id;
        set
        {
            _id = value;
            HasIdentity = true;
        }
    }

    /// <inheritdoc />
    public override string? Name
    {
        get => _name;
        set => BeingDirty.SetPropertyValueAndDetectChanges(value, ref _name!, nameof(Name));
    }

    /// <inheritdoc />
    public override string? NormalizedName { get => base.Name ?? string.Empty; set => base.Name = value; }

    /// <summary>
    ///     Gets or sets a value indicating whether returns an Id has been set on this object this will be false if the object
    ///     is new and not persisted to the database
    /// </summary>
    public bool HasIdentity { get; protected set; }

    /// <summary>
    /// Gets or sets a random value that changes whenever the role is persisted to the store.
    /// This value is used to implement optimistic concurrency checks to prevent conflicting updates.
    /// </summary>
    /// <remarks>
    /// NOTE: The purpose
    /// of this value is to try to prevent concurrent writes in the DB but this is
    /// an implementation detail at the data source level that has leaked into the
    /// model. A good writeup of that is here:
    /// https://stackoverflow.com/a/37362173
    /// For our purposes currently we won't worry about this.
    /// </remarks>
    public override string? ConcurrencyStamp { get => base.ConcurrencyStamp; set => base.ConcurrencyStamp = value; }

    /// <summary>
    ///     Gets the <see cref="BeingDirty" /> instance used for change tracking.
    /// </summary>
    protected BeingDirty BeingDirty { get; } = new();

    /// <inheritdoc />
    public bool IsDirty() => BeingDirty.IsDirty();

    /// <inheritdoc />
    public bool IsPropertyDirty(string propName) => BeingDirty.IsPropertyDirty(propName);

    /// <inheritdoc />
    public IEnumerable<string> GetDirtyProperties() => BeingDirty.GetDirtyProperties();

    /// <inheritdoc />
    public void ResetDirtyProperties() => BeingDirty.ResetDirtyProperties();

    /// <inheritdoc />
    public bool WasDirty() => BeingDirty.WasDirty();

    /// <inheritdoc />
    public bool WasPropertyDirty(string propertyName) => BeingDirty.WasPropertyDirty(propertyName);

    /// <inheritdoc />
    public void ResetWereDirtyProperties() => BeingDirty.ResetWereDirtyProperties();

    /// <inheritdoc />
    public void ResetDirtyProperties(bool rememberDirty) => BeingDirty.ResetDirtyProperties(rememberDirty);

    /// <inheritdoc />
    public IEnumerable<string> GetWereDirtyProperties() => BeingDirty.GetWereDirtyProperties();

    /// <summary>
    ///     Disables change tracking.
    /// </summary>
    public void DisableChangeTracking() => BeingDirty.DisableChangeTracking();

    /// <summary>
    ///     Enables change tracking.
    /// </summary>
    public void EnableChangeTracking() => BeingDirty.EnableChangeTracking();
}
