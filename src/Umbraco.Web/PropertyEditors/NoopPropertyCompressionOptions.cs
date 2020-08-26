namespace Umbraco.Web.PropertyEditors
{
    /// <summary>
    /// Disables all compression for all properties
    /// </summary>
    internal class NoopPropertyCompressionOptions : IPropertyCompressionOptions
    {
        public bool IsCompressed(int contentTypeId, string alias) => false;
    }
}
