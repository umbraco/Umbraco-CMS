using System;
using System.Reflection;
using System.Runtime.Serialization;
using Umbraco.Core.Models.EntityBase;

namespace Umbraco.Core.Models
{
    [Serializable]
    [DataContract(IsReference = true)]
    public class RedirectUrl : Entity, IRedirectUrl
    {
        public RedirectUrl()
        {
            CreateDateUtc = DateTime.UtcNow;
        }

        private static readonly Lazy<PropertySelectors> Ps = new Lazy<PropertySelectors>();

        private class PropertySelectors
        {
            public readonly PropertyInfo ContentIdSelector = ExpressionHelper.GetPropertyInfo<RedirectUrl, int>(x => x.ContentId);
            public readonly PropertyInfo CreateDateUtcSelector = ExpressionHelper.GetPropertyInfo<RedirectUrl, DateTime>(x => x.CreateDateUtc);
            public readonly PropertyInfo UrlSelector = ExpressionHelper.GetPropertyInfo<RedirectUrl, string>(x => x.Url);
        }

        private int _contentId;
        private DateTime _createDateUtc;
        private string _url;

        public int ContentId
        {
            get { return _contentId; }
            set
            {
                SetPropertyValueAndDetectChanges(value, ref _contentId, Ps.Value.ContentIdSelector);
            }
        }

        public DateTime CreateDateUtc
        {
            get {  return _createDateUtc; }
            set
            {
                SetPropertyValueAndDetectChanges(value, ref _createDateUtc, Ps.Value.CreateDateUtcSelector);
            }
        }

        public string Url
        {
            get { return _url; }
            set
            {
                SetPropertyValueAndDetectChanges(value, ref _url, Ps.Value.UrlSelector);
            }
        }
    }
}