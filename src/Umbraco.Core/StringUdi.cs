using System;
using System.ComponentModel;
using System.Linq;

namespace Umbraco.Core
{
    /// <summary>
    /// Represents a string-based entity identifier.
    /// </summary>
    [TypeConverter(typeof(UdiTypeConverter))]
    public class StringUdi : Udi
    {
        /// <summary>
        /// The string part of the identifier.
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        /// Initializes a new instance of the StringUdi class with an entity type and a string id.
        /// </summary>
        /// <param name="entityType">The entity type part of the udi.</param>
        /// <param name="id">The string id part of the udi.</param>
        public StringUdi(string entityType, string id)
            : base(entityType, "umb://" + entityType + "/" + EscapeUriString(id))
        {
            Id = id;
        }

        /// <summary>
        /// Initializes a new instance of the StringUdi class with a uri value.
        /// </summary>
        /// <param name="uriValue">The uri value of the udi.</param>
        public StringUdi(Uri uriValue)
            : base(uriValue)
        {
            Id = Uri.UnescapeDataString(uriValue.AbsolutePath.TrimStart(Constants.CharArrays.ForwardSlash));
        }

        private static string EscapeUriString(string s)
        {
            // Uri.EscapeUriString preserves / but also [ and ] which is bad
            // Uri.EscapeDataString does not preserve / which is bad

            // reserved = : / ? # [ ] @ ! $ & ' ( ) * + , ; =
            // unreserved = alpha digit - . _ ~

            // we want to preserve the / and the unreserved
            // so...
            return string.Join("/", s.Split(Constants.CharArrays.ForwardSlash).Select(Uri.EscapeDataString));
        }

        /// <summary>
        /// Converts the string representation of an entity identifier into the equivalent StringUdi instance.
        /// </summary>
        /// <param name="s">The string to convert.</param>
        /// <returns>A StringUdi instance that contains the value that was parsed.</returns>
        public new static StringUdi Parse(string s)
        {
            var udi = Udi.Parse(s);
            if (udi is StringUdi == false)
                throw new FormatException("String \"" + s + "\" is not a string entity id.");

            return (StringUdi) udi;
        }

        public static bool TryParse(string s, out StringUdi udi)
        {
            udi = null;
            Udi tmp;
            if (TryParse(s, out tmp) == false || tmp is StringUdi == false) return false;
            udi = (StringUdi) tmp;
            return true;
        }

        /// <inheritdoc/>
        public override bool IsRoot
        {
            get { return Id == string.Empty; }
        }

        public StringUdi EnsureClosed()
        {
            EnsureNotRoot();
            return this;
        }
    }
}
