using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;

namespace Umbraco.Web.Mvc
{
    public abstract class UmbracoViewPage : UmbracoViewPage<IPublishedContent>
    { }
}
