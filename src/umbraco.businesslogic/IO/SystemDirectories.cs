using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using umbraco.BusinessLogic;
using System.Web;
using System.IO;

namespace umbraco.IO
{
    [Obsolete("Use Umbraco.Core.IO.SystemDirectories instead")]
    public class SystemDirectories
    {
        public static string Bin
        {
			get { return global::Umbraco.Core.IO.SystemDirectories.Bin; }
        }

        public static string Base
        {
			get { return global::Umbraco.Core.IO.SystemDirectories.Base; }
        }

        public static string Config
        {
			get { return global::Umbraco.Core.IO.SystemDirectories.Config; }
        }
                
        public static string Css
        {
			get { return global::Umbraco.Core.IO.SystemDirectories.Css; }
        }

        public static string Data
        {
			get { return global::Umbraco.Core.IO.SystemDirectories.Data; }
        }

        public static string Install
        {
			get { return global::Umbraco.Core.IO.SystemDirectories.Install; }
        }

        public static string Masterpages
        {
			get { return global::Umbraco.Core.IO.SystemDirectories.Masterpages; }
        }

       
        public static string Media
        {
			get { return global::Umbraco.Core.IO.SystemDirectories.Media; }
        }

        [Obsolete("Please use MacroScripts instead!", true)]
        public static string Python
        {
			get { return global::Umbraco.Core.IO.SystemDirectories.MacroScripts; }
        }

        public static string MacroScripts
        {
			get { return global::Umbraco.Core.IO.SystemDirectories.MacroScripts; }
        }

        public static string Scripts
        {
			get { return global::Umbraco.Core.IO.SystemDirectories.Scripts; }
        }

        public static string Umbraco
        {
			get { return global::Umbraco.Core.IO.SystemDirectories.Umbraco; }
        }

        public static string Umbraco_client
        {
			get { return global::Umbraco.Core.IO.SystemDirectories.UmbracoClient; }
        }

        public static string Usercontrols
        {
			get { return global::Umbraco.Core.IO.SystemDirectories.UserControls; }
        }

        public static string Webservices
        {
			get { return global::Umbraco.Core.IO.SystemDirectories.WebServices; }
        }

        public static string Xslt
        {
			get { return global::Umbraco.Core.IO.SystemDirectories.Xslt; }
        }

        public static string Packages
        {
			get { return global::Umbraco.Core.IO.SystemDirectories.Packages; }
        }

        public static string Preview
        {
			get { return global::Umbraco.Core.IO.SystemDirectories.Preview; }
        }

        public static string Root
        {
			get { return global::Umbraco.Core.IO.SystemDirectories.Root; }
        }
    }


    
}
