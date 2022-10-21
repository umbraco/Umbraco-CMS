import {AliasHelper, ConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";
import {
  DocumentTypeBuilder,
  TextBoxDataTypeBuilder,
  ApprovedColorPickerDataTypeBuilder,
  DataTypeBuilder, BlockListDataType, GridDataTypeBuilder, ContentBuilder
} from "@umbraco/json-models-builders";
import {BlockListDataTypeBuilder} from "@umbraco/json-models-builders/dist/lib/builders/dataTypes";

test.describe('BlockListEditorDataType', () => {

  const documentName = 'DocumentName'
  const elementName = 'TestElement';
  const blockListName = 'BlockListTest';

  const documentAlias = AliasHelper.toAlias(documentName);
  const blockListAlias = AliasHelper.toAlias(blockListName);
  const elementAlias = AliasHelper.toAlias(elementName);

  test.beforeEach(async ({page, umbracoApi, umbracoUi}) => {
    await umbracoApi.login();
  });

  async function createElement(umbracoApi, elementName) {
    const elementAlias = AliasHelper.toAlias(elementName);
    const elementType = new DocumentTypeBuilder()
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
      .done()
      .build();
    await umbracoApi.documentTypes.save(elementType);

    return elementType;
  }

  async function navigateToDataType(page, umbracoUi, dataTypeName) {
    await umbracoUi.goToSection(ConstantHelper.sections.settings);
    await page.locator('[data-element="tree-item-dataTypes"]').locator('[data-element="tree-item-expand"]').click();
    await page.locator('[data-element="tree-item-' + dataTypeName + '"]').click();
  }

  async function createContentWithOneBlockListEditor(umbracoApi, documentName, documentAlias, elementName, elementAlias, blockListName, blockListAlias) {
    const element = await createElement(umbracoApi, elementName);

    const dataTypeBlockList = new BlockListDataTypeBuilder()
      .withName(blockListName)
      .addBlock()
      .withContentElementTypeKey(element['key'])
      .withSettingsElementTypeKey(element['key'])
      .done()
      .build();
    const dataType = await umbracoApi.dataTypes.save(dataTypeBlockList);

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
    const generatedDocType = await umbracoApi.documentTypes.save(docType);

    const rootContentNode = new ContentBuilder()
      .withContentTypeAlias(generatedDocType["alias"])
      .withAction("saveNew")
      .addVariant()
      .withName(blockListName)
      .withSave(true)
      .addBlockListProperty()
      .withAlias("blocklisttest")
      .addValue()
      .addBlockListEntry()
      .withContentTypeKey(element['key'])
      .appendContentProperties(element.groups[0].properties[0].alias, "aliasTest")
      .done()
      .done()
      .done()
      .done()
      .build()
    await umbracoApi.content.save(rootContentNode);
  }

  test('can create content using the block list editor', async ({page, umbracoApi, umbracoUi}) => {

    await umbracoApi.documentTypes.ensureNameNotExists(documentName);
    await umbracoApi.documentTypes.ensureNameNotExists(elementName);
    await umbracoApi.dataTypes.ensureNameNotExists(blockListName);

    const element = await createElement(umbracoApi, elementName)

    const dataTypeBlockList = new BlockListDataTypeBuilder()
      .withName(blockListName)
      .addBlock()
      .withContentElementTypeKey(element['key'])
      .done()
      .build();
    const dataType = await umbracoApi.dataTypes.save(dataTypeBlockList);

    const docType = new DocumentTypeBuilder()
      .withName(documentName)
      .withAlias(documentAlias)
      .withAllowAsRoot(true)
      .addGroup()
      .withName('BlockListGroup')
      .addCustomProperty(dataType['id'])
      .withAlias(blockListAlias)
      .withLabel(blockListName)
      .done()
      .done()
      .build();
    await umbracoApi.documentTypes.save(docType);

    const rootContentNode = new ContentBuilder()
      .withContentTypeAlias(docType["alias"])
      .withAction("saveNew")
      .addVariant()
      .withName(blockListName)
      .withSave(true)
      .done()
      .build()
    await umbracoApi.content.save(rootContentNode);

    await umbracoUi.goToSection(ConstantHelper.sections.content);
    await umbracoUi.refreshContentTree();

    // Opens the content with the block list editor
    await page.locator('[data-element="tree-item-' + blockListName + '"]').click();

    // Adds TestElement
    await page.locator('[key="blockEditor_addThis"]', {hasText: elementName}).click();
    await page.locator('[id="title"]').fill('El hombres');
    await page.locator('[label="Create"]').click();

    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.save));

    // Assert
    await expect(page.locator('.alert-success', {hasText: 'Content saved'})).toBeVisible();

    // Clean
    await umbracoApi.documentTypes.ensureNameNotExists(documentName);
    await umbracoApi.documentTypes.ensureNameNotExists(elementName);
    await umbracoApi.dataTypes.ensureNameNotExists(blockListName);
  });

  test('can delete content with a block list editor', async ({page, umbracoApi, umbracoUi}) => {
    await umbracoApi.documentTypes.ensureNameNotExists(documentName);
    await umbracoApi.documentTypes.ensureNameNotExists(elementName);
    await umbracoApi.dataTypes.ensureNameNotExists(blockListName);
    await umbracoApi.content.deleteAllContent();

    await createContentWithOneBlockListEditor(umbracoApi, documentName, documentAlias, elementName, elementAlias, blockListName, blockListAlias);

    await umbracoUi.goToSection(ConstantHelper.sections.content);
    await umbracoUi.refreshContentTree();

    // Opens the content with the block list editor
    await page.locator('[data-element="tree-item-' + blockListName + '"]').click();

    // Deletes the block list editor inside of the content
    await page.locator('[title="Delete"]').click();
    await page.locator('[label-key="actions_delete"]').click();

    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.saveAndPublish));

    // Assert
    await umbracoUi.isSuccessNotificationVisible();
    // Checks if the content is actually deleted
    await expect(page.locator('[ui-sortable="vm.sortableOptions"]').nth(0)).not.toBeVisible();

    // Clean
    await umbracoApi.documentTypes.ensureNameNotExists(documentName);
    await umbracoApi.documentTypes.ensureNameNotExists(elementName);
    await umbracoApi.dataTypes.ensureNameNotExists(blockListName);
    await umbracoApi.content.deleteAllContent();
  });

  test('can update content with a block list editor', async ({page, umbracoApi, umbracoUi}) => {
    await umbracoApi.documentTypes.ensureNameNotExists(documentName);
    await umbracoApi.documentTypes.ensureNameNotExists(elementName);
    await umbracoApi.dataTypes.ensureNameNotExists(blockListName);
    await umbracoApi.content.deleteAllContent();

    await createContentWithOneBlockListEditor(umbracoApi, documentName, documentAlias, elementName, elementAlias, blockListName, blockListAlias);

    await umbracoUi.goToSection(ConstantHelper.sections.content);
    await umbracoUi.refreshContentTree();

    // Opens the content with the block list editor
    await page.locator('[data-element="tree-item-' + blockListName + '"]').click();

    // Updates the block list editor inside of the content
    await page.locator('[ui-sortable="vm.sortableOptions"]').nth(0).click();
    // Updates content
    await page.locator('[id="sub-view-0"]').locator('[id="title"]').fill('ContentTest');
    await page.locator('[data-element="sub-view-settings"]').click()
    // Adds text to the setting element
    await page.locator('[id="sub-view-1"]').locator('[id="title"]').fill('SettingTest');
    await page.locator('[label="Submit"]').click();
    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.saveAndPublish));

    // Assert
    await umbracoUi.isSuccessNotificationVisible();

    // Clean
    await umbracoApi.documentTypes.ensureNameNotExists(documentName);
    await umbracoApi.documentTypes.ensureNameNotExists(elementName);
    await umbracoApi.dataTypes.ensureNameNotExists(blockListName);
    await umbracoApi.content.deleteAllContent();
  });

  test('can copy block list content and paste it', async ({page, umbracoApi, umbracoUi}) => {
    await umbracoApi.documentTypes.ensureNameNotExists(documentName);
    await umbracoApi.documentTypes.ensureNameNotExists(elementName);
    await umbracoApi.dataTypes.ensureNameNotExists(blockListName);
    await umbracoApi.content.deleteAllContent();

    await createContentWithOneBlockListEditor(umbracoApi, documentName, documentAlias, elementName, elementAlias, blockListName, blockListAlias);

    await umbracoUi.goToSection(ConstantHelper.sections.content);
    await umbracoUi.refreshContentTree();

    // Opens the content with the block list editor
    await page.locator('[data-element="tree-item-' + blockListName + '"]').click();

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
    await umbracoApi.documentTypes.ensureNameNotExists(documentName);
    await umbracoApi.documentTypes.ensureNameNotExists(elementName);
    await umbracoApi.dataTypes.ensureNameNotExists(blockListName);
    await umbracoApi.content.deleteAllContent();
  });

  test('can copy block list content and paste it into another group with the same block list editor', async ({
                                                                                                               page,
                                                                                                               umbracoApi,
                                                                                                               umbracoUi
                                                                                                             }) => {
    await umbracoApi.documentTypes.ensureNameNotExists(documentName);
    await umbracoApi.documentTypes.ensureNameNotExists(elementName);
    await umbracoApi.dataTypes.ensureNameNotExists(blockListName);
    await umbracoApi.content.deleteAllContent();

    const element = await createElement(umbracoApi, elementName);

    const dataTypeBlockList = new BlockListDataTypeBuilder()
      .withName(blockListName)
      .addBlock()
      .withContentElementTypeKey(element['key'])
      .done()
      .build();
    const dataType = await umbracoApi.dataTypes.save(dataTypeBlockList);

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
    const generatedDocType = await umbracoApi.documentTypes.save(docType);

    const rootContentNode = new ContentBuilder()
      .withContentTypeAlias(generatedDocType["alias"])
      .withAction("saveNew")
      .addVariant()
      .withName(blockListName)
      .withSave(true)
      .addBlockListProperty()
      .withAlias("blocklisttest")
      .addValue()
      .addBlockListEntry()
      .withContentTypeKey(element['key'])
      .appendContentProperties(element.groups[0].properties[0].alias, "aliasTest")
      .done()
      .done()
      .done()
      .done()
      .build()
    await umbracoApi.content.save(rootContentNode);

    await umbracoUi.goToSection(ConstantHelper.sections.content);
    await umbracoUi.refreshContentTree();

    // Opens the content with the block list editor
    await page.locator('[data-element="tree-item-' + blockListName + '"]').click();

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
    await umbracoApi.documentTypes.ensureNameNotExists(documentName);
    await umbracoApi.documentTypes.ensureNameNotExists(elementName);
    await umbracoApi.dataTypes.ensureNameNotExists(blockListName);
    await umbracoApi.content.deleteAllContent();
  });

  //TODO: TESTCASES
  //  Remove all items in one blocklist
  //  Test content with different Block List Editor values
  //  Show content with block list editor

});
