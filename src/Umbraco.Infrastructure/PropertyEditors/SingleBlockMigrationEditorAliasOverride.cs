namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
/// Provides an ambient, opt-in override of the property editor alias used to resolve the value editor of a block
/// property value while mapping block editor values back from the editor.
/// </summary>
/// <remarks>
/// <para>
/// The migration to the single block property editor converts nested block property values to a
/// <see cref="Models.Blocks.SingleBlockValue" /> and switches the corresponding data types over to
/// <see cref="Constants.PropertyEditors.Aliases.SingleBlock" /> in the same transaction. Re-serializing a converted
/// value runs through the containing block editor, which resolves the value editor of every nested property from the
/// property editor alias of the property's data type - so without this the conversion would depend on the migration
/// being able to observe its own, uncommitted, update of those data types. It cannot: the values are re-serialized on
/// separate scopes, and therefore separate connections. Handing the Block List value editor a value that is already
/// in single block shape makes it yield null, silently replacing the content (see
/// https://github.com/umbraco/Umbraco-CMS/issues/23596).
/// </para>
/// <para>
/// Registering the converted data type keys here makes that resolution explicit, so the conversion no longer depends
/// on when - or whether - the data type update becomes visible.
/// </para>
/// </remarks>
internal static class SingleBlockMigrationEditorAliasOverride
{
    private static readonly AsyncLocal<IReadOnlySet<Guid>?> _dataTypeKeys = new();

    /// <summary>
    /// Resolves block property values of the specified data types with the single block property editor until the
    /// returned <see cref="IDisposable" /> is disposed.
    /// </summary>
    /// <param name="dataTypeKeys">
    /// The keys of the data types being converted to the single block property editor. Held by reference for the
    /// lifetime of the returned scope - which is opened once per converted property value - so it must not be
    /// mutated while in use.
    /// </param>
    /// <returns>A disposable that restores the previous override when disposed.</returns>
    public static IDisposable For(IReadOnlySet<Guid> dataTypeKeys) => new OverrideScope(dataTypeKeys);

    /// <summary>
    /// Gets the property editor alias to resolve the value editor of a block property value with.
    /// </summary>
    /// <param name="dataTypeKey">The key of the block property value's data type.</param>
    /// <param name="propertyEditorAlias">The property editor alias of the block property value's property type.</param>
    /// <returns>
    /// <see cref="Constants.PropertyEditors.Aliases.SingleBlock" /> if the data type is being converted on the
    /// executing context; otherwise <paramref name="propertyEditorAlias" />.
    /// </returns>
    public static string Resolve(Guid dataTypeKey, string propertyEditorAlias)
    {
        IReadOnlySet<Guid>? dataTypeKeys = _dataTypeKeys.Value;

        return dataTypeKeys is not null && dataTypeKeys.Contains(dataTypeKey)
            ? Constants.PropertyEditors.Aliases.SingleBlock
            : propertyEditorAlias;
    }

    private sealed class OverrideScope : IDisposable
    {
        private readonly IReadOnlySet<Guid>? _previous;

        public OverrideScope(IReadOnlySet<Guid> dataTypeKeys)
        {
            _previous = _dataTypeKeys.Value;
            _dataTypeKeys.Value = dataTypeKeys;
        }

        public void Dispose() => _dataTypeKeys.Value = _previous;
    }
}
