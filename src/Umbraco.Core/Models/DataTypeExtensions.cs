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
        internal static bool IsBuildInDataType(this IDataTypeDefinition dataType)
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
