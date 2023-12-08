import {AliasHelper, ApiHelpers, ConstantHelper, test, UiHelpers} from '@umbraco/playwright-testhelpers';
import {
  ContentBuilder,
  DocumentTypeBuilder,
} from "@umbraco/json-models-builders";

test.describe('Modelsbuilder tests', () => {

  test.beforeEach(async ({ page, umbracoApi }, testInfo) => {
    // Added a longer timeout for our tests
    test.slow();

    await umbracoApi.report.report(testInfo);
    await umbracoApi.login();
  });

  test('Can create and render content', async ({page, umbracoApi, umbracoUi}) => {
    const docTypeName = "TestDocument";
    const docTypeAlias = AliasHelper.toAlias(docTypeName);
    const contentName = "Home";

    await umbracoApi.content.deleteAllContent();
    await umbracoApi.documentTypes.ensureNameNotExists(docTypeName);
    await umbracoApi.templates.ensureNameNotExists(docTypeName);

    const docType = new DocumentTypeBuilder()
      .withName(docTypeName)
      .withAlias(docTypeAlias)
      .withAllowAsRoot(true)
      .withDefaultTemplate(docTypeAlias)
      .addTab()
        .withName("Content")
        .addTextBoxProperty()
          .withAlias("title")
        .done()
      .done()
      .build();
    await umbracoApi.documentTypes.save(docType);

    await umbracoApi.templates.edit(docTypeName, `@using Umbraco.Cms.Web.Common.PublishedModels;
@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage<ContentModels.Testdocument>
@using ContentModels = Umbraco.Cms.Web.Common.PublishedModels;
@{
\tLayout = null;
}

<h1>@Model.Title</h1>`);

    // Time to manually create the content
    await umbracoUi.createContentWithDocumentType(docTypeName);
    await umbracoUi.setEditorHeaderName(contentName);
    // Fortunately for us the input field of a text box has the alias of the property as an id :)
    await page.locator("#title").type("Hello world!");
    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.saveAndPublish));
    await umbracoUi.isSuccessNotificationVisible({timeout:20000});
    // Ensure that we can render it on the frontend = we can compile the models and views
    await umbracoApi.content.verifyRenderedContent("/", "<h1>Hello world!</h1>", true);

    await umbracoApi.content.deleteAllContent();
    await umbracoApi.documentTypes.ensureNameNotExists(docTypeName);
    await umbracoApi.templates.ensureNameNotExists(docTypeName);
  });

  test('Can update document type without updating view', async ({page, umbracoApi, umbracoUi}) => {
    const docTypeName = "TestDocument";
    const docTypeAlias = AliasHelper.toAlias(docTypeName);
    const propertyAlias = "title";
    const propertyValue = "Hello world!";

    await umbracoApi.content.deleteAllContent();
    await umbracoApi.documentTypes.ensureNameNotExists(docTypeName);
    await umbracoApi.templates.ensureNameNotExists(docTypeName);

    const docType = new DocumentTypeBuilder()
      .withName(docTypeName)
      .withAlias(docTypeAlias)
      .withAllowAsRoot(true)
      .withDefaultTemplate(docTypeAlias)
      .addTab()
        .withName("Content")
        .addTextBoxProperty()
          .withAlias(propertyAlias)
        .done()
      .done()
      .build();
    const savedDocType = await umbracoApi.documentTypes.save(docType);

    await umbracoApi.templates.edit(docTypeName, `@using Umbraco.Cms.Web.Common.PublishedModels;
@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage<ContentModels.Testdocument>
@using ContentModels = Umbraco.Cms.Web.Common.PublishedModels;
@{
\tLayout = null;
}

<h1>@Model.Title</h1>`);
    const content = new ContentBuilder()
      .withContentTypeAlias(savedDocType["alias"])
      .withAction("publishNew")
      .addVariant()
        .withName("Home")
        .withSave(true)
        .withPublish(true)
        .addProperty()
          .withAlias(propertyAlias)
          .withValue(propertyValue)
        .done()
      .done()
      .build()
    await umbracoApi.content.save(content);

    // Navigate to the document type
    await umbracoUi.goToSection(ConstantHelper.sections.settings);
    await umbracoUi.clickElement(umbracoUi.getTreeItem("settings", ["Document Types", docTypeName]));
    // Add a new property (this might cause a version error if the viewcache is not cleared, hence this test
    await page.locator('.umb-box-content >> [data-element="property-add"]').click();
    await page.locator('[data-element="property-name"]').type("Second Title");
    await page.locator('[data-element="editor-add"]').click();
    await page.locator('[input-id="datatype-search"]').type("Textstring");
    await page.locator('.umb-card-grid >> [title="Textstring"]').click();
    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.submit));
    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.save));
    // Has a long timeout because it can sometimes take longer than 5 sec to save on the pipeline
    await umbracoUi.isSuccessNotificationVisible({timeout:20000});

    // Now that the content is updated and the models are rebuilt, ensure that we can still render the frontend.
    await umbracoApi.content.verifyRenderedContent("/", "<h1>" + propertyValue + "</h1>", true)

    await umbracoApi.content.deleteAllContent();
    await umbracoApi.documentTypes.ensureNameNotExists(docTypeName);
    await umbracoApi.templates.ensureNameNotExists(docTypeName);
  });

  test('Can update view without updating document type', async ({page, umbracoApi, umbracoUi}) => {
    const docTypeName = "TestDocument";
    const docTypeAlias = AliasHelper.toAlias(docTypeName);
    const propertyAlias = "title";
    const propertyValue = "Hello world!";

    await umbracoApi.content.deleteAllContent();
    await umbracoApi.documentTypes.ensureNameNotExists(docTypeName);
    await umbracoApi.templates.ensureNameNotExists(docTypeName);

    const docType = new DocumentTypeBuilder()
      .withName(docTypeName)
      .withAlias(docTypeAlias)
      .withAllowAsRoot(true)
      .withDefaultTemplate(docTypeAlias)
      .addTab()
        .withName("Content")
        .addTextBoxProperty()
          .withAlias(propertyAlias)
        .done()
      .done()
      .build();
    const savedDocType = await umbracoApi.documentTypes.save(docType);

    await umbracoApi.templates.edit(docTypeName, `@using Umbraco.Cms.Web.Common.PublishedModels;
@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage<ContentModels.Testdocument>
@using ContentModels = Umbraco.Cms.Web.Common.PublishedModels;
@{
\tLayout = null;
}

<h1>@Model.Title</h1>`);

    const content = new ContentBuilder()
      .withContentTypeAlias(savedDocType["alias"])
      .withAction("publishNew")
      .addVariant()
        .withName("Home")
        .withSave(true)
        .withPublish(true)
        .addProperty()
          .withAlias(propertyAlias)
          .withValue(propertyValue)
        .done()
      .done()
      .build();
    await umbracoApi.content.save(content);

    // Navigate to the document type
    await umbracoUi.goToSection(ConstantHelper.sections.settings);
    await umbracoUi.clickElement(umbracoUi.getTreeItem("settings", ["templates", docTypeName]));
    const editor = await page.locator('.ace_content');
    await editor.click();
    // We only have to type out the opening tag, the editor adds the closing tag automatically.
    await editor.type("<p>Edited");
    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.save));

    await umbracoUi.isSuccessNotificationVisible({timeout:20000});

    await umbracoApi.content.verifyRenderedContent("/", "<h1>" + propertyValue + "</h1><p>Edited</p>", true);

    await umbracoApi.content.deleteAllContent();
    await umbracoApi.documentTypes.ensureNameNotExists(docTypeName);
    await umbracoApi.templates.ensureNameNotExists(docTypeName);
  });

  test('Can update view and document type', async ({page, umbracoApi, umbracoUi},testInfo) => {
    const docTypeName = "TestDocument";
    const docTypeAlias = AliasHelper.toAlias(docTypeName);
    const propertyAlias = "title";
    const propertyValue = "Hello world!";
    const contentName = "Home";

    await umbracoApi.content.deleteAllContent();
    await umbracoApi.documentTypes.ensureNameNotExists(docTypeName);
    await umbracoApi.templates.ensureNameNotExists(docTypeName);

    const docType = new DocumentTypeBuilder()
      .withName(docTypeName)
      .withAlias(docTypeAlias)
      .withAllowAsRoot(true)
      .withDefaultTemplate(docTypeAlias)
      .addTab()
        .withName("Content")
        .addTextBoxProperty()
          .withAlias(propertyAlias)
        .done()
      .done()
      .build();
    const savedDocType = await umbracoApi.documentTypes.save(docType);

    await umbracoApi.templates.edit(docTypeName, `@using Umbraco.Cms.Web.Common.PublishedModels;
@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage<ContentModels.Testdocument>
@using ContentModels = Umbraco.Cms.Web.Common.PublishedModels;
@{
\tLayout = null;
}

<h1>@Model.Title</h1>`);

    const content = new ContentBuilder()
      .withContentTypeAlias(savedDocType["alias"])
      .withAction("publishNew")
      .addVariant()
        .withName(contentName)
        .withSave(true)
        .withPublish(true)
        .addProperty()
          .withAlias(propertyAlias)
          .withValue(propertyValue)
        .done()
      .done()
      .build();
    await umbracoApi.content.save(content);

    // Navigate to the document type
    await umbracoUi.goToSection(ConstantHelper.sections.settings);
    await umbracoUi.clickElement(umbracoUi.getTreeItem("settings", ["Document Types", docTypeName]));
    // Add a new property (this might cause a version error if the viewcache is not cleared, hence this test
    await page.locator('.umb-box-content >> [data-element="property-add"]').click();
    await page.locator('[data-element="property-name"]').type("Bod");
    await page.locator('[data-element="editor-add"]').click();
    await page.locator('[input-id="datatype-search"]').type("Textstring");
    await page.locator('.umb-card-grid >> [title="Textstring"]').click();
    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.submit));
    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.save));
    await umbracoUi.isSuccessNotificationVisible();
    await page.locator('span:has-text("×")').click();

    // Update the template
    await umbracoUi.clickElement(umbracoUi.getTreeItem("settings", ["templates", docTypeName]));
    const editor = await page.locator('.ace_content');
    await editor.click();
    // We only have to type out the opening tag, the editor adds the closing tag automatically.
    await editor.type("<p>@Model.Bod");
    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.save));
    await umbracoUi.isSuccessNotificationVisible();
    await page.locator('span:has-text("×")').click();

    // Navigate to the content section and update the content
    await umbracoUi.goToSection(ConstantHelper.sections.content);
    await umbracoUi.refreshContentTree();
    await umbracoUi.clickElement(umbracoUi.getTreeItem("content", [contentName]));
    await page.locator("#bod").type("Fancy body text");
    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.saveAndPublish));

    await page.waitForTimeout(2000);

    await umbracoApi.content.verifyRenderedContent("/", "<h1>" + propertyValue + "</h1><p>Fancy body text</p>", true);

    await umbracoApi.content.deleteAllContent();
    await umbracoApi.documentTypes.ensureNameNotExists(docTypeName);
    await umbracoApi.templates.ensureNameNotExists(docTypeName);
  });
});
