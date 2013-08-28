using System.Linq;
using NUnit.Framework;
using Newtonsoft.Json.Linq;
using Umbraco.Web.PropertyEditors;

namespace Umbraco.Tests.PropertyEditors
{
    [TestFixture]
    public class EnsureUniqueValuesValidatorTest
    {
        [Test]
        public void Only_Tests_On_JArray()
        {
            var validator = new ValueListPreValueEditor.EnsureUniqueValuesValidator();
            var result = validator.Validate("hello", null, new ColorPickerPropertyEditor());
            Assert.AreEqual(0, result.Count());
        }

        [Test]
        public void Only_Tests_On_JArray_Of_Item_JObject()
        {
            var validator = new ValueListPreValueEditor.EnsureUniqueValuesValidator();
            var result = validator.Validate(new JArray("hello", "world"), null, new ColorPickerPropertyEditor());
            Assert.AreEqual(0, result.Count());
        }

        [Test]
        public void Allows_Unique_Values()
        {
            var validator = new ValueListPreValueEditor.EnsureUniqueValuesValidator();
            var result = validator.Validate(new JArray(JObject.FromObject(new { value = "hello" }), JObject.FromObject(new { value = "world" })), null, new ColorPickerPropertyEditor());
            Assert.AreEqual(0, result.Count());
        }

        [Test]
        public void Does_Not_Allow_Multiple_Values()
        {            
            var validator = new ValueListPreValueEditor.EnsureUniqueValuesValidator();
            var result = validator.Validate(new JArray(JObject.FromObject(new { value = "hello" }), JObject.FromObject(new { value = "hello" })),
                                            null, new ColorPickerPropertyEditor());
            Assert.AreEqual(1, result.Count());
        }

        [Test]
        public void Validates_Multiple_Duplicate_Values()
        {
            var validator = new ValueListPreValueEditor.EnsureUniqueValuesValidator();
            var result = validator.Validate(new JArray(
                                                JObject.FromObject(new { value = "hello" }), 
                                                JObject.FromObject(new { value = "hello" }),
                                                JObject.FromObject(new { value = "world" }),
                                                JObject.FromObject(new { value = "world" })), 
                                            null, new ColorPickerPropertyEditor());
            Assert.AreEqual(2, result.Count());
        }
    }
}