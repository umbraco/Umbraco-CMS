using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Web.Mvc;

namespace Umbraco.TestData
{
    public class SegmentTestController : SurfaceController
    {

        public ActionResult EnableDocTypeSegments(string alias, string propertyTypeAlias)
        {
            if (ConfigurationManager.AppSettings["Umbraco.TestData.Enabled"] != "true")
                return HttpNotFound();

            var ct = Services.ContentTypeService.Get(alias);
            if (ct == null)
                return Content($"No document type found by alias {alias}");

            var propType = ct.PropertyTypes.FirstOrDefault(x => x.Alias == propertyTypeAlias);
            if (propType == null)
                return Content($"The document type {alias} does not have a property type {propertyTypeAlias ?? "null"}");

            if (ct.Variations.VariesBySegment())
                return Content($"The document type {alias} already allows segments, nothing has been changed");

            ct.SetVariesBy(ContentVariation.Segment);
            propType.SetVariesBy(ContentVariation.Segment);

            Services.ContentTypeService.Save(ct);
            return Content($"The document type {alias} and property type {propertyTypeAlias} now allows segments");
        }

        public ActionResult DisableDocTypeSegments(string alias)
        {
            if (ConfigurationManager.AppSettings["Umbraco.TestData.Enabled"] != "true")
                return HttpNotFound();

            var ct = Services.ContentTypeService.Get(alias);
            if (ct == null)
                return Content($"No document type found by alias {alias}");

            if (!ct.VariesBySegment())
                return Content($"The document type {alias} does not allow segments, nothing has been changed");

            ct.SetVariesBy(ContentVariation.Segment, false);

            Services.ContentTypeService.Save(ct);
            return Content($"The document type {alias} no longer allows segments");
        }

        public ActionResult AddSegmentData(int contentId, string propertyAlias, string value, string segment, string culture = null)
        {
            var content = Services.ContentService.GetById(contentId);
            if (content == null)
                return Content($"No content found by id {contentId}");

            if (propertyAlias.IsNullOrWhiteSpace() || !content.HasProperty(propertyAlias))
                return Content($"The content by id {contentId} does not contain a property with alias {propertyAlias ?? "null"}");

            if (content.ContentType.VariesByCulture() && culture.IsNullOrWhiteSpace())
                return Content($"The content by id {contentId} varies by culture but no culture was specified");

            if (value.IsNullOrWhiteSpace())
                return Content("'value' cannot be null");

            if (segment.IsNullOrWhiteSpace())
                return Content("'segment' cannot be null");

            content.SetValue(propertyAlias, value, culture, segment);
            Services.ContentService.Save(content);

            return Content($"Segment value has been set on content {contentId} for property {propertyAlias}");
        }
    }
}
