using System.IO;
using Umbraco.Core.Models;

namespace Umbraco.Core.Persistence.Repositories
{
    public interface IUserControlRepository : IRepository<string, UserControl>
    {
        bool ValidateUserControl(UserControl userControl);
        Stream GetFileContentStream(string filepath);
        void SetFileContent(string filepath, Stream content);
        long GetFileSize(string filepath);
    }
}