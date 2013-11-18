using System.Diagnostics;
using System.Reflection;
using System;
using System.Linq;
using Umbraco.Web;
using Umbraco.Core.Persistence;
using umbraco.cms.businesslogic;
using System.Xml;
using umbraco.cms.businesslogic.web;
using NUnit.Framework;
using System.Text;
using Umbraco.Core.Models.Rdbms;
using System.Runtime.CompilerServices;
using umbraco.cms.businesslogic.packager;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic.datatype;
using System.Collections.Generic;
using Umbraco.Core;
using umbraco.cms.businesslogic.media;
using umbraco.cms.businesslogic.relation;

namespace Umbraco.Tests.TestHelpers
{
    public partial class TestRepositoryAbstractionLayer
    {
        private UmbracoContext _context;
        private UmbracoDatabase _database;

        internal NodesRepository Nodes { get; private set; }
        internal MacroRepository Macro { get; private set; }
        internal PreValueRepository PreValue { get; private set; }
        internal PropertyRepository Property { get; private set; }
        internal RecycleBinRepository RecycleBin { get; private set; }
        internal RelationRepository Relation { get; private set; }
        internal TemplateRepository Template { get; private set; }

        public UmbracoDatabase Repository { get; private set; }

        public TestRepositoryAbstractionLayer(UmbracoContext context)
        {
            _context = context;
            _database = context.Application.DatabaseContext.Database;

            Repository = (new DefaultDatabaseFactory()).CreateDatabase();
            this.Nodes = new NodesRepository(this.Repository);
            this.Macro = new MacroRepository(this.Repository);
            this.PreValue = new PreValueRepository(this.Repository);
            this.Property = new PropertyRepository(this.Repository);
            this.RecycleBin = new RecycleBinRepository(this.Repository);
            this.Relation = new RelationRepository(this.Repository);
            this.Template = new TemplateRepository(this.Repository);
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


        internal class NodesRepository
        {
            UmbracoDatabase _r;
            internal NodesRepository(UmbracoDatabase Repository)
            {
                _r = Repository;
            }

            public int CountNodesByObjectTypeGuid(Guid objectType)
            {
                return _r.ExecuteScalar<int>("SELECT COUNT(*) from umbracoNode WHERE nodeObjectType = @objectType", new { objectType });
            }

        }

        internal class MacroRepository
        {
            UmbracoDatabase _r;
            internal MacroRepository(UmbracoDatabase Repository)
            {
                _r = Repository;
            }
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
        }

        internal class PreValueRepository
        {
            UmbracoDatabase _r;
            internal PreValueRepository(UmbracoDatabase Repository)
            {
                _r = Repository;
            }

            public int CountDataTypeOfId(int dataTypeDefinitionId)
            {
                return _r.ExecuteScalar<int>("select count(pk) from cmsDataType where nodeid = @0", dataTypeDefinitionId);
            }
            public int CountDataTypeNodes(int dataTypeDefinitionId)
            {
                return _r.ExecuteScalar<int>("select count(id) from umbracoNode where id = @0", dataTypeDefinitionId);
            }

        }

        internal class PropertyRepository
        {
            UmbracoDatabase _r;
            internal PropertyRepository(UmbracoDatabase Repository)
            {
                _r = Repository;
            }

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

        }

        internal class RecycleBinRepository
        {
            UmbracoDatabase _r;
            internal RecycleBinRepository(UmbracoDatabase Repository)
            {
                _r = Repository;
            }

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

        internal class RelationRepository
        {
            UmbracoDatabase _r;
            internal RelationRepository(UmbracoDatabase Repository)
            {
                _r = Repository;
            }

            public int CountAllRelationTypes
            {
                get
                {
                    return _r.ExecuteScalar<int>(
                         "SELECT count(id) FROM umbracoRelationType");
                }
            }
        }


        internal class TemplateRepository
        {
            UmbracoDatabase _r;
            internal TemplateRepository(UmbracoDatabase Repository)
            {
                _r = Repository;
            }

            public int CountTopMostTemplateNodes
            {
                get
                {
                    //var topMostNodes = independentDatabase.Fetch<Guid>(
                    //        "Select uniqueID from umbracoNode where nodeObjectType = @type And parentId = -1 order by sortOrder",
                    //        new { type = new Guid(Constants.ObjectTypes.Template) }
                    //        ); 
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


        }


    }

    public abstract partial class BaseORMTest : BaseDatabaseFactoryTest
    {

    }
}
