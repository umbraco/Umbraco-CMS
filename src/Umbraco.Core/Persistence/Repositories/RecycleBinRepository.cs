using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Models.Rdbms;

using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Persistence.Repositories
{
    internal abstract class RecycleBinRepository<TId, TEntity> : VersionableRepositoryBase<TId, TEntity>, IRecycleBinRepository<TEntity> 
        where TEntity : class, IUmbracoEntity
    {
        protected RecycleBinRepository(IDatabaseUnitOfWork work, CacheHelper cache, ILogger logger, ISqlSyntaxProvider sqlSyntax)
            : base(work, cache, logger, sqlSyntax)
        {
        }

        protected abstract int RecycleBinId { get; }

        public virtual IEnumerable<TEntity> GetEntitiesInRecycleBin()
        {
            return GetByQuery(new Query<TEntity>().Where(entity => entity.Trashed));
        }

        /// <summary>
        /// Empties the Recycle Bin by running single bulk-Delete queries
        /// against the Content- or Media's Recycle Bin.
        /// </summary>
        /// <returns></returns>
        public virtual bool EmptyRecycleBin()
        {
            var db = this.Database;

            //Construct and execute delete statements for all trashed items by 'nodeObjectType'
            var deletes = new List<string>
            {
                FormatDeleteStatement("umbracoUser2NodeNotify", "nodeId"),
                FormatDeleteStatement("umbracoUser2NodePermission", "nodeId"),
                FormatDeleteStatement("umbracoRelation", "parentId"),
                FormatDeleteStatement("umbracoRelation", "childId"),
                FormatDeleteStatement("cmsTagRelationship", "nodeId"),
                FormatDeleteStatement("umbracoDomains", "domainRootStructureID"),
                FormatDeleteStatement("cmsDocument", "nodeId"),
                FormatDeleteStatement("cmsPropertyData", "contentNodeId"),
                FormatDeleteStatement("cmsPreviewXml", "nodeId"),
                FormatDeleteStatement("cmsContentVersion", "ContentId"),
                FormatDeleteStatement("cmsContentXml", "nodeId"),
                FormatDeleteStatement("cmsContent", "nodeId"),
                "UPDATE umbracoNode SET parentID = '" + RecycleBinId + "' WHERE trashed = '1' AND nodeObjectType = @NodeObjectType",
                "DELETE FROM umbracoNode WHERE trashed = '1' AND nodeObjectType = @NodeObjectType"
            };

            //Wraps in transaction - this improves performance and also ensures
            // that if any of the deletions fails that the whole thing is rolled back.
            using (var trans = db.GetTransaction())
            {
                try
                {
                    foreach (var delete in deletes)
                    {
                        db.Execute(delete, new { NodeObjectType = NodeObjectTypeId });
                    }

                    trans.Complete();

                    return true;
                }
                catch (Exception ex)
                {
                    trans.Dispose();
                    Logger.Error<RecycleBinRepository<TId, TEntity>>("An error occurred while emptying the Recycle Bin: " + ex.Message, ex);
                    return false;
                }
            }
        }

        /// <summary>
        /// Deletes all files passed in.
        /// </summary>
        /// <param name="files"></param>
        /// <returns></returns>
        public virtual bool DeleteFiles(IEnumerable<string> files)
        {
            //ensure duplicates are removed
            files = files.Distinct();

            var allsuccess = true;

            var fs = FileSystemProviderManager.Current.GetFileSystemProvider<MediaFileSystem>();
            Parallel.ForEach(files, file =>
            {
                try
                {
                    if (file.IsNullOrWhiteSpace()) return;

                    var relativeFilePath = fs.GetRelativePath(file);
                    if (fs.FileExists(relativeFilePath) == false) return;
                    
                    var parentDirectory = System.IO.Path.GetDirectoryName(relativeFilePath);

                    // don't want to delete the media folder if not using directories.
                    if (UmbracoConfig.For.UmbracoSettings().Content.UploadAllowDirectories && parentDirectory != fs.GetRelativePath("/"))
                    {
                        //issue U4-771: if there is a parent directory the recursive parameter should be true
                        fs.DeleteDirectory(parentDirectory, String.IsNullOrEmpty(parentDirectory) == false);
                    }
                    else
                    {
                        fs.DeleteFile(file, true);
                    }
                }
                catch (Exception e)
                {
                    Logger.Error<RecycleBinRepository<TId, TEntity>>("An error occurred while deleting file attached to nodes: " + file, e);
                    allsuccess = false;
                }
            });

            return allsuccess;
        }

        private string FormatDeleteStatement(string tableName, string keyName)
        {
            //This query works with sql ce and sql server:
            //DELETE FROM umbracoUser2NodeNotify WHERE umbracoUser2NodeNotify.nodeId IN 
            //(SELECT nodeId FROM umbracoUser2NodeNotify as TB1 INNER JOIN umbracoNode as TB2 ON TB1.nodeId = TB2.id WHERE TB2.trashed = '1' AND TB2.nodeObjectType = 'C66BA18E-EAF3-4CFF-8A22-41B16D66A972')
            return
                string.Format(
                    "DELETE FROM {0} WHERE {0}.{1} IN (SELECT TB1.{1} FROM {0} as TB1 INNER JOIN umbracoNode as TB2 ON TB1.{1} = TB2.id WHERE TB2.trashed = '1' AND TB2.nodeObjectType = @NodeObjectType)",
                    tableName, keyName);
        }

        /// <summary>
        /// Gets a list of files, which are referenced on items in the Recycle Bin.
        /// The list is generated by the convention that a file is referenced by 
        /// the Upload data type or a property type with the alias 'umbracoFile'.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// This is purely for backwards compatibility
        /// </remarks>
        internal List<string> GetFilesInRecycleBinForUploadField()
        {
            var db = this.Database;

            //Issue query to get all trashed content or media that has the Upload field as a property
            //The value for each field is stored in a list: FilesToDelete<string>()
            //Alias: Constants.Conventions.Media.File and PropertyEditorAlias: Constants.PropertyEditors.UploadField
            var sql = new Sql();
            sql.Select("DISTINCT(dataNvarchar)")
                .From<PropertyDataDto>()
                .InnerJoin<NodeDto>().On<PropertyDataDto, NodeDto>(left => left.NodeId, right => right.NodeId)
                .InnerJoin<PropertyTypeDto>().On<PropertyDataDto, PropertyTypeDto>(left => left.PropertyTypeId, right => right.Id)
                .InnerJoin<DataTypeDto>().On<PropertyTypeDto, DataTypeDto>(left => left.DataTypeId, right => right.DataTypeId)
                .Where("umbracoNode.trashed = '1' AND umbracoNode.nodeObjectType = @NodeObjectType AND dataNvarchar IS NOT NULL AND (cmsPropertyType.Alias = @FileAlias OR cmsDataType.propertyEditorAlias = @PropertyEditorAlias)",
                    new { FileAlias = Constants.Conventions.Media.File, NodeObjectType = NodeObjectTypeId, PropertyEditorAlias = Constants.PropertyEditors.UploadFieldAlias });

            var files = db.Fetch<string>(sql);
            return files;
        }
    }
}