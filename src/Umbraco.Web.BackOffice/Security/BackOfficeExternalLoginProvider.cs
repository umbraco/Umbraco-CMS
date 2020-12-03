using System;

namespace Umbraco.Web.BackOffice.Security
{
    /// <summary>
    /// An external login (OAuth) provider for the back office
    /// </summary>
    public class BackOfficeExternalLoginProvider : IEquatable<BackOfficeExternalLoginProvider>
    {
        public BackOfficeExternalLoginProvider(string name, string authenticationType, BackOfficeExternalLoginProviderOptions properties)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            AuthenticationType = authenticationType ?? throw new ArgumentNullException(nameof(authenticationType));
            Options = properties ?? throw new ArgumentNullException(nameof(properties));
        }

        public string Name { get; }
        public string AuthenticationType { get; }
        public BackOfficeExternalLoginProviderOptions Options { get; }

        public override bool Equals(object obj)
        {
            return Equals(obj as BackOfficeExternalLoginProvider);
        }

        public bool Equals(BackOfficeExternalLoginProvider other)
        {
            return other != null &&
                   Name == other.Name &&
                   AuthenticationType == other.AuthenticationType;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name, AuthenticationType);
        }
    }

}
