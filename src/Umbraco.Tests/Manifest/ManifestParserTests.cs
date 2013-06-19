using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Core.Manifest;

namespace Umbraco.Belle.Tests
{    
    [TestFixture]
    public class ManifestParserTests
    {

        [Test]
        public void Parse_Property_Editors()
        {

            var a = JsonConvert.DeserializeObject<JArray>(@"[
    {
        id: '0EEBB7CE-51BA-4F6B-9D9C-78BB3314366C',
        name: 'Test 1',        
        editor: {
            view: '~/App_Plugins/MyPackage/PropertyEditors/MyEditor.html',
            valueType: 'int',
            validation: [
                {
                    type: 'Required'
                },
                {
                    type: 'Regex',
                    value: '\\d*'
                },
            ]
        }
    },
    {
        id: '1FCF5C39-5FC7-4BCE-AFBE-6500D9EBA261',
        name: 'Test 2',
        editor: {
            view: '~/App_Plugins/MyPackage/PropertyEditors/CsvEditor.html'
        }
    },
]");
            var parser = ManifestParser.GetPropertyEditors(a);

            Assert.AreEqual(2, parser.Count());
            Assert.AreEqual(new Guid("0EEBB7CE-51BA-4F6B-9D9C-78BB3314366C"), parser.ElementAt(0).Id);
            Assert.AreEqual("Test 1", parser.ElementAt(0).Name);
            Assert.AreEqual("~/App_Plugins/MyPackage/PropertyEditors/MyEditor.html", parser.ElementAt(0).ValueEditor.View);
            Assert.AreEqual("int", parser.ElementAt(0).ValueEditor.ValueType);
            Assert.AreEqual(2, parser.ElementAt(0).ValueEditor.Validators.Count());

            Assert.AreEqual(new Guid("1FCF5C39-5FC7-4BCE-AFBE-6500D9EBA261"), parser.ElementAt(1).Id);
            Assert.AreEqual("Test 2", parser.ElementAt(1).Name);
        }

        [Test]
        public void Merge_JArrays()
        {
            var obj1 = JArray.FromObject(new[] { "test1", "test2", "test3" });
            var obj2 = JArray.FromObject(new[] { "test1", "test2", "test3", "test4" });
            
            ManifestParser.MergeJArrays(obj1, obj2);

            Assert.AreEqual(4, obj1.Count());
        }

        [Test]
        public void Merge_JObjects_Replace_Original()
        {
            var obj1 = JObject.FromObject(new
                {
                    Property1 = "Value1",
                    Property2 = "Value2",
                    Property3 = "Value3"
                });

            var obj2 = JObject.FromObject(new
            {
                Property3 = "Value3/2",
                Property4 = "Value4",
                Property5 = "Value5"
            });

            ManifestParser.MergeJObjects(obj1, obj2);

            Assert.AreEqual(5, obj1.Properties().Count());
            Assert.AreEqual("Value3/2", obj1.Properties().ElementAt(2).Value.Value<string>());
        }

        [Test]
        public void Merge_JObjects_Keep_Original()
        {
            var obj1 = JObject.FromObject(new
            {
                Property1 = "Value1",
                Property2 = "Value2",
                Property3 = "Value3"
            });

            var obj2 = JObject.FromObject(new
            {
                Property3 = "Value3/2",
                Property4 = "Value4",
                Property5 = "Value5"
            });

            ManifestParser.MergeJObjects(obj1, obj2, true);

            Assert.AreEqual(5, obj1.Properties().Count());
            Assert.AreEqual("Value3", obj1.Properties().ElementAt(2).Value.Value<string>());
        }

        

        [TestCase("C:\\Test", "C:\\Test\\MyFolder\\AnotherFolder", 2)]
        [TestCase("C:\\Test", "C:\\Test\\MyFolder\\AnotherFolder\\YetAnother", 3)]
        [TestCase("C:\\Test", "C:\\Test\\", 0)]
        public void Get_Folder_Depth(string baseFolder, string currFolder, int expected)
        {
            Assert.AreEqual(expected,
                ManifestParser.FolderDepth(
                new DirectoryInfo(baseFolder), 
                new DirectoryInfo(currFolder)));
        }

       

        //[Test]
        //public void Parse_Property_Editor()
        //{

        //}

        [Test]
        public void Create_Manifest_From_File_Content()
        {
            var content1 = "{}";
            var content2 = "{javascript: []}";
            var content3 = "{javascript: ['~/test.js', '~/test2.js']}";
            var content4 = "{propertyEditors: [], javascript: ['~/test.js', '~/test2.js']}";

            var result = ManifestParser.CreateManifests(null, content1, content2, content3, content4);

            Assert.AreEqual(4, result.Count());
            Assert.AreEqual(0, result.ElementAt(1).JavaScriptInitialize.Count);
            Assert.AreEqual(2, result.ElementAt(2).JavaScriptInitialize.Count);
            Assert.AreEqual(2, result.ElementAt(3).JavaScriptInitialize.Count);
        }

        

    }
}
