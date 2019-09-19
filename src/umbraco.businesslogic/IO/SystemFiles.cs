using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace umbraco.IO
{

	[Obsolete("Use Umbraco.Core.IO.SystemFiles instead")]
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

        [Obsolete("This file is no longer used and should not be accessed!")]
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

        [Obsolete("This is not used and will be removed in future versions")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static bool ContentCacheXmlIsEphemeral
		{
			get { return Umbraco.Core.IO.SystemFiles.ContentCacheXmlStoredInCodeGen; }
		}
	}
}
