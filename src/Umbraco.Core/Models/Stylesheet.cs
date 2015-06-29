using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Umbraco.Core.IO;
using Umbraco.Core.Strings.Css;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Represents a Stylesheet file
    /// </summary>
    [Serializable]
    [DataContract(IsReference = true)]
    public class Stylesheet : File
    {
        public Stylesheet(string path) 
            : base(path)
        {          
            InitializeProperties();
        }
      
        private Lazy<List<StylesheetProperty>> _properties;

        private void InitializeProperties()
        {
            //if the value is already created, we need to be created and update the collection according to 
            //what is now in the content
            if (_properties != null && _properties.IsValueCreated)
            {
                //re-parse it so we can check what properties are different and adjust the event handlers
                var parsed = StylesheetHelper.ParseRules(Content).ToArray();
                var names = parsed.Select(x => x.Name).ToArray();
                var existing = _properties.Value.Where(x => names.Contains(x.Name)).ToArray();
                //update existing
                foreach (var stylesheetProperty in existing)
                {
                    var updateFrom = parsed.Single(x => x.Name == stylesheetProperty.Name);
                    //remove current event handler while we update, we'll reset it after
                    stylesheetProperty.PropertyChanged -= Property_PropertyChanged;
                    stylesheetProperty.Alias = updateFrom.Selector;
                    stylesheetProperty.Value = updateFrom.Styles;
                    //re-add
                    stylesheetProperty.PropertyChanged += Property_PropertyChanged;
                }
                //remove no longer existing
                var nonExisting = _properties.Value.Where(x => names.Contains(x.Name) == false).ToArray();
                foreach (var stylesheetProperty in nonExisting)
                {
                    stylesheetProperty.PropertyChanged -= Property_PropertyChanged;
                    _properties.Value.Remove(stylesheetProperty);
                }
                //add new ones
                var newItems = parsed.Where(x => _properties.Value.Select(p => p.Name).Contains(x.Name) == false);
                foreach (var stylesheetRule in newItems)
                {
                    var prop = new StylesheetProperty(stylesheetRule.Name, stylesheetRule.Selector, stylesheetRule.Styles);
                    prop.PropertyChanged += Property_PropertyChanged;
                    _properties.Value.Add(prop);
                }               
            }

            //we haven't read the properties yet so create the lazy delegate
            _properties = new Lazy<List<StylesheetProperty>>(() =>
            {
                var parsed = StylesheetHelper.ParseRules(Content);
                return parsed.Select(statement =>
                {
                    var property = new StylesheetProperty(statement.Name, statement.Selector, statement.Styles);
                    property.PropertyChanged += Property_PropertyChanged;
                    return property;

                }).ToList();
            });
        }

        /// <summary>
        /// If the property has changed then we need to update the content
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Property_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var prop = (StylesheetProperty) sender;

            //Ensure we are setting base.Content here so that the properties don't get reset and thus any event handlers would get reset too
            base.Content = StylesheetHelper.ReplaceRule(Content, prop.Name, new StylesheetRule
            {
                Name = prop.Name,
                Selector = prop.Alias,
                Styles = prop.Value
            });
        }

        /// <summary>
        /// Gets or sets the Content of a File
        /// </summary>
        public override string Content
        {
            get { return base.Content; }
            set
            {
                base.Content = value;
                //re-set the properties so they are re-read from the content
                InitializeProperties();
            }
        }

        /// <summary>
        /// Returns a list of umbraco back office enabled stylesheet properties
        /// </summary>
        /// <remarks>
        /// An umbraco back office enabled stylesheet property has a special prefix, for example: 
        /// 
        /// /** umb_name: MyPropertyName */ p { font-size: 1em; }
        /// </remarks>
        [IgnoreDataMember]
        public IEnumerable<StylesheetProperty> Properties
        {
            get { return _properties.Value; }
        }

        /// <summary>
        /// Adds an Umbraco stylesheet property for use in the back office
        /// </summary>
        /// <param name="property"></param>
        public void AddProperty(StylesheetProperty property)
        {
            if (Properties.Any(x => x.Name == property.Name))
            {
                throw new DuplicateNameException("The property with the name " + property.Name + " already exists in the collection");
            }

            //now we need to serialize out the new property collection over-top of the string Content. 
            Content = StylesheetHelper.AppendRule(Content, new StylesheetRule
            {
                Name = property.Name,
                Selector = property.Alias,
                Styles = property.Value
            });

            //re-set lazy collection
            InitializeProperties();
        }

        /// <summary>
        /// Removes an Umbraco stylesheet property
        /// </summary>
        /// <param name="name"></param>
        public void RemoveProperty(string name)
        {
            if (Properties.Any(x => x.Name == name))
            {
                Content = StylesheetHelper.ReplaceRule(Content, name, null);
            }
        }
  
        /// <summary>
        /// Indicates whether the current entity has an identity, which in this case is a path/name.
        /// </summary>
        /// <remarks>
        /// Overrides the default Entity identity check.
        /// </remarks>
        public override bool HasIdentity
        {
            get { return string.IsNullOrEmpty(Path) == false; }
        }
    }
}