using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core;

namespace Umbraco.Web.Models.ContentEditing
{
    [DataContract(Name = "scriptFile", Namespace = "")]
    public class CodeFileDisplay : INotificationModel, IValidatableObject
    {
        public CodeFileDisplay()
        {
            Notifications = new List<Notification>();
        }

        /// <summary>
        /// VirtualPath is the path to the file on disk
        /// /views/partials/file.cshtml
        /// </summary>
        [DataMember(Name = "virtualPath", IsRequired = true)]
        public string VirtualPath { get; set; }

        /// <summary>
        /// Path represents the path used by the backoffice tree
        /// For files stored on disk, this is a URL encoded, comma separated
        /// path to the file, always starting with -1.
        ///
        /// -1,Partials,Parials%2FFolder,Partials%2FFolder%2FFile.cshtml
        /// </summary>
        [DataMember(Name = "path")]
        [ReadOnly(true)]
        public string Path { get; set; }

        [DataMember(Name = "name", IsRequired = true)]
        public string Name { get; set; }

        [DataMember(Name = "content", IsRequired = true)]
        public string Content { get; set; }

        [DataMember(Name = "fileType", IsRequired = true)]
        public string FileType { get; set; }

        [DataMember(Name = "snippet")]
        [ReadOnly(true)]
        public string Snippet { get; set; }

        [DataMember(Name = "id")]
        [ReadOnly(true)]
        public string Id { get; set; }

        public List<Notification> Notifications { get; private set; }

        /// <summary>
        /// Some custom validation is required for valid file names
        /// </summary>
        /// <param name="validationContext"></param>
        /// <returns></returns>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var illegalChars = System.IO.Path.GetInvalidFileNameChars();
            if (Name.ContainsAny(illegalChars))
            {
                yield return new ValidationResult(
                    "The file name cannot contain illegal characters",
                    new[] { "Name" });
            }
            else if (System.IO.Path.GetFileNameWithoutExtension(Name).IsNullOrWhiteSpace())
            {
                yield return new ValidationResult(
                    "The file name cannot be empty",
                    new[] { "Name" });
            }
        }
    }
}
