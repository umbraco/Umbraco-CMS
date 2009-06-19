using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.IO;

namespace umbraco.cms.businesslogic.packager
{
	public class Settings
	{

		public static string PackagerRoot
		{
			get { return GlobalSettings.StorageDirectory + System.IO.Path.DirectorySeparatorChar.ToString() + "packages"; }
		}

		public static string PackagesStorage
		{
			get { return GlobalSettings.StorageDirectory + System.IO.Path.DirectorySeparatorChar.ToString() + "packages" + System.IO.Path.DirectorySeparatorChar.ToString() + "created"; }
		}

		public static string InstalledPackagesStorage
		{
			get { return GlobalSettings.StorageDirectory + System.IO.Path.DirectorySeparatorChar.ToString() + "packages" + System.IO.Path.DirectorySeparatorChar.ToString() + "installed"; }
		}

		public static string InstalledPackagesSettings
		{
			get { return GlobalSettings.StorageDirectory + System.IO.Path.DirectorySeparatorChar.ToString() + "packages" + System.IO.Path.DirectorySeparatorChar.ToString() + "installed" + System.IO.Path.DirectorySeparatorChar.ToString() + "installedPackages.config"; }
		}

		public static string CreatedPackagesSettings
		{
			get { return GlobalSettings.StorageDirectory + System.IO.Path.DirectorySeparatorChar.ToString() + "packages" + System.IO.Path.DirectorySeparatorChar.ToString() + "created" + System.IO.Path.DirectorySeparatorChar.ToString() + "createdPackages.config"; }
		}

		public static string PackageFileExtension
		{
			get { return "zip"; }
		}

		public static bool HasFileAccess(ref Exception exp)
		{
			bool hasAccess = false;
			StreamWriter sw1 = null;
			StreamWriter sw2 = null;
			try
			{
				sw1 = System.IO.File.AppendText(HttpContext.Current.Server.MapPath(InstalledPackagesSettings));
				sw1.Close();

				sw2 = System.IO.File.AppendText(HttpContext.Current.Server.MapPath(CreatedPackagesSettings));
				sw1.Close();

				System.IO.Directory.CreateDirectory(HttpContext.Current.Server.MapPath(PackagesStorage) + "\\__testFolder__");
				System.IO.Directory.CreateDirectory(HttpContext.Current.Server.MapPath(InstalledPackagesStorage) + "\\__testFolder__");

				System.IO.Directory.Delete(HttpContext.Current.Server.MapPath(PackagesStorage) + "\\__testFolder__", true);
				System.IO.Directory.Delete(HttpContext.Current.Server.MapPath(InstalledPackagesStorage) + "\\__testFolder__", true);

				hasAccess = true;
			}
			finally
			{
				if (sw1 != null)
					sw1.Close();
				if (sw2 != null)
					sw2.Close();
			}

			return hasAccess;
		}


	}

}
