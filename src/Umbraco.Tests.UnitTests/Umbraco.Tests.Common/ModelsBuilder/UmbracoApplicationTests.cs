﻿using System;
using System.Collections.Generic;
using NUnit.Framework;
using Umbraco.ModelsBuilder.Embedded;
using Umbraco.ModelsBuilder.Embedded.Building;

namespace Umbraco.Tests.ModelsBuilder
{
    [TestFixture]
    public class UmbracoApplicationTests
    {
        //[Test]
        //public void Test()
        //{
        //    // start and terminate
        //    using (var app = Application.GetApplication(TestOptions.ConnectionString, TestOptions.DatabaseProvider))
        //    { }

        //    // start and terminate
        //    using (var app = Application.GetApplication(TestOptions.ConnectionString, TestOptions.DatabaseProvider))
        //    { }

        //    // start, use and terminate
        //    using (var app = Application.GetApplication(TestOptions.ConnectionString, TestOptions.DatabaseProvider))
        //    {
        //        var types = app.GetContentTypes();
        //    }
        //}

        [Test]
        public void ThrowsOnDuplicateAliases()
        {
            var typeModels = new List<TypeModel>
            {
                new TypeModel { ItemType = TypeModel.ItemTypes.Content, Alias = "content1" },
                new TypeModel { ItemType = TypeModel.ItemTypes.Content, Alias = "content2" },
                new TypeModel { ItemType = TypeModel.ItemTypes.Media, Alias = "media1" },
                new TypeModel { ItemType = TypeModel.ItemTypes.Media, Alias = "media2" },
                new TypeModel { ItemType = TypeModel.ItemTypes.Member, Alias = "member1" },
                new TypeModel { ItemType = TypeModel.ItemTypes.Member, Alias = "member2" },
            };

            Assert.AreEqual(6, UmbracoServices.EnsureDistinctAliases(typeModels).Count);

            typeModels.Add(new TypeModel { ItemType = TypeModel.ItemTypes.Media, Alias = "content1" });

            try
            {
                UmbracoServices.EnsureDistinctAliases(typeModels);
            }
            catch (NotSupportedException e)
            {
                Console.WriteLine(e.Message);
                return;
            }

            Assert.Fail("Expected NotSupportedException.");
        }
    }
}
