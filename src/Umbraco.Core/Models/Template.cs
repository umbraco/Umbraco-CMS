using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Represents a Template file
    /// </summary>
    [Serializable]
    [DataContract(IsReference = true)]
    public class Template : File, ITemplate
    {
        private readonly string _alias;
        private readonly string _name;

        internal Template(string path)
            : base(path)
        {
            base.Path = path;
            ParentId = -1;
        }

        public Template(string path, string name, string alias)
            : base(path)
        {
            base.Path = path;
            ParentId = -1;
            _name = name.Replace("/", ".").Replace("\\", "");
            _alias = alias.ToSafeAlias();
        }

        [DataMember]
        internal int CreatorId { get; set; }

        [DataMember]
        internal int Level { get; set; }

        [DataMember]
        internal int SortOrder { get; set; }

        [DataMember]
        internal int ParentId { get; set; }

        [DataMember]
        internal string NodePath { get; set; }

        [DataMember]
        internal Lazy<int> MasterTemplateId { get; set; }

        [DataMember]
        internal string MasterTemplateAlias { get; set; }

        [DataMember]
        public override string Alias
        {
            get
            {
                return _alias;
            }
        }

        [DataMember]
        public override string Name
        {
            get
            {
                return _name;
            }
        }

        /// <summary>
        /// Returns the <see cref="RenderingEngine"/> that corresponds to the template file
        /// </summary>
        /// <returns><see cref="RenderingEngine"/></returns>
        public RenderingEngine GetTypeOfRenderingEngine()
        {
            if(Path.EndsWith("cshtml") || Path.EndsWith("vbhtml"))
                return RenderingEngine.Mvc;

            return RenderingEngine.WebForms;
        }

        /// <summary>
        /// Boolean indicating whether the file could be validated
        /// </summary>
        /// <returns>True if file is valid, otherwise false</returns>
        public override bool IsValid()
        {
            var exts = new List<string>();
            if (UmbracoSettings.DefaultRenderingEngine == RenderingEngine.Mvc)
            {
                exts.Add("cshtml");
                exts.Add("vbhtml");
            }
            else
            {
                exts.Add(UmbracoSettings.UseAspNetMasterPages ? "masterpage" : "aspx");
            }

            var dirs = SystemDirectories.Masterpages;
            if (UmbracoSettings.DefaultRenderingEngine == RenderingEngine.Mvc)
                dirs += "," + SystemDirectories.MvcViews;

            //Validate file
            var validFile = IOHelper.ValidateEditPath(Path, dirs.Split(','));

            //Validate extension
            var validExtension = IOHelper.ValidateFileExtension(Path, exts);

            return validFile && validExtension;
        }

        /// <summary>
        /// Method to call when Entity is being saved
        /// </summary>
        /// <remarks>Created date is set and a Unique key is assigned</remarks>
        internal override void AddingEntity()
        {
            base.AddingEntity();

            if (Key == Guid.Empty)
                Key = Guid.NewGuid();
        }
    }
}