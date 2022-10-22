import {AliasHelper, ConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";
import {
  ApprovedColorPickerDataTypeBuilder,
  DocumentTypeBuilder,
  TextBoxDataTypeBuilder
} from "@umbraco/json-models-builders";

test.describe('DataTypes', () => {

  test.beforeEach(async ({page, umbracoApi}) => {
    await umbracoApi.login();
  });

  test('Tests Approved Colors', async ({page, umbracoApi, umbracoUi}) => {
    const name = 'Approved Colour Test';
    const alias = AliasHelper.toAlias(name);

    await umbracoApi.documentTypes.ensureNameNotExists(name);
    await umbracoApi.content.deleteAllContent();
    await umbracoApi.dataTypes.ensureNameNotExists(name);
    await umbracoApi.templates.ensureNameNotExists(name);

    const pickerDataType = new ApprovedColorPickerDataTypeBuilder()
      .withName(name)
      .withPrevalues(['000000', 'FF0000'])
      .build()
    await umbracoApi.content.createDocTypeWithContent(name, alias, pickerDataType);

    // This is an ugly wait, but we have to wait for cache to rebuild
    await page.waitForTimeout(5000);

    // Editing template with some content
    await umbracoApi.templates.edit(name,
      '@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage<ApprovedColourTest>' +
      '\n@{' +
      '\n    Layout = null;' +
      '\n}' +
      '\n<p style="color:@Model.UmbracoTest">Lorem ipsum dolor sit amet</p>');

    // Enter content
    await umbracoUi.refreshContentTree();
    await umbracoUi.clickElement(umbracoUi.getTreeItem("content", [name]));

    // Pick a colour
    await page.locator('.btn-000000').click();

    // Save
    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.saveAndPublish));
    await umbracoUi.isSuccessNotificationVisible();

    // Assert
    const expected = `<p style="color:000000" > Lorem ipsum dolor sit amet </p>`;
    await expect(umbracoApi.content.verifyRenderedContent('/', expected, true)).toBeTruthy();
    await expect(await page.locator('.umb-button__overlay')).not.toBeVisible();

    // Pick another colour to verify both work
    await page.locator('.btn-FF0000').click();

    // Save
    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.saveAndPublish));
    await umbracoUi.isSuccessNotificationVisible();
    await expect(await page.locator('.umb-button__overlay')).not.toBeVisible();

    // Assert
    const expected2 = '<p style="color:FF0000">Lorem ipsum dolor sit amet</p>';
    await expect(await umbracoApi.content.verifyRenderedContent('/', expected2, true)).toBeTruthy();

    // Clean
    await umbracoApi.documentTypes.ensureNameNotExists(name);
    await umbracoApi.content.deleteAllContent();
    await umbracoApi.dataTypes.ensureNameNotExists(name);
    await umbracoApi.templates.ensureNameNotExists(name);
  });

  test('Tests Textbox Maxlength', async ({page, umbracoApi, umbracoUi}) => {
    const name = 'Textbox Maxlength Test';
    const alias = AliasHelper.toAlias(name);

    await umbracoApi.documentTypes.ensureNameNotExists(name);
    await umbracoApi.content.deleteAllContent();
    await umbracoApi.dataTypes.ensureNameNotExists(name);
    await umbracoApi.templates.ensureNameNotExists(name);

    const textBoxDataType = new TextBoxDataTypeBuilder()
      .withName(name)
      .withMaxChars(10)
      .build()
    await umbracoApi.content.createDocTypeWithContent(name, alias, textBoxDataType);

    // Needs to wait for content to be created.
    await page.waitForTimeout(1000);
    await umbracoUi.refreshContentTree();

    // Enter content
    await umbracoUi.clickElement(umbracoUi.getTreeItem('content', [name]));
    await page.locator('input[name="textbox"]').type('12345678');
    await expect(await page.locator('localize[key="textbox_characters_left"]')).not.toBeVisible();
    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.saveAndPublish));
    await umbracoUi.isSuccessNotificationVisible();

    // Add char and assert helptext appears - no publish to save time & has been asserted above & below
    await page.locator('input[name="textbox"]').type('9');
    await expect(page.locator('localize[key="textbox_characters_left"]', {hasText: "characters left"}).first()).toBeVisible();
    await expect(await umbracoUi.getErrorNotification()).not.toBeVisible();

    // Add char and assert errortext appears and can't save
    await page.locator('input[name="textbox"]').type('10'); // 1 char over max
    await expect(page.locator('localize[key="textbox_characters_exceed"]', {hasText: 'too many'}).first()).toBeVisible();
    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.saveAndPublish));
    await expect(await page.locator('.property-error')).toBeVisible();

    // Clean
    await umbracoApi.documentTypes.ensureNameNotExists(name);
    await umbracoApi.content.deleteAllContent();
    await umbracoApi.dataTypes.ensureNameNotExists(name);
    await umbracoApi.templates.ensureNameNotExists(name);
  });

  test('Test Url Picker', async ({page, umbracoApi, umbracoUi}) => {

    const urlPickerDocTypeName = 'Url Picker Test';
    const pickerDocTypeAlias = AliasHelper.toAlias(urlPickerDocTypeName);

    await umbracoApi.documentTypes.ensureNameNotExists(urlPickerDocTypeName);
    await umbracoApi.content.deleteAllContent();
    await umbracoApi.templates.ensureNameNotExists(urlPickerDocTypeName);

    const pickerDocType = new DocumentTypeBuilder()
      .withName(urlPickerDocTypeName)
      .withAlias(pickerDocTypeAlias)
      .withAllowAsRoot(true)
      .withDefaultTemplate(pickerDocTypeAlias)
      .addGroup()
        .withName('ContentPickerGroup')
        .addUrlPickerProperty()
          .withAlias('picker')
        .done()
      .done()
      .build();
    await umbracoApi.documentTypes.save(pickerDocType);

    await umbracoApi.templates.edit(urlPickerDocTypeName, '@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage<UrlPickerTest>' +
      '\n@{' +
      '\n    Layout = null;' +
      '\n}' +
      '\n@foreach(var link in @Model.Picker)' +
      '\n{' +
      '\n    <a href="@link.Url">@link.Name</a>' +
      '\n}');

    // Create content with url picker
    await page.locator('.umb-tree-root').click({button: "right"});
    await page.locator('[data-element="action-create"]').click();
    await page.locator('[data-element="action-create-' + pickerDocTypeAlias + '"] > .umb-action-link').click();

    // Fill out content
    await umbracoUi.setEditorHeaderName('UrlPickerContent');
    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.saveAndPublish));
    await umbracoUi.isSuccessNotificationVisible();
    await page.locator('.umb-node-preview-add').click();

    // Should really try and find a better way to do this, but umbracoTreeItem tries to click the content pane in the background
    await page.locator('#treePicker >> [data-element="tree-item-UrlPickerContent"]').click();
    await page.locator('.umb-editor-footer-content__right-side > [button-style="success"] > .umb-button > .btn > .umb-button__content').click();
    await expect(await page.locator('.umb-node-preview__name').first()).toBeVisible();

    // Save and publish
    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.saveAndPublish));
    await umbracoUi.isSuccessNotificationVisible();

    // Assert
    await expect(await umbracoUi.getErrorNotification()).not.toBeVisible();
    
    // Testing if the edits match the expected results
    const expected = '<a href="/">UrlPickerContent</a>';
    await expect(await umbracoApi.content.verifyRenderedContent('/', expected, true)).toBeTruthy();

    // Clean
    await umbracoApi.documentTypes.ensureNameNotExists(urlPickerDocTypeName);
    await umbracoApi.content.deleteAllContent();
    await umbracoApi.content.deleteAllContent();
    await umbracoApi.templates.ensureNameNotExists(urlPickerDocTypeName);
  });
});