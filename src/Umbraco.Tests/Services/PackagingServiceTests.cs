using System;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Tests.TestHelpers.Entities;

namespace Umbraco.Tests.Services
{
    //[TestFixture]
    //public class PackagingServiceTests : BaseServiceTest
    //{
    //    [Test]
    //    public void Export_Content()
    //    {
    //        var yesNo = DataTypesResolver.Current.GetById(new Guid(Constants.PropertyEditors.TrueFalse));
    //        var txtField = DataTypesResolver.Current.GetById(new Guid(Constants.PropertyEditors.Textbox));

    //        var contentWithDataType = MockedContentTypes.CreateSimpleContentType(
    //            "test",
    //            "Test",
    //            new PropertyTypeCollection(
    //                new PropertyType[]
    //                    {
    //                        new PropertyType(new DataTypeDefinition(-1, txtField.Id)
    //                            {
    //                                Name = "Testing Textfield", DatabaseType = DataTypeDatabaseType.Ntext
    //                            }),
    //                        new PropertyType(new DataTypeDefinition(-1, yesNo.Id)
    //                            {
    //                                Name = "Testing intfield", DatabaseType = DataTypeDatabaseType.Integer
    //                            })
    //                    }));

    //        var content = MockedContent.CreateSimpleContent(contentWithDataType);
    //        content.Name = "Test";

    //        var exported = ServiceContext.PackagingService.Export(content);

    //    }
    //}
}