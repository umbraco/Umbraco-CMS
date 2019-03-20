﻿using System;
using System.Runtime.Serialization;

namespace Umbraco.Web.Routing
{
    /// <summary>
    /// Represents infos for a url.
    /// </summary>
    [DataContract(Name = "urlInfo", Namespace = "")]
    public class UrlInfo : IEquatable<UrlInfo>
    {

        /// <summary>
        /// Creates a <see cref="UrlInfo"/> instance representing a true url.
        /// </summary>
        public static UrlInfo Url(string text, string culture = null) => new UrlInfo(text, true, culture);

        /// <summary>
        /// Creates a <see cref="UrlInfo"/> instance representing a message.
        /// </summary>
        public static UrlInfo Message(string text, string culture = null) => new UrlInfo(text, false, culture);

        /// <summary>
        /// Initializes a new instance of the <see cref="UrlInfo"/> class.
        /// </summary>
        public UrlInfo(string text, bool isUrl, string culture)
        {
            if (string.IsNullOrWhiteSpace(text)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(text));
            IsUrl = isUrl;
            Text = text;
            Culture = culture;
        }

        /// <summary>
        /// Gets the culture.
        /// </summary>
        [DataMember(Name = "culture")]
        public string Culture { get; }

        /// <summary>
        /// Gets a value indicating whether the url is a true url.
        /// </summary>
        /// <remarks>Otherwise, it is a message.</remarks>
        [DataMember(Name = "isUrl")]
        public bool IsUrl { get; }

        /// <summary>
        /// Gets the text, which is either the url, or a message.
        /// </summary>
        [DataMember(Name = "text")]
        public string Text { get; }

        /// <summary>
        /// Checks equality 
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        /// <remarks>
        /// Compare both culture and Text as invariant strings since URLs are not case sensitive, nor are culture names within Umbraco
        /// </remarks>
        public bool Equals(UrlInfo other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(Culture, other.Culture, StringComparison.InvariantCultureIgnoreCase) && IsUrl == other.IsUrl && string.Equals(Text, other.Text, StringComparison.InvariantCultureIgnoreCase);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((UrlInfo) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Culture != null ? StringComparer.InvariantCultureIgnoreCase.GetHashCode(Culture) : 0);
                hashCode = (hashCode * 397) ^ IsUrl.GetHashCode();
                hashCode = (hashCode * 397) ^ (Text != null ? StringComparer.InvariantCultureIgnoreCase.GetHashCode(Text) : 0);
                return hashCode;
            }
        }

        public static bool operator ==(UrlInfo left, UrlInfo right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(UrlInfo left, UrlInfo right)
        {
            return !Equals(left, right);
        }

        public override string ToString()
        {
            return Text;
        }
    }
}
