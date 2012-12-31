using System.Web.UI;
using Umbraco.Core;
using Umbraco.Core.Services;
using umbraco.BusinessLogic;
using umbraco.DataLayer;

namespace Umbraco.Web.UI.Controls
{
	/// <summary>
	/// A control that exposes the helpful Umbraco context objects
	/// </summary>
	internal class UmbracoControl : Control
	{
		public UmbracoControl(UmbracoContext umbracoContext)
		{
			_umbracoContext = umbracoContext;
		}

		public UmbracoControl()
		{
			
		}

		private UmbracoContext _umbracoContext;
		protected UmbracoContext UmbracoContext
		{
			get { return _umbracoContext ?? (_umbracoContext = UmbracoContext.Current); }
		}
		protected ApplicationContext ApplicationContext
		{
			get { return UmbracoContext.Application; }
		}
		protected DatabaseContext DatabaseContext
		{
			get { return ApplicationContext.DatabaseContext; }
		}
		protected ServiceContext Services
		{
			get { return ApplicationContext.Services; }
		}

		/// <summary>
		/// Returns the legacy SqlHelper
		/// </summary>
		protected ISqlHelper SqlHelper
		{
			get { return Application.SqlHelper; }
		}
	}
}