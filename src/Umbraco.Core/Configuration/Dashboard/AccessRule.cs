namespace Umbraco.Core.Configuration.Dashboard
{
    /// <summary>
    /// Implements <see cref="IAccessRule"/>.
    /// </summary>
    internal class AccessRule : IAccessRule
    {
        /// <inheritdoc />
        public AccessRuleType Type { get; set; }

        /// <inheritdoc />
        public string Value { get; set; }
    }
}
