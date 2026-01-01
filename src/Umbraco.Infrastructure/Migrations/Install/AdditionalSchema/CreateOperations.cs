using Umbraco.Cms.Infrastructure.Persistence;

namespace Umbraco.Cms.Infrastructure.Migrations.Install.AdditionalSchema;

/// <summary>
/// Defines SQL schema definition statemnets for creating the additional schema objects used for provider specific operations.
/// </summary>
internal static class CreateOperations
{
    /// <summary>
    /// Creates or recreates the database schema required for the property data replacement operation, including the
    /// necessary table-valued type and stored procedure.
    /// </summary>
    /// <remarks>This method drops any existing related database objects before creating new ones to ensure
    /// the schema is up to date. It should be called during application startup or upgrade routines where the property
    /// data replacement functionality is required.</remarks>
    /// <param name="database">The database connection used to execute the schema creation commands. Must be a valid and open IUmbracoDatabase
    /// instance.</param>
    internal static void CreateSchemaForPropertyDataReplacementOperation(IUmbracoDatabase database)
    {
        // Drop existing objects if they exist (in correct order due to dependencies).
        DropSchemaForPropertyDataReplacementOperation(database);

        // Create the Table-Valued Type that matches PropertyDataDto structure (excluding auto-generated id).
        const string createTypeSql = """
        CREATE TYPE [umbracoPropertyDataTableType] AS TABLE
        (
            [versionId] INT NOT NULL,
            [propertyTypeId] INT NOT NULL,
            [languageId] INT NULL,
            [segment] NVARCHAR(256) NULL,
            [intValue] INT NULL,
            [decimalValue] DECIMAL(38, 6) NULL,
            [dateValue] DATETIME NULL,
            [varcharValue] NVARCHAR(512) NULL,
            [textValue] NVARCHAR(MAX) NULL
        );
        """;
        database.Execute(createTypeSql);

        // Create the stored procedure that accepts the TVP and performs atomic MERGE.
        // The procedure:
        // 1. Gets distinct versionIds from the input data
        // 2. Locks existing rows for those versionIds using UPDLOCK, HOLDLOCK
        // 3. Uses MERGE to UPDATE existing, INSERT new, and DELETE removed rows
        const string createProcedureSql = """
        CREATE PROCEDURE [umbracoReplacePropertyData]
            @@propertyData [umbracoPropertyDataTableType] READONLY
        AS
        BEGIN
            SET NOCOUNT ON;

            -- Get the distinct versionIds from the input data.
            -- We need to lock and process all rows for these versions.
            DECLARE @@versionIds TABLE (versionId INT PRIMARY KEY);
            INSERT INTO @@versionIds (versionId)
            SELECT DISTINCT versionId FROM @@propertyData;

            -- If no data provided, nothing to do.
            IF NOT EXISTS (SELECT 1 FROM @@versionIds)
                RETURN;

            -- Acquire locks on all existing rows for the affected versionIds.
            -- UPDLOCK prevents other transactions from modifying these rows.
            -- HOLDLOCK (SERIALIZABLE) holds locks until transaction completes.
            SELECT id FROM [umbracoPropertyData] WITH (UPDLOCK, HOLDLOCK)
            WHERE versionId IN (SELECT versionId FROM @@versionIds);

            -- Use MERGE to atomically UPDATE, INSERT, and DELETE in one statement.
            -- This matches the original ReplacePropertyValues behavior:
            -- - UPDATE rows where (versionId, propertyTypeId, languageId, segment) match
            -- - INSERT rows that don't exist in target
            -- - DELETE rows in target that aren't in source (for the affected versionIds)
            MERGE [umbracoPropertyData] AS target
            USING @@propertyData AS source
            ON (
                target.versionId = source.versionId
                AND target.propertyTypeId = source.propertyTypeId
                AND (target.languageId = source.languageId OR (target.languageId IS NULL AND source.languageId IS NULL))
                AND (target.segment = source.segment OR (target.segment IS NULL AND source.segment IS NULL))
            )
            WHEN MATCHED THEN
                UPDATE SET
                    intValue = source.intValue,
                    decimalValue = source.decimalValue,
                    dateValue = source.dateValue,
                    varcharValue = source.varcharValue,
                    textValue = source.textValue
            WHEN NOT MATCHED BY TARGET THEN
                INSERT (versionId, propertyTypeId, languageId, segment, intValue, decimalValue, dateValue, varcharValue, textValue)
                VALUES (source.versionId, source.propertyTypeId, source.languageId, source.segment,
                        source.intValue, source.decimalValue, source.dateValue, source.varcharValue, source.textValue)
            WHEN NOT MATCHED BY SOURCE AND target.versionId IN (SELECT versionId FROM @@versionIds) THEN
                DELETE;
        END
        """;
        database.Execute(createProcedureSql);
    }

    /// <summary>
    /// Drops the database schema required for the property data replacement operation, including the
    /// necessary table-valued type and stored procedure.
    /// </summary>
    /// <param name="database">The database connection used to execute the schema creation commands. Must be a valid and open IUmbracoDatabase
    /// instance.</param>
    internal static void DropSchemaForPropertyDataReplacementOperation(IUmbracoDatabase database)
    {
        // Drop existing objects if they exist (in correct order due to dependencies).
        const string dropExistingSql = """
        IF OBJECT_ID('umbracoReplacePropertyData', 'P') IS NOT NULL
            DROP PROCEDURE [umbracoReplacePropertyData];

        IF TYPE_ID('umbracoPropertyDataTableType') IS NOT NULL
            DROP TYPE [umbracoPropertyDataTableType];
        """;
        database.Execute(dropExistingSql);
    }
}
