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

  // test('can create empty block list in a document', async ({page, umbracoApi, umbracoUi}) => {
  //   const groupName = 'blockListGroup';
  //
  //   await umbracoApi.documentTypes.ensureNameNotExists(documentName);
  //   await umbracoApi.dataTypes.ensureNameNotExists(blockListName);
  //
  //   const rootDocType = new DocumentTypeBuilder()
  //     .withName(documentName)
  //     .withAllowAsRoot(true)
  //     .build();
  //   await umbracoApi.documentTypes.save(rootDocType);
  //
  //   await umbracoUi.goToSection(ConstantHelper.sections.settings);
  //   // Goes to the document created by the API
  //   await page.locator('[data-element="tree-item-documentTypes"]').click({button: "right"});
  //   await page.locator('[data-element="action-refreshNode"]').click();
  //   await page.locator('[data-element="tree-item-' + documentName + '"]').click();
  //
  //   // Adds a group with a BlockList editor
  //   await umbracoUi.goToAddEditor(groupName, blockListName);
  //   await page.locator('[data-element="datatype-Block List"]').click();
  //
  //   // Creates new BlockList editor
  //   await page.locator('[title="Create a new configuration of Block List"]').click();
  //   await page.locator('[id="dataTypeName"]').fill(blockListName);
  //   await page.locator('[data-element="button-submit"]').nth(1).click();
  //   // Needs to wait for the other button do disappear 
  //   await page.waitForTimeout(500);
  //   await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.submit));
  //
  //   // Saves the document
  //   await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.save));
  //
  //   // Assert
  //   await expect(page.locator('.umb-notifications__notifications > .alert-success', {hasText: "Datatype saved"})).toBeVisible();
  //   await expect(page.locator('.umb-notifications__notifications > .alert-success', {hasText: "Document Type saved"})).toBeVisible();
  //
  //   // Clean
  //   await umbracoApi.documentTypes.ensureNameNotExists(documentName);
  //   await umbracoApi.dataTypes.ensureNameNotExists(blockListName);
  // });
  //
  // test('can create empty block list datatype', async ({page, umbracoApi, umbracoUi}) => {
  //   await umbracoApi.documentTypes.ensureNameNotExists(documentName);
  //   await umbracoApi.dataTypes.ensureNameNotExists(blockListName);
  //
  //   await umbracoUi.goToSection(ConstantHelper.sections.settings);
  //
  //   // Creates a new datatype
  //   await page.locator('[data-element="tree-item-dataTypes"]').click({button: "right"});
  //   await page.locator('[data-element="action-create"]').click();
  //   await page.locator('[data-element="action-data-type"]').click();
  //
  //   await umbracoUi.setEditorHeaderName(blockListName);
  //
  //   // Adds BlockList as property editor
  //   await page.locator('[data-element="property-editor-add"]').click();
  //   await page.locator('[data-element="propertyeditor-"]', {hasText: "Block List"}).click();
  //
  //   await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.save));
  //
  //   // Assert
  //   await expect(page.locator('.umb-notifications__notifications > .alert-success', {hasText: "Datatype saved"})).toBeVisible();
  //
  //   // Clean 
  //   await umbracoApi.documentTypes.ensureNameNotExists(documentName);
  //   await umbracoApi.dataTypes.ensureNameNotExists(blockListName);
  // });
  //
  // test('can create block list datatype with an element', async ({page, umbracoApi, umbracoUi}) => {
  //   await umbracoApi.documentTypes.ensureNameNotExists(elementName);
  //   await umbracoApi.dataTypes.ensureNameNotExists(blockListName);
  //
  //   await createElement(umbracoApi, elementName);
  //
  //   const blockListType = new BlockListDataTypeBuilder()
  //     .withName(blockListName)
  //     .build();
  //   await umbracoApi.dataTypes.save(blockListType);
  //
  //   await navigateToDataType(page, umbracoUi, blockListName);
  //
  //   // Adds an element to the block list
  //   await page.locator('[key="general_add"]').click();
  //   await page.locator('[data-element="editor-container"]').locator('[data-element="tree-item-' + elementName + '"]').click()
  //   await page.locator('[label-key="buttons_submitChanges"]').click();
  //   await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.save));
  //
  //   // Assert
  //   await expect(page.locator('.umb-notifications__notifications > .alert-success', {hasText: "Datatype saved"})).toBeVisible();
  //
  //   // Clean
  //   await umbracoApi.documentTypes.ensureNameNotExists(elementName);
  //   await umbracoApi.dataTypes.ensureNameNotExists(blockListName);
  // });
  //
  // test('can create block list datatype with multiple elements', async ({page, umbracoApi, umbracoUi}) => {
  //   const elementNameTwo = 'TestElementTheSecond';
  //
  //   await umbracoApi.documentTypes.ensureNameNotExists(elementName);
  //   await umbracoApi.documentTypes.ensureNameNotExists(elementNameTwo);
  //   await umbracoApi.dataTypes.ensureNameNotExists(blockListName);
  //
  //   const element = await createElement(umbracoApi, elementName);
  //   await createElement(umbracoApi, elementNameTwo);
  //
  //   const blockListType = new BlockListDataTypeBuilder()
  //     .withName(blockListName)
  //     .addBlock()
  //       .withContentElementTypeKey(element['key'])
  //     .done()
  //     .build();
  //   await umbracoApi.dataTypes.save(blockListType);
  //
  //   await navigateToDataType(page, umbracoUi, blockListName);
  //
  //   // Adds an element to the block list
  //   await page.locator('[key="general_add"]').click();
  //   await page.locator('[data-element="editor-container"]').locator('[data-element="tree-item-' + elementNameTwo + '"]').click()
  //   await page.locator('[label-key="buttons_submitChanges"]').click();
  //   await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.save));
  //
  //   // Assert
  //   await expect(page.locator('.umb-notifications__notifications > .alert-success', {hasText: "Datatype saved"})).toBeVisible();
  //
  //   // Clean
  //   await umbracoApi.documentTypes.ensureNameNotExists(elementName);
  //   await umbracoApi.documentTypes.ensureNameNotExists(elementNameTwo);
  //   await umbracoApi.dataTypes.ensureNameNotExists(blockListName);
  // });

  test('can create multiple block list editors in a document', async ({page, umbracoApi, umbracoUi}) => {
    const elementNameSecond = 'TestElementTwo';
    const blockListNameSecond = 'BlockListTestNumbaTwo';
    
    const blockListAlias = AliasHelper.toAlias(blockListName)
    const blockListSecondAlias = AliasHelper.toAlias(blockListNameSecond)

    await umbracoApi.documentTypes.ensureNameNotExists(documentName);
    await umbracoApi.documentTypes.ensureNameNotExists(elementName);
    await umbracoApi.documentTypes.ensureNameNotExists(elementNameSecond);
    await umbracoApi.dataTypes.ensureNameNotExists(blockListName);
    await umbracoApi.dataTypes.ensureNameNotExists(blockListNameSecond);
    
    const element = await createElement(umbracoApi, elementName);

    const elementAliasSecond = AliasHelper.toAlias(elementNameSecond);
    const elementTypeSecond = new DocumentTypeBuilder()
      .withName(elementNameSecond)
      .withAlias(elementAliasSecond)
      .AsElementType()
      .addGroup()
        .withName('TestString')
        .withAlias('testString')
        .addTextBoxProperty()
          .withLabel('Title')
          .withAlias('title')
        .done()
        .addTextBoxProperty()
          .withLabel('Body')
          .withAlias('body')
        .done()
      .done()
      .build();
    await umbracoApi.documentTypes.save(elementTypeSecond);

    const blockListType = new BlockListDataTypeBuilder()
      .withName(blockListName)
      .addBlock()
        .withContentElementTypeKey(element['key'])
      .done()
      .build();
    const blockList = await umbracoApi.dataTypes.save(blockListType);

    const blockListTypeTwo = new BlockListDataTypeBuilder()
      .withName(blockListNameSecond)
      .addBlock()
        .withContentElementTypeKey(elementTypeSecond['key'])
        .withLabel('Howdy')
        .withBackgroundColor('#e06666')
        .withIconColor('#93c47d')
      .done()
      .build();
    const blockListTwo = await umbracoApi.dataTypes.save(blockListTypeTwo);

    const rootDocType = new DocumentTypeBuilder()
      .withName(documentName)
      .withAllowAsRoot(true)
      .addGroup()
        .withName('TestName')
        .addCustomProperty(blockList['id'])
      .with
          .withAlias(blockListAlias)
        .done()
      .done()
      .addGroup()
        .withName('Uno Mas')
        .addCustomProperty(blockListTwo['id'])
          .withAlias(blockListSecondAlias)
        .done()
      .done()
      .build();
    await umbracoApi.documentTypes.save(rootDocType);
    
  });

  // test('cant put the same element in a block list editor', async ({page, umbracoApi, umbracoUi}) => {
  //
  //   await umbracoApi.documentTypes.ensureNameNotExists(elementName);
  //   await umbracoApi.dataTypes.ensureNameNotExists(blockListName);
  //
  //   const element = await createElement(umbracoApi, elementName)
  //
  //   const blockListType = new BlockListDataTypeBuilder()
  //     .withName(blockListName)
  //     .addBlock()
  //       .withContentElementTypeKey(element['key'])
  //     .done()
  //     .build();
  //   await umbracoApi.dataTypes.save(blockListType);
  //
  //   await navigateToDataType(page, umbracoUi, blockListName);
  //
  //   // Tries adding the same element to the block list editor
  //   await page.locator('[key="general_add"]').click();
  //   await page.locator('[data-element="editor-container"]').locator('[data-element="tree-item-' + elementName + '"]').click();
  //
  //   // Assert
  //   await expect(page.locator('.not-allowed', {hasText: elementName})).toBeVisible();
  //   // Checks if the button create New Element Type is still visible. If visible the element was not clickable.
  //   await expect(page.locator('[label-key="blockEditor_labelcreateNewElementType"]')).toBeVisible();
  //   await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.close));
  //   await expect(page.locator('[block-config-model="block"]')).toHaveCount(1);
  //
  //   // Clean
  //   await umbracoApi.documentTypes.ensureNameNotExists(elementName);
  //   await umbracoApi.dataTypes.ensureNameNotExists(blockListName);
  // })
  //
  // test('can edit a block list editor', async ({page, umbracoApi, umbracoUi}) => {
  //   const elementNameTwo = 'AnotherTestElement';
  //
  //   await umbracoApi.documentTypes.ensureNameNotExists(elementName);
  //   await umbracoApi.documentTypes.ensureNameNotExists(elementNameTwo);
  //   await umbracoApi.dataTypes.ensureNameNotExists(blockListName);
  //
  //   const element = await createElement(umbracoApi, elementName);
  //   await createElement(umbracoApi, elementNameTwo);
  //
  //   const blockListType = new BlockListDataTypeBuilder()
  //     .withName(blockListName)
  //     .addBlock()
  //       .withContentElementTypeKey(element['key'])
  //     .done()
  //     .build();
  //   await umbracoApi.dataTypes.save(blockListType);
  //
  //   await navigateToDataType(page, umbracoUi, blockListName);
  //
  //   // Updates properties in a block
  //   await page.locator('[block-config-model="block"] >> nth=0').click();
  //   await page.locator('[name="label"]').fill('test');
  //
  //   await page.locator('[id="editorSize"]').selectOption({value: "large"});
  //
  //   // Changes element type
  //   await page.locator('[key="blockEditor_addSettingsElementType"]').click();
  //   await page.locator('[data-element="tree-item-' + elementNameTwo + '"]').click();
  //
  //   await page.locator('.sp-replacer >> nth=0').click();
  //   await page.locator('[title="#f44336"] >> nth=0').click();
  //   await page.locator('.sp-choose >> nth=0').click();
  //
  //   await page.locator('.sp-replacer >> nth=1').click();
  //   await page.locator('[title="#c90076"] >> nth=1').click();
  //   await page.locator('.sp-choose >> nth=1').click();
  //
  //   await page.locator('[label-key="buttons_submitChanges"]').click();
  //
  //   // Updates properties in the block list editor
  //   await page.locator('[name="numberFieldMin"]').fill('1');
  //   await page.locator('[name="numberFieldMax"]').fill('10');
  //   await page.locator('[id="useLiveEditing"]').click();
  //   await page.locator('[id="useInlineEditingAsDefault"]').click();
  //   await page.locator('[id="maxPropertyWidth"]').fill('100px');
  //
  //   await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.save));
  //
  //   // Assert
  //   await expect(page.locator('.alert-success', {hasText: 'Datatype saved'})).toBeVisible();
  //
  //   // Clean
  //   await umbracoApi.documentTypes.ensureNameNotExists(elementName);
  //   await umbracoApi.documentTypes.ensureNameNotExists(elementNameTwo);
  //   await umbracoApi.dataTypes.ensureNameNotExists(blockListName);
  // });
  //
  // test('can delete a block list editor', async ({page, umbracoApi, umbracoUi}) => {
  //   const elementNameTwo = 'AnotherTestElement';
  //
  //   await umbracoApi.documentTypes.ensureNameNotExists(elementName);
  //   await umbracoApi.documentTypes.ensureNameNotExists(elementNameTwo);
  //   await umbracoApi.dataTypes.ensureNameNotExists(blockListName);
  //
  //   const element = await createElement(umbracoApi, elementName);
  //   const elementTwo = await createElement(umbracoApi, elementNameTwo);
  //
  //   const blockListType = new BlockListDataTypeBuilder()
  //     .withName(blockListName)
  //     .addBlock()
  //       .withContentElementTypeKey(element['key'])
  //     .done()
  //     .addBlock()
  //       .withContentElementTypeKey(elementTwo['key'])
  //     .done()
  //     .build();
  //   await umbracoApi.dataTypes.save(blockListType);
  //
  //   // Navigates to the block list editor
  //   await umbracoUi.goToSection(ConstantHelper.sections.settings);
  //   await page.locator('[data-element="tree-item-dataTypes"]').locator('[data-element="tree-item-expand"]').click();
  //   await page.locator('[data-element="tree-item-' + blockListName + '"]').click({button: "right"});
  //  
  //   // Deletes the block list editor
  //   await page.locator('[data-element="action-delete"]').click();
  //   await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.delete));
  //
  //   // Assert
  //   await expect(page.locator('[data-element="tree-item-' + blockListName + '"]')).not.toBeVisible();
  //
  //   // Clean
  //   await umbracoApi.documentTypes.ensureNameNotExists(elementName);
  //   await umbracoApi.documentTypes.ensureNameNotExists(elementNameTwo);
  // });
  

  //TODO: TESTCASES
  //  

});