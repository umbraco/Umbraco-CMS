using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace umbraco.IO
{

	[Obsolete("Use Umbraco.Core.UI.SystemFiles instead")]
	public class SystemFiles
	{
        [Obsolete("This file is no longer used and should not be accessed!")]
		public static string AccessXml
		{
			get { return Umbraco.Core.IO.SystemFiles.AccessXml; }
		}

		public static string CreateUiXml
		{
			get { return Umbraco.Core.IO.SystemFiles.CreateUiXml; }
		}

		public static string TinyMceConfig
		{
			get { return Umbraco.Core.IO.SystemFiles.TinyMceConfig; }
		}

		public static string MetablogConfig
		{
			get { return Umbraco.Core.IO.SystemFiles.MetablogConfig; }
		}

		public static string DashboardConfig
		{
			get { return Umbraco.Core.IO.SystemFiles.DashboardConfig; }
		}


		public static string NotFoundhandlersConfig
		{
			get { return Umbraco.Core.IO.SystemFiles.NotFoundhandlersConfig; }
		}

		public static string FeedProxyConfig
		{
			get { return Umbraco.Core.IO.SystemFiles.FeedProxyConfig; }
		}

		public static string ContentCacheXml
		{
			get { return Umbraco.Core.IO.SystemFiles.ContentCacheXml; }
		}

		public static bool ContentCacheXmlIsEphemeral
		{
			get { return Umbraco.Core.IO.SystemFiles.ContentCacheXmlStoredInCodeGen; }
		}
	}
}
