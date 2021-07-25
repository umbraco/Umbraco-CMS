// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Collections;

namespace Umbraco.Tests.Collections
{
    public static class XUnitAssertion
    {

        public static void Collection<TKey, TValue>(KeyValuePair<TKey, TValue>[]? collection, params Action<KeyValuePair<TKey, TValue>>[] orderedAssertions)
        {
            for (int i = 0; i < orderedAssertions.Length; i++)
            {
                orderedAssertions[i](collection[i]);
            }

        }
        public static void Collection<T>(IList<T> collection, params Action<T>[] orderedAssertions)
        {
            for (int i = 0; i < orderedAssertions.Length; i++)
            {
                orderedAssertions[i](collection[i]);
            }

        }
        public static void Collection<T>(IOrderedEnumerable<T> collection, params Action<T>[] orderedAssertions)
        {
            var list = collection.ToList();
            for (int i = 0; i < orderedAssertions.Length; i++)
            {
                orderedAssertions[i](list[i]);
            }
        }
        public static void Collection<TKey, TValue>(IDictionary<TKey, TValue> collection, params Action<KeyValuePair<TKey, TValue>>[] orderedAssertions)
        {
            var list = collection.ToList();
            for (int i = 0; i < orderedAssertions.Length; i++)
            {
                orderedAssertions[i](list[i]);
            }
        }
    }

    [TestFixture]
    public class AdaptiveCapacityDictionaryTests
    {
        [Test]
        public void DefaultCtor()
        {
            // Arrange
            // Act
            var dict = new AdaptiveCapacityDictionary<string, string>();

            // Assert
            Assert.IsEmpty(dict);
            Assert.IsEmpty(dict._arrayStorage);
            Assert.Null(dict._dictionaryStorage);
        }

        [Test]
        public void CreateFromNull()
        {
            // Arrange
            // Act
            var dict = new AdaptiveCapacityDictionary<string, string>();

            // Assert
            Assert.IsEmpty(dict);
            Assert.IsEmpty(dict._arrayStorage);
            Assert.Null(dict._dictionaryStorage);
        }

        public static KeyValuePair<string, object>[] IEnumerableKeyValuePairData
        {
            get
            {
                return new[]
                {
                    new KeyValuePair<string, object?>("Name", "James"),
                    new KeyValuePair<string, object?>("Age", 30),
                    new KeyValuePair<string, object?>("Address", new Address() { City = "Redmond", State = "WA" })
                };
            }
        }

        public static KeyValuePair<string, string>[] IEnumerableStringValuePairData
        {
            get
            {
                return new[]
                {
                    new KeyValuePair<string, string>("First Name", "James"),
                    new KeyValuePair<string, string>("Last Name", "Henrik"),
                    new KeyValuePair<string, string>("Middle Name", "Bob")
                };
            }
        }

        [Test]
        public void CreateFromIEnumerableKeyValuePair_CopiesValues()
        {
            // Arrange & Act
            var dict = new AdaptiveCapacityDictionary<string, object?>(IEnumerableKeyValuePairData, capacity: IEnumerableKeyValuePairData.Length, EqualityComparer<string>.Default);

            // Assert
            Assert.IsInstanceOf<KeyValuePair<string, object?>[]>(dict._arrayStorage);
            XUnitAssertion.Collection(
                dict.OrderBy(kvp => kvp.Key),
                kvp =>
                {
                    Assert.AreEqual("Address", kvp.Key);
                    Assert.IsInstanceOf<Address>(kvp.Value);
                    var address = kvp.Value as Address;
                    Assert.AreEqual("Redmond", address.City);
                    Assert.AreEqual("WA", address.State);
                },
                kvp => { Assert.AreEqual("Age", kvp.Key); Assert.AreEqual(30, kvp.Value); },
                kvp => { Assert.AreEqual("Name", kvp.Key); Assert.AreEqual("James", kvp.Value); });
        }

        [Test]
        public void CreateFromIEnumerableStringValuePair_CopiesValues()
        {
            // Arrange & Act
            var dict = new AdaptiveCapacityDictionary<string, string>(IEnumerableStringValuePairData, capacity: 3, StringComparer.OrdinalIgnoreCase);

            // Assert
            Assert.IsInstanceOf<KeyValuePair<string, string>[]>(dict._arrayStorage);
            XUnitAssertion.Collection(
                dict.OrderBy(kvp => kvp.Key),
                kvp => { Assert.AreEqual("First Name", kvp.Key); Assert.AreEqual("James", kvp.Value); },
                kvp => { Assert.AreEqual("Last Name", kvp.Key); Assert.AreEqual("Henrik", kvp.Value); },
                kvp => { Assert.AreEqual("Middle Name", kvp.Key); Assert.AreEqual("Bob", kvp.Value); });
        }

        [Test]
        public void CreateFromIEnumerableKeyValuePair_ThrowsExceptionForDuplicateKey()
        {
            // Arrange, Act & Assert
            Assert.Throws<ArgumentException>(
                () => new AdaptiveCapacityDictionary<string, object?>(StringComparer.OrdinalIgnoreCase)
                {
                    {  "name", "Billy" },
                    {  "Name", "Joey" }
                },
                "key",
                $"An element with the key 'Name' already exists in the {nameof(AdaptiveCapacityDictionary<string, object?>)}.");
        }
        [Test]
        public void CreateFromIEnumerableStringValuePair_ThrowsExceptionForDuplicateKey()
        {
            // Arrange
            var values = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("name", "Billy"),
                new KeyValuePair<string, string>("Name", "Joey"),
            };
            // Act & Assert
            Assert.Throws<ArgumentException>(
                () => new AdaptiveCapacityDictionary<string, string>(values, capacity: 3, StringComparer.OrdinalIgnoreCase),
                "key",
                $"An element with the key 'Name' already exists in the {nameof(AdaptiveCapacityDictionary<string, object>)}.");
        }
        [Test]
        public void Comparer_IsOrdinalIgnoreCase()
        {
            // Arrange
            // Act
            var dict = new AdaptiveCapacityDictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            // Assert
            Assert.AreSame(StringComparer.OrdinalIgnoreCase, dict.Comparer);
        }
        // Our comparer is hardcoded to be IsReadOnly==false no matter what.
        [Test]
        public void IsReadOnly_False()
        {
            // Arrange
            var dict = new AdaptiveCapacityDictionary<string, object>();
            // Act
            var result = ((ICollection<KeyValuePair<string, object?>>)dict).IsReadOnly;
            // Assert
            Assert.False(result);
        }
        [Test]
        public void IndexGet_EmptyStringIsAllowed()
        {
            // Arrange
            var dict = new AdaptiveCapacityDictionary<string, string>();
            // Act
            var value = dict[""];
            // Assert
            Assert.Null(value);
        }
        [Test]
        public void IndexGet_EmptyStorage_ReturnsNull()
        {
            // Arrange
            var dict = new AdaptiveCapacityDictionary<string, string>();
            // Act
            var value = dict["key"];
            // Assert
            Assert.Null(value);
        }
        [Test]
        public void IndexGet_ArrayStorage_NoMatch_ReturnsNull()
        {
            // Arrange
            var dict = new AdaptiveCapacityDictionary<string, object>();
            dict.Add("age", 30);
            // Act
            var value = dict["key"];
            // Assert
            Assert.Null(value);
            Assert.IsInstanceOf<KeyValuePair<string, object?>[]>(dict._arrayStorage);
        }
        [Test]
        public void IndexGet_ListStorage_Match_ReturnsValue()
        {
            // Arrange
            var dict = new AdaptiveCapacityDictionary<string, object>()
            {
                { "key", "value" },
            };
            // Act
            var value = dict["key"];
            // Assert
            Assert.AreEqual("value", value);
            Assert.IsInstanceOf<KeyValuePair<string, object?>[]>(dict._arrayStorage);
        }
        [Test]
        public void IndexGet_ListStorage_MatchIgnoreCase_ReturnsValue()
        {
            // Arrange
            var dict = new AdaptiveCapacityDictionary<string, object>(StringComparer.OrdinalIgnoreCase)
            {
                { "key", "value" },
            };
            // Act
            var value = dict["kEy"];
            // Assert
            Assert.AreEqual("value", value);
            Assert.IsInstanceOf<KeyValuePair<string, object?>[]>(dict._arrayStorage);
        }
        [Test]
        public void IndexSet_EmptyStringIsAllowed()
        {
            // Arrange
            var dict = new AdaptiveCapacityDictionary<string, string>();
            // Act
            dict[""] = "foo";
            // Assert
            Assert.AreEqual("foo", dict[""]);
        }
        [Test]
        public void IndexSet_EmptyStorage_UpgradesToList()
        {
            // Arrange
            var dict = new AdaptiveCapacityDictionary<string, object>();
            // Act
            dict["key"] = "value";
            // Assert
            XUnitAssertion.Collection(dict, kvp => { Assert.AreEqual("key", kvp.Key); Assert.AreEqual("value", kvp.Value); });
            Assert.IsInstanceOf<KeyValuePair<string, object?>[]>(dict._arrayStorage);
        }
        [Test]
        public void IndexSet_ListStorage_NoMatch_AddsValue()
        {
            // Arrange
            var dict = new AdaptiveCapacityDictionary<string, object>()
            {
                { "age", 30 },
            };
            // Act
            dict["key"] = "value";
            // Assert
            XUnitAssertion.Collection(
                dict.OrderBy(kvp => kvp.Key),
                kvp => { Assert.AreEqual("age", kvp.Key); Assert.AreEqual(30, kvp.Value); },
                kvp => { Assert.AreEqual("key", kvp.Key); Assert.AreEqual("value", kvp.Value); });
            Assert.IsInstanceOf<KeyValuePair<string, object?>[]>(dict._arrayStorage);
        }
        [Test]
        public void IndexSet_ListStorage_Match_SetsValue()
        {
            // Arrange
            var dict = new AdaptiveCapacityDictionary<string, object>()
            {
                { "key", "value" },
            };
            // Act
            dict["key"] = "value";
            // Assert
            XUnitAssertion.Collection(dict, kvp => { Assert.AreEqual("key", kvp.Key); Assert.AreEqual("value", kvp.Value); });
            Assert.IsInstanceOf<KeyValuePair<string, object?>[]>(dict._arrayStorage);
        }
        [Test]
        public void IndexSet_ListStorage_MatchIgnoreCase_SetsValue()
        {
            // Arrange
            var dict = new AdaptiveCapacityDictionary<string, object>()
            {
                { "key", "value" },
            };
            // Act
            dict["key"] = "value";
            // Assert
            XUnitAssertion.Collection(dict, kvp => { Assert.AreEqual("key", kvp.Key); Assert.AreEqual("value", kvp.Value); });
            Assert.IsInstanceOf<KeyValuePair<string, object?>[]>(dict._arrayStorage);
        }
        [Test]
        public void Count_EmptyStorage()
        {
            // Arrange
            var dict = new AdaptiveCapacityDictionary<string, string>();
            // Act
            var count = dict.Count;
            // Assert
            Assert.AreEqual(0, count);
        }
        [Test]
        public void Count_ListStorage()
        {
            // Arrange
            var dict = new AdaptiveCapacityDictionary<string, object>()
            {
                { "key", "value" },
            };
            // Act
            var count = dict.Count;
            // Assert
            Assert.AreEqual(1, count);
            Assert.IsInstanceOf<KeyValuePair<string, object?>[]>(dict._arrayStorage);
        }
        [Test]
        public void Keys_EmptyStorage()
        {
            // Arrange
            var dict = new AdaptiveCapacityDictionary<string, object>();
            // Act
            var keys = dict.Keys;
            // Assert
            Assert.IsEmpty(keys);
            Assert.IsInstanceOf<KeyValuePair<string, object?>[]>(dict._arrayStorage);
        }
        [Test]
        public void Keys_ListStorage()
        {
            // Arrange
            var dict = new AdaptiveCapacityDictionary<string, object>()
            {
                { "key", "value" },
            };
            // Act
            var keys = dict.Keys;
            // Assert
            Assert.AreEqual(new[] { "key" }, keys);
            Assert.IsInstanceOf<KeyValuePair<string, object?>[]>(dict._arrayStorage);
        }
        [Test]
        public void Values_EmptyStorage()
        {
            // Arrange
            var dict = new AdaptiveCapacityDictionary<string, object>();
            // Act
            var values = dict.Values;
            // Assert
            Assert.IsEmpty(values);
            Assert.IsInstanceOf<KeyValuePair<string, object?>[]>(dict._arrayStorage);
        }
        [Test]
        public void Values_ListStorage()
        {
            // Arrange
            var dict = new AdaptiveCapacityDictionary<string, object>()
            {
                { "key", "value" },
            };
            // Act
            var values = dict.Values;
            // Assert
            Assert.AreEqual(new object[] { "value" }, values);
            Assert.IsInstanceOf<KeyValuePair<string, object?>[]>(dict._arrayStorage);
        }
        [Test]
        public void Add_EmptyStorage()
        {
            // Arrange
            var dict = new AdaptiveCapacityDictionary<string, object>();
            // Act
            dict.Add("key", "value");
            // Assert
            XUnitAssertion.Collection(dict, kvp => { Assert.AreEqual("key", kvp.Key); Assert.AreEqual("value", kvp.Value); });
            Assert.IsInstanceOf<KeyValuePair<string, object?>[]>(dict._arrayStorage);
        }
        [Test]
        public void Add_EmptyStringIsAllowed()
        {
            // Arrange
            var dict = new AdaptiveCapacityDictionary<string, string>();
            // Act
            dict.Add("", "foo");
            // Assert
            Assert.AreEqual("foo", dict[""]);
        }
        [Test]
        public void Add_ListStorage()
        {
            // Arrange
            var dict = new AdaptiveCapacityDictionary<string, object>()
            {
                { "age", 30 },
            };
            // Act
            dict.Add("key", "value");
            // Assert
            XUnitAssertion.Collection(
                dict.OrderBy(kvp => kvp.Key),
                kvp => { Assert.AreEqual("age", kvp.Key); Assert.AreEqual(30, kvp.Value); },
                kvp => { Assert.AreEqual("key", kvp.Key); Assert.AreEqual("value", kvp.Value); });
            Assert.IsInstanceOf<KeyValuePair<string, object?>[]>(dict._arrayStorage);
        }
        [Test]
        public void Add_DuplicateKey()
        {
            // Arrange
            var dict = new AdaptiveCapacityDictionary<string, object>()
            {
                { "key", "value" },
            };
            var message = $"An element with the key 'key' already exists in the {nameof(AdaptiveCapacityDictionary<string, string>)}";
            // Act & Assert
            Assert.Throws<ArgumentException>(() => dict.Add("key", "value2"), "key", message);
            // Assert
            XUnitAssertion.Collection(
                dict.OrderBy(kvp => kvp.Key),
                kvp => { Assert.AreEqual("key", kvp.Key); Assert.AreEqual("value", kvp.Value); });
            Assert.IsInstanceOf<KeyValuePair<string, object?>[]>(dict._arrayStorage);
        }
        [Test]
        public void Add_DuplicateKey_CaseInsensitive()
        {
            // Arrange
            var dict = new AdaptiveCapacityDictionary<string, object>(StringComparer.OrdinalIgnoreCase)
            {
                { "key", "value" },
            };
            var message = $"An element with the key 'kEy' already exists in the {nameof(AdaptiveCapacityDictionary<string, string>)}";
            // Act & Assert
            Assert.Throws<ArgumentException>(() => dict.Add("kEy", "value2"), "key", message);
            // Assert
            XUnitAssertion.Collection(
                dict.OrderBy(kvp => kvp.Key),
                kvp => { Assert.AreEqual("key", kvp.Key); Assert.AreEqual("value", kvp.Value); });
            Assert.IsInstanceOf<KeyValuePair<string, object?>[]>(dict._arrayStorage);
        }
        [Test]
        public void Add_KeyValuePair()
        {
            // Arrange
            var dict = new AdaptiveCapacityDictionary<string, object>()
            {
                { "age", 30 },
            };
            // Act
            ((ICollection<KeyValuePair<string, object?>>)dict).Add(new KeyValuePair<string, object?>("key", "value"));
            // Assert
            XUnitAssertion.Collection(
                dict.OrderBy(kvp => kvp.Key),
                kvp => { Assert.AreEqual("age", kvp.Key); Assert.AreEqual(30, kvp.Value); },
                kvp => { Assert.AreEqual("key", kvp.Key); Assert.AreEqual("value", kvp.Value); });
            Assert.IsInstanceOf<KeyValuePair<string, object?>[]>(dict._arrayStorage);
        }
        [Test]
        public void Clear_EmptyStorage()
        {
            // Arrange
            var dict = new AdaptiveCapacityDictionary<string, string>();
            // Act
            dict.Clear();
            // Assert
            Assert.IsEmpty(dict);
        }
        [Test]
        public void Clear_ListStorage()
        {
            // Arrange
            var dict = new AdaptiveCapacityDictionary<string, object>()
            {
                { "key", "value" },
            };
            // Act
            dict.Clear();
            // Assert
            Assert.IsEmpty(dict);
            Assert.IsInstanceOf<KeyValuePair<string, object?>[]>(dict._arrayStorage);
            Assert.Null(dict._dictionaryStorage);
        }
        [Test]
        public void Contains_ListStorage_KeyValuePair_True()
        {
            // Arrange
            var dict = new AdaptiveCapacityDictionary<string, object>()
            {
                { "key", "value" },
            };
            var input = new KeyValuePair<string, object?>("key", "value");
            // Act
            var result = ((ICollection<KeyValuePair<string, object?>>)dict).Contains(input);
            // Assert
            Assert.True(result);
            Assert.IsInstanceOf<KeyValuePair<string, object?>[]>(dict._arrayStorage);
        }
        [Test]
        public void Contains_ListStory_KeyValuePair_True_CaseInsensitive()
        {
            // Arrange
            var dict = new AdaptiveCapacityDictionary<string, object>(StringComparer.OrdinalIgnoreCase)
            {
                { "key", "value" },
            };
            var input = new KeyValuePair<string, object?>("KEY", "value");
            // Act
            var result = ((ICollection<KeyValuePair<string, object?>>)dict).Contains(input);
            // Assert
            Assert.True(result);
            Assert.IsInstanceOf<KeyValuePair<string, object?>[]>(dict._arrayStorage);
        }
        [Test]
        public void Contains_ListStorage_KeyValuePair_False()
        {
            // Arrange
            var dict = new AdaptiveCapacityDictionary<string, object>()
            {
                { "key", "value" },
            };
            var input = new KeyValuePair<string, object?>("other", "value");
            // Act
            var result = ((ICollection<KeyValuePair<string, object?>>)dict).Contains(input);
            // Assert
            Assert.False(result);
            Assert.IsInstanceOf<KeyValuePair<string, object?>[]>(dict._arrayStorage);
        }
        // Value comparisons use the default equality comparer.
        [Test]
        public void Contains_ListStorage_KeyValuePair_False_ValueComparisonIsDefault()
        {
            // Arrange
            var dict = new AdaptiveCapacityDictionary<string, object>()
            {
                { "key", "value" },
            };
            var input = new KeyValuePair<string, object?>("key", "valUE");
            // Act
            var result = ((ICollection<KeyValuePair<string, object?>>)dict).Contains(input);
            // Assert
            Assert.False(result);
            Assert.IsInstanceOf<KeyValuePair<string, object?>[]>(dict._arrayStorage);
        }
        [Test]
        public void ContainsKey_EmptyStorage()
        {
            // Arrange
            var dict = new AdaptiveCapacityDictionary<string, string>();
            // Act
            var result = dict.ContainsKey("key");
            // Assert
            Assert.False(result);
        }
        [Test]
        public void ContainsKey_EmptyStringIsAllowed()
        {
            // Arrange
            var dict = new AdaptiveCapacityDictionary<string, string>();
            // Act
            var result = dict.ContainsKey("");
            // Assert
            Assert.False(result);
        }
        [Test]
        public void ContainsKey_ListStorage_False()
        {
            // Arrange
            var dict = new AdaptiveCapacityDictionary<string, object>()
            {
                { "key", "value" },
            };
            // Act
            var result = dict.ContainsKey("other");
            // Assert
            Assert.False(result);
            Assert.IsInstanceOf<KeyValuePair<string, object?>[]>(dict._arrayStorage);
        }
        [Test]
        public void ContainsKey_ListStorage_True()
        {
            // Arrange
            var dict = new AdaptiveCapacityDictionary<string, object>()
            {
                { "key", "value" },
            };
            // Act
            var result = dict.ContainsKey("key");
            // Assert
            Assert.True(result);
            Assert.IsInstanceOf<KeyValuePair<string, object?>[]>(dict._arrayStorage);
        }
        [Test]
        public void ContainsKey_ListStorage_True_CaseInsensitive()
        {
            // Arrange
            var dict = new AdaptiveCapacityDictionary<string, object>(StringComparer.OrdinalIgnoreCase)
            {
                { "key", "value" },
            };
            // Act
            var result = dict.ContainsKey("kEy");
            // Assert
            Assert.True(result);
            Assert.IsInstanceOf<KeyValuePair<string, object?>[]>(dict._arrayStorage);
        }
        [Test]
        public void CopyTo()
        {
            // Arrange
            var dict = new AdaptiveCapacityDictionary<string, object>()
            {
                { "key", "value" },
            };
            var array = new KeyValuePair<string, object?>[2];
            // Act
            ((ICollection<KeyValuePair<string, object?>>)dict).CopyTo(array, 1);
            // Assert
            Assert.AreEqual(
                new KeyValuePair<string, object?>[]
                {
                    default(KeyValuePair<string, object?>),
                    new KeyValuePair<string, object?>("key", "value")
                },
                array);
            Assert.IsInstanceOf<KeyValuePair<string, object?>[]>(dict._arrayStorage);
        }
        [Test]
        public void Remove_KeyValuePair_True()
        {
            // Arrange
            var dict = new AdaptiveCapacityDictionary<string, object>()
            {
                { "key", "value" },
            };
            var input = new KeyValuePair<string, object?>("key", "value");
            // Act
            var result = ((ICollection<KeyValuePair<string, object?>>)dict).Remove(input);
            // Assert
            Assert.True(result);
            Assert.IsEmpty(dict);
            Assert.IsInstanceOf<KeyValuePair<string, object?>[]>(dict._arrayStorage);
        }
        [Test]
        public void Remove_KeyValuePair_True_CaseInsensitive()
        {
            // Arrange
            var dict = new AdaptiveCapacityDictionary<string, object>(StringComparer.OrdinalIgnoreCase)
            {
                { "key", "value" },
            };
            var input = new KeyValuePair<string, object?>("KEY", "value");
            // Act
            var result = ((ICollection<KeyValuePair<string, object?>>)dict).Remove(input);
            // Assert
            Assert.True(result);
            Assert.IsEmpty(dict);
            Assert.IsInstanceOf<KeyValuePair<string, object?>[]>(dict._arrayStorage);
        }
        [Test]
        public void Remove_KeyValuePair_False()
        {
            // Arrange
            var dict = new AdaptiveCapacityDictionary<string, object>()
            {
                { "key", "value" },
            };
            var input = new KeyValuePair<string, object?>("other", "value");
            // Act
            var result = ((ICollection<KeyValuePair<string, object?>>)dict).Remove(input);
            // Assert
            Assert.False(result);
            XUnitAssertion.Collection(dict, kvp => { Assert.AreEqual("key", kvp.Key); Assert.AreEqual("value", kvp.Value); });
            Assert.IsInstanceOf<KeyValuePair<string, object?>[]>(dict._arrayStorage);
        }
        // Value comparisons use the default equality comparer.
        [Test]
        public void Remove_KeyValuePair_False_ValueComparisonIsDefault()
        {
            // Arrange
            var dict = new AdaptiveCapacityDictionary<string, object>()
            {
                { "key", "value" },
            };
            var input = new KeyValuePair<string, object?>("key", "valUE");
            // Act
            var result = ((ICollection<KeyValuePair<string, object?>>)dict).Remove(input);
            // Assert
            Assert.False(result);
            XUnitAssertion.Collection(dict, kvp => { Assert.AreEqual("key", kvp.Key); Assert.AreEqual("value", kvp.Value); });
            Assert.IsInstanceOf<KeyValuePair<string, object?>[]>(dict._arrayStorage);
        }
        [Test]
        public void Remove_EmptyStorage()
        {
            // Arrange
            var dict = new AdaptiveCapacityDictionary<string, string>();
            // Act
            var result = dict.Remove("key");
            // Assert
            Assert.False(result);
        }
        [Test]
        public void Remove_EmptyStringIsAllowed()
        {
            // Arrange
            var dict = new AdaptiveCapacityDictionary<string, string>();
            // Act
            var result = dict.Remove("");
            // Assert
            Assert.False(result);
        }
        [Test]
        public void Remove_ListStorage_False()
        {
            // Arrange
            var dict = new AdaptiveCapacityDictionary<string, object>()
            {
                { "key", "value" },
            };
            // Act
            var result = dict.Remove("other");
            // Assert
            Assert.False(result);
            XUnitAssertion.Collection(dict, kvp => { Assert.AreEqual("key", kvp.Key); Assert.AreEqual("value", kvp.Value); });
            Assert.IsInstanceOf<KeyValuePair<string, object?>[]>(dict._arrayStorage);
        }
        [Test]
        public void Remove_ListStorage_True()
        {
            // Arrange
            var dict = new AdaptiveCapacityDictionary<string, object>()
            {
                { "key", "value" },
            };
            // Act
            var result = dict.Remove("key");
            // Assert
            Assert.True(result);
            Assert.IsEmpty(dict);
            Assert.IsInstanceOf<KeyValuePair<string, object?>[]>(dict._arrayStorage);
        }
        [Test]
        public void Remove_ListStorage_True_CaseInsensitive()
        {
            // Arrange
            var dict = new AdaptiveCapacityDictionary<string, object>(StringComparer.OrdinalIgnoreCase)
            {
                { "key", "value" },
            };
            // Act
            var result = dict.Remove("kEy");
            // Assert
            Assert.True(result);
            Assert.IsEmpty(dict);
            Assert.IsInstanceOf<KeyValuePair<string, object?>[]>(dict._arrayStorage);
        }
        [Test]
        public void Remove_KeyAndOutValue_EmptyStorage()
        {
            // Arrange
            var dict = new AdaptiveCapacityDictionary<string, string>();
            // Act
            var result = dict.Remove("key", out var removedValue);
            // Assert
            Assert.False(result);
            Assert.Null(removedValue);
        }
        [Test]
        public void Remove_KeyAndOutValue_EmptyStringIsAllowed()
        {
            // Arrange
            var dict = new AdaptiveCapacityDictionary<string, string>();
            // Act
            var result = dict.Remove("", out var removedValue);
            // Assert
            Assert.False(result);
            Assert.Null(removedValue);
        }
        [Test]
        public void Remove_KeyAndOutValue_ListStorage_False()
        {
            // Arrange
            var dict = new AdaptiveCapacityDictionary<string, object>()
            {
                { "key", "value" },
            };
            // Act
            var result = dict.Remove("other", out var removedValue);
            // Assert
            Assert.False(result);
            Assert.Null(removedValue);
            XUnitAssertion.Collection(dict, kvp => { Assert.AreEqual("key", kvp.Key); Assert.AreEqual("value", kvp.Value); });
            Assert.IsInstanceOf<KeyValuePair<string, object?>[]>(dict._arrayStorage);
        }
        [Test]
        public void Remove_KeyAndOutValue_ListStorage_True()
        {
            // Arrange
            object value = "value";
            var dict = new AdaptiveCapacityDictionary<string, object>()
            {
                { "key", value }
            };
            // Act
            var result = dict.Remove("key", out var removedValue);
            // Assert
            Assert.True(result);
            Assert.AreSame(value, removedValue);
            Assert.IsEmpty(dict);
            Assert.IsInstanceOf<KeyValuePair<string, object?>[]>(dict._arrayStorage);
        }
        [Test]
        public void Remove_KeyAndOutValue_ListStorage_True_CaseInsensitive()
        {
            // Arrange
            object value = "value";
            var dict = new AdaptiveCapacityDictionary<string, object>(StringComparer.OrdinalIgnoreCase)
            {
                { "key", value }
            };
            // Act
            var result = dict.Remove("kEy", out var removedValue);
            // Assert
            Assert.True(result);
            Assert.AreSame(value, removedValue);
            Assert.IsEmpty(dict);
            Assert.IsInstanceOf<KeyValuePair<string, object?>[]>(dict._arrayStorage);
        }
        [Test]
        public void Remove_KeyAndOutValue_ListStorage_KeyExists_First()
        {
            // Arrange
            object value = "value";
            var dict = new AdaptiveCapacityDictionary<string, object>()
            {
                { "key", value },
                { "other", 5 },
                { "dotnet", "rocks" }
            };
            // Act
            var result = dict.Remove("key", out var removedValue);
            // Assert
            Assert.True(result);
            Assert.AreSame(value, removedValue);
            Assert.AreEqual(2, dict.Count);
            Assert.False(dict.ContainsKey("key"));
            Assert.True(dict.ContainsKey("other"));
            Assert.True(dict.ContainsKey("dotnet"));
            Assert.IsInstanceOf<KeyValuePair<string, object?>[]>(dict._arrayStorage);
        }
        [Test]
        public void Remove_KeyAndOutValue_ListStorage_KeyExists_Middle()
        {
            // Arrange
            object value = "value";
            var dict = new AdaptiveCapacityDictionary<string, object>()
            {
                { "other", 5 },
                { "key", value },
                { "dotnet", "rocks" }
            };
            // Act
            var result = dict.Remove("key", out var removedValue);
            // Assert
            Assert.True(result);
            Assert.AreSame(value, removedValue);
            Assert.AreEqual(2, dict.Count);
            Assert.False(dict.ContainsKey("key"));
            Assert.True(dict.ContainsKey("other"));
            Assert.True(dict.ContainsKey("dotnet"));
            Assert.IsInstanceOf<KeyValuePair<string, object?>[]>(dict._arrayStorage);
        }
        [Test]
        public void Remove_KeyAndOutValue_ListStorage_KeyExists_Last()
        {
            // Arrange
            object value = "value";
            var dict = new AdaptiveCapacityDictionary<string, object>()
            {
                { "other", 5 },
                { "dotnet", "rocks" },
                { "key", value }
            };
            // Act
            var result = dict.Remove("key", out var removedValue);
            // Assert
            Assert.True(result);
            Assert.AreSame(value, removedValue);
            Assert.AreEqual(2, dict.Count);
            Assert.False(dict.ContainsKey("key"));
            Assert.True(dict.ContainsKey("other"));
            Assert.True(dict.ContainsKey("dotnet"));
            Assert.IsInstanceOf<KeyValuePair<string, object?>[]>(dict._arrayStorage);
        }
        [Test]
        public void TryAdd_EmptyStringIsAllowed()
        {
            // Arrange
            var dict = new AdaptiveCapacityDictionary<string, string>();
            // Act
            var result = dict.TryAdd("", "foo");
            // Assert
            Assert.True(result);
        }
        [Test]
        public void TryAdd_EmptyStorage_CanAdd()
        {
            // Arrange
            var dict = new AdaptiveCapacityDictionary<string, object>();
            // Act
            var result = dict.TryAdd("key", "value");
            // Assert
            Assert.True(result);
            KeyValuePair<string, object> defaultKvp = default;
            XUnitAssertion.Collection(
                dict._arrayStorage,
                kvp => Assert.AreEqual(new KeyValuePair<string, object?>("key", "value"), kvp),
                kvp => Assert.AreEqual(defaultKvp, kvp),
                kvp => Assert.AreEqual(defaultKvp, kvp),
                kvp => Assert.AreEqual(defaultKvp, kvp),
                kvp => Assert.AreEqual(defaultKvp, kvp),
                kvp => Assert.AreEqual(defaultKvp, kvp),
                kvp => Assert.AreEqual(defaultKvp, kvp),
                kvp => Assert.AreEqual(defaultKvp, kvp),
                kvp => Assert.AreEqual(defaultKvp, kvp),
                kvp => Assert.AreEqual(defaultKvp, kvp));
        }
        [Test]
        public void TryAdd_ArrayStorage_CanAdd()
        {
            // Arrange
            var dict = new AdaptiveCapacityDictionary<string, object>()
            {
                { "key0", "value0" },
            };
            // Act
            var result = dict.TryAdd("key1", "value1");
            // Assert
            Assert.True(result);
            KeyValuePair<string, object> defaultKvp = default;
            XUnitAssertion.Collection(
                dict._arrayStorage,
                kvp => Assert.AreEqual(new KeyValuePair<string, object?>("key0", "value0"), kvp),
                kvp => Assert.AreEqual(new KeyValuePair<string, object?>("key1", "value1"), kvp),
                kvp => Assert.AreEqual(defaultKvp, kvp),
                kvp => Assert.AreEqual(defaultKvp, kvp),
                kvp => Assert.AreEqual(defaultKvp, kvp),
                kvp => Assert.AreEqual(defaultKvp, kvp),
                kvp => Assert.AreEqual(defaultKvp, kvp),
                kvp => Assert.AreEqual(defaultKvp, kvp),
                kvp => Assert.AreEqual(defaultKvp, kvp),
                kvp => Assert.AreEqual(defaultKvp, kvp));
        }
        [Test]
        public void TryAdd_ArrayStorage_DoesNotAddWhenKeyIsPresent()
        {
            // Arrange
            var dict = new AdaptiveCapacityDictionary<string, object>()
            {
                { "key0", "value0" },
            };
            // Act
            var result = dict.TryAdd("key0", "value1");
            // Assert
            Assert.False(result);
            KeyValuePair<string, object> defaultKvp = default;
            XUnitAssertion.Collection(
                dict._arrayStorage,
                kvp => Assert.AreEqual(new KeyValuePair<string, object?>("key0", "value0"), kvp),
                kvp => Assert.AreEqual(defaultKvp, kvp),
                kvp => Assert.AreEqual(defaultKvp, kvp),
                kvp => Assert.AreEqual(defaultKvp, kvp),
                kvp => Assert.AreEqual(defaultKvp, kvp),
                kvp => Assert.AreEqual(defaultKvp, kvp),
                kvp => Assert.AreEqual(defaultKvp, kvp),
                kvp => Assert.AreEqual(defaultKvp, kvp),
                kvp => Assert.AreEqual(defaultKvp, kvp),
                kvp => Assert.AreEqual(defaultKvp, kvp));
        }
        [Test]
        public void TryGetValue_EmptyStorage()
        {
            // Arrange
            var dict = new AdaptiveCapacityDictionary<string, string>();
            // Act
            var result = dict.TryGetValue("key", out var value);
            // Assert
            Assert.False(result);
            Assert.Null(value);
        }
        [Test]
        public void TryGetValue_EmptyStringIsAllowed()
        {
            // Arrange
            var dict = new AdaptiveCapacityDictionary<string, string>();
            // Act
            var result = dict.TryGetValue("", out var value);
            // Assert
            Assert.False(result);
            Assert.Null(value);
        }
        [Test]
        public void TryGetValue_ListStorage_False()
        {
            // Arrange
            var dict = new AdaptiveCapacityDictionary<string, object>()
            {
                { "key", "value" },
            };
            // Act
            var result = dict.TryGetValue("other", out var value);
            // Assert
            Assert.False(result);
            Assert.Null(value);
            Assert.IsInstanceOf<KeyValuePair<string, object?>[]>(dict._arrayStorage);
        }
        [Test]
        public void TryGetValue_ListStorage_True()
        {
            // Arrange
            var dict = new AdaptiveCapacityDictionary<string, object>()
            {
                { "key", "value" },
            };
            // Act
            var result = dict.TryGetValue("key", out var value);
            // Assert
            Assert.True(result);
            Assert.AreEqual("value", value);
            Assert.IsInstanceOf<KeyValuePair<string, object?>[]>(dict._arrayStorage);
        }
        [Test]
        public void TryGetValue_ListStorage_True_CaseInsensitive()
        {
            // Arrange
            var dict = new AdaptiveCapacityDictionary<string, object>(StringComparer.OrdinalIgnoreCase)
            {
                { "key", "value" },
            };
            // Act
            var result = dict.TryGetValue("kEy", out var value);
            // Assert
            Assert.True(result);
            Assert.AreEqual("value", value);
            Assert.IsInstanceOf<KeyValuePair<string, object?>[]>(dict._arrayStorage);
        }
        [Test]
        public void ListStorage_SwitchesToDictionaryAfter10_Add()
        {
            // Arrange
            var dict = new AdaptiveCapacityDictionary<string, object>();
            // Act 1
            dict.Add("key", "value");
            // Assert 1
            Assert.IsInstanceOf<KeyValuePair<string, object?>[]>(dict._arrayStorage);

            var storage = dict._arrayStorage;
            Assert.AreEqual(10, storage.Length);
            // Act 2
            dict.Add("key2", "value2");
            dict.Add("key3", "value3");
            dict.Add("key4", "value4");
            dict.Add("key5", "value5");
            dict.Add("key6", "value2");
            dict.Add("key7", "value3");
            dict.Add("key8", "value4");
            dict.Add("key9", "value5");
            dict.Add("key10", "value2");
            dict.Add("key11", "value3");
            // Assert 2
            Assert.Null(dict._arrayStorage);
            Assert.AreEqual(11, dict.Count);
        }
        [Test]
        public void ListStorage_SwitchesToDictionaryAfter10_TryAdd()
        {
            // Arrange
            var dict = new AdaptiveCapacityDictionary<string, object>();
            // Act 1
            dict.TryAdd("key", "value");
            // Assert 1
            Assert.IsInstanceOf<KeyValuePair<string, object?>[]>(dict._arrayStorage);

            var storage = dict._arrayStorage;
            Assert.AreEqual(10, storage.Length);
            // Act 2
            dict.TryAdd("key2", "value2");
            dict.TryAdd("key3", "value3");
            dict.TryAdd("key4", "value4");
            dict.TryAdd("key5", "value5");
            dict.TryAdd("key6", "value2");
            dict.TryAdd("key7", "value3");
            dict.TryAdd("key8", "value4");
            dict.TryAdd("key9", "value5");
            dict.TryAdd("key10", "value2");
            dict.TryAdd("key11", "value3");
            // Assert 2
            Assert.Null(dict._arrayStorage);
            Assert.AreEqual(11, dict.Count);
        }
        [Test]
        public void ListStorage_SwitchesToDictionaryAfter10_Index()
        {
            // Arrange
            var dict = new AdaptiveCapacityDictionary<string, object>();
            // Act 1
            dict["key"] = "value";
            // Assert 1
            Assert.IsInstanceOf<KeyValuePair<string, object?>[]>(dict._arrayStorage);

            var storage = dict._arrayStorage;
            Assert.AreEqual(10, storage.Length);
            // Act 2
            dict["key1"] = "value";
            dict["key2"] = "value";
            dict["key3"] = "value";
            dict["key4"] = "value";
            dict["key5"] = "value";
            dict["key6"] = "value";
            dict["key7"] = "value";
            dict["key8"] = "value";
            dict["key9"] = "value";
            dict["key10"] = "value";
            // Assert 2
            Assert.Null(dict._arrayStorage);
            Assert.AreEqual(11, dict.Count);
        }
        [Test]
        public void ListStorage_RemoveAt_RearrangesInnerArray()
        {
            // Arrange
            var dict = new AdaptiveCapacityDictionary<string, object>();
            dict.Add("key", "value");
            dict.Add("key2", "value2");
            dict.Add("key3", "value3");
            // Assert 1
            Assert.IsInstanceOf<KeyValuePair<string, object?>[]>(dict._arrayStorage);

            var storage = dict._arrayStorage;
            Assert.AreEqual(3, dict.Count);
            // Act
            dict.Remove("key2");
            // Assert 2
            Assert.IsInstanceOf<KeyValuePair<string, object?>[]>(dict._arrayStorage);

            storage = dict._arrayStorage;
            Assert.AreEqual(2, dict.Count);
            Assert.AreEqual("key", storage[0].Key);
            Assert.AreEqual("value", storage[0].Value);
            Assert.AreEqual("key3", storage[1].Key);
            Assert.AreEqual("value3", storage[1].Value);
        }
        private void AssertEmptyArrayStorage(AdaptiveCapacityDictionary<string, string> value)
        {
            Assert.AreSame(Array.Empty<KeyValuePair<string, object?>>(), value._arrayStorage);
        }
        private class RegularType
        {
            public bool IsAwesome { get; set; }
            public int CoolnessFactor { get; set; }
        }
        private class Visibility
        {
            private string? PrivateYo { get; set; }
            internal int ItsInternalDealWithIt { get; set; }
            public bool IsPublic { get; set; }
        }
        private class StaticProperty
        {
            public static bool IsStatic { get; set; }
        }
        private class SetterOnly
        {
            private bool _coolSetOnly;
            public bool CoolSetOnly { set { _coolSetOnly = value; } }
        }
        private class Base
        {
            public bool DerivedProperty { get; set; }
        }
        private class Derived : Base
        {
            public bool TotallySweetProperty { get; set; }
        }
        private class DerivedHiddenProperty : Base
        {
            public new int DerivedProperty { get; set; }
        }
        private class IndexerProperty
        {
            public bool this[string key]
            {
                get { return false; }
                set { }
            }
        }
        private class Address
        {
            public string? City { get; set; }
            public string? State { get; set; }
        }
    }
}
