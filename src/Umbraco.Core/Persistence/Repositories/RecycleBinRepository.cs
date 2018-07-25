using System;
using System.Collections.Generic;
using Umbraco.Core.Configuration.UmbracoSettings;
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
        protected RecycleBinRepository(IScopeUnitOfWork work, CacheHelper cache, ILogger logger, ISqlSyntaxProvider sqlSyntax, IContentSection contentSection)
            : base(work, cache, logger, sqlSyntax, contentSection)
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
                FormatDeleteStatement("cmsTask", "nodeId"),
                FormatDeleteStatement("umbracoUser2NodeNotify", "nodeId"),
                FormatDeleteStatement("umbracoUserGroup2NodePermission", "nodeId"),
                @"DELETE FROM umbracoAccessRule WHERE umbracoAccessRule.accessId IN (
                    SELECT TB1.id FROM umbracoAccess as TB1 
                    INNER JOIN umbracoNode as TB2 ON TB1.nodeId = TB2.id 
                    WHERE TB2.trashed = '1' AND TB2.nodeObjectType = @NodeObjectType)",
                FormatDeleteStatement("umbracoAccess", "nodeId"),
                @"DELETE FROM umbracoRedirectUrl WHERE umbracoRedirectUrl.id IN(
                    SELECT TB1.id FROM umbracoRedirectUrl as TB1
                    INNER JOIN umbracoNode as TB2 ON TB1.contentKey = TB2.uniqueId
                    WHERE TB2.trashed = '1' AND TB2.nodeObjectType = @NodeObjectType)",
                FormatDeleteStatement("umbracoUserStartNode", "startNode"),
                FormatUpdateStatement("umbracoUserGroup", "startContentId"),
                FormatUpdateStatement("umbracoUserGroup", "startMediaId"),
                FormatDeleteStatement("umbracoRelation", "parentId"),
                FormatDeleteStatement("umbracoRelation", "childId"),
                FormatDeleteStatement("cmsTagRelationship", "nodeId"),
                FormatDeleteStatement("umbracoDomains", "domainRootStructureID"),
                FormatDeleteStatement("cmsDocument", "nodeId"),
                FormatDeleteStatement("cmsMedia", "nodeId"),
                FormatDeleteStatement("cmsPropertyData", "contentNodeId"),
                FormatDeleteStatement("cmsPreviewXml", "nodeId"),
                FormatDeleteStatement("cmsContentVersion", "ContentId"),
                FormatDeleteStatement("cmsContentXml", "nodeId"),
                FormatDeleteStatement("cmsContent", "nodeId"),
                //TODO: Why is this being done? We just delete this exact data in the next line
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
                    // transaction will rollback
                    Logger.Error<RecycleBinRepository<TId, TEntity>>("An error occurred while emptying the Recycle Bin: " + ex.Message, ex);
                    throw;
                }
            }
        }

        /// <summary>
        /// A delete statement that will delete anything in the table specified where its PK (keyName) is found in the
        /// list of umbracoNode.id that have trashed flag set
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="keyName"></param>
        /// <returns></returns>
        private string FormatDeleteStatement(string tableName, string keyName)
        {
            return
                string.Format(
                    "DELETE FROM {0} WHERE {0}.{1} IN (SELECT id FROM umbracoNode WHERE trashed = '1' AND nodeObjectType = @NodeObjectType)",
                    tableName, keyName);
        }

        /// <summary>
        /// An update statement that will update a value to NULL in the table specified where its PK (keyName) is found in the
        /// list of umbracoNode.id that have trashed flag set
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="keyName"></param>
        /// <returns></returns>
        private string FormatUpdateStatement(string tableName, string keyName)
        {
            return
                string.Format(
                    "UPDATE {0} SET {0}.{1} = NULL WHERE {0}.{1} IN (SELECT id FROM umbracoNode WHERE trashed = '1' AND nodeObjectType = @NodeObjectType)",
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