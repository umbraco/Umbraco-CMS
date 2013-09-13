namespace Umbraco.Core.Configuration.BaseRest
{
    public interface IMethod
    {
        string Name { get; }

        bool AllowAll { get; }

        string AllowGroup { get; }

        string AllowType { get; }

        string AllowMember { get; }

        bool ReturnXml { get; }
    }
}