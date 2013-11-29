namespace Umbraco.Core.Packaging
{
    public interface IUnpackHelper
    {
        void UnPack(string sourcefilePath, string destinationDirectory);
        string UnPackToTempDirectory(string sourcefilePath);
        string ReadSingleTextFile(string sourcefilePath, string fileToRead);
    }
}