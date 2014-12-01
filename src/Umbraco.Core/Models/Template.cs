using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Strings;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Represents a Template file
    /// </summary>
    [Serializable]
    [DataContract(IsReference = true)]
    public class Template : File, ITemplate
    {
        private string _alias;
        private string _name;
        private string _masterTemplateAlias;
        private Lazy<int> _masterTemplateId;

        private static readonly PropertyInfo MasterTemplateAliasSelector = ExpressionHelper.GetPropertyInfo<Template, string>(x => x.MasterTemplateAlias);
        private static readonly PropertyInfo MasterTemplateIdSelector = ExpressionHelper.GetPropertyInfo<Template, Lazy<int>>(x => x.MasterTemplateId);

        public Template(string name, string alias)
            : base(string.Empty)
        {
            _name = name;
            _alias = alias.ToCleanString(CleanStringType.UnderscoreAlias);
        }

        public Template(string path, string name, string alias)
            : base(path)
        {
            base.Path = path;
            _name = name;
            _alias = alias.ToCleanString(CleanStringType.UnderscoreAlias);
        }

        [DataMember]
        public Lazy<int> MasterTemplateId
        {
            get { return _masterTemplateId; }
            set
            {
                SetPropertyValueAndDetectChanges(o =>
                {
                    _masterTemplateId = value;
                    return _masterTemplateId;
                }, _masterTemplateId, MasterTemplateIdSelector);
            }
        }

        public string MasterTemplateAlias
        {
            get { return _masterTemplateAlias; }
            set
            {
                SetPropertyValueAndDetectChanges(o =>
                {
                    _masterTemplateAlias = value;
                    return _masterTemplateAlias;
                }, _masterTemplateAlias, MasterTemplateAliasSelector);
            }
        }

        [DataMember]
        string ITemplate.Name
        {
            get { return _name; }
            set { _name = value; }
        }

        [DataMember]
        string ITemplate.Alias
        {
            get { return _alias; }
            set { _alias = value; }
        }

        public override string Alias
        {
            get { return ((ITemplate)this).Alias; }
        }
        
        public override string Name
        {
            get { return ((ITemplate)this).Name; }
        }


        /// <summary>
        /// Returns true if the template is used as a layout for other templates (i.e. it has 'children')
        /// </summary>
        public bool IsMasterTemplate { get; internal set; }

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
            if (UmbracoConfig.For.UmbracoSettings().Templates.DefaultRenderingEngine == RenderingEngine.Mvc)
            {
                exts.Add("cshtml");
                exts.Add("vbhtml");
            }
            else
            {
                exts.Add(UmbracoConfig.For.UmbracoSettings().Templates.UseAspNetMasterPages ? "master" : "aspx");
            }

            var dirs = SystemDirectories.Masterpages;
            if (UmbracoConfig.For.UmbracoSettings().Templates.DefaultRenderingEngine == RenderingEngine.Mvc)
                dirs += "," + SystemDirectories.MvcViews;

            //Validate file
            var validFile = IOHelper.VerifyEditPath(Path, dirs.Split(','));

            //Validate extension
            var validExtension = IOHelper.VerifyFileExtension(Path, exts);

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


        public void SetMasterTemplate(ITemplate masterTemplate)
        {
            MasterTemplateId = new Lazy<int>(() => masterTemplate.Id);
        }

        public override object DeepClone()
        {
            var clone = (Template)base.DeepClone();

            //need to manually assign since they are readonly properties
            clone._alias = Alias;
            clone._name = Name;

            clone.ResetDirtyProperties(false);

            return clone;
        }

        
    }
}
