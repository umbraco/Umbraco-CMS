using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Umbraco.Core.IO;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Persistence.Repositories
{
    internal class PartialViewRepository : FileRepository<string, IPartialView>, IPartialViewRepository
    {
        public PartialViewRepository(IUnitOfWork work)
			: this(work, new PhysicalFileSystem(SystemDirectories.MvcViews + "/Partials/"))
        {
        }

        public PartialViewRepository(IUnitOfWork work, IFileSystem fileSystem) : base(work, fileSystem)
        {
        }

        public override IPartialView Get(string id)
        {
            if (FileSystem.FileExists(id) == false)
            {
                return null;
            }

            string content;
            using (var stream = FileSystem.OpenFile(id))
            {
                var bytes = new byte[stream.Length];
                stream.Position = 0;
                stream.Read(bytes, 0, (int)stream.Length);
                content = Encoding.UTF8.GetString(bytes);
            }

            var path = FileSystem.GetRelativePath(id);
            var created = FileSystem.GetCreated(path).UtcDateTime;
            var updated = FileSystem.GetLastModified(path).UtcDateTime;


            var script = new PartialView(path)
            {
                //id can be the hash
                Id = path.GetHashCode(),
                Content = content,
                Key = path.EncodeAsGuid(),
                CreateDate = created,
                UpdateDate = updated,
                VirtualPath = FileSystem.GetUrl(id)
            };

            //on initial construction we don't want to have dirty properties tracked
            // http://issues.umbraco.org/issue/U4-1946
            script.ResetDirtyProperties(false);

            return script;
        }

        public override IEnumerable<IPartialView> GetAll(params string[] ids)
        {
            //ensure they are de-duplicated, easy win if people don't do this as this can cause many excess queries
            ids = ids.Distinct().ToArray();

            if (ids.Any())
            {
                foreach (var id in ids)
                {
                    yield return Get(id);
                }
            }
            else
            {
                var files = FindAllFiles("", "*.*");
                foreach (var file in files)
                {
                    yield return Get(file);
                }
            }
        }

        /// <summary>
        /// Gets a stream that is used to write to the file
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        /// <remarks>
        /// This ensures the stream includes a utf8 BOM
        /// </remarks>
        protected override Stream GetContentStream(string content)
        {
            var data = Encoding.UTF8.GetBytes(content);
            var withBom = Encoding.UTF8.GetPreamble().Concat(data).ToArray();
            return new MemoryStream(withBom);
        }
    }
}