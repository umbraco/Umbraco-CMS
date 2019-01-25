namespace Umbraco.Core.Dashboards
{
    /// <summary>
    /// Implements <see cref="IAccessRule"/>.
    /// </summary>
    public class AccessRule : IAccessRule
    {
        /// <inheritdoc />
        public AccessRuleType Type { get; set; }

        /// <inheritdoc />
        public string Value { get; set; }
    }
}
