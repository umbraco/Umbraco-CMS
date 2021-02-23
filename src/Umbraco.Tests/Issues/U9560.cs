using System;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Tests.TestHelpers;

namespace Umbraco.Tests.Issues
{
    [TestFixture]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest, WithApplication = true)]
    public class U9560 : TestWithDatabaseBase
    {
        [Test]
        public void Test()
        {
            // create a content type and some properties
            var contentType = new ContentType(ShortStringHelper, -1);
            contentType.Alias = "test";
            contentType.Name = "test";
            var propertyType = new PropertyType(ShortStringHelper, "test", ValueStorageType.Ntext, "prop") { Name = "Prop", Description = "", Mandatory = false, SortOrder = 1, DataTypeId = -88 };
            contentType.PropertyTypeCollection.Add(propertyType);
            ServiceContext.ContentTypeService.Save(contentType);

            var aliasName = string.Empty;

            // read fields, same as what we do with PetaPoco Fetch<dynamic>
            using (var db = Factory.GetRequiredService<IUmbracoDatabaseFactory>().CreateDatabase())
            {
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
            }

            // note that although the query is for 'alias' the field is named 'Alias'
            Assert.AreEqual("Alias", aliasName);

            // try differently
            using (var db = Factory.GetRequiredService<IUmbracoDatabaseFactory>().CreateDatabase())
            {
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
            }

            // and now it is OK
            Assert.AreEqual("alias", aliasName);

            //// get the legacy content type
            //var legacyContentType = new umbraco.cms.businesslogic.ContentType(contentType.Id);
            //Assert.AreEqual("test", legacyContentType.Alias);

            //// get the legacy properties
            //var legacyProperties = legacyContentType.PropertyTypes;

            //// without the fix, due to some (swallowed) inner exception, we have no properties
            ////Assert.IsNull(legacyProperties);

            //// thanks to the fix, it works
            //Assert.IsNotNull(legacyProperties);
            //Assert.AreEqual(1, legacyProperties.Count);
            //Assert.AreEqual("prop", legacyProperties[0].Alias);
        }
    }
}
