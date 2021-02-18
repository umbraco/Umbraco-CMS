using System.IO;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Persistence.Repositories
{
    public interface IStylesheetRepository : IReadRepository<string, IStylesheet>, IWriteRepository<IStylesheet>
    {
        bool ValidateStylesheet(IStylesheet stylesheet);
        Stream GetFileContentStream(string filepath);
        void SetFileContent(string filepath, Stream content);
        long GetFileSize(string filepath);

        void AddFolder(string folderPath);
        void DeleteFolder(string folderPath);
    }
}
