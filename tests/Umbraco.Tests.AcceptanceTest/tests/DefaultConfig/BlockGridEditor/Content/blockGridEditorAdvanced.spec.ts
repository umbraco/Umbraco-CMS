import {AliasHelper, ConstantHelper, test} from "@umbraco/playwright-testhelpers";
import {BlockGridDataTypeBuilder} from "@umbraco/json-models-builders/dist/lib/builders/dataTypes";
import {ContentBuilder, DocumentTypeBuilder} from "@umbraco/json-models-builders";
import {expect} from "@playwright/test";

test.describe('BlockGridEditorAdvancedContent', () => {
  const documentName = 'DocumentTest';
  const blockGridName = 'BlockGridTest';
  const elementName = 'ElementTitle';

  const documentAlias = AliasHelper.toAlias(documentName);
  const blockGridAlias = AliasHelper.toAlias(blockGridName);
  const elementAlias = AliasHelper.toAlias(elementName);

  test.beforeEach(async ({page, umbracoApi, umbracoUi}, testInfo) => {
    await umbracoApi.report.report(testInfo);
    await umbracoApi.login();
    await umbracoApi.documentTypes.ensureNameNotExists(documentName);
    await umbracoApi.documentTypes.ensureNameNotExists(elementName);
    await umbracoApi.dataTypes.ensureNameNotExists(blockGridName);
  });

  test.afterEach(async ({page, umbracoApi, umbracoUi}) => {
    await umbracoApi.documentTypes.ensureNameNotExists(documentName);
    await umbracoApi.documentTypes.ensureNameNotExists(elementName);
    await umbracoApi.dataTypes.ensureNameNotExists(blockGridName);
  });

  test.describe('Advanced', () => {
    test('can see custom view in content for a block grid editor', async ({page, umbracoApi, umbracoUi}) => {
      // CustomView
      const customViewItemName = "Image";
      const customViewFileName = "Image.html";
      const customViewPath = 'customViews/' + customViewFileName;
      const customViewMimeType = "text/html";
      // Image
      const imageName = "Umbraco";
      const umbracoFileValue = {"src": "Umbraco.png"};
      const imageFileName = "Umbraco.png";
      const imagePath = 'mediaLibrary/' + imageFileName;
      const imageMimeType = "image/png";

      await umbracoApi.media.ensureNameNotExists(imageName);
      await umbracoApi.media.ensureNameNotExists(customViewItemName);

      const imageData = await umbracoApi.media.createImageWithFile(imageName, umbracoFileValue, imageFileName, imagePath, imageMimeType);

      const customViewData = await umbracoApi.media.createFileWithFile(customViewItemName, customViewFileName, customViewPath, customViewMimeType);
      const customViewMediaPath = customViewData.mediaLink;

      const element = new DocumentTypeBuilder()
        .withName(elementName)
        .withAlias(elementAlias)
        .AsElementType()
        .addGroup()
          .withName('ImageGroup')
          .withAlias('imageGroup')
          .addImagePickerProperty()
            .withLabel('Image')
            .withAlias('image')
          .done()
        .done()
        .build();
      await umbracoApi.documentTypes.save(element);

      const dataTypeBlockGrid = new BlockGridDataTypeBuilder()
        .withName(blockGridName)
        .addBlock()
          .withContentElementTypeKey(element['key'])
          .withView(customViewMediaPath)
        .done()
        .build();
      const dataType = await umbracoApi.dataTypes.save(dataTypeBlockGrid);

      await umbracoApi.documentTypes.createDefaultDocumentWithBlockGridEditor(element, dataType);

      const rootContentNode = new ContentBuilder()
        .withContentTypeAlias(documentAlias)
        .withAction(ConstantHelper.actions.save)
        .addVariant()
          .withName(blockGridName)
          .withSave(true)
          .addProperty()
            .withAlias(blockGridAlias)
            .addBlockGridValue()
              .addBlockGridEntry()
                .withContentTypeKey(element['key'])
                .addImage()
                  .withMediaKey(imageData.key)
                .done()
              .done()
              .addLayout()
                .withContentUdi(element['key'])
              .done()
            .done()
          .done()
        .done()
        .build();
      await umbracoApi.content.save(rootContentNode);

      await umbracoUi.navigateToContent(blockGridName);

      // Assert
      // Checks if the block has the correct CustomView
      await expect(page.locator('[data-content-element-type-key="' + element['key'] + '"]').locator('[view="' + customViewMediaPath + '"]')).toBeVisible();
      // Checks if the custom view updated the block by locating a name in the customView
      await expect(page.locator('[data-content-element-type-key="' + element['key'] + '"]').locator('[view="' + customViewMediaPath + '"]').locator('[name="BlockGridCustomView"]')).toBeVisible();
      // Checks if the block is clickable
      await page.locator('[data-content-element-type-key="' + element['key'] + '"]').click();

      // Clean
      await umbracoApi.media.ensureNameNotExists(customViewItemName);
      await umbracoApi.media.ensureNameNotExists(imageName);
    });

    test('can see custom stylesheet in content for a block grid editor', async ({page, umbracoApi, umbracoUi}) => {
      const stylesheetName = "StylesheetBlockGrid.css";
      const stylesheetPath = "stylesheet/" + stylesheetName;
      const stylesheetMimeType = "text/css";

      await umbracoApi.media.ensureNameNotExists(stylesheetName);

      const stylesheetData = await umbracoApi.media.createFileWithFile(stylesheetName, stylesheetName, stylesheetPath, stylesheetMimeType);
      const stylesheetDataPath = stylesheetData.mediaLink;

      const element = await umbracoApi.documentTypes.createDefaultElementType(elementName, elementAlias);

      const dataTypeBlockGrid = new BlockGridDataTypeBuilder()
        .withName(blockGridName)
        .addBlock()
          .withContentElementTypeKey(element['key'])
          .withStylesheet(stylesheetDataPath)
        .done()
        .build();
      const dataType = await umbracoApi.dataTypes.save(dataTypeBlockGrid);

      await umbracoApi.content.createDefaultContentWithABlockGridEditor(element, dataType, false);

      await umbracoUi.navigateToContent(blockGridName);

      // Assert
      // Checks if the block has the correct template
      await expect(page.locator('umb-block-grid-entry', {hasText: elementName}).locator('umb-block-grid-block')).toHaveAttribute('stylesheet', stylesheetDataPath);

      // Clean
      await umbracoApi.media.ensureNameNotExists(stylesheetName);
    });

    test('can see changed overlay editor size in content for a block grid editor', async ({page, umbracoApi, umbracoUi}) => {
      const editorSize = 'large';

      const element = await umbracoApi.documentTypes.createDefaultElementType(elementName, elementAlias);

      const dataTypeBlockGrid = new BlockGridDataTypeBuilder()
        .withName(blockGridName)
        .addBlock()
          .withContentElementTypeKey(element['key'])
          .withEditorSize(editorSize)
        .done()
        .build();
      const dataType = await umbracoApi.dataTypes.save(dataTypeBlockGrid);

      await umbracoApi.content.createDefaultContentWithABlockGridEditor(element, dataType, false);

      await umbracoUi.navigateToContent(blockGridName);

      // Opens the content editor
      await page.locator('[data-content-element-type-key="' + element['key'] + '"]').click();

      // Assert
      // Checks if the editor contains the correct editor size
      await expect(page.locator('.umb-editor--' + editorSize).locator('[id="sub-view-0"]')).toBeVisible();
    });

    test('can use inline editing mode in content for a block grid editor', async ({page, umbracoApi, umbracoUi}) => {
      const newText = 'UpdatedText';

      const element = await umbracoApi.documentTypes.createDefaultElementType(elementName, elementAlias);

      const dataTypeBlockGrid = new BlockGridDataTypeBuilder()
        .withName(blockGridName)
        .addBlock()
          .withContentElementTypeKey(element['key'])
          .withInlineEditing(true)
        .done()
        .build();
      const dataType = await umbracoApi.dataTypes.save(dataTypeBlockGrid);

      await umbracoApi.content.createDefaultContentWithABlockGridEditor(element, dataType, false);

      await umbracoUi.navigateToContent(blockGridName);

      // Updates the block text by using the inline editing mode
      await page.locator('[data-element="property-' + blockGridAlias + '"]').locator('[id="title"]').click();
      await page.locator('[data-element="property-' + blockGridAlias + '"]').locator('[id="title"]').fill(newText);
      await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.saveAndPublish));

      // Assert
      await umbracoUi.isSuccessNotificationVisible();
      // Checks if the value in the block was updated by using the inline editing mode
      await expect(page.locator('[data-element="property-' + blockGridAlias + '"]').locator('[id="title"]:visible')).toHaveValue(newText);
    });

    test('can hide content editor', async ({page, umbracoApi, umbracoUi}) => {
      const element = await umbracoApi.documentTypes.createDefaultElementType(elementName, elementAlias);

      const dataTypeBlockGrid = new BlockGridDataTypeBuilder()
        .withName(blockGridName)
        .addBlock()
          .withContentElementTypeKey(element['key'])
          .withForceHideContentEditorInOverlay(true)
        .done()
        .build();
      const dataType = await umbracoApi.dataTypes.save(dataTypeBlockGrid);

      await umbracoApi.content.createDefaultContentWithABlockGridEditor(element, dataType, false);

      await umbracoUi.navigateToContent(blockGridName);

      // Click the block
      await page.locator('[data-content-element-type-key="' + element['key'] + '"]', {hasText: elementName}).click();

      // Assert
      // Checks if the block is still clickable, if it's clickable that means the the content editor is hidden
      await page.locator('[data-content-element-type-key="' + element['key'] + '"]', {hasText: elementName}).click();
    });
  });

  test.describe('Catalogue appearance', () => {
    test('can see background color in content for a block grid editor', async ({page, umbracoApi, umbracoUi}) => {
      const elementTwoName = 'TheSecondElement';
      const elementTwoAlias = AliasHelper.toAlias(elementTwoName);

      const backgroundColor = 'rgb(244, 67, 54)';

      await umbracoApi.documentTypes.ensureNameNotExists(elementTwoName);

      const element = await umbracoApi.documentTypes.createDefaultElementType(elementName, elementAlias);
      const elementTwo = await umbracoApi.documentTypes.createDefaultElementType(elementTwoName, elementTwoAlias);

      const dataTypeBlockGrid = new BlockGridDataTypeBuilder()
        .withName(blockGridName)
        .addBlock()
          .withContentElementTypeKey(element['key'])
          .withBackgroundColor(backgroundColor)
        .done()
        .addBlock()
          .withContentElementTypeKey(elementTwo['key'])
        .done()
        .build();
      const dataType = await umbracoApi.dataTypes.save(dataTypeBlockGrid);

      await umbracoApi.content.createDefaultContentWithABlockGridEditor(element, dataType, false);

      await umbracoUi.navigateToContent(blockGridName);

      // Opens the content editor
      await page.locator('[data-element="property-' + blockGridAlias + '"]').getByRole('button', {name: 'Add content'}).click();

      // Assert
      // Checks if the element has the set background color
      await expect(page.locator('umb-block-card', {hasText: elementName}).locator('[style="background-color: ' + backgroundColor + '; background-image: none;"]')).toBeVisible();

      // Clean
      await umbracoApi.documentTypes.ensureNameNotExists(elementTwoName);
    });

    test('can see icon color in content for a block grid editor', async ({page, umbracoApi, umbracoUi}) => {
      const elementTwoName = 'TheSecondElement';
      const elementTwoAlias = AliasHelper.toAlias(elementTwoName);

      const iconColor = 'rgb(25, 92, 201)';

      await umbracoApi.documentTypes.ensureNameNotExists(elementTwoName);

      const element = await umbracoApi.documentTypes.createDefaultElementType(elementName, elementAlias);
      const elementTwo = await umbracoApi.documentTypes.createDefaultElementType(elementTwoName, elementTwoAlias);

      const dataTypeBlockGrid = new BlockGridDataTypeBuilder()
        .withName(blockGridName)
        .addBlock()
          .withContentElementTypeKey(element['key'])
          .withIconColor(iconColor)
        .done()
        .addBlock()
          .withContentElementTypeKey(elementTwo['key'])
        .done()
        .build();
      const dataType = await umbracoApi.dataTypes.save(dataTypeBlockGrid);

      await umbracoApi.content.createDefaultContentWithABlockGridEditor(element, dataType, false);

      await umbracoUi.navigateToContent(blockGridName);

      // Opens the content editor
      await page.locator('[data-element="property-' + blockGridAlias + '"]').getByRole('button', {name: 'Add content'}).click();

      // Assert
      // Checks if the element has the set icon color
      await expect(page.locator('umb-block-card', {hasText: elementName}).locator('.umb-icon')).toHaveAttribute('style', 'color:' + iconColor);

      // Clean
      await umbracoApi.documentTypes.ensureNameNotExists(elementTwoName);
    });

    test('can see thumbnail in content for a block grid editor', async ({page, umbracoApi, umbracoUi}) => {
      // Thumbnail
      const imageName = "Umbraco";
      const umbracoFileValue = {"src": "Umbraco.png"};
      const imageFileName = "Umbraco.png";
      const imagePath = 'mediaLibrary/' + imageFileName;
      const imageMimeType = "image/png";

      // ElementTwo
      const elementTwoName = 'TheSecondElement';
      const elementTwoAlias = AliasHelper.toAlias(elementTwoName);

      await umbracoApi.media.ensureNameNotExists(imageName);
      await umbracoApi.documentTypes.ensureNameNotExists(elementTwoName);

      const imageData = await umbracoApi.media.createImageWithFile(imageName, umbracoFileValue, imageFileName, imagePath, imageMimeType);
      const imageDataPath = imageData.mediaLink;

      const element = await umbracoApi.documentTypes.createDefaultElementType(elementName, elementAlias);
      const elementTwo = await umbracoApi.documentTypes.createDefaultElementType(elementTwoName, elementTwoAlias);

      const dataTypeBlockGrid = new BlockGridDataTypeBuilder()
        .withName(blockGridName)
        .addBlock()
          .withContentElementTypeKey(element['key'])
          .withThumbnail(imageDataPath)
        .done()
        .addBlock()
          .withContentElementTypeKey(elementTwo['key'])
        .done()
        .build();
      const dataType = await umbracoApi.dataTypes.save(dataTypeBlockGrid);

      await umbracoApi.content.createDefaultContentWithABlockGridEditor(element, dataType, false);

      await umbracoUi.navigateToContent(blockGridName);

      // Opens the content editor
      await page.locator('[data-element="property-' + blockGridAlias + '"]').getByRole('button', {name: 'Add content'}).click();

      // Assert
      // Checks if the element has the thumbnail
      const updatedImagePath = encodeURIComponent(imageDataPath);
      await expect(page.locator('umb-block-card', {hasText: elementName}).locator('.__showcase')).toHaveAttribute('style', 'background-image: url("/umbraco/backoffice/umbracoapi/images/GetBigThumbnail?originalImagePath=' + updatedImagePath + '\");');

      // Clean
      await umbracoApi.documentTypes.ensureNameNotExists(elementTwoName);
      await umbracoApi.media.ensureNameNotExists(imageName);
    });
  });
});
