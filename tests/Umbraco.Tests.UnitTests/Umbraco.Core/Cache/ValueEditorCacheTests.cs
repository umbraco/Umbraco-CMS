using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Cache;

/// <summary>
/// Contains unit tests for the <see cref="ValueEditorCache"/> functionality in the Umbraco CMS core cache.
/// </summary>
[TestFixture]
public class ValueEditorCacheTests
{
    /// <summary>
    /// Tests that the ValueEditorCache caches and returns the same value editor instance
    /// when requested multiple times with the same parameters.
    /// </summary>
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

    /// <summary>
    /// Tests that different data editors return different value editors from the cache.
    /// </summary>
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

    /// <summary>
    /// Tests that different data types return different value editors from the cache.
    /// </summary>
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

    /// <summary>
    /// Tests that clearing the cache removes only the value editors associated with the specified data type IDs.
    /// Ensures that value editors for other data types remain cached.
    /// </summary>
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

    /// <summary>
    /// Initializes a new instance of the <see cref="FakeDataEditor"/> class.
    /// </summary>
    /// <param name="alias">The alias for the data editor.</param>
        public FakeDataEditor(string alias) => Alias = alias;

    /// <summary>
    /// Gets the alias of the FakeDataEditor.
    /// </summary>
        public string Alias { get; }

    /// <summary>
    /// Gets the name of the fake data editor.
    /// </summary>
        public string Name { get; }

    /// <summary>
    /// Gets a value indicating whether this editor is deprecated.
    /// </summary>
        public bool IsDeprecated { get; }

    /// <summary>
    /// Returns a mocked <see cref="IDataValueEditor"/> instance and increments the <c>ValueEditorCount</c>.
    /// </summary>
    /// <returns>A mocked <see cref="IDataValueEditor"/> instance.</returns>
        public IDataValueEditor GetValueEditor()
        {
            ValueEditorCount++;
            return Mock.Of<IDataValueEditor>();
        }

        public IDataValueEditor GetValueEditor(object configurationObject) => GetValueEditor();

    /// <summary>
    /// Gets the default configuration dictionary for the <see cref="FakeDataEditor"/>.
    /// </summary>
        public IDictionary<string, object> DefaultConfiguration { get; }

    /// <summary>
    /// Returns an instance of the configuration editor associated with this data editor.
    /// </summary>
    /// <returns>An <see cref="IConfigurationEditor"/> instance for configuring the data editor.</returns>
        public IConfigurationEditor GetConfigurationEditor() => throw new NotImplementedException();

    /// <summary>
    /// Gets the <see cref="IPropertyIndexValueFactory"/> instance used by this <see cref="FakeDataEditor"/> for property index value creation.
    /// </summary>
        public IPropertyIndexValueFactory PropertyIndexValueFactory { get; }
    }
}
