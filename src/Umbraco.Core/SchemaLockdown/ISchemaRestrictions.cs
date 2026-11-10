namespace Umbraco.Cms.Core.SchemaLockdown;

/// <summary>
/// Answers which permission verbs schema lockdown denies.
/// </summary>
/// <remarks>
/// The restrictions are built once at start-up through <see cref="ISchemaRestrictionsBuilder"/> and can't change
/// afterwards. The verbs of the entity types Umbraco governs are listed in <see cref="Constants.Permissions"/>.
/// </remarks>
public interface ISchemaRestrictions
{
    /// <summary>
    /// Gets the permission verbs that are denied, for example <c>Umb.DocumentType.Create</c>. Any other verb is permitted.
    /// </summary>
    IReadOnlyCollection<string> DeniedPermissions { get; }

    /// <summary>
    /// Gets a value indicating whether the supplied permission verb is denied.
    /// </summary>
    /// <param name="permission">The permission verb, for example one of <see cref="Constants.Permissions"/>. Matched case-sensitively.</param>
    /// <returns><c>true</c> if the permission verb is denied; otherwise <c>false</c>.</returns>
    bool IsDenied(string permission);
}
