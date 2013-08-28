using System.Linq;
using NUnit.Framework;
using Newtonsoft.Json.Linq;
using Umbraco.Web.PropertyEditors;

namespace Umbraco.Tests.PropertyEditors
{
    [TestFixture]
    public class ColorListValidatorTest
    {
        [Test]
        public void Only_Tests_On_JArray()
        {
            var validator = new ColorListPreValueEditor.ColorListValidator();
            var result = validator.Validate("hello", null, new ColorPickerPropertyEditor());
            Assert.AreEqual(0, result.Count());
        }

        [Test]
        public void Only_Tests_On_JArray_Of_Item_JObject()
        {
            var validator = new ColorListPreValueEditor.ColorListValidator();
            var result = validator.Validate(new JArray("hello", "world"), null, new ColorPickerPropertyEditor());
            Assert.AreEqual(0, result.Count());
        }
        
        [Test]
        public void Validates_Color_Vals()
        {
            var validator = new ColorListPreValueEditor.ColorListValidator();
            var result = validator.Validate(new JArray(
                                                JObject.FromObject(new { value = "CC0000" }),
                                                JObject.FromObject(new { value = "zxcvzxcvxzcv" }),
                                                JObject.FromObject(new { value = "ABC" }),
                                                JObject.FromObject(new { value = "1234567" })),
                                            null, new ColorPickerPropertyEditor());
            Assert.AreEqual(2, result.Count());
        }
    }
}