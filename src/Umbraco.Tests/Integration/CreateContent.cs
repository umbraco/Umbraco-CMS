using System;
using System.Runtime.InteropServices;
using System.Threading;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using Umbraco.Tests.TestHelpers;

namespace Umbraco.Tests.Integration
{
    [Ignore("We don't want to run Selenium tests on TeamCity")]
    [TestFixture]
    public class CreateContent : BaseSeleniumTest
    {
        [SetUp]
        public override void Initialize()
        {
            base.Initialize();
        }

        [TearDown]
        public void TearDown()
        {
            base.TearDown();
        }

        [Test]
        public void Install_StarterKit_And_Create_Textpage()
        {
            // Login to Umbraco
            Driver.Navigate().GoToUrl(BaseUrl + "/umbraco/login.aspx?redir=");
            Driver.FindElement(By.Id("lname")).Clear();
            Driver.FindElement(By.Id("lname")).SendKeys("admin");
            Driver.FindElement(By.Id("passw")).Clear();
            Driver.FindElement(By.Id("passw")).SendKeys("test");
            Driver.FindElement(By.Id("Button1")).Click();
            Thread.Sleep(500);

            // Install starter kit
            Driver.Navigate().GoToUrl(BaseUrl + "/umbraco/umbraco.aspx#developer");
            Thread.Sleep(2000);

            var builder = new Actions(Driver);
            var packagesNode = Driver.FindElement(By.XPath("//*[@id='init'][3]"));
            builder.MoveToElement(packagesNode).DoubleClick().Build().Perform();
            Thread.Sleep(500);

            var installStarterKitNode = Driver.FindElement(By.XPath("(//li[@id='-1']/a/div)[4]"));
            builder.MoveToElement(installStarterKitNode).Click().Build().Perform();
            Thread.Sleep(500);

            var rightFrame = Driver.FindElement(By.XPath("//*[@id='right']"));
            Driver.SwitchTo().Frame(rightFrame);
            Thread.Sleep(500);

            var simpleKit = Driver.FindElement(By.XPath("/html/body/form/div[2]/div[2]/div/div/div/div/div[4]/nav/ul/li[1]/a"));
            builder.MoveToElement(simpleKit).Click().Build().Perform();
            Thread.Sleep(6000);

            // Create new content
            Driver.SwitchTo().DefaultContent();
            Driver.Navigate().GoToUrl(BaseUrl + "/umbraco/umbraco.aspx#content");
            Thread.Sleep(500);

            var contentNode = Driver.FindElement(By.XPath("//*[@id = 'JTree_TreeContainer']/div/ul/li/ul/li/a/div"));
            builder.MoveToElement(contentNode).ContextClick().Build().Perform();
            Thread.Sleep(1000);

            var createButton = Driver.FindElement(By.XPath("//*[@id = 'jstree-contextmenu']/li[1]/a/span/div[2]"));
            createButton.Click();
            Thread.Sleep(500);

            var umbracoModalBoxIframe = Driver.FindElement(By.CssSelector(".umbModalBoxIframe"));
            Driver.SwitchTo().Frame(umbracoModalBoxIframe);

            var createTextBox = Driver.FindElement(By.XPath("//input[@type='text'][1]"));
            createTextBox.Clear();
            createTextBox.SendKeys("Test Page");

            var submitButton = Driver.FindElement(By.XPath("//input[@type='submit'][1]"));
            submitButton.Click();
            Thread.Sleep(2000);

            Driver.SwitchTo().Frame(rightFrame);
            var saveAndPublishButton = Driver.FindElement(By.XPath("//*[@id='body_TabView1_tab01layer_publish']"));
            saveAndPublishButton.Click();
            Thread.Sleep(2000);

            var contentArea = Driver.FindElement(By.XPath("//*[@id='body_TabView1']"));
            Assert.IsNotNull(contentArea);
            Thread.Sleep(500);

            // Perform delete
            Driver.SwitchTo().DefaultContent();
            var installingModulesNode = Driver.FindElement(By.XPath("//*[@id='1052']/a/div"));
            builder.MoveToElement(installingModulesNode).ContextClick().Build().Perform();
            Thread.Sleep(1000);

            var deleteButton = Driver.FindElement(By.CssSelector(".sprDelete"));
            deleteButton.Click();
            var alert = Driver.SwitchTo().Alert();
            alert.Accept();
            Thread.Sleep(1000);

            // Perform move
            var goFurtherNode = Driver.FindElement(By.XPath("//*[@id='1053']/a/div"));
            builder.MoveToElement(goFurtherNode).ContextClick().Build().Perform();
            Thread.Sleep(1000);

            var moveButton = Driver.FindElement(By.CssSelector(".sprMove"));
            moveButton.Click();
            Thread.Sleep(2000);

            umbracoModalBoxIframe = Driver.FindElement(By.CssSelector(".umbModalBoxIframe"));
            Driver.SwitchTo().Frame(umbracoModalBoxIframe);

            var rootNode = Driver.FindElement(By.XPath("//*[@id='1051']"));
            builder.MoveToElement(rootNode).DoubleClick().Build().Perform();
            Thread.Sleep(500);
            var gettinStartedNode = Driver.FindElement(By.XPath("//*[@id='1055']/a/div"));
            builder.MoveToElement(gettinStartedNode).Click().Build().Perform();
            Thread.Sleep(500);
            submitButton = Driver.FindElement(By.XPath("//input[@type='submit'][1]"));
            builder.MoveToElement(submitButton).Click().Build().Perform();
            Thread.Sleep(2000);
            var closeLink = Driver.FindElement(By.XPath("/html/body/form/div[2]/p[2]/a"));
            builder.MoveToElement(closeLink).Click().Build().Perform();
            
            Driver.SwitchTo().DefaultContent();
            Thread.Sleep(1000);

            // Perform copy
            var gettingStartedNode = Driver.FindElement(By.XPath("//*[@id='1054']/a/div"));
            builder.MoveToElement(gettingStartedNode).ContextClick().Build().Perform();
            Thread.Sleep(1000);

            var copyButton = Driver.FindElement(By.CssSelector(".sprCopy"));
            copyButton.Click();
            Thread.Sleep(2000);

            umbracoModalBoxIframe = Driver.FindElement(By.CssSelector(".umbModalBoxIframe"));
            Driver.SwitchTo().Frame(umbracoModalBoxIframe);

            rootNode = Driver.FindElement(By.XPath("//*[@id='1051']"));
            builder.MoveToElement(rootNode).DoubleClick().Build().Perform();
            Thread.Sleep(500);
            
            submitButton = Driver.FindElement(By.XPath("//input[@type='submit'][1]"));
            builder.MoveToElement(submitButton).Click().Build().Perform();
            Thread.Sleep(2000);
            closeLink = Driver.FindElement(By.XPath("/html/body/form/div[2]/p[2]/a"));
            builder.MoveToElement(closeLink).Click().Build().Perform();
        }
    }
}