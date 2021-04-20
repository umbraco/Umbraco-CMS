using System;
using System.ComponentModel;

namespace Umbraco.Core
{
    /// <summary>
    /// Represents a guid-based entity identifier.
    /// </summary>
    [TypeConverter(typeof(UdiTypeConverter))]
    public class GuidUdi : Udi
    {
        /// <summary>
        /// The guid part of the identifier.
        /// </summary>
        public Guid Guid { get; private set; }

        /// <summary>
        /// Initializes a new instance of the GuidUdi class with an entity type and a guid.
        /// </summary>
        /// <param name="entityType">The entity type part of the udi.</param>
        /// <param name="guid">The guid part of the udi.</param>
        public GuidUdi(string entityType, Guid guid)
            : base(entityType, "umb://" + entityType + "/" + guid.ToString("N"))
        {
            Guid = guid;
        }

        /// <summary>
        /// Initializes a new instance of the GuidUdi class with an uri value.
        /// </summary>
        /// <param name="uriValue">The uri value of the udi.</param>
        public GuidUdi(Uri uriValue)
            : base(uriValue)
        {
            Guid guid;
            if (Guid.TryParse(uriValue.AbsolutePath.TrimStart(Constants.CharArrays.ForwardSlash), out guid) == false)
                throw new FormatException("URI \"" + uriValue + "\" is not a GUID entity ID.");

            Guid = guid;
        }

        /// <summary>
        /// Converts the string representation of an entity identifier into the equivalent GuidUdi instance.
        /// </summary>
        /// <param name="s">The string to convert.</param>
        /// <returns>A GuidUdi instance that contains the value that was parsed.</returns>
        public new static GuidUdi Parse(string s)
        {
            var udi = Udi.Parse(s);
            if (udi is GuidUdi == false)
                throw new FormatException("String \"" + s + "\" is not a GUID entity id.");

            return (GuidUdi) udi;
        }

        public static bool TryParse(string s, out GuidUdi udi)
        {
            Udi tmp;
            udi = null;
            if (TryParse(s, out tmp) == false) return false;
            udi = tmp as GuidUdi;
            return udi != null;
        }

        public override bool Equals(object obj)
        {
            var other = obj as GuidUdi;
            if (other == null) return false;
            return EntityType == other.EntityType && Guid == other.Guid;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <inheritdoc/>
        public override bool IsRoot
        {
            get { return Guid == Guid.Empty; }
        }

        public GuidUdi EnsureClosed()
        {
            EnsureNotRoot();
            return this;
        }
    }
}
