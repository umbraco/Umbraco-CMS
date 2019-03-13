using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Http.ModelBinding;
using Moq;
using NUnit.Framework;
using Umbraco.Core.Services;
using Umbraco.Web;

namespace Umbraco.Tests.Web
{
    [TestFixture]
    public class ModelStateExtensionsTests
    {

        [Test]
        public void Get_Cultures_With_Property_Errors()
        {
            var ms = new ModelStateDictionary();
            var localizationService = new Mock<ILocalizationService>();
            localizationService.Setup(x => x.GetDefaultLanguageIsoCode()).Returns("en-US");

            ms.AddPropertyError(new ValidationResult("no header image"), "headerImage", null); //invariant property
            ms.AddPropertyError(new ValidationResult("title missing"), "title", "en-US"); //variant property

            var result = ms.GetCulturesWithPropertyErrors(localizationService.Object);

            //even though there are 2 errors, they are both for en-US since that is the default language and one of the errors is for an invariant property
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("en-US", result[0]);
        }

        [Test]
        public void Add_Invariant_Property_Error()
        {
            var ms = new ModelStateDictionary();
            var localizationService = new Mock<ILocalizationService>();
            localizationService.Setup(x => x.GetDefaultLanguageIsoCode()).Returns("en-US");

            ms.AddPropertyError(new ValidationResult("no header image"), "headerImage", null); //invariant property

            Assert.AreEqual("_Properties.headerImage.invariant", ms.Keys.First());
        }

        [Test]
        public void Add_Variant_Property_Error()
        {
            var ms = new ModelStateDictionary();
            var localizationService = new Mock<ILocalizationService>();
            localizationService.Setup(x => x.GetDefaultLanguageIsoCode()).Returns("en-US");

            ms.AddPropertyError(new ValidationResult("no header image"), "headerImage", "en-US"); //invariant property

            Assert.AreEqual("_Properties.headerImage.en-US", ms.Keys.First());
        }
    }
}
