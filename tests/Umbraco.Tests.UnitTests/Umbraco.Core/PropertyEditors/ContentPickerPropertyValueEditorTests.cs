using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models.Editors;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.Serialization;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.PropertyEditors;

[TestFixture]
internal sealed class ContentPickerPropertyValueEditorTests
{
    private static readonly Guid _first = new("0f4f9a0e-6b1e-4d0f-9b1c-2a3f6e5d4c3b");
    private static readonly Guid _second = new("8c2d7e61-3a4b-4c5d-8e9f-0a1b2c3d4e5f");

    [Test]
    public void Stored_Udi_To_Editor_Yields_Key()
        => Assert.That(
            CreateValueEditor().ToEditor(EntityPickerTestValues.Property(EntityPickerTestValues.Udi(Constants.UdiEntityType.Document, _second))),
            Is.EqualTo(_second));

    [Test]
    public void Stored_Udi_List_To_Editor_Yields_First_Key()
        => Assert.That(
            CreateValueEditor().ToEditor(EntityPickerTestValues.Property(EntityPickerTestValues.UdiList(Constants.UdiEntityType.Document, _first, _second))),
            Is.EqualTo(_first));

    [Test]
    public void Stored_Udi_List_To_Editor_Ignores_Other_Entity_Types()
        => Assert.That(
            CreateValueEditor().ToEditor(EntityPickerTestValues.Property(
                $"{EntityPickerTestValues.Udi(Constants.UdiEntityType.Media, _first)},{EntityPickerTestValues.Udi(Constants.UdiEntityType.Document, _second)}")),
            Is.EqualTo(_second));

    [Test]
    public void Stored_Udi_List_References_Every_Document()
        => Assert.That(
            CreateValueEditor().GetReferences(EntityPickerTestValues.UdiList(Constants.UdiEntityType.Document, _first, _second)).Select(r => r.Udi),
            Is.EqualTo(new[] { new GuidUdi(Constants.UdiEntityType.Document, _first), new GuidUdi(Constants.UdiEntityType.Document, _second) }));

    [Test]
    public void Selection_From_Editor_Yields_Udi()
        => Assert.That(
            CreateValueEditor().FromEditor(new ContentPropertyData(_first.ToString(), null), null),
            Is.EqualTo(EntityPickerTestValues.Udi(Constants.UdiEntityType.Document, _first)));

    private static ContentPickerPropertyEditor.ContentPickerPropertyValueEditor CreateValueEditor()
        => new(
            Mock.Of<IShortStringHelper>(),
            new SystemTextJsonSerializer(new DefaultJsonSerializerEncoderFactory()),
            Mock.Of<IIOHelper>(),
            new DataEditorAttribute(Constants.PropertyEditors.Aliases.ContentPicker) { ValueType = ValueTypes.String },
            Mock.Of<ICoreScopeProvider>(),
            Mock.Of<IContentService>(),
            Mock.Of<ILocalizedTextService>());
}
