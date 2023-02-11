// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Dashboards;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Manifest;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PropertyEditors.Validators;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.Serialization;
using Umbraco.Cms.Tests.UnitTests.TestHelpers;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Manifest;

[TestFixture]
public class ManifestParserTests
{
    [SetUp]
    public void Setup()
    {
        var validators = new IManifestValueValidator[]
        {
            new RequiredValidator(Mock.Of<ILocalizedTextService>()),
            new RegexValidator(Mock.Of<ILocalizedTextService>(), null),
            new DelimitedValueValidator(),
        };
        _ioHelper = TestHelper.IOHelper;
        var loggerFactory = NullLoggerFactory.Instance;
        _parser = new ManifestParser(
            AppCaches.Disabled,
            new ManifestValueValidatorCollection(() => validators),
            new ManifestFilterCollection(() => Enumerable.Empty<IManifestFilter>()),
            loggerFactory.CreateLogger<ManifestParser>(),
            _ioHelper,
            TestHelper.GetHostingEnvironment(),
            new JsonNetSerializer(),
            Mock.Of<ILocalizedTextService>(),
            Mock.Of<IShortStringHelper>(),
            Mock.Of<IDataValueEditorFactory>());
    }

    private ManifestParser _parser;
    private IIOHelper _ioHelper;

    [Test]
    public void DelimitedValueValidator()
    {
        const string json = @"{'propertyEditors': [
    {
        alias: 'Test.Test2',
        name: 'Test 2',
        isParameterEditor: true,
        defaultConfig: { key1: 'some default val' },
        editor: {
            view: '~/App_Plugins/MyPackage/PropertyEditors/MyEditor.html',
            valueType: 'int',
            validation: {
                delimited: {
                    delimiter: ',',
                    pattern: '^[a-zA-Z]*$'
                }
            }
        }
    }
]}";

        var manifest = _parser.ParseManifest(json);

        Assert.AreEqual(1, manifest.ParameterEditors.Length);
        Assert.AreEqual(1, manifest.ParameterEditors[0].GetValueEditor().Validators.Count);

        Assert.IsTrue(manifest.ParameterEditors[0].GetValueEditor().Validators[0] is DelimitedValueValidator);
        var validator = manifest.ParameterEditors[0].GetValueEditor().Validators[0] as DelimitedValueValidator;

        Assert.IsNotNull(validator.Configuration);
        Assert.AreEqual(",", validator.Configuration.Delimiter);
        Assert.AreEqual("^[a-zA-Z]*$", validator.Configuration.Pattern);
    }

    [Test]
    public void CanParseComments()
    {
        const string json1 = @"
// this is a single-line comment
{
    ""x"": 2, // this is an end-of-line comment
    ""y"": 3, /* this is a single line comment block
/* comment */ ""z"": /* comment */ 4,
    ""t"": ""this is /* comment */ a string"",
    ""u"": ""this is // more comment in a string""
}
";

        var jobject = (JObject)JsonConvert.DeserializeObject(json1);
        Assert.AreEqual("2", jobject.Property("x").Value.ToString());
        Assert.AreEqual("3", jobject.Property("y").Value.ToString());
        Assert.AreEqual("4", jobject.Property("z").Value.ToString());
        Assert.AreEqual("this is /* comment */ a string", jobject.Property("t").Value.ToString());
        Assert.AreEqual("this is // more comment in a string", jobject.Property("u").Value.ToString());
    }

    [Test]
    public void ThrowOnJsonError()
    {
        // invalid json, missing the final ']' on javascript
        const string json = @"{
propertyEditors: []/*we have empty property editors**/,
javascript: ['~/test.js',/*** some note about stuff asd09823-4**09234*/ '~/test2.js' }";

        // parsing fails
        Assert.Throws<JsonReaderException>(() => _parser.ParseManifest(json));
    }

    [Test]
    public void CanParseManifest_ScriptsAndStylesheets()
    {
        var json = "{}";
        var manifest = _parser.ParseManifest(json);
        Assert.AreEqual(0, manifest.Scripts.Length);

        json = "{javascript: []}";
        manifest = _parser.ParseManifest(json);
        Assert.AreEqual(0, manifest.Scripts.Length);

        json = "{javascript: ['~/test.js', '~/test2.js']}";
        manifest = _parser.ParseManifest(json);
        Assert.AreEqual(2, manifest.Scripts.Length);

        json = "{propertyEditors: [], javascript: ['~/test.js', '~/test2.js']}";
        manifest = _parser.ParseManifest(json);
        Assert.AreEqual(2, manifest.Scripts.Length);

        Assert.AreEqual(_ioHelper.ResolveUrl("/test.js"), manifest.Scripts[0]);
        Assert.AreEqual(_ioHelper.ResolveUrl("/test2.js"), manifest.Scripts[1]);

        // kludge is gone - must filter before parsing
        json = Encoding.UTF8.GetString(Encoding.UTF8.GetPreamble()) +
               "{propertyEditors: [], javascript: ['~/test.js', '~/test2.js']}";
        Assert.Throws<JsonReaderException>(() => _parser.ParseManifest(json));

        json = "{}";
        manifest = _parser.ParseManifest(json);
        Assert.AreEqual(0, manifest.Stylesheets.Length);

        json = "{css: []}";
        manifest = _parser.ParseManifest(json);
        Assert.AreEqual(0, manifest.Stylesheets.Length);

        json = "{css: ['~/style.css', '~/folder-name/sdsdsd/stylesheet.css']}";
        manifest = _parser.ParseManifest(json);
        Assert.AreEqual(2, manifest.Stylesheets.Length);

        json = "{propertyEditors: [], css: ['~/stylesheet.css', '~/random-long-name.css']}";
        manifest = _parser.ParseManifest(json);
        Assert.AreEqual(2, manifest.Stylesheets.Length);

        json =
            "{propertyEditors: [], javascript: ['~/test.js', '~/test2.js'], css: ['~/stylesheet.css', '~/random-long-name.css']}";
        manifest = _parser.ParseManifest(json);
        Assert.AreEqual(2, manifest.Scripts.Length);
        Assert.AreEqual(2, manifest.Stylesheets.Length);
    }

    [Test]
    public void CanParseManifest_PropertyEditors()
    {
        const string json = @"{'propertyEditors': [
    {
        alias: 'Test.Test1',
        name: 'Test 1',
        editor: {
            view: '~/App_Plugins/MyPackage/PropertyEditors/MyEditor.html',
            valueType: 'int',
            hideLabel: true,
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
    },
    {
        alias: 'Test.Test2',
        name: 'Test 2',
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
    }
]}";

        var manifest = _parser.ParseManifest(json);
        Assert.AreEqual(2, manifest.PropertyEditors.Length);

        var editor = manifest.PropertyEditors[1];
        Assert.IsTrue((editor.Type & EditorType.MacroParameter) > 0);
        Assert.IsNotEmpty(editor.DefaultConfiguration);
        Assert.AreEqual("some default val", editor.DefaultConfiguration["key1"]);

        editor = manifest.PropertyEditors[0];
        Assert.AreEqual("Test.Test1", editor.Alias);
        Assert.AreEqual("Test 1", editor.Name);
        Assert.IsFalse((editor.Type & EditorType.MacroParameter) > 0);

        var valueEditor = editor.GetValueEditor();
        Assert.AreEqual(_ioHelper.ResolveUrl("/App_Plugins/MyPackage/PropertyEditors/MyEditor.html"), valueEditor.View);
        Assert.AreEqual("int", valueEditor.ValueType);
        Assert.IsTrue(valueEditor.HideLabel);

        // these two don't make much sense here
        //// valueEditor.RegexValidator;
        //// valueEditor.RequiredValidator;

        var validators = valueEditor.Validators;
        Assert.AreEqual(2, validators.Count);
        var validator = validators[0];
        var v1 = validator as RequiredValidator;
        Assert.IsNotNull(v1);
        Assert.AreEqual("Required", v1.ValidationName);
        validator = validators[1];
        var v2 = validator as RegexValidator;
        Assert.IsNotNull(v2);
        Assert.AreEqual("Regex", v2.ValidationName);
        Assert.AreEqual("\\d*", v2.Configuration);

        // this is not part of the manifest
        var preValues = editor.GetConfigurationEditor().DefaultConfiguration;
        Assert.IsEmpty(preValues);

        var preValueEditor = editor.GetConfigurationEditor();
        Assert.IsNotNull(preValueEditor);
        Assert.IsNotNull(preValueEditor.Fields);
        Assert.AreEqual(2, preValueEditor.Fields.Count);

        var f = preValueEditor.Fields[0];
        Assert.AreEqual("key1", f.Key);
        Assert.AreEqual("Some config 1", f.Name);
        Assert.AreEqual(_ioHelper.ResolveUrl("/App_Plugins/MyPackage/PropertyEditors/Views/pre-val1.html"), f.View);
        var fvalidators = f.Validators;
        Assert.IsNotNull(fvalidators);
        Assert.AreEqual(1, fvalidators.Count);
        var fv = fvalidators[0] as RequiredValidator;
        Assert.IsNotNull(fv);
        Assert.AreEqual("Required", fv.ValidationName);

        f = preValueEditor.Fields[1];
        Assert.AreEqual("key2", f.Key);
        Assert.AreEqual("Some config 2", f.Name);
        Assert.AreEqual(_ioHelper.ResolveUrl("/App_Plugins/MyPackage/PropertyEditors/Views/pre-val2.html"), f.View);
        fvalidators = f.Validators;
        Assert.IsNotNull(fvalidators);
        Assert.AreEqual(0, fvalidators.Count);
    }

    [Test]
    public void CanParseManifest_ParameterEditors()
    {
        const string json = @"{'parameterEditors': [
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
    },
    {
        alias: 'parameter3',
        name: 'Yet another parameter'
    }
]}";

        var manifest = _parser.ParseManifest(json);
        Assert.AreEqual(3, manifest.ParameterEditors.Length);

        Assert.IsTrue(manifest.ParameterEditors.All(x => (x.Type & EditorType.MacroParameter) > 0));

        var editor = manifest.ParameterEditors[1];
        Assert.AreEqual("parameter2", editor.Alias);
        Assert.AreEqual("Another parameter", editor.Name);

        var config = editor.DefaultConfiguration;
        Assert.AreEqual(1, config.Count);
        Assert.IsTrue(config.ContainsKey("key1"));
        Assert.AreEqual("some config val", config["key1"]);

        var valueEditor = editor.GetValueEditor();
        Assert.AreEqual(_ioHelper.ResolveUrl("/App_Plugins/MyPackage/PropertyEditors/CsvEditor.html"), valueEditor.View);

        editor = manifest.ParameterEditors[2];
        Assert.Throws<InvalidOperationException>(() =>
        {
            var valueEditor = editor.GetValueEditor();
        });
    }

    [Test]
    public void CanParseManifest_GridEditors()
    {
        const string json = @"{
    'javascript': [    ],
    'css': [     ],
    'gridEditors': [
        {
            'name': 'Small Hero',
            'alias': 'small-hero',
            'view': '~/App_Plugins/MyPlugin/small-hero/editortemplate.html',
            'render': '~/Views/Partials/Grid/Editors/SmallHero.cshtml',
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
            'view': '~/App_Plugins/MyPlugin/document-links-by-category/editortemplate.html',
            'render': '~/Views/Partials/Grid/Editors/DocumentLinksByCategory.cshtml',
            'icon': 'icon-umb-members'
        }
    ]
}";
        var manifest = _parser.ParseManifest(json);
        Assert.AreEqual(2, manifest.GridEditors.Length);

        var editor = manifest.GridEditors[0];
        Assert.AreEqual("small-hero", editor.Alias);
        Assert.AreEqual("Small Hero", editor.Name);
        Assert.AreEqual(_ioHelper.ResolveUrl("/App_Plugins/MyPlugin/small-hero/editortemplate.html"), editor.View);
        Assert.AreEqual(_ioHelper.ResolveUrl("/Views/Partials/Grid/Editors/SmallHero.cshtml"), editor.Render);
        Assert.AreEqual("icon-presentation", editor.Icon);

        var config = editor.Config;
        Assert.AreEqual(2, config.Count);
        Assert.IsTrue(config.ContainsKey("image"));
        var c = config["image"];
        Assert.IsInstanceOf<JObject>(c); // FIXME: is this what we want?
        Assert.IsTrue(config.ContainsKey("link"));
        c = config["link"];
        Assert.IsInstanceOf<JObject>(c); // FIXME: is this what we want?

        // FIXME: should we resolveUrl in configs?
    }

    [Test]
    public void CanParseManifest_ContentApps()
    {
        const string json = @"{'contentApps': [
    {
        alias: 'myPackageApp1',
        name: 'My App1',
        icon: 'icon-foo',
        view: '~/App_Plugins/MyPackage/ContentApps/MyApp1.html'
    },
    {
        alias: 'myPackageApp2',
        name: 'My App2',
        config: { key1: 'some config val' },
        icon: 'icon-bar',
        view: '~/App_Plugins/MyPackage/ContentApps/MyApp2.html'
    }
]}";

        var manifest = _parser.ParseManifest(json);
        Assert.AreEqual(2, manifest.ContentApps.Length);

        Assert.IsInstanceOf<ManifestContentAppDefinition>(manifest.ContentApps[0]);
        var app0 = manifest.ContentApps[0];
        Assert.AreEqual("myPackageApp1", app0.Alias);
        Assert.AreEqual("My App1", app0.Name);
        Assert.AreEqual("icon-foo", app0.Icon);
        Assert.AreEqual(_ioHelper.ResolveUrl("/App_Plugins/MyPackage/ContentApps/MyApp1.html"), app0.View);

        Assert.IsInstanceOf<ManifestContentAppDefinition>(manifest.ContentApps[1]);
        var app1 = manifest.ContentApps[1];
        Assert.AreEqual("myPackageApp2", app1.Alias);
        Assert.AreEqual("My App2", app1.Name);
        Assert.AreEqual("icon-bar", app1.Icon);
        Assert.AreEqual(_ioHelper.ResolveUrl("/App_Plugins/MyPackage/ContentApps/MyApp2.html"), app1.View);
    }

    [Test]
    public void CanParseManifest_Dashboards()
    {
        const string json = @"{'dashboards': [
    {
        'alias': 'something',
        'view': '~/App_Plugins/MyPackage/Dashboards/one.html',
        'sections': [ 'content' ],
        'access': [ {'grant':'user'}, {'deny':'foo'} ]

    },
    {
        'alias': 'something.else',
        'weight': -1,
        'view': '~/App_Plugins/MyPackage/Dashboards/two.html',
        'sections': [ 'forms' ],
    }
]}";

        var manifest = _parser.ParseManifest(json);
        Assert.AreEqual(2, manifest.Dashboards.Length);

        Assert.IsInstanceOf<ManifestDashboard>(manifest.Dashboards[0]);
        var db0 = manifest.Dashboards[0];
        Assert.AreEqual("something", db0.Alias);
        Assert.AreEqual(100, db0.Weight);
        Assert.AreEqual(_ioHelper.ResolveUrl("/App_Plugins/MyPackage/Dashboards/one.html"), db0.View);
        Assert.AreEqual(1, db0.Sections.Length);
        Assert.AreEqual("content", db0.Sections[0]);
        Assert.AreEqual(2, db0.AccessRules.Length);
        Assert.AreEqual(AccessRuleType.Grant, db0.AccessRules[0].Type);
        Assert.AreEqual("user", db0.AccessRules[0].Value);
        Assert.AreEqual(AccessRuleType.Deny, db0.AccessRules[1].Type);
        Assert.AreEqual("foo", db0.AccessRules[1].Value);

        Assert.IsInstanceOf<ManifestDashboard>(manifest.Dashboards[1]);
        var db1 = manifest.Dashboards[1];
        Assert.AreEqual("something.else", db1.Alias);
        Assert.AreEqual(-1, db1.Weight);
        Assert.AreEqual(_ioHelper.ResolveUrl("/App_Plugins/MyPackage/Dashboards/two.html"), db1.View);
        Assert.AreEqual(1, db1.Sections.Length);
        Assert.AreEqual("forms", db1.Sections[0]);
    }

    [Test]
    public void CanParseManifest_Sections()
    {
        const string json = @"{'sections': [
    { ""alias"": ""content"", ""name"": ""Content"" },
    { ""alias"": ""hello"", ""name"": ""World"" }
]}";

        var manifest = _parser.ParseManifest(json);
        Assert.AreEqual(2, manifest.Sections.Length);
        Assert.AreEqual("content", manifest.Sections[0].Alias);
        Assert.AreEqual("hello", manifest.Sections[1].Alias);
        Assert.AreEqual("Content", manifest.Sections[0].Name);
        Assert.AreEqual("World", manifest.Sections[1].Name);
    }

    [Test]
    public void CanParseManifest_Version()
    {
        const string json = @"{""name"": ""VersionPackage"", ""version"": ""1.0.0""}";
        var manifest = _parser.ParseManifest(json);

        Assert.Multiple(() =>
        {
            Assert.AreEqual("VersionPackage", manifest.PackageName);
            Assert.AreEqual("1.0.0", manifest.Version);
        });
    }

    [Test]
    public void CanParseManifest_TrackingAllowed()
    {
        const string json = @"{""allowPackageTelemetry"": false }";
        var manifest = _parser.ParseManifest(json);

        Assert.IsFalse(manifest.AllowPackageTelemetry);
    }

    [Test]
    public void CanParseManifest_ParameterEditors_SupportsReadOnly()
    {
        const string json = @"{'parameterEditors': [
    {
        alias: 'parameter1',
        name: 'My Parameter',
        view: '~/App_Plugins/MyPackage/PropertyEditors/MyEditor.html',
        supportsReadOnly: true
    }]}";
        

        var manifest = _parser.ParseManifest(json);
        Assert.IsTrue(manifest.ParameterEditors.FirstOrDefault().SupportsReadOnly);
    }
    
    [Test]
    public void CanParseManifest_PropertyEditors_SupportsReadOnly()
    {
        const string json = @"{'propertyEditors': [
    {
        alias: 'Test.Test1',
        name: 'Test 1',
        supportsReadOnly: true,
        editor: {
            view: '~/App_Plugins/MyPackage/PropertyEditors/MyEditor.html',
            valueType: 'int',
            hideLabel: true,
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
    }]}";
        

        var manifest = _parser.ParseManifest(json);
        Assert.IsTrue(manifest.PropertyEditors.FirstOrDefault().SupportsReadOnly);
    }
}
