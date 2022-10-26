import {AliasHelper, ConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";
import {
  DocumentTypeBuilder, MediaBuilder, MediaFileBuilder, StylesheetBuilder,
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

  test('can create an empty block list datatype', async ({page, umbracoApi, umbracoUi}) => {
    await umbracoApi.documentTypes.ensureNameNotExists(documentName);
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
    await umbracoApi.documentTypes.ensureNameNotExists(documentName);
    await umbracoApi.dataTypes.ensureNameNotExists(blockListName);
  });

  test('can create a block list datatype with an element', async ({page, umbracoApi, umbracoUi}) => {
    await umbracoApi.documentTypes.ensureNameNotExists(elementName);
    await umbracoApi.dataTypes.ensureNameNotExists(blockListName);

    await createElement(umbracoApi, elementName);

    const blockListType = new BlockListDataTypeBuilder()
      .withName(blockListName)
      .build();
    await umbracoApi.dataTypes.save(blockListType);

    await navigateToDataType(page, umbracoUi, blockListName);

    // Adds an element to the block list
    await page.locator('[key="general_add"]').click();
    await page.locator('[data-element="editor-container"]').locator('[data-element="tree-item-' + elementName + '"]').click();
    await page.locator('[label-key="buttons_submitChanges"]').click();
    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.save));

    // Assert
    await expect(page.locator('.umb-notifications__notifications > .alert-success', {hasText: "Datatype saved"})).toBeVisible();
    // Checks if the element is added
    await expect(page.locator('.umb-block-card-grid', {hasText: elementName})).toBeVisible();

    // Clean
    await umbracoApi.documentTypes.ensureNameNotExists(elementName);
    await umbracoApi.dataTypes.ensureNameNotExists(blockListName);
  });

  test('can create block list datatype with two elements', async ({page, umbracoApi, umbracoUi}) => {
    const elementNameTwo = 'SecondTestElement';

    await umbracoApi.documentTypes.ensureNameNotExists(elementName);
    await umbracoApi.documentTypes.ensureNameNotExists(elementNameTwo);
    await umbracoApi.dataTypes.ensureNameNotExists(blockListName);

    const element = await createElement(umbracoApi, elementName);
    await createElement(umbracoApi, elementNameTwo);

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
    await page.locator('[data-element="editor-container"]').locator('[data-element="tree-item-' + elementNameTwo + '"]').click();
    await page.locator('[label-key="buttons_submitChanges"]').click();
    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.save));

    // Assert
    await expect(page.locator('.umb-notifications__notifications > .alert-success', {hasText: "Datatype saved"})).toBeVisible();
    // Checks if the elements are added
    await expect(page.locator('[block-config-model="block"] >> nth=0', {hasText: elementName})).toBeVisible();
    await expect(page.locator('[block-config-model="block"] >> nth=1', {hasText: elementNameTwo})).toBeVisible();

    // Clean
    await umbracoApi.documentTypes.ensureNameNotExists(elementName);
    await umbracoApi.documentTypes.ensureNameNotExists(elementNameTwo);
    await umbracoApi.dataTypes.ensureNameNotExists(blockListName);
  });

  test('can remove an element in a block list datatype', async ({page, umbracoApi, umbracoUi}) => {
    await umbracoApi.documentTypes.ensureNameNotExists(elementName);
    await umbracoApi.dataTypes.ensureNameNotExists(blockListName);

    const element = await createElement(umbracoApi, elementName);
    const blockListType = new BlockListDataTypeBuilder()
      .withName(blockListName)
      .addBlock()
        .withContentElementTypeKey(element['key'])
      .done()
      .build();
    await umbracoApi.dataTypes.save(blockListType);

    await navigateToDataType(page, umbracoUi, blockListName);

    // Deletes the element in the block list
    await page.locator('[key="general_delete"]').click();
    await page.locator('[label-key="actions_delete"]').click();
    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.save));

    // Assert
    await expect(page.locator('.umb-notifications__notifications > .alert-success', {hasText: "Datatype saved"})).toBeVisible();
    // Checks if the element is deleted
    await expect(page.locator('.umb-block-card-grid', {hasText: elementName})).not.toBeVisible();

    // Clean
    await umbracoApi.documentTypes.ensureNameNotExists(elementName);
    await umbracoApi.dataTypes.ensureNameNotExists(blockListName);
  });

  test('cant put the same element in a block list editor', async ({page, umbracoApi, umbracoUi}) => {
    await umbracoApi.documentTypes.ensureNameNotExists(elementName);
    await umbracoApi.dataTypes.ensureNameNotExists(blockListName);

    const element = await createElement(umbracoApi, elementName)

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
    await page.locator('[data-element="editor-container"]').locator('[data-element="tree-item-' + elementName + '"]').click();

    // Assert
    await expect(page.locator('.not-allowed', {hasText: elementName})).toBeVisible();
    // Checks if the button create New Element Type is still visible. If visible the element was not clickable.
    await expect(page.locator('[label-key="blockEditor_labelcreateNewElementType"]')).toBeVisible();
    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.close));
    await expect(page.locator('[block-config-model="block"]')).toHaveCount(1);

    // Clean
    await umbracoApi.documentTypes.ensureNameNotExists(elementName);
    await umbracoApi.dataTypes.ensureNameNotExists(blockListName);
  });  

  test('can edit a block list editor', async ({page, umbracoApi, umbracoUi}) => {
    const elementNameTwo = 'AnotherTestElement';
    const stylesheetName = 'TestStyleSheet';

    const imageName = "Umbraco";
    const umbracoFileValue = {"src": "umbraco.png"};
    const fileName = "umbraco.png"
    const path = fileName;
    const mimeType = "image/png";

    await umbracoApi.documentTypes.ensureNameNotExists(elementName);
    await umbracoApi.documentTypes.ensureNameNotExists(elementNameTwo);
    await umbracoApi.dataTypes.ensureNameNotExists(blockListName);
    await umbracoApi.stylesheets.ensureNameNotExists(stylesheetName + '.css');
    await umbracoApi.media.ensureNameNotExists(imageName);

    const mediaItem = new MediaBuilder()
      .withName(imageName)
      .withContentTypeAlias('Image')
      .addProperty()
        .withAlias('umbracoFile')
        .withValue(umbracoFileValue)
      .done()
      .build();
    const mediaFile = new MediaFileBuilder()
      .withName(fileName)
      .withPath(path)
      .withMimeType(mimeType)
    const testImage = await umbracoApi.media.saveFile(mediaItem, mediaFile)
    // Finds the image path so we are able to locate where the image is located in the wwwroot
    const imagePath = testImage.mediaLink.replace('/media/', '').replace('/' + fileName, '');

    const stylesheet = new StylesheetBuilder()
      .withVirtualPath("/css/")
      .withFileType("stylesheets")
      .withName(stylesheetName)
      .build();
    await umbracoApi.stylesheets.save(stylesheet);

    const element = await createElement(umbracoApi, elementName);
    await createElement(umbracoApi, elementNameTwo);

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
    // Changes label
    await page.locator('[name="label"]').fill('test');
    // Adds a stylesheet
    await page.locator('[key="blockEditor_addCustomStylesheet"]').click();
    await page.locator('[data-element="tree-item-wwwroot"]').locator('[data-element="tree-item-expand"]').click();
    await page.locator('[data-element="tree-item-css"]').locator('[data-element="tree-item-expand"]').click();
    await page.locator('.umb-tree-item__label', {hasText: stylesheetName + '.css'}).click();
    // Changes editor size
    await page.locator('[id="editorSize"]').selectOption({value: "large"});
    // Adds element type as settings model
    await page.locator('[key="blockEditor_addSettingsElementType"]').click();
    await page.locator('[data-element="tree-item-' + elementNameTwo + '"]').click();
    // Changes background color
    await page.locator('.sp-replacer >> nth=0').click();
    await page.locator('[title="#f44336"] >> nth=0').click();
    await page.locator('.sp-choose >> nth=0').click();
    // Changes icon color
    await page.locator('.sp-replacer >> nth=1').click();
    await page.locator('[title="#c90076"] >> nth=1').click();
    await page.locator('.sp-choose >> nth=1').click();
    // Adds a thumbnail
    await page.locator('[key="blockEditor_addThumbnail"]').click();
    await page.locator('[data-element="tree-item-wwwroot"]').locator('[data-element="tree-item-expand"]').click();
    await page.locator('[data-element="tree-item-media"]').locator('[data-element="tree-item-expand"]').click();
    await page.locator('[data-element="tree-item-' + imagePath + '"]').locator('[data-element="tree-item-expand"]').click();
    await page.locator('.umb-tree-item__label', {hasText: fileName}).click();

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
    await umbracoApi.documentTypes.ensureNameNotExists(elementName);
    await umbracoApi.documentTypes.ensureNameNotExists(elementNameTwo);
    await umbracoApi.dataTypes.ensureNameNotExists(blockListName);
    await umbracoApi.stylesheets.ensureNameNotExists(stylesheetName + '.css');
    await umbracoApi.media.ensureNameNotExists(imageName);
  });

  test('can delete a block list editor', async ({page, umbracoApi, umbracoUi}) => {
    const elementNameTwo = 'AnotherTestElement';

    await umbracoApi.documentTypes.ensureNameNotExists(elementName);
    await umbracoApi.documentTypes.ensureNameNotExists(elementNameTwo);
    await umbracoApi.dataTypes.ensureNameNotExists(blockListName);

    const element = await createElement(umbracoApi, elementName);
    const elementTwo = await createElement(umbracoApi, elementNameTwo);

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
    await umbracoApi.documentTypes.ensureNameNotExists(elementName);
    await umbracoApi.documentTypes.ensureNameNotExists(elementNameTwo);
  });
});