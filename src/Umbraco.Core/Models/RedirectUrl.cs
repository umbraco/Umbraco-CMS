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

        private static readonly PropertyInfo ContentIdSelector = ExpressionHelper.GetPropertyInfo<RedirectUrl, int>(x => x.ContentId);
        private static readonly PropertyInfo CreateDateUtcSelector = ExpressionHelper.GetPropertyInfo<RedirectUrl, DateTime>(x => x.CreateDateUtc);
        private static readonly PropertyInfo UrlSelector = ExpressionHelper.GetPropertyInfo<RedirectUrl, string>(x => x.Url);

        private int _contentId;
        private DateTime _createDateUtc;
        private string _url;

        public int ContentId
        {
            get { return _contentId; }
            set
            {
                SetPropertyValueAndDetectChanges(o =>
                {
                    return _contentId = value;
                }, _contentId, ContentIdSelector);
            }
        }

        public DateTime CreateDateUtc
        {
            get {  return _createDateUtc; }
            set
            {
                SetPropertyValueAndDetectChanges(o =>
                {
                    return _createDateUtc = value;
                }, _createDateUtc, CreateDateUtcSelector);
            }
        }

        public string Url
        {
            get { return _url; }
            set
            {
                SetPropertyValueAndDetectChanges(o =>
                {
                    return _url = value;
                }, _url, UrlSelector);
            }
        }
    }
}