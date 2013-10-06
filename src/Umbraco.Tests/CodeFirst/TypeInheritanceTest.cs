using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Tests.CodeFirst.Attributes;
using Umbraco.Tests.CodeFirst.Definitions;
using Umbraco.Tests.CodeFirst.TestModels.Composition;
using Umbraco.Tests.TestHelpers;

namespace Umbraco.Tests.CodeFirst
{
    [TestFixture]
    public class TypeInheritanceTest
    {
        [SetUp]
        public void Initialize()
        {
            TestHelper.SetupLog4NetForTests();

            //this ensures its reset
            PluginManager.Current = new PluginManager(false);

            //for testing, we'll specify which assemblies are scanned for the PluginTypeResolver
            PluginManager.Current.AssembliesToScan = new[]
                {
                    typeof (ContentTypeBase).Assembly
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
            var foundTypes = PluginManager.Current.ResolveContentTypeBaseTypes();

            Assert.That(foundTypes.Count(), Is.EqualTo(15));
            Assert.AreEqual(1,
                            PluginManager.Current.GetTypeLists()
                                .Count(x => x.IsTypeList<ContentTypeBase>(PluginManager.TypeResolutionKind.FindAllTypes)));
        }

        [TearDown]
        public void TearDown()
        {
            PluginManager.Current = null;
        }
    }
}