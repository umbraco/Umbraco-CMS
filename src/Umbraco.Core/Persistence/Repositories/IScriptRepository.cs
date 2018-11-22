using System.IO;
using Umbraco.Core.Models;

namespace Umbraco.Core.Persistence.Repositories
{
    public interface IScriptRepository : IReadRepository<string, Script>, IWriteRepository<Script>
    {
        bool ValidateScript(Script script);
        Stream GetFileContentStream(string filepath);
        void SetFileContent(string filepath, Stream content);
        long GetFileSize(string filepath);
    }
}
