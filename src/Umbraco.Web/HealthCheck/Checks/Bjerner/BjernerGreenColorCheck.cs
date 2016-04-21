using Umbraco.Web.HealthCheck.Checks.Config;

namespace Umbraco.Web.HealthCheck.Checks.Bjerner {
    
    [HealthCheck("2168F7C0-7CB5-4334-B398-086B41F27607", "Green",
        Description = "The color should obviously be green.",
        Group = "Bjerner")]
    public class BjernerGreenColorCheck : AbstractConfigCheck {

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
            get { return "Green"; }
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

        public BjernerGreenColorCheck(HealthCheckContext healthCheckContext) : base(healthCheckContext) { }

        #endregion

    }

}