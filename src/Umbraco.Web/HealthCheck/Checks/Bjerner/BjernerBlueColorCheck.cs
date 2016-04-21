using Umbraco.Web.HealthCheck.Checks.Config;

namespace Umbraco.Web.HealthCheck.Checks.Bjerner {
    
    [HealthCheck("BC850B85-7AD2-4280-98DB-FE5C3D724598", "Blue",
        Description = "The color should obviously be blue.",
        Group = "Bjerner")]
    public class BjernerBlueColorCheck : AbstractConfigCheck {

        #region Properties

        public override string FilePath {
            get { return "~/Bjerner.config"; }
        }

        public override string XPath {
            get { return "/root/hest/color/@value"; }
        }

        public override ValueComparisonType ValueComparisonType {
            get { return ValueComparisonType.ShouldEqual; }
        }

        public override string Value {
            get { return "Blue"; }
        }

        public override string CheckSuccessMessage {
            get { return "Yay! The color is {2}."; }
        }

        public override string CheckErrorMessage {
            get { return "Nay. The color is {3}. It should totally be {2}."; }
        }

        public override string RectifySuccessMessage {
            get { return "Yay! The color is now {2}."; }
        }

        #endregion

        #region Constructors

        public BjernerBlueColorCheck(HealthCheckContext healthCheckContext) : base(healthCheckContext) { }

        #endregion

    }

}