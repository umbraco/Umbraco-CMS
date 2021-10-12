﻿using Umbraco.Cms.Core.Models;

namespace Umbraco.Tests.TestHelpers.Entities
{
    public class MockedPropertyTypes
    {
        /// <summary>
        /// Returns a decimal property.
        /// Requires a datatype definition Id, since this is not one of the pre-populated datatypes
        /// </summary>
        /// <param name="alias">Alias of the created property type</param>
        /// <param name="name">Name of the created property type</param>
        /// <param name="dtdId">Integer Id of a decimal datatype to use</param>
        /// <returns>Property type storing decimal value</returns>
        public static PropertyType CreateDecimalProperty(string alias, string name, int dtdId)
        {
            return
                new PropertyType(TestHelper.ShortStringHelper, "test", ValueStorageType.Decimal, alias)
                {
                    Name = name,
                    Description = "Decimal property type",
                    Mandatory = false,
                    SortOrder = 4,
                    DataTypeId = dtdId
                };
        }

        /// <summary>
        /// Returns a integer property.
        /// </summary>
        /// <param name="alias">Alias of the created property type</param>
        /// <param name="name">Name of the created property type</param>
        /// <returns>Property type storing integer value</returns>
        public static PropertyType CreateIntegerProperty(string alias, string name)
        {
            return
                new PropertyType(TestHelper.ShortStringHelper, "test", ValueStorageType.Integer, alias)
                {
                    Name = name,
                    Description = "Integer property type",
                    Mandatory = false,
                    SortOrder = 4,
                    DataTypeId = -51
                };
        }

        /// <summary>
        /// Returns a DateTime property.
        /// </summary>
        /// <param name="alias">Alias of the created property type</param>
        /// <param name="name">Name of the created property type</param>
        /// <returns>Property type storing DateTime value</returns>
        public static PropertyType CreateDateTimeProperty(string alias, string name)
        {
            return
                new PropertyType(TestHelper.ShortStringHelper, "test", ValueStorageType.Date, alias)
                {
                    Name = name,
                    Description = "DateTime property type",
                    Mandatory = false,
                    SortOrder = 4,
                    DataTypeId = -36
                };
        }
    }
}
