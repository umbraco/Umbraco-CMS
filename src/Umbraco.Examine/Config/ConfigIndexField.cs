using System.Configuration;

namespace Umbraco.Examine.Config
{
    ///<summary>
    /// A configuration item representing a field to index
    ///</summary>
    public sealed class ConfigIndexField : ConfigurationElement
    {
        [ConfigurationProperty("Name", IsRequired = true)]
        public string Name
        {
            get => (string)this["Name"];
            set => this["Name"] = value;
        }

        [ConfigurationProperty("EnableSorting", IsRequired = false)]
        public bool EnableSorting
        {
            get => (bool)this["EnableSorting"];
            set => this["EnableSorting"] = value;
        }

        [ConfigurationProperty("Type", IsRequired = false, DefaultValue = "String")]
        public string Type
        {
            get => (string)this["Type"];
            set => this["Type"] = value;
        }

        public bool Equals(ConfigIndexField other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(Name, other.Name);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ConfigIndexField)obj);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public static bool operator ==(ConfigIndexField left, ConfigIndexField right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ConfigIndexField left, ConfigIndexField right)
        {
            return !Equals(left, right);
        }
    }
}