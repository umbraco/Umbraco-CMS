using System;
using System.Web;
using System.IO;
using System.Web.Script.Serialization;
using System.Net;
using System.Text;
using System.Xml;

// NB: This class was moved out of the client tinymce folder to aid with upgrades
// but we'll keep the old namespace to make things easier for now (MB)
namespace umbraco.presentation.umbraco_client.tinymce3.plugins.spellchecker
{
    public class GoogleSpellChecker : SpellChecker, IHttpHandler
    {
        private static string SendRequest(string lang, string data)
        {
            string googleResponse;
            string requestUriString = string.Format("https://www.google.com:443/tbproxy/spell?lang={0}&hl={0}", lang);
            string s = string.Format("<?xml version=\"1.0\" encoding=\"utf-8\" ?><spellrequest textalreadyclipped=\"0\" ignoredups=\"0\" ignoredigits=\"1\" ignoreallcaps=\"1\"><text>{0}</text></spellrequest>", HttpContext.Current.Server.UrlEncode(data));
            StreamReader reader = null;
            HttpWebResponse response = null;
            Stream requestStream = null;
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(requestUriString);
                request.KeepAlive = false;
                request.Method = "POST";
                request.ContentType = "application/PTI26";
                request.ContentLength = s.Length;
                WebHeaderCollection headers = request.Headers;
                headers.Add("MIME-Version: 1.0");
                headers.Add("Request-number: 1");
                headers.Add("Document-type: Request");
                headers.Add("Interface-Version: Test 1.4");
                requestStream = request.GetRequestStream();
                byte[] bytes = new ASCIIEncoding().GetBytes(s);
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
            XmlDocument document = new XmlDocument();
            string data = string.Join(" ", words); //turn them into a space-separated string as that's what google takes
            string xml = SendRequest(language, data);
            document.LoadXml(xml);

            var res = new SpellCheckerResult();
            foreach (XmlNode node in document.SelectNodes("//c")) //go through each of the incorrectly spelt words
            {
                XmlElement element = (XmlElement)node;
                res.result.Add(data.Substring(Convert.ToInt32(element.GetAttribute("o")), Convert.ToInt32(element.GetAttribute("l"))));
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
            XmlDocument document = new XmlDocument();
            string xml = SendRequest(language, word);
            document.LoadXml(xml);
            var res = new SpellCheckerResult();
            foreach (XmlNode node in document.SelectNodes("//c")) //select each incorrectly spelt work
            {
                XmlElement element = (XmlElement)node;
                foreach (string s in element.InnerText.Split(new char[] { '\t' })) //they are tab-separated for suggestions
                {
                    if (!string.IsNullOrEmpty(s))
                    {
                        res.result.Add(s);
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
