using System;
using System.Collections.Generic;
using System.Web;

// NB: This class was moved out of the client tinymce folder to aid with upgrades
// but we'll keep the old namespace to make things easier for now (MB)
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

    /// <summary>
    /// Object used to deserialise the Google Json response
    /// </summary>
    public class JsonSpellCheckerResult
    {
        public GoogleResponseResult result { get; set; }

        public class GoogleResponseResult
        {
            public GoogleResponseSpellingCheckResponse spellingCheckResponse { get; set; }

        }
        public class GoogleResponseSpellingCheckResponse
        {
            public List<GoogleResponseMisspelling> misspellings { get; set; }

        }
        public class GoogleResponseMisspelling
        {
            public int charStart { get; set; }
            public int charLength { get; set; }
            public List<GoogleResponseSuggestion> suggestions { get; set; }

        }
        public class GoogleResponseSuggestion
        {
            public string suggestion { get; set; }
        }
    }
}
