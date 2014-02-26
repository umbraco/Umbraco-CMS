using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Umbraco.Core.IO;
using umbraco.cms.businesslogic.installer;

namespace Umbraco.Web.Install.Steps
{
    internal class License : InstallerStep
    {
        public override string Alias
        {
            get { return "license"; }
        }

        public override string Name
        {
            get { return "License"; }
        }



        public override string UserControl
        {
            get { return SystemDirectories.Install + "/steps/license.ascx"; }
        }

        public override bool Completed()
        {
            return false;
        }


    }
}