using System;
using System.Collections.Generic;
using System.Web;

namespace umbraco.presentation.umbraco_client.tinymce3.plugins.spellchecker
{
    /// <summary>
    /// Object which will be returned to TinyMCE from the spellchecker
    /// </summary>
    public class SpellCheckerResult
    {
        public SpellCheckerResult()
        {
            result = new List<string>();
        }

        public List<string> result { get; set; }
        public string id { get; set; }
        public string error { get; set; }
    }
}
