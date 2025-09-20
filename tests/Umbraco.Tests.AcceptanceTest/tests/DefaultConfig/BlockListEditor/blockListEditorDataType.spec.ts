import {AliasHelper, ConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";
import {MediaBuilder, MediaFileBuilder, StylesheetBuilder} from "@umbraco/json-models-builders";
import {BlockListDataTypeBuilder} from "@umbraco/json-models-builders/dist/lib/builders/dataTypes";

test.describe('BlockListEditorDataType', () => {
  const blockListName = 'BlockListTest';
  const elementName = 'TestElement';

  const elementAlias = AliasHelper.toAlias(elementName);

  test.beforeEach(async ({page, umbracoApi, umbracoUi}, testInfo) => {
    await umbracoApi.report.report(testInfo);
    await umbracoApi.login();
    await umbracoApi.dataTypes.ensureNameNotExists(blockListName);
    await umbracoApi.documentTypes.ensureNameNotExists(elementName);
  });

  test.afterEach(async({page, umbracoApi, umbracoUi}) => {
    await umbracoApi.dataTypes.ensureNameNotExists(blockListName);
    await umbracoApi.documentTypes.ensureNameNotExists(elementName);
  })

  test('can create an empty block list datatype', async ({page, umbracoApi, umbracoUi}) => {
    await umbracoUi.goToSection(ConstantHelper.sections.settings);

    // Creates a new datatype
    await umbracoUi.clickDataElementByElementName('tree-item-dataTypes', {button: 'right'});
    await umbracoUi.clickDataElementByElementName(ConstantHelper.actions.create);
    await umbracoUi.clickDataElementByElementName(ConstantHelper.actions.dataType);

    await umbracoUi.setEditorHeaderName(blockListName);

    // Adds BlockList as property editor
    await umbracoUi.clickDataElementByElementName('property-editor-add');
    await umbracoUi.clickDataElementByElementName('propertyeditor-', {hasText: 'Block List'});

    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.save));

    // Assert
    await expect(page.locator('.umb-notifications__notifications > .alert-success', {hasText: "Datatype saved"})).toBeVisible();
  });

  test('can create a block list datatype with an element', async ({page, umbracoApi, umbracoUi}) => {
    await umbracoApi.documentTypes.createDefaultElementType(elementName, elementAlias);

    const blockListType = new BlockListDataTypeBuilder()
      .withName(blockListName)
      .build();
    await umbracoApi.dataTypes.save(blockListType);

    await umbracoUi.navigateToDataType(blockListName);

    // Adds an element to the block list
    await umbracoUi.clickElement(umbracoUi.getButtonByKey('blockEditor_addBlockType'));
    await page.locator('[data-element="editor-container"]').locator('[data-element="tree-item-' + elementName + '"]').click();
    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.submitChanges));
    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.save));

    // Assert
    await expect(page.locator('.umb-notifications__notifications > .alert-success', {hasText: "Datatype saved"})).toBeVisible();
    // Checks if the element is added
    await expect(page.locator('.umb-block-card-grid', {hasText: elementName})).toBeVisible();
  });

  test('can create block list datatype with two elements', async ({page, umbracoApi, umbracoUi}) => {
    const elementNameTwo = 'SecondElement';
    const elementTwoAlias = AliasHelper.toAlias(elementNameTwo);

    await umbracoApi.documentTypes.ensureNameNotExists(elementNameTwo);

    const element = await umbracoApi.documentTypes.createDefaultElementType(elementName, elementAlias);
    await umbracoApi.documentTypes.createDefaultElementType(elementNameTwo, elementTwoAlias);

    const blockListType = new BlockListDataTypeBuilder()
      .withName(blockListName)
      .addBlock()
        .withContentElementTypeKey(element['key'])
      .done()
      .build();
    await umbracoApi.dataTypes.save(blockListType);

    await umbracoUi.navigateToDataType(blockListName);

    // Adds an element to the block list
    await umbracoUi.clickElement(umbracoUi.getButtonByKey('blockEditor_addBlockType'));
    await page.locator('[data-element="editor-container"]').locator('[data-element="tree-item-' + elementNameTwo + '"]').click();
    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.submitChanges));
    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.save));

    // Assert
    await expect(page.locator('.umb-notifications__notifications > .alert-success', {hasText: "Datatype saved"})).toBeVisible();
    // Checks if the elements are added
    await expect(page.locator('umb-block-card', {hasText: elementName})).toBeVisible();
    await expect(page.locator('umb-block-card', {hasText: elementNameTwo})).toBeVisible();

    // Clean
    await umbracoApi.documentTypes.ensureNameNotExists(elementNameTwo);
  });

  test('can remove an element in a block list datatype', async ({page, umbracoApi, umbracoUi}) => {
    const element = await umbracoApi.documentTypes.createDefaultElementType(elementName, elementAlias);

    const blockListType = new BlockListDataTypeBuilder()
      .withName(blockListName)
      .addBlock()
        .withContentElementTypeKey(element['key'])
      .done()
      .build();
    await umbracoApi.dataTypes.save(blockListType);

    await umbracoUi.navigateToDataType(blockListName);

    // Deletes the element in the block list
    await umbracoUi.clickElement(umbracoUi.getButtonByKey(ConstantHelper.buttons.delete));

    // Can't use our constant helper because the action for delete does not contain an s. The correct way is 'action-delete'
    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey('actions_delete'));
    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.save));

    // Assert
    await expect(page.locator('.umb-notifications__notifications > .alert-success', {hasText: "Datatype saved"})).toBeVisible();
    // Checks if the element is deleted
    await expect(page.locator('.umb-block-card-grid', {hasText: elementName})).not.toBeVisible();
  });

  test('cant put the same element in a block list editor', async ({page, umbracoApi, umbracoUi}) => {
    const element = await umbracoApi.documentTypes.createDefaultElementType(elementName, elementAlias);

    const blockListType = new BlockListDataTypeBuilder()
      .withName(blockListName)
      .addBlock()
        .withContentElementTypeKey(element['key'])
      .done()
      .build();
    await umbracoApi.dataTypes.save(blockListType);

    await umbracoUi.navigateToDataType(blockListName);

    // Tries adding the same element to the block list editor
    await umbracoUi.clickElement(umbracoUi.getButtonByKey('blockEditor_addBlockType'));
    await page.locator('[data-element="editor-container"]').locator('[data-element="tree-item-' + elementName + '"]').click();

    // Assert
    await expect(page.locator('.not-allowed', {hasText: elementName})).toBeVisible();
    // Checks if the button create New Element Type is still visible. If visible the element was not clickable.
    await expect(page.locator('[label-key="blockEditor_labelcreateNewElementType"]')).toBeVisible();
    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.close));
    await expect(page.locator('[block-config-model="block"]')).toHaveCount(1);
  });

  test('can edit a block list editor', async ({page, umbracoApi, umbracoUi}, testInfo) => {
    // We need to increase the timeout because the test is taking too long to end
    await testInfo.slow()

    const elementNameTwo = 'SecondElement';
    const elementTwoAlias = AliasHelper.toAlias(elementNameTwo);
    const stylesheetName = 'TestStyleSheet';

    const imageName = "Umbraco";
    const umbracoFileValue = {"src": "Umbraco.png"};
    const fileName = "Umbraco.png";
    const path = 'mediaLibrary/' + fileName;
    const mimeType = "image/png";

    await umbracoApi.documentTypes.ensureNameNotExists(elementNameTwo);
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
    const imagePath = testImage.mediaLink.replace('/media/', '').replace('/umbraco.png', '');
    const stylesheet = new StylesheetBuilder()
      .withVirtualPath("/css/")
      .withFileType("stylesheets")
      .withName(stylesheetName)
      .build();
    await umbracoApi.stylesheets.save(stylesheet);

    const element = await umbracoApi.documentTypes.createDefaultElementType(elementName, elementAlias);
    await umbracoApi.documentTypes.createDefaultElementType(elementNameTwo, elementTwoAlias);

    const blockListType = new BlockListDataTypeBuilder()
      .withName(blockListName)
      .addBlock()
        .withContentElementTypeKey(element['key'])
      .done()
      .build();
    await umbracoApi.dataTypes.save(blockListType);

    await umbracoUi.navigateToDataType(blockListName);

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
    await page.locator('[id="blockEditorSize"]').selectOption({value: "large"});
    // Adds element type as settings model
    await page.locator('[key="blockEditor_addSettingsElementType"]').click();
    await umbracoUi.clickDataElementByElementName('tree-item-' + elementNameTwo);
    // Changes background color
    await page.locator('.umb-el-wrap', {hasText: 'Background color'}).locator('.sp-replacer').click();
    await page.locator('[title="#f44336"] >> nth=0').click();
    await page.locator('.sp-choose >> nth=0').click();
    // Changes icon color
    await page.locator('.umb-el-wrap', {hasText: 'Icon color'}).locator('.sp-replacer').click();
    await page.locator('[title="#c90076"] >> nth=1').click();
    await page.locator('.sp-choose >> nth=1').click();
    // Adds a thumbnail
    await page.locator('[key="blockEditor_addThumbnail"]').click();
    await page.locator('[data-element="tree-item-wwwroot"]').locator('[data-element="tree-item-expand"]').click();
    await page.locator('[data-element="tree-item-media"]').locator('[data-element="tree-item-expand"]').click();
    await page.locator('[data-element="tree-item-' + imagePath + '"]').locator('[data-element="tree-item-expand"]').click();
    await page.locator('.umb-tree-item__label', {hasText: fileName}).click();

    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.submitChanges));

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
    await umbracoApi.documentTypes.ensureNameNotExists(elementNameTwo);
    await umbracoApi.stylesheets.ensureNameNotExists(stylesheetName + '.css');
    await umbracoApi.media.ensureNameNotExists(imageName);
  });

  test('can delete a block list editor', async ({page, umbracoApi, umbracoUi}) => {
    const elementNameTwo = 'SecondElement';
    const elementTwoAlias = AliasHelper.toAlias(elementNameTwo);

    await umbracoApi.documentTypes.ensureNameNotExists(elementNameTwo);

    const element = await umbracoApi.documentTypes.createDefaultElementType(elementName, elementAlias);
    const elementTwo = await umbracoApi.documentTypes.createDefaultElementType(elementNameTwo, elementTwoAlias);

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
    await umbracoUi.clickDataElementByElementName('tree-item-' + blockListName, {button: "right"});

    // Deletes the block list editor
    await umbracoUi.clickDataElementByElementName(ConstantHelper.actions.delete);
    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.delete));

    // Assert
    await expect(await umbracoUi.getDataElementByElementName('tree-item-' + blockListName)).not.toBeVisible();

    // Clean
    await umbracoApi.documentTypes.ensureNameNotExists(elementNameTwo);
  });
});
