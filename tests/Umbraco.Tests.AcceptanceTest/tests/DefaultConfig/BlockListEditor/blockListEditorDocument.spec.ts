import {AliasHelper, ConstantHelper, test} from "@umbraco/playwright-testhelpers";
import {DocumentTypeBuilder} from "@umbraco/json-models-builders";
import {expect} from "@playwright/test";
import {BlockListDataTypeBuilder} from "@umbraco/json-models-builders/dist/lib/builders/dataTypes";

test.describe('BlockListEditorDocument', () => {
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

  test('can create empty block list in a document', async ({page, umbracoApi, umbracoUi}) => {
    const groupName = 'blockListGroup';

    await umbracoApi.documentTypes.ensureNameNotExists(documentName);
    await umbracoApi.dataTypes.ensureNameNotExists(blockListName);

    const rootDocType = new DocumentTypeBuilder()
      .withName(documentName)
      .withAllowAsRoot(true)
      .build();
    await umbracoApi.documentTypes.save(rootDocType);

    // Goes to the document created by the API
    await umbracoUi.goToSection(ConstantHelper.sections.settings);
    await page.locator('[data-element="tree-item-documentTypes"]').click({button: "right"});
    await page.locator('[data-element="action-refreshNode"]').click();
    await page.locator('[data-element="tree-item-' + documentName + '"]').click();

    // Adds a group with a BlockList editor
    await umbracoUi.goToAddEditor(groupName, blockListName);
    await page.locator('[data-element="datatype-Block List"]').click();

    // Creates new BlockList editor
    await page.locator('[title="Create a new configuration of Block List"]').click();
    await page.locator('[id="dataTypeName"]').fill(blockListName);
    await page.locator('[data-element="button-submit"]').nth(1).click();
    // Needs to wait for the other button do disappear 
    await page.waitForTimeout(500);
    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.submit));

    // Saves the document
    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.save));

    // Assert
    await expect(page.locator('.umb-notifications__notifications > .alert-success', {hasText: "Datatype saved"})).toBeVisible();
    await expect(page.locator('.umb-notifications__notifications > .alert-success', {hasText: "Document Type saved"})).toBeVisible();

    // Clean
    await umbracoApi.documentTypes.ensureNameNotExists(documentName);
    await umbracoApi.dataTypes.ensureNameNotExists(blockListName);
  });
  
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

    // Goes to the document created by the API
    await umbracoUi.goToSection(ConstantHelper.sections.settings);
    await page.locator('[data-element="tree-item-documentTypes"]').click({button: "right"});
    await page.locator('[data-element="action-refreshNode"]').click();
    await page.locator('[data-element="tree-item-' + documentName + '"]').click();

    // Adds another block list editor to the first group
    await page.locator('[data-element="property-add"]').nth(0).click();
    await page.locator('[data-element="property-name"]').fill('TheBlock');
    await page.locator('[data-element="editor-add"]').click();
    await page.locator('[data-element="datatype-Block List"]').click();
    await page.locator('[title="Select BlockListTestNumbaTwo"]').click();
    await page.locator('[label-key="general_submit"]').click();
    await page.locator('[label-key="buttons_save"]').click();

    // Assert
    await expect(page.locator('.umb-notifications__notifications > .alert-success', {hasText: "Document Type saved"})).toBeVisible();
    // Checks if the new block list is in the group
    await expect(page.locator('[data-element="group-TestName"] >> [data-element="property-theBlock"]')).toBeVisible();

    // Clean
    await umbracoApi.documentTypes.ensureNameNotExists(documentName);
    await umbracoApi.documentTypes.ensureNameNotExists(elementName);
    await umbracoApi.documentTypes.ensureNameNotExists(elementNameSecond);
    await umbracoApi.dataTypes.ensureNameNotExists(blockListName);
    await umbracoApi.dataTypes.ensureNameNotExists(blockListNameSecond);
  });
});