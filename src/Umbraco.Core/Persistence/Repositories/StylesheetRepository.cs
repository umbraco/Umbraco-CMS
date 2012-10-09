using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Umbraco.Core.IO;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Persistence.Repositories
{
    /// <summary>
    /// Represents the Stylesheet Repository
    /// </summary>
    internal class StylesheetRepository : FileRepository<string, Stylesheet>, IStylesheetRepository
    {
        public StylesheetRepository(IUnitOfWork work)
            : base(work, FileSystemProviderManager.Current.GetFileSystemProvider("stylesheet"))
        {
        }

        #region Overrides of FileRepository<string,Stylesheet>

        public override Stylesheet Get(string id)
        {
            if (!FileSystem.FileExists(id))
            {
                throw new Exception(string.Format("The file {0} was not found", id));
            }

            var stream = FileSystem.OpenFile(id);
            byte[] bytes = new byte[stream.Length];
            stream.Position = 0;
            stream.Read(bytes, 0, (int)stream.Length);
            var content = Encoding.UTF8.GetString(bytes);

            var path = FileSystem.GetRelativePath(id);

            var stylesheet = new Stylesheet(path) { Content = content };
            return stylesheet;
        }

        public override IEnumerable<Stylesheet> GetAll(params string[] ids)
        {
            if (ids.Any())
            {
                foreach (var id in ids)
                {
                    yield return Get(id);
                }
            }
            else
            {
                var files = FileSystem.GetFiles("", "*");
                foreach (var file in files)
                {
                    yield return Get(file);
                }
            }
        }

        #endregion
    }
}