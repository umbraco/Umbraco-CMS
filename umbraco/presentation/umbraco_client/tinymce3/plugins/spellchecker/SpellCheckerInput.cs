
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

        public string Id { get; set; }
        public string Method { get; set; }
        public string Language { get; set; }
        public List<string> Words { get; set; }


        /// <summary>
        /// Parses the specified stream into the object
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <returns></returns>
        public static SpellCheckerInput Parse(StreamReader stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }
            if (stream.EndOfStream)
            {
                throw new ArgumentException("Stream end reached before we started!");
            }
            var jsonString = stream.ReadLine();
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
