using System;
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
            ConstantsCore.DataTypes.Guids.ContentPickerGuid,
            ConstantsCore.DataTypes.Guids.MemberPickerGuid,
            ConstantsCore.DataTypes.Guids.MediaPickerGuid,
            ConstantsCore.DataTypes.Guids.MultipleMediaPickerGuid,
            ConstantsCore.DataTypes.Guids.RelatedLinksGuid,
            ConstantsCore.DataTypes.Guids.MemberGuid,
            ConstantsCore.DataTypes.Guids.ImageCropperGuid,
            ConstantsCore.DataTypes.Guids.TagsGuid,
            ConstantsCore.DataTypes.Guids.ListViewContentGuid,
            ConstantsCore.DataTypes.Guids.ListViewMediaGuid,
            ConstantsCore.DataTypes.Guids.ListViewMembersGuid,
            ConstantsCore.DataTypes.Guids.DatePickerWithTimeGuid,
            ConstantsCore.DataTypes.Guids.ApprovedColorGuid,
            ConstantsCore.DataTypes.Guids.DropdownMultipleGuid,
            ConstantsCore.DataTypes.Guids.RadioboxGuid,
            ConstantsCore.DataTypes.Guids.DatePickerGuid,
            ConstantsCore.DataTypes.Guids.DropdownGuid,
            ConstantsCore.DataTypes.Guids.CheckboxListGuid,
            ConstantsCore.DataTypes.Guids.CheckboxGuid,
            ConstantsCore.DataTypes.Guids.NumericGuid,
            ConstantsCore.DataTypes.Guids.RichtextEditorGuid,
            ConstantsCore.DataTypes.Guids.TextstringGuid,
            ConstantsCore.DataTypes.Guids.TextareaGuid,
            ConstantsCore.DataTypes.Guids.UploadGuid,
            ConstantsCore.DataTypes.Guids.LabelStringGuid,
            ConstantsCore.DataTypes.Guids.LabelDecimalGuid,
            ConstantsCore.DataTypes.Guids.LabelDateTimeGuid,
            ConstantsCore.DataTypes.Guids.LabelBigIntGuid,
            ConstantsCore.DataTypes.Guids.LabelTimeGuid,
            ConstantsCore.DataTypes.Guids.LabelDateTimeGuid,
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
