using System;
using NUnit.Framework;
using Umbraco.Core.Models;
using Umbraco.Tests.TestHelpers;

namespace Umbraco.Tests.Issues
{
    [TestFixture]
    [DatabaseTestBehavior(DatabaseBehavior.NewDbFileAndSchemaPerTest)]
    public class U9560 : BaseDatabaseFactoryTest
    {
        [Test]
        public void Test()
        {
            // create a content type and some properties
            var contentType = new ContentType(-1);
            contentType.Alias = "test";
            contentType.Name = "test";
            var propertyType = new PropertyType("test", DataTypeDatabaseType.Ntext, "prop") { Name = "Prop", Description = "", Mandatory = false, SortOrder = 1, DataTypeDefinitionId = -88 };
            contentType.PropertyTypeCollection.Add(propertyType);
            ServiceContext.ContentTypeService.Save(contentType);

            var aliasName = string.Empty;

            // read fields, same as what we do with PetaPoco Fetch<dynamic>
            var db = DatabaseContext.Database;
            db.OpenSharedConnection();
            try
            {
                var conn = db.Connection;
                var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT mandatory, dataTypeId, propertyTypeGroupId, contentTypeId, sortOrder, alias, name, validationRegExp, description from cmsPropertyType where id=" + propertyType.Id;
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        for (var i = 0; i < reader.FieldCount; i++)
                            Console.WriteLine(reader.GetName(i));
                        aliasName = reader.GetName(5);
                    }
                }
            }
            finally
            {
                db.CloseSharedConnection();
            }

            // note that although the query is for 'alias' the field is named 'Alias'
            Assert.AreEqual("Alias", aliasName);

            // try differently
            db.OpenSharedConnection();
            try
            {
                var conn = db.Connection;
                var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT mandatory, dataTypeId, propertyTypeGroupId, contentTypeId, sortOrder, alias as alias, name, validationRegExp, description from cmsPropertyType where id=" + propertyType.Id;
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        for (var i = 0; i < reader.FieldCount; i++)
                            Console.WriteLine(reader.GetName(i));
                        aliasName = reader.GetName(5);
                    }
                }
            }
            finally
            {
                db.CloseSharedConnection();
            }

            // and now it is OK
            Assert.AreEqual("alias", aliasName);

            // get the legacy content type
            var legacyContentType = new umbraco.cms.businesslogic.ContentType(contentType.Id);
            Assert.AreEqual("test", legacyContentType.Alias);

            // get the legacy properties
            var legacyProperties = legacyContentType.PropertyTypes;

            // without the fix, due to some (swallowed) inner exception, we have no properties
            //Assert.IsNull(legacyProperties);

            // thanks to the fix, it works
            Assert.IsNotNull(legacyProperties);
            Assert.AreEqual(1, legacyProperties.Count);
            Assert.AreEqual("prop", legacyProperties[0].Alias);
        }
    }
}
