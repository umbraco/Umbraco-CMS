namespace Umbraco.Core.Persistence.DatabaseAnnotations
{
    /// <summary>
    /// Enum with the two special types that has to be supported because
    /// of the current umbraco db schema.
    /// </summary>
    public enum SpecialDbTypes
    {
        NTEXT,
        NCHAR,
        NVARCHARMAX
    }
}
