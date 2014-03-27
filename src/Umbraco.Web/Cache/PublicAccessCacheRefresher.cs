using System;
using System.Xml;
using Newtonsoft.Json;
using umbraco.cms.businesslogic.web;
using Umbraco.Core;
using Umbraco.Core.Cache;

namespace Umbraco.Web.Cache
{
    public sealed class PublicAccessCacheRefresher : JsonCacheRefresherBase<PublicAccessCacheRefresher>
    {
        #region Static helpers

        internal static JsonPayload DeserializeFromJsonPayload(string json)
        {
            return JsonConvert.DeserializeObject<JsonPayload>(json);
        }

        internal static string SerializeToJsonPayload(XmlDocument doc)
        {
            return JsonConvert.SerializeObject(FromXml(doc));
        }

        internal static JsonPayload FromXml(XmlDocument doc)
        {
            if (doc == null) return null;

            var payload = new JsonPayload
            {
                XmlContent = doc.OuterXml
            };
            return payload;
        }

        #endregion

        #region Sub classes

        internal class JsonPayload
        {
            public string XmlContent { get; set; }
        }

        #endregion

        protected override PublicAccessCacheRefresher Instance
        {
            get { return this; }
        }

        public override Guid UniqueIdentifier
        {
            get { return new Guid(DistributedCache.PublicAccessCacheRefresherId); }
        }

        public override string Name
        {
            get { return "Public access cache refresher"; }
        }

        public override void Refresh(string jsonPayload)
        {
            if (jsonPayload.IsNullOrWhiteSpace()) return;
            var deserialized = DeserializeFromJsonPayload(jsonPayload);
            if (deserialized == null) return;
            var xDoc = new XmlDocument();
            xDoc.LoadXml(deserialized.XmlContent);
            ClearCache(xDoc);
            base.Refresh(jsonPayload);
        }

        private void ClearCache(XmlDocument xDoc)
        {
            Access.UpdateInMemoryDocument(xDoc);
        }
    }
}