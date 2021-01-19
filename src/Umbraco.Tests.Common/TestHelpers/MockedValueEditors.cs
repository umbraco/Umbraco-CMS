// Copyright (c) Umbraco.
// See LICENSE for more details.

using Moq;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Serialization;
using Umbraco.Core.Services;
using Umbraco.Core.Strings;

namespace Umbraco.Tests.TestHelpers.Entities
{
    public class MockedValueEditors
    {
        public static DataValueEditor CreateDataValueEditor(string name)
        {
            var valueType = ValueTypes.IsValue(name) ? name : ValueTypes.String;

            return new DataValueEditor(
                Mock.Of<IDataTypeService>(),
                Mock.Of<ILocalizationService>(),
                Mock.Of<ILocalizedTextService>(),
                Mock.Of<IShortStringHelper>(),
                new JsonNetSerializer(),
                new DataEditorAttribute(name, name, name)
                {
                    ValueType = valueType
                });
        }
    }
}
