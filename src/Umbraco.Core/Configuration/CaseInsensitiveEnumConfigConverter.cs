using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Configuration;

namespace Umbraco.Core.Configuration
{
    /// <summary>
    /// A case-insensitive configuration converter for enumerations.
    /// </summary>
    /// <typeparam name="T">The type of the enumeration.</typeparam>
    public class CaseInsensitiveEnumConfigConverter<T> : ConfigurationConverterBase
        where T : struct
    {
        public override object ConvertFrom(ITypeDescriptorContext ctx, CultureInfo ci, object data)
        {
            if (data == null)
                throw new ArgumentNullException("data");

            //return Enum.Parse(typeof(T), (string)data, true);

            T value;
            if (Enum.TryParse((string)data, true, out value))
                return value;

            throw new Exception(string.Format("\"{0}\" is not valid {1} value. Valid values are: {2}.",
                data, typeof(T).Name,
                string.Join(", ", Enum.GetValues(typeof(T)).Cast<T>())));
        }
    }
}
