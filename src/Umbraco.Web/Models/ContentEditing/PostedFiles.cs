using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Umbraco.Web.Models.ContentEditing
{
    /// <summary>
    /// This is used for the response of PostAddFile so that we can analyze the response in a filter and remove the 
    /// temporary files that were created.
    /// </summary>
    [DataContract]
    internal class PostedFiles : IHaveUploadedFiles, INotificationModel
    {
        public PostedFiles()
        {
            UploadedFiles = new List<ContentItemFile>();
            Notifications = new List<Notification>();
        }
        public List<ContentItemFile> UploadedFiles { get; private set; }

        [DataMember(Name = "notifications")]
        public List<Notification> Notifications { get; private set; }
    }
}