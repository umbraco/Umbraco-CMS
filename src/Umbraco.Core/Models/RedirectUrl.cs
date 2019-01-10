using System;
using System.Reflection;
using System.Runtime.Serialization;
using Umbraco.Core.Models.Entities;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Implements <see cref="IRedirectUrl"/>.
    /// </summary>
    [Serializable]
    [DataContract(IsReference = true)]
    public class RedirectUrl : EntityBase, IRedirectUrl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RedirectUrl"/> class.
        /// </summary>
        public RedirectUrl()
        {
            CreateDateUtc = DateTime.UtcNow;
        }

        private static readonly Lazy<PropertySelectors> Ps = new Lazy<PropertySelectors>();

        // ReSharper disable once ClassNeverInstantiated.Local
        private class PropertySelectors
        {
            public readonly PropertyInfo ContentIdSelector = ExpressionHelper.GetPropertyInfo<RedirectUrl, int>(x => x.ContentId);
            public readonly PropertyInfo ContentKeySelector = ExpressionHelper.GetPropertyInfo<RedirectUrl, Guid>(x => x.ContentKey);
            public readonly PropertyInfo CreateDateUtcSelector = ExpressionHelper.GetPropertyInfo<RedirectUrl, DateTime>(x => x.CreateDateUtc);
            public readonly PropertyInfo CultureSelector = ExpressionHelper.GetPropertyInfo<RedirectUrl, string>(x => x.Culture);
            public readonly PropertyInfo UrlSelector = ExpressionHelper.GetPropertyInfo<RedirectUrl, string>(x => x.Url);
        }

        private int _contentId;
        private Guid _contentKey;
        private DateTime _createDateUtc;
        private string _culture;
        private string _url;

        /// <inheritdoc />
        public int ContentId
        {
            get { return _contentId; }
            set { SetPropertyValueAndDetectChanges(value, ref _contentId, Ps.Value.ContentIdSelector); }
        }

        /// <inheritdoc />
        public Guid ContentKey
        {
            get { return _contentKey; }
            set { SetPropertyValueAndDetectChanges(value, ref _contentKey, Ps.Value.ContentKeySelector); }
        }

        /// <inheritdoc />
        public DateTime CreateDateUtc
        {
            get {  return _createDateUtc; }
            set { SetPropertyValueAndDetectChanges(value, ref _createDateUtc, Ps.Value.CreateDateUtcSelector); }
        }

        /// <inheritdoc />
        public string Culture
        {
            get { return _culture; }
            set { SetPropertyValueAndDetectChanges(value, ref _culture, Ps.Value.CultureSelector); }
        }

        /// <inheritdoc />
        public string Url
        {
            get { return _url; }
            set { SetPropertyValueAndDetectChanges(value, ref _url, Ps.Value.UrlSelector); }
        }
    }
}
