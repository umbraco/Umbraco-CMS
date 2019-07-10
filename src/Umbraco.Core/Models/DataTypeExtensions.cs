﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Provides extensions methods for <see cref="IDataType"/>.
    /// </summary>
    public static class DataTypeExtensions
    {
        /// <summary>
        /// Gets the configuration object.
        /// </summary>
        /// <typeparam name="T">The expected type of the configuration object.</typeparam>
        /// <param name="dataType">This datatype.</param>
        /// <exception cref="InvalidCastException">When the datatype configuration is not of the expected type.</exception>
        public static T ConfigurationAs<T>(this IDataType dataType)
            where T : class
        {
            if (dataType == null)
                throw new ArgumentNullException(nameof(dataType));

            var configuration = dataType.Configuration;

            switch (configuration)
            {
                case null:
                    return null;
                case T configurationAsT:
                    return configurationAsT;
            }

            throw new InvalidCastException($"Cannot cast dataType configuration, of type {configuration.GetType().Name}, to {typeof(T).Name}.");
        }
        
        private static readonly ISet<Guid> IdsOfBuildInDataTypes = new HashSet<Guid>()
        {
            Constants.DataTypes.Guids.ContentPickerGuid,
            Constants.DataTypes.Guids.MemberPickerGuid,
            Constants.DataTypes.Guids.MediaPickerGuid,
            Constants.DataTypes.Guids.MultipleMediaPickerGuid,
            Constants.DataTypes.Guids.RelatedLinksGuid,
            Constants.DataTypes.Guids.MemberGuid,
            Constants.DataTypes.Guids.ImageCropperGuid,
            Constants.DataTypes.Guids.TagsGuid,
            Constants.DataTypes.Guids.ListViewContentGuid,
            Constants.DataTypes.Guids.ListViewMediaGuid,
            Constants.DataTypes.Guids.ListViewMembersGuid,
            Constants.DataTypes.Guids.DatePickerWithTimeGuid,
            Constants.DataTypes.Guids.ApprovedColorGuid,
            Constants.DataTypes.Guids.DropdownMultipleGuid,
            Constants.DataTypes.Guids.RadioboxGuid,
            Constants.DataTypes.Guids.DatePickerGuid,
            Constants.DataTypes.Guids.DropdownGuid,
            Constants.DataTypes.Guids.CheckboxListGuid,
            Constants.DataTypes.Guids.CheckboxGuid,
            Constants.DataTypes.Guids.NumericGuid,
            Constants.DataTypes.Guids.RichtextEditorGuid,
            Constants.DataTypes.Guids.TextstringGuid,
            Constants.DataTypes.Guids.TextareaGuid,
            Constants.DataTypes.Guids.UploadGuid,
            Constants.DataTypes.Guids.LabelStringGuid,
            Constants.DataTypes.Guids.LabelDecimalGuid,
            Constants.DataTypes.Guids.LabelDateTimeGuid,
            Constants.DataTypes.Guids.LabelBigIntGuid,
            Constants.DataTypes.Guids.LabelTimeGuid,
            Constants.DataTypes.Guids.LabelDateTimeGuid,
        };

        /// <summary>
        /// Returns true if this date type is build-in/default.
        /// </summary>
        /// <param name="dataType">The data type definition.</param>
        /// <returns></returns>
        internal static bool IsBuildInDataType(this IDataType dataType)
        {
            return IsBuildInDataType(dataType.Key);
        }

        /// <summary>
        /// Returns true if this date type is build-in/default.
        /// </summary>
        internal static bool IsBuildInDataType(Guid key)
        {
            return IdsOfBuildInDataTypes.Contains(key);
        }

    }
}
