using System.IO;
using Umbraco.Core.Models;

namespace Umbraco.Core.Persistence.Repositories
{
    public interface IStylesheetRepository : IRepository<string, Stylesheet>
    {
        bool ValidateStylesheet(Stylesheet stylesheet);
        Stream GetFileContentStream(string filepath);
        void SetFileContent(string filepath, Stream content);
        long GetFileSize(string filepath);
    }
}