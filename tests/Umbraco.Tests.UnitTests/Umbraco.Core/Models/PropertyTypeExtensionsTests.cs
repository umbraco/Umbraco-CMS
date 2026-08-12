// Copyright (c) Umbraco.
// See LICENSE for more details.

using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Models;

[TestFixture]
public class PropertyTypeExtensionsTests
{
    private const int DataTypeId = 1234;
    private static readonly Guid _dataTypeKey = new("11111111-1111-1111-1111-111111111111");

    [Test]
    public void GetDataType_Uses_DataTypeKey_Directly_When_Set()
    {
        IPropertyType propertyType = new PropertyTypeBuilder()
            .WithDataTypeId(DataTypeId)
            .WithDataTypeKey(_dataTypeKey)
            .Build();

        var expected = Mock.Of<IDataType>();
        var dataTypeService = new Mock<IDataTypeService>();
        dataTypeService.Setup(x => x.GetAsync(_dataTypeKey)).ReturnsAsync(expected);
        var idKeyMap = new Mock<IIdKeyMap>(MockBehavior.Strict);

        IDataType? result = propertyType.GetDataType(dataTypeService.Object, idKeyMap.Object);

        Assert.AreSame(expected, result);
        idKeyMap.VerifyNoOtherCalls();
    }

    [Test]
    public void GetDataType_Falls_Back_To_IIdKeyMap_When_DataTypeKey_Empty()
    {
        IPropertyType propertyType = new PropertyTypeBuilder()
            .WithDataTypeId(DataTypeId)
            .WithDataTypeKey(Guid.Empty)
            .Build();

        var expected = Mock.Of<IDataType>();
        var idKeyMap = new Mock<IIdKeyMap>();
        idKeyMap.Setup(x => x.GetKeyForId(DataTypeId, UmbracoObjectTypes.DataType))
            .Returns(Attempt.Succeed(_dataTypeKey));
        var dataTypeService = new Mock<IDataTypeService>();
        dataTypeService.Setup(x => x.GetAsync(_dataTypeKey)).ReturnsAsync(expected);

        IDataType? result = propertyType.GetDataType(dataTypeService.Object, idKeyMap.Object);

        Assert.AreSame(expected, result);
        idKeyMap.Verify(x => x.GetKeyForId(DataTypeId, UmbracoObjectTypes.DataType), Times.Once);
    }

    [Test]
    public void GetDataType_Returns_Null_When_DataTypeKey_Empty_And_IIdKeyMap_Lookup_Fails()
    {
        IPropertyType propertyType = new PropertyTypeBuilder()
            .WithDataTypeId(DataTypeId)
            .WithDataTypeKey(Guid.Empty)
            .Build();

        var idKeyMap = new Mock<IIdKeyMap>();
        idKeyMap.Setup(x => x.GetKeyForId(DataTypeId, UmbracoObjectTypes.DataType))
            .Returns(Attempt<Guid>.Fail());
        var dataTypeService = new Mock<IDataTypeService>(MockBehavior.Strict);

        IDataType? result = propertyType.GetDataType(dataTypeService.Object, idKeyMap.Object);

        Assert.IsNull(result);
        dataTypeService.Verify(x => x.GetAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Test]
    public void GetDataType_Returns_Null_When_DataType_Not_Found()
    {
        IPropertyType propertyType = new PropertyTypeBuilder()
            .WithDataTypeKey(_dataTypeKey)
            .Build();

        var dataTypeService = new Mock<IDataTypeService>();
        dataTypeService.Setup(x => x.GetAsync(_dataTypeKey)).ReturnsAsync((IDataType?)null);
        var idKeyMap = Mock.Of<IIdKeyMap>();

        IDataType? result = propertyType.GetDataType(dataTypeService.Object, idKeyMap);

        Assert.IsNull(result);
    }
}
