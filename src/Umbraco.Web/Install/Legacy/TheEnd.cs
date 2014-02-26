using Umbraco.Core.IO;

namespace Umbraco.Web.Install.Steps
{
    internal class TheEnd : InstallerStep
    {
        public override string Alias
        {
            get { return "theend"; }
        }

        public override string Name
        {
          get { return "You’re Done"; }
        }

        public override string UserControl
        {
            get { return SystemDirectories.Install + "/steps/theend.ascx"; }
        }

        public override bool Completed()
        {
            return false;
        }
    }
}