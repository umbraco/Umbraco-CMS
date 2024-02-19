// Copyright (c) Umbraco.
// See LICENSE for more details.

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
public class LegacyManifestParserTests
{
    [SetUp]
    public void Setup()
    {
        var validators = new IManifestValueValidator[]
        {
            new RequiredValidator(),
            new RegexValidator(),
            new DelimitedValueValidator(),
        };
        _ioHelper = TestHelper.IOHelper;
        var loggerFactory = NullLoggerFactory.Instance;
        _parser = new LegacyManifestParser(
            AppCaches.Disabled,
            new ManifestValueValidatorCollection(() => validators),
            new LegacyManifestFilterCollection(() => Enumerable.Empty<ILegacyManifestFilter>()),
            loggerFactory.CreateLogger<LegacyManifestParser>(),
            _ioHelper,
            TestHelper.GetHostingEnvironment(),
            new JsonNetSerializer(),
            Mock.Of<ILocalizedTextService>(),
            Mock.Of<IShortStringHelper>(),
            Mock.Of<IDataValueEditorFactory>(),
            Mock.Of<ILegacyPackageManifestFileProviderFactory>());
    }

    private LegacyManifestParser _parser;
    private IIOHelper _ioHelper;

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

        Assert.IsInstanceOf<LegacyManifestContentAppDefinition>(manifest.ContentApps[0]);
        var app0 = manifest.ContentApps[0];
        Assert.AreEqual("myPackageApp1", app0.Alias);
        Assert.AreEqual("My App1", app0.Name);
        Assert.AreEqual("icon-foo", app0.Icon);
        Assert.AreEqual(_ioHelper.ResolveUrl("/App_Plugins/MyPackage/ContentApps/MyApp1.html"), app0.View);

        Assert.IsInstanceOf<LegacyManifestContentAppDefinition>(manifest.ContentApps[1]);
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

        Assert.IsInstanceOf<LegacyManifestDashboard>(manifest.Dashboards[0]);
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

        Assert.IsInstanceOf<LegacyManifestDashboard>(manifest.Dashboards[1]);
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
}
