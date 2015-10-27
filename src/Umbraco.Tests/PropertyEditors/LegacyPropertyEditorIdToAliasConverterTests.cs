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

            Assert.AreEqual(
                id,
                LegacyPropertyEditorIdToAliasConverter.GetLegacyIdFromAlias(
                    "test",
                    LegacyPropertyEditorIdToAliasConverter.NotFoundLegacyIdResponseBehavior.ThrowException));
        }

        [Test]
        public void Can_Get_Alias_From_Legacy_Id()
        {
            var id = Guid.NewGuid();
            LegacyPropertyEditorIdToAliasConverter.CreateMap(id, "test");

            Assert.AreEqual(
                "test",
                LegacyPropertyEditorIdToAliasConverter.GetAliasFromLegacyId(
                    id,
                    true));
        }

        [Test]
        public void Can_Generate_Id_From_Missing_Alias()
        {
            var gen1 = LegacyPropertyEditorIdToAliasConverter.GetLegacyIdFromAlias("Donotfindthisone", LegacyPropertyEditorIdToAliasConverter.NotFoundLegacyIdResponseBehavior.GenerateId);
            var gen2 = LegacyPropertyEditorIdToAliasConverter.GetLegacyIdFromAlias("Donotfindthisone", LegacyPropertyEditorIdToAliasConverter.NotFoundLegacyIdResponseBehavior.GenerateId);

            Assert.IsNotNull(gen1);
            Assert.IsNotNull(gen2);
            Assert.AreEqual(gen1, gen2);
        }

        [Test]
        public void Creates_Map_For_Core_Editors()
        {
            LegacyPropertyEditorIdToAliasConverter.CreateMappingsForCoreEditors();

            Assert.AreEqual(36, LegacyPropertyEditorIdToAliasConverter.Count());
        }
    }
}