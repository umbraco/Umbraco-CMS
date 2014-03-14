namespace Umbraco.Core.Packaging
{
    public interface IUnpackHelper
    {
        void UnPack(string sourcefilePath, string destinationDirectory);
        string UnPackToTempDirectory(string sourcefilePath);
        string ReadTextFileFromArchive(string sourcefilePath, string fileToRead);
    }
}