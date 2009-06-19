using System;
using System.Collections.Generic;
using System.Web;

namespace umbraco.presentation.umbraco_client.tinymce3.plugins.spellchecker
{
    /// <summary>
    /// Base class for a spellchecker for TinyMCE
    /// </summary>
    public abstract class SpellChecker
    {
        /// <summary>
        /// Checks all the words submitted
        /// </summary>
        /// <param name="language">The language.</param>
        /// <param name="words">The words.</param>
        /// <returns></returns>
        public abstract SpellCheckerResult CheckWords(string language, string[] words);
        /// <summary>
        /// Gets the suggestions for a single word
        /// </summary>
        /// <param name="language">The language.</param>
        /// <param name="word">The word.</param>
        /// <returns></returns>
        public abstract SpellCheckerResult GetSuggestions(string language, string word);
    }
}
