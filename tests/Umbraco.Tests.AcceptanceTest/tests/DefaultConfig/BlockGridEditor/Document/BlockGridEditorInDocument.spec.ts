import {AliasHelper, ConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {DocumentTypeBuilder} from "@umbraco/json-models-builders";
import {BlockGridDataTypeBuilder} from "@umbraco/json-models-builders/dist/lib/builders/dataTypes";
import {expect} from "@playwright/test";

test.describe('BlockGridEditorInDocument', () => {
  const documentName = 'DocumentName';
  const elementName = 'TestElement';
  let blockGridName = 'BlockGridTest';
  const documentGroupName = 'blockGridGroup';

  const blockGridAlias = AliasHelper.toAlias(blockGridName);
  const elementAlias = AliasHelper.toAlias(elementName);

  test.beforeEach(async ({page, umbracoApi, umbracoUi}, testInfo) => {
    await umbracoApi.report.report(testInfo);
    await umbracoApi.login();
    await umbracoApi.documentTypes.ensureNameNotExists(documentName);
    await umbracoApi.dataTypes.ensureNameNotExists(blockGridName);
  });

  test.afterEach(async ({page, umbracoApi, umbracoUi}) => {
    await umbracoApi.documentTypes.ensureNameNotExists(documentName);
    await umbracoApi.dataTypes.ensureNameNotExists(blockGridName);
  });

  async function createDefaultBlockGridEditorWithoutElement(umbracoApi, element, BlockGridName) {
    const blockGridOne = new BlockGridDataTypeBuilder()
      .withName(BlockGridName)
      .addBlock()
        .withContentElementTypeKey(element['key'])
      .done()
      .build();
    return await umbracoApi.dataTypes.save(blockGridOne);
  }

  async function createDefaultDocumentTypeWithBlockGridEditor(umbracoApi, BlockGridEditor?) {
    const rootDocType = new DocumentTypeBuilder()
      .withName(documentName)
      .withAllowAsRoot(true)
      .addGroup()
        .withName(documentGroupName)
        .addCustomProperty(BlockGridEditor['id'])
          .withLabel(blockGridName)
          .withAlias(blockGridAlias)
        .done()
      .done()
      .build();
    return await umbracoApi.documentTypes.save(rootDocType);
  }

  test('can create an empty block grid editor in a document', async ({page, umbracoApi, umbracoUi}) => {
    // Creates a empty document
    const rootDocType = new DocumentTypeBuilder()
      .withName(documentName)
      .withAllowAsRoot(true)
      .build();
    await umbracoApi.documentTypes.save(rootDocType);

    await umbracoUi.navigateToDocumentType(documentName);

    // Adds a group with a BlockList editor
    await umbracoUi.goToAddEditor(documentGroupName, blockGridName);
    // Waits until the selector is visible
    await expect(page.locator('[data-element="datatype-Block Grid"]')).toBeVisible();
    await umbracoUi.clickDataElementByElementName('datatype-Block Grid');

    // Creates a new BlockGridEditor
    await page.locator('[title="Create a new configuration of Block Grid"]').click();
    await page.locator('[id="dataTypeName"]').fill(blockGridName);
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
    // Checks if the BlockGridEditor is in the document
    await expect(page.locator('[data-element="group-' + documentGroupName + '"]', {hasText: blockGridName})).toBeVisible();
  });

  test('can add a block grid editor with two elements to a document', async ({page, umbracoApi, umbracoUi}) => {
    const elementNameTwo = 'SecondElement';
    const elementTwoAlias = AliasHelper.toAlias(elementNameTwo);

    await umbracoApi.documentTypes.ensureNameNotExists(elementName);
    await umbracoApi.documentTypes.ensureNameNotExists(elementNameTwo);

    const elementOne = await umbracoApi.documentTypes.createDefaultElementType(elementName, elementAlias);
    const elementTwo = await umbracoApi.documentTypes.createDefaultElementType(elementNameTwo, elementTwoAlias);

    // Creates a BlockGridEditor with two elements
    const blockGridType = new BlockGridDataTypeBuilder()
      .withName(blockGridName)
      .addBlock()
        .withContentElementTypeKey(elementOne['key'])
      .done()
      .addBlock()
        .withContentElementTypeKey(elementTwo['key'])
      .done()
      .build();
    await umbracoApi.dataTypes.save(blockGridType);

    // Creates empty document
    const rootDocType = new DocumentTypeBuilder()
      .withName(documentName)
      .withAllowAsRoot(true)
      .build();
    await umbracoApi.documentTypes.save(rootDocType);

    await umbracoUi.navigateToDocumentType(documentName);

    // Adds a group with a BlockGridEditor
    await umbracoUi.goToAddEditor(documentGroupName, blockGridName);
    // Waits until the selector is visible
    await expect(page.locator('[data-element="datatype-Block Grid"]')).toBeVisible();
    await umbracoUi.clickDataElementByElementName('datatype-Block Grid');
    await page.locator('[title="Select ' + blockGridName + '"]').click();
    // Checks to be sure that the clicked button is not visible
    await expect(page.locator('[data-element="editor-data-type-settings"]').locator('[label-key=' + ConstantHelper.buttons.submit + ']')).not.toBeVisible();
    // Checks to ensure that the button is visible
    await expect(page.locator('[name="propertySettingsForm"]').locator('[label-key=' + ConstantHelper.buttons.submit + ']')).toBeVisible();
    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.submit));
    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.save));

    // Assert
    await expect(page.locator('.umb-notifications__notifications > .alert-success', {hasText: "Document Type saved"})).toBeVisible();
    // Checks if the BlockGridEditor is in the document
    await expect(page.locator('[data-element="group-' + documentGroupName + '"]', {hasText: blockGridName})).toBeVisible();

    // Clean
    await umbracoApi.documentTypes.ensureNameNotExists(elementName);
    await umbracoApi.documentTypes.ensureNameNotExists(elementNameTwo);
  });

  test('can create multiple block list editors in a document', async ({page, umbracoApi, umbracoUi}) => {
    const elementNameTwo = 'ElementNameTwo';
    const elementAliasTwo = AliasHelper.toAlias(elementNameTwo);
    const blockGridNameTwo = 'BlockGridNameTwo';

    await umbracoApi.documentTypes.ensureNameNotExists(elementName);
    await umbracoApi.documentTypes.ensureNameNotExists(elementNameTwo);
    await umbracoApi.dataTypes.ensureNameNotExists(blockGridNameTwo);

    // Creates the first BlockGridEditor with an Element
    const element = await umbracoApi.documentTypes.createDefaultElementType(elementName, elementAlias);
    const BlockGridEditorOne = await createDefaultBlockGridEditorWithoutElement(umbracoApi, element, blockGridName);

    // Creates the second BlockGridEditor with an Element
    // Element
    const elementTwo = new DocumentTypeBuilder()
      .withName(elementNameTwo)
      .withAlias(elementAliasTwo)
      .AsElementType()
      .addGroup()
        .withName('TestString')
        .withAlias('testString')
        .addTextBoxProperty()
          .withLabel('Title')
          .withAlias('title')
        .done()
      .done()
      .build();
    await umbracoApi.documentTypes.save(elementTwo);
    // BlockGrid
    const blockGridTwo = new BlockGridDataTypeBuilder()
      .withName(blockGridNameTwo)
      .addBlock()
        .withContentElementTypeKey(elementTwo['key'])
        .withLabel('Howdy')
      .done()
      .build();
    await umbracoApi.dataTypes.save(blockGridTwo);

    // Creates a Document with the first BlockGridEditor
    await createDefaultDocumentTypeWithBlockGridEditor(umbracoApi, BlockGridEditorOne);

    await umbracoUi.navigateToDocumentType(documentName);

    // Adds another BlockGridEditor
    await page.locator('[data-element="group-' + documentGroupName + '"]').locator('[data-element="property-add"]').click();
    await page.locator('[data-element="property-name"]').fill('TheBlock');
    await umbracoUi.clickDataElementByElementName('editor-add');
    await expect(page.locator('[data-element="datatype-Block Grid"]')).toBeVisible();
    await umbracoUi.clickDataElementByElementName('datatype-Block Grid');
    await page.locator('[title="Select ' + blockGridNameTwo + '"]').click();
    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.submit));
    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.save));

    // Assert
    await expect(page.locator('.umb-notifications__notifications > .alert-success', {hasText: "Document Type saved"})).toBeVisible();
    // Checks if the new block list is in the group
    await expect(page.locator('[data-element="group-' + documentGroupName + '"]', {hasText: blockGridName})).toBeVisible();
    await expect(page.locator('[data-element="group-' + documentGroupName + '"]', {hasText: blockGridNameTwo})).toBeVisible();

    // Clean
    await umbracoApi.documentTypes.ensureNameNotExists(elementName);
    await umbracoApi.documentTypes.ensureNameNotExists(elementNameTwo);
    await umbracoApi.dataTypes.ensureNameNotExists(blockGridNameTwo);
  });

  test('can change a block grid editor in a document to another block grid editor', async ({page, umbracoApi, umbracoUi}) => {
    const blockGridNameTwo = 'BlockGridNameTwo';
    const elementNameTwo = 'ElementNameTwo';
    const elementAliasTwo = AliasHelper.toAlias(elementNameTwo);

    await umbracoApi.documentTypes.ensureNameNotExists(elementName);
    await umbracoApi.documentTypes.ensureNameNotExists(elementNameTwo);
    await umbracoApi.dataTypes.ensureNameNotExists(blockGridNameTwo);

    // Creates the first BlockGridEditor
    const element = await umbracoApi.documentTypes.createDefaultElementType(elementName, elementAlias);
    const BlockGridEditorOne = await createDefaultBlockGridEditorWithoutElement(umbracoApi, element, blockGridName);
    // Creates the second BlockGridEditor
    const elementTwo = await umbracoApi.documentTypes.createDefaultElementType(elementNameTwo, elementAliasTwo);
    await createDefaultBlockGridEditorWithoutElement(umbracoApi, elementTwo, blockGridNameTwo);
    // Creates Document with the first BlockGridEditor
    await createDefaultDocumentTypeWithBlockGridEditor(umbracoApi, BlockGridEditorOne);

    await umbracoUi.navigateToDocumentType(documentName);

    // Switches from the first BlockGridBuilder to the second
    await page.locator('[data-element="group-' + documentGroupName + '"] >> [data-element="property-' + blockGridAlias + '"] >> [title="Edit"]').click();
    await umbracoUi.clickElement(umbracoUi.getButtonByKey('general_change'));
    await umbracoUi.clickDataElementByElementName('datatype-Block Grid');
    await umbracoUi.clickDataElementByElementName('datatypeconfig-' + blockGridNameTwo);
    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.submit));
    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.save));

    // Assert
    await expect(page.locator('.umb-notifications__notifications > .alert-success', {hasText: "Document Type saved"})).toBeVisible();
    // Checks if the second BlockGridEditor is visible
    await expect(page.locator('[data-element="group-' + documentGroupName + '"] >> [data-element="property-' + blockGridAlias + '"]', {hasText: blockGridNameTwo})).toBeVisible();
    // Checks if the first BlockGridEditor is not visible
    await expect(page.locator('[data-element="group-' + documentGroupName + '"] >> [data-element="property-' + blockGridAlias + '"]', {hasText: blockGridName})).not.toBeVisible();

    // Clean
    await umbracoApi.documentTypes.ensureNameNotExists(elementName);
    await umbracoApi.documentTypes.ensureNameNotExists(elementNameTwo);
    await umbracoApi.dataTypes.ensureNameNotExists(blockGridNameTwo);
  });

  test('can remove a block grid editor from a document', async ({page, umbracoApi, umbracoUi}) => {
    await umbracoApi.documentTypes.ensureNameNotExists(elementName);

    // Creates the Element and BlockGridEditor
    const element = await umbracoApi.documentTypes.createDefaultElementType(elementName, elementAlias);
    const BlockGridEditorOne = await createDefaultBlockGridEditorWithoutElement(umbracoApi, element, blockGridName);
    // Creates a Document with the BlockGridEditor
    await createDefaultDocumentTypeWithBlockGridEditor(umbracoApi, BlockGridEditorOne);

    await umbracoUi.navigateToDocumentType(documentName);

    // Deletes the BlockGridEditor from the document
    await page.locator('[data-element="group-' + documentGroupName + '"] >> [data-element="property-' + blockGridAlias + '"] >> [aria-label="Delete property"]').click();
    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey('actions_delete'));
    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.save));

    // Assert
    await expect(page.locator('.umb-notifications__notifications > .alert-success', {hasText: "Document Type saved"})).toBeVisible();
    // Checks if the BlockGridEditor is still in the group
    await expect(page.locator('[data-element="group-' + documentGroupName + '"] >> [data-element="property-' + blockGridAlias + '"]')).not.toBeVisible();

    // Clean
    await umbracoApi.documentTypes.ensureNameNotExists(elementName);
  });
});
