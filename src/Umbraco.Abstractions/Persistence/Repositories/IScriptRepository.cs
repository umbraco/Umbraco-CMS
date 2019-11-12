using System.IO;
using Umbraco.Core.Models;

namespace Umbraco.Core.Persistence.Repositories
{
    public interface IScriptRepository : IReadRepository<string, IScript>, IWriteRepository<IScript>
    {
        bool ValidateScript(IScript script);
        Stream GetFileContentStream(string filepath);
        void SetFileContent(string filepath, Stream content);
        long GetFileSize(string filepath);

        void AddFolder(string folderPath);
        void DeleteFolder(string folderPath);
    }
}
