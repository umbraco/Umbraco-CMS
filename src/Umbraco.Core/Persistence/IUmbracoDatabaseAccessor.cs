namespace Umbraco.Core.Persistence
{
    /// <summary>
    /// Provides access to UmbracoDatabase.
    /// </summary>
    public interface IUmbracoDatabaseAccessor
    {
        UmbracoDatabase UmbracoDatabase { get; set; }
    }
}
