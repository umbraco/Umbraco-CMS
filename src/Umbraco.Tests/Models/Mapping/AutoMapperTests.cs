using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AutoMapper;
using NUnit.Framework;
using Umbraco.Core.Cache;
using Umbraco.Core.Manifest;
using Umbraco.Core.PropertyEditors;
using Umbraco.Tests.TestHelpers;

namespace Umbraco.Tests.Models.Mapping
{
    [TestFixture]
    [UmbracoTest(AutoMapper = true)]
    public class AutoMapperTests : TestWithApplicationBase
    {
        protected override void Compose()
        {
            base.Compose();

            var manifestBuilder = new ManifestBuilder(
                CacheHelper.CreateDisabledCacheHelper().RuntimeCache,
                new ManifestParser(Logger, new DirectoryInfo(TestHelper.CurrentAssemblyDirectory), CacheHelper.CreateDisabledCacheHelper().RuntimeCache));
            Container.Register(_ => manifestBuilder);

            Func<IEnumerable<Type>> typeListProducerList = Enumerable.Empty<Type>;
            Container.GetInstance<PropertyEditorCollectionBuilder>()
                .Clear()
                .Add(typeListProducerList);
        }

        [Test]
        public void Assert_Valid_Mappings()
        {
            Mapper.AssertConfigurationIsValid();
        }
    }
}