namespace Umbraco.Core.Configuration.BaseRest
{
    public interface IExtension
    {
        string Alias { get; }

        string Type { get; }

        IMethod this[string index] { get; }
    }
}