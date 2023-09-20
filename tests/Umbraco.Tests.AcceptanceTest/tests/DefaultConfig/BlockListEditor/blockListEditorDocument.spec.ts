import {AliasHelper, ConstantHelper, test} from "@umbraco/playwright-testhelpers";
import {DocumentTypeBuilder} from "@umbraco/json-models-builders";
import {expect} from "@playwright/test";
import {BlockListDataTypeBuilder} from "@umbraco/json-models-builders/dist/lib/builders/dataTypes";

test.describe('BlockListEditorDocument', () => {
  const documentName = 'DocumentName';
  const elementName = 'TestElement';
  const blockListName = 'BlockListTest';

  const blockListAlias = AliasHelper.toAlias(blockListName);
  const elementAlias = AliasHelper.toAlias(elementName);

  test.beforeEach(async ({page, umbracoApi, umbracoUi}, testInfo) => {
    await umbracoApi.report.report(testInfo);
    await umbracoApi.login();
    await umbracoApi.documentTypes.ensureNameNotExists(documentName);
    await umbracoApi.dataTypes.ensureNameNotExists(blockListName);
  });

  test.afterEach(async ({page, umbracoApi, umbracoUi}) => {
    await umbracoApi.documentTypes.ensureNameNotExists(documentName);
    await umbracoApi.dataTypes.ensureNameNotExists(blockListName);
  });

  test('can create empty block list in a document', async ({page, umbracoApi, umbracoUi}) => {
    const groupName = 'blockListGroup';

    const rootDocType = new DocumentTypeBuilder()
      .withName(documentName)
      .withAllowAsRoot(true)
      .build();
    await umbracoApi.documentTypes.save(rootDocType);

    await umbracoUi.navigateToDocumentType(documentName);

    // Adds a group with a BlockList editor
    await umbracoUi.goToAddEditor(groupName, blockListName);
    // Waits until the selector is visible
    await expect(page.locator('[data-element="datatype-Block List"]')).toBeVisible();
    await page.getByRole('button', { name: 'Block List' }).click();
    // Creates new BlockList editor
    await page.locator('[title="Create a new configuration of Block List"]').click();
    await page.locator('[id="dataTypeName"]').fill(blockListName);
    await page.locator('[data-element="editor-data-type-settings"]').locator('[label-key=' + ConstantHelper.buttons.submit + ']').click();
    // Checks to be sure that the clicked button is not visible
    await expect(page.locator('[data-element="editor-data-type-settings"]').locator('[label-key=' + ConstantHelper.buttons.submit + ']')).not.toBeVisible();
    // Checks to ensure that the button is visible
    await expect(page.locator('[name="propertySettingsForm"]').locator('[label-key=' + ConstantHelper.buttons.submit + ']')).toBeVisible();
    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.submit));

    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.save));

    // Assert
    await expect(page.locator('.umb-notifications__notifications > .alert-success', {hasText: "Datatype saved"})).toBeVisible();
    await expect(page.locator('.umb-notifications__notifications > .alert-success', {hasText: "Document Type saved"})).toBeVisible();
  });

  test('can create multiple block list editors in a document', async ({page, umbracoApi, umbracoUi}) => {
    const elementNameSecond = 'TestElementTwo';
    const blockListNameSecond = 'BlockListTestNumbaTwo';

    const blockListSecondAlias = AliasHelper.toAlias(blockListNameSecond);

    await umbracoApi.documentTypes.ensureNameNotExists(elementName);
    await umbracoApi.documentTypes.ensureNameNotExists(elementNameSecond);
    await umbracoApi.dataTypes.ensureNameNotExists(blockListNameSecond);

    const element = await umbracoApi.documentTypes.createDefaultElementType(elementName,elementAlias);

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

    await umbracoUi.navigateToDocumentType(documentName);

    // Adds another block list editor to the first group
    await page.locator('[data-element="group-TestName"]').locator('[data-element="property-add"]').click();
    await page.locator('[data-element="property-name"]').fill('TheBlock');
    await umbracoUi.clickDataElementByElementName('editor-add');
    await umbracoUi.clickDataElementByElementName('datatype-Block List');
    await page.locator('[title="Select ' + blockListNameSecond + '"]').click();
    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.submit));
    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.save));

    // Assert
    await expect(page.locator('.umb-notifications__notifications > .alert-success', {hasText: "Document Type saved"})).toBeVisible();
    // Checks if the new block list is in the group
    await expect(page.locator('[data-element="group-TestName"] >> [data-element="property-theBlock"]')).toBeVisible();

    // Clean
    await umbracoApi.documentTypes.ensureNameNotExists(elementName);
    await umbracoApi.documentTypes.ensureNameNotExists(elementNameSecond);
    await umbracoApi.dataTypes.ensureNameNotExists(blockListNameSecond);
  });
});
