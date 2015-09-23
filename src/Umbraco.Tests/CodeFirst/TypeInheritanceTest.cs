using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Profiling;
using Umbraco.Tests.CodeFirst.Attributes;
using Umbraco.Tests.CodeFirst.Definitions;
using Umbraco.Tests.CodeFirst.TestModels.Composition;

namespace Umbraco.Tests.CodeFirst
{
    [TestFixture]
    public class TypeInheritanceTest
    {
        private PluginManager _pluginManager;

        [SetUp]
        public void Initialize()
        {
            var logger = new ProfilingLogger(Mock.Of<ILogger>(), Mock.Of<IProfiler>());

            //this ensures its reset
            _pluginManager = new PluginManager(new ActivatorServiceProvider(), new NullCacheProvider(),
                logger,
                false)
            {
                AssembliesToScan = new[]
		        {
		            typeof (ContentTypeBase).Assembly
		        }
            };

          
        }

        [Test]
        public void Can_Get_Interfaces_From_Type()
        {
            var type = typeof (News);
            var interfaces = type.GetInterfaces().ToList();

            bool hasSeo = interfaces.Any(x => x.Name == typeof(IMetaSeo).Name);
            bool hasMeta = interfaces.Any(x => x.Name == typeof(IMeta).Name);

            Assert.That(hasSeo, Is.True);
            Assert.That(hasMeta, Is.True);
            Assert.That(interfaces.Count, Is.EqualTo(3));
        }

        [Test]
        public void Can_Get_MixinAttribute_From_Types()
        {
            var type = typeof(News);
            var interfaces = type.GetInterfaces().ToList();

            var list = new List<string>();

            foreach (var interfaceType in interfaces)
            {
                var mixinAttribute = interfaceType.FirstAttribute<MixinAttribute>();
                if(mixinAttribute != null)
                {
                    var modelType = mixinAttribute.Type;
                    var contentTypeAttribute = modelType.FirstAttribute<ContentTypeAttribute>();
                    var contentTypeAlias = contentTypeAttribute == null ? modelType.Name.ToUmbracoAlias() : contentTypeAttribute.Alias;
                    list.Add(contentTypeAlias);
                }
            }

            Assert.That(list.Count, Is.EqualTo(3));
            Assert.Contains("Meta", list);
            Assert.Contains("MetaSeo", list);
            Assert.Contains("Base", list);
        }

        [Test]
        public void Ensure_Only_One_Type_List_Created()
        {
            var foundTypes = _pluginManager.ResolveContentTypeBaseTypes();

            Assert.That(foundTypes.Count(), Is.EqualTo(15));
            Assert.AreEqual(1,
                            _pluginManager.GetTypeLists()
                                .Count(x => x.IsTypeList<ContentTypeBase>(PluginManager.TypeResolutionKind.FindAllTypes)));
        }

    }
}