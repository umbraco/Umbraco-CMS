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
using Umbraco.Core.IO;

namespace umbraco.cms.businesslogic.packager
{
	public class Settings
	{

		public static string PackagerRoot
		{
            get { return SystemDirectories.Packages; }
		}

		public static string PackagesStorage
		{
            get { return SystemDirectories.Packages + IOHelper.DirSepChar + "created"; }
		}

		public static string InstalledPackagesStorage
		{
            get { return SystemDirectories.Packages + IOHelper.DirSepChar + "installed"; }
		}

		public static string InstalledPackagesSettings
		{
            get { return SystemDirectories.Packages + IOHelper.DirSepChar + "installed" + IOHelper.DirSepChar + "installedPackages.config"; }
		}

		public static string CreatedPackagesSettings
		{
            get { return SystemDirectories.Packages + IOHelper.DirSepChar + "created" + IOHelper.DirSepChar + "createdPackages.config"; }
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
				sw1 = System.IO.File.AppendText(IOHelper.MapPath(InstalledPackagesSettings));
				sw1.Close();

                sw2 = System.IO.File.AppendText(IOHelper.MapPath(CreatedPackagesSettings));
				sw1.Close();

                System.IO.Directory.CreateDirectory(IOHelper.MapPath(PackagesStorage) + IOHelper.DirSepChar + "__testFolder__");
                System.IO.Directory.CreateDirectory(IOHelper.MapPath(InstalledPackagesStorage) + IOHelper.DirSepChar + "__testFolder__");

                System.IO.Directory.Delete(IOHelper.MapPath(PackagesStorage) + IOHelper.DirSepChar + "__testFolder__", true);
                System.IO.Directory.Delete(IOHelper.MapPath(InstalledPackagesStorage) + IOHelper.DirSepChar + "__testFolder__", true);

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
