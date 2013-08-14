using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Umbraco.Core.Services;

namespace Umbraco.Tests.Services
{
    [TestFixture]
    public class PreValueConverterTests
    {
        [Test]
        public void Can_Convert_To_Dictionary_Pre_Value_Collection()
        {
            var list = new List<Tuple<int, string, int, string>>
                {
                    new Tuple<int, string, int, string>(10, "key1", 0, "value1"),
                    new Tuple<int, string, int, string>(11, "key2", 3, "value2"),
                    new Tuple<int, string, int, string>(12, "key3", 2, "value3"),
                    new Tuple<int, string, int, string>(13, "key4", 1, "value4")
                };

            var result = DataTypeService.PreValueConverter.ConvertToPreValuesCollection(list);

            Assert.Throws<InvalidOperationException>(() =>
                {
                    var blah = result.PreValuesAsArray;
                });

            Assert.AreEqual(4, result.PreValuesAsDictionary.Count);
            Assert.AreEqual("key1", result.PreValuesAsDictionary.ElementAt(0).Key);
            Assert.AreEqual("key4", result.PreValuesAsDictionary.ElementAt(1).Key);
            Assert.AreEqual("key3", result.PreValuesAsDictionary.ElementAt(2).Key);
            Assert.AreEqual("key2", result.PreValuesAsDictionary.ElementAt(3).Key);

        }

        [Test]
        public void Can_Convert_To_Array_Pre_Value_Collection_When_Empty_Key()
        {
            var list = new List<Tuple<int, string, int, string>>
                {
                    new Tuple<int, string, int, string>(10, "", 0, "value1"),
                    new Tuple<int, string, int, string>(11, "", 3, "value2"),
                    new Tuple<int, string, int, string>(12, "", 2, "value3"),
                    new Tuple<int, string, int, string>(13, "", 1, "value4")
                };

            var result = DataTypeService.PreValueConverter.ConvertToPreValuesCollection(list);

            Assert.Throws<InvalidOperationException>(() =>
                {
                    var blah = result.PreValuesAsDictionary;
                });

            Assert.AreEqual(4, result.PreValuesAsArray.Count());
            Assert.AreEqual("value1", result.PreValuesAsArray.ElementAt(0));
            Assert.AreEqual("value4", result.PreValuesAsArray.ElementAt(1));
            Assert.AreEqual("value3", result.PreValuesAsArray.ElementAt(2));
            Assert.AreEqual("value2", result.PreValuesAsArray.ElementAt(3));

        }

        [Test]
        public void Can_Convert_To_Array_Pre_Value_Collection()
        {
            var list = new List<Tuple<int, string, int, string>>
                {
                    new Tuple<int, string, int, string>(10, "key1", 0, "value1"),
                    new Tuple<int, string, int, string>(11, "key1", 3, "value2"),
                    new Tuple<int, string, int, string>(12, "key3", 2, "value3"),
                    new Tuple<int, string, int, string>(13, "key4", 1, "value4")
                };

            var result = DataTypeService.PreValueConverter.ConvertToPreValuesCollection(list);

            Assert.Throws<InvalidOperationException>(() =>
                {
                    var blah = result.PreValuesAsDictionary;
                });

            Assert.AreEqual(4, result.PreValuesAsArray.Count());
            Assert.AreEqual("value1", result.PreValuesAsArray.ElementAt(0));
            Assert.AreEqual("value4", result.PreValuesAsArray.ElementAt(1));
            Assert.AreEqual("value3", result.PreValuesAsArray.ElementAt(2));
            Assert.AreEqual("value2", result.PreValuesAsArray.ElementAt(3));

        }
    }
}