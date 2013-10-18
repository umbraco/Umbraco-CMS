using System;
using System.Diagnostics;
using System.Xml;
using Moq;
using NUnit.Framework;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Core.Strings;
using Umbraco.Tests.TestHelpers;
using umbraco.cms.businesslogic.datatype;
using umbraco.interfaces;

namespace Umbraco.Tests.Models
{
    [TestFixture]
    public class DataValueSetterTests : BaseUmbracoApplicationTest
    {
        protected override void FreezeResolution()
        {
            ShortStringHelperResolver.Current = new ShortStringHelperResolver(new DefaultShortStringHelper());
            base.FreezeResolution();
        }

        [Test]
        public void LoadValueFromDatabase_Is_Not_Called_When_SetValue_Is_Used()
        {
            // Arrange            
            var dataTypeMock = new Mock<BaseDataType>();
            var dataTypeDataMock = new Mock<DefaultData>(dataTypeMock.Object);

            // Act

            ((IDataValueSetter)dataTypeDataMock.Object).SetValue("Hello world", DataTypeDatabaseType.Nvarchar.ToString());
            var val = dataTypeDataMock.Object.Value;

            // Assert

            dataTypeDataMock.Verify(data => data.LoadValueFromDatabase(), Times.Never());
        }

        [Test]
        public void LoadValueFromDatabase_Is_Called_When_SetValue_Is_Not_Used()
        {
            // Arrange            
            var dataTypeMock = new Mock<BaseDataType>();
            var dataTypeDataMock = new Mock<DefaultData>(dataTypeMock.Object) {CallBase = true};
            dataTypeDataMock
                .Setup(data => data.LoadValueFromDatabase()).Callback(() => Debug.WriteLine("asdf"));

            // Act

            var val = dataTypeDataMock.Object.Value;

            // Assert

            dataTypeDataMock.Verify(data => data.LoadValueFromDatabase());
        }

        [Test]
        public void SetValue_Is_Called_When_Executing_ToXml_On_A_Property_With_DataType_That_Implements_IDataValueSetter()
        {
            // Arrange            
            var dataTypeId = Guid.NewGuid();
            
            var dataTypeDataMock = new Mock<IData>();
            var dataValueSetterMock = dataTypeDataMock.As<IDataValueSetter>();

            dataTypeDataMock
                .Setup(data => data.ToXMl(It.IsAny<XmlDocument>()))
                .Returns((XmlDocument xdoc) => xdoc.CreateElement("test"));

            var dataTypeMock = new Mock<IDataType>();
            dataTypeMock.Setup(type => type.Data).Returns(dataTypeDataMock.Object);

            var dataTypeSvcMock = new Mock<IDataTypeService>();
            dataTypeSvcMock.Setup(service => service.GetDataTypeById(dataTypeId)).Returns(dataTypeMock.Object);

            var property = new Property(
                1234,
                Guid.NewGuid(),
                new PropertyType(dataTypeId, DataTypeDatabaseType.Nvarchar)
                    {
                        Alias = "test"
                    }, "Hello world");                

            // Act

            var xml = property.ToXml(dataTypeSvcMock.Object);

            // Assert

            dataValueSetterMock.Verify(setter => setter.SetValue("Hello world", DataTypeDatabaseType.Nvarchar.ToString()));
        }

        [TestCase(DataTypeDatabaseType.Nvarchar)]
        [TestCase(DataTypeDatabaseType.Date)]
        [TestCase(DataTypeDatabaseType.Integer)]
        [TestCase(DataTypeDatabaseType.Ntext)]
        public void DefaultData_SetValue_Ensures_Empty_String_When_Null_Value_Any_Data_Type(DataTypeDatabaseType type)
        {
            var defaultData = new DefaultData(new Mock<BaseDataType>().Object);

            ((IDataValueSetter)defaultData).SetValue(null, type.ToString());

            Assert.AreEqual(string.Empty, defaultData.Value);
        }

    }
}