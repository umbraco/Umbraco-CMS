using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Umbraco.Web.Templates;

namespace Umbraco.Web.PropertyEditors
{
    class TextValueConverterHelper
    {
        // ensures string value sources are parsed for {localLink} and urls are resolved correctly
        // fixme - but then that one should get "previewing" too?
        public static string ParseStringValueSource(string stringValueSource)
        {
            return TemplateUtilities.ResolveUrlsFromTextString(TemplateUtilities.ParseInternalLinks(stringValueSource));
        }
    }
}
