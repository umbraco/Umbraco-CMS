using Umbraco.Core.IO;

namespace Umbraco.Web.Install.Steps
{
    internal class Welcome : InstallerStep
    {
        public override string Alias
        {
            get { return "welcome"; }
        }

        public override string Name
        {
            get { return "Welcome"; }
        }

      
        public override string UserControl
        {
            get { return SystemDirectories.Install + "/steps/welcome.ascx"; }
        }

        public override bool Completed()
        {
            return false;
        }

     }
}