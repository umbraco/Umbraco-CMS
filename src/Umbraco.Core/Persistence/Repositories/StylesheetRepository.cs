using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Umbraco.Core.IO;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Persistence.Repositories
{
    /// <summary>
    /// Represents the Stylesheet Repository
    /// </summary>
    internal class StylesheetRepository : FileRepository<string, Stylesheet>, IStylesheetRepository
    {
        private readonly IDatabaseUnitOfWork _dbwork;

        internal StylesheetRepository(IUnitOfWork work, IDatabaseUnitOfWork db, IFileSystem fileSystem)
            : base(work, fileSystem)
        {
            _dbwork = db;
        }

        public StylesheetRepository(IUnitOfWork work, IDatabaseUnitOfWork db)
            : this(work, db, new PhysicalFileSystem(SystemDirectories.Css))
        {
        }

        #region Overrides of FileRepository<string,Stylesheet>

        public override Stylesheet Get(string id)
        {
            if (FileSystem.FileExists(id) == false)
            {
                throw new Exception(string.Format("The file {0} was not found", id));
            }

            var content = string.Empty;

            using (var stream = FileSystem.OpenFile(id))
            {
                byte[] bytes = new byte[stream.Length];
                stream.Position = 0;
                stream.Read(bytes, 0, (int)stream.Length);
                content = Encoding.UTF8.GetString(bytes);
            }

            var path = FileSystem.GetRelativePath(id);
            var created = FileSystem.GetCreated(path).UtcDateTime;
            var updated = FileSystem.GetLastModified(path).UtcDateTime;

            var stylesheet = new Stylesheet(path)
            {
                Content = content,
                Key = path.EncodeAsGuid(),
                CreateDate = created,
                UpdateDate = updated,
                Id = GetStylesheetId(path),
                VirtualPath = FileSystem.GetUrl(id)
            };

            //on initial construction we don't want to have dirty properties tracked
            // http://issues.umbraco.org/issue/U4-1946
            stylesheet.ResetDirtyProperties(false);

            return stylesheet;

        }

        // Fix for missing Id's on FileService.GetStylesheets() call.  This is needed as sytlesheets can only bo loaded in the editor via 
        //  their Id so listing stylesheets needs to list there Id as well for custom plugins to render the build in editor.
        //  http://issues.umbraco.org/issue/U4-3258
        private int GetStylesheetId(string path)
        {
            var sql = new Sql()
                .Select("*")
                .From<NodeDto>()
                .Where("nodeObjectType = @NodeObjectType AND umbracoNode.text = @Alias",
                    new
                    {
                        NodeObjectType = UmbracoObjectTypes.Stylesheet.GetGuid(),
                        Alias = path.TrimEnd(".css").Replace("\\", "/")
                    });
            var nodeDto = _dbwork.Database.FirstOrDefault<NodeDto>(sql);
            return nodeDto == null ? 0 : nodeDto.NodeId;
        }

        //This should be used later to do GetAll properly without individual selections
        private IEnumerable<Tuple<int, string>> GetStylesheetIds(string[] paths)
        {
            var sql = new Sql()
                .Select("*")
                .From<NodeDto>()
                .Where("nodeObjectType = @NodeObjectType AND umbracoNode.text in (@aliases)",
                    new
                    {
                        NodeObjectType = UmbracoObjectTypes.Stylesheet.GetGuid(),
                        aliases = paths.Select(x => x.TrimEnd(".css").Replace("\\", "/")).ToArray()
                    });
            var dtos = _dbwork.Database.Fetch<NodeDto>(sql);

            return dtos.Select(x => new Tuple<int, string>(
                //the id
                x.NodeId,
                //the original path requested for the id
                paths.First(p => p.TrimEnd(".css").Replace("\\", "/") == x.Text)));
        }

        public override IEnumerable<Stylesheet> GetAll(params string[] ids)
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
                var files = FindAllFiles("");
                foreach (var file in files)
                {
                    yield return Get(file);
                }
            }
        }

        #endregion
    }
}