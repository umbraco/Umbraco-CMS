using System;
using System.Web;
using System.IO;
using System.Web.Script.Serialization;
using System.Net;
using System.Text;
using System.Xml;
using System.Collections.Generic;
using Newtonsoft.Json;

// NB: This class was moved out of the client tinymce folder to aid with upgrades
// but we'll keep the old namespace to make things easier for now (MB)
namespace umbraco.presentation.umbraco_client.tinymce3.plugins.spellchecker
{
    public class GoogleSpellChecker : SpellChecker, IHttpHandler
    {
        private static string SendRequest(string lang, string data)
        {
            string googleResponse;
            string requestUriString = "https://www.googleapis.com:443/rpc";
            var requestData = new Dictionary<string, object>();
            var requestParams = new Dictionary<string, object>();
            requestParams.Add("language", lang);
            requestParams.Add("text", data);
            requestParams.Add("key", "AIzaSyCLlKc60a3z7lo8deV-hAyDU7rHYgL4HZg");
            requestData.Add("method", "spelling.check");
            requestData.Add("apiVersion", "v2");
            requestData.Add("params", requestParams);
            string jsonString = new JavaScriptSerializer().Serialize(requestData);
            StreamReader reader = null;
            HttpWebResponse response = null;
            Stream requestStream = null;
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(requestUriString);
                request.KeepAlive = false;
                request.Method = "POST";
                request.ContentType = "application/json";
                request.ContentLength = jsonString.Length;
                requestStream = request.GetRequestStream();
                byte[] bytes = new ASCIIEncoding().GetBytes(jsonString);
                requestStream.Write(bytes, 0, bytes.Length);
                response = (HttpWebResponse)request.GetResponse();
                reader = new StreamReader(response.GetResponseStream());
                googleResponse = reader.ReadToEnd();
            }
            finally
            {
                if (requestStream != null)
                {
                    requestStream.Close();
                }
                if (reader != null)
                {
                    reader.Close();
                }
                if (response != null)
                {
                    response.Close();
                }
            }
            return googleResponse;
        }

        /// <summary>
        /// Checks all the words submitted against Google for spelling
        /// </summary>
        /// <param name="language">The language code.</param>
        /// <param name="words">The words to be checked.</param>
        /// <returns></returns>
        public override SpellCheckerResult CheckWords(string language, string[] words)
        {
            string data = string.Join(" ", words); //turn them into a space-separated string as that's what google takes
            string json = SendRequest(language, data);            
            var jsonRes = JsonConvert.DeserializeObject<JsonSpellCheckerResult>(json);

            var res = new SpellCheckerResult();
            // Get list of misspelled words
            if (jsonRes.Result != null && jsonRes.Result.SpellingCheckResponse != null)
            {
                foreach (var misspelling in jsonRes.Result.SpellingCheckResponse.Misspellings)
                {
                    res.result.Add(data.Substring(misspelling.CharStart, misspelling.CharLength));
                }
            }

            return res;
        }

        /// <summary>
        /// Gets the suggested spelling for a misspelt word
        /// </summary>
        /// <param name="language">The language the word is.</param>
        /// <param name="word">The word that is misspelt.</param>
        /// <returns></returns>
        public override SpellCheckerResult GetSuggestions(string language, string word)
        {
            string json = SendRequest(language, word);
            var jsonRes = JsonConvert.DeserializeObject<JsonSpellCheckerResult>(json);

            var res = new SpellCheckerResult();
            // Get list of suggestions
            if (jsonRes.Result != null && jsonRes.Result.SpellingCheckResponse != null)
            {
                foreach (var misspelling in jsonRes.Result.SpellingCheckResponse.Misspellings)
                {
                    foreach (var suggestion in misspelling.Suggestions)
                    {
                        res.result.Add(suggestion.Suggestion);
                    }
                }
            }

            return res;
        }

        #region IHttpHandler Members

        public bool IsReusable
        {
            get { return false; }
        }

        public void ProcessRequest(HttpContext context)
        {
            SpellCheckerInput input = SpellCheckerInput.Parse(new StreamReader(context.Request.InputStream));
            SpellCheckerResult suggestions = null;
            switch (input.Method)
            {
                case "checkWords":
                    suggestions = CheckWords(input.Language, input.Words.ToArray());
                    break;

                case "getSuggestions":
                    suggestions = GetSuggestions(input.Language, input.Words[0]);
                    break;

                default:
                    suggestions = new SpellCheckerResult();
                    break;
            }

            suggestions.id = input.Id;
            JavaScriptSerializer ser = new JavaScriptSerializer();
            var res = ser.Serialize(suggestions);
            context.Response.Write(res);
        }

        #endregion
    }
}
