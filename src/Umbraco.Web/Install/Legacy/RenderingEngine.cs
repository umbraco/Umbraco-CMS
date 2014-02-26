using Umbraco.Core.IO;

namespace Umbraco.Web.Install.Steps
{
    internal class RenderingEngine : InstallerStep
    {
        public RenderingEngine()
        {
            _moveToNextStepAutomaticly = true;
        }

        private bool _moveToNextStepAutomaticly;

        public override string Alias
        {
            get { return "renderingEngine"; }
        }

        public override string Name
        {
            get { return "Default Rendering Engine"; }
        }

        public override string UserControl
        {
            get { return SystemDirectories.Install + "/steps/renderingengine.ascx"; }
        }

        public override bool Completed()
        {
            return false;
        }

        public override bool HideFromNavigation
        {
            get { return true; }
        }

        public override bool MoveToNextStepAutomaticly
        {
            get { return _moveToNextStepAutomaticly; }
            set { _moveToNextStepAutomaticly = value; }
        }
    }
}