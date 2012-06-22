using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using umbraco.DataLayer;
using umbraco.Linq.DTMetal.CodeBuilder;

namespace umbraco.Linq.DTMetal.Engine
{
    internal sealed class DocTypeObjectBuilder : IDisposable
    {
        private string queryDocTypes = @"SELECT [t0].[id] AS [Id], [t0].[uniqueID] AS [UniqueID], [t0].[text] AS [Name], [t1].[alias] AS [Alias], [t1].[description] AS [Description], [t1].[masterContentType] AS [ParentID]
                                                    FROM [umbracoNode] AS [t0]
                                                    INNER JOIN [cmsContentType] AS [t1] ON [t0].[id] = [t1].[nodeId]
                                                    WHERE [t0].[NodeObjectType] = @objectType ";

        private string queryProperty = @"SELECT [t0].[id] AS [Id], [t0].[Alias], [t0].[Name], [t0].[mandatory] AS [Mandatory], [t0].[validationRegExp] AS [RegularExpression], [t0].[Description], [t1].[dbType] AS [DbType], [t1].[controlId] AS [ControlId]
                                        FROM [cmsPropertyType] AS [t0]
                                        LEFT OUTER JOIN [cmsDataType] AS [t1] ON [t1].[nodeId] = [t0].[dataTypeId]
                                        WHERE [t0].[contentTypeId] = @docTypeId
                                        ORDER BY [t0].[id], [t1].[pk] ";

        private ISqlHelper _sqlHelper;
        public ISqlHelper SqlHelper
        {
            get
            {
                if (_sqlHelper == null)
                {
                    try
                    {
                        _sqlHelper = DataLayerHelper.CreateSqlHelper(this.connectionString);
                    }
                    catch
                    {
                    }
                }
                return _sqlHelper;
            }
        }

        private string connectionString;

        public List<DocType> DocumentTypes { get; set; }

        public DocTypeObjectBuilder(string connString)
        {
            this.connectionString = connString;
            this.DocumentTypes = new List<DocType>();
        }

        internal void LoadDocTypes()
        {
            var docTypes = SqlHelper.ExecuteReader(queryDocTypes + "ORDER BY [t0].[id]", SqlHelper.CreateParameter("@objectType", umbraco.Linq.DTMetal.Engine.Properties.Settings.Default.DocTypeId));

            if (docTypes.HasRecords)
            {
                while (docTypes.Read())
                {
                    DocType newDocType = BuildDocumentType(docTypes);
                    if (newDocType.ParentId > 0 && this.DocumentTypes.SingleOrDefault(p => p.Id == newDocType.ParentId) == null)
                    {
                        LoadParentDocType(newDocType.ParentId);
                    }
                    this.DocumentTypes.Add(newDocType);
                }
            }

            docTypes.Dispose();
        }

        internal DocType BuildDocumentType(IRecordsReader docTypes)
        {
            DocType newDocType = new DocType
            {
                Id = docTypes.GetId(),
                Alias = docTypes.GetAlias(),
                Description = docTypes.GetDescription(),
                Name = docTypes.GetName(),
                ParentId = docTypes.GetParentId(),
            };

            newDocType.Properties = GetProperties(newDocType.Id);
            newDocType.Associations = BuildAssociations(newDocType.Id);

            return newDocType;
        }

        internal void LoadParentDocType(int parentId)
        {
            var docType =
                SqlHelper.ExecuteReader(queryDocTypes + " AND [t1].[MasterContentType] == @parentID",
                    SqlHelper.CreateParameter("@parentID", parentId),
                    SqlHelper.CreateParameter("@objectType", umbraco.Linq.DTMetal.Engine.Properties.Settings.Default.DocTypeId)
                );

            if (docType.HasRecords)
            {
                while (docType.Read())
                {
                    var dt = BuildDocumentType(docType);
                    if (dt.ParentId != -1 && this.DocumentTypes.SingleOrDefault(p => p.Id == dt.ParentId) == null)
                    {
                        LoadParentDocType(dt.ParentId);
                    }
                    this.DocumentTypes.Add(dt);
                }
            }
            else
            {
                throw new IndexOutOfRangeException("ParentId of \'" + parentId + "\' does not exist within the database");
            }

            docType.Dispose();
        }

        internal List<DocTypeProperty> GetProperties(int docTypeId)
        {
            var properties = SqlHelper.ExecuteReader(queryProperty, SqlHelper.CreateParameter("@docTypeId", docTypeId));
            var builtProperties = new List<DocTypeProperty>();

            if (properties.HasRecords)
            {
                while (properties.Read())
                {
                    var p = BuildProperty(properties);
                    builtProperties.Add(p);
                }
            }

            properties.Dispose();

            return builtProperties;
        }

        internal DocTypeProperty BuildProperty(IRecordsReader reader)
        {
            var p = new DocTypeProperty
            {
                Alias = reader.GetAlias(),
                Description = reader.GetDescription(),
                Id = reader.GetId(),
                Mandatory = reader.GetBoolean("Mandatory"),
                Name = reader.GetName(),
                RegularExpression = reader.GetString("RegularExpression"),
                ControlId = reader.GetGuid("ControlId")
            };

            switch (reader.GetDbType())
            {
                case "Date":
                    p.DatabaseType = typeof(DateTime);
                    break;
                case "Integer":
                    p.DatabaseType = typeof(int);
                    break;
                case "Ntext":
                case "Nvarchar":
                    p.DatabaseType = typeof(string);
                    break;
                default:
                    p.DatabaseType = typeof(object);
                    break;
            }

            return p;
        }

        internal List<DocTypeAssociation> BuildAssociations(int docTypeId)
        {
            var reader =
                SqlHelper.ExecuteReader(@"SELECT [t0].[AllowedId]
                                        FROM [cmsContentTypeAllowedContentType] AS [t0]
                                        WHERE [t0].[Id] = @docTypeId",
                                    SqlHelper.CreateParameter("@docTypeId", docTypeId)
                                );

            var allowedChildren = new List<DocTypeAssociation>();

            if (reader.HasRecords)
            {
                while (reader.Read())
                {
                    allowedChildren.Add(new DocTypeAssociation
                    {
                        AllowedId = reader.GetInt("AllowedId")
                    });
                }
            }

            return allowedChildren;
        }

        #region IDisposable Members

        public void Dispose()
        {
            this.SqlHelper.Dispose();
        }

        #endregion
    }
}
