using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using umbraco.cms.presentation.Trees;
using System.Xml.Linq;
using System.Reflection;
using umbraco.presentation;

namespace umbraco
{
    public class dynamicLoader : BaseTree
    {
        public dynamicLoader(string app) : base(app){
            this._loadedTrees = new Dictionary<Type, BaseTree>();
            LoadTreeTypes();
        }

        private Dictionary<Type, BaseTree> _loadedTrees;
        private bool _treesLoaded = false;

        protected override void CreateRootNode(ref XmlTreeNode rootNode)
        {
        }

        protected override void CreateAllowedActions(ref List<umbraco.interfaces.IAction> actions)
        {
            actions.Clear();
        }

        public override void Render(ref XmlTree tree)
        {
            if (!this._treesLoaded)
                LoadTreeTypes();

            foreach (var loadedTree in this._loadedTrees)
            {
                tree.Add(loadedTree.Value.RootNode);
            }
        }

        private void LoadTreeTypes()
        {
            var xmlFile = UmbracoContext.Current.Server.DataFolder + "\\" + this.app + ".config"; //this is so not cool :/
            if (string.IsNullOrEmpty(xmlFile))
                throw new ArgumentException("PromotionsTreeXml");

            var xml = XDocument.Load(HttpContext.Current.Server.MapPath(xmlFile));

            var treesXml = xml.Descendants("Tree");

            foreach (var treeXml in treesXml)
            {
                var typeName = treeXml.Element("Class").Value;
                var assemblyName = treeXml.Element("Assembly").Value;

                var treeType = Type.GetType(typeName + "," + assemblyName);
                var instance = Activator.CreateInstance(treeType, this.app) as BaseTree;
                if (instance == null)
                    throw new ArgumentException("Type \"" + typeName + "," + assemblyName + "\" does not inherit umbraco.cms.presentation.Trees.BaseTree, and hence can't be used as a Tree");

                var properties = treeXml.Element("Properties");
                if (properties != null)
                {
                    foreach (var property in properties.Elements())
                    {
                        var accessorType = (string)property.Attribute("AccessMode");
                        var name = (string)property.Attribute("Name");
                        switch (accessorType)
                        {
                            case "Property":
                                var p = treeType.GetProperty(name);
                                p.SetValue(instance, Convert.ChangeType(property.Value, p.PropertyType), null); 
                                break;

                            case "Field":
                                var f = treeType.GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
                                f.SetValue(instance, Convert.ChangeType(property.Value, f.FieldType)); 
                                break;
                            default:
                                throw new NotSupportedException("AccessMode of " + accessorType + " is not supported");
                        }
                    }
                }

                this._loadedTrees.Add(treeType, instance);
            }

            this._treesLoaded = true;
        }

        public override void RenderJS(ref System.Text.StringBuilder Javascript)
        {
            if (!this._treesLoaded)
                LoadTreeTypes();

            foreach (var tree in this._loadedTrees)
                tree.Value.RenderJS(ref Javascript);
        }
    }
}
