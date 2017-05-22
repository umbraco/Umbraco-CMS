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
using Umbraco.Tests.Testing;
using LightInject;

namespace Umbraco.Tests.Models.Mapping
{
    [TestFixture]
    [UmbracoTest(WithApplication = true)]
    public class AutoMapperTests : UmbracoTestBase
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