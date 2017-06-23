using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Web.WebApi.Filters;
using Umbraco.Core.Cache;
using Umbraco.Core.Logging;

namespace Umbraco.Web.Editors
{
    public class HelpController : UmbracoAuthorizedJsonController
    {

        /// <summary>
        /// Fetches available lessons for a given section from our.umbaco.org
        /// </summary>
        /// <param name="section">Name of the documentation section to fetch from, ex: "getting-started", "getting-started/backoffice" </param>
        /// <param name="baseUrl">The url to retrieve from, by default https://our.umbraco.org</param>
        /// <returns></returns>
        [ValidateAngularAntiForgeryToken]
        public async Task<IEnumerable<Lesson>> GetLessons(string path, string baseUrl = "https://our.umbraco.org")
        {
            //We need the umbraco context to fetch the currrent user and version
            var context = UmbracoContext;

            //this only works in the context of a umbraco backoffice request
            if (context == null)
                throw new HttpResponseException(HttpStatusCode.InternalServerError);

            //information for the request, so we could in the future filter by user, allowed sections, langugae and user-type
            var user = Security.CurrentUser;
            var userType = user.UserType.Alias;
            var allowedSections = string.Join(",", user.AllowedSections);
            var language = user.Language;
            var version = UmbracoVersion.GetSemanticVersion().ToSemanticString();

            //construct the url and cache key
            var url = string.Format(baseUrl + "/Umbraco/Documentation/Lessons/GetDocsForPath?path={0}&userType={1}&allowedSections={2}&lang={3}&version={4}", path, userType, allowedSections, language, version);
            var key = "umbraco-lessons-" + userType + language + allowedSections.Replace(",", "-") + path;

            //try and find an already cached version of this request
            var content = ApplicationContext.ApplicationCache.RuntimeCache.GetCacheItem<List<Lesson>>(key);


            var result = new List<Lesson>();
            if (content != null)
            {
                result = content;
            }
            else
            {
                //content is null, go get it
                try
                {
                    using (var web = new HttpClient())
                    {
                        //fetch dashboard json and parse to JObject
                        var json = await web.GetStringAsync(url);
                        result = JsonConvert.DeserializeObject<IEnumerable<Lesson>>(json).ToList();
                    }

                    ApplicationContext.ApplicationCache.RuntimeCache.InsertCacheItem<List<Lesson>>(key, () => result, new TimeSpan(0, 30, 0));
                }
                catch (HttpRequestException ex)
                {
                    LogHelper.Debug<HelpController>(string.Format("Error getting lesson content from '{0}': {1}\n{2}", url, ex.Message, ex.InnerException));

                    //it's still new JObject() - we return it like this to avoid error codes which triggers UI warnings
                    ApplicationContext.ApplicationCache.RuntimeCache.InsertCacheItem<List<Lesson>>(key, () => result, new TimeSpan(0, 5, 0));
                }
            }

            return result;
        }

        [ValidateAngularAntiForgeryToken]
        public async Task< IEnumerable<LessonStep> > GetLessonSteps(string path, string baseUrl = "https://our.umbraco.org")
        {
            var url = string.Format(baseUrl + "/Umbraco/Documentation/Lessons/GetStepsForPath?path={0}", path);
            using (var web = new HttpClient())
            {
                //fetch dashboard json and parse to JObject
                var json = await web.GetStringAsync(url);
                return JsonConvert.DeserializeObject< List<LessonStep> >(json);
            }


        }

        public async Task< List<HelpPage>> GetContextHelpForPage(string section, string tree,string baseUrl = "https://our.umbraco.org")
        {
            var url = string.Format(baseUrl + "/Umbraco/Documentation/Lessons/GetContextHelpDocs?sectionAlias={0}&treeAlias={1}", section, tree);
            using (var web = new HttpClient())
            {
                //fetch dashboard json and parse to JObject
                var json = await web.GetStringAsync(url);
                var result =  JsonConvert.DeserializeObject<List<HelpPage>>(json);
                if (result != null)
                    return result;

                return new List<HelpPage>();
            }
        }
    }



    [DataContract(Name = "lesson")]
    public class Lesson
    {
        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "path")]
        public string Path { get; set; }

        [DataMember(Name = "level")]
        public string Level { get; set; }

        [DataMember(Name = "url")]
        public string Url { get; set; }

        [DataMember(Name = "directories")]
        public IEnumerable<Lesson> Directories { get; set; }
    }


    [DataContract(Name = "LesssonStep")]
    public class LessonStep
    {
        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "content")]
        public string Content { get; set; }

    }

    [DataContract(Name = "HelpPage")]
    public class HelpPage
    {
        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "description")]
        public string Description { get; set; }

        [DataMember(Name = "url")]
        public string Url { get; set; }

        [DataMember(Name = "type")]
        public string Type { get; set; }

    }
}
