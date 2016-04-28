namespace Umbraco.Web.HealthCheck.Checks.Config {
    
    [HealthCheck("61214FF3-FC57-4B31-B5CF-1D095C977D6D", "Debug Compilation Mode",
        Description = "Leaving debug compilation mode enabled can severely slow down a website and take up more memory on the server.",
        Group = "Live Environment")]
    public class CompilationDebugCheck : AbstractConfigCheck {

        #region Properties

        public override string FilePath {
            get { return "~/Web.config"; }
        }

        public override string XPath {
            get { return "/configuration/system.web/compilation/@debug"; }
        }

        public override ValueComparisonType ValueComparisonType {
            get { return ValueComparisonType.ShouldEqual; }
        }

        public override string Value {
            get { return bool.FalseString.ToLower(); }
        }

        public override string CheckSuccessMessage {
            get { return "Debug compilation mode is disabled."; }
        }

        public override string CheckErrorMessage {
            get { return "Debug compilation mode is currently enabled. It is recommended to disable this setting before go live."; }
        }

        public override string RectifySuccessMessage {
            get { return "Debug compulation mode successfully disabled."; }
        }

        #endregion

        #region Constructors

        public CompilationDebugCheck(HealthCheckContext healthCheckContext) : base(healthCheckContext) { }

        #endregion

    }

}