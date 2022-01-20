namespace Umbraco.Cms.Core
{
    public class UnknownTypeUdi : Udi
    {
        private UnknownTypeUdi()
            : base("unknown", "umb://unknown/")
        { }

        public static readonly UnknownTypeUdi Instance = new UnknownTypeUdi();

        public override bool IsRoot
        {
            get { return false; }
        }
    }
}
