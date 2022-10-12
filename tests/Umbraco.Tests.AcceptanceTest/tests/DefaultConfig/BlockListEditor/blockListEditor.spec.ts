import {AliasHelper, ConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";
import {
  DocumentTypeBuilder,
  TextBoxDataTypeBuilder,
  ApprovedColorPickerDataTypeBuilder,
  DataTypeBuilder, BlockListDataType, GridDataTypeBuilder, ContentBuilder
} from "@umbraco/json-models-builders";
import {BlockListDataTypeBuilder} from "@umbraco/json-models-builders/dist/lib/builders/dataTypes";

test.describe('BlockListEditor', () => {

  test.beforeEach(async ({page, umbracoApi, umbracoUi}) => {
    await umbracoApi.login();
  });

  async function createElement(umbracoApi, elementName) {
    const alias = AliasHelper.toAlias(elementName);
    const elementType = new DocumentTypeBuilder()
      .withName(elementName)
      .withAlias(alias)
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

  test('create empty  block list in a document', async ({page, umbracoApi, umbracoUi}) => {
    const documentTypeName = 'TestDocument';
    const blockListPropertyName = 'TestBlock';
    const groupName = 'blockListGroup';

    await umbracoApi.documentTypes.ensureNameNotExists(documentTypeName);
    await umbracoApi.dataTypes.ensureNameNotExists(blockListPropertyName);

    await umbracoUi.goToSection(ConstantHelper.sections.settings);

    const rootDocType = new DocumentTypeBuilder()
      .withName(documentTypeName)
      .withAllowAsRoot(true)
      .build();
    await umbracoApi.documentTypes.save(rootDocType);

    // Goes to the document created by the API
    await page.locator('[data-element="tree-item-documentTypes"]').click({button: "right"});
    await page.locator('[data-element="action-refreshNode"]').click();
    await page.locator('[data-element="tree-item-' + documentTypeName + '"]').click();

    // Adds a group with a BlockList editor
    await umbracoUi.goToAddEditor(groupName, blockListPropertyName);
    await page.locator('[data-element="datatype-Block List"]').click();

    // Creates new BlockList editor
    await page.locator('[title="Create a new configuration of Block List"]').click();
    await page.locator('[id="dataTypeName"]').fill(blockListPropertyName);
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
    await umbracoApi.documentTypes.ensureNameNotExists(documentTypeName);
    await umbracoApi.dataTypes.ensureNameNotExists(blockListPropertyName);
  });

  test('create empty block list datatype', async ({page, umbracoApi, umbracoUi}) => {
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

  test('create block list datatype with an element', async ({page, umbracoApi, umbracoUi}) => {
    const elementTypeName = 'TestDocument';
    const blockListName = 'BlockListTest';

    await umbracoApi.documentTypes.ensureNameNotExists(elementTypeName);
    await umbracoApi.dataTypes.ensureNameNotExists(blockListName);

    await createElement(umbracoApi, elementTypeName);

    const blockListType = new BlockListDataTypeBuilder()
      .withName(blockListName)
      .build();
    await umbracoApi.dataTypes.save(blockListType);

    await navigateToDataType(page, umbracoUi, blockListName);

    // Adds an element to the block list
    await page.locator('[key="general_add"]').click();
    await page.locator('[data-element="editor-container"]').locator('[data-element="tree-item-' + elementTypeName + '"]').click()
    await page.locator('[label-key="buttons_submitChanges"]').click();
    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.save));

    // Assert
    await expect(page.locator('.umb-notifications__notifications > .alert-success', {hasText: "Datatype saved"})).toBeVisible();

    // Clean
    await umbracoApi.documentTypes.ensureNameNotExists(elementTypeName);
    await umbracoApi.dataTypes.ensureNameNotExists(blockListName);
  });

  test('create block list datatype with multiple elements', async ({page, umbracoApi, umbracoUi}) => {
    const elementTypeName = 'TestElement';
    const elementTypeNameTwo = 'TestElementTheSecond';
    const blockListName = 'BlockListTest';

    await umbracoApi.documentTypes.ensureNameNotExists(elementTypeName);
    await umbracoApi.documentTypes.ensureNameNotExists(elementTypeNameTwo);
    await umbracoApi.dataTypes.ensureNameNotExists(blockListName);

    const element = await createElement(umbracoApi, elementTypeName)
    await createElement(umbracoApi, elementTypeNameTwo)

    const blockListType = new BlockListDataTypeBuilder()
      .withName(blockListName)
      .addBlock()
        .withContentElementTypeKey(element['key'])
      .done()
      .build();
    await umbracoApi.dataTypes.save(blockListType);

    await navigateToDataType(page, umbracoUi, blockListName);

    // Adds an element to the block list
    await page.locator('[key="general_add"]').click();
    await page.locator('[data-element="editor-container"]').locator('[data-element="tree-item-' + elementTypeNameTwo + '"]').click()
    await page.locator('[label-key="buttons_submitChanges"]').click();
    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.save));

    // Assert
    await expect(page.locator('.umb-notifications__notifications > .alert-success', {hasText: "Datatype saved"})).toBeVisible();

    // Clean
    await umbracoApi.documentTypes.ensureNameNotExists(elementTypeName);
    await umbracoApi.documentTypes.ensureNameNotExists(elementTypeNameTwo);
    await umbracoApi.dataTypes.ensureNameNotExists(blockListName);
  });

  test('cant put the same element in a block list editor', async ({page, umbracoApi, umbracoUi}) => {
    const elementTypeName = 'TestElement';
    const blockListName = 'BlockListTest';

    await umbracoApi.documentTypes.ensureNameNotExists(elementTypeName);
    await umbracoApi.dataTypes.ensureNameNotExists(blockListName);

    const element = await createElement(umbracoApi, elementTypeName)

    const blockListType = new BlockListDataTypeBuilder()
      .withName(blockListName)
      .addBlock()
        .withContentElementTypeKey(element['key'])
      .done()
      .build();
    await umbracoApi.dataTypes.save(blockListType);

    await navigateToDataType(page, umbracoUi, blockListName);

    // Tries adding the same element to the block list editor
    await page.locator('[key="general_add"]').click();
    await page.locator('[data-element="editor-container"]').locator('[data-element="tree-item-' + elementTypeName + '"]').click();

    // Assert
    await expect(page.locator('.not-allowed', {hasText: elementTypeName})).toBeVisible();
    // Checks if the button create New Element Type is still visible. If visible the element was not clickable.
    await expect(page.locator('[label-key="blockEditor_labelcreateNewElementType"]')).toBeVisible();
    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.close));
    await expect(page.locator('[block-config-model="block"]')).toHaveCount(1);

    // Clean
    await umbracoApi.documentTypes.ensureNameNotExists(elementTypeName);
    await umbracoApi.dataTypes.ensureNameNotExists(blockListName);
  })

  test('can edit a block list editor', async ({page, umbracoApi, umbracoUi}) => {
    const elementTypeName = 'TestElement';
    const elementTypeNameTwo = 'AnotherTestElement';
    const blockListName = 'BlockListTest';

    await umbracoApi.documentTypes.ensureNameNotExists(elementTypeName);
    await umbracoApi.documentTypes.ensureNameNotExists(elementTypeNameTwo);
    await umbracoApi.dataTypes.ensureNameNotExists(blockListName);

    const element = await createElement(umbracoApi, elementTypeName);
    await createElement(umbracoApi, elementTypeNameTwo);

    const blockListType = new BlockListDataTypeBuilder()
      .withName(blockListName)
      .addBlock()
        .withContentElementTypeKey(element['key'])
      .done()
      .build();
    await umbracoApi.dataTypes.save(blockListType);

    await navigateToDataType(page, umbracoUi, blockListName);

    // Updates properties in a block
    await page.locator('[block-config-model="block"] >> nth=0').click();
    await page.locator('[name="label"]').fill('test');

    await page.locator('[id="editorSize"]').selectOption({value: "large"});

    // Changes element type
    await page.locator('[key="blockEditor_addSettingsElementType"]').click();
    await page.locator('[data-element="tree-item-' + elementTypeNameTwo + '"]').click();

    await page.locator('.sp-replacer >> nth=0').click();
    await page.locator('[title="#f44336"] >> nth=0').click();
    await page.locator('.sp-choose >> nth=0').click();

    await page.locator('.sp-replacer >> nth=1').click();
    await page.locator('[title="#c90076"] >> nth=1').click();
    await page.locator('.sp-choose >> nth=1').click();

    await page.locator('[label-key="buttons_submitChanges"]').click();

    // Updates properties in the block list editor
    await page.locator('[name="numberFieldMin"]').fill('1');
    await page.locator('[name="numberFieldMax"]').fill('10');
    await page.locator('[id="useLiveEditing"]').click();
    await page.locator('[id="useInlineEditingAsDefault"]').click();
    await page.locator('[id="maxPropertyWidth"]').fill('100px');

    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.save));

    // Assert
    await expect(page.locator('.alert-success', {hasText: 'Datatype saved'})).toBeVisible();

    // Clean
    await umbracoApi.documentTypes.ensureNameNotExists(elementTypeName);
    await umbracoApi.documentTypes.ensureNameNotExists(elementTypeNameTwo);
    await umbracoApi.dataTypes.ensureNameNotExists(blockListName);
  });

  test('can delete a block list editor', async ({page, umbracoApi, umbracoUi}) => {
    const elementTypeName = 'TestElement';
    const elementTypeNameTwo = 'AnotherTestElement';
    const blockListName = 'BlockListTest';

    await umbracoApi.documentTypes.ensureNameNotExists(elementTypeName);
    await umbracoApi.documentTypes.ensureNameNotExists(elementTypeNameTwo);
    await umbracoApi.dataTypes.ensureNameNotExists(blockListName);

    const element = await createElement(umbracoApi, elementTypeName);
    const elementTwo = await createElement(umbracoApi, elementTypeNameTwo);

    const blockListType = new BlockListDataTypeBuilder()
      .withName(blockListName)
      .addBlock()
        .withContentElementTypeKey(element['key'])
      .done()
      .addBlock()
        .withContentElementTypeKey(elementTwo['key'])
      .done()
      .build();
    await umbracoApi.dataTypes.save(blockListType);

    // Navigates to the block list editor
    await umbracoUi.goToSection(ConstantHelper.sections.settings);
    await page.locator('[data-element="tree-item-dataTypes"]').locator('[data-element="tree-item-expand"]').click();
    await page.locator('[data-element="tree-item-' + blockListName + '"]').click({button: "right"});
    // Deletes the block list editor
    await page.locator('[data-element="action-delete"]').click();
    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.delete));

    // Assert
    await expect(page.locator('[data-element="tree-item-' + blockListName + '"]')).not.toBeVisible();

    // Clean
    await umbracoApi.documentTypes.ensureNameNotExists(elementTypeName);
    await umbracoApi.documentTypes.ensureNameNotExists(elementTypeNameTwo);
  });

  test('create content using the block list editor', async ({page, umbracoApi, umbracoUi}) => {
    const documentTypeName = 'DocumentName'
    const elementTypeName = 'TestElement';
    const blockListName = 'BlockListTest';

    await umbracoApi.documentTypes.ensureNameNotExists(documentTypeName);
    await umbracoApi.documentTypes.ensureNameNotExists(elementTypeName);
    await umbracoApi.dataTypes.ensureNameNotExists(blockListName);

    const aliasDocument = AliasHelper.toAlias(documentTypeName);
    const dataTypeAlias = AliasHelper.toAlias(blockListName);

    const element = await createElement(umbracoApi, elementTypeName)

    const dataTypeBlockList = new BlockListDataTypeBuilder()
      .withName(blockListName)
      .addBlock()
        .withContentElementTypeKey(element['key'])
      .done()
      .build();
    const dataType = await umbracoApi.dataTypes.save(dataTypeBlockList);

    const docType = new DocumentTypeBuilder()
      .withName(documentTypeName)
      .withAlias(aliasDocument)
      .withAllowAsRoot(true)
      .addGroup()
        .withName('BlockListGroup')
        .addCustomProperty(dataType['id'])
          .withAlias(dataTypeAlias)
          .withLabel(blockListName)
        .done()
      .done()
      .build();
    await umbracoApi.documentTypes.save(docType);

    await umbracoUi.goToSection(ConstantHelper.sections.content);

    // Creates content with the created DocumentType
    await page.locator('[data-element="tree-root"]').click({button: "right"});
    await page.locator('[data-element="action-create"]').click();
    await page.locator('[data-element="action-create-' + aliasDocument + '"]').click();

    await umbracoUi.setEditorHeaderName('ContentTest');

    // Adds TestElement
    await page.locator('[key="blockEditor_addThis"]', {hasText: elementTypeName}).click();
    await page.locator('[id="title"]').fill('El hombres');
    await page.locator('[label="Create"]').click();

    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.save));

    // Assert
    await expect(page.locator('.alert-success', {hasText: 'Content saved'})).toBeVisible();

    // Clean
    await umbracoApi.documentTypes.ensureNameNotExists(documentTypeName);
    await umbracoApi.documentTypes.ensureNameNotExists(elementTypeName);
    await umbracoApi.dataTypes.ensureNameNotExists(blockListName);
  });
  
  // Cursed Test
  test('create block list datatype with range of blocks', async ({page, umbracoApi, umbracoUi}) => {
    const documentTypeName = 'DocumentName'
    const elementTypeName = 'TestElement';
    const blockListName = 'BlockListTest';

    const alias = AliasHelper.toAlias(documentTypeName);
    const dataTypeAlias = AliasHelper.toAlias(blockListName);
    const elementAlias = AliasHelper.toAlias(elementTypeName);

    await umbracoApi.documentTypes.ensureNameNotExists(documentTypeName);
    await umbracoApi.documentTypes.ensureNameNotExists(elementTypeName);
    await umbracoApi.dataTypes.ensureNameNotExists(blockListName);

    const element = await createElement(umbracoApi, elementTypeName);

    const dataTypeBlockList = new BlockListDataTypeBuilder()
      .withName(blockListName)
      .addBlock()
        .withContentElementTypeKey(element['key'])
      .done()
      .build();
    const dataType = await umbracoApi.dataTypes.save(dataTypeBlockList);

    const docType = new DocumentTypeBuilder()
      .withName(documentTypeName)
      .withAlias(alias)
      .withAllowAsRoot(true)
      .addGroup()
        .withName('BlockListGroup')
        .addCustomProperty(dataType['id'])
          .withAlias(dataTypeAlias)
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
          .addValue()
            .addBlockListEntry()
              .withContentTypeKey(dataType['key'])
              .appendContentProperties(element.groups[0].properties[0].alias,'Virker det?')
            .done()
          .done()
        .done()
      .done()
      .build()
    const testContent = await umbracoApi.content.save(rootContentNode);
    
    
    // console.log(testContent);
    console.log(rootContentNode);
    // console.log(rootContentNode.variants[0].properties[0].alias);
    console.log(rootContentNode.variants[0]);
    console.log(rootContentNode.variants[0].properties[0]);
    console.log(rootContentNode.variants[0].properties);
    console.log(rootContentNode.variants[0].name);
    
    await expect(page.locator('.qw33qk3ttkm3+ktr32')).toBeVisible();


    // console.log(rootContentNode);
    // console.log(dataType);
    // console.log(dataType['alias']);
    // console.log(dataType['udi']);
    // console.log(dataType['contentData']);
    // console.log(dataType['contentElementTypeKey']);
    // console.log(dataType['contentUdi']);
    // console.log(dataType['key']);
    // console.log(element['key']); 
    // console.log(element['alias']); 
    // console.log(element);
    
    console.log(element.groups[0].properties[0].alias);

    // console.log(element.groups);
    // console.log(element['groups']);
    // console.log(element['properties']); 
    // console.log(element);

    
    
    
    
    
    
    
    
    
    
    
    
    
    
    // Adds an element to the block list
    // await expect(page.locator('2262636')).toHaveCount(2);
    // await page.locator('[k'ey="general_add"]').click();
    // await page.locator('[data-element="editor-container"]').locator('[data-element="tree-item-' + elementTypeName + '"]').click();
    // await page.locator('[label-key="buttons_submitChanges"]').click();
    // await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.save));

  });

  //TEST
  //   const blockListType = new BlockListDataTypeBuilder()
  //     .withName(blockListName)
  //     .addBlock()
  //       .withContentElementTypeKey("4a419e35-8a18-4dc1-a266-c7c758f3d71b")
  //       .withLabel('20202020')
  //       .withIconColor('#2986cc')
  //       .withSettingsElementTypeKey("4a419e35-8a18-4dc1-a266-c7c758f3d71b")
  //       .withBackgroundColor('#990000')
  //       .withThumbnail("~/media/r1umbwhf/umbraco.png")
  //     .withStylesheet("~/css/Test.css")
  //     .done()
  //     .addBlock()
  //       .withContentElementTypeKey("4a419e35-8a18-4dc1-a266-c7c758f3d71b")
  //       .withEditorSize('large')
  //       .withIconColor('#990000')
  //     .done()
  //     .withMax(20)
  //     .withMin(2)
  //     .withMaxPropertyWidth('69')
  //     .withUseLiveEditing(true)
  //     .withUseInlineEditingAsDefault(true)
  //     .build();
  //   await umbracoApi.dataTypes.save(blockListType);
  //  

});