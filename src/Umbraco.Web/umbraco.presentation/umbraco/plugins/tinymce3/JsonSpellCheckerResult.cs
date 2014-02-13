using System.Collections.Generic;
using Newtonsoft.Json;

namespace umbraco.presentation.umbraco_client.tinymce3.plugins.spellchecker
{
    /// <summary>
    /// Object used to deserialise the Google Json response
    /// </summary>
    internal class JsonSpellCheckerResult
    {
        [JsonProperty("result")]
        public GoogleResponseResult Result { get; set; }

        public class GoogleResponseResult
        {
            [JsonProperty("spellingCheckResponse")]
            public GoogleResponseSpellingCheckResponse SpellingCheckResponse { get; set; }

        }
        public class GoogleResponseSpellingCheckResponse
        {
            [JsonProperty("misspellings")]
            public List<GoogleResponseMisspelling> Misspellings { get; set; }

        }
        public class GoogleResponseMisspelling
        {
            [JsonProperty("charStart")]
            public int CharStart { get; set; }

            [JsonProperty("charLength")]
            public int CharLength { get; set; }

            [JsonProperty("suggestions")]
            public List<GoogleResponseSuggestion> Suggestions { get; set; }

        }
        public class GoogleResponseSuggestion
        {
            [JsonProperty("suggestion")]
            public string Suggestion { get; set; }
        }
    }
}