using System.IO;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Persistence.Repositories
{
    public interface IScriptRepository : IAsyncReadRepository<string, IScript>, IAsyncWriteRepository<IScript>, IFileRepository, IFileWithFoldersRepository
    {
    }
}
