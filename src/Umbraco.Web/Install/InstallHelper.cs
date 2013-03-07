using System.Web.Script.Serialization;
using System.Web.UI;

namespace Umbraco.Web.Install
{
    internal static class InstallHelper
    {

        private static readonly InstallerStepCollection Steps = new InstallerStepCollection
            {
                new Steps.Welcome(),
                new Steps.License(),
                new Steps.FilePermissions(),
                new Steps.Database(),
                new Steps.DefaultUser(),
                //new Steps.RenderingEngine(),
                new Steps.Skinning(),
                new Steps.WebPi(),
                new Steps.TheEnd()
            };

        internal static InstallerStepCollection InstallerSteps
        {
            get { return Steps; }
        }

        public static void RedirectToNextStep(Page page, string currentStep)
        {
            var s = InstallerSteps.GotoNextStep(currentStep);
            page.Response.Redirect("?installStep=" + s.Alias);
        }

        public static void RedirectToLastStep(Page page)
        {
            var s = InstallerSteps.Get("theend");
            page.Response.Redirect("?installStep=" + s.Alias);
        }


        private static int _percentage = -1;
        public static int Percentage 
        { 
            get { return _percentage; } 
            set { _percentage = value; } 
        }

        public static string Description { get; set; }
        public static string Error { get; set; }


        public static void ClearProgress()
        {
            Percentage = -1;
            Description = string.Empty;
            Error = string.Empty;
        }

        public static void SetProgress(int percent, string description, string error)
        {
            if (percent > 0)
                Percentage = percent;

            Description = description;
            Error = error;
        }

        public static string GetProgress()
        {
            var pr = new ProgressResult(Percentage, Description, Error);
            var js = new JavaScriptSerializer();
            return js.Serialize(pr);
        }
    }
}