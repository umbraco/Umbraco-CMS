using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using Umbraco.Core.IO;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Represents a Partial View file
    /// </summary>
    [Serializable]
    [DataContract(IsReference = true)]
    internal class PartialView : File
    {
        private readonly Regex _headerMatch = new Regex("^@inherits\\s+?.*$", RegexOptions.Multiline | RegexOptions.Compiled);

        public PartialView(string path)
            : base(path)
        {
            base.Path = path;
        }

        /// <summary>
        /// Boolean indicating whether the file could be validated
        /// </summary>
        /// <returns>True if file is valid, otherwise false</returns>
        public override bool IsValid()
        {
            //Validate extension
            return IOHelper.VerifyFileExtension(Path, new List<string> { "cshtml" });
        }

        public string FileName { get; set; }

        public string SnippetName { get; set; }

        public bool CreateMacro { get; set; }

        public string CodeHeader { get; set; }

        public string ParentFolderName { get; set; }

        public string EditViewFile { get; set; }

        public string BasePath { get; set; }

        public string ReturnUrl { get; set; }

        public bool SaveSucceeded { get; set; }

        internal Regex HeaderMatch
        {
            get { return _headerMatch; }
        }

        internal Attempt<string> TryGetSnippetPath(string fileName)
        {
            var partialViewsFileSystem = new PhysicalFileSystem(BasePath);
            var snippetPath = IOHelper.MapPath(string.Format("{0}/PartialViewMacros/Templates/{1}", SystemDirectories.Umbraco, fileName));

            return partialViewsFileSystem.FileExists(snippetPath)
                ? Attempt<string>.Succeed(snippetPath)
                : Attempt<string>.Fail();
        }
    }
}