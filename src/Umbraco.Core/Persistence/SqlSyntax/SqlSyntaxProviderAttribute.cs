using System;

namespace Umbraco.Core.Persistence.SqlSyntax
{
    /// <summary>
    /// Attribute for implementations of an ISqlSyntaxProvider
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class SqlSyntaxProviderAttribute : Attribute
    {
        public SqlSyntaxProviderAttribute(string providerName)
        {
            ProviderName = providerName;
        }

        /// <summary>
        /// Gets or sets the ProviderName that corresponds to the sql syntax in a provider.
        /// </summary>
        public string ProviderName { get; set; }
    }
}
