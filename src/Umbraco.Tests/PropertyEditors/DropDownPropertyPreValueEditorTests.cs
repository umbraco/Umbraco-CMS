using System.Collections.Generic;
using NUnit.Framework;
using Umbraco.Core.Models;
using Umbraco.Web.PropertyEditors;

namespace Umbraco.Tests.PropertyEditors
{
    [TestFixture]
    public class DropDownPropertyPreValueEditorTests
    {
        [Test]
        public void Format_Data_For_Editor()
        {

            var defaultVals = new Dictionary<string, object>();
            var persisted = new PreValueCollection(new Dictionary<string, PreValue>
                {
                    {"item1", new PreValue(1, "Item 1")},
                    {"item2", new PreValue(2, "Item 2")},
                    {"item3", new PreValue(3, "Item 3")}
                });

            var editor = new DropDownPreValueEditor();

            var result = editor.FormatDataForEditor(defaultVals, persisted);

            Assert.AreEqual(1, result.Count);
            Assert.IsTrue(result.ContainsKey("items"));
            var items = result["items"] as IDictionary<int, string>;
            Assert.IsNotNull(items);
            Assert.AreEqual("Item 1", items[1]);
            Assert.AreEqual("Item 2", items[2]);
            Assert.AreEqual("Item 3", items[3]);
        }
    }
}