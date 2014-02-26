using Umbraco.Core.IO;
using umbraco.BusinessLogic;

namespace Umbraco.Web.Install.Steps
{
    internal class WebPi : InstallerStep
    {
        public override string Alias
        {
            get { return "webpi"; }
        }

        public override string Name
        {
            get { return "Hi " + new User(0).Name + " you are running umbraco"; }
        }

        public override bool HideFromNavigation {
          get {
            return true;
          }
        }

        public override string UserControl
        {
            get { return SystemDirectories.Install + "/steps/skinning.ascx"; }
        }

        public override bool Completed()
        {
            //this is always completed, we just want to be able to hook into directly after finishing web pi 
            return true;
        }
    }
}