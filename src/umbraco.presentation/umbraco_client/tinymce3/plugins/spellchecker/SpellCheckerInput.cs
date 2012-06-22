
using System.IO;
using System;
using System.Web.Script.Serialization;
using System.Collections.Generic;

namespace umbraco.presentation.umbraco_client.tinymce3.plugins.spellchecker
{
    /// <summary>
    /// Object representation of the input from TinyMCE's spellchecker plugin
    /// </summary>
    public class SpellCheckerInput
    {
        private SpellCheckerInput()
        {
            Words = new List<string>();
        }

        /// <summary>
        /// Gets or sets the id from TinyMCE
        /// </summary>
        /// <value>The id.</value>
        public string Id { get; set; }
        /// <summary>
        /// Gets or sets the spellchecking method. eg: checkWords, getSuggestions
        /// </summary>
        /// <value>The method.</value>
        public string Method { get; set; }
        /// <summary>
        /// Gets or sets the language used by the content
        /// </summary>
        /// <value>The language.</value>
        public string Language { get; set; }
        /// <summary>
        /// Gets or sets the words which are to be spell checked
        /// </summary>
        /// <value>The words.</value>
        public List<string> Words { get; set; }


        /// <summary>
        /// Parses the specified stream into the object
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <returns></returns>
        public static SpellCheckerInput Parse(StreamReader inputStream)
        {
            if (inputStream == null)
            {
                throw new ArgumentNullException("stream");
            }
            if (inputStream.EndOfStream)
            {
                throw new ArgumentException("Stream end reached before we started!");
            }
            var jsonString = inputStream.ReadLine();
            var deserialized = (Dictionary<string, object>)new JavaScriptSerializer().DeserializeObject(jsonString);
            
            var input = new SpellCheckerInput();
            input.Id = (string)deserialized["id"];
            input.Method = (string)deserialized["method"];
            input.Language = (string)((object[])deserialized["params"])[0];
            if (((object[])deserialized["params"])[1] is string)
            {
                input.Words.Add((string)((object[])deserialized["params"])[1]);
            }
            else
            {
                var words = ((object[])((object[])deserialized["params"])[1]);
                foreach (var word in words)
                {
                    input.Words.Add((string)word);
                } 
            }
            return input;
        }
    }
}
