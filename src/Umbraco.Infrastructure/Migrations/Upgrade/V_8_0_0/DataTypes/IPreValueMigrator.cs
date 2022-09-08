namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_8_0_0.DataTypes;

/// <summary>
///     Defines a service migrating preValues.
/// </summary>
public interface IPreValueMigrator
{
    /// <summary>
    ///     Determines whether this migrator can migrate a data type.
    /// </summary>
    /// <param name="editorAlias">The data type editor alias.</param>
    bool CanMigrate(string editorAlias);

    /// <summary>
    ///     Gets the v8 codebase data type editor alias.
    /// </summary>
    /// <param name="editorAlias">The original v7 codebase editor alias.</param>
    /// <remarks>
    ///     <para>
    ///         This is used to validate that the migrated configuration can be parsed
    ///         by the new property editor. Return <c>null</c> to bypass this validation,
    ///         when for instance we know it will fail, and another, later migration will
    ///         deal with it.
    ///     </para>
    /// </remarks>
    string? GetNewAlias(string editorAlias);

    /// <summary>
    ///     Gets the configuration object corresponding to preValue.
    /// </summary>
    /// <param name="dataTypeId">The data type identifier.</param>
    /// <param name="editorAlias">The data type editor alias.</param>
    /// <param name="preValues">PreValues.</param>
    object GetConfiguration(int dataTypeId, string editorAlias, Dictionary<string, PreValueDto> preValues);
}
