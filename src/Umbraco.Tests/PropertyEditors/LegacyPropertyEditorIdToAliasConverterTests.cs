using System;
using NUnit.Framework;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Tests.PropertyEditors
{
    [TestFixture]
    public class LegacyPropertyEditorIdToAliasConverterTests
    {
        [TearDown]
        public void Reset()
        {
            LegacyPropertyEditorIdToAliasConverter.Reset();
        }

        [Test]
        public void Doesnt_Create_Duplicates_Without_Exception()
        {
            var id = Guid.NewGuid();
            LegacyPropertyEditorIdToAliasConverter.CreateMap(id, "test");
            LegacyPropertyEditorIdToAliasConverter.CreateMap(id, "test");
            Assert.AreEqual(1, LegacyPropertyEditorIdToAliasConverter.Count());
        }

        [Test]
        public void Can_Get_Legacy_Id_From_Alias()
        {
            var id = Guid.NewGuid();
            LegacyPropertyEditorIdToAliasConverter.CreateMap(id, "test");

            Assert.AreEqual(id, LegacyPropertyEditorIdToAliasConverter.GetLegacyIdFromAlias("test", true));
        }

        [Test]
        public void Can_Get_Alias_From_Legacy_Id()
        {
            var id = Guid.NewGuid();
            LegacyPropertyEditorIdToAliasConverter.CreateMap(id, "test");

            Assert.AreEqual("test", LegacyPropertyEditorIdToAliasConverter.GetAliasFromLegacyId(id, true));
        }

        [Test]
        public void Creates_Map_For_Core_Editors()
        {
            LegacyPropertyEditorIdToAliasConverter.CreateMappingsForCoreEditors();

            Assert.AreEqual(36, LegacyPropertyEditorIdToAliasConverter.Count());
        }
    }
}