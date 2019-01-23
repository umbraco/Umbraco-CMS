using System;
using System.Configuration;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Umbraco.Core.Dashboards;
using Umbraco.Tests.TestHelpers;

namespace Umbraco.Tests.Configurations.DashboardSettings
{
    //[Obsolete("Dashboard tests not in config/XML", true)]
    //[TestFixture]
    //public class DashboardSettingsTests
    //{
    //    [SetUp]
    //    public void Init()
    //    {
    //        var config = new FileInfo(TestHelper.MapPathForTest("~/Configurations/DashboardSettings/web.config"));

    //        var fileMap = new ExeConfigurationFileMap() { ExeConfigFilename = config.FullName };
    //        var configuration = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);

    //        SettingsSection = configuration.GetSection("umbracoConfiguration/dashBoard") as DashboardSection;

    //        Assert.IsNotNull(SettingsSection);
    //    }

    //    protected IDashboardSection SettingsSection { get; private set; }

    //    [Test]
    //    public void Test_Sections()
    //    {
    //        Assert.AreEqual(5, SettingsSection.Sections.Count());

    //        Assert.AreEqual("StartupSettingsDashboardSection", SettingsSection.Sections.ElementAt(0).Alias);
    //        Assert.AreEqual("StartupDeveloperDashboardSection", SettingsSection.Sections.ElementAt(1).Alias);
    //        Assert.AreEqual("StartupMediaDashboardSection", SettingsSection.Sections.ElementAt(2).Alias);
    //        Assert.AreEqual("StartupDashboardSection", SettingsSection.Sections.ElementAt(3).Alias);
    //        Assert.AreEqual("StartupMemberDashboardSection", SettingsSection.Sections.ElementAt(4).Alias);
    //    }

    //    [Test]
    //    public void Test_Section_Area()
    //    {
    //        Assert.AreEqual("settings", SettingsSection.Sections.ElementAt(0).Areas.First());
    //        Assert.AreEqual("developer", SettingsSection.Sections.ElementAt(1).Areas.First());
    //        Assert.AreEqual("media", SettingsSection.Sections.ElementAt(2).Areas.First());
    //        Assert.AreEqual("content", SettingsSection.Sections.ElementAt(3).Areas.First());
    //        Assert.AreEqual("default", SettingsSection.Sections.ElementAt(4).Areas.First());
    //        Assert.AreEqual("member", SettingsSection.Sections.ElementAt(4).Areas.Last());
    //    }

    //    [Test]
    //    public void Test_Section_Access()
    //    {

    //        Assert.AreEqual(3, SettingsSection.Sections.ElementAt(3).AccessRights.Rules.Count());

    //        Assert.AreEqual("translator", SettingsSection.Sections.ElementAt(3).AccessRights.Rules.ElementAt(0).Value);
    //        Assert.AreEqual(AccessRuleType.Deny, SettingsSection.Sections.ElementAt(3).AccessRights.Rules.ElementAt(0).Type);
    //        Assert.AreEqual("hello", SettingsSection.Sections.ElementAt(3).AccessRights.Rules.ElementAt(1).Value);
    //        Assert.AreEqual(AccessRuleType.Grant, SettingsSection.Sections.ElementAt(3).AccessRights.Rules.ElementAt(1).Type);
    //        Assert.AreEqual("world", SettingsSection.Sections.ElementAt(3).AccessRights.Rules.ElementAt(2).Value);
    //        Assert.AreEqual(AccessRuleType.GrantBySection, SettingsSection.Sections.ElementAt(3).AccessRights.Rules.ElementAt(2).Type);
    //    }

    //    [Test]
    //    public void Test_Section_Tabs()
    //    {
    //        //Element 0 Alias "StartupSettingsDashboardSection"
    //        Assert.AreEqual(2, SettingsSection.Sections.ElementAt(0).Tabs.Count());

    //        //Element 1 Alias "StartupDeveloperDashboardSection"
    //        Assert.AreEqual(1, SettingsSection.Sections.ElementAt(1).Tabs.Count());

    //        //Element 2 Alias "StartupMediaDashboardSection"
    //        Assert.AreEqual(2, SettingsSection.Sections.ElementAt(2).Tabs.Count());

    //        //Element 3 Alias "StartupDashboardSection"
    //        Assert.AreEqual(3, SettingsSection.Sections.ElementAt(3).Tabs.Count());

    //        //Element 4 Alias "StartupMemberDashboardSection"
    //        Assert.AreEqual(1, SettingsSection.Sections.ElementAt(4).Tabs.Count());

    //    }

    //    [Test]
    //    public void Test_Tab()
    //    {
    //        Assert.AreEqual("Get Started", SettingsSection.Sections.ElementAt(0).Tabs.ElementAt(0).Caption);
    //        Assert.AreEqual(2, SettingsSection.Sections.ElementAt(0).Tabs.ElementAt(0).Controls.Count());
    //    }

    //    [Test]
    //    public void Test_Tab_Access()
    //    {
    //        Assert.AreEqual(1, SettingsSection.Sections.ElementAt(2).Tabs.ElementAt(1).AccessRights.Rules.Count());
    //        Assert.AreEqual(AccessRuleType.Grant, SettingsSection.Sections.ElementAt(2).Tabs.ElementAt(1).AccessRights.Rules.ElementAt(0).Type);
    //        Assert.AreEqual("admin", SettingsSection.Sections.ElementAt(2).Tabs.ElementAt(1).AccessRights.Rules.ElementAt(0).Value);
    //    }

    //    [Test]
    //    public void Test_Control()
    //    {
    //        Assert.AreEqual("hello", SettingsSection.Sections.ElementAt(0).Tabs.ElementAt(0).Controls.ElementAt(0).PanelCaption);
    //        Assert.AreEqual("views/dashboard/settings/settingsdashboardintro.html",
    //            SettingsSection.Sections.ElementAt(0).Tabs.ElementAt(0).Controls.ElementAt(0).ControlPath);

    //        Assert.AreEqual("", SettingsSection.Sections.ElementAt(0).Tabs.ElementAt(0).Controls.ElementAt(1).PanelCaption);
    //        Assert.AreEqual("views/dashboard/settings/settingsdashboardvideos.html",
    //            SettingsSection.Sections.ElementAt(0).Tabs.ElementAt(0).Controls.ElementAt(1).ControlPath);
    //    }

    //    [Test]
    //    public void Test_Control_Access()
    //    {
    //        Assert.AreEqual(2, SettingsSection.Sections.ElementAt(3).Tabs.ElementAt(0).Controls.ElementAt(1).AccessRights.Rules.Count());
    //        Assert.AreEqual(AccessRuleType.Deny, SettingsSection.Sections.ElementAt(3).Tabs.ElementAt(0).Controls.ElementAt(1).AccessRights.Rules.ElementAt(0).Type);
    //        Assert.AreEqual("editor", SettingsSection.Sections.ElementAt(3).Tabs.ElementAt(0).Controls.ElementAt(1).AccessRights.Rules.ElementAt(0).Value);
    //        Assert.AreEqual(AccessRuleType.Deny, SettingsSection.Sections.ElementAt(3).Tabs.ElementAt(0).Controls.ElementAt(1).AccessRights.Rules.ElementAt(1).Type);
    //        Assert.AreEqual("writer", SettingsSection.Sections.ElementAt(3).Tabs.ElementAt(0).Controls.ElementAt(1).AccessRights.Rules.ElementAt(1).Value);
    //    }
    //}
}
