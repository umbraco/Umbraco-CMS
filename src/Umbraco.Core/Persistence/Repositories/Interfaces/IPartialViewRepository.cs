using System.IO;
using Umbraco.Core.Models;

namespace Umbraco.Core.Persistence.Repositories
{
    internal interface IPartialViewRepository : IRepository<string, IPartialView>
    {
        void AddFolder(string folderPath);
        void DeleteFolder(string folderPath);
        bool ValidatePartialView(IPartialView partialView);
        Stream GetFileContentStream(string filepath);
        void SetFileContent(string filepath, Stream content);
        long GetFileSize(string filepath);
    }
}