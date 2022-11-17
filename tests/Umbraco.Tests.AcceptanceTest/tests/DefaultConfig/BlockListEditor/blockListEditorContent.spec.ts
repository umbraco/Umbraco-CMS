import {AliasHelper, ConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";
import {ContentBuilder, DocumentTypeBuilder, PartialViewBuilder} from "@umbraco/json-models-builders";
import {BlockListDataTypeBuilder} from "@umbraco/json-models-builders/dist/lib/builders/dataTypes";

test.describe('BlockListEditorContent', () => {

  const documentName = 'DocumentTestName';
  const blockListName = 'BlockListTest';
  const elementName = 'TestElement';

  const documentAlias = AliasHelper.toAlias(documentName);
  const blockListAlias = AliasHelper.toAlias(blockListName);
  // Won't work if I use the to alias for the elementAlias
  const elementAlias = 'testElement';
  
  test.beforeEach(async ({page, umbracoApi, umbracoUi}, testInfo) => {
    await umbracoApi.report.report(testInfo);
    await umbracoApi.login();
    await umbracoApi.documentTypes.ensureNameNotExists(documentName);
    await umbracoApi.documentTypes.ensureNameNotExists(elementName);
    await umbracoApi.dataTypes.ensureNameNotExists(blockListName);
  });
  
  test.afterEach(async ({page, umbracoApi, umbracoUi}) => {
    await umbracoApi.documentTypes.ensureNameNotExists(documentName);
    await umbracoApi.documentTypes.ensureNameNotExists(elementName);
    await umbracoApi.dataTypes.ensureNameNotExists(blockListName);
  });

  async function createDefaultBlockList(umbracoApi, blockListName, element){
    const dataTypeBlockList = new BlockListDataTypeBuilder()
      .withName(blockListName)
      .addBlock()
      .withContentElementTypeKey(element['key'])
      .withSettingsElementTypeKey(element['key'])
      .done()
      .build();
    return await umbracoApi.dataTypes.save(dataTypeBlockList);
  }
  
  async function createDocumentWithOneBlockListEditor(umbracoApi, element, dataType){
    
    if(element == null) {
      element = await umbracoApi.documentTypes.createDefaultElementType(elementName, elementAlias);
    }
    
    if(dataType == null) {
      dataType = await createDefaultBlockList(umbracoApi, blockListName, element);
    }
    
    const docType = new DocumentTypeBuilder()
      .withName(documentName)
      .withAlias(documentAlias)
      .withAllowAsRoot(true)
      .addGroup()
        .withName('BlockListGroup')
        .addCustomProperty(dataType['id'])
          .withAlias(blockListAlias)
        .done()
      .done()
      .build();
    await umbracoApi.documentTypes.save(docType);
    
    return element;
  }
  
  async function createContentWithOneBlockListEditor(umbracoApi, element) {
    
    if(element == null) {
      element = await createDocumentWithOneBlockListEditor(umbracoApi, null, null);
    }
    
    const rootContentNode = new ContentBuilder()
      .withContentTypeAlias(documentAlias)
      .withAction(ConstantHelper.actions.save)
      .addVariant()
        .withName(blockListName)
        .withSave(true)
        .addProperty()
          .withAlias(blockListAlias)
          .addBlockListValue()
            .addBlockListEntry()
              .withContentTypeKey(element['key'])
              .appendContentProperties(element.groups[0].properties[0].alias, "aliasTest")
            .done()
          .done()
        .done()
      .done()
      .build();
    await umbracoApi.content.save(rootContentNode);
    
    return element;
  }

  test('can create content with a block list editor', async ({page, umbracoApi, umbracoUi}) => {
    await createDocumentWithOneBlockListEditor(umbracoApi, null, null);
    
    const rootContentNode = new ContentBuilder()
      .withContentTypeAlias(documentAlias)
      .withAction(ConstantHelper.actions.save)
      .addVariant()
        .withName(blockListName)
        .withSave(true)
      .done()
      .build();
    await umbracoApi.content.save(rootContentNode);

    await umbracoUi.goToSection(ConstantHelper.sections.content);
    await umbracoUi.refreshContentTree();

    // Opens the content with the block list editor
    await umbracoUi.clickDataElementByElementName('tree-item-' + blockListName);

    // Adds TestElement
    await page.locator('[key="blockEditor_addThis"]', {hasText: elementName}).click();
    await page.locator('[id="sub-view-0"]').locator('[id="title"]').fill('Testing...');
    await page.locator('[label="Create"]').click();

    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.saveAndPublish));

    // Assert
    await umbracoUi.isSuccessNotificationVisible();
    
    // Checks if the content was created
    await expect(page.locator('.umb-block-list__block--view')).toHaveCount(1);
    await expect(page.locator('.umb-block-list__block--view').nth(0)).toHaveText(elementName);
  });

  test('can update content with a block list editor', async ({page, umbracoApi, umbracoUi}) => {
    await umbracoApi.content.deleteAllContent();

    await createContentWithOneBlockListEditor(umbracoApi, null);

    await umbracoUi.goToSection(ConstantHelper.sections.content);
    await umbracoUi.refreshContentTree();

    // Opens the content with the block list editor
    await umbracoUi.clickDataElementByElementName('tree-item-' + blockListName);

    // Updates the block list editor inside of the content
    await page.locator('[ui-sortable="vm.sortableOptions"]').nth(0).click();
    // Updates content
    await page.locator('[id="sub-view-0"]').locator('[id="title"]').fill('ContentTest');
    await umbracoUi.clickDataElementByElementName('sub-view-settings');
    // Adds text to the setting element
    await page.locator('[id="sub-view-1"]').locator('[id="title"]').fill('SettingTest');
    await page.locator('[label="Submit"]').click();
    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.saveAndPublish));

    // Assert
    await umbracoUi.isSuccessNotificationVisible();

    // Clean
    await umbracoApi.content.deleteAllContent();
  });

  test('can delete a block list editor in content', async ({page, umbracoApi, umbracoUi}) => {
    await umbracoApi.content.deleteAllContent();

    await createContentWithOneBlockListEditor(umbracoApi, null);

    await umbracoUi.goToSection(ConstantHelper.sections.content);
    await umbracoUi.refreshContentTree();

    // Opens the content with the block list editor
    await umbracoUi.clickDataElementByElementName('tree-item-' + blockListName);

    // Deletes the block list editor inside of the content
    await page.locator('[title="Delete"]').click();

    // Can't use our constant helper because the action for delete does not contain an s. The correct way is 'action-delete'
    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey('actions_delete'));

    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.saveAndPublish));

    // Assert
    await umbracoUi.isSuccessNotificationVisible();
    
    // Checks if the content is actually deleted
    await expect(page.locator('[ui-sortable="vm.sortableOptions"]').nth(0)).not.toBeVisible();

    // Clean
    await umbracoApi.content.deleteAllContent();
  });

  test('can copy block list content and paste it', async ({page, umbracoApi, umbracoUi}) => {
    await umbracoApi.content.deleteAllContent();

    await createContentWithOneBlockListEditor(umbracoApi, null);

    await umbracoUi.goToSection(ConstantHelper.sections.content);
    await umbracoUi.refreshContentTree();

    // Opens the content with the block list editor
    await umbracoUi.clickDataElementByElementName('tree-item-' + blockListName);

    // Checks to make sure that there is only one item
    await expect(page.locator('.umb-block-list__block--view')).toHaveCount(1);

    // Copies block list content
    await page.locator('[title="Copy"]').click();
    await expect(page.locator('.alert-success', {hasText: 'Copied to clipboard'})).toBeVisible();
    // Pastes block list content
    await page.locator('[title="Clipboard"]').click();
    await page.locator('umb-block-card', {hasText: elementName}).click();
    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.saveAndPublish));

    // Assert
    await expect(page.locator('.umb-block-list__block--view')).toHaveCount(2);
    await page.locator('.umb-block-list__block--view').nth(1).click();
    await expect(page.locator('[id="sub-view-0"] >> [name="textbox"]')).toHaveValue('aliasTest');

    // Clean
    await umbracoApi.content.deleteAllContent();
  });

  test('can copy block list content and paste it into another group with the same block list editor', async ({page, umbracoApi, umbracoUi}) => {
    await umbracoApi.content.deleteAllContent();

    const element = await umbracoApi.documentTypes.createDefaultElementType(elementName, elementAlias);

    const dataType = await createDefaultBlockList(umbracoApi, blockListName, element);

    const docType = new DocumentTypeBuilder()
      .withName(documentName)
      .withAlias(documentAlias)
      .withAllowAsRoot(true)
      .addGroup()
        .withName('BlockListGroup')
        .addCustomProperty(dataType['id'])
          .withAlias(blockListAlias)
        .done()
      .done()
      .addGroup()
        .withName('TheBlockListGroupTheSecond')
        .addCustomProperty(dataType['id'])
          .withAlias('theBlockListAliasTheSecond')
        .done()
      .done()
      .build();
    await umbracoApi.documentTypes.save(docType);

    await createContentWithOneBlockListEditor(umbracoApi, element);

    await umbracoUi.goToSection(ConstantHelper.sections.content);
    await umbracoUi.refreshContentTree();

    // Opens the content with the block list editor
    await umbracoUi.clickDataElementByElementName('tree-item-' + blockListName);

    // Checks to make sure that there is only one item in the first group
    await expect(page.locator('[data-element="group-aBlockListGroup"] >> .umb-block-list__block--view')).toHaveCount(1);
    // Checks to make sure that there is no items in the second group
    await expect(page.locator('[data-element="group-aTheBlockListGroupTheSecond"] >> .umb-block-list__block--view')).toHaveCount(0);

    // Copies block list content from the first group
    await page.locator('[title="Copy"]').click();
    await expect(page.locator('.alert-success', {hasText: 'Copied to clipboard'})).toBeVisible();
    // Pastes into the second group
    await page.locator('[title="Clipboard"]').nth(1).click();
    await page.locator('umb-block-card', {hasText: elementName}).click();
    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.saveAndPublish));
    await expect(page.locator('.alert-success', {hasText: 'Content Published'})).toBeVisible();

    // Assert
    await expect(page.locator('[data-element="group-aBlockListGroup"] >> .umb-block-list__block--view')).toHaveCount(1);
    await expect(page.locator('[data-element="group-aTheBlockListGroupTheSecond"] >> .umb-block-list__block--view')).toHaveCount(1);
    await page.locator('[data-element="group-aTheBlockListGroupTheSecond"] >> .umb-block-list__block--view').click();
    await expect(page.locator('[id="sub-view-0"] >> [name="textbox"]')).toHaveValue('aliasTest');

    // Clean
    await umbracoApi.content.deleteAllContent();
  });

  test('can set a minimum of required blocks in content with a block list editor', async ({page, umbracoApi, umbracoUi}) => {
    await umbracoApi.content.deleteAllContent();

    const element = await umbracoApi.documentTypes.createDefaultElementType(elementName, elementAlias);

    const dataTypeBlockList = new BlockListDataTypeBuilder()
      .withName(blockListName)
      .withMin(2)
      .addBlock()
        .withContentElementTypeKey(element['key'])
      .done()
      .build();
    const dataType = await umbracoApi.dataTypes.save(dataTypeBlockList);

    await createDocumentWithOneBlockListEditor(umbracoApi, element, dataType);
    
    await createContentWithOneBlockListEditor(umbracoApi, element);

    await umbracoUi.goToSection(ConstantHelper.sections.content);
    await umbracoUi.refreshContentTree();

    // Opens the content with the block list editor
    await umbracoUi.clickDataElementByElementName('tree-item-' + blockListName);
    // Checks if there is validation for needing 2 entries or more
    await expect(page.locator('[key="validation_entriesShort"]')).toContainText('Minimum 2 entries');
    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.saveAndPublish));
    // Checks if a validation error is thrown when trying to save when there is not enough blocks
    await expect(page.locator('.alert-error')).toBeVisible();

    // Adds another block
    await page.locator('[id="' + blockListAlias + '"]').click();
    await page.locator('[label="Create"]').click();

    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.saveAndPublish));

    // Assert
    await umbracoUi.getSuccessNotification();

    // Clean
    await umbracoApi.content.deleteAllContent();
  });

  test('can set a maximum of required blocks in content with a block list editor', async ({page, umbracoApi, umbracoUi}) => {
    await umbracoApi.content.deleteAllContent();

    const element = await umbracoApi.documentTypes.createDefaultElementType(elementName, elementAlias);

    const dataTypeBlockList = new BlockListDataTypeBuilder()
      .withName(blockListName)
      .withMax(2)
      .addBlock()
        .withContentElementTypeKey(element['key'])
      .done()
      .build();
    const dataType = await umbracoApi.dataTypes.save(dataTypeBlockList);

    await createDocumentWithOneBlockListEditor(umbracoApi, element, dataType);

    const rootContentNode = new ContentBuilder()
      .withContentTypeAlias(documentAlias)
      .withAction(ConstantHelper.actions.save)
      .addVariant()
        .withName(blockListName)
        .withSave(true)
        .addProperty()
          .withAlias(blockListAlias)
          .addBlockListValue()
            .addBlockListEntry()
              .withContentTypeKey(element['key'])
              .appendContentProperties(element.groups[0].properties[0].alias, "aliasTest")
            .done()
            .addBlockListEntry()
              .withContentTypeKey(element['key'])
              .appendContentProperties(element.groups[0].properties[0].alias, "aliasTests")
            .done()
            .addBlockListEntry()
              .withContentTypeKey(element['key'])
              .appendContentProperties(element.groups[0].properties[0].alias, "aliasTester")
            .done()
          .done()
        .done()
      .done()
      .build();
    await umbracoApi.content.save(rootContentNode);

    await umbracoUi.goToSection(ConstantHelper.sections.content);
    await umbracoUi.refreshContentTree();

    // Opens the content with the block list editor
    await umbracoUi.clickDataElementByElementName('tree-item-' + blockListName);

    // Checks if there is validation
    await expect(page.locator('[key="validation_entriesExceed"]')).toContainText('Maximum 2 entries');
    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.saveAndPublish));
    // Checks if a validation error is thrown when trying to save when there is too many blocks
    await expect(page.locator('.alert-error')).toBeVisible();

    // Deletes a block
    await page.locator('[title="Delete"]').nth(2).click();

    // Can't use our constant helper because the action for delete does not contain an s. The correct way is 'action-delete'
    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey('actions_delete'));

    // Assert
    await umbracoUi.getSuccessNotification();

    // Clean
    await umbracoApi.content.deleteAllContent();
  });

  test('can use inline editing mode in content with a block list editor', async ({page, umbracoApi, umbracoUi}) => {
    await umbracoApi.content.deleteAllContent();

    const element = await umbracoApi.documentTypes.createDefaultElementType(elementName, elementAlias);

    const dataTypeBlockList = new BlockListDataTypeBuilder()
      .withName(blockListName)
      .withUseInlineEditingAsDefault(true)
      .addBlock()
        .withContentElementTypeKey(element['key'])
      .done()
      .build();
    const dataType = await umbracoApi.dataTypes.save(dataTypeBlockList);

    await createDocumentWithOneBlockListEditor(umbracoApi, element, dataType);
    
    await createContentWithOneBlockListEditor(umbracoApi, element);

    await umbracoUi.goToSection(ConstantHelper.sections.content);
    await umbracoUi.refreshContentTree();

    // Opens the content with the block list editor
    await umbracoUi.clickDataElementByElementName('tree-item-' + blockListName);

    // Opens the block in content
    await page.locator('[ui-sortable="vm.sortableOptions"]').nth(0).click();

    // Assert
    await expect(page.locator('[ui-sortable="vm.sortableOptions"]').nth(0).locator('[data-element="property-title"]')).toBeVisible();

    // Clean
    await umbracoApi.content.deleteAllContent();
  });

  test('can see rendered content with a block list editor', async ({page, umbracoApi, umbracoUi}) => {
    await umbracoApi.templates.ensureNameNotExists(documentName);
    await umbracoApi.partialViews.ensureNameNotExists(elementName + '.cshtml');
    await umbracoApi.content.deleteAllContent();

    const element = new DocumentTypeBuilder()
      .withName(elementName)
      .withAlias(elementAlias)
      .AsElementType()
      .addGroup()
        .withName("TestString")
        .withAlias('testString')
        .addTextBoxProperty()
          .withLabel("Title")
          .withAlias("title")
        .done()
        .addRichTextProperty()
          .withLabel('Body')
          .withAlias('body')
        .done()
      .done()
      .build();
    await umbracoApi.documentTypes.save(element);

    const dataType = await createDefaultBlockList(umbracoApi, blockListName, element);

    const docType = new DocumentTypeBuilder()
      .withName(documentName)
      .withAlias('documentTestName')
      .withDefaultTemplate('documentTestName')
      .withAllowAsRoot(true)
      .addGroup()
        .withName('BlockListGroup')
        .addCustomProperty(dataType['id'])
          .withAlias(elementAlias)
        .done()
      .done()
      .build();
    await umbracoApi.documentTypes.save(docType);

    await umbracoApi.templates.edit(documentName, '@using Umbraco.Cms.Web.Common.PublishedModels;\n' +
      '@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage<ContentModels.' + documentName + '>\n' +
      '@using ContentModels = Umbraco.Cms.Web.Common.PublishedModels;' +
      '\n@{' +
      '\n    Layout = null;' +
      '\n}' +
      '\n' +
      '@Html.GetBlockListHtml(Model.' + elementName + ')');

    const partialView = new PartialViewBuilder()
      .withName(elementAlias)
      .withContent("@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage<Umbraco.Cms.Core.Models.Blocks.BlockListItem>;\n" +
        "@using ContentModels = Umbraco.Cms.Web.Common.PublishedModels;\n" +
        "@{\n" +
        "var content = (ContentModels." + elementName + ")Model.Content;\n" +
        "var settings = (ContentModels." + elementName + ")Model.Settings;\n" +
        "}\n" +
        "\n" +
        "<h1>@content.Title</h1>" +
        "<p>@content.Body</p>" +
        "\n" +
        "<h1>@settings.Title</h1>" +
        "<p>@settings.Body</p>")
      .build();
    partialView.virtualPath = "/Views/Partials/blocklist/Components/";
    await umbracoApi.partialViews.save(partialView);

    const rootContentNode = new ContentBuilder()
      .withContentTypeAlias(documentAlias)
      .withAction(ConstantHelper.actions.publish)
      .addVariant()
        .withName('BlockListContent')
        .withSave(true)
        .withPublish(true)
        .addProperty()
          .withAlias(elementAlias)
          .addBlockListValue()
            .addBlockListEntry()
              .withContentTypeKey(element['key'])
              .appendContentProperties(element.groups[0].properties[0].alias, "ContentTest")
              .appendContentProperties(element.groups[0].properties[1].alias, "RTEContent")
              .withSettingsTypeKey(element['key'])
              .appendSettingsProperties(element.groups[0].properties[0].alias, "SettingTest")
              .appendSettingsProperties(element.groups[0].properties[1].alias, "RTESetting")
            .done()
          .done()
        .done()
      .done()
      .build();
    await umbracoApi.content.save(rootContentNode);

    // Assert
    // Ensure that the view gets rendered correctly
    const expected = `<divclass="umb-block-list"><h1>ContentTest</h1><p>RTEContent</p><h1>SettingTest</h1><p>RTESetting</p></div>`;
    await expect(await umbracoApi.content.verifyRenderedContent('/', expected, true)).toBeTruthy();
    
    // Clean
    await umbracoApi.templates.ensureNameNotExists(documentName);
    await umbracoApi.partialViews.ensureNameNotExists(elementAlias + '.cshtml');
    await umbracoApi.content.deleteAllContent();
  });
});