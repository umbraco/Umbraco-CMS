using System;
using System.Linq;
using Umbraco.Web;
using Umbraco.Core.Persistence;
using umbraco.cms.businesslogic;
using umbraco.cms.businesslogic.web;
using NUnit.Framework;
using System.Text;
using Umbraco.Core.Models.Rdbms;
using System.Runtime.CompilerServices;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic.datatype;
using System.Collections.Generic;
using Umbraco.Core;
using umbraco.cms.businesslogic.media;
using Umbraco.Tests.BusinessLogic;

namespace Umbraco.Tests.ORM
{
    public partial class TestRepositoryAbstractionLayer
    {
        private class TestDatabase
        {
            private UmbracoContext _context;
            private UmbracoDatabase _database;
            public TestDatabase(UmbracoContext context)
            {
                _context = context;
                _database = context.Application.DatabaseContext.Database;
            }
        }

        private TestDatabase _testDatabase; // just to have test context and its database initialized

        internal NodesRepository Nodes { get; private set; }
        internal MacroRepository Macro { get; private set; }
        internal PreValueRepository PreValue { get; private set; }
        internal PropertyRepository Property { get; private set; }
        internal RecycleBinRepository RecycleBin { get; private set; }
        internal RelationRepository Relation { get; private set; }
        internal TemplateRepository Template { get; private set; }
        internal UsersRepository Users { get; private set; }
        internal TagRepository Tag { get; private set; }
        internal TaskRepository Task { get; private set; }

        public UmbracoDatabase Repository { get; private set; }

        public TestRepositoryAbstractionLayer(UmbracoContext context)
        {
            _testDatabase = new TestDatabase(context);  

            Repository = (new DefaultDatabaseFactory()).CreateDatabase();

            this.Nodes = new NodesRepository(this);
            this.Macro = new MacroRepository(this);
            this.PreValue = new PreValueRepository(this);
            this.Property = new PropertyRepository(this);
            this.RecycleBin = new RecycleBinRepository(this);
            this.Relation = new RelationRepository(this);
            this.Template = new TemplateRepository(this);
            this.Users = new UsersRepository(this);
            this.Tag = new TagRepository(this);
            this.Task = new TaskRepository(this);
        }

        // short-cut
        private UmbracoDatabase r { get { return this.Repository; } }

        public T GetDto<T>(int id, string idKeyName = "id")
        {
            return Repository.SingleOrDefault<T>(string.Format("where {0} = @0", idKeyName), id);
        }
        public T GetDto<T>(string whereStr, params object[] args)
        {
            return Repository.SingleOrDefault<T>(whereStr, args);
        }

        public void Setter_Persists_Ext<T, S, U>(
              Func<T, S> getter,
              Action<T> setter,
              string tableName,
              string fieldName,
              U expected,
              string idFieldName,
              int id,
              bool useSecondGetter = false,
              Func<T, U> getter2 = null,
              U oldValue2 = default(U)
         )
        {
            T testORMClass = (T)Activator.CreateInstance(typeof(T), id);
            S oldValue = getter(testORMClass);
            try
            {
                setter(testORMClass);  // set new value and get it persisted via ORM
                // extract saved value via independent database
                var persisted = Repository.ExecuteScalar<U>(
                    String.Format("SELECT [{0}] FROM [{1}] WHERE [{2}] = @0", fieldName, tableName, idFieldName), id);
                Assert.AreEqual(expected, persisted);
                if (!useSecondGetter)
                    Assert.AreEqual(expected, getter(testORMClass));
                else
                    Assert.AreEqual(expected, getter2(testORMClass));
            }
            finally
            {
                // reset to oldValue
                string sql = String.Format("Update [{0}] set [{1}] = @0 WHERE [{2}] = @1", tableName, fieldName, idFieldName);
                if (!useSecondGetter)
                    Repository.Execute(sql, oldValue, id);
                else
                    Repository.Execute(sql, oldValue2, id);

                // double check
                var persisted2 = Repository.ExecuteScalar<U>(
                    String.Format("SELECT [{0}] FROM [{1}] WHERE [{2}] = @0", fieldName, tableName, idFieldName), id);
                if (!useSecondGetter)
                    Assert.AreEqual(oldValue, persisted2);
                else
                    Assert.AreEqual(oldValue2, persisted2);
            }
        }



        internal class EntityRepository
        {
            protected TestRepositoryAbstractionLayer _tral;
            protected UmbracoDatabase _r { get { return _tral.Repository; } }

            internal EntityRepository(TestRepositoryAbstractionLayer tral)            
            {
                _tral = tral;
            }

            #region 'One-Liners'
            protected void l(string format, params object[] args)
            {
                System.Console.WriteLine(format, args);
            }


            protected string uniqueLabel
            {
                get
                {
                    return string.Format("* {0} *", uniqueValue);
                }
            }
            protected string uniqueNameSuffix
            {
                get
                {
                    return string.Format(" - {0}", uniqueValue);
                }
            }
            protected string uniqueAliasSuffix
            {
                get
                {
                    return uniqueValue;
                }
            }
            protected string uniqueValue
            {
                get
                {
                    return Guid.NewGuid().ToString();
                }
            }

            protected string uniqueRelationName
            {
                get
                {
                    return string.Format("Relation Name {0}", Guid.NewGuid().ToString());
                }
            }

            protected string uniqueRelationType
            {
                get
                {
                    return string.Format("Relation Type {0}", Guid.NewGuid().ToString());
                }
            }


            #endregion


            protected const string TEST_MACRO_PROPERTY_TYPE_ALIAS1 = "testAlias1";
            protected const string TEST_MACRO_PROPERTY_TYPE_ALIAS2 = "testAlias2";

            protected const string TEST_MACRO_PROPERTY_NAME = "Your Web Site";
            protected const string TEST_MACRO_PROPERTY_ALIAS = "yourWebSite";

            protected const int NON_EXISTENT_TEST_ID_VALUE = 12345;
            protected const string TEST_VALUE_VALUE = "test value";
            protected const string TEST_RELATION_TYPE_NAME = "Test Relation Type";
            protected const string TEST_RELATION_TYPE_ALIAS = "testRelationTypeAlias";
            protected int TEST_ITEMS_MAX_COUNT = 7;
            protected const string TEST_GROUP_NAME = "my test group";
            protected const string FOLDER_NODE_GUID_STR = "f38bd2d7-65d0-48e6-95dc-87ce06ec2d3d";
            protected const int FOLDER_NODE_ID = 1031;

        }

        internal class UsersRepository : EntityRepository
        {
            internal UsersRepository(TestRepositoryAbstractionLayer tral) : base(tral) { }

            public User GetUser(int userId)
            {
                return new User(userId);  
            }

        }

        internal class NodesRepository : EntityRepository
        {
            internal NodesRepository(TestRepositoryAbstractionLayer tral):base(tral) {}

            public int CountNodesByObjectTypeGuid(Guid objectType)
            {
                return _r.ExecuteScalar<int>("SELECT COUNT(*) from umbracoNode WHERE nodeObjectType = @objectType", new { objectType });
            }

            public CMSNode MakeNew(
                int parentId,
                int level,
                string text,
                Guid objectType)
            {
                return ORMTestCMSNode.MakeNew(parentId, level, text, objectType);
            }

            internal class ORMTestCMSNode : CMSNode
            {
                public static CMSNode MakeNew(
                    int parentId,
                    int level,
                    string text,
                    Guid objectType)
                {
                    return CMSNode.MakeNew(parentId, objectType, 0, level, text, Guid.NewGuid());
                }
            }

        }

        internal class MacroRepository: EntityRepository
        {
            internal MacroRepository(TestRepositoryAbstractionLayer tral):base(tral) {}

            public int CountAll
            {
                get
                {
                    return _r.ExecuteScalar<int>("select count(id) from cmsMacro");
                }
            }

            public int CountAllProperties(int macroId)
            {
                return _r.ExecuteScalar<int>("select count(id) from cmsMacroProperty where [macro] = @0", macroId);
            }

            public int CountAllPropertyTypes
            {
                get
                {
                    return _r.ExecuteScalar<int>("select count(id) from cmsMacroPropertyType");
                }
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public MacroPropertyDto CreateTestMacroProperty(int macroId, int macroPropertyTypeId, string propertyName = TEST_MACRO_PROPERTY_NAME, string propertyAlias = TEST_MACRO_PROPERTY_ALIAS)
            {
                _r.Execute("insert into [cmsMacroProperty] (macroPropertyHidden, macroPropertyType, macro, macroPropertySortOrder, macroPropertyAlias, macroPropertyName) " +
                                           " values (@macroPropertyHidden, @macroPropertyType, @macro, @macroPropertySortOrder, @macroPropertyAlias, @macroPropertyName) ",
                                           new
                                           {
                                               macroPropertyHidden = true,
                                               macroPropertyType = macroPropertyTypeId,
                                               macro = macroId,
                                               macroPropertySortOrder = 0,
                                               macroPropertyAlias = propertyName + uniqueAliasSuffix,
                                               macroPropertyName = propertyAlias + uniqueNameSuffix
                                           });
                int id = _r.ExecuteScalar<int>("select MAX(id) from [cmsMacroProperty]");
                return _tral.GetDto<MacroPropertyDto>(id);
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public MacroDto CreateMacro(string name, string alias)
            {
                _r.Execute("insert into [cmsMacro] " +
                     " (macroUseInEditor, macroRefreshRate, macroAlias, macroName, macroScriptType, " +
                     " macroScriptAssembly, macroXSLT, macroPython, macroDontRender, macroCacheByPage, macroCachePersonalized) " +
                     " VALUES  " +
                     " (@macroUseInEditor, @macroRefreshRate, @macroAlias, @macroName, @macroScriptType, " +
                     " @macroScriptAssembly, @macroXSLT, @macroPython, @macroDontRender, @macroCacheByPage, @macroCachePersonalized) ",
                     new
                     {
                         macroUseInEditor = true,
                         macroRefreshRate = 0,
                         macroAlias = alias, // "twitterAds",
                         macroName = name, //"Twitter Ads",
                         macroScriptType = "mst",
                         macroScriptAssembly = "",
                         macroXSLT = "some.xslt",
                         macroPython = "",
                         macroDontRender = false,
                         macroCacheByPage = true,
                         macroCachePersonalized = false
                     });
                int id = _r.ExecuteScalar<int>("select MAX(id) from [cmsMacro]");
                return _tral.GetDto<MacroDto>(id);
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public MacroPropertyTypeDto CreateMacroPropertyType(string alias)
            {
                _r.Execute("insert into [cmsMacroPropertyType] (macroPropertyTypeAlias, macroPropertyTypeRenderAssembly, macroPropertyTypeRenderType, macroPropertyTypeBaseType) " +
                                          " values (@macroPropertyTypeAlias, @macroPropertyTypeRenderAssembly, @macroPropertyTypeRenderType, @macroPropertyTypeBaseType) ",
                                          new
                                          {
                                              macroPropertyTypeAlias = alias,
                                              macroPropertyTypeRenderAssembly = "umbraco.macroRenderings",
                                              macroPropertyTypeRenderType = "context",
                                              macroPropertyTypeBaseType = "string"
                                          });
                int id = _r.ExecuteScalar<int>("select MAX(id) from [cmsMacroPropertyType]");
                return _tral.GetDto<MacroPropertyTypeDto>(id);
            }

            public void DeleteMacro(int macroId)
            {
                _r.Execute("delete from  [cmsMacro] where id = @0", macroId);
            }
            public void DeleteMacroProperty(int macroPropertyId)
            {
                _r.Execute("delete from  [cmsMacroProperty] where id = @0", macroPropertyId);
            }
            public void DeleteMacroPropertyType(int macroPropertyTypeId)
            {
                _r.Execute("delete from  [cmsMacroPropertyType] where id = @0", macroPropertyTypeId);
            }



        }

        internal class PreValueRepository: EntityRepository
        {
            internal PreValueRepository(TestRepositoryAbstractionLayer tral):base(tral) {}

            public int CountDataTypeOfId(int dataTypeDefinitionId)
            {
                return _r.ExecuteScalar<int>("select count(pk) from cmsDataType where nodeid = @0", dataTypeDefinitionId);
            }
            public int CountDataTypeNodes(int dataTypeDefinitionId)
            {
                return _r.ExecuteScalar<int>("select count(id) from umbracoNode where id = @0", dataTypeDefinitionId);
            }

            public DataTypeDefinition CreateDataTypeDefinition(User user, string dataTypeText)
            {
                return DataTypeDefinition.MakeNew(user, dataTypeText);
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public List<umbraco.cms.businesslogic.datatype.PreValue.PreValueDto> CreateDataTypePrevalues(int dataTypeDefinitionId, int count)
            {
                var list = new List<umbraco.cms.businesslogic.datatype.PreValue.PreValueDto>();

                var values = new List<string> 
                {
                    "default",
                    ",code,undo,redo,cut,copy,mcepasteword,stylepicker,bold,italic,bullist,numlist,outdent,indent,mcelink,unlink,mceinsertanchor,mceimage,umbracomacro,mceinserttable,umbracoembed,mcecharmap,|1|1,2,3,|0|500,400|1049,|true|",
                    "test"
                };

                if (count > values.Count)
                {
                    for (int i = values.Count+1; i <= count; i++)
                    {
                        values.Add("test," + uniqueValue); 
                    }
                }

                int totalCount = 0;

                if ((int)PreValues.Database.ExecuteScalar<int>("select count(*) from cmsDataTypePreValues where datatypenodeid = @0", dataTypeDefinitionId) == 0)
                {
                    foreach (var x in values)
                    {
                        _r.Execute(
                            "insert into cmsDataTypePreValues (datatypenodeid,[value],sortorder,alias) values (@dtdefid, @value,0,'')",
                            new { dtdefid = dataTypeDefinitionId, value = x });
                        var id = _r.ExecuteScalar<int>("SELECT MAX(id) FROM cmsDataTypePreValues");
                        list.Add(_tral.GetDto<umbraco.cms.businesslogic.datatype.PreValue.PreValueDto>(id));
                        if (++totalCount == count) break;
                    }
                }

                return list;
            }

            public int CountPreValuesByDataTypeId(int dataTypeId)
            {
                return _r.ExecuteScalar<int>("Select count(id) from cmsDataTypePreValues where DataTypeNodeId = @dataTypeId", new { dataTypeId = dataTypeId });
            }

            public void DeletePreValuesByDataTypeDefinitionId(int dataTypeDefinitionId)
            {
                _r.Execute("delete from cmsDataTypePreValues where datatypenodeid = @0", dataTypeDefinitionId);
            }
            public void DeleteDataTypeByDataTypeDefinitionId(int dataTypeDefinitionId)
            {
                _r.Execute("delete from cmsDataType where nodeid = @0", dataTypeDefinitionId);
            }
            public void DeleteNodeByDataTypeDefinitionId(int dataTypeDefinitionId)
            {
                _r.Execute("delete from umbracoNode where id = @0", dataTypeDefinitionId);
            }
        }

        internal class PropertyRepository:EntityRepository
        {
            internal PropertyRepository(TestRepositoryAbstractionLayer tral):base(tral) {}

            public int CountAllPropertyTypes
            {
                get
                {
                    return _r.ExecuteScalar<int>("select count(id) from cmsPropertyType");
                }
            }

            public int CountPropertyTypesByGroupId(int propertyTypeGroupId)
            {
                return _r.ExecuteScalar<int>("SELECT count(id) FROM cmsPropertyType WHERE propertyTypeGroupId = @0", propertyTypeGroupId);
            }

            public int CountPropertyTypesByDataTypeId(int dataTypeId)
            {
                return _r.ExecuteScalar<int>("select count(id) from cmsPropertyType where dataTypeId=@0", dataTypeId);
            }

            public int CountPropertyTypeGroupsByGroupId(int propertyTypeGroupId)
            {
                return _r.ExecuteScalar<int>(@"SELECT count(id) FROM cmsPropertyTypeGroup WHERE parentGroupId = @0", propertyTypeGroupId);
            }

            public int CountPropertyTypeGroupsByContentTypeId(int contentTypeId)
            {
                return _r.ExecuteScalar<int>(@" SELECT count(id) FROM cmsPropertyTypeGroup WHERE contenttypeNodeId = @0", contentTypeId);
            }

            public int CountPropertyTypeGroupsByGroupIdAndContentTypeId(int propertyTypeGroupId, int contentTypeId)
            {
                return _r.ExecuteScalar<int>("SELECT count(id) FROM cmsPropertyType WHERE propertyTypeGroupId = @0 and contentTypeId = @1",
                        propertyTypeGroupId, contentTypeId);
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public ContentTypeDto CreateContentType(int index, int nodeId, string alias)
            {
                _r.Execute("insert into [cmsContentType] " +
                      "([nodeId] ,[alias],[icon],[thumbnail],[description],[isContainer],[allowAtRoot]) values " +
                      "(@nodeId , @alias, @icon, @thumbnail, @description, @isContainer, @allowAtRoot)",
                      new
                      {
                          nodeId = nodeId,
                          alias = alias,
                          icon = string.Format("test{0}.gif", index),
                          thumbnail = string.Format("test{0}.png", index),
                          description = string.Format("test{0}", index),
                          isContainer = false,
                          allowAtRoot = false
                      });
                int id = _r.ExecuteScalar<int>("select MAX(pk) from [cmsContentType]");
                return _tral.GetDto<ContentTypeDto>(id, idKeyName: "pk");
            }


            [MethodImpl(MethodImplOptions.Synchronized)]
            public PropertyTypeGroupDto CreatePropertyTypeGroup(int contentNodeId, int? parentNodeId, string text)
            {
                _r.Execute("insert into [cmsPropertyTypeGroup] " +
                           " ([parentGroupId],[contenttypeNodeId],[text],[sortorder]) VALUES " +
                           " (@parentGroupId,@contenttypeNodeId,@text,@sortorder)",
                           new { parentGroupId = parentNodeId, contenttypeNodeId = contentNodeId, text = text, sortorder = 1 });
                int id = _r.ExecuteScalar<int>("select MAX(id) from [cmsPropertyTypeGroup]");
                return _tral.GetDto<PropertyTypeGroupDto>(id);
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public PropertyTypeDto CreatePropertyType(int dataTypeId, int contentNodeId, int? propertyGroupId, string text)
            {
                _r.Execute("insert into [cmsPropertyType] " +
                           " (dataTypeId, contentTypeId, propertyTypeGroupId, [Alias], [Name], helpText, sortOrder, " +
                           "  mandatory, validationRegExp, [Description]) VALUES " +
                           " (@dataTypeId, @contentTypeId, @propertyTypeGroupId, @alias, @name, @helpText, @sortOrder, @mandatory, @validationRegExp, @description)",
                           new
                           {
                               dataTypeId = dataTypeId,
                               contentTypeId = contentNodeId,
                               propertyTypeGroupId = propertyGroupId,
                               alias = text.Replace(" ", ""),
                               name = "text",
                               helpText = "",
                               sortOrder = 0,
                               mandatory = false,
                               validationRegExp = "",
                               description = ""
                           });
                int id = _r.ExecuteScalar<int>("select MAX(id) from [cmsPropertyType]");
                return _tral.GetDto<PropertyTypeDto>(id);
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public PropertyDataDto CreatePropertyTypeData(int propertyTypeId, int contentNodeId)
            {
                _r.Execute("insert into [cmsPropertyData] " +
                           " (contentNodeId, versionId, propertytypeid, dataInt, dataDate, dataNvarchar, dataNtext) " +
                           "   VALUES " +
                           " (@contentNodeId, @versionId, @propertytypeid, @dataInt, @dataDate, @dataNvarchar, @dataNtext)",
                           new
                           {
                               contentNodeId = contentNodeId,
                               versionId = Guid.NewGuid(),
                               propertytypeid = propertyTypeId,
                               dataInt = (int?)null,
                               dataDate = (DateTime?)null,
                               dataNvarchar = (string)null,
                               dataNtext = (string)null
                           });
                int id = _r.ExecuteScalar<int>("select MAX(id) from [cmsPropertyData]");
                return _tral.GetDto<PropertyDataDto>(id);
            }

            public void DeletePropertyDataById(int propertyDataId)
            {
                _r.Execute("delete from [cmsPropertyData] where id = @0", propertyDataId);
            }

            public void DeletePropertyTypeById(int propertyTypeId)
            {
                _r.Execute("delete from [cmsPropertyType] where id = @0", propertyTypeId);
            }

            public void DeletePropertyTypeGroupById(int propertyTypeGroupId)
            {
                _r.Execute("delete from [cmsPropertyTypeGroup] where id = @0", propertyTypeGroupId);
            }

            public void DeleteContentTypeByPrimaryKey(int contentTypePrimaryKey)
            {
                _r.Execute("delete from [cmsContentType] where pk = @0", contentTypePrimaryKey);
            }

        }

        internal class RecycleBinRepository: EntityRepository
        {
            internal RecycleBinRepository(TestRepositoryAbstractionLayer tral):base(tral) {}

            const string countSQL = @"select count(id) from umbracoNode where nodeObjectType = @nodeObjectType and path like '%,{0},%'";
            public int MediaItemsCount
            {
                get
                {
                    return _r.ExecuteScalar<int>(string.Format(countSQL, Constants.System.RecycleBinMedia), new { nodeObjectType = Media._objectType });
                }
            }

            public int ContentItemsCount
            {
                get
                {
                    return _r.ExecuteScalar<int>(string.Format(countSQL, Constants.System.RecycleBinContent), new { nodeObjectType = Document._objectType });
                }
            }

            const string childSQL = @"SELECT count(id) FROM umbracoNode where parentId = @parentId And nodeObjectType = @nodeObjectType";
            public int ChildrenMediaItemsCount
            {
                get
                {
                    return _r.ExecuteScalar<int>(
                        childSQL, new { parentId = umbraco.cms.businesslogic.RecycleBin.RecycleBinType.Media, nodeObjectType = Media._objectType });
                }
            }

            public int ChildrenContentItemsCount
            {
                get
                {
                    return _r.ExecuteScalar<int>(
                        childSQL, new { parentId = umbraco.cms.businesslogic.RecycleBin.RecycleBinType.Content, nodeObjectType = Document._objectType });
                }
            }
        }

        internal class RelationRepository: EntityRepository
        {
            internal RelationRepository(TestRepositoryAbstractionLayer tral):base(tral) {}
 
            public int CountAllRelationTypes
            {
                get
                {
                    return _r.ExecuteScalar<int>(
                         "SELECT count(id) FROM umbracoRelationType");
                }
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public RelationTypeDto CreateTestRelationType()
            {
                string name = uniqueRelationType; 
                _r.Execute("insert into [umbracoRelationType] ([dual], [parentObjectType], [childObjectType], [name], [alias]) values " +
                                "(@dual, @parentObjectType, @childObjectType, @name, @alias)",
                                new
                                {
                                    dual = 1,
                                    parentObjectType = Guid.NewGuid(),
                                    childObjectType = Guid.NewGuid(),
                                    name =  name,                 
                                    alias = name.Replace(" ", "") 
                                });
                int relationTypeId = _r.ExecuteScalar<int>("select max(id) from [umbracoRelationType]");
                return _tral.GetDto<RelationTypeDto>(relationTypeId);
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public RelationDto CreateTestRelation(int relationTypeId, int parentNodeId, int childNodeId, string comment)
            {
                _r.Execute("insert into [umbracoRelation] (parentId, childId, relType, datetime, comment) values (@parentId, @childId, @relType, @datetime, @comment)",
                                 new { parentId = parentNodeId, childId = childNodeId, relType = relationTypeId, datetime = DateTime.Now, comment = comment });
                int relationId = _r.ExecuteScalar<int>("select max(id) from [umbracoRelation]");
                return _tral.GetDto<RelationDto>(relationId);
            }


            public void DeleteRelationType(int relationTypeId)
            {
                _r.Execute("delete from [umbracoRelationType] where (Id = @0)", relationTypeId);
            }
        }


        internal class TemplateRepository: EntityRepository
        {
            internal TemplateRepository(TestRepositoryAbstractionLayer tral):base(tral) {}

            public int CountTopMostTemplateNodes
            {
                get
                {
                    return _r.ExecuteScalar<int>(
                     "Select count(uniqueID) from umbracoNode where nodeObjectType = @type And parentId = -1",
                          new { type = new Guid(Constants.ObjectTypes.Template) }
                     );
                }
            }

            public TemplateDto GetTemplateNodeByTemplateNodeId(int templateNodeId)
            {
                return _r.SingleOrDefault<TemplateDto>("where nodeId = @0", templateNodeId);
            }

            public int CountChildrenNodesByTemplateId(int templateNodeId)
            {
                return _r.ExecuteScalar<int>("select count(NodeId) as tmp from cmsTemplate where master = " + templateNodeId);
            }

            public void DeleteNodeByDataTypeDefinitionId(int dataTypeDefinitionId)
            {
                _r.Execute("delete from umbracoNode where id = @0", dataTypeDefinitionId);
            }

            public void Delete(TemplateDto dto)
            {
                if (dto != null) _r.Delete<TemplateDto>("where [pk] = @0", dto.PrimaryKey);
            }

            public static string SpaceCamelCasing(string text)
            {
                return text.Length < 2 ? text : text.SplitPascalCasing().ToFirstUpperInvariant();
            }

            private int GetNewDocumentSortOrder(int parentId)
            {
                // I'd let other node types than document get a sort order too, but that'll be a bugfix
                // I'd also let it start on 1, but then again, it didn't.
                var max = _r.SingleOrDefault<int?>(
                    "SELECT MAX(sortOrder) FROM umbracoNode WHERE parentId = @parentId AND nodeObjectType = @ObjectTypeId",
                    new { parentId, ObjectTypeId = Document._objectType }
                    );
                return (max ?? -1) + 1;
            }

            public string UniqueTemplateAlias
            {
                get
                {
                    return string.Format("umbTemplateAlias-{0}", Guid.NewGuid().ToString());
                }
            }
            public string UniqueTemplateName
            {
                get
                {
                    return string.Format("Template Name {0}", Guid.NewGuid().ToString());
                }
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            internal TemplateDto CreateTemplate(int? masterNodeId, string design, int userId, int level, string alias = "")
            {
                if (string.IsNullOrWhiteSpace(alias)) alias = UniqueTemplateAlias;
                Guid ObjectType = new Guid(Constants.ObjectTypes.Template);

                CMSNode parent = null;
                string path = "";
                int sortOrder = 0;

                if (level > 0 && masterNodeId != null)
                {
                    parent = new CMSNode((int)masterNodeId);
                    sortOrder = GetNewDocumentSortOrder((int)masterNodeId);
                    path = parent.Path;
                }
                else
                    path = "-1";

                if (masterNodeId == null) masterNodeId = -1;

                _r.Execute(
                    @"INSERT INTO umbracoNode(
                      trashed, parentID, nodeObjectType, nodeUser, level, 
                      path, sortOrder, uniqueID, text, createDate) 
                  VALUES (
                      @trashed, @parentID, @nodeObjectType, @nodeUser, @level, 
                      @path, @sortOrder, @uniqueID, @text, @createDate)",
                    new
                    {
                        trashed = false,
                        parentID = masterNodeId,
                        nodeObjectType = ObjectType,
                        nodeUser = userId,
                        level,
                        path,
                        sortOrder,
                        uniqueID = Guid.NewGuid(),
                        text = alias,
                        createDate = DateTime.Now
                    });

                int nodeId = _r.ExecuteScalar<int>("Select max(id) from umbracoNode");

                _r.Execute(
                     @"insert into [cmsTemplate] ([nodeId],[master],[alias],[design]) 
                     values (@nodeId, @master, @alias, @design)",
                         new { nodeId = nodeId, master = masterNodeId, design = design, alias = alias });
                int pk = _r.ExecuteScalar<int>("select Max(pk) from cmsTemplate");
                var dto = _tral.GetDto<TemplateDto>(pk, idKeyName: "pk");
                dto.NodeDto = _tral.GetDto<NodeDto>(nodeId);
                return dto;
            }


            #region Templates Text

            internal class TemplatesText
            {
                public static string CommentRSS
                {
                    get
                    {
                        return @"
<%@ Master Language=""C#"" MasterPageFile=""~/umbraco/masterpages/default.master"" AutoEventWireup=""true"" %>
<asp:Content ContentPlaceHolderID=""ContentPlaceHolderDefault"" runat=""server""><umbraco:Macro iscommentfeed=""1"" Alias=""BlogRss"" runat=""server""></umbraco:Macro></asp:Content>";
                    }
                }

                public static string NewsArticle
                {
                    get
                    {
                        return @"
<%@ Master Language=""C#"" MasterPageFile=""~/masterpages/umbMaster.master"" AutoEventWireup=""true"" %>

<asp:Content ContentPlaceHolderID=""cp_content"" runat=""server"">

<div id=""content"" class=""textpage"">
  
      <div id=""contentHeader"">  
          <h2><umbraco:Item runat=""server"" field=""pageName""/></h2>
      </div>
        
      <h4><umbraco:Item field=""introduction"" convertLineBreaks=""true"" runat=""server""></umbraco:Item></h4>
  
      <umbraco:Item runat=""server"" field=""bodyText"" />
</div>


</asp:Content>
";

                    }
                }

                public static string HomePage
                {
                    get
                    {
                        return @"
<%@ Master Language=""C#"" MasterPageFile=""~/masterpages/umbMaster.master"" AutoEventWireup=""true"" %>
    <asp:Content ContentPlaceHolderID=""cp_content"" runat=""server"">
      <div id=""content"" class=""frontPage"">
        <umbraco:Item runat=""server"" field=""bodyText""/>
        

      </div>
    </asp:Content>";
                    }
                }


                public static string Master
                {
                    get
                    {
                        return @"
<%@ Master Language=""C#"" MasterPageFile=""~/umbraco/masterpages/default.master"" AutoEventWireup=""true"" %>
<asp:Content ContentPlaceHolderID=""ContentPlaceHolderDefault"" runat=""server"">

<!DOCTYPE html PUBLIC ""-//W3C//DTD XHTML 1.0 Strict//EN"" ""http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd""[]> 
<html xmlns=""http://www.w3.org/1999/xhtml"">
    <head id=""head"" runat=""server"">
    
  
    <meta http-equiv=""Content-Type"" content=""text/html; charset=iso-8859-1"" />
    <title><asp:placeholder runat=""server""><umbraco:Item runat=""server"" field=""pageName"" /> - <umbraco:Item runat=""server"" field=""siteName"" recursive=""true"" /></asp:placeholder></title>
    
    <link rel=""stylesheet"" type=""text/css"" href=""/css/Starterkit.css"" /> 
  
    <umbraco:Macro Alias=""BlogRssFeedLink"" runat=""server""></umbraco:Macro>
    
    <asp:contentplaceholder id=""cp_head"" runat=""server"" />
</head>
    <body>    
    <div id=""main"">
      
        <asp:contentplaceholder id=""cp_top"" runat=""server"">
        <div id=""top"">
            <h1 id=""siteName""><a href=""/""><umbraco:Item runat=""server"" field=""siteName"" recursive=""true"" /></a></h1>
            <h2 id=""siteDescription""><umbraco:Item runat=""server"" field=""siteDescription"" recursive=""true"" /></h2>
        
            <umbraco:Macro Alias=""umbTopNavigation"" runat=""server"" />
        </div>
        </asp:contentplaceholder>
            
        <div id=""body"" class=""clearfix"">
            <form id=""RunwayMasterForm"" runat=""server"">
            <asp:ContentPlaceHolder ID=""cp_content"" runat=""server""></asp:ContentPlaceHolder>
            </form>
        </div> 
      
        <asp:contentplaceholder id=""cp_footer"" runat=""server"">
        <div id=""footer""></div>
        </asp:contentplaceholder>
    </div>
    </body>
</html> 
</asp:content>";
                    }
                }

                public static string RSS
                {
                    get
                    {
                        return
                            @"<%@ Master Language=""C#"" MasterPageFile=""~/umbraco/masterpages/default.master"" AutoEventWireup=""true"" %>
                            <asp:Content ContentPlaceHolderID=""ContentPlaceHolderDefault"" runat=""server"">
                            <umbraco:Macro Alias=""BlogRss"" runat=""server""></umbraco:Macro>
                            </asp:Content>";
                    }
                }


            }
            #endregion

            //public int CountTasksByMasterNodeId(int nodeId)
            //{
            //    return _r.ExecuteScalar<int>("select count(pk) from cmsTemplate where master = @0", nodeId);
            //}

        }

        internal class TagRepository : EntityRepository
        {
            internal TagRepository(TestRepositoryAbstractionLayer tral):base(tral) {}

            [MethodImpl(MethodImplOptions.Synchronized)]
            public TagDto CreateTestTag(string tag, string group)
            {
                _r.Execute("insert into [cmsTags] ([Tag], [Group]) values (@0, @1)", tag, group);
                int id = _r.ExecuteScalar<int>("select Max(id) from cmsTags");
                return _tral.GetDto<TagDto>(id);
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public TagRelationshipDto CreateTestTagRelationship(int nodeId, int tagId)
            {
                _r.Execute("insert into [cmsTagRelationship] ([nodeId], [tagId]) values (@0, @1)", nodeId, tagId);
                return GetTestTagRelationshipDto(nodeId, tagId);
            }

            public TagRelationshipDto GetTestTagRelationshipDto(int nodeId, int tagId)
            {
                return _r.SingleOrDefault<TagRelationshipDto>(string.Format("where [nodeId] = @0 and [tagId] = @1"), nodeId, tagId);
            }

            public void DeleteTagRelationShip(TagRelationshipDto tagRel)  //delRel
            {
                if (tagRel != null) _r.Execute("delete from [cmsTagRelationship] " +
                                                                          string.Format("where [nodeId] = @0 and [tagId] = @1"), tagRel.NodeId, tagRel.TagId);
            }

            public void Delete(TagDto tag)  // delTag
            {
                if (tag != null) _r.Execute("delete from [cmsTags] " +
                                     string.Format("where [Id] = @0"), tag.Id);
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public int CreateTestTags(int qty = 5, string groupName = TEST_GROUP_NAME, int? nodeId = null)
            {
                for (int i = 1; i <= qty; i++)
                {
                    string tagName = string.Format("Tag #{0}", uniqueLabel);
                    if (nodeId == null)
                        CreateTag(tagName, groupName);
                    else
                        AddTagToNode((int)nodeId, tagName, groupName);
                }
                return qty;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public int AddTagToNode(int nodeId, string tag, string group)
            {
                int id = getTagId(tag, group);
                if (id == 0) id = CreateTag(tag, group);

                //Perform a subselect insert into cmsTagRelationship using a left outer join to perform the if not exists check
                string sql = "insert into cmsTagRelationship (nodeId,tagId) select " + string.Format("{0}", nodeId) + ", " + string.Format("{0}", id) + " from cmsTags ";
                //sorry, gotta do this, @params not supported in join clause
                sql += "left outer join cmsTagRelationship on (cmsTags.id = cmsTagRelationship.TagId and cmsTagRelationship.nodeId = " + string.Format("{0}", nodeId) + ") ";
                sql += "where cmsTagRelationship.tagId is null and cmsTags.id = " + string.Format("{0}", id);

                _r.Execute(sql);

                return id;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public int CreateTag(string tag, string group)  // addTag
            {
                // _r.Insert returns System.Decimal - why?
                return (int)(decimal)_r.Insert(new TagDto() { Tag = tag, Group = group });
            }

            internal class TagDtoExt : TagDto
            {
                public int NodeCount { get; set; }
            }

            private IEnumerable<TagDto> convertSqlToTags(string sql, params object[] param)
            {
                //string testSql = sql;
                //int index = 0;
                //foreach (var p in param)
                //{
                //    testSql = testSql.Replace(string.Format("@{0}", index++), p.ToString());  
                //}

                //l("TEST SQL = '{0}'", testSql);

                foreach (var tagDto in _r.Query<TagDtoExt>(sql, param))
                {
                    yield return tagDto;
                    //new Tag(tagDto.Id, tagDto.Tag, tagDto.Group, tagDto.NodeCount);
                }
            }
            private int convertSqlToTagsCount(string sql, params object[] param)
            {
                return convertSqlToTags(sql, param).ToArray().Length;
            }

            public int CountTags(int? nodeId = null, string groupName = "")
            {
                var groupBy = " GROUP BY cmsTags.id, cmsTags.tag, cmsTags.[group]";
                var sql = @"SELECT cmsTags.id, cmsTags.tag, cmsTags.[group], count(cmsTagRelationShip.tagid) AS nodeCount FROM cmsTags " +
                            "INNER JOIN cmsTagRelationship ON cmsTagRelationShip.tagId = cmsTags.id";
                if (nodeId != null && !string.IsNullOrWhiteSpace(groupName))
                    return convertSqlToTagsCount(sql + " WHERE cmsTags.[group] = @0 AND cmsTagRelationship.nodeid = @1" + groupBy, groupName, nodeId);
                else if (nodeId != null)
                    return convertSqlToTagsCount(sql + " WHERE cmsTagRelationship.nodeid = @0" + groupBy, nodeId);
                else if (!string.IsNullOrWhiteSpace(groupName))
                    return convertSqlToTagsCount(sql + " WHERE cmsTags.[group] = @0" + groupBy, groupName);
                else
                    return convertSqlToTagsCount(sql.Replace("INNER JOIN", "LEFT JOIN") + groupBy);
            }


            private int getTagId(string tag, string group)
            {
                var tagDto = _r.FirstOrDefault<TagDto>("where tag=@0 AND [group]=@1", tag, group);
                if (tagDto == null) return 0;
                return tagDto.Id;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public void AddTagsToNode(int nodeId, string tags, string group)
            {
                string[] allTags = tags.Split(",".ToCharArray());
                for (int i = 0; i < allTags.Length; i++)
                {
                    //if not found we'll get zero and handle that onsave instead...
                    int id = getTagId(allTags[i], group);
                    if (id == 0)
                        id = CreateTag(allTags[i], group);

                    //Perform a subselect insert into cmsTagRelationship using a left outer join to perform the if not exists check
                    string sql = "insert into cmsTagRelationship (nodeId,tagId) select " + string.Format("{0}", nodeId) + ", " + string.Format("{0}", id) + " from cmsTags ";
                    //sorry, gotta do this, @params not supported in join clause
                    sql += "left outer join cmsTagRelationship on (cmsTags.id = cmsTagRelationship.TagId and cmsTagRelationship.nodeId = " + string.Format("{0}", nodeId) + ") ";
                    sql += "where cmsTagRelationship.tagId is null and cmsTags.id = " + string.Format("{0}", id);

                    _r.Execute(sql);
                }
            }

            public int CountDocumentsWithTags(string tags, bool publishedOnly = false)
            {
                int count = 0;
                var docs = new List<DocumentDto>();
                string sql = @"SELECT DISTINCT cmsTagRelationShip.nodeid from cmsTagRelationShip
                            INNER JOIN cmsTags ON cmsTagRelationShip.tagid = cmsTags.id 
                            INNER JOIN umbracoNode ON cmsTagRelationShip.nodeId = umbracoNode.id
                            WHERE (cmsTags.tag IN ({0})) AND nodeObjectType=@nodeType";
                foreach (var id in _r.Query<int>(string.Format(sql, getSqlStringArray(tags)),
                                                       new { nodeType = Document._objectType }))
                {
                    // temp comment - results in runtime error in abridged project
                    //Document cnode = new Document(id);
                    //if (cnode != null && (!publishedOnly || cnode.Published)) 
                    count++;
                }

                return count;
            }

            public int CountNodesWithTags(string tags)
            {
                int count = 0;

                string sql = @"SELECT DISTINCT cmsTagRelationShip.nodeid from cmsTagRelationShip
                            INNER JOIN cmsTags ON cmsTagRelationShip.tagid = cmsTags.id WHERE (cmsTags.tag IN (" + getSqlStringArray(tags) + "))";
                foreach (var id in _r.Query<int>(sql)) count++;
                return count;
            }


            protected string getSqlStringArray(string commaSeparatedArray)
            {
                // create array
                string[] array = commaSeparatedArray.Trim().Split(',');

                // build SQL array
                StringBuilder sqlArray = new StringBuilder();
                foreach (string item in array)
                {
                    string trimmedItem = item.Trim();
                    if (trimmedItem.Length > 0)
                    {
                        sqlArray.Append("'").Append(escapeString(trimmedItem)).Append("',");
                    }
                }

                // remove last comma
                if (sqlArray.Length > 0)
                    sqlArray.Remove(sqlArray.Length - 1, 1);
                return sqlArray.ToString();
            }

            protected string escapeString(string value)
            {
                return string.IsNullOrEmpty(value) ? string.Empty : value.Replace("'", "''");
            }


        }


        internal class TaskRepository : EntityRepository
        {
            internal TaskRepository(TestRepositoryAbstractionLayer tral) : base(tral) { }


            [MethodImpl(MethodImplOptions.Synchronized)]
            internal TaskTypeDto CreateTestTaskType(string alias)
            {
                _r.Execute("insert into [cmsTaskType] ([Alias]) values (@0)", alias);
                int id = _r.ExecuteScalar<int>("select Max(id) from cmsTaskType");
                return _tral.GetDto<TaskTypeDto>(id);
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            internal TaskDto CreateTestTask(TaskTypeDto taskType, CMSNode node, User parentUser, User user, string comment, bool closed = false)
            {
                _r.Execute(
                      @"insert into [cmsTask] ([closed], taskTypeId, nodeId, parentUserId, userId, [DateTime], [Comment]) 
                   values (@closed, @taskTypeId, @nodeId, @parentUserId, @userId, @dateTime, @comment)",
                      new
                      {
                          closed = closed,
                          taskTypeId = taskType.Id,
                          nodeId = node.Id,
                          parentUserId = parentUser.Id,
                          userId = user.Id,
                          dateTime = DateTime.Now,
                          comment = comment
                      });
                int id = _r.ExecuteScalar<int>("select Max(id) from cmsTask");
                return _tral.GetDto<TaskDto>(id);
            }

            public string NewTaskTypeAlias
            {
                get
                {
                    return string.Format("Test TaskType, GUID = {0}", Guid.NewGuid());
                }
            }


            [MethodImpl(MethodImplOptions.Synchronized)]
            private TaskDto CreateTestTask(TaskTypeDto taskType, CMSNode node, UserDto parentUser, UserDto user, string comment)
            {
                _r.Execute(
                      @"insert into [cmsTask] ([closed], taskTypeId, nodeId, parentUserId, userId, [DateTime], [Comment]) 
                               values (@closed, @taskTypeId, @nodeId, @parentUserId, @userId, @dateTime, @comment)",
                      new
                      {
                          closed = false,
                          taskTypeId = taskType.Id,
                          nodeId = node.Id,
                          parentUserId = parentUser.Id,
                          userId = user.Id,
                          dateTime = DateTime.Now,
                          comment = comment
                      });
                int id = _r.ExecuteScalar<int>("select Max(id) from cmsTask");
                return _tral.GetDto<TaskDto>(id);
            }

            internal int CountTasksByTaskType(int taskTypeId)
            {
                return _r.ExecuteScalar<int>("select count(id) from cmsTask where taskTypeId = @0", taskTypeId);
            }

            internal int CountTasksByNodeId(int id)
            {
                return _r.ExecuteScalar<int>("select count(id) from cmsTask where nodeId = @0", id);
            }

            internal int CountTasksByUserAndIncludeClosedFlag(int userId, bool includeClosed)
            {
                string sql = "select count(id) from  cmsTask  where userId = @0";
                if (!includeClosed)
                    sql += " and closed = 0";
                return _r.ExecuteScalar<int>(sql, userId);
            }

            internal int CountOwnedTasksByUserAndIncludeClosedFlag(int userId, bool includeClosed)
            {
                string sql = "select count(id) from  cmsTask  where parentUserId = @0";
                if (!includeClosed)
                    sql += " and closed = 0";
                return _r.ExecuteScalar<int>(sql, userId);
            }

            internal void DeleteTaskType(TaskTypeDto dto)
            {
                if (dto != null) _r.Delete<TaskTypeDto>("where [Id] = @0", dto.Id);
            }
            internal void DeleteTask(TaskDto dto)
            {
                if (dto != null) _r.Delete<TaskDto>("where [Id] = @0", dto.Id);
            }

            internal int GetAllTaskTypesCount()
            {
                return _r.ExecuteScalar<int>("select count(*) from cmsTaskType");
            }

            internal int GetTaskTypesTasks(int taskTypeId)
            {
                return _r.ExecuteScalar<int>("select count(*) from cmsTask where TaskTypeId = @0", taskTypeId);
            }


        }

    }

    //public abstract partial class BaseORMTest : BaseDatabaseFactoryTest
    //{

    //}
}
