using System;
namespace umbraco.Linq.Core
{
    /// <summary>
    /// Abstraction of the UmbracoDataContext
    /// </summary>
    public interface IUmbracoDataContext : IDisposable
    {
        /// <summary>
        /// Gets the data provider.
        /// </summary>
        /// <value>The data provider.</value>
        UmbracoDataProvider DataProvider { get; }

        /// <summary>
        /// Submits the changes tracked in this data context
        /// </summary>
        void SubmitChanges();
    }
}
