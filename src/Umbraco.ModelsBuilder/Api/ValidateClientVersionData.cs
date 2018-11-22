using System;
using System.Runtime.Serialization;

namespace Umbraco.ModelsBuilder.Api
{
    [DataContract]
    public class ValidateClientVersionData
    {
        // issues 32, 34... problems when serializing versions
        //
        // make sure System.Version objects are transfered as strings
        // depending on the JSON serializer version, it looks like versions are causing issues
        // see
        // http://stackoverflow.com/questions/13170386/why-system-version-in-json-string-does-not-deserialize-correctly
        //
        // if the class is marked with [DataContract] then only properties marked with [DataMember]
        // are serialized and the rest is ignored, see
        // http://www.asp.net/web-api/overview/formats-and-model-binding/json-and-xml-serialization

        [DataMember]
        public string ClientVersionString
        {
            get { return VersionToString(ClientVersion); }
            set { ClientVersion = ParseVersion(value, false, "client"); }
        }

        [DataMember]
        public string MinServerVersionSupportingClientString
        {
            get { return VersionToString(MinServerVersionSupportingClient); }
            set { MinServerVersionSupportingClient = ParseVersion(value, true, "minServer"); }
        }

        // not serialized
        public Version ClientVersion { get; set; }
        public Version MinServerVersionSupportingClient { get; set; }

        private static string VersionToString(Version version)
        {
            return version?.ToString() ?? "0.0.0.0";
        }

        private static Version ParseVersion(string value, bool canBeNull, string name)
        {
            if (string.IsNullOrWhiteSpace(value) && canBeNull)
                return null;

            Version version;
            if (Version.TryParse(value, out version))
                return version;

            throw new ArgumentException($"Failed to parse \"{value}\" as {name} version.");
        }

        public virtual bool IsValid => ClientVersion != null;
    }
}