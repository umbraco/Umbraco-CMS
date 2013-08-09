using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Umbraco.Core.Configuration;
using Umbraco.Core.Events;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Persistence.Repositories
{
    /// <summary>
    /// Represents a repository specific to the Recycle Bins
    /// available for Content and Media.
    /// </summary>
    internal class RecycleBinRepository
    {
        private readonly IDatabaseUnitOfWork _unitOfWork;

        public RecycleBinRepository(IDatabaseUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public bool EmptyRecycleBin(Guid nodeObjectType)
        {
            var db = _unitOfWork.Database;

            //Issue query to get all trashed content or media that has the Upload field as a property
            //The value for each field is stored in a list: FilesToDelete<string>()
            //Alias: Constants.Conventions.Media.File and ControlId: Constants.PropertyEditors.UploadField
            var sql = new Sql();
            sql.Select("DISTINCT(dataNvarchar)")
                .From<PropertyDataDto>()
                .InnerJoin<NodeDto>().On<PropertyDataDto, NodeDto>(left => left.NodeId, right => right.NodeId)
                .InnerJoin<PropertyTypeDto>().On<PropertyDataDto, PropertyTypeDto>(left => left.PropertyTypeId, right => right.Id)
                .InnerJoin<DataTypeDto>().On<PropertyTypeDto, DataTypeDto>(left => left.DataTypeId, right => right.DataTypeId)
                .Where("umbracoNode.trashed = '1' AND umbracoNode.nodeObjectType = @NodeObjectType AND dataNvarchar IS NOT NULL AND (cmsPropertyType.Alias = @FileAlias OR cmsDataType.controlId = @ControlId)",
                    new { FileAlias = Constants.Conventions.Media.File, NodeObjectType = nodeObjectType, ControlId = Constants.PropertyEditors.UploadField });

            var files = db.Fetch<string>(sql);

            //Construct and execute delete statements for all trashed items by 'nodeObjectType'
            var deletes = new List<string>
                          {
                              FormatDeleteStatement("umbracoUser2NodeNotify", "nodeId"),
                              FormatDeleteStatement("umbracoUser2NodePermission", "nodeId"),
                              FormatDeleteStatement("umbracoRelation", "parentId"),
                              FormatDeleteStatement("umbracoRelation", "childId"),
                              FormatDeleteStatement("cmsTagRelationship", "nodeId"),
                              FormatDeleteStatement("umbracoDomains", "domainRootStructureID"),
                              FormatDeleteStatement("cmsDocument", "NodeId"),
                              FormatDeleteStatement("cmsPropertyData", "contentNodeId"),
                              FormatDeleteStatement("cmsPreviewXml", "nodeId"),
                              FormatDeleteStatement("cmsContentVersion", "ContentId"),
                              FormatDeleteStatement("cmsContentXml", "nodeID"),
                              FormatDeleteStatement("cmsContent", "NodeId"),
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
                        db.Execute(delete, new { NodeObjectType = nodeObjectType });
                    }

                    trans.Complete();

                    //Trigger (internal) event with list of files to delete - RecycleBinEmptied
                    RecycleBinEmptied.RaiseEvent(new RecycleBinEventArgs(nodeObjectType, files), this);

                    return true;
                }
                catch (Exception ex)
                {
                    trans.Dispose();
                    LogHelper.Error<RecycleBinRepository>("An error occurred while emptying the Recycle Bin: " + ex.Message, ex);
                    return false;
                }
            }
        }

        public bool DeleteFiles(IEnumerable<string> files)
        {
            try
            {
                var fs = FileSystemProviderManager.Current.GetFileSystemProvider<MediaFileSystem>();
                Parallel.ForEach(files, file =>
                                        {
                                            if (UmbracoSettings.UploadAllowDirectories)
                                            {
                                                var relativeFilePath = fs.GetRelativePath(file);
                                                var parentDirectory = System.IO.Path.GetDirectoryName(relativeFilePath);
                                                fs.DeleteDirectory(parentDirectory, true);
                                            }
                                            else
                                            {
                                                fs.DeleteFile(file, true);
                                            }
                                        });

                return true;
            }
            catch (Exception ex)
            {
                LogHelper.Error<RecycleBinRepository>("An error occurred while deleting files attached to deleted nodes: " + ex.Message, ex);
                return false;
            }
        }

        private string FormatDeleteStatement(string tableName, string keyName)
        {
            return
                string.Format(
                    "DELETE FROM {0} FROM {0} as TB1 INNER JOIN umbracoNode as TB2 ON TB1.{1} = TB2.id WHERE TB2.trashed = '1' AND TB2.nodeObjectType = @NodeObjectType",
                    tableName, keyName);
        }

        /// <summary>
        /// Occurs after RecycleBin was been Emptied
        /// </summary>
        internal static event TypedEventHandler<RecycleBinRepository, RecycleBinEventArgs> RecycleBinEmptied;
    }
}