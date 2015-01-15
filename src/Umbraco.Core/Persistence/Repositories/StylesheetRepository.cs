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

        public StylesheetRepository(IUnitOfWork work, IDatabaseUnitOfWork db, IFileSystem fileSystem)
            : base(work, fileSystem)
        {
            _dbwork = db;
        }

        #region Overrides of FileRepository<string,Stylesheet>

        public override Stylesheet Get(string id)
        {
            if (FileSystem.FileExists(id) == false)
            {
                return null;
            }

            string content;

            using (var stream = FileSystem.OpenFile(id))
            using (var reader = new StreamReader(stream, Encoding.UTF8))
            {
                content = reader.ReadToEnd();
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
                Id = path.GetHashCode(),
                VirtualPath = FileSystem.GetUrl(id)
            };

            //on initial construction we don't want to have dirty properties tracked
            // http://issues.umbraco.org/issue/U4-1946
            stylesheet.ResetDirtyProperties(false);

            return stylesheet;

        }

        //// Fix for missing Id's on FileService.GetStylesheets() call.  This is needed as sytlesheets can only bo loaded in the editor via 
        ////  their Id so listing stylesheets needs to list there Id as well for custom plugins to render the build in editor.
        ////  http://issues.umbraco.org/issue/U4-3258
        //private int GetStylesheetId(string path)
        //{
        //    var sql = new Sql()
        //        .Select("*")
        //        .From<NodeDto>()
        //        .Where("nodeObjectType = @NodeObjectType AND umbracoNode.text = @Alias",
        //            new
        //            {
        //                NodeObjectType = UmbracoObjectTypes.Stylesheet.GetGuid(),
        //                Alias = path.TrimEnd(".css").Replace("\\", "/")
        //            });
        //    var nodeDto = _dbwork.Database.FirstOrDefault<NodeDto>(sql);
        //    return nodeDto == null ? 0 : nodeDto.NodeId;
        //}

        ////This should be used later to do GetAll properly without individual selections
        //private IEnumerable<Tuple<int, string>> GetStylesheetIds(string[] paths)
        //{
        //    var sql = new Sql()
        //        .Select("*")
        //        .From<NodeDto>()
        //        .Where("nodeObjectType = @NodeObjectType AND umbracoNode.text in (@aliases)",
        //            new
        //            {
        //                NodeObjectType = UmbracoObjectTypes.Stylesheet.GetGuid(),
        //                aliases = paths.Select(x => x.TrimEnd(".css").Replace("\\", "/")).ToArray()
        //            });
        //    var dtos = _dbwork.Database.Fetch<NodeDto>(sql);

        //    return dtos.Select(x => new Tuple<int, string>(
        //        //the id
        //        x.NodeId,
        //        //the original path requested for the id
        //        paths.First(p => p.TrimEnd(".css").Replace("\\", "/") == x.Text)));
        //}

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
                var files = FindAllFiles("", "*.css");
                foreach (var file in files)
                {
                    yield return Get(file);
                }
            }
        }

        /// <summary>
        /// Gets a list of all <see cref="Stylesheet"/> that exist at the relative path specified. 
        /// </summary>
        /// <param name="rootPath">
        /// If null or not specified, will return the stylesheets at the root path relative to the IFileSystem
        /// </param>
        /// <returns></returns>
        public IEnumerable<Stylesheet> GetStylesheetsAtPath(string rootPath = null)
        {
            return FileSystem.GetFiles(rootPath ?? string.Empty, "*.css").Select(Get);
        }

        public bool ValidateStylesheet(Stylesheet stylesheet)
        {
            var dirs = SystemDirectories.Css;

            //Validate file
            var validFile = IOHelper.VerifyEditPath(stylesheet.VirtualPath, dirs.Split(','));

            //Validate extension
            var validExtension = IOHelper.VerifyFileExtension(stylesheet.VirtualPath, new List<string> { "css" });

            var fileValid = validFile && validExtension;

            //var parser = new CssParser(stylesheet.Content);

            //try
            //{
            //    var styleSheet = parser.StyleSheet;//Get stylesheet to invoke parsing
            //}
            //catch (Exception ex)
            //{
            //    //Log exception?
            //    return false;
            //}

            //return !parser.Errors.Any() && fileValid;

            return fileValid;
        }

        protected override void PersistDeletedItem(Stylesheet entity)
        {
            //find any stylesheet props in the db - this is legacy!! we don't really care about the db but we'll try to keep it tidy
            var props = _dbwork.Database.Fetch<NodeDto>(
                new Sql().Select("*")
                    .From("umbracoNode")
                    .Where("umbracoNode.parentID = @id AND nodeObjectType = @NodeObjectType", new { id = entity.Id, NodeObjectType = new Guid(Constants.ObjectTypes.StylesheetProperty) }));

            foreach (var prop in props)
            {
                _dbwork.Database.Execute("DELETE FROM cmsStylesheetProperty WHERE nodeId = @Id", new { Id = prop.NodeId });
                _dbwork.Database.Execute("DELETE FROM umbracoNode WHERE id = @Id AND nodeObjectType = @NodeObjectType", new { Id = prop.NodeId, NodeObjectType = new Guid(Constants.ObjectTypes.StylesheetProperty) });
            }
            _dbwork.Database.Execute("DELETE FROM cmsStylesheet WHERE nodeId = @Id", new { Id = entity.Id });
            _dbwork.Database.Execute("DELETE FROM umbracoNode WHERE id = @Id AND nodeObjectType = @NodeObjectType", new { Id = entity.Id, NodeObjectType = new Guid(Constants.ObjectTypes.Stylesheet) });

            base.PersistDeletedItem(entity);
        }

        #endregion
    }
}