using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TypeMock.ArrangeActAssert;
using System.Xml.Linq;
using umbraco.Linq.DTMetal.CodeBuilder;

namespace umbraco.Linq.DTMetal.Engine.Tests
{
    /// <summary>
    /// Summary description for DocTypeMarkupLanguageBuilderTests
    /// </summary>
    [TestClass]
    public class DocTypeMarkupLanguageBuilderTests
    {
        [TestMethod]
        [Isolated]
        [ExpectedException(typeof(ArgumentNullException))]
        public void DocTypeMarkupLanguageBuilderTest_Constructor()
        {
            var dtmlGen = new DocTypeMarkupLanguageBuilder(null, string.Empty, false);
        }

        [TestMethod, Isolated]
        public void DocTypeMarkupLanguageBuilderTest_XmlValidates()
        {
            var dtob = Isolate.Fake.Instance<DocTypeObjectBuilder>(Members.MustSpecifyReturnValues);

            Isolate.WhenCalled(() => dtob.DocumentTypes).WillReturn(new List<DocType>
            {
               new DocType{
                   Alias = "DocType1",
                   Name = "Document Type 1",
                   Description = "Document type 1 description",
                   Id = 1,
                   ParentId = -1,
                   Associations= new List<DocTypeAssociation>(),
                   Properties = new List<DocTypeProperty>(),
               }
            });

            var dtmlGen = new DocTypeMarkupLanguageBuilder(dtob.DocumentTypes, string.Empty, false);

            dtmlGen.BuildXml();

            Assert.IsTrue(true);
        }

        [TestMethod, Isolated]
        public void DocTypeMarkupLanguageBuilderTest_DataContextName()
        {
            var dtob = Isolate.Fake.Instance<DocTypeObjectBuilder>(Members.MustSpecifyReturnValues);

            Isolate.WhenCalled(() => dtob.DocumentTypes).WillReturn(new List<DocType>
            {
               new DocType{
                   Alias = "DocType1",
                   Name = "Document Type 1",
                   Description = "Document Type 1 description",
                   Id = 1,
                   ParentId = -1,
                   Associations= null,
                   Properties = null,
               }
            });

            var dtmlGen = new DocTypeMarkupLanguageBuilder(dtob.DocumentTypes, string.Empty, false);
            dtmlGen.BuildXml();
            Assert.AreEqual<string>("umbraco", (string)dtmlGen.DocTypeMarkupLanguage.Root.Attribute("DataContextName"));

            dtmlGen = new DocTypeMarkupLanguageBuilder(dtob.DocumentTypes, "Myumbraco", false);
            dtmlGen.BuildXml();
            Assert.AreEqual<string>("Myumbraco", (string)dtmlGen.DocTypeMarkupLanguage.Root.Attribute("DataContextName"));
        }

        [TestMethod]
        [Isolated]
        public void DocTypeMarkupLanguageBuilderTest_SingleDocType()
        {
            var xmlDoc = XDocument.Parse(@"<?xml version=""1.0"" encoding=""utf-8""?>
<DocumentTypes Serialization=""None"" DataContextName=""umbraco"" PluralizeCollections=""true"">
  <DocumentType ParentId=""-1"">
    <Id>1</Id>
    <Name>Document Type 1</Name>
    <Alias>DocType1</Alias>
    <Description>Document Type 1 description</Description>
    <Properties />
    <Associations />
  </DocumentType>
</DocumentTypes>");

            var dtob = Isolate.Fake.Instance<DocTypeObjectBuilder>(Members.MustSpecifyReturnValues);

            Isolate.WhenCalled(() => dtob.DocumentTypes).WillReturn(new List<DocType>
            {
               new DocType{
                   Alias = "DocType1",
                   Name = "Document Type 1",
                   Description = "Document Type 1 description",
                   Id = 1,
                   ParentId = -1,
                   Associations= null,
                   Properties = null,
               }
            });

            var dtmlGen = new DocTypeMarkupLanguageBuilder(dtob.DocumentTypes, string.Empty, false);

            Isolate.Verify.WasCalledWithAnyArguments(() => dtob.DocumentTypes.GetEnumerator());

            dtmlGen.BuildXml();

            Assert.AreEqual(xmlDoc.ToString(SaveOptions.None), dtmlGen.DocTypeMarkupLanguage.ToString(SaveOptions.None));
        }

        [TestMethod, Isolated]
        public void DocTypeMarkupLanguageBuilderTest_DocTypeWithProperties()
        {
            var dtob = Isolate.Fake.Instance<DocTypeObjectBuilder>(Members.ReturnRecursiveFakes);

            Isolate.WhenCalled(() => dtob.DocumentTypes).WillReturn(new List<DocType>
            {
               new DocType{
                   Alias = "DocType1",
                   Name = "Document Type 1",
                   Description = "Document Type 1 description",
                   Id = 1,
                   ParentId = -1,
                   Associations= null,
                   Properties = new List<DocTypeProperty>{
                    new DocTypeProperty{
                       Alias = "Property1",
                       ControlId = new Guid("15e66384-3fb6-435e-8fc0-fa63b47f0f4c"),
                       DatabaseType= typeof(int),
                       Description = string.Empty,
                       Id = 1,
                       Mandatory = false,
                       Name = "Property 1"
                    }
                   },
               }
            });

            var dtmlGen = new DocTypeMarkupLanguageBuilder(dtob.DocumentTypes, string.Empty, false);

            dtmlGen.BuildXml();

            Assert.IsNotNull(dtmlGen.DocTypeMarkupLanguage);
            Assert.AreEqual<int>(1, dtmlGen.DocTypeMarkupLanguage.Descendants("Properties").Count());

            var propertiesXml = dtmlGen.DocTypeMarkupLanguage.Descendants("Properties").First();
            Assert.AreEqual<string>("Property1", propertiesXml.Element("Property").Element("Alias").Value);
            Assert.AreEqual<string>(new Guid("15e66384-3fb6-435e-8fc0-fa63b47f0f4c").ToString(), propertiesXml.Element("Property").Element("ControlId").Value);
            Assert.AreEqual<string>(typeof(int).ToString(), propertiesXml.Element("Property").Element("Type").Value);
            Assert.AreEqual<string>(string.Empty, propertiesXml.Element("Property").Element("Description").Value);
            Assert.AreEqual<int>(1, (int)propertiesXml.Element("Property").Element("Id"));
            Assert.AreEqual<bool>(false, (bool)propertiesXml.Element("Property").Element("Mandatory"));
            Assert.AreEqual<string>("Property 1", propertiesXml.Element("Property").Element("Name").Value);
        }

        [TestMethod, Isolated]
        public void DocTypeMarkupLanguageBuilderTest_DocTypeWithAssociations()
        {
            var dtob = Isolate.Fake.Instance<DocTypeObjectBuilder>(Members.MustSpecifyReturnValues);

            Isolate.WhenCalled(() => dtob.DocumentTypes).WillReturn(new List<DocType>
            {
               new DocType{
                   Alias = "DocType1",
                   Name = "Document Type 1",
                   Description = "Document Type 1 description",
                   Id = 1,
                   ParentId = -1,
                   Associations = new List<DocTypeAssociation>{
                        new DocTypeAssociation{
                            AllowedId = 1
                        }
                   },
                   Properties = null,
               }
            });

            var dtmlGen = new DocTypeMarkupLanguageBuilder(dtob.DocumentTypes, string.Empty, false);
            dtmlGen.BuildXml();

            Assert.AreEqual<int>(1, dtmlGen.DocTypeMarkupLanguage.Descendants("Associations").Count());

            var association = dtmlGen.DocTypeMarkupLanguage.Descendants("Associations").First();
            Assert.AreEqual<int>(1, (int)association.Element("Association"));
        }
    }
}
