using umbraco.Linq.DTMetal.Engine;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TypeMock.ArrangeActAssert;
using umbraco.DataLayer;
using System;
using TypeMock;
using System.Linq;
using umbraco.Linq.DTMetal.CodeBuilder;
namespace umbraco.Linq.DTMetal.Engine.Tests
{


    /// <summary>
    ///This is a test class for DocTypeObjectBuilderTest and is intended
    ///to contain all CodeCreatorTest Unit Tests
    ///</summary>
    [TestClass()]
    public class DocTypeObjectBuilderTest
    {
        [TestMethod]
        [Isolated]
        public void LoadDocTypesTest_DbCalled()
        {
            ISqlHelper sql = Isolate.Fake.Instance<ISqlHelper>(Members.ReturnRecursiveFakes);

            DocTypeObjectBuilder target = new DocTypeObjectBuilder(string.Empty);
            Isolate.WhenCalled(() => target.SqlHelper).WillReturn(sql);
            target.LoadDocTypes();
            Isolate.Verify.WasCalledWithAnyArguments(() => target.SqlHelper);
            Isolate.Verify.WasCalledWithAnyArguments(() => sql.ExecuteReader(string.Empty));
        }

        [TestMethod]
        [Isolated]
        public void LoadDocTypesTest_ReaderAccessed()
        {
            var sql = Isolate.Fake.Instance<ISqlHelper>();
            var reader = Isolate.Fake.Instance<IRecordsReader>();

            Isolate.WhenCalled(() => reader.HasRecords).WillReturn(true);
            Isolate.WhenCalled(() => reader.Read()).WillReturn(false);
            Isolate.WhenCalled(() => sql.ExecuteReader(string.Empty)).WillReturn(reader);

            DocTypeObjectBuilder target = new DocTypeObjectBuilder(string.Empty);
            Isolate.WhenCalled(() => target.SqlHelper).WillReturn(sql);
            target.LoadDocTypes();

            Isolate.Verify.WasCalledWithAnyArguments(() => reader.HasRecords);
            Isolate.Verify.WasCalledWithExactArguments(() => reader.Read());
        }

        [TestMethod()]
        [Isolated]
        public void LoadDocTypesTest_CreateDocType()
        {
            ISqlHelper fakeSql = Isolate.Fake.Instance<ISqlHelper>(Members.CallOriginal);

            IRecordsReader fakeReader = Isolate.Fake.Instance<IRecordsReader>();

            var dtGuid = Guid.NewGuid();

            //get the data for the DT
            Isolate.WhenCalled(() => fakeReader.HasRecords).WillReturn(true);
            Isolate.WhenCalled(() => fakeReader.Read()).WillReturn(true);
            Isolate.WhenCalled(() => fakeReader.Read()).CallOriginal();
            Isolate.WhenCalled(() => fakeReader.GetId()).WillReturn(1);
            Isolate.WhenCalled(() => fakeReader.GetName()).WillReturn("Name");
            Isolate.WhenCalled(() => fakeReader.GetDescription()).WillReturn("Description");
            Isolate.WhenCalled(() => fakeReader.GetAlias()).WillReturn("Alias");
            Isolate.WhenCalled(() => fakeReader.GetParentId()).WillReturn(-1);
            Isolate.WhenCalled(() => fakeSql.ExecuteReader(string.Empty)).WillReturn(fakeReader);

            DocTypeObjectBuilder target = new DocTypeObjectBuilder(string.Empty);
            Isolate.WhenCalled(() => target.SqlHelper).WillReturn(fakeSql);
            target.LoadDocTypes();

            Assert.IsNotNull(target.DocumentTypes);
            Assert.AreEqual(target.DocumentTypes.Count, 1);

            Isolate.Verify.WasCalledWithExactArguments(() => target.BuildDocumentType(fakeReader));
            Isolate.Verify.WasCalledWithExactArguments(() => fakeReader.GetAlias());
            Isolate.Verify.WasCalledWithExactArguments(() => fakeReader.GetParentId());
            Isolate.Verify.WasCalledWithExactArguments(() => fakeReader.GetDescription());
            Isolate.Verify.WasCalledWithExactArguments(() => fakeReader.GetName());
            Isolate.Verify.WasCalledWithExactArguments(() => fakeReader.GetId());

            var dt = target.DocumentTypes[0];
            Assert.AreEqual("Alias", dt.Alias);
            Assert.AreEqual("Description", dt.Description);
            Assert.AreEqual(1, dt.Id);
            Assert.AreEqual("Name", dt.Name);
            Assert.AreEqual(-1, dt.ParentId);
            Assert.AreEqual(0, dt.Properties.Count);
        }

        [TestMethod]
        [Isolated]
        public void LoadDocTypesTest_IntPropertyMapping()
        {
            DocTypeObjectBuilder target = new DocTypeObjectBuilder(string.Empty);

            var intReader = Isolate.Fake.Instance<IRecordsReader>();
            Isolate.WhenCalled(() => intReader.GetDbType()).WillReturn("Integer");
            var intProp = target.BuildProperty(intReader);
            Assert.AreEqual(typeof(int), intProp.DatabaseType);
        }

        [TestMethod, Isolated]
        public void LoadDocTypesTest_NtextPropertyMapping()
        {
            DocTypeObjectBuilder target = new DocTypeObjectBuilder(string.Empty);
            var ntextReader = Isolate.Fake.Instance<IRecordsReader>();
            Isolate.WhenCalled(() => ntextReader.GetDbType()).WillReturn("Ntext");
            var ntextProp = target.BuildProperty(ntextReader);
            Assert.AreEqual(typeof(string), ntextProp.DatabaseType);
        }

        [TestMethod, Isolated]
        public void LoadDocTypesTest_NvarcharPropertyMapping()
        {
            DocTypeObjectBuilder target = new DocTypeObjectBuilder(string.Empty);
            var varcharReader = Isolate.Fake.Instance<IRecordsReader>();
            Isolate.WhenCalled(() => varcharReader.GetDbType()).WillReturn("Nvarchar");
            var varcharProp = target.BuildProperty(varcharReader);
            Assert.AreEqual(typeof(string), varcharProp.DatabaseType);
        }

        [TestMethod, Isolated]
        public void LoadDocTypesTest_DatePropertyMapping()
        {
            DocTypeObjectBuilder target = new DocTypeObjectBuilder(string.Empty);
            var dateReader = Isolate.Fake.Instance<IRecordsReader>();
            Isolate.WhenCalled(() => dateReader.GetDbType()).WillReturn("Date");
            var dateProp = target.BuildProperty(dateReader);
            Assert.AreEqual(typeof(DateTime), dateProp.DatabaseType);
        }

        [TestMethod, Isolated]
        public void LoadDocTypesTest_ObjectPropertyMapping()
        {
            DocTypeObjectBuilder target = new DocTypeObjectBuilder(string.Empty);
            var objReader = Isolate.Fake.Instance<IRecordsReader>();
            Isolate.WhenCalled(() => objReader.GetDbType()).WillReturn("Something undefined");
            var objProp = target.BuildProperty(objReader);
            Assert.AreEqual(typeof(object), objProp.DatabaseType);
        }

        [TestMethod]
        [Isolated]
        public void LoadDocTypesTest_DocTypeProperties()
        {
            ISqlHelper fakeSql = Isolate.Fake.Instance<ISqlHelper>(Members.ReturnRecursiveFakes);

            IRecordsReader fakeReader = Isolate.Fake.Instance<IRecordsReader>();

            var dtGuid = Guid.NewGuid();

            //get the data for the DT
            Isolate.WhenCalled(() => fakeReader.HasRecords).WillReturn(true);
            Isolate.WhenCalled(() => fakeReader.Read()).WillReturn(true);
            Isolate.WhenCalled(() => fakeSql.ExecuteReader(string.Empty)).WillReturn(fakeReader);
            Isolate.WhenCalled(() => fakeReader.GetId()).WillReturn(0);
            Isolate.WhenCalled(() => fakeReader.Read()).WillReturnRepeat(true, 4).AndThen().CallOriginal();
            Isolate.WhenCalled(() => fakeReader.GetParentId()).WillReturn(-1);

            Isolate.WhenCalled(() => fakeReader.GetAlias()).WillReturn("Property1 Alias");
            Isolate.WhenCalled(() => fakeReader.GetString("RegularExpression")).WillReturn(string.Empty);
            Isolate.WhenCalled(() => fakeReader.GetDbType()).WillReturn("Integer");
            Isolate.WhenCalled(() => fakeReader.GetDescription()).WillReturn("Property1 Description");
            Isolate.WhenCalled(() => fakeReader.GetId()).WillReturn(1);
            Isolate.WhenCalled(() => fakeReader.GetName()).WillReturn("Property1");

            Isolate.WhenCalled(() => fakeReader.GetString("RegularExpression")).WillReturn(string.Empty);
            Isolate.WhenCalled(() => fakeReader.GetDbType()).WillReturn("Ntext");

            Isolate.WhenCalled(() => fakeReader.GetString("RegularExpression")).WillReturn(string.Empty);
            Isolate.WhenCalled(() => fakeReader.GetDbType()).WillReturn("Nvarchar");

            Isolate.WhenCalled(() => fakeReader.GetString("RegularExpression")).WillReturn(string.Empty);
            Isolate.WhenCalled(() => fakeReader.GetDbType()).WillReturn("Date");

            DocTypeObjectBuilder target = new DocTypeObjectBuilder(string.Empty);
            Isolate.WhenCalled(() => target.SqlHelper).WillReturn(fakeSql);
            target.LoadDocTypes();

            Isolate.Verify.WasCalledWithExactArguments(() => target.GetProperties(0));
            Isolate.Verify.WasCalledWithExactArguments(() => target.BuildProperty(fakeReader));

            Isolate.Verify.WasCalledWithAnyArguments(() => fakeSql.ExecuteReader(string.Empty));
            Isolate.Verify.WasCalledWithExactArguments(() => fakeReader.GetAlias());
            Isolate.Verify.WasCalledWithExactArguments(() => fakeReader.GetDbType());
            Isolate.Verify.WasCalledWithExactArguments(() => fakeReader.GetDescription());
            Isolate.Verify.WasCalledWithExactArguments(() => fakeReader.GetName());
            Isolate.Verify.WasCalledWithExactArguments(() => fakeReader.GetString("RegularExpression"));
            Isolate.Verify.WasCalledWithExactArguments(() => fakeReader.GetId());

            var dt = target.DocumentTypes[0];

            Assert.IsNotNull(dt.Properties);
            Assert.AreEqual(4, dt.Properties.Count);

            Assert.AreEqual("Property1 Alias", dt.Properties[0].Alias);
            Assert.AreEqual(typeof(int), dt.Properties[0].DatabaseType);
            Assert.AreEqual("Property1 Description", dt.Properties[0].Description);
            Assert.AreEqual(1, dt.Properties[0].Id);
            Assert.AreEqual(false, dt.Properties[0].Mandatory);
            Assert.AreEqual("Property1", dt.Properties[0].Name);
            Assert.AreEqual(string.Empty, dt.Properties[0].RegularExpression);
            Assert.AreEqual(typeof(string), dt.Properties[1].DatabaseType);
            Assert.AreEqual(typeof(string), dt.Properties[2].DatabaseType);
            Assert.AreEqual(typeof(DateTime), dt.Properties[3].DatabaseType);

        }

        [TestMethod]
        [Isolated]
        public void LoadDocTypesTest_ParentIncluded()
        {
            ISqlHelper fakeSql = Isolate.Fake.Instance<ISqlHelper>(Members.CallOriginal);

            IRecordsReader fakeReader = Isolate.Fake.Instance<IRecordsReader>();

            Isolate.WhenCalled(() => fakeReader.HasRecords).WillReturn(true);
            Isolate.WhenCalled(() => fakeReader.Read()).WillReturnRepeat(true, 2).AndThen().CallOriginal();

            Isolate.WhenCalled(() => fakeReader.GetId()).WillReturn(1);
            Isolate.WhenCalled(() => fakeReader.GetParentId()).WillReturn(-1);
            Isolate.WhenCalled(() => fakeReader.GetId()).WillReturn(2);
            Isolate.WhenCalled(() => fakeReader.GetParentId()).WillReturn(1);

            Isolate.WhenCalled(() => fakeSql.ExecuteReader(string.Empty)).WillReturn(fakeReader);

            DocTypeObjectBuilder target = new DocTypeObjectBuilder(string.Empty);
            Isolate.WhenCalled(() => target.SqlHelper).WillReturn(fakeSql);
            Isolate.WhenCalled(() => target.GetProperties(0)).WillReturn(new System.Collections.Generic.List<DocTypeProperty>());
            Isolate.WhenCalled(() => target.BuildAssociations(0)).WillReturn(new System.Collections.Generic.List<DocTypeAssociation>());

            target.LoadDocTypes();

            Assert.AreEqual(2, target.DocumentTypes.Count);
            Assert.AreEqual(target.DocumentTypes[0].Id, target.DocumentTypes[1].ParentId);
        }

        [TestMethod]
        [Isolated]
        public void LoadDocTypesTest_ParentNotIncluded()
        {
            ISqlHelper fakeSql = Isolate.Fake.Instance<ISqlHelper>(Members.CallOriginal);

            IRecordsReader reader = Isolate.Fake.Instance<IRecordsReader>();

            Isolate.WhenCalled(() => reader.HasRecords).WillReturnRepeat(true, 2).AndThen().CallOriginal();
            Isolate.WhenCalled(() => reader.Read()).WillReturnRepeat(true, 2).AndThen().CallOriginal();
            Isolate.WhenCalled(() => reader.GetId()).WillReturn(2);
            Isolate.WhenCalled(() => reader.GetParentId()).WillReturn(1);

            Isolate.WhenCalled(() => reader.GetId()).WillReturn(1);
            Isolate.WhenCalled(() => reader.GetParentId()).WillReturn(-1);

            Isolate.WhenCalled(() => fakeSql.ExecuteReader(string.Empty)).WillReturn(reader);

            DocTypeObjectBuilder target = new DocTypeObjectBuilder(string.Empty);
            Isolate.WhenCalled(() => target.SqlHelper).WillReturn(fakeSql);
            Isolate.WhenCalled(() => target.GetProperties(0)).WillReturn(new System.Collections.Generic.List<DocTypeProperty>());
            Isolate.WhenCalled(() => target.BuildAssociations(0)).WillReturn(new System.Collections.Generic.List<DocTypeAssociation>());

            target.LoadDocTypes();

            Assert.AreEqual(2, target.DocumentTypes.Count);
            Assert.AreEqual(target.DocumentTypes[0].Id, target.DocumentTypes[1].ParentId);

            Isolate.Verify.WasCalledWithExactArguments(() => target.LoadParentDocType(1));
        }

        [TestMethod]
        [Isolated]
        [ExpectedException(typeof(IndexOutOfRangeException))]
        public void LoadDocTypesTest_ParentNotExists()
        {
            ISqlHelper fakeSql = Isolate.Fake.Instance<ISqlHelper>(Members.CallOriginal);

            IRecordsReader fakeReader = Isolate.Fake.Instance<IRecordsReader>();

            Isolate.WhenCalled(() => fakeReader.HasRecords).WillReturn(true);
            Isolate.WhenCalled(() => fakeReader.HasRecords).CallOriginal();
            Isolate.WhenCalled(() => fakeReader.GetParentId()).WillReturn(1);
            Isolate.WhenCalled(() => fakeReader.Read()).WillReturn(true);
            Isolate.WhenCalled(() => fakeReader.Read()).CallOriginal();

            Isolate.WhenCalled(() => fakeSql.ExecuteReader(string.Empty)).WillReturn(fakeReader);

            DocTypeObjectBuilder target = new DocTypeObjectBuilder(string.Empty);
            Isolate.WhenCalled(() => target.SqlHelper).WillReturn(fakeSql);

            target.LoadDocTypes();

            Isolate.Verify.WasCalledWithAnyArguments(() => target.LoadParentDocType(1));
        }

        [TestMethod]
        [Isolated]
        public void LoadDocTypesTest_GenerateAssociations()
        {
            var sql = Isolate.Fake.Instance<ISqlHelper>();
            var reader = Isolate.Fake.Instance<IRecordsReader>();

            DocTypeObjectBuilder target = new DocTypeObjectBuilder(string.Empty);
            Isolate.WhenCalled(() => target.SqlHelper).WillReturn(sql);
            Isolate.WhenCalled(() => target.GetProperties(0)).WillReturn(new System.Collections.Generic.List<DocTypeProperty>());

            Isolate.WhenCalled(() => sql.ExecuteReader(string.Empty)).WillReturn(reader);

            Isolate.WhenCalled(() => reader.HasRecords).WillReturn(true);
            Isolate.WhenCalled(() => reader.Read()).WillReturnRepeat(true, 2);
            Isolate.WhenCalled(() => reader.Read()).WillReturn(false);

            Isolate.WhenCalled(() => reader.GetInt("AllowedId")).WillReturn(1);

            target.LoadDocTypes();

            Isolate.Verify.WasCalledWithExactArguments(() => target.BuildAssociations(1));

            Assert.AreEqual(1, target.DocumentTypes.Count);
            Assert.AreEqual(1, target.DocumentTypes[0].Associations.Count);
            Assert.AreEqual(1, target.DocumentTypes[0].Associations[0].AllowedId);
        }

        [TestMethod]
        [Isolated]
        public void LoadDocTypesTest_RecursiveParent()
        {
            ISqlHelper fakeSql = Isolate.Fake.Instance<ISqlHelper>(Members.CallOriginal);

            IRecordsReader reader = Isolate.Fake.Instance<IRecordsReader>();

            Isolate.WhenCalled(() => reader.HasRecords).WillReturnRepeat(true, 3).AndThen().CallOriginal();
            Isolate.WhenCalled(() => reader.Read()).WillReturnRepeat(true, 3).AndThen().CallOriginal();
            Isolate.WhenCalled(() => reader.GetId()).WillReturn(2);
            Isolate.WhenCalled(() => reader.GetParentId()).WillReturn(1);

            Isolate.WhenCalled(() => reader.GetId()).WillReturn(1);
            Isolate.WhenCalled(() => reader.GetParentId()).WillReturn(3);

            Isolate.WhenCalled(() => reader.GetId()).WillReturn(3);
            Isolate.WhenCalled(() => reader.GetParentId()).WillReturn(-1);

            Isolate.WhenCalled(() => fakeSql.ExecuteReader(string.Empty)).WillReturn(reader);

            DocTypeObjectBuilder target = new DocTypeObjectBuilder(string.Empty);
            Isolate.WhenCalled(() => target.SqlHelper).WillReturn(fakeSql);
            Isolate.WhenCalled(() => target.GetProperties(0)).WillReturn(new System.Collections.Generic.List<DocTypeProperty>());
            Isolate.WhenCalled(() => target.BuildAssociations(0)).WillReturn(new System.Collections.Generic.List<DocTypeAssociation>());

            target.LoadDocTypes();

            Assert.AreEqual(3, target.DocumentTypes.Count);
            Assert.AreEqual(target.DocumentTypes[0].Id, target.DocumentTypes[1].ParentId);

            Isolate.Verify.WasCalledWithExactArguments(() => target.LoadParentDocType(3));
        }
    }
}
