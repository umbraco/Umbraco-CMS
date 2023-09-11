import {AliasHelper, ConstantHelper, test} from "@umbraco/playwright-testhelpers";
import {ContentBuilder, DocumentTypeBuilder, MediaBuilder, MediaFileBuilder} from "@umbraco/json-models-builders";
import {BlockGridDataTypeBuilder} from "@umbraco/json-models-builders/dist/lib/builders/dataTypes";
import {expect} from "@playwright/test";

test.describe('BlockGridEditorContent', () => {
  const documentName = 'DocumentTest';
  const blockGridName = 'BlockGridTest';
  const elementName = 'ElementTest';

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

  test('can create content with a block grid editor', async ({page, umbracoApi, umbracoUi}) => {
    await umbracoApi.documentTypes.createDefaultDocumentWithBlockGridEditor(null, null);

    const rootContentNode = new ContentBuilder()
      .withContentTypeAlias(documentAlias)
      .withAction(ConstantHelper.actions.save)
      .addVariant()
        .withName(blockGridName)
        .withSave(true)
      .done()
      .build();
    await umbracoApi.content.save(rootContentNode);

    await umbracoUi.navigateToContent(blockGridName);

    // Adds TestElement
    await page.locator('[key="blockEditor_addThis"]', {hasText: elementName}).click();
    await page.locator('[id="sub-view-0"]').locator('[id="title"]').fill('Hi there!');
    await page.locator('[label="Create"]').click();
    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.saveAndPublish));

    // Assert
    await umbracoUi.isSuccessNotificationVisible();
    // Checks if the content was created
    await expect(page.locator('.umb-block-grid__block--view')).toHaveCount(1);
    await expect(page.locator('.umb-block-grid__block--view').first()).toHaveText(elementName);
  });

  test('can update content with a block grid editor', async ({page, umbracoApi, umbracoUi}) => {
    const newContentValue = 'UpdatedTitle';
    const newSettingValue = 'UpdatedSetting';

    const element = await umbracoApi.content.createDefaultContentWithABlockGridEditor(null, null, null);

    await umbracoUi.navigateToContent(blockGridName);

    // Updates the already created content text
    await page.locator('[data-content-element-type-key="' + element['key'] + '"]', {hasText: elementName}).click();
    await page.locator('[id="sub-view-0"]').locator('[id="title"]').fill(newContentValue);
    await umbracoUi.clickDataElementByElementName('sub-view-settings');
    // Adds text to the setting element
    await page.locator('[id="sub-view-1"]').locator('[id="title"]').fill(newSettingValue);
    await page.locator('[label="Submit"]').click();
    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.saveAndPublish));

    // Assert
    await umbracoUi.isSuccessNotificationVisible();
    // Checks if the Content and Setting were updated after it was saved
    await page.locator('[data-content-element-type-key="' + element['key'] + '"]', {hasText: elementName}).click();
    await expect(page.locator('[id="sub-view-0"]').locator('[id="title"]')).toHaveValue(newContentValue);
    await umbracoUi.clickDataElementByElementName('sub-view-settings');
    await expect(page.locator('[id="sub-view-1"]').locator('[id="title"]')).toHaveValue(newSettingValue);
  });

  test('can delete a block grid editor in content', async ({page, umbracoApi, umbracoUi}) => {
    const element = await umbracoApi.content.createDefaultContentWithABlockGridEditor(null, null, null);

    await umbracoUi.navigateToContent(blockGridName);

    // Deletes the block grid editor inside of the content
    await page.getByTitle("Delete").click();

    // Can't use our constant helper because the action for delete does not contain an s. The correct way is 'action-delete'
    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey('actions_delete'));

    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.saveAndPublish));

    // Assert
    await umbracoUi.isSuccessNotificationVisible();
    // Checks if the content is actually deleted
    await expect(page.locator('[data-content-element-type-key="' + element['key'] + '"]', {hasText: elementName})).not.toBeVisible();
    await expect(page.locator('.umb-block-grid__block--view')).toHaveCount(0);
  });

  test('can copy block grid content and paste it', async ({page, umbracoApi, umbracoUi}) => {
    await umbracoApi.content.createDefaultContentWithABlockGridEditor(null, null, null);

    await umbracoUi.navigateToContent(blockGridName);
    // Checks to make sure that there is only one item
    await expect(page.locator('.umb-block-grid__block--view')).toHaveCount(1);

    // Copies the block grid content
    await page.getByTitle("Copy").click();

    await expect(page.locator('.alert-success', {hasText: 'Copied to clipboard'})).toBeVisible();
    // Pastes block grid content
    await page.getByTitle("Clipboard").click();

    await page.locator('umb-block-card', {hasText: elementName}).click();
    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.saveAndPublish));

    // Assert
    // Checks if there now are two blocks
    await expect(page.locator('.umb-block-grid__block')).toHaveCount(2);
    // Checks if the text was copied to the pasted block
    await page.locator('.umb-block-grid__block--view').nth(1).click();
    await expect(page.locator('[id="sub-view-0"] >> [name="textbox"]')).toHaveValue('aliasTest');
  });

  test('can copy block grid content and paste it into another group with the same block grid editor', async ({page, umbracoApi, umbracoUi}) => {
    const blockGridTwoName = 'BlockGridTwo';
    const blockGridTwoAlias = AliasHelper.toAlias(blockGridTwoName);

    const groupOne = 'BlockGridGroupOne';
    const groupTwo = 'BlockGridGroupTwo';

    const element = await umbracoApi.documentTypes.createDefaultElementType(elementName, elementAlias);

    const dataType = await umbracoApi.dataTypes.createDefaultBlockGrid(blockGridName, element);

    const docType = new DocumentTypeBuilder()
      .withName(documentName)
      .withAlias(documentAlias)
      .withAllowAsRoot(true)
      .addGroup()
        .withName(groupOne)
        .addCustomProperty(dataType['id'])
          .withLabel(blockGridName)
          .withAlias(blockGridAlias)
        .done()
      .done()
      .addGroup()
        .withName(groupTwo)
        .addCustomProperty(dataType['id'])
          .withLabel(blockGridTwoName)
          .withAlias(blockGridTwoAlias)
        .done()
      .done()
      .build();
    await umbracoApi.documentTypes.save(docType);

    await umbracoApi.content.createDefaultContentWithABlockGridEditor(element, dataType, true);

    await umbracoUi.navigateToContent(blockGridName);

    // Checks to make sure that there is only one item in the first group
    await expect(page.locator('[data-element="group-a' + groupOne + '"] >> .umb-block-grid__block--view')).toHaveCount(1);
    // Checks to make sure that there are no items in the second group
    await expect(page.locator('[data-element="group-a' + groupTwo + '"] >> .umb-block-grid__block--view')).toHaveCount(0);

    // Copies block grid content from the first group
    await page.locator('[title="Copy"]').click();
    await expect(page.locator('.alert-success', {hasText: 'Copied to clipboard'})).toBeVisible();
    // Pastes into the second group
    await page.locator('[title="Clipboard"]').nth(1).click();
    await page.locator('umb-block-card', {hasText: elementName}).click();
    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.saveAndPublish));
    await expect(page.locator('.alert-success', {hasText: 'Content Published'})).toBeVisible();

    // Assert
    // Checks if both groups has one item each
    await expect(page.locator('[data-element="group-a' + groupOne + '"] >> .umb-block-grid__block--view')).toHaveCount(1);
    await expect(page.locator('[data-element="group-a' + groupTwo + '"] >> .umb-block-grid__block--view')).toHaveCount(1);
    // Opens the second group to see if the block grid item was copied with the correct text
    await page.locator('[data-element="group-a' + groupTwo + '"] >> .umb-block-grid__block--view').click();
    await expect(page.locator('[id="sub-view-0"] >> [name="textbox"]')).toHaveValue('aliasTest');
  });

  test.describe('Moving blocks', () => {
    test('can move a block under another block', async ({page, umbracoApi, umbracoUi}) => {
      const bottomBlock = "BottomBlock";
      const topBlock = "TopBlock";

      const element = await umbracoApi.documentTypes.createDefaultElementType(elementName, elementAlias);

      // We give the dataType a label so we can differentiate between the top and bottom block in the content editor.
      const dataType = await umbracoApi.dataTypes.createDefaultBlockGrid(blockGridName, element, '{{' + element.groups[0].properties[0].alias + '}}');

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
                .appendContentProperties(element.groups[0].properties[0].alias, bottomBlock)
              .done()
              .addBlockGridEntry()
                .withContentTypeKey(element['key'])
                .appendContentProperties(element.groups[0].properties[0].alias, topBlock)
              .done()
              .addLayout()
                .withContentUdi(element['key'], 0)
              .done()
              .addLayout()
                .withContentUdi(element['key'], 1)
              .done()
            .done()
          .done()
        .done()
        .build();
      await umbracoApi.content.save(rootContentNode);

      await umbracoUi.navigateToContent(blockGridName);

      // Drag and Drop
      const dragFromLocator = await page.locator('[data-content-element-type-key="' + element['key'] + '"]', {hasText: bottomBlock});
      const dragToLocator = await page.locator('[data-content-element-type-key="' + element['key'] + '"]', {hasText: topBlock});
      await umbracoUi.dragAndDrop(dragFromLocator, dragToLocator, 10, -5, 15);

      // Assert
      // Checks if the BottomBlock is moved to be under TopBlock
      await expect(page.locator('[data-content-element-type-key="' + element['key'] + '"]').nth(1)).toContainText(bottomBlock);
    });

    test('can move a block next to another block', async ({page, umbracoApi, umbracoUi}) => {
      const leftBlock = "LeftBlock";
      const rightBlock = "RightBlock";

      const element = await umbracoApi.documentTypes.createDefaultElementType(elementName, elementAlias);

      const dataTypeBlockGrid = new BlockGridDataTypeBuilder()
        .withName(blockGridName)
        .addBlock()
          .withLabel('{{' + element.groups[0].properties[0].alias +'}}')
          .withContentElementTypeKey(element['key'])
          .addColumnSpanOptions(6)
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
                .appendContentProperties(element.groups[0].properties[0].alias, rightBlock)
              .done()
              .addBlockGridEntry()
                .withContentTypeKey(element['key'])
                .appendContentProperties(element.groups[0].properties[0].alias, leftBlock)
              .done()
              .addLayout()
                .withContentUdi(element['key'], 0)
              .done()
              .addLayout()
                .withContentUdi(element['key'], 1)
              .done()
            .done()
          .done()
        .done()
        .build();
      await umbracoApi.content.save(rootContentNode);

      await umbracoUi.navigateToContent(blockGridName);

      // Drag and Drop
      const dragFromLocator = await page.locator('[data-content-element-type-key="' + element['key'] + '"]', {hasText: rightBlock});
      const dragToLocator = await page.locator('[data-content-element-type-key="' + element['key'] + '"]', {hasText: leftBlock});
      await umbracoUi.dragAndDrop(dragFromLocator, dragToLocator, -5, 20, 15);

      // Assert
      // Checks if the rightBlock is moved to the right side of the blocks
      await expect(page.locator('[data-content-element-type-key="' + element['key'] + '"]').nth(1)).toContainText(rightBlock);
    });
  });

  test('can create content with a image', async ({page, umbracoApi, umbracoUi}) => {
    const imageName = "Umbraco";

    await umbracoApi.media.ensureNameNotExists(imageName);

    const element = new DocumentTypeBuilder()
      .withName(elementName)
      .withAlias(elementAlias)
      .AsElementType()
      .addGroup()
        .withName("ImageGroup")
        .withAlias('imageGroup')
        .addImagePickerProperty()
          .withLabel("Image")
          .withAlias("image")
        .done()
      .done()
      .build();
    await umbracoApi.documentTypes.save(element);

    await umbracoApi.media.createDefaultImage(imageName);

    await umbracoApi.documentTypes.createDefaultDocumentWithBlockGridEditor(element, null);

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

    // Selects the created image for the block
    await page.locator('[data-content-element-type-key="' + element['key'] + '"]').click();
    await page.getByRole('button', { name: 'Add', exact: true }).click();
    await page.locator('[data-element="media-grid"] >> [title="' + imageName + '"]').click();
    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.select));
    await page.locator('[label="Submit"]').click();
    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.saveAndPublish));

    // Assert
    await umbracoUi.isSuccessNotificationVisible();
    // Checks if the image was added to the block
    await page.locator('[data-content-element-type-key="' + element['key'] + '"]').click();
    await expect(page.locator('.umb-media-card-grid__cell >> [title="' + imageName + '"]')).toBeVisible();
    // Clean
    await umbracoApi.media.ensureNameNotExists(imageName);
  });

  test.describe('Amount', () => {
    test('can set a minimum of required blocks in content with a block grid editor', async ({page, umbracoApi, umbracoUi}) => {
      const element = await umbracoApi.documentTypes.createDefaultElementType(elementName, elementAlias);

      const dataTypeBlockGrid = new BlockGridDataTypeBuilder()
        .withName(blockGridName)
        .addBlock()
          .withContentElementTypeKey(element['key'])
        .done()
        .withMin(2)
        .build();
      const dataType = await umbracoApi.dataTypes.save(dataTypeBlockGrid);

      await umbracoApi.content.createDefaultContentWithABlockGridEditor(element, dataType, false);

      await umbracoUi.navigateToContent(blockGridName);
      // Checks if there is validation for needing 2 entries or more
      await expect(page.locator('[key="validation_entriesShort"]')).toContainText('Minimum 2 entries');
      await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.saveAndPublish));
      // Checks if a validation error is thrown when trying to save when there is not enough blocks
      await expect(page.locator('.alert-error')).toBeVisible();

      // Adds another block
      await page.locator('[key="blockEditor_addThis"]', {hasText: elementName}).click();
      await page.locator('[label="Create"]').click();

      await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.saveAndPublish));

      // Assert
      await umbracoUi.isSuccessNotificationVisible();
      // Checks if there are two blocks in the area
      await expect(page.locator('[data-element="property-' + blockGridAlias + '"]').locator('umb-block-grid-entry')).toHaveCount(2);

    });

    test('can set a maximum of required blocks in content with a block grid editor', async ({page, umbracoApi, umbracoUi}) => {
      const element = await umbracoApi.documentTypes.createDefaultElementType(elementName, elementAlias);

      const dataTypeBlockGrid = new BlockGridDataTypeBuilder()
        .withName(blockGridName)
        .addBlock()
          .withContentElementTypeKey(element['key'])
        .done()
        .withMax(2)
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
                .appendContentProperties(element.groups[0].properties[0].alias, "aliasTest")
              .done()
              .addBlockGridEntry()
                .withContentTypeKey(element['key'])
                .appendContentProperties(element.groups[0].properties[0].alias, "aliasTestes")
              .done()
              .addBlockGridEntry()
                .withContentTypeKey(element['key'])
                .appendContentProperties(element.groups[0].properties[0].alias, "aliasTester")
              .done()
              .addLayout()
                .withContentUdi(element['key'], 0)
              .done()
              .addLayout()
                .withContentUdi(element['key'], 1)
              .done()
              .addLayout()
                .withContentUdi(element['key'], 2)
              .done()
            .done()
          .done()
        .done()
        .build();
      await umbracoApi.content.save(rootContentNode);

      await umbracoUi.navigateToContent(blockGridName);

      // Checks if there is validation for needing 2 entries or less
      await expect(page.locator('[key="validation_entriesExceed"]')).toContainText('Maximum 2 entries');
      await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.saveAndPublish));
      // Checks if a validation error is thrown when trying to save when there is too many blocks
      await expect(page.locator('.alert-error')).toBeVisible();

      // Deletes a block
      await page.locator('[title="Delete"]').nth(2).click();
      await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey('actions_delete'));

      await page.waitForTimeout(2000);
      await page.getByRole('button', { name: 'Save and publish' }).click();

      // Assert
      await umbracoUi.isSuccessNotificationVisible();
      // Checks if there are two blocks in the area
      await expect(page.locator('[data-element="property-' + blockGridAlias + '"]').locator('umb-block-grid-entry')).toHaveCount(2);
    });
  });

  test.describe('Live editing mode', () => {
    test('can use live editing mode in content with a block grid editor', async ({page, umbracoApi, umbracoUi}) => {
      const newText = 'LiveUpdatedContent';

      const element = await umbracoApi.documentTypes.createDefaultElementType(elementName, elementAlias);

      const dataTypeBlockGrid = new BlockGridDataTypeBuilder()
        .withName(blockGridName)
        .addBlock()
          // We use the a label so we can see if the block is live updated when content is being written to the element
          .withLabel('{{' + element.groups[0].properties[0].alias + '}}')
          .withContentElementTypeKey(element['key'])
          .withEditorSize('small')
        .done()
        .withUseLiveEditing(true)
        .build();
      const dataType = await umbracoApi.dataTypes.save(dataTypeBlockGrid);

      await umbracoApi.content.createDefaultContentWithABlockGridEditor(element, dataType, false);

      await umbracoUi.navigateToContent(blockGridName);

      // Checks if the block contains the correct text before being edited
      await expect(page.locator('[data-content-element-type-alias="'+elementAlias +'"]')).toContainText('aliasTest');

      // Updates the text without saving the changes.
      await page.locator('[data-content-element-type-key="' + element['key'] + '"]').click();
      await page.locator('[id="sub-view-0"]').locator('[id="title"]').fill(newText);

      // Checks if the block is being live updated as the content is being updated.
      await expect(page.locator('[data-content-element-type-alias="'+elementAlias +'"]')).toContainText(newText);
    });
  });

  test.describe('Editor width', () => {
    test('can see updated editor width in a block grid editor', async ({page, umbracoApi, umbracoUi}) => {
      const maxWidth = "40%";
      const element = await umbracoApi.documentTypes.createDefaultElementType(elementName, elementAlias);

      const dataTypeBlockGrid = new BlockGridDataTypeBuilder()
        .withName(blockGridName)
        .addBlock()
          .withContentElementTypeKey(element['key'])
        .done()
        .withMaxPropertyWidth(maxWidth)
        .build();
      const dataType = await umbracoApi.dataTypes.save(dataTypeBlockGrid);

      await umbracoApi.content.createDefaultContentWithABlockGridEditor(element, dataType, false);

      await umbracoUi.navigateToContent(blockGridName);

      // Assert
      // Checks if the max-width size is the same as defined in the block grid editor
      await expect(page.locator('[data-element="property-' + blockGridAlias + '"] >> [style="max-width: ' + maxWidth + ';"]')).toBeVisible();
    });
  });

  test.describe('Grid Columns', () => {
    test('can see updated grid columns in content', async ({page, umbracoApi, umbracoUi}) => {
      const gridColumns = 6;

      const element = await umbracoApi.documentTypes.createDefaultElementType(elementName, elementAlias);

      const dataTypeBlockGrid = new BlockGridDataTypeBuilder()
        .withName(blockGridName)
        .addBlock()
          .withContentElementTypeKey(element['key'])
        .done()
        .withGridColumns(gridColumns)
        .build();
      const dataType = await umbracoApi.dataTypes.save(dataTypeBlockGrid);

      await umbracoApi.content.createDefaultContentWithABlockGridEditor(element, dataType, false);

      await umbracoUi.navigateToContent(blockGridName);

      // Assert
      // Checks if the grid columns are the same as defined in the block grid editor
      await expect(page.locator('[data-element="property-' + blockGridAlias + '"] >> [grid-columns="' + gridColumns + '"]')).toBeVisible();
    });
  });

  test.describe('Layout Stylesheet', () => {
    test('can use a layout stylesheet in content', async ({page, umbracoApi, umbracoUi}) => {
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
        .done()
        .withLayoutStylesheet(stylesheetDataPath)
        .build();
      const dataType = await umbracoApi.dataTypes.save(dataTypeBlockGrid);

      await umbracoApi.content.createDefaultContentWithABlockGridEditor(element, dataType, null);

      await umbracoUi.navigateToContent(blockGridName);

      // Assert
      // Checks if the block has the correct template
      await expect(page.locator('[data-element="property-'+blockGridAlias+'"]').locator('umb-block-grid-root')).toHaveAttribute('stylesheet', stylesheetDataPath);

      // Clean
      await umbracoApi.media.ensureNameNotExists(stylesheetName);
    });
  });

  test.describe('Create Button Label', () => {
    test('can see updated create button label in content', async ({page, umbracoApi, umbracoUi}) => {
      const newButtonLabel = 'AddTestButton';

      const element = await umbracoApi.documentTypes.createDefaultElementType(elementName, elementAlias);

      const dataTypeBlockGrid = new BlockGridDataTypeBuilder()
        .withName(blockGridName)
        .addBlock()
          .withContentElementTypeKey(element['key'])
        .done()
        .withCreateLabel(newButtonLabel)
        .build();
      const dataType = await umbracoApi.dataTypes.save(dataTypeBlockGrid);

      await umbracoApi.content.createDefaultContentWithABlockGridEditor(element, dataType, false);

      // Goes to the created Content
      await umbracoUi.goToSection(ConstantHelper.sections.content);
      await umbracoUi.refreshContentTree();
      await umbracoUi.clickDataElementByElementName('tree-item-' + blockGridName);

      // Assert
      // Checks if the new button label is on the BlockGridEditor
      await expect(page.locator('[data-element="property-' + blockGridAlias + '"] >> .umb-block-grid__actions')).toContainText(newButtonLabel);
      // Checks if the button label can be clicked
      await page.locator('[data-element="property-' + blockGridAlias + '"] >> .umb-block-grid__actions', {hasText: newButtonLabel}).click();
    });
  });
});
