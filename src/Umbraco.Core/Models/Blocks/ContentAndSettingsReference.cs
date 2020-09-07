﻿using System;
using System.Collections.Generic;

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

        public override bool Equals(object obj) => obj is ContentAndSettingsReference reference && Equals(reference);

        public bool Equals(ContentAndSettingsReference other) => other != null
            && EqualityComparer<Udi>.Default.Equals(ContentUdi, other.ContentUdi)
            && EqualityComparer<Udi>.Default.Equals(SettingsUdi, other.SettingsUdi);

        public override int GetHashCode() => (ContentUdi, SettingsUdi).GetHashCode();

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
