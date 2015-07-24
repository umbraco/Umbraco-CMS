using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Core.Manifest;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Tests.Manifest
{    
    [TestFixture]
    public class ManifestParserTests
    {
        
        [Test]
        public void Parse_Property_Editors_With_Pre_Vals()
        {

            var a = JsonConvert.DeserializeObject<JArray>(@"[
    {
        alias: 'Test.Test1',
        name: 'Test 1',        
        editor: {
            view: '~/App_Plugins/MyPackage/PropertyEditors/MyEditor.html',
            valueType: 'int',
            validation: {
                'required': true,               
                'Regex': '\\d*'
            }
        },
        prevalues: {
				fields: [
					{
                        label: 'Some config 1',
						key: 'key1',
						view: '~/App_Plugins/MyPackage/PropertyEditors/Views/pre-val1.html',
						validation: {
                            required: true
                        }
					},
                    {
                        label: 'Some config 2',
						key: 'key2',
						view: '~/App_Plugins/MyPackage/PropertyEditors/Views/pre-val2.html'
					}
				]
			}
    }
]");
            var parser = ManifestParser.GetPropertyEditors(a);

            Assert.AreEqual(1, parser.Count());
            Assert.AreEqual(2, parser.ElementAt(0).PreValueEditor.Fields.Count());
            Assert.AreEqual("key1", parser.ElementAt(0).PreValueEditor.Fields.ElementAt(0).Key);
            Assert.AreEqual("Some config 1", parser.ElementAt(0).PreValueEditor.Fields.ElementAt(0).Name);
            Assert.AreEqual("/App_Plugins/MyPackage/PropertyEditors/Views/pre-val1.html", parser.ElementAt(0).PreValueEditor.Fields.ElementAt(0).View);
            Assert.AreEqual(1, parser.ElementAt(0).PreValueEditor.Fields.ElementAt(0).Validators.Count());
            
            Assert.AreEqual("key2", parser.ElementAt(0).PreValueEditor.Fields.ElementAt(1).Key);
            Assert.AreEqual("Some config 2", parser.ElementAt(0).PreValueEditor.Fields.ElementAt(1).Name);
            Assert.AreEqual("/App_Plugins/MyPackage/PropertyEditors/Views/pre-val2.html", parser.ElementAt(0).PreValueEditor.Fields.ElementAt(1).View);
            Assert.AreEqual(0, parser.ElementAt(0).PreValueEditor.Fields.ElementAt(1).Validators.Count());
        }

        [Test]
        public void Parse_Grid_Editors()
        {
            var a = JsonConvert.DeserializeObject<JArray>(@"[
    {
        alias: 'Test.Test1',
        name: 'Test 1',        
        view: 'blah',    
        icon: 'hello'
    },
    {
        alias: 'Test.Test2',
        name: 'Test 2',        
        config: { key1: 'some default val' },
        view: '~/hello/world.cshtml',
        icon: 'helloworld'
    },
    {
        alias: 'Test.Test3',
        name: 'Test 3',        
        config: { key1: 'some default val' },
        view: '/hello/world.html',
        render: '~/hello/world.cshtml',
        icon: 'helloworld'
    }
]");
            var parser = ManifestParser.GetGridEditors(a).ToArray();

            Assert.AreEqual(3, parser.Count());

            Assert.AreEqual("Test.Test1", parser.ElementAt(0).Alias);
            Assert.AreEqual("Test 1", parser.ElementAt(0).Name);
            Assert.AreEqual("blah", parser.ElementAt(0).View);
            Assert.AreEqual("hello", parser.ElementAt(0).Icon);
            Assert.IsNull(parser.ElementAt(0).Render);
            Assert.AreEqual(0, parser.ElementAt(0).Config.Count);

            Assert.AreEqual("Test.Test2", parser.ElementAt(1).Alias);
            Assert.AreEqual("Test 2", parser.ElementAt(1).Name);
            Assert.AreEqual("/hello/world.cshtml", parser.ElementAt(1).View);
            Assert.AreEqual("helloworld", parser.ElementAt(1).Icon);
            Assert.IsNull(parser.ElementAt(1).Render);
            Assert.AreEqual(1, parser.ElementAt(1).Config.Count);
            Assert.AreEqual("some default val", parser.ElementAt(1).Config["key1"]);

            Assert.AreEqual("Test.Test3", parser.ElementAt(2).Alias);
            Assert.AreEqual("Test 3", parser.ElementAt(2).Name);
            Assert.AreEqual("/hello/world.html", parser.ElementAt(2).View);
            Assert.AreEqual("helloworld", parser.ElementAt(2).Icon);
            Assert.AreEqual("/hello/world.cshtml", parser.ElementAt(2).Render);
            Assert.AreEqual(1, parser.ElementAt(2).Config.Count);
            Assert.AreEqual("some default val", parser.ElementAt(2).Config["key1"]);
            
        }

        [Test]
        public void Parse_Property_Editors()
        {

            var a = JsonConvert.DeserializeObject<JArray>(@"[
    {
        alias: 'Test.Test1',
        name: 'Test 1',        
        editor: {
            view: '~/App_Plugins/MyPackage/PropertyEditors/MyEditor.html',
            valueType: 'int',
            validation: {
                required : true,
                regex : '\\d*'
            }
        }
    },
    {
        alias: 'Test.Test2',
        name: 'Test 2',        
        defaultConfig: { key1: 'some default pre val' },
        editor: {
            view: '~/App_Plugins/MyPackage/PropertyEditors/CsvEditor.html',
            hideLabel: true
        }
    }
]");
            var parser = ManifestParser.GetPropertyEditors(a);

            Assert.AreEqual(2, parser.Count());

            Assert.AreEqual(false, parser.ElementAt(0).ValueEditor.HideLabel);
            Assert.AreEqual("Test.Test1", parser.ElementAt(0).Alias);
            Assert.AreEqual("Test 1", parser.ElementAt(0).Name);
            Assert.AreEqual("/App_Plugins/MyPackage/PropertyEditors/MyEditor.html", parser.ElementAt(0).ValueEditor.View);
            Assert.AreEqual("int", parser.ElementAt(0).ValueEditor.ValueType);
            Assert.AreEqual(2, parser.ElementAt(0).ValueEditor.Validators.Count());
            var manifestValidator1 = parser.ElementAt(0).ValueEditor.Validators.ElementAt(0) as ManifestPropertyValidator;
            Assert.IsNotNull(manifestValidator1);
            Assert.AreEqual("required", manifestValidator1.Type);
            var manifestValidator2 = parser.ElementAt(0).ValueEditor.Validators.ElementAt(1) as ManifestPropertyValidator;
            Assert.IsNotNull(manifestValidator2);
            Assert.AreEqual("regex", manifestValidator2.Type);

            Assert.AreEqual(true, parser.ElementAt(1).ValueEditor.HideLabel);
            Assert.AreEqual("Test.Test2", parser.ElementAt(1).Alias);
            Assert.AreEqual("Test 2", parser.ElementAt(1).Name);
            Assert.IsTrue(parser.ElementAt(1).DefaultPreValues.ContainsKey("key1"));
            Assert.AreEqual("some default pre val", parser.ElementAt(1).DefaultPreValues["key1"]);
        }

        [Test]
        public void Property_Editors_Can_Be_Parameter_Editor()
        {

            var a = JsonConvert.DeserializeObject<JArray>(@"[
    {
        alias: 'Test.Test1',
        name: 'Test 1',   
        isParameterEditor: true,     
        defaultConfig: { key1: 'some default val' },
        editor: {
            view: '~/App_Plugins/MyPackage/PropertyEditors/MyEditor.html',
            valueType: 'int',
            validation: {
                required : true,
                regex : '\\d*'
            }
        }
    },
    {
        alias: 'Test.Test2',
        name: 'Test 2',
        defaultConfig: { key1: 'some default pre val' },
        editor: {
            view: '~/App_Plugins/MyPackage/PropertyEditors/CsvEditor.html'
        }
    }
]");
            var parser = ManifestParser.GetPropertyEditors(a);

            Assert.AreEqual(1, parser.Count(x => x.IsParameterEditor));

            IParameterEditor parameterEditor = parser.First();
            Assert.AreEqual(1, parameterEditor.Configuration.Count);
            Assert.IsTrue(parameterEditor.Configuration.ContainsKey("key1"));
            Assert.AreEqual("some default val", parameterEditor.Configuration["key1"]);
        }

        [Test]
        public void Parse_Parameter_Editors()
        {

            var a = JsonConvert.DeserializeObject<JArray>(@"[
    {
        alias: 'parameter1',
        name: 'My Parameter',        
        view: '~/App_Plugins/MyPackage/PropertyEditors/MyEditor.html'
    },
    {
        alias: 'parameter2',
        name: 'Another parameter',
        config: { key1: 'some config val' },
        view: '~/App_Plugins/MyPackage/PropertyEditors/CsvEditor.html'
    }
]");
            var parser = ManifestParser.GetParameterEditors(a);

            Assert.AreEqual(2, parser.Count());
            Assert.AreEqual("parameter1", parser.ElementAt(0).Alias);
            Assert.AreEqual("My Parameter", parser.ElementAt(0).Name);
            Assert.AreEqual("/App_Plugins/MyPackage/PropertyEditors/MyEditor.html", parser.ElementAt(0).ValueEditor.View);

            Assert.AreEqual("parameter2", parser.ElementAt(1).Alias);
            Assert.AreEqual("Another parameter", parser.ElementAt(1).Name);
            Assert.IsTrue(parser.ElementAt(1).Configuration.ContainsKey("key1"));
            Assert.AreEqual("some config val", parser.ElementAt(1).Configuration["key1"]);
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
        public void Create_Manifests_Editors()
        {
            var package1 = @"{
propertyEditors: [], 
javascript: ['~/test.js', '~/test2.js']}";
            
            var package2 = "{css: ['~/style.css', '~/folder-name/sdsdsd/stylesheet.css']}";

            var package3 = @"{
    'javascript': [    ],
    'css': [     ],
    'gridEditors': [
        {
            'name': 'Small Hero',
            'alias': 'small-hero',
            'view': '/App_Plugins/MyPlugin/small-hero/editortemplate.html',
            'render': '/Views/Partials/Grid/Editors/SmallHero.cshtml',
            'icon': 'icon-presentation',
            'config': {
                'image': {
                    'size': {
                        'width': 1200,
                        'height': 185
                    }
                },
                'link': {
                    'maxNumberOfItems': 1,
                    'minNumberOfItems': 0
                }
            }
        },
        {
            'name': 'Document Links By Category',
            'alias': 'document-links-by-category',
            'view': '/App_Plugins/MyPlugin/document-links-by-category/editortemplate.html',
            'render': '/Views/Partials/Grid/Editors/DocumentLinksByCategory.cshtml',
            'icon': 'icon-umb-members'
        }
    ]
}";
            var package4 = @"{'propertyEditors': [
    {
        alias: 'Test.Test1',
        name: 'Test 1',        
        editor: {
            view: '~/App_Plugins/MyPackage/PropertyEditors/MyEditor.html',
            valueType: 'int',
            validation: {
                'required': true,               
                'Regex': '\\d*'
            }
        },
        prevalues: {
				fields: [
					{
                        label: 'Some config 1',
						key: 'key1',
						view: '~/App_Plugins/MyPackage/PropertyEditors/Views/pre-val1.html',
						validation: {
                            required: true
                        }
					},
                    {
                        label: 'Some config 2',
						key: 'key2',
						view: '~/App_Plugins/MyPackage/PropertyEditors/Views/pre-val2.html'
					}
				]
			}
    }
]}";

            var package5 = @"{'parameterEditors': [
    {
        alias: 'parameter1',
        name: 'My Parameter',        
        view: '~/App_Plugins/MyPackage/PropertyEditors/MyEditor.html'
    },
    {
        alias: 'parameter2',
        name: 'Another parameter',
        config: { key1: 'some config val' },
        view: '~/App_Plugins/MyPackage/PropertyEditors/CsvEditor.html'
    }
]}";

            var result = ManifestParser.CreateManifests(package1, package2, package3, package4, package5).ToArray();
            
            var paramEditors = result.SelectMany(x => ManifestParser.GetParameterEditors(x.ParameterEditors)).ToArray();
            var propEditors = result.SelectMany(x => ManifestParser.GetPropertyEditors(x.PropertyEditors)).ToArray();
            var gridEditors = result.SelectMany(x => ManifestParser.GetGridEditors(x.GridEditors)).ToArray();
            
            Assert.AreEqual(2, gridEditors.Count());
            Assert.AreEqual(2, paramEditors.Count());
            Assert.AreEqual(1, propEditors.Count()); 

        }

        [Test]
        public void Create_Manifest_With_Line_Comments()
        {
            var content4 = @"{
//here's the property editors
propertyEditors: [], 
//and here's the javascript
javascript: ['~/test.js', '~/test2.js']}";

            var result = ManifestParser.CreateManifests(null, content4);

            Assert.AreEqual(1, result.Count()); 
        }

        [Test]
        public void Create_Manifest_With_Surround_Comments()
        {
            var content4 = @"{
propertyEditors: []/*we have empty property editors**/, 
javascript: ['~/test.js',/*** some note about stuff asd09823-4**09234*/ '~/test2.js']}";

            var result = ManifestParser.CreateManifests(null, content4);

            Assert.AreEqual(1, result.Count());
        }

        [Test]
        public void Create_Manifest_With_Error()
        {
            //NOTE: This is missing the final closing ]
            var content4 = @"{
propertyEditors: []/*we have empty property editors**/, 
javascript: ['~/test.js',/*** some note about stuff asd09823-4**09234*/ '~/test2.js' }";

            var result = ManifestParser.CreateManifests(null, content4);

            //an error has occurred and been logged but processing continues
            Assert.AreEqual(0, result.Count());
        }

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

        [Test]
        public void Parse_Stylesheet_Initialization()
        {
            var content1 = "{}";
            var content2 = "{css: []}";
            var content3 = "{css: ['~/style.css', '~/folder-name/sdsdsd/stylesheet.css']}";
            var content4 = "{propertyEditors: [], css: ['~/stylesheet.css', '~/random-long-name.css']}";

            var result = ManifestParser.CreateManifests(null, content1, content2, content3, content4);

            Assert.AreEqual(4, result.Count());
            Assert.AreEqual(0, result.ElementAt(1).StylesheetInitialize.Count);
            Assert.AreEqual(2, result.ElementAt(2).StylesheetInitialize.Count);
            Assert.AreEqual(2, result.ElementAt(3).StylesheetInitialize.Count);
        }
        

    }
}
