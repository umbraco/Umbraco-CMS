using System;
using System.Web.UI;
using Umbraco.Core;
using Umbraco.Core.Services;

namespace Umbraco.Web
{
    /// <summary>
    /// A base class for all Presentation UserControls to inherit from
    /// </summary>
    public abstract class UmbracoUserControl : UserControl
    {
         /// <summary>
		/// Default constructor
		/// </summary>
		/// <param name="umbracoContext"></param>
		protected UmbracoUserControl(UmbracoContext umbracoContext)
		{
            if (umbracoContext == null) throw new ArgumentNullException("umbracoContext");
            UmbracoContext = umbracoContext;
            InstanceId = Guid.NewGuid();
            Umbraco = new UmbracoHelper(umbracoContext);
		}

		/// <summary>
		/// Empty constructor, uses Singleton to resolve the UmbracoContext
		/// </summary>
        protected UmbracoUserControl()
			: this(UmbracoContext.Current)
		{			
		}

        /// <summary>
        /// Useful for debugging
        /// </summary>
        internal Guid InstanceId { get; private set; }

        /// <summary>
        /// Returns an UmbracoHelper object
        /// </summary>
        public UmbracoHelper Umbraco { get; private set; }

        /// <summary>
        /// Returns the current UmbracoContext
        /// </summary>
        public UmbracoContext UmbracoContext { get; private set; }

        /// <summary>
        /// Returns the current ApplicationContext
        /// </summary>
        public ApplicationContext ApplicationContext
        {
            get { return UmbracoContext.Application; }
        }

        /// <summary>
        /// Returns a ServiceContext
        /// </summary>
        public ServiceContext Services
        {
            get { return ApplicationContext.Services; }
        }

        /// <summary>
        /// Returns a DatabaseContext
        /// </summary>
        public DatabaseContext DatabaseContext
        {
            get { return ApplicationContext.DatabaseContext; }
        }
    }
}