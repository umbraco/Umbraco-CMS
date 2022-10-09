using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Cache;

[TestFixture]
public class ValueEditorCacheTests
{
    [Test]
    public void Caches_ValueEditor()
    {
        var sut = new ValueEditorCache();

        var dataEditor = new FakeDataEditor("TestEditor");
        var dataType = CreateDataTypeMock(1).Object;

        // Request the same value editor twice
        var firstEditor = sut.GetValueEditor(dataEditor, dataType);
        var secondEditor = sut.GetValueEditor(dataEditor, dataType);

        Assert.Multiple(() =>
        {
            Assert.AreSame(firstEditor, secondEditor);
            Assert.AreEqual(1, dataEditor.ValueEditorCount, "GetValueEditor invoked more than once.");
        });
    }

    [Test]
    public void Different_Data_Editors_Returns_Different_Value_Editors()
    {
        var sut = new ValueEditorCache();

        var dataEditor1 = new FakeDataEditor("Editor1");
        var dataEditor2 = new FakeDataEditor("Editor2");
        var dataType = CreateDataTypeMock(1).Object;

        var firstEditor = sut.GetValueEditor(dataEditor1, dataType);
        var secondEditor = sut.GetValueEditor(dataEditor2, dataType);

        Assert.AreNotSame(firstEditor, secondEditor);
    }

    [Test]
    public void Different_Data_Types_Returns_Different_Value_Editors()
    {
        var sut = new ValueEditorCache();
        var dataEditor = new FakeDataEditor("Editor");
        var dataType1 = CreateDataTypeMock(1).Object;
        var dataType2 = CreateDataTypeMock(2).Object;

        var firstEditor = sut.GetValueEditor(dataEditor, dataType1);
        var secondEditor = sut.GetValueEditor(dataEditor, dataType2);

        Assert.AreNotSame(firstEditor, secondEditor);
    }

    [Test]
    public void Clear_Cache_Removes_Specific_Editors()
    {
        var sut = new ValueEditorCache();

        var dataEditor1 = new FakeDataEditor("Editor 1");
        var dataEditor2 = new FakeDataEditor("Editor 2");

        var dataType1 = CreateDataTypeMock(1).Object;
        var dataType2 = CreateDataTypeMock(2).Object;

        // Load the editors into cache
        var editor1DataType1 = sut.GetValueEditor(dataEditor1, dataType1);
        var editor1Datatype2 = sut.GetValueEditor(dataEditor1, dataType2);
        var editor2DataType1 = sut.GetValueEditor(dataEditor2, dataType1);
        var editor2Datatype2 = sut.GetValueEditor(dataEditor2, dataType2);

        sut.ClearCache(new[] { dataType1.Id });

        // New value editor objects should be created after it's cleared
        Assert.AreNotSame(editor1DataType1, sut.GetValueEditor(dataEditor1, dataType1), "Value editor was not cleared from cache");
        Assert.AreNotSame(editor2DataType1, sut.GetValueEditor(dataEditor2, dataType1), "Value editor was not cleared from cache");

        // But the value editors for data type 2 should be the same
        Assert.AreSame(editor1Datatype2, sut.GetValueEditor(dataEditor1, dataType2), "Too many editors was cleared from cache");
        Assert.AreSame(editor2Datatype2, sut.GetValueEditor(dataEditor2, dataType2), "Too many editors was cleared from cache");
    }

    private Mock<IDataType> CreateDataTypeMock(int id)
    {
        var mock = new Mock<IDataType>();
        mock.Setup(x => x.Id).Returns(id);
        return mock;
    }

    /// <summary>
    ///     A fake IDataEditor
    /// </summary>
    /// <remarks>
    ///     This is necessary to ensure that different objects are returned from GetValueEditor
    /// </remarks>
    private class FakeDataEditor : IDataEditor
    {
        public int ValueEditorCount;

        public FakeDataEditor(string alias) => Alias = alias;

        public string Alias { get; }

        public EditorType Type { get; }

        public string Name { get; }

        public string Icon { get; }

        public string Group { get; }

        public bool IsDeprecated { get; }

        public IDataValueEditor GetValueEditor()
        {
            ValueEditorCount++;
            return Mock.Of<IDataValueEditor>();
        }

        public IDataValueEditor GetValueEditor(object configuration) => GetValueEditor();

        public IDictionary<string, object> DefaultConfiguration { get; }

        public IConfigurationEditor GetConfigurationEditor() => throw new NotImplementedException();

        public IPropertyIndexValueFactory PropertyIndexValueFactory { get; }
    }
}
