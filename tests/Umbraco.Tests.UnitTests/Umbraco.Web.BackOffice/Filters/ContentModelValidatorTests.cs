// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Umbraco.Cms.Core.PropertyEditors.Validation;
using Umbraco.Cms.Web.BackOffice.PropertyEditors.Validation;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Web.BackOffice.Filters;

[TestFixture]
public class ContentModelValidatorTests
{
    [Test]
    public void Test_Serializer()
    {
        var nestedLevel2 = new ComplexEditorValidationResult();
        var id1 = Guid.NewGuid();
        var addressInfoElementTypeResult = new ComplexEditorElementTypeValidationResult("addressInfo", id1);
        var cityPropertyTypeResult = new ComplexEditorPropertyTypeValidationResult("city");
        cityPropertyTypeResult.AddValidationResult(new ValidationResult("City is invalid"));
        cityPropertyTypeResult.AddValidationResult(new ValidationResult("City cannot be empty"));
        cityPropertyTypeResult.AddValidationResult(new ValidationResult("City is not in Australia", new[] { "country" }));
        cityPropertyTypeResult.AddValidationResult(new ValidationResult("Not a capital city", new[] { "capital" }));
        addressInfoElementTypeResult.ValidationResults.Add(cityPropertyTypeResult);
        nestedLevel2.ValidationResults.Add(addressInfoElementTypeResult);

        var nestedLevel1 = new ComplexEditorValidationResult();
        var id2 = Guid.NewGuid();
        var addressBookElementTypeResult = new ComplexEditorElementTypeValidationResult("addressBook", id2);
        var addressesPropertyTypeResult = new ComplexEditorPropertyTypeValidationResult("addresses");
        addressesPropertyTypeResult.AddValidationResult(new ValidationResult("Must have at least 3 addresses", new[] { "counter" }));
        addressesPropertyTypeResult.AddValidationResult(nestedLevel2); // This is a nested result within the level 1
        addressBookElementTypeResult.ValidationResults.Add(addressesPropertyTypeResult);
        var bookNamePropertyTypeResult = new ComplexEditorPropertyTypeValidationResult("bookName");
        bookNamePropertyTypeResult.AddValidationResult(
            new ValidationResult("Invalid address book name", new[] { "book" }));
        addressBookElementTypeResult.ValidationResults.Add(bookNamePropertyTypeResult);
        nestedLevel1.ValidationResults.Add(addressBookElementTypeResult);

        var id3 = Guid.NewGuid();
        var addressBookElementTypeResult2 = new ComplexEditorElementTypeValidationResult("addressBook", id3);
        var addressesPropertyTypeResult2 = new ComplexEditorPropertyTypeValidationResult("addresses");
        addressesPropertyTypeResult2.AddValidationResult(new ValidationResult("Must have at least 2 addresses", new[] { "counter" }));
        addressBookElementTypeResult2.ValidationResults.Add(addressesPropertyTypeResult);
        var bookNamePropertyTypeResult2 = new ComplexEditorPropertyTypeValidationResult("bookName");
        bookNamePropertyTypeResult2.AddValidationResult(new ValidationResult("Name is too long"));
        addressBookElementTypeResult2.ValidationResults.Add(bookNamePropertyTypeResult2);
        nestedLevel1.ValidationResults.Add(addressBookElementTypeResult2);

        // books is the outer most validation result and doesn't have it's own direct ValidationResult errors
        var outerError = new ComplexEditorValidationResult();
        var id4 = Guid.NewGuid();
        var addressBookCollectionElementTypeResult =
            new ComplexEditorElementTypeValidationResult("addressBookCollection", id4);
        var booksPropertyTypeResult = new ComplexEditorPropertyTypeValidationResult("books");
        booksPropertyTypeResult.AddValidationResult(nestedLevel1); // books is the outer most validation result
        addressBookCollectionElementTypeResult.ValidationResults.Add(booksPropertyTypeResult);
        outerError.ValidationResults.Add(addressBookCollectionElementTypeResult);

        var serialized = JsonConvert.SerializeObject(outerError, Formatting.Indented, new ValidationResultConverter());
        Console.WriteLine(serialized);

        var jsonError = JsonConvert.DeserializeObject<JArray>(serialized);

        Assert.IsNotNull(jsonError.SelectToken("$[0]"));
        Assert.AreEqual(id4.ToString(), jsonError.SelectToken("$[0].$id").Value<string>());
        Assert.AreEqual("addressBookCollection", jsonError.SelectToken("$[0].$elementTypeAlias").Value<string>());
        Assert.AreEqual(string.Empty, jsonError.SelectToken("$[0].ModelState['_Properties.books.invariant.null'][0]").Value<string>());

        var error0 = jsonError.SelectToken("$[0].books") as JArray;
        Assert.IsNotNull(error0);
        Assert.AreEqual(id2.ToString(), error0.SelectToken("$[0].$id").Value<string>());
        Assert.AreEqual("addressBook", error0.SelectToken("$[0].$elementTypeAlias").Value<string>());
        Assert.IsNotNull(error0.SelectToken("$[0].ModelState"));
        Assert.AreEqual(string.Empty, error0.SelectToken("$[0].ModelState['_Properties.addresses.invariant.null'][0]").Value<string>());
        var error1 = error0.SelectToken("$[0].ModelState['_Properties.addresses.invariant.null.counter']") as JArray;
        Assert.IsNotNull(error1);
        Assert.AreEqual(1, error1.Count);
        var error2 = error0.SelectToken("$[0].ModelState['_Properties.bookName.invariant.null.book']") as JArray;
        Assert.IsNotNull(error2);
        Assert.AreEqual(1, error2.Count);

        Assert.AreEqual(id3.ToString(), error0.SelectToken("$[1].$id").Value<string>());
        Assert.AreEqual("addressBook", error0.SelectToken("$[1].$elementTypeAlias").Value<string>());
        Assert.IsNotNull(error0.SelectToken("$[1].ModelState"));
        Assert.AreEqual(string.Empty, error0.SelectToken("$[1].ModelState['_Properties.addresses.invariant.null'][0]").Value<string>());
        var error6 = error0.SelectToken("$[1].ModelState['_Properties.addresses.invariant.null.counter']") as JArray;
        Assert.IsNotNull(error6);
        Assert.AreEqual(1, error6.Count);
        var error7 = error0.SelectToken("$[1].ModelState['_Properties.bookName.invariant.null']") as JArray;
        Assert.IsNotNull(error7);
        Assert.AreEqual(1, error7.Count);

        Assert.IsNotNull(error0.SelectToken("$[0].addresses"));
        Assert.AreEqual(id1.ToString(), error0.SelectToken("$[0].addresses[0].$id").Value<string>());
        Assert.AreEqual("addressInfo", error0.SelectToken("$[0].addresses[0].$elementTypeAlias").Value<string>());
        Assert.IsNotNull(error0.SelectToken("$[0].addresses[0].ModelState"));
        var error3 =
            error0.SelectToken("$[0].addresses[0].ModelState['_Properties.city.invariant.null.country']") as JArray;
        Assert.IsNotNull(error3);
        Assert.AreEqual(1, error3.Count);
        var error4 =
            error0.SelectToken("$[0].addresses[0].ModelState['_Properties.city.invariant.null.capital']") as JArray;
        Assert.IsNotNull(error4);
        Assert.AreEqual(1, error4.Count);
        var error5 = error0.SelectToken("$[0].addresses[0].ModelState['_Properties.city.invariant.null']") as JArray;
        Assert.IsNotNull(error5);
        Assert.AreEqual(2, error5.Count);
    }
}
