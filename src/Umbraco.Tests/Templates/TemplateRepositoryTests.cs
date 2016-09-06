using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Tests.Templates
{
    [TestFixture]
    public class TemplateRepositoryTests
    {
        private readonly Mock<IDatabaseUnitOfWork> _unitOfWorkMock = new Mock<IDatabaseUnitOfWork>();
        private readonly Mock<CacheHelper> _cacheMock = new Mock<CacheHelper>();
        private TemplateRepository _templateRepository;
        private readonly Mock<IFileSystem> _viewFileSystemMock = new Mock<IFileSystem>();
        private readonly Mock<IFileSystem> _masterpageFileSystemMock = new Mock<IFileSystem>();
        private readonly Mock<ITemplatesSection> _templateConfigMock = new Mock<Core.Configuration.UmbracoSettings.ITemplatesSection>();

        [SetUp]
        public void Setup()
        {
            var loggerMock = new Mock<ILogger>();
            var sqlSyntaxMock = new Mock<ISqlSyntaxProvider>();
            _templateRepository = new TemplateRepository(_unitOfWorkMock.Object, _cacheMock.Object, loggerMock.Object, sqlSyntaxMock.Object, _masterpageFileSystemMock.Object, _viewFileSystemMock.Object, _templateConfigMock.Object);

        }

        [Test]
        public void DetermineTemplateRenderingEngine_Returns_MVC_When_ViewFile_Exists_And_Content_Has_Webform_Markup()
        {
            // Project in MVC mode
            _templateConfigMock.Setup(x => x.DefaultRenderingEngine).Returns(RenderingEngine.Mvc);

            // Template has masterpage content
            var templateMock = new Mock<ITemplate>();
            templateMock.Setup(x => x.Alias).Returns("Something");
            templateMock.Setup(x => x.Content).Returns("<asp:Content />");

            // but MVC View already exists
            _viewFileSystemMock.Setup(x => x.FileExists(It.IsAny<string>())).Returns(true);

            var res = _templateRepository.DetermineTemplateRenderingEngine(templateMock.Object);
            Assert.AreEqual(RenderingEngine.Mvc, res);
        }

        [Test]
        public void DetermineTemplateRenderingEngine_Returns_WebForms_When_ViewFile_Doesnt_Exist_And_Content_Has_Webform_Markup()
        {
            // Project in MVC mode
            _templateConfigMock.Setup(x => x.DefaultRenderingEngine).Returns(RenderingEngine.Mvc);

            // Template has masterpage content
            var templateMock = new Mock<ITemplate>();
            templateMock.Setup(x => x.Alias).Returns("Something");
            templateMock.Setup(x => x.Content).Returns("<asp:Content />");

            // MVC View doesn't exist
            _viewFileSystemMock.Setup(x => x.FileExists(It.IsAny<string>())).Returns(false);

            var res = _templateRepository.DetermineTemplateRenderingEngine(templateMock.Object);
            Assert.AreEqual(RenderingEngine.WebForms, res);
        }

        [Test]
        public void DetermineTemplateRenderingEngine_Returns_WebForms_When_MasterPage_Exists_And_In_Mvc_Mode()
        {
            // Project in MVC mode
            _templateConfigMock.Setup(x => x.DefaultRenderingEngine).Returns(RenderingEngine.Mvc);

            var templateMock = new Mock<ITemplate>();
            templateMock.Setup(x => x.Alias).Returns("Something");

            // but masterpage already exists
            _viewFileSystemMock.Setup(x => x.FileExists(It.IsAny<string>())).Returns(false);
            _masterpageFileSystemMock.Setup(x => x.FileExists(It.IsAny<string>())).Returns(true);

            var res = _templateRepository.DetermineTemplateRenderingEngine(templateMock.Object);
            Assert.AreEqual(RenderingEngine.WebForms, res);
        }

        [Test]
        public void DetermineTemplateRenderingEngine_Returns_Mvc_When_ViewPage_Exists_And_In_Webforms_Mode()
        {
            // Project in WebForms mode
            _templateConfigMock.Setup(x => x.DefaultRenderingEngine).Returns(RenderingEngine.WebForms);

            var templateMock = new Mock<ITemplate>();
            templateMock.Setup(x => x.Alias).Returns("Something");

            // but MVC View already exists
            _viewFileSystemMock.Setup(x => x.FileExists(It.IsAny<string>())).Returns(true);
            _masterpageFileSystemMock.Setup(x => x.FileExists(It.IsAny<string>())).Returns(false);

            var res = _templateRepository.DetermineTemplateRenderingEngine(templateMock.Object);
            Assert.AreEqual(RenderingEngine.Mvc, res);
        }

    }
}
