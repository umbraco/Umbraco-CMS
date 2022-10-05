import {AliasHelper, ConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";
import {
  DocumentTypeBuilder,
  TextBoxDataTypeBuilder,
  ApprovedColorPickerDataTypeBuilder,
  DataTypeBuilder
} from "@umbraco/json-models-builders";

test.describe('BlockListEditor', () => {

  test.beforeEach(async ({page, umbracoApi, umbracoUi}) => {
    await umbracoApi.login();
  });
  
  async function createElement(umbracoApi, documentName){
    const alias = AliasHelper.toAlias(documentName);
    const rootDocType = new DocumentTypeBuilder()
      .withName(documentName)
      .withAlias(alias)
      .withIsAnElementType(true)
      .addGroup()
        .withName("TestString")
        .addTextBoxProperty()
          .withLabel("Title")
          .withAlias("title")
        .done()
      .done()
      .build();
    await umbracoApi.documentTypes.save(rootDocType);
  }

  // test('create empty block list in a document', async ({page, umbracoApi, umbracoUi}) => {
  //   const documentTypeName = 'TestDocument';
  //   const blockListPropertyName = 'TestBlock';
  //   const groupName = 'blockListGroup';
  //
  //   await umbracoApi.documentTypes.ensureNameNotExists(documentTypeName);
  //   await umbracoApi.dataTypes.ensureNameNotExists(blockListPropertyName);
  //
  //   await umbracoUi.goToSection(ConstantHelper.sections.settings);
  //  
  //   const rootDocType = new DocumentTypeBuilder()
  //     .withName(documentTypeName)
  //     .withAllowAsRoot(true)
  //     .build();
  //   await umbracoApi.documentTypes.save(rootDocType);
  //  
  //   // Goes to the document created by the API
  //   await page.locator('[data-element="tree-item-documentTypes"]').click({button: "right"});
  //   await page.locator('[data-element="action-refreshNode"]').click();
  //   await page.locator('[data-element="tree-item-' + documentTypeName + '"]').click();
  //  
  //   // Adds a group with a BlockList editor
  //   await umbracoUi.goToAddEditor(groupName,blockListPropertyName);
  //   await page.locator('[data-element="datatype-Block List"]').click();
  //
  //   // Creates new BlockList editor
  //   await page.locator('[title="Create a new configuration of Block List"]').click();
  //   await page.locator('[id="dataTypeName"]').fill(blockListPropertyName);
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
  //   await umbracoApi.documentTypes.ensureNameNotExists(documentTypeName);
  //   await umbracoApi.dataTypes.ensureNameNotExists(blockListPropertyName);
  // });

  test('create block list datatype', async ({page, umbracoApi, umbracoUi}) => {
    const documentTypeName = 'Test Document';
    const blockListName = 'TestBlockList';

    await umbracoApi.documentTypes.ensureNameNotExists(documentTypeName);
    await umbracoApi.dataTypes.ensureNameNotExists(blockListName);

    await umbracoUi.goToSection(ConstantHelper.sections.settings);

    // Creates a new datatype
    await page.locator('[data-element="tree-item-dataTypes"]').click({button: "right"});
    await page.locator('[data-element="action-create"]').click();
    await page.locator('[data-element="action-data-type"]').click();

    await umbracoUi.setEditorHeaderName(blockListName);

    // Adds BlockList as property editor
    await page.locator('[data-element="property-editor-add"]').click();
    await page.locator('[data-element="propertyeditor-"]', {hasText: "Block List"}).click();
    
    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.save));

    // Assert
    await expect(page.locator('.umb-notifications__notifications > .alert-success', {hasText: "Datatype saved"})).toBeVisible();

    // Clean 
    await umbracoApi.documentTypes.ensureNameNotExists(documentTypeName);
    await umbracoApi.dataTypes.ensureNameNotExists(blockListName);
  });
  
  //TODO: Change so it uses a builder
  test('create block list with a element', async ({page, umbracoApi, umbracoUi}) => {
    const documentTypeName = 'Test Document';
    const blockListName = 'TestBlockList';

    await umbracoApi.documentTypes.ensureNameNotExists(documentTypeName);
    await umbracoApi.dataTypes.ensureNameNotExists(blockListName);

    await createElement(umbracoApi, documentTypeName);

    await umbracoUi.goToSection(ConstantHelper.sections.settings);
    
    // Creates a new datatype
    await page.locator('[data-element="tree-item-dataTypes"]').click({button: "right"});
    await page.locator('[data-element="action-create"]').click();
    await page.locator('[data-element="action-data-type"]').click();
    
    await umbracoUi.setEditorHeaderName(blockListName);

    // Adds BlockList as property editor
    await page.locator('[data-element="property-editor-add"]').click();
    await page.locator('[data-element="propertyeditor-"]', {hasText: "Block List"}).click();

    // Adds Element
    await page.locator('[key="general_add"]').click();
    await page.locator('[data-element="tree-item-' + documentTypeName + '"]').click();
    await page.locator('[label-key="buttons_submitChanges"]').click();

    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.save));

    // Assert
    await expect(page.locator('.umb-notifications__notifications > .alert-success', {hasText: "Datatype saved"})).toBeVisible();

    // Clean 
    await umbracoApi.documentTypes.ensureNameNotExists(documentTypeName);
    await umbracoApi.dataTypes.ensureNameNotExists(blockListName);
  });
});