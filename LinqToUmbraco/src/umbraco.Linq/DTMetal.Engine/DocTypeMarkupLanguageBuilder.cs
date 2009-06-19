using System.Collections.Generic;
using System.Xml.Linq;
using System;
using System.Linq;
using System.Text;
using System.Xml.Schema;
using umbraco.Linq.DTMetal.CodeBuilder;

namespace umbraco.Linq.DTMetal.Engine
{
    internal sealed class DocTypeMarkupLanguageBuilder
    {
        private IEnumerable<DocType> _theList;
        private XDocument _theXml;
        private const string DTML_XSD_PATH = "umbraco.Linq.DTMetal.Engine.DocTypeML.xsd";
        private string _dataContextName;
        private bool _disablePluralization;

        public DocTypeMarkupLanguageBuilder(IEnumerable<DocType> docTypes, string dataContextName, bool disablePluralization)
        {
            
            if (docTypes == null)
            {
                throw new ArgumentNullException("docTypes");
            }
            if (string.IsNullOrEmpty(dataContextName))
            {
                dataContextName = "umbraco";
            }

            this._dataContextName = dataContextName;
            this._theList = docTypes;
            this._disablePluralization = disablePluralization;
        }

        internal void BuildXml()
        {
            var root = new XElement("DocumentTypes",
                        new XAttribute("Serialization", "None"),
                        new XAttribute("DataContextName", this._dataContextName),
                        new XAttribute("PluralizeCollections", !this._disablePluralization),
                        this._theList.Select(dt => BuildDocTypeXml(dt))
                        );

            this._theXml = new XDocument(root);

            this.ValidateSchema();
        }

        private XElement BuildDocTypeXml(DocType dt)
        {
            var dtXml = new XElement("DocumentType",
                new XAttribute("ParentId", dt.ParentId),
                new XElement("Id", dt.Id),
                new XElement("Name", dt.Name),
                new XElement("Alias", dt.Alias),
                new XElement("Description", dt.Description),
                BuildPropertiesXml(dt.Properties),
                BuildAssociationsXml(dt.Associations)
                );

            return dtXml;
        }

        private XElement BuildAssociationsXml(List<DocTypeAssociation> list)
        {
            var associationsNode = new XElement("Associations");
            if (list != null)
            {
                foreach (var item in list)
                {
                    associationsNode.Add(new XElement("Association", item.AllowedId));
                }
            }
            return associationsNode;
        }

        private XElement BuildPropertiesXml(List<DocTypeProperty> list)
        {
            var propertiesNode = new XElement("Properties");

            if (list != null)
            {
                
                propertiesNode.Add(list.Select(p => new XElement("Property",
                                                        new XElement("Id", p.Id),
                                                        new XElement("Name", p.Name),
                                                        new XElement("Alias", p.Alias),
                                                        new XElement("Mandatory", p.Mandatory),
                                                        new XElement("RegularExpression", p.RegularExpression),
                                                        new XElement("Type", p.DatabaseType),
                                                        new XElement("ControlId", p.ControlId.ToString()),
                                                        new XElement("Description", p.Description)
                                                    )
                                                )
                                            );
            }

            return propertiesNode;
        }

        internal XDocument DocTypeMarkupLanguage
        {
            get
            {
                return this._theXml;
            }
        }

        internal void ValidateSchema()
        {
            if (this._theXml == null)
            {
                throw new NullReferenceException("DTML has not be generated yet");
            }

            XmlSchemaSet schemas = new XmlSchemaSet();
            //read the resorce for the XSD to validate against
            schemas.Add("", System.Xml.XmlReader.Create(this.GetType().Assembly.GetManifestResourceStream(DTML_XSD_PATH)));

            //we'll have a list of all validation exceptions to put them to the screen
            List<XmlSchemaException> exList = new List<XmlSchemaException>();

            //some funky in-line event handler. Lambda loving goodness ;)
            this._theXml.Validate(schemas, (o, e) => { exList.Add(e.Exception); });

            if (exList.Count > 0)
            {
                //dump out the exception list
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("The following validation errors occuring with the XML:");
                foreach (var item in exList)
                {
                    sb.AppendLine(" * " + item.Message + " - " + item.StackTrace);
                }
                throw new XmlSchemaException(sb.ToString());
            }
        }

        public void Save(string outputFilePath)
        {
            this.DocTypeMarkupLanguage.Save(outputFilePath);
        }
    }
}
