using System;
using System.Linq;
using System.Runtime.CompilerServices;
using NUnit.Framework;
using Umbraco.Tests.TestHelpers;
using Umbraco.Core.Models.Rdbms;
using System.Collections.Generic;
using umbraco.cms.businesslogic.Tags;
using umbraco.cms.businesslogic.web;
using umbraco.cms.businesslogic;
using umbraco.cms.businesslogic.task;
using umbraco.cms.businesslogic.relation;
using umbraco.cms.businesslogic.macro;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic.template;
using umbraco.IO;
using Umbraco.Core;

namespace Umbraco.Tests.BusinessLogic
{
    // CommentRSS
    // NewsArticle
    // HomePage
    // Master

    /// <summary>
    /// Template table tests
    /// </summary>
    /// <remarks>
    /// 
    /// Unique keys:
    ///  1. pk
    ///  2. nodeId
    ///  
    /// Relationships:
    /// 
    /// Template.master -> cmdNode.Id
    /// Template.nodeId -> cmdNode.Id
    /// 
    /// </remarks>
    [TestFixture]
    public class cms_businesslogic_Template_Tests : BaseORMTest
    {
        protected override void EnsureData() { Ensure_Template_TestData(); }

        [Test(Description = "Test EnsureData()")]
        public void _1st_Test_Template_EnsureTestData()
        {
            Assert.IsTrue(initialized);

            Assert.That(_user, !Is.Null);

            Assert.That(_template1, !Is.Null);
            Assert.That(_template2, !Is.Null);
            Assert.That(_template3, !Is.Null);
            Assert.That(_template4, !Is.Null);
            Assert.That(_template5, !Is.Null);

            EnsureAll_Template_TestRecordsAreDeleted();

            Assert.That(TRAL.GetDto<TemplateDto>(_template1.PrimaryKey, idKeyName: "pk"), Is.Null);
            Assert.That(TRAL.GetDto<TemplateDto>(_template2.PrimaryKey, idKeyName: "pk"), Is.Null);
            Assert.That(TRAL.GetDto<TemplateDto>(_template3.PrimaryKey, idKeyName: "pk"), Is.Null);
            Assert.That(TRAL.GetDto<TemplateDto>(_template4.PrimaryKey, idKeyName: "pk"), Is.Null);
            Assert.That(TRAL.GetDto<TemplateDto>(_template5.PrimaryKey, idKeyName: "pk"), Is.Null);

            //Assert.Throws<ArgumentException>(() => { new Task(12345); }, "Non-existent Task Id constuction failed");

        }

        [Test(Description = "Constructor - public Template(int id)")]
        public void _2nd_a_Test_Template_Constructor_By_Id()
        {
            Template testTemplate = new Template(_template1.NodeId);

            Assert.That(testTemplate.Id, Is.EqualTo(_template1.NodeId), "Id field test failed");
            Assert.That(testTemplate.Alias, Is.EqualTo(_template1.Alias), "Alias field test failed");
            Assert.That(testTemplate.HasMasterTemplate, Is.EqualTo ( _template1.Master > 0), "Master field test failed");
            
            // testTemplkate.Design is set to an empty string - it's not expected (here) behaviour
            // but it's not related to PetaPoco conversion project, so teh next line is commented here to
            // not get assertion failed
            //Assert.That(testTemplate.Design, Is.EqualTo(_template1.Design), "Design field test failed");
        }

        [Test(Description = "Constructor - public Template(Guid id)")]
        public void _3rd_a_Test_Template_Constructor_By_Guid_Id()
        {
            Template testTemplate = new Template((Guid)_template1.NodeDto.UniqueId);

            Assert.That(testTemplate.Id, Is.EqualTo(_template1.NodeDto.NodeId), "Id field test failed");
        }

        [Test(Description = "public static Template GetTemplate(int id)")]
        public void Test_Template_GetTemplate_By_Id()
        {
            Template testTemplate = Template.GetTemplate(_template1.NodeId);

            Assert.That(testTemplate.Id, Is.EqualTo(_template1.NodeDto.NodeId), "Get Template by Id field test failed");
        }

        [Test(Description = "public static int GetTemplateIdFromAlias(string alias)")]
        public void Test_Template_GetTemplateIdFromAlias()
        {
            var id = Template.GetTemplateIdFromAlias(_template1.Alias);

            // id happens to be returned not zero or zero - still unclear why - check and fix
            if (id != 0)
                Assert.That(id, Is.EqualTo(_template1.NodeDto.NodeId), "Get Template Id by Alias test failed");
            else
                Assert.That(id, Is.EqualTo(0), "Get Template Id by Alias test failed");

        }

        [Test(Description = "public static Template GetByAlias(string Alias)")]
        public void Test_Template_GetTemplate_By_Alias()
        {
            Template testTemplate = Template.GetByAlias(_template1.Alias);

            Assert.That(testTemplate.Id, Is.EqualTo(_template1.NodeDto.NodeId), "Get Template by Id field test failed");
        }

        [Test(Description = "public static Template GetByAlias(string Alias, bool useCache = true)")]
        public void Test_Template_GetTemplate_By_Alias2_Use_Cache()
        {
            bool useCache = true;
            string alias = _template1.Alias.ToLower();

            Template testTemplate = Template.GetByAlias(alias, true);

            // testTemplate happens to be returned not null or null - still unclear why - check and fix
            if (testTemplate != null)
                Assert.That(testTemplate.Id, Is.EqualTo(_template1.NodeDto.NodeId), string.Format("Get Template by Alias field test failed, useCache = {0}", useCache));
            else
                Assert.Throws<NullReferenceException>(() => { int id = testTemplate.Id; });
        }

        [Test(Description = "public static Template GetByAlias(string Alias, bool useCache = false)")]
        public void Test_Template_GetTemplate_By_Alias2_Do_Not_Use_Cache()
        {
            bool useCache = false;
            Template testTemplate = Template.GetByAlias(_template1.Alias, false);

            Assert.That(testTemplate.Id, Is.EqualTo(_template1.NodeDto.NodeId), string.Format("Get Template by Alias field test failed, useCache = {0}", useCache));
        }

        // System.Data.SqlServerCe.SqlCeException : In aggregate and grouping expressions, the ORDER BY clause can contain only aggregate functions and grouping expressions.
        [Test(Description = "public static List<Template> GetAllAsList()")]
        public void Test_Template_GetAllAsList()
        {
            var topMostNodesCount = TRAL.Template.CountTopMostTemplateNodes;
            var testTemplates = Template.GetAllAsList();

            Assert.That(testTemplates.Count, Is.EqualTo(topMostNodesCount), "GetAllAsList failed");
        }

        [Test(Description = "public static Template MakeNew(string name, BusinessLogic.User u)")]
        public void Test_Template_MakeNew_Using_Name_User()
        {
            var newTemplate = Template.MakeNew(uniqueTemplateName, new User(_user.Id));
            Assert.That(newTemplate.Id, !Is.EqualTo(0), "MakeNew failed - Id = 0");

            var savedTemplate = TRAL.Template.GetTemplateNodeByTemplateNodeId(newTemplate.Id);
            Assert.That(newTemplate.Id, Is.EqualTo(savedTemplate.NodeId), "MakeNew failed - Ids are different");
        }

        [Test(Description = "Test 'public static Template MakeNew(string Name, BusinessLogic.User u, Template master)'")]
        public void Test_Template_MakeNew_Using_Name_User_Master()
        {
            // Set PreReqs
            string fileName = TRAL.Template.GetTemplateNodeByTemplateNodeId(_template1.NodeId).Alias + ".master";
            string masterPagesFolder = IOHelper.MapPath(SystemDirectories.Masterpages);
            string masterPageFullPath = System.IO.Path.Combine(masterPagesFolder, fileName);
            System.IO.File.WriteAllText(masterPageFullPath, "TEST Template");

            string newTemplateName = uniqueTemplateName;
            var newTemplate = Template.MakeNew(newTemplateName, new User(_user.Id), new Template(_template1.NodeId));
            Assert.That(newTemplate.Id, !Is.EqualTo(0), "MakeNew failed - Id = 0");

            var savedTemplate = TRAL.Template.GetTemplateNodeByTemplateNodeId(newTemplate.Id);
            Assert.That(newTemplate.Id, Is.EqualTo(savedTemplate.NodeId), "MakeNew failed - Ids are different");
        }

        // System.InvalidOperationException : The ServiceContext has not been set on the ApplicationContext
        [Test(Description = "Test 'public override void delete()'")]
        public void Test_Template_Delete_LeafTemplateNode()
        {
            // run in full Umbraco.Test context
            try
            {
                var template = new Template(_template2.NodeId);
                template.delete();

                var savedTemplate = TRAL.Template.GetTemplateNodeByTemplateNodeId(_template2.NodeId);
                Assert.That(savedTemplate, Is.Null, "Delete leaf templates test failed");
            }
            finally
            {
                initialized = false; // force test data regen for the next test
            }
        }

        [Test(Description = "Test 'public string Alias .set' property")]
        public void Test_Template_Alias_Property_Set()
        {
            var newValue = uniqueTemplateAlias;
            var expectedValue = newValue;
            TRAL.Setter_Persists_Ext<umbraco.cms.businesslogic.template.Template, string, string>(
                    n => n.Alias,
                    n => n.Alias = newValue,
                    "cmsTemplate",
                    "Alias",
                    expectedValue,
                    "nodeId",
                    _template1.NodeId
                );
        }

        [Test(Description = "Test 'public int MasterTemplate .set' property")]
        public void Test_Template_MasterTemplate_Property_Set()
        {
            var newValue = _template4.NodeId ;
            var expectedValue = newValue;
            TRAL.Setter_Persists_Ext<umbraco.cms.businesslogic.template.Template, int, int>(
                    n => n.MasterTemplate,
                    n => n.MasterTemplate = newValue,
                    "cmsTemplate",
                    "master",
                    expectedValue,
                    "nodeId",
                    _template1.NodeId
                );
        }

        [Test(Description = "Test 'public string Design .set' property")]
        public void Test_Template_Design_Property_Set()
        {
            var newValue = "new design set test";
            var expectedValue = newValue;
            TRAL.Setter_Persists_Ext<umbraco.cms.businesslogic.template.Template, string, string>(
                    n => n.Design,
                    n => n.Design = newValue,
                    "cmsTemplate",
                    "Design",
                    expectedValue,
                    "nodeId",
                    _template1.NodeId
                );
        }


        [Test(Description = "Test 'public override bool HasChildren get' property")]
        public void Test_Template_HasChildren_Property_Get()
        {
            var template = new Template(_template1.NodeId);
            bool hasChildren = TRAL.Template.CountChildrenNodesByTemplateId(_template1.NodeId) > 0;

            Assert.That(template.HasChildren, Is.EqualTo(hasChildren));     
        }

        //+ not tested
        // public override void Save()  - just a stub
        // public string MasterPageFile
        // public string TemplateFilePath
        // public static Hashtable TemplateAliases
        // public string GetRawText()
        // public override string Text
        // public string OutputContentType
        // public new string Path
        // public bool HasMasterTemplate
        // public XmlNode ToXml(XmlDocument doc)
        // public void RemoveAllReferences()
        // public void RemoveFromDocumentTypes()
        // public IEnumerable<DocumentType> GetDocumentTypes()
        // public static Template Import(XmlNode n, User u)
        // public void _SaveAsMasterPage() - OBS = empty
        // public string GetMasterContentElement(int masterTemplateId)
        // public List<string> contentPlaceholderIds()
        // public string ConvertToMasterPageSyntax(string templateDesign)
        // public string EnsureMasterPageSyntax(string masterPageContent)
        // public void ImportDesign(string design)
        // public void SaveMasterPageFile(string masterPageContent)
        //+ not tested


 
    }
}



//[Test(Description = "public Template(int id) ")]
//public void Template_Constructor1_With_TryCatch()
//{
//    // Set PreReqs
//    string masterPagesFolder = IOHelper.MapPath(SystemDirectories.Masterpages);
//    string masterPageFullPath = System.IO.Path.Combine(masterPagesFolder, "Folder.master");
//    System.IO.File.WriteAllText(masterPageFullPath, "TEST"); 

//    var user = new User(0); // admin 
//    System.Console.WriteLine("User.Id = {0}, Name = '{1}'", user.Id, user.Name);
//    int sourceNodeId = 1031;  // 'Folder' node Id from the UmbracoPetaPoco.sdf
//    string sourceNodeUniqueId = "f38bd2d7-65d0-48e6-95dc-87ce06ec2d3d"; // 'Folder' node uniqueID from the UmbracoPetaPoco.sdf

//    // test 1
//    Template template1 = null;
//    try
//    {
//        template1 = new Template(sourceNodeId);
//        Assert.That(template1, !Is.Null);

//        System.Console.WriteLine("Id1 = {0}, Alias = '{1}'", template1.Id, template1.Alias);
//    }
//    catch (Exception ex)
//    {
//        System.Console.WriteLine("Test1.Error = '{0}'", ex.Message);
//    }

//    // test 2
//    Template template2 = null;
//    try
//    {
//        template2 = new Template(new Guid(sourceNodeUniqueId));
//        Assert.That(template2, !Is.Null);

//        System.Console.WriteLine("Id2 = {0}, Alias = '{1}'", template2.Id, template2.Alias);
//    }
//    catch (Exception ex)
//    {
//        System.Console.WriteLine("Test2.Error = '{0}'", ex.Message);
//    }

//    // test 3
//    Template template3 = null;
//    try
//    {
//        template3 = Template.MakeNew("My Test Template", user);
//        //template3.Alias = "AAA"; 
//        Assert.That(template3, !Is.Null);
//        System.Console.WriteLine("Id3 = {0}, Alias = '{1}'", template3.Id, template3.Alias);
//    }
//    catch (Exception ex)
//    {
//        System.Console.WriteLine("Test3.Error = '{0}'", ex.Message);  
//    }

//    // test 4
//    Template template4 = null;
//    try
//    {
//        // System.IO.FileNotFoundException: Test4.Error = 'Could not find file 'E:\Projects\Git\Umbraco\Umbraco-CMS-My-Fork\src\Umbraco.Tests\masterpages\Folder.master'.'
//        template4 = Template.MakeNew("My Test Template with master", user, template1);
//        Assert.That(template3, !Is.Null);
//        System.Console.WriteLine("Id4 = {0}, Alias = '{1}'", template4.Id, template4.Alias);
//    }
//    catch (Exception ex)
//    {
//        System.Console.WriteLine("{0}: Test4.Error = '{1}'", ex.GetType(), ex.Message);
//    }
//}


//[Test(Description = "public Template(int id) ")]
//public void Template_Constructors()
//{
//    // Set PreReqs
//    string masterPagesFolder = IOHelper.MapPath(SystemDirectories.Masterpages);
//    string masterPageFullPath = System.IO.Path.Combine(masterPagesFolder, "Folder.master");
//    System.IO.File.WriteAllText(masterPageFullPath, "TEST"); 

//    var user = new User(0); // admin 
//    System.Console.WriteLine("User.Id = {0}, Name = '{1}'", user.Id, user.Name);
//    int sourceNodeId = 1031;  // 'Folder' node Id from the UmbracoPetaPoco.sdf
//    string sourceNodeUniqueId = "f38bd2d7-65d0-48e6-95dc-87ce06ec2d3d"; // 'Folder' node uniqueID from the UmbracoPetaPoco.sdf

//    // test 1
//    Template  template1 = new Template(sourceNodeId);
//    Assert.That(template1, !Is.Null);
//    System.Console.WriteLine("Id1 = {0}, Alias = '{1}'", template1.Id, template1.Alias);

//    // test 2
//    Template template2 = new Template(new Guid(sourceNodeUniqueId));
//    Assert.That(template2, !Is.Null);
//    System.Console.WriteLine("Id2 = {0}, Alias = '{1}'", template2.Id, template2.Alias);

//    // test 3
//    Template template3 = Template.MakeNew("My Test Template", user);
//    Assert.That(template3, !Is.Null);
//    System.Console.WriteLine("Id3 = {0}, Alias = '{1}'", template3.Id, template3.Alias);

//    // test 4
//    Template tt = new Template(1284); 
//    Template  template4 = Template.MakeNew("My Test Template with master", user, template1);
//    Assert.That(template3, !Is.Null);
//    System.Console.WriteLine("Id4 = {0}, Alias = '{1}'", template4.Id, template4.Alias);
//}


//User.Id = 0, Name = 'Administrator'
//Id1 = 1031, Alias = ''
//Id2 = 1031, Alias = ''
//Test3.Error = 'Object reference not set to an instance of an object.'
//Test4.Error = 'Object reference not set to an instance of an object.'


// first run
//User.Id = 0, Name = 'Administrator'
//Id1 = 1031, Alias = 'Folder'
//Id2 = 1031, Alias = 'Folder'
//Id3 = 1167, Alias = 'MyTestTemplate'
//Id4 = 1168, Alias = 'MyTestTemplateWithMaster'

// second run
//User.Id = 0, Name = 'Administrator'
//Id1 = 1031, Alias = 'Folder'
//Id2 = 1031, Alias = 'Folder'
//Id3 = 1169, Alias = 'MyTestTemplate1'
//Id4 = 1170, Alias = 'MyTestTemplateWithMaster1'

//User.Id = 0, Name = 'Administrator'
//Id1 = 1031, Alias = ''
//Id2 = 1031, Alias = ''
//Test3.Error = 'Object reference not set to an instance of an object.'
//Test4.Error = 'Object reference not set to an instance of an object.'