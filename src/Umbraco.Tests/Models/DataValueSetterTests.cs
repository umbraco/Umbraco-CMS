using System;
using System.Diagnostics;
using System.Xml;
using NUnit.Framework;
using Rhino.Mocks;
using Rhino.Mocks.Interfaces;
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
            var baseDataType = MockRepository.GenerateStub<BaseDataType>();
            var dataTypeData = MockRepository.GenerateMock<DefaultData>(baseDataType);
            dataTypeData.Stub(x => x.Value).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            // Act

            ((IDataValueSetter)dataTypeData).SetValue("Hello world", DataTypeDatabaseType.Nvarchar.ToString());
            var val = dataTypeData.Value;

            // Assert

            dataTypeData.AssertWasNotCalled(data => data.LoadValueFromDatabase());
        }

        [Test]
        public void LoadValueFromDatabase_Is_Called_When_SetValue_Is_Not_Used()
        {
            // Arrange            
            var baseDataType = MockRepository.GenerateStub<BaseDataType>();
            var dataTypeData = MockRepository.GenerateMock<DefaultData>(baseDataType);
            dataTypeData
                .Stub(data => data.LoadValueFromDatabase()).WhenCalled(invocation => Debug.WriteLine("asdf"));
            dataTypeData.Stub(x => x.Value).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            // Act

            var val = dataTypeData.Value;

            // Assert

            dataTypeData.AssertWasCalled(data => data.LoadValueFromDatabase());
        }

        [Test]
        public void SetValue_Is_Called_When_Executing_ToXml_On_A_Property_With_DataType_That_Implements_IDataValueSetter()
        {
            // Arrange            
            var dataTypeId = Guid.NewGuid();
            
            var dataTypeData = MockRepository.GenerateMock<IData, IDataValueSetter>();
            dataTypeData
                .Stub(data => data.ToXMl(Arg<XmlDocument>.Is.Anything))
                .Return(null) // you have to call Return() even though we're about to override it
                .WhenCalled(invocation =>
                    {
                        var xmlDoc = (XmlDocument) invocation.Arguments[0];
                        invocation.ReturnValue = xmlDoc.CreateElement("test");
                    });

            var dataType = MockRepository.GenerateStub<IDataType>();
            dataType.Stub(type => type.Data).Return(dataTypeData);

            var dataTypeSvc = MockRepository.GenerateStub<IDataTypeService>();
            dataTypeSvc.Stub(service => service.GetDataTypeById(dataTypeId)).Return(dataType);

            var property = new Property(
                1234,
                Guid.NewGuid(),
                new PropertyType(dataTypeId, DataTypeDatabaseType.Nvarchar)
                    {
                        Alias = "test"
                    }, "Hello world");                

            // Act

            var xml = property.ToXml(dataTypeSvc);

            // Assert

            ((IDataValueSetter)dataTypeData).AssertWasCalled(setter => setter.SetValue("Hello world", DataTypeDatabaseType.Nvarchar.ToString()));
        }

    }
}