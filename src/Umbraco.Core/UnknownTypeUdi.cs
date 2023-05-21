namespace Umbraco.Cms.Core;

public class UnknownTypeUdi : Udi
{
    public static readonly UnknownTypeUdi Instance = new();

    private UnknownTypeUdi()
        : base("unknown", "umb://unknown/")
    {
    }

    public override bool IsRoot => false;
}
