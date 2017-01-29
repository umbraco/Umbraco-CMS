namespace Umbraco.Core.Deploy
{
    public interface IPrettyArtifact
    {
        string Name { get; }
        string Alias { get; }
    }
}