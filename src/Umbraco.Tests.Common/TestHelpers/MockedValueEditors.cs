using System;
using System.Collections.Generic;
using System.Text;
using Moq;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;
using Umbraco.Core.Strings;

namespace Umbraco.Tests.Common.TestHelpers
{
    public static class MockedValueEditors
    {
        public static DataValueEditor CreateDataValueEditor(string name)
        {
            var valueType = (ValueTypes.IsValue(name)) ? name : ValueTypes.String;

            return new DataValueEditor(
                Mock.Of<IDataTypeService>(),
                Mock.Of<ILocalizationService>(),
                Mock.Of<ILocalizedTextService>(),
                Mock.Of<IShortStringHelper>(),
                new DataEditorAttribute(name, name, name)
                {
                    ValueType = valueType
                }

            );
        }
    }
}
