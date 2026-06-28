// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Text.RegularExpressions;
using NPoco;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Persistence.SqlSyntax;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_17_5_0;

/// <summary>
/// Converts legacy Rich Text Editor block references stored in property data from the deprecated
/// <c>data-content-udi="umb://element/{guid}"</c> attribute to the current
/// <c>data-content-key="{guid}"</c> attribute.
/// </summary>
/// <remarks>
/// <para>
/// Before the new (Tiptap) Rich Text Editor, in-line and block-level RTE blocks were persisted in the
/// <c>markup</c> with a <c>data-content-udi</c> attribute holding a full entity identifier
/// (<c>umb://element/{guid-without-dashes}</c>). The current editor and the server-side value parser
/// (<c>RichTextParsingRegexes.BlockRegex</c>) only recognise <c>data-content-key="{guid-with-dashes}"</c>,
/// where the key matches <c>blocks.contentData[].key</c>.
/// </para>
/// <para>
/// When the attribute is left in the legacy form the blocks can no longer be matched to their content, so
/// they fall out-of-band: on load they are hoisted out of the text flow and the surrounding markup collapses.
/// This migration rewrites the attribute in every affected property value.
/// </para>
/// <para>
/// The conversion is performed as a string replacement directly against <c>umbracoPropertyData.textValue</c>
/// so it is agnostic of the hosting property editor and works at any nesting depth (for example a Rich Text
/// Editor nested inside a Block List or Block Grid). It is idempotent: once an attribute has been converted to
/// <c>data-content-key</c> it is no longer matched.
/// </para>
/// </remarks>
public partial class ConvertRichTextBlockUdisToKeys : AsyncMigrationBase
{
    // The legacy attribute is only ever emitted in Rich Text Editor block markup, which makes it a safe and
    // selective filter for the rows we need to inspect.
    private const string LegacyAttribute = "data-content-udi=";

    /// <summary>
    /// Initializes a new instance of the <see cref="ConvertRichTextBlockUdisToKeys"/> class.
    /// </summary>
    /// <param name="context">The migration context.</param>
    public ConvertRichTextBlockUdisToKeys(IMigrationContext context)
        : base(context)
    {
    }

    /// <inheritdoc/>
    protected override Task MigrateAsync() => ExecuteMigration(Database);

    /// <summary>
    /// Performs the migration: rewrites legacy <c>data-content-udi</c> Rich Text Editor block references to the
    /// current <c>data-content-key</c> form in every property data row that contains them.
    /// </summary>
    /// <remarks>Extracted into an internal static method to support integration testing.</remarks>
    internal static async Task ExecuteMigration(IUmbracoDatabase database)
    {
        // On large umbracoPropertyData tables the scan/update could exceed the default command timeout.
        EnsureLongCommandTimeout(database);

        ISqlSyntaxProvider syntax = database.SqlContext.SqlSyntax;
        var table = syntax.GetQuotedTableName(PropertyDataDto.TableName);
        var idColumn = syntax.GetQuotedColumnName(PropertyDataDto.PrimaryKeyColumnName);
        var textValueColumn = syntax.GetQuotedColumnName(PropertyDataDto.TextValueColumnName);

        Sql<ISqlContext> selectSql = database.SqlContext.Sql(
            $"SELECT {idColumn}, {textValueColumn} FROM {table} WHERE {textValueColumn} LIKE @0",
            $"%{LegacyAttribute}%");

        List<PropertyDataDto> rows = await database.FetchAsync<PropertyDataDto>(selectSql);

        foreach (PropertyDataDto row in rows)
        {
            if (string.IsNullOrEmpty(row.TextValue))
            {
                continue;
            }

            var converted = ConvertLegacyBlockReferences(row.TextValue);
            if (string.Equals(converted, row.TextValue, StringComparison.Ordinal))
            {
                continue;
            }

            Sql<ISqlContext> updateSql = database.SqlContext.Sql(
                $"UPDATE {table} SET {textValueColumn} = @0 WHERE {idColumn} = @1",
                converted,
                row.Id);

            await database.ExecuteAsync(updateSql);
        }
    }

    /// <summary>
    /// Rewrites every legacy <c>data-content-udi="umb://element/{guid}"</c> Rich Text Editor block reference in the
    /// supplied value to <c>data-content-key="{guid}"</c>, normalising the identifier to the dashed GUID form used
    /// by <c>blocks.contentData[].key</c>.
    /// </summary>
    /// <param name="value">The raw property data value (typically the serialized Rich Text Editor value JSON).</param>
    /// <returns>The value with all legacy block references converted.</returns>
    /// <remarks>
    /// The captured <c>esc</c> group preserves any JSON-escaping backslashes in front of the attribute's quotes, so
    /// the same replacement works whether the markup is stored directly or nested (and therefore escaped multiple
    /// times) inside another editor's value.
    /// </remarks>
    internal static string ConvertLegacyBlockReferences(string value)
        => LegacyBlockReferenceRegex().Replace(value, static match =>
        {
            var quote = match.Groups["q"].Value;
            var key = Guid.Parse(match.Groups["guid"].Value).ToString("D");
            return $"data-content-key={quote}{key}{quote}";
        });

    // The attribute-value quote is whatever delimiter the stored markup uses, captured so it can be
    // reproduced verbatim on both sides. Depending on how the value was serialized (and how deeply it
    // is nested) that delimiter may be a literal quote, a JSON-escaped quote (\") or a unicode-escaped
    // quote ("), each optionally preceded by extra escaping backslashes when nested.
    [GeneratedRegex("data-content-udi=(?<q>\\\\*(?:u0022|\"))umb://element/(?<guid>[0-9a-fA-F]{8}-?[0-9a-fA-F]{4}-?[0-9a-fA-F]{4}-?[0-9a-fA-F]{4}-?[0-9a-fA-F]{12})\\k<q>")]
    private static partial Regex LegacyBlockReferenceRegex();
}
