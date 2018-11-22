//using System;
//using System.Linq;
//using NUnit.Framework;
//using Umbraco.Core.Models;
//using Umbraco.Core.Persistence;
//using Umbraco.Core.Persistence.SqlSyntax;
//using Umbraco.Tests.TestHelpers;
//using Umbraco.Tests.TestHelpers.Entities;

//namespace Umbraco.Tests.Persistence
//{
//    [DatabaseTestBehavior(DatabaseBehavior.NewDbFileAndSchemaPerTest)]
//    [TestFixture]
//    public class PetaPocoDynamicQueryTests : BaseDatabaseFactoryTest
//    {
//        [Test]
//        public void Check_Poco_Storage_Growth()
//        {
//            //CreateStuff();

//            for (int i = 0; i < 1000; i++)
//            {
//                DatabaseContext.Database.Fetch<dynamic>(
//                    "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME='" + i + "'");
//            }

//            //var oc11 = Database.PocoData.GetCachedPocoData().Count();
//            //var oc12 = Database.PocoData.GetConverters().Count();
//            //var oc13 = Database.GetAutoMappers().Count();
//            //var oc14 = Database.GetMultiPocoFactories().Count();

//            //for (int i = 0; i < 2; i++)
//            //{
//            //    i1 = DatabaseContext.Database.Fetch<dynamic>("SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES");
//            //    r1 = i1.Select(x => x.TABLE_NAME).Cast<string>().ToList();    
//            //}

//            //var oc21 = Database.PocoData.GetCachedPocoData().Count();
//            //var oc22 = Database.PocoData.GetConverters().Count();
//            //var oc23 = Database.GetAutoMappers().Count();
//            //var oc24 = Database.GetMultiPocoFactories().Count();

//            //var roots = ServiceContext.ContentService.GetRootContent();
//            //foreach (var content in roots)
//            //{
//            //    var d = ServiceContext.ContentService.GetDescendants(content);
//            //}

//            //var oc31 = Database.PocoData.GetCachedPocoData().Count();
//            //var oc32 = Database.PocoData.GetConverters().Count();
//            //var oc33 = Database.GetAutoMappers().Count();
//            //var oc34 = Database.GetMultiPocoFactories().Count();

//            //for (int i = 0; i < 2; i++)
//            //{
//            //    roots = ServiceContext.ContentService.GetRootContent();
//            //    foreach (var content in roots)
//            //    {
//            //        var d = ServiceContext.ContentService.GetDescendants(content);
//            //    }    
//            //}

//            //var oc41 = Database.PocoData.GetCachedPocoData().Count();
//            //var oc42 = Database.PocoData.GetConverters().Count();
//            //var oc43 = Database.GetAutoMappers().Count();
//            //var oc44 = Database.GetMultiPocoFactories().Count();

//            //var i2 = DatabaseContext.Database.Fetch<dynamic>("SELECT TABLE_NAME, COLUMN_NAME, ORDINAL_POSITION, COLUMN_DEFAULT, IS_NULLABLE, DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS");
//            //var r2 =
//            //    i2.Select(
//            //        item =>
//            //        new ColumnInfo(item.TABLE_NAME, item.COLUMN_NAME, item.ORDINAL_POSITION, item.COLUMN_DEFAULT,
//            //                       item.IS_NULLABLE, item.DATA_TYPE)).ToList();


//            var pocoData = Database.PocoData.GetCachedPocoData();
//            Console.WriteLine("GetCachedPocoData: " + pocoData.Count());
//            foreach (var keyValuePair in pocoData)
//            {
//                Console.WriteLine(keyValuePair.Value.GetFactories().Count());
//            }

//            Console.WriteLine("GetConverters: " + Database.PocoData.GetConverters().Count());
//            Console.WriteLine("GetAutoMappers: " + Database.GetAutoMappers().Count());
//            Console.WriteLine("GetMultiPocoFactories: " + Database.GetMultiPocoFactories().Count());

//            //Assert.AreEqual(oc11, oc21);
//            //Assert.AreEqual(oc12, oc22);
//            //Assert.AreEqual(oc13, oc23);
//            //Assert.AreEqual(oc14, oc24);

//            //Assert.AreEqual(oc31, oc41);
//            //Assert.AreEqual(oc32, oc42);
//            //Assert.AreEqual(oc33, oc43);
//            //Assert.AreEqual(oc34, oc44);
//        }

//        public void CreateStuff()
//        {
//            var contentType1 = MockedContentTypes.CreateTextpageContentType("test1", "test1");
//            var contentType2 = MockedContentTypes.CreateTextpageContentType("test2", "test2");
//            var contentType3 = MockedContentTypes.CreateTextpageContentType("test3", "test3");
//            ServiceContext.ContentTypeService.Save(new[] { contentType1, contentType2, contentType3 });
//            contentType1.AllowedContentTypes = new[]
//            {
//                new ContentTypeSort(new Lazy<int>(() => contentType2.Id), 0, contentType2.Alias),
//                new ContentTypeSort(new Lazy<int>(() => contentType3.Id), 1, contentType3.Alias)
//            };
//            contentType2.AllowedContentTypes = new[]
//            {
//                new ContentTypeSort(new Lazy<int>(() => contentType1.Id), 0, contentType1.Alias),
//                new ContentTypeSort(new Lazy<int>(() => contentType3.Id), 1, contentType3.Alias)
//            };
//            contentType3.AllowedContentTypes = new[]
//            {
//                new ContentTypeSort(new Lazy<int>(() => contentType1.Id), 0, contentType1.Alias),
//                new ContentTypeSort(new Lazy<int>(() => contentType2.Id), 1, contentType2.Alias)
//            };
//            ServiceContext.ContentTypeService.Save(new[] { contentType1, contentType2, contentType3 });

//            var roots = MockedContent.CreateTextpageContent(contentType1, -1, 3);
//            ServiceContext.ContentService.Save(roots);
//            foreach (var root in roots)
//            {
//                var item1 = MockedContent.CreateTextpageContent(contentType1, root.Id, 3);
//                var item2 = MockedContent.CreateTextpageContent(contentType2, root.Id, 3);
//                var item3 = MockedContent.CreateTextpageContent(contentType3, root.Id, 3);

//                ServiceContext.ContentService.Save(item1.Concat(item2).Concat(item3));
//            }
//        }
//    }
//}