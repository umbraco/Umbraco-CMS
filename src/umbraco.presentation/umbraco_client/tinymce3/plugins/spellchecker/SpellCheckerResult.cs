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

        /// <summary>
        /// Gets or sets the spellcheck words
        /// </summary>
        /// <value>The result.</value>
        public List<string> result { get; set; }
        /// <summary>
        /// Gets or sets the id of the initial request
        /// </summary>
        /// <value>The id.</value>
        public string id { get; set; }
        /// <summary>
        /// Gets or sets the error details if there was a problem when performing the spellcheck
        /// </summary>
        /// <value>The error.</value>
        public string error { get; set; }
    }
}
