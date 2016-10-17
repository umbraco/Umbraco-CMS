using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.IO;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Services;
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

        private static readonly Lazy<PropertySelectors> Ps = new Lazy<PropertySelectors>();

        private class PropertySelectors
        {
            public readonly PropertyInfo MasterTemplateAliasSelector = ExpressionHelper.GetPropertyInfo<Template, string>(x => x.MasterTemplateAlias);
            public readonly PropertyInfo MasterTemplateIdSelector = ExpressionHelper.GetPropertyInfo<Template, Lazy<int>>(x => x.MasterTemplateId);
            public readonly PropertyInfo AliasSelector = ExpressionHelper.GetPropertyInfo<Template, string>(x => x.Alias);
            public readonly PropertyInfo NameSelector = ExpressionHelper.GetPropertyInfo<Template, string>(x => x.Name);
        }

        public Template(string name, string alias)
            : this(name, alias, (Func<File, string>) null)
        { }

        internal Template(string name, string alias, Func<File, string> getFileContent)
            : base(string.Empty, getFileContent)
        {
            _name = name;
            _alias = alias.ToCleanString(CleanStringType.UnderscoreAlias);
            _masterTemplateId = new Lazy<int>(() => -1);
        }

        [Obsolete("This constructor should not be used, file path is determined by alias, setting the path here will have no affect")]
        public Template(string path, string name, string alias)
            : this(name, alias)
        {            
        }

        [DataMember]
        public Lazy<int> MasterTemplateId
        {
            get { return _masterTemplateId; }
            set { SetPropertyValueAndDetectChanges(value, ref _masterTemplateId, Ps.Value.MasterTemplateIdSelector); }
        }

        public string MasterTemplateAlias
        {
            get { return _masterTemplateAlias; }
            set { SetPropertyValueAndDetectChanges(value, ref _masterTemplateAlias, Ps.Value.MasterTemplateAliasSelector); }
        }

        [DataMember]
        public new string Name
        {
            get { return _name; }
            set { SetPropertyValueAndDetectChanges(value, ref _name, Ps.Value.NameSelector); }
        }

        [DataMember]
        public new string Alias
        {
            get { return _alias; }
            set { SetPropertyValueAndDetectChanges(value.ToCleanString(CleanStringType.UnderscoreAlias), ref _alias, Ps.Value.AliasSelector); }
        }

        /// <summary>
        /// Returns true if the template is used as a layout for other templates (i.e. it has 'children')
        /// </summary>
        public bool IsMasterTemplate { get; internal set; }

        [Obsolete("This is no longer used and will be removed from the codebase in future versions, use the IFileSystem DetermineRenderingEngine method instead")]
        public RenderingEngine GetTypeOfRenderingEngine()
        {
            //Hack! TODO: Remove this method entirely
            return ApplicationContext.Current.Services.FileService.DetermineTemplateRenderingEngine(this);
        }

        public void SetMasterTemplate(ITemplate masterTemplate)
        {
            if (masterTemplate == null)
            {
                MasterTemplateId = new Lazy<int>(() => -1);
                MasterTemplateAlias = null;
            }
            else
            {
                MasterTemplateId = new Lazy<int>(() => masterTemplate.Id);
                MasterTemplateAlias = masterTemplate.Alias;
            }
           
        }

        protected override void DeepCloneNameAndAlias(File clone)
        {
            // do nothing - prevents File from doing its stuff
        }
    }
}
