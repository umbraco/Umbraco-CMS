using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Moq;
using NUnit.Framework;
using Umbraco.Core.Logging;
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
            var propertyEditorResolver = new Mock<PropertyEditorResolver>(
                //ctor args
                Mock.Of<IServiceProvider>(),
                Mock.Of<ILogger>(),
                typeListProducerList,
                Umbraco.Core.CacheHelper.CreateDisabledCacheHelper().RuntimeCache);

            PropertyEditorResolver.Current = propertyEditorResolver.Object;

            base.FreezeResolution();
        }

        [Test]
        public void Assert_Valid_Mappings()
        {
            Mapper.AssertConfigurationIsValid();
        }
    }
}