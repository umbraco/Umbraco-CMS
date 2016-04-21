using System.Web.Configuration;

namespace Umbraco.Web.HealthCheck.Checks.Config {

    [HealthCheck("4090C0A1-2C52-4124-92DD-F028FD066A64", "Custom Errors",
        Description = "Leaving custom errors off will display a complete stack trace to your visitors if an exception occurs.",
        Group = "Live Environment")]
    public class CustomErrorsCheck : AbstractConfigCheck {

        #region Properties

        public override string FilePath {
            get { return "~/Web.config"; }
        }

        public override string XPath {
            get { return "/configuration/system.web/customErrors/@mode"; }
        }

		public override ValueComparisonType ValueComparisonType {
			get { return ValueComparisonType.ShouldEqual; }
		}

		public override string Value {
			get { return CustomErrorsMode.RemoteOnly.ToString(); }
		}

		public override string CheckSuccessMessage {
			get { return "Custom errors are enabled."; }
		}

		public override string CheckErrorMessage {
			get { return "Custom errors are currently disabled. It is recommended to enable this setting before go live."; }
		}

        public override string RectifySuccessMessage {
            get { return "Trace mode successfully disabled."; }
        }

        #endregion

        #region Constructors

        public CustomErrorsCheck(HealthCheckContext healthCheckContext) : base(healthCheckContext) { }

        #endregion
    
    }

}