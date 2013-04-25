using System.IO;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using Umbraco.Core;
using Umbraco.Tests.TestHelpers;

namespace Umbraco.Tests.Integration
{
    [Ignore("We don't want to run Selenium tests on TeamCity")]
    [TestFixture]
    public class InstallPackage : BaseSeleniumTest
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
        public void Install_Courier_Package()
        {
            Driver.Navigate().GoToUrl(BaseUrl + "/umbraco/login.aspx?redir=");
            Driver.FindElement(By.Id("lname")).Clear();
            Driver.FindElement(By.Id("lname")).SendKeys("admin");
            Driver.FindElement(By.Id("passw")).Clear();
            Driver.FindElement(By.Id("passw")).SendKeys("test");
            Driver.FindElement(By.Id("Button1")).Click();
            Thread.Sleep(1000);

            Driver.Navigate().GoToUrl(BaseUrl + "/umbraco/umbraco.aspx#developer");
            Thread.Sleep(2000);

            var builder = new Actions(Driver);
            var packagesNode = Driver.FindElement(By.XPath("//*[@id='init'][3]"));
            builder.MoveToElement(packagesNode).DoubleClick().Build().Perform();
            Thread.Sleep(1000);

            var installPackageNode = Driver.FindElement(By.XPath("/html/body/div[2]/div/div/div[2]/div/div/div/div/ul/li/ul/li[3]/ul/li[4]/a/div"));
            builder.MoveToElement(installPackageNode).Click().Build().Perform();
            Thread.Sleep(1000);

            var rightFrame = Driver.FindElement(By.XPath("//*[@id='right']"));
            Driver.SwitchTo().Frame(rightFrame);

            const string packagesDir = @"C:\\Downloads\\Packages\\";
            var file = new DirectoryInfo(packagesDir).GetFiles().First(f => f.Name.ToLowerInvariant().StartsWith("Courier".ToLowerInvariant()));
            
            Driver.FindElement(By.Id("cb")).Click();
            Driver.FindElement(By.XPath("//input[@type='file']")).SendKeys(file.FullName);
            Driver.FindElement(By.Id("body_ButtonLoadPackage")).Click();
            Driver.FindElement(By.Id("body_acceptCheckbox")).Click();
            Driver.FindElement(By.Id("body_ButtonInstall")).Click();
            Thread.Sleep(1000);

            var successPanel = Driver.FindElement(By.XPath("//*[@id='body_Panel1_content']"));
            Assert.IsNotNull(successPanel);
        }

        [Test]
        public void Install_Contour_Package()
        {
            Driver.Navigate().GoToUrl(BaseUrl + "/umbraco/login.aspx?redir=");
            Driver.FindElement(By.Id("lname")).Clear();
            Driver.FindElement(By.Id("lname")).SendKeys("admin");
            Driver.FindElement(By.Id("passw")).Clear();
            Driver.FindElement(By.Id("passw")).SendKeys("test");
            Driver.FindElement(By.Id("Button1")).Click();
            Thread.Sleep(1000);

            Driver.Navigate().GoToUrl(BaseUrl + "/umbraco/umbraco.aspx#developer");
            Thread.Sleep(2000);

            var builder = new Actions(Driver);
            var packagesNode = Driver.FindElement(By.XPath("//*[@id='init'][3]"));
            builder.MoveToElement(packagesNode).DoubleClick().Build().Perform();
            Thread.Sleep(1000);

            var installPackageNode = Driver.FindElement(By.XPath("/html/body/div[2]/div/div/div[2]/div/div/div/div/ul/li/ul/li[3]/ul/li[4]/a/div"));
            builder.MoveToElement(installPackageNode).Click().Build().Perform();
            Thread.Sleep(1000);

            var rightFrame = Driver.FindElement(By.XPath("//*[@id='right']"));
            Driver.SwitchTo().Frame(rightFrame);

            const string packagesDir = @"C:\\Downloads\\Packages\\";
            var file = new DirectoryInfo(packagesDir).GetFiles().First(f => f.Name.ToLowerInvariant().StartsWith("UmbracoContour".ToLowerInvariant()));
            
            Driver.FindElement(By.Id("cb")).Click();
            Driver.FindElement(By.Id("body_file1")).SendKeys(file.Name);
            Driver.FindElement(By.Id("body_ButtonLoadPackage")).Click();
            Driver.FindElement(By.Id("body_acceptCheckbox")).Click();
            Driver.FindElement(By.Id("body_ButtonInstall")).Click();
            Thread.Sleep(1000);

            var successPanel = Driver.FindElement(By.XPath("//*[@id='body_Panel1_content']"));
            Assert.IsNotNull(successPanel);
        }

        //[Test]
        //public void Install_CMS_Import_Package()
        //{
        //    Driver.Navigate().GoToUrl(BaseUrl + "/umbraco/login.aspx?redir=");
        //    Driver.FindElement(By.Id("lname")).Clear();
        //    Driver.FindElement(By.Id("lname")).SendKeys("admin");
        //    Driver.FindElement(By.Id("passw")).Clear();
        //    Driver.FindElement(By.Id("passw")).SendKeys("test");
        //    Driver.FindElement(By.Id("Button1")).Click();
        //    Thread.Sleep(1000);

        //    Driver.Navigate().GoToUrl(BaseUrl + "/umbraco/umbraco.aspx#developer");
        //    Thread.Sleep(2000);

        //    var builder = new Actions(Driver);
        //    var packagesNode = Driver.FindElement(By.XPath("//*[@id='init'][3]"));
        //    builder.MoveToElement(packagesNode).DoubleClick().Build().Perform();
        //    Thread.Sleep(1000);

        //    var installPackageNode = Driver.FindElement(By.XPath("/html/body/div[2]/div/div/div[2]/div/div/div/div/ul/li/ul/li[3]/ul/li[4]/a/div"));
        //    builder.MoveToElement(installPackageNode).Click().Build().Perform();
        //    Thread.Sleep(1000);

        //    var rightFrame = Driver.FindElement(By.XPath("//*[@id='right']"));
        //    Driver.SwitchTo().Frame(rightFrame);

        //    Driver.FindElement(By.Id("cb")).Click();
        //    Driver.FindElement(By.Id("body_file1")).SendKeys("C:\\Downloads\\Packages\\CMSImport-2.3.1.zip");
        //    Driver.FindElement(By.Id("body_ButtonLoadPackage")).Click();
        //    Driver.FindElement(By.Id("body_acceptCheckbox")).Click();
        //    Driver.FindElement(By.Id("body_ButtonInstall")).Click();
        //    Thread.Sleep(1000);

        //    var successPanel = Driver.FindElement(By.XPath("//*[@id='body_Panel1_content']"));
        //    Assert.IsNotNull(successPanel);
        //}
    }
}