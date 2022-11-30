// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

// ReSharper disable CommentTypo
// ReSharper disable StringLiteralTypo
namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest, PublishedRepositoryEvents = true,
    WithApplication = true)]
public class ContentServicePublishBranchTests : UmbracoIntegrationTest
{
    private IContentService ContentService => GetRequiredService<IContentService>();

    private ILocalizationService LocalizationService => GetRequiredService<ILocalizationService>();

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    [TestCase(1)] // use overload w/ culture: "*"
    [TestCase(2)] // use overload w/ cultures: new [] { "*" }
    public void Can_Publish_Invariant_Branch(int method)
    {
        CreateTypes(out var iContentType, out _);

        IContent iRoot = new Content("iroot", -1, iContentType);
        iRoot.SetValue("ip", "iroot");
        ContentService.Save(iRoot);
        IContent ii1 = new Content("ii1", iRoot, iContentType);
        ii1.SetValue("ip", "vii1");
        ContentService.Save(ii1);
        IContent ii2 = new Content("ii2", iRoot, iContentType);
        ii2.SetValue("ip", "vii2");
        ContentService.Save(ii2);

        // iroot    !published   !edited
        //  ii1     !published   !edited
        //  ii2     !published   !edited

        // !force = publishes those that are actually published, and have changes
        // here: root (root is always published)
        var r = SaveAndPublishInvariantBranch(iRoot, false, method).ToArray();

        // not forcing, ii1 and ii2 not published yet: only root got published
        AssertPublishResults(r, x => x.Content.Name, "iroot");
        AssertPublishResults(r, x => x.Result, PublishResultType.SuccessPublish);

        // prepare
        ContentService.SaveAndPublish(iRoot);
        ContentService.SaveAndPublish(ii1);

        IContent ii11 = new Content("ii11", ii1, iContentType);
        ii11.SetValue("ip", "vii11");
        ContentService.SaveAndPublish(ii11);
        IContent ii12 = new Content("ii12", ii1, iContentType);
        ii11.SetValue("ip", "vii12");
        ContentService.Save(ii12);

        ContentService.SaveAndPublish(ii2);
        IContent ii21 = new Content("ii21", ii2, iContentType);
        ii21.SetValue("ip", "vii21");
        ContentService.SaveAndPublish(ii21);
        IContent ii22 = new Content("ii22", ii2, iContentType);
        ii22.SetValue("ip", "vii22");
        ContentService.Save(ii22);
        ContentService.Unpublish(ii2);

        // iroot    published    !edited
        //  ii1     published    !edited
        //    ii11  published    !edited
        //    ii12  !published   !edited
        //  ii2     !published   !edited
        //    ii21  (published)  !edited
        //    ii22  !published   !edited

        // !force = publishes those that are actually published, and have changes
        // here: nothing
        r = SaveAndPublishInvariantBranch(iRoot, false, method).ToArray();

        // not forcing, ii12 and ii2, ii21, ii22 not published yet: only root, ii1, ii11 got published
        AssertPublishResults(r, x => x.Content.Name, "iroot", "ii1", "ii11");
        AssertPublishResults(
            r,
            x => x.Result,
            PublishResultType.SuccessPublishAlready,
            PublishResultType.SuccessPublishAlready,
            PublishResultType.SuccessPublishAlready);

        // prepare
        iRoot.SetValue("ip", "changed");
        ContentService.Save(iRoot);
        ii11.SetValue("ip", "changed");
        ContentService.Save(ii11);

        // iroot    published    edited     ***
        //  ii1     published    !edited
        //    ii11  published    edited     ***
        //    ii12  !published   !edited
        //  ii2     !published   !edited
        //    ii21  (published)  !edited
        //    ii22  !published   !edited

        // !force = publishes those that are actually published, and have changes
        // here: iroot and ii11

        // not forcing, ii12 and ii2, ii21, ii22 not published yet: only root, ii1, ii11 got published
        r = SaveAndPublishInvariantBranch(iRoot, false, method).ToArray();
        AssertPublishResults(r, x => x.Content.Name, "iroot", "ii1", "ii11");
        AssertPublishResults(
            r,
            x => x.Result,
            PublishResultType.SuccessPublish,
            PublishResultType.SuccessPublishAlready,
            PublishResultType.SuccessPublish);

        // force = publishes everything that has changes
        // here: ii12, ii2, ii22 - ii21 was published already but masked
        r = SaveAndPublishInvariantBranch(iRoot, true, method).ToArray();
        AssertPublishResults(
            r,
            x => x.Content.Name,
            "iroot",
            "ii1",
            "ii11",
            "ii12",
            "ii2",
            "ii21",
            "ii22");
        AssertPublishResults(
            r,
            x => x.Result,
            PublishResultType.SuccessPublishAlready,
            PublishResultType.SuccessPublishAlready,
            PublishResultType.SuccessPublishAlready,
            PublishResultType.SuccessPublish,
            PublishResultType.SuccessPublish,
            PublishResultType.SuccessPublishAlready, // was masked
            PublishResultType.SuccessPublish);

        ii21 = ContentService.GetById(ii21.Id);
        Assert.IsTrue(ii21.Published);
    }

    [Test]
    public void Can_Publish_Variant_Branch_When_No_Changes_On_Root_All_Cultures()
    {
        CreateTypes(out _, out var vContentType);

        // create/publish root
        IContent vRoot = new Content("vroot", -1, vContentType, "de");
        vRoot.SetCultureName("vroot.de", "de");
        vRoot.SetCultureName("vroot.ru", "ru");
        vRoot.SetCultureName("vroot.es", "es");
        vRoot.SetValue("ip", "vroot");
        vRoot.SetValue("vp", "vroot.de", "de");
        vRoot.SetValue("vp", "vroot.ru", "ru");
        vRoot.SetValue("vp", "vroot.es", "es");
        ContentService.SaveAndPublish(vRoot);

        // create/publish child
        IContent iv1 = new Content("iv1", vRoot, vContentType, "de");
        iv1.SetCultureName("iv1.de", "de");
        iv1.SetCultureName("iv1.ru", "ru");
        iv1.SetCultureName("iv1.es", "es");
        iv1.SetValue("ip", "iv1");
        iv1.SetValue("vp", "iv1.de", "de");
        iv1.SetValue("vp", "iv1.ru", "ru");
        iv1.SetValue("vp", "iv1.es", "es");
        ContentService.SaveAndPublish(iv1);

        // update the child
        iv1.SetValue("vp", "UPDATED-iv1.de", "de");
        ContentService.Save(iv1);

        var r = ContentService.SaveAndPublishBranch(vRoot, false)
            .ToArray(); // no culture specified so "*" is used, so all cultures
        Assert.AreEqual(PublishResultType.SuccessPublishAlready, r[0].Result);
        Assert.AreEqual(PublishResultType.SuccessPublishCulture, r[1].Result);
    }

    [Test]
    public void Can_Publish_Variant_Branch_When_No_Changes_On_Root_Specific_Culture()
    {
        CreateTypes(out _, out var vContentType);

        // create/publish root
        IContent vRoot = new Content("vroot", -1, vContentType, "de");
        vRoot.SetCultureName("vroot.de", "de");
        vRoot.SetCultureName("vroot.ru", "ru");
        vRoot.SetCultureName("vroot.es", "es");
        vRoot.SetValue("ip", "vroot");
        vRoot.SetValue("vp", "vroot.de", "de");
        vRoot.SetValue("vp", "vroot.ru", "ru");
        vRoot.SetValue("vp", "vroot.es", "es");
        ContentService.SaveAndPublish(vRoot);

        // create/publish child
        IContent iv1 = new Content("iv1", vRoot, vContentType, "de");
        iv1.SetCultureName("iv1.de", "de");
        iv1.SetCultureName("iv1.ru", "ru");
        iv1.SetCultureName("iv1.es", "es");
        iv1.SetValue("ip", "iv1");
        iv1.SetValue("vp", "iv1.de", "de");
        iv1.SetValue("vp", "iv1.ru", "ru");
        iv1.SetValue("vp", "iv1.es", "es");
        ContentService.SaveAndPublish(iv1);

        // update the child
        iv1.SetValue("vp", "UPDATED-iv1.de", "de");
        var saveResult = ContentService.Save(iv1);

        var r = ContentService.SaveAndPublishBranch(vRoot, false, "de").ToArray();
        Assert.AreEqual(PublishResultType.SuccessPublishAlready, r[0].Result);
        Assert.AreEqual(PublishResultType.SuccessPublishCulture, r[1].Result);
    }

    [Test]
    public void Can_Publish_Variant_Branch()
    {
        CreateTypes(out _, out var vContentType);

        IContent vRoot = new Content("vroot", -1, vContentType, "de");
        vRoot.SetCultureName("vroot.de", "de");
        vRoot.SetCultureName("vroot.ru", "ru");
        vRoot.SetCultureName("vroot.es", "es");
        vRoot.SetValue("ip", "vroot");
        vRoot.SetValue("vp", "vroot.de", "de");
        vRoot.SetValue("vp", "vroot.ru", "ru");
        vRoot.SetValue("vp", "vroot.es", "es");
        ContentService.Save(vRoot);

        IContent iv1 = new Content("iv1", vRoot, vContentType, "de");
        iv1.SetCultureName("iv1.de", "de");
        iv1.SetCultureName("iv1.ru", "ru");
        iv1.SetCultureName("iv1.es", "es");
        iv1.SetValue("ip", "iv1");
        iv1.SetValue("vp", "iv1.de", "de");
        iv1.SetValue("vp", "iv1.ru", "ru");
        iv1.SetValue("vp", "iv1.es", "es");
        ContentService.Save(iv1);

        IContent iv2 = new Content("iv2", vRoot, vContentType, "de");
        iv2.SetCultureName("iv2.de", "de");
        iv2.SetCultureName("iv2.ru", "ru");
        iv2.SetCultureName("iv2.es", "es");
        iv2.SetValue("ip", "iv2");
        iv2.SetValue("vp", "iv2.de", "de");
        iv2.SetValue("vp", "iv2.ru", "ru");
        iv2.SetValue("vp", "iv2.es", "es");
        ContentService.Save(iv2);

        // vroot    !published   !edited
        //  iv1     !published   !edited
        //  iv2     !published   !edited

        // !force = publishes those that are actually published, and have changes
        // here: nothing
        var r = ContentService.SaveAndPublishBranch(vRoot, false).ToArray(); // no culture specified = all cultures

        // not forcing, iv1 and iv2 not published yet: only root got published
        AssertPublishResults(r, x => x.Content.Name, "vroot.de");
        AssertPublishResults(r, x => x.Result, PublishResultType.SuccessPublishCulture);

        // prepare
        vRoot.SetValue("ip", "changed");
        vRoot.SetValue("vp", "changed.de", "de");
        vRoot.SetValue("vp", "changed.ru", "ru");
        vRoot.SetValue("vp", "changed.es", "es");
        ContentService.Save(vRoot); // now root has drafts in all cultures

        ContentService.SaveAndPublish(iv1, new[] { "de", "ru" }); // now iv1 de and ru are published

        iv1.SetValue("ip", "changed");
        iv1.SetValue("vp", "changed.de", "de");
        iv1.SetValue("vp", "changed.ru", "ru");
        iv1.SetValue("vp", "changed.es", "es");
        ContentService.Save(iv1); // now iv1 has drafts in all cultures

        // validate - everything published for root, because no culture was specified = all
        Assert.IsTrue(vRoot.Published);
        Assert.IsTrue(vRoot.IsCulturePublished("de"));
        Assert.IsTrue(vRoot.IsCulturePublished("ru"));
        Assert.IsTrue(vRoot.IsCulturePublished("es"));

        // validate - only some cultures published for iv1
        Assert.IsTrue(iv1.Published);
        Assert.IsTrue(iv1.IsCulturePublished("de"));
        Assert.IsTrue(iv1.IsCulturePublished("ru"));
        Assert.IsFalse(iv1.IsCulturePublished("es"));

        r = ContentService.SaveAndPublishBranch(vRoot, false, "de").ToArray();

        // not forcing, iv2 not published yet: only root and iv1 got published
        AssertPublishResults(r, x => x.Content.Name, "vroot.de", "iv1.de");
        AssertPublishResults(
            r,
            x => x.Result,
            PublishResultType.SuccessPublishCulture,
            PublishResultType.SuccessPublishCulture);

        // reload - SaveAndPublishBranch has modified other instances
        Reload(ref iv1);
        Reload(ref iv2);

        // validate - root
        Assert.IsTrue(vRoot.Published);
        Assert.IsTrue(vRoot.IsCulturePublished("de"));
        Assert.IsFalse(vRoot.IsCultureEdited("de")); // no drafts, this was just published
        Assert.IsTrue(vRoot.IsCulturePublished("ru"));
        Assert.IsTrue(vRoot.IsCultureEdited("ru")); // has draft
        Assert.IsTrue(vRoot.IsCulturePublished("es"));
        Assert.IsTrue(vRoot.IsCultureEdited("es")); // has draft

        Assert.AreEqual("changed",
            vRoot.GetValue("ip", published: true)); // publishing de implies publishing invariants
        Assert.AreEqual("changed.de", vRoot.GetValue("vp", "de", published: true));

        // validate - de and ru are published, es has not been published
        Assert.IsTrue(iv1.Published);
        Assert.IsTrue(iv1.IsCulturePublished("de"));
        Assert.IsTrue(iv1.IsCulturePublished("ru"));
        Assert.IsFalse(iv1.IsCulturePublished("es"));
        Assert.AreEqual("changed", iv1.GetValue("ip", published: true));
        Assert.AreEqual("changed.de", iv1.GetValue("vp", "de", published: true));
        Assert.AreEqual("iv1.ru", iv1.GetValue("vp", "ru", published: true));
    }

    private void Can_Publish_Mixed_Branch(out IContent iRoot, out IContent ii1, out IContent iv11)
    {
        // invariant root -> variant -> invariant
        // variant root -> variant -> invariant
        // variant root -> invariant -> variant
        CreateTypes(out var iContentType, out var vContentType);

        // invariant root -> invariant -> variant
        iRoot = new Content("iroot", -1, iContentType);
        iRoot.SetValue("ip", "iroot");
        ContentService.SaveAndPublish(iRoot);
        ii1 = new Content("ii1", iRoot, iContentType);
        ii1.SetValue("ip", "vii1");
        ContentService.SaveAndPublish(ii1);
        ii1.SetValue("ip", "changed");
        ContentService.Save(ii1);
        iv11 = new Content("iv11.de", ii1, vContentType, "de");
        iv11.SetValue("ip", "iv11");
        iv11.SetValue("vp", "iv11.de", "de");
        iv11.SetValue("vp", "iv11.ru", "ru");
        iv11.SetValue("vp", "iv11.es", "es");
        ContentService.Save(iv11);

        iv11.SetCultureName("iv11.ru", "ru");
        var xxx = ContentService.SaveAndPublish(iv11, new[] { "de", "ru" });

        Assert.AreEqual("iv11.de", iv11.GetValue("vp", "de", published: true));
        Assert.AreEqual("iv11.ru", iv11.GetValue("vp", "ru", published: true));

        iv11.SetValue("ip", "changed");
        iv11.SetValue("vp", "changed.de", "de");
        iv11.SetValue("vp", "changed.ru", "ru");
        ContentService.Save(iv11);
    }

    [Test]
    public void Can_Publish_Mixed_Branch_1()
    {
        Can_Publish_Mixed_Branch(out var iRoot, out var ii1, out var iv11);

        var r = ContentService.SaveAndPublishBranch(iRoot, false, "de").ToArray();
        AssertPublishResults(r, x => x.Content.Name, "iroot", "ii1", "iv11.de");
        AssertPublishResults(
            r,
            x => x.Result,
            PublishResultType.SuccessPublishAlready,
            PublishResultType.SuccessPublish,
            PublishResultType.SuccessPublishCulture);

        // reload - SaveAndPublishBranch has modified other instances
        Reload(ref ii1);
        Reload(ref iv11);

        // the invariant child has been published
        // the variant child has been published for 'de' only
        Assert.AreEqual("changed", ii1.GetValue("ip", published: true));
        Assert.AreEqual("changed", iv11.GetValue("ip", published: true));
        Assert.AreEqual("changed.de", iv11.GetValue("vp", "de", published: true));
        Assert.AreEqual("iv11.ru", iv11.GetValue("vp", "ru", published: true));
    }

    [Test]
    public void Can_Publish_MixedBranch_2()
    {
        Can_Publish_Mixed_Branch(out var iRoot, out var ii1, out var iv11);

        var r = ContentService.SaveAndPublishBranch(iRoot, false, new[] { "de", "ru" }).ToArray();
        AssertPublishResults(r, x => x.Content.Name, "iroot", "ii1", "iv11.de");
        AssertPublishResults(
            r,
            x => x.Result,
            PublishResultType.SuccessPublishAlready,
            PublishResultType.SuccessPublish,
            PublishResultType.SuccessPublishCulture);

        // reload - SaveAndPublishBranch has modified other instances
        Reload(ref ii1);
        Reload(ref iv11);

        // the invariant child has been published
        // the variant child has been published for 'de' and 'ru'
        Assert.AreEqual("changed", ii1.GetValue("ip", published: true));
        Assert.AreEqual("changed", iv11.GetValue("ip", published: true));
        Assert.AreEqual("changed.de", iv11.GetValue("vp", "de", published: true));
        Assert.AreEqual("changed.ru", iv11.GetValue("vp", "ru", published: true));
    }

    private void AssertPublishResults<T>(PublishResult[] values, Func<PublishResult, T> getter, params T[] expected)
    {
        if (expected.Length != values.Length)
        {
            Console.WriteLine(string.Join(", ", values.Select(x => getter(x).ToString())));
        }

        Assert.AreEqual(expected.Length, values.Length);

        for (var i = 0; i < values.Length; i++)
        {
            var value = getter(values[i]);
            Assert.AreEqual(expected[i], value, $"Expected {expected[i]} at {i} but got {value}.");
        }
    }

    private void Reload(ref IContent document)
        => document = ContentService.GetById(document.Id);

    private void CreateTypes(out IContentType iContentType, out IContentType vContentType)
    {
        var langDe = new Language("de", "German") { IsDefault = true };
        LocalizationService.Save(langDe);
        var langRu = new Language("ru", "Russian");
        LocalizationService.Save(langRu);
        var langEs = new Language("es", "Spanish");
        LocalizationService.Save(langEs);

        iContentType = new ContentType(ShortStringHelper, -1)
        {
            Alias = "ict",
            Name = "Invariant Content Type",
            Variations = ContentVariation.Nothing
        };
        iContentType.AddPropertyType(
            new PropertyType(ShortStringHelper, Constants.PropertyEditors.Aliases.TextBox, ValueStorageType.Nvarchar,
                "ip")
            { Variations = ContentVariation.Nothing });
        ContentTypeService.Save(iContentType);

        vContentType = new ContentType(ShortStringHelper, -1)
        {
            Alias = "vct",
            Name = "Variant Content Type",
            Variations = ContentVariation.Culture
        };
        vContentType.AddPropertyType(
            new PropertyType(ShortStringHelper, Constants.PropertyEditors.Aliases.TextBox, ValueStorageType.Nvarchar,
                "ip")
            { Variations = ContentVariation.Nothing });
        vContentType.AddPropertyType(
            new PropertyType(ShortStringHelper, Constants.PropertyEditors.Aliases.TextBox, ValueStorageType.Nvarchar,
                "vp")
            { Variations = ContentVariation.Culture });
        ContentTypeService.Save(vContentType);
    }

    private IEnumerable<PublishResult> SaveAndPublishInvariantBranch(IContent content, bool force, int method)
    {
        // ReSharper disable RedundantArgumentDefaultValue
        // ReSharper disable ArgumentsStyleOther
        switch (method)
        {
            case 1:
                return ContentService.SaveAndPublishBranch(content, force, "*");
            case 2:
                return ContentService.SaveAndPublishBranch(content, force, cultures: new[] { "*" });
            default:
                throw new ArgumentOutOfRangeException(nameof(method));
        }

        // ReSharper restore RedundantArgumentDefaultValue
        // ReSharper restore ArgumentsStyleOther
    }
}
