using System.Collections.Generic;
using System;

namespace Umbraco.Core.Models.Blocks
{
    public struct ContentAndSettingsReference : IEquatable<ContentAndSettingsReference>
    {
        public ContentAndSettingsReference(Udi contentUdi, Udi settingsUdi)
        {
            ContentUdi = contentUdi ?? throw new ArgumentNullException(nameof(contentUdi));
            SettingsUdi = settingsUdi;
        }

        public Udi ContentUdi { get; }
        public Udi SettingsUdi { get; }

        public override bool Equals(object obj)
        {
            return obj is ContentAndSettingsReference reference && Equals(reference);
        }

        public bool Equals(ContentAndSettingsReference other)
        {
            return EqualityComparer<Udi>.Default.Equals(ContentUdi, other.ContentUdi) &&
                   EqualityComparer<Udi>.Default.Equals(SettingsUdi, other.SettingsUdi);
        }

        public override int GetHashCode()
        {
            var hashCode = 272556606;
            hashCode = hashCode * -1521134295 + EqualityComparer<Udi>.Default.GetHashCode(ContentUdi);
            hashCode = hashCode * -1521134295 + EqualityComparer<Udi>.Default.GetHashCode(SettingsUdi);
            return hashCode;
        }

        public static bool operator ==(ContentAndSettingsReference left, ContentAndSettingsReference right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ContentAndSettingsReference left, ContentAndSettingsReference right)
        {
            return !(left == right);
        }
    }
}
