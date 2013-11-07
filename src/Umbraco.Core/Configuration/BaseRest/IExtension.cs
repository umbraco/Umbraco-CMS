namespace Umbraco.Core.Configuration.BaseRest
{
    public interface IExtension
    {
        string Alias { get; }

        string Type { get; }

        IMethodSection this[string index] { get; }
    }
}