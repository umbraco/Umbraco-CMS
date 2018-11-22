using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Services;

namespace Umbraco.Tests.Services
{
    [TestFixture]
    public class PreValueConverterTests
    {
        [Test]
        public void Can_Convert_To_Dictionary_Pre_Value_Collection()
        {
            var list = new List<Tuple<PreValue, string, int>>
                {
                    new Tuple<PreValue, string, int>(new PreValue(10, "value1"), "key1", 0),
                    new Tuple<PreValue, string, int>(new PreValue(11, "value2"), "key2", 3),
                    new Tuple<PreValue, string, int>(new PreValue(12, "value3"), "key3", 2),
                    new Tuple<PreValue, string, int>(new PreValue(13, "value4"), "key4", 1)
                };

            var result = DataTypeDefinitionRepository.PreValueConverter.ConvertToPreValuesCollection(list);

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
            var list = new List<Tuple<PreValue, string, int>>
                {
                    new Tuple<PreValue, string, int>(new PreValue(10, "value1"), "", 0),
                    new Tuple<PreValue, string, int>(new PreValue(11, "value2"), "", 3),
                    new Tuple<PreValue, string, int>(new PreValue(12, "value3"), "", 2),
                    new Tuple<PreValue, string, int>(new PreValue(13, "value4"), "", 1)
                };

            var result = DataTypeDefinitionRepository.PreValueConverter.ConvertToPreValuesCollection(list);

            Assert.Throws<InvalidOperationException>(() =>
                {
                    var blah = result.PreValuesAsDictionary;
                });

            Assert.AreEqual(4, result.PreValuesAsArray.Count());
            Assert.AreEqual("value1", result.PreValuesAsArray.ElementAt(0).Value);
            Assert.AreEqual("value4", result.PreValuesAsArray.ElementAt(1).Value);
            Assert.AreEqual("value3", result.PreValuesAsArray.ElementAt(2).Value);
            Assert.AreEqual("value2", result.PreValuesAsArray.ElementAt(3).Value);

        }

        [Test]
        public void Can_Convert_To_Array_Pre_Value_Collection()
        {
            var list = new List<Tuple<PreValue, string, int>>
                {
                    new Tuple<PreValue, string, int>(new PreValue(10, "value1"), "key1", 0),
                    new Tuple<PreValue, string, int>(new PreValue(11, "value2"), "key1", 3),
                    new Tuple<PreValue, string, int>(new PreValue(12, "value3"), "key3", 2),
                    new Tuple<PreValue, string, int>(new PreValue(13, "value4"), "key4", 1)
                };

            var result = DataTypeDefinitionRepository.PreValueConverter.ConvertToPreValuesCollection(list);

            Assert.Throws<InvalidOperationException>(() =>
                {
                    var blah = result.PreValuesAsDictionary;
                });

            Assert.AreEqual(4, result.PreValuesAsArray.Count());
            Assert.AreEqual("value1", result.PreValuesAsArray.ElementAt(0).Value);
            Assert.AreEqual("value4", result.PreValuesAsArray.ElementAt(1).Value);
            Assert.AreEqual("value3", result.PreValuesAsArray.ElementAt(2).Value);
            Assert.AreEqual("value2", result.PreValuesAsArray.ElementAt(3).Value);

        }
    }
}