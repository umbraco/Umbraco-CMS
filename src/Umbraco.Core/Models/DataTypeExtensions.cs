﻿using System;
using System.Collections.Generic;

namespace Umbraco.Core.Models
{
    internal static class DataTypeExtensions
    {
        private static readonly ISet<Guid> IdsOfBuildInDataTypes = new HashSet<Guid>()
        {
            Constants.DataTypes.ContentPickerGuid,
            Constants.DataTypes.MemberPickerGuid,
            Constants.DataTypes.MediaPickerGuid,
            Constants.DataTypes.MultipleMediaPickerGuid,
            Constants.DataTypes.RelatedLinksGuid,
            Constants.DataTypes.MemberGuid,
            Constants.DataTypes.ImageCropperGuid,
            Constants.DataTypes.TagsGuid,
            Constants.DataTypes.ListViewContentGuid,
            Constants.DataTypes.ListViewMediaGuid,
            Constants.DataTypes.ListViewMembersGuid,
            Constants.DataTypes.DatePickerWithTimeGuid,
            Constants.DataTypes.ApprovedColorGuid,
            Constants.DataTypes.DropdownMultipleGuid,
            Constants.DataTypes.RadioboxGuid,
            Constants.DataTypes.DatePickerGuid,
            Constants.DataTypes.DropdownGuid,
            Constants.DataTypes.CheckboxListGuid,
            Constants.DataTypes.CheckboxGuid,
            Constants.DataTypes.NumericGuid,
            Constants.DataTypes.RichtextEditorGuid,
            Constants.DataTypes.TextstringGuid,
            Constants.DataTypes.TextareaGuid,
            Constants.DataTypes.UploadGuid,
            Constants.DataTypes.LabelGuid,
        };

        /// <summary>
        /// Returns true if this date type is build-in/default.
        /// </summary>
        /// <param name="dataType">The data type definition.</param>
        /// <returns></returns>
        public static bool IsBuildInDataType(this IDataTypeDefinition dataType)
        {
            return IsBuildInDataType(dataType.Key);
        }

        /// <summary>
        /// Returns true if this date type is build-in/default.
        /// </summary>
        public static bool IsBuildInDataType(Guid key)
        {
            return IdsOfBuildInDataTypes.Contains(key);
        }
    }
}
