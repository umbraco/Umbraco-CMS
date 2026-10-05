// Copyright (c) Umbraco.
// See LICENSE for more details.

using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Cache.PropertyEditors;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Infrastructure.PropertyEditors.NotificationHandlers;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.PropertyEditors.NotificationHandlers;

[TestFixture]
public class FileUploadNotificationHandlerBaseTests
{
    [TestCase("Umbraco.UploadField", true, TestName = "Can_Recognise_Built_In_File_Upload_Editor")]
    [TestCase("Test.PrefixedUpload", true, TestName = "Can_Recognise_File_Upload_Editor_Subclass")]
    [TestCase("Test.NotUpload", false, TestName = "Cannot_Recognise_Unrelated_Editor")]
    [TestCase("Does.Not.Exist", false, TestName = "Cannot_Recognise_Unknown_Alias")]
    public void Can_Recognise_File_Upload_Editor_Property_Types(string alias, bool expected)
    {
        var factory = Mock.Of<IDataValueEditorFactory>();
        var ioHelper = Mock.Of<IIOHelper>();
        var dataEditors = new DataEditorCollection(() => new IDataEditor[]
        {
            new FileUploadPropertyEditor(factory, ioHelper),
            new TestPrefixedFileUploadPropertyEditor(factory, ioHelper),
            new TestNotUploadPropertyEditor(factory),
        });
        var propertyEditors = new PropertyEditorCollection(dataEditors);
        var handler = new TestHandler(propertyEditors);

        var propertyType = Mock.Of<IPropertyType>(x => x.PropertyEditorAlias == alias);

        Assert.AreEqual(expected, handler.IsUploadField(propertyType));
    }

    [DataEditor("Test.PrefixedUpload", ValueEditorIsReusable = true)]
    private sealed class TestPrefixedFileUploadPropertyEditor : FileUploadPropertyEditor
    {
        public TestPrefixedFileUploadPropertyEditor(IDataValueEditorFactory dataValueEditorFactory, IIOHelper ioHelper)
            : base(dataValueEditorFactory, ioHelper)
        {
        }
    }

    [DataEditor("Test.NotUpload", ValueEditorIsReusable = true)]
    private sealed class TestNotUploadPropertyEditor : DataEditor
    {
        public TestNotUploadPropertyEditor(IDataValueEditorFactory dataValueEditorFactory)
            : base(dataValueEditorFactory)
        {
        }
    }

    private sealed class TestHandler : FileUploadNotificationHandlerBase
    {
        public TestHandler(PropertyEditorCollection propertyEditors)
            : base(Mock.Of<IJsonSerializer>(), null!, Mock.Of<IBlockEditorElementTypeCache>(), propertyEditors)
        {
        }

        public bool IsUploadField(IPropertyType propertyType) => IsUploadFieldPropertyType(propertyType);
    }
}
