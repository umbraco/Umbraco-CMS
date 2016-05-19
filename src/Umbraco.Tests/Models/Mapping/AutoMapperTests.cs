using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AutoMapper;
using Moq;
using NUnit.Framework;
using Umbraco.Core.Cache;
using Umbraco.Core.Logging;
using Umbraco.Core.Manifest;
using Umbraco.Core.PropertyEditors;
using Umbraco.Tests.TestHelpers;

namespace Umbraco.Tests.Models.Mapping
{
    [RequiresAutoMapperMappings]
    [TestFixture]
    public class AutoMapperTests : BaseUmbracoApplicationTest
    {
        /// <summary>
        /// Inheritors can override this to setup any resolvers before resolution is frozen
        /// </summary>
        protected override void FreezeResolution()
        {
            Func<IEnumerable<Type>> typeListProducerList = Enumerable.Empty<Type>;
            var propertyEditorResolver = new PropertyEditorResolver(
                Container,
                Logger,
                typeListProducerList,
                new ManifestBuilder(
                    CacheHelper.CreateDisabledCacheHelper().RuntimeCache,
                    new ManifestParser(Logger, new DirectoryInfo(TestHelper.CurrentAssemblyDirectory), CacheHelper.CreateDisabledCacheHelper().RuntimeCache)));

            PropertyEditorResolver.Current = propertyEditorResolver;

            base.FreezeResolution();
        }

        [Test]
        public void Assert_Valid_Mappings()
        {
            Mapper.AssertConfigurationIsValid();
        }
    }
}