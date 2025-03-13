import {AliasHelper, ConstantHelper, test} from "@umbraco/playwright-testhelpers";
import {ContentBuilder} from "@umbraco/json-models-builders";
import {BlockGridDataTypeBuilder} from "@umbraco/json-models-builders/dist/lib/builders/dataTypes";
import {expect} from "@playwright/test";

test.describe('BlockGridEditorAreasContent', () => {
  const documentName = 'DocumentTest';
  const blockGridName = 'BlockGridTest';
  const elementTitleName = 'ElementTitle';
  const titleText = 'ElementTitle';
  const titleArea = 'AreaTitle';
  const elementBodyName = 'ElementBody';
  const bodyText = 'Lorem ipsum dolor sit amet';

  const elementBodyAlias = AliasHelper.toAlias(elementBodyName);
  const documentAlias = AliasHelper.toAlias(documentName);
  const blockGridAlias = AliasHelper.toAlias(blockGridName);
  const elementTitleAlias = AliasHelper.toAlias(elementTitleName);

  async function createContentWithABlockInAnotherBlock(umbracoApi, elementParent, elementChild, dataType?, document?) {

    if (dataType == null) {
      dataType = await umbracoApi.dataTypes.createBlockGridDataTypeWithArea(elementParent, elementChild, blockGridName, titleArea);
    }
    if (document == null) {
      await umbracoApi.documentTypes.createDefaultDocumentWithBlockGridEditor(elementParent, dataType);
    }

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
              .appendContentProperties(elementChild.groups[0].properties[0].alias, bodyText)
              .withContentTypeKey(elementChild['key'])
            .done()
            .addBlockGridEntry()
              .appendContentProperties(elementParent.groups[0].properties[0].alias, titleText)
              .withContentTypeKey(elementParent['key'])
            .done()
            .addLayout()
              .withContentUdi(elementParent['key'])
              .addAreas()
                .withKey(dataType.preValues[0].value[1].areas[0].key)
                .addItems()
                  .withContentUdi(elementChild['key'])
                .done()
              .done()
            .done()
          .done()
        .done()
      .done()
      .build();
   return await umbracoApi.content.save(rootContentNode);
  }

  test.beforeEach(async ({page, umbracoApi, umbracoUi}, testInfo) => {
    await umbracoApi.report.report(testInfo);
    await umbracoApi.login();
    await umbracoApi.documentTypes.ensureNameNotExists(documentName);
    await umbracoApi.documentTypes.ensureNameNotExists(elementTitleName);
    await umbracoApi.dataTypes.ensureNameNotExists(blockGridName);
    await umbracoApi.documentTypes.ensureNameNotExists(elementBodyName);
  });

  test.afterEach(async ({page, umbracoApi, umbracoUi}) => {
    await umbracoApi.documentTypes.ensureNameNotExists(documentName);
    await umbracoApi.documentTypes.ensureNameNotExists(elementTitleName);
    await umbracoApi.dataTypes.ensureNameNotExists(blockGridName);
    await umbracoApi.documentTypes.ensureNameNotExists(elementBodyName);
  });

  test.describe('Areas test', () => {
    test('can add an area from a block grid editor to content', async ({page, umbracoApi, umbracoUi}) => {
      const element = await umbracoApi.documentTypes.createDefaultElementType(elementTitleName, elementTitleAlias);
      const elementBody = await umbracoApi.documentTypes.createDefaultElementType(elementBodyName, elementBodyAlias);

      const dataType = await umbracoApi.dataTypes.createBlockGridDataTypeWithArea(element, elementBody, blockGridName, titleArea);

      await umbracoApi.content.createDefaultContentWithABlockGridEditor(element, dataType, false);

      await umbracoUi.navigateToContent(blockGridName);

      // Adds a body to the area
      await page.locator('[data-content-element-type-key="' + element['key'] + '"] >> [data-area-alias="' + titleArea + '"]').click();
      await page.locator('[name="infiniteEditorForm"]').locator('[data-element="editor-container"]').getByRole('button', {name: elementBodyName}).click();
      await page.locator('[id="sub-view-0"]').locator('[id="title"]').fill(bodyText);
      await page.locator('[label="Create"]').click();
      await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.saveAndPublish));

      // Assert
      await umbracoUi.isSuccessNotificationVisible();
      // Checks if the body element is under the first element
      await expect(page.locator('[data-content-element-type-key="' + element['key'] + '"] >> [data-content-element-type-key="' + elementBody['key'] + '"]')).toBeVisible();
      // Checks if the value inside of body is correct
      await page.locator('[data-content-element-type-key="' + element['key'] + '"] >> [data-content-element-type-key="' + elementBody['key'] + '"]').click();
      await expect(page.locator('[id="sub-view-0"]').locator('[id="title"]')).toHaveValue(bodyText);
    });

    test('can update an area in a block grid editor', async ({page, umbracoApi, umbracoUi}) => {
      const newBodyText = 'Ipsum';

      const element = await umbracoApi.documentTypes.createDefaultElementType(elementTitleName, elementTitleAlias);
      const elementBody = await umbracoApi.documentTypes.createDefaultElementType(elementBodyName, elementBodyAlias);

      await createContentWithABlockInAnotherBlock(umbracoApi, element, elementBody);

      await umbracoUi.navigateToContent(blockGridName);

      // Updates ElementBody with new text
      await page.locator('[data-content-element-type-key="' + element['key'] + '"] >> [data-content-element-type-key="' + elementBody['key'] + '"]').click();
      await page.locator('[id="sub-view-0"]').locator('[id="title"]').fill(newBodyText);
      await page.getByRole('button', {name: 'Submit'}).click();
      await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.saveAndPublish));

      // Assert
      await umbracoUi.isSuccessNotificationVisible();
      // Checks if the value inside of body has changed to the correct text
      await page.locator('[data-content-element-type-key="' + element['key'] + '"] >> [data-content-element-type-key="' + elementBody['key'] + '"]').click();
      await expect(page.locator('[id="sub-view-0"]').locator('[id="title"]')).toHaveValue(newBodyText);
    });

    test('can delete an area from a block grid editor', async ({page, umbracoApi, umbracoUi}) => {
      const element = await umbracoApi.documentTypes.createDefaultElementType(elementTitleName, elementTitleAlias);
      const elementBody = await umbracoApi.documentTypes.createDefaultElementType(elementBodyName, elementBodyAlias);

      await createContentWithABlockInAnotherBlock(umbracoApi, element, elementBody);

      await umbracoUi.navigateToContent(blockGridName);

      // Deletes the element in the area
      await page.locator('[data-content-element-type-key="' + element['key'] + '"] >> [data-content-element-type-key="' + elementBody['key'] + '"]').getByTitle("Delete").click();
      await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey('actions_delete'));
      await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.saveAndPublish));

      // Assert
      await umbracoUi.isSuccessNotificationVisible();
      // Checks if the element was removed from the area
      await expect(await page.locator('[data-content-element-type-key="' + element['key'] + '"] >> [data-content-element-type-key="' + elementBody['key'] + '"]')).not.toBeVisible();
    });

    test('can add multiple areas to a block grid editor', async ({page, umbracoApi, umbracoUi}) => {
      const areaAlias = "AreaBody";

      const element = await umbracoApi.documentTypes.createDefaultElementType(elementTitleName, elementTitleAlias);
      const elementBody = await umbracoApi.documentTypes.createDefaultElementType(elementBodyName, elementBodyAlias);

      const dataTypeBlockGrid = new BlockGridDataTypeBuilder()
        .withName(blockGridName)
        .addBlock()
          .withContentElementTypeKey(elementBody['key'])
        .done()
        .addBlock()
          .withContentElementTypeKey(element['key'])
          .addArea()
            .withAlias(titleArea)
          .done()
          .addArea()
            .withAlias(areaAlias)
          .done()
        .done()
        .build();
      const dataType = await umbracoApi.dataTypes.save(dataTypeBlockGrid);

      await createContentWithABlockInAnotherBlock(umbracoApi, element, elementBody, dataType);

      await umbracoUi.navigateToContent(blockGridName);

      // Adds another body to the area
      await page.locator('[data-content-element-type-key="' + element['key'] + '"] >> [data-area-alias="' + areaAlias + '"]').click();
      await page.locator('[name="infiniteEditorForm"]').locator('[data-element="editor-container"]').getByRole('button', {name: 'body'}).click();
      await page.locator('[id="sub-view-0"]').locator('[id="title"]').fill(bodyText);
      await page.locator('[label="Create"]').click();
      await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.saveAndPublish));

      // Assert
      await umbracoUi.isSuccessNotificationVisible();
      // Checks if both elements exist in the area
      await expect(page.locator('[data-content-element-type-key="' + element['key'] + '"] >> umb-block-grid-entries')).toHaveCount(2);
    });
  });

  test.describe('Grid Columns for Areas', () => {
    test('can see updated grid columns in areas for a block grid editor', async ({page, umbracoApi, umbracoUi}) => {
      const areaGridColumns = 6;

      const element = await umbracoApi.documentTypes.createDefaultElementType(elementTitleName, elementTitleAlias);

      const dataTypeBlockGrid = new BlockGridDataTypeBuilder()
        .withName(blockGridName)
        .addBlock()
          .withContentElementTypeKey(element['key'])
          .withAreaGridColumns(areaGridColumns)
          .addArea()
            .withColumnSpan(areaGridColumns)
            .withAlias(titleArea)
          .done()
        .done()
        .build();
      const dataType = await umbracoApi.dataTypes.save(dataTypeBlockGrid);

      await umbracoApi.content.createDefaultContentWithABlockGridEditor(element, dataType, false);

      await umbracoUi.navigateToContent(blockGridName);

      // Assert
      // Checks if the area in content has the value that is defined in the block grid editor
      await expect(page.locator('[data-content-element-type-key="' + element['key'] + '"]').locator('[data-area-alias="' + titleArea + '"]')).toHaveAttribute('data-area-col-span', '6');
    });

    test('can add two different blocks in a area with different grid columns', async ({page, umbracoApi, umbracoUi}) => {
      const secondArea = 'AreaFour';
      const columnSpanEight = "8";
      const columnSpanFour = "4";

      const element = await umbracoApi.documentTypes.createDefaultElementType(elementTitleName, elementTitleAlias);
      const elementBody = await umbracoApi.documentTypes.createDefaultElementType(elementBodyName, elementBodyAlias);

      const dataTypeBlockGrid = new BlockGridDataTypeBuilder()
        .withName(blockGridName)
        .addBlock()
          .withContentElementTypeKey(elementBody['key'])
        .done()
        .addBlock()
          .withContentElementTypeKey(element['key'])
          .addArea()
            .withAlias(titleArea)
            .withColumnSpan(columnSpanEight)
          .done()
          .addArea()
            .withAlias(secondArea)
            .withColumnSpan(columnSpanFour)
          .done()
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
                .appendContentProperties(elementBody.groups[0].properties[0].alias, bodyText)
                .withContentTypeKey(elementBody['key'])
              .done()
              .addBlockGridEntry()
                .appendContentProperties(element.groups[0].properties[0].alias, titleText)
                .withContentTypeKey(element['key'])
              .done()
              .addLayout()
                .withContentUdi(element['key'])
                .addAreas()
                  .withKey(dataType.preValues[0].value[1].areas[0].key)
                  .addItems()
                    .withContentUdi(elementBody['key'])
                  .done()
                .done()
              .done()
            .done()
          .done()
        .done()
        .build();
      await umbracoApi.content.save(rootContentNode);

      await umbracoUi.navigateToContent(blockGridName);

      // Adds a block to the area with a different column span
      await page.locator('[data-content-element-type-key="' + element['key'] + '"]').locator('[data-area-alias="' + secondArea + '"]').getByRole('button', {name: 'Add content'}).click();
      await page.locator('[name="infiniteEditorForm"]').locator('[data-element="editor-container"]').getByRole('button', {name: elementBodyName}).click();
      await page.locator('[id="sub-view-0"]').locator('[id="title"]').fill('BodyTwoText');
      await page.locator('[label="Create"]').click();
      await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.saveAndPublish));

      // Assert
      await umbracoUi.isSuccessNotificationVisible();
      // Checks if there are two block in the ElementTitle Area
      await expect(page.locator('[data-content-element-type-alias="'+elementTitleAlias+'"]').locator('umb-block-grid-entries')).toHaveCount(2);
      // Checks if there are two blocks with different column spans
      await expect(page.locator('[data-content-element-type-alias="' + elementTitleAlias + '"]').locator('umb-block-grid-entries').locator('[data-col-span="' + columnSpanEight + '"]')).toBeVisible();
      await expect(page.locator('[data-content-element-type-alias="' + elementTitleAlias + '"]').locator('umb-block-grid-entries').locator('[data-col-span="' + columnSpanFour + '"]')).toBeVisible();
    });

    test('can add a block with a different row span for an area in block grid editor', async ({page, umbracoApi, umbracoUi}) => {
      const element = await umbracoApi.documentTypes.createDefaultElementType(elementTitleName, elementTitleAlias);
      const elementBody = await umbracoApi.documentTypes.createDefaultElementType(elementBodyName, elementBodyAlias);

      const dataTypeBlockGrid = new BlockGridDataTypeBuilder()
        .withName(blockGridName)
        .addBlock()
          .withContentElementTypeKey(elementBody['key'])
          .withRowMaxSpan(3)
        .done()
        .addBlock()
          .withContentElementTypeKey(element['key'])
          .addArea()
            .withAlias(titleArea)
            .withRowSpan(3)
          .done()
        .done()
        .build();
      const dataType = await umbracoApi.dataTypes.save(dataTypeBlockGrid);

      await umbracoApi.content.createDefaultContentWithABlockGridEditor(element, dataType, false);

      await umbracoUi.navigateToContent(blockGridName);

      // Checks if the button has the correct row span
      await expect(page.locator('[data-content-element-type-key="' + element['key'] + '"]').locator('[data-area-row-span="3"]').getByRole('button', {name: 'Add content'})).toBeVisible();

      // Adds a block to the area
      await page.locator('[data-content-element-type-key="' + element['key'] + '"]').locator('[data-area-alias="' + titleArea + '"]').getByRole('button', {name: 'Add content'}).click();
      await page.locator('[name="infiniteEditorForm"]').locator('[data-element="editor-container"]').getByRole('button', {name: elementBodyName}).click();
      await page.locator('[id="sub-view-0"]').locator('[id="title"]').fill(bodyText);
      await page.locator('[label="Create"]').click();
      // Drags the block so it gets a row span of 3
      const dragFrom = await page.locator('[data-content-element-type-key="' + element['key'] + '"]').locator('[data-content-element-type-alias="' + elementBodyAlias + '"]').getByTitle('Drag to scale');
      const dragTo = await page.locator('[data-element="property-' + blockGridAlias + '"]').getByRole('button', {name: 'Clipboard'});
      await umbracoUi.dragAndDrop(dragFrom, dragTo, 0, 0, 10);
      await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.saveAndPublish));

      // Assert
      await umbracoUi.isSuccessNotificationVisible();
      // Checks if the row span of the block has changed from 1 to 3
      await expect(page.locator('[data-content-element-type-key="' + element['key'] + '"]').locator('[data-content-element-type-alias="' + elementBodyAlias + '"]')).toHaveAttribute('data-row-span', '3');
    });
  });

  test.describe('Create Button Label', () => {
    test('can add a create button label for an area in block grid editor', async ({page, umbracoApi, umbracoUi}) => {
      const newButtonLabel = 'NewAreaBlock';

      const element = await umbracoApi.documentTypes.createDefaultElementType(elementTitleName, elementTitleAlias);
      const elementBody = await umbracoApi.documentTypes.createDefaultElementType(elementBodyName, elementBodyAlias);

      const dataTypeBlockGrid = new BlockGridDataTypeBuilder()
        .withName(blockGridName)
        .addBlock()
          .withContentElementTypeKey(elementBody['key'])
        .done()
        .addBlock()
          .withContentElementTypeKey(element['key'])
          .addArea()
            .withCreateLabel(newButtonLabel)
            .withAlias(titleArea)
          .done()
        .done()
        .build();
      const dataType = await umbracoApi.dataTypes.save(dataTypeBlockGrid);

      await umbracoApi.content.createDefaultContentWithABlockGridEditor( element, dataType, false);
      await umbracoUi.navigateToContent(blockGridName);

      // Assert
      const createButtonLocator = await page.locator('[data-content-element-type-key="' + element['key'] + '"]').locator('[create-label="' + newButtonLabel + '"]');
      // Check if the area has the correct create button label
      await expect(createButtonLocator).toBeVisible();
      // Checks if the button is clickable
      await createButtonLocator.click();
    });
  });

  test.describe('Number of blocks', () => {
    test('can add a minimum number of blocks for a block grid editor', async ({page, umbracoApi, umbracoUi}) => {
      const element = await umbracoApi.documentTypes.createDefaultElementType(elementTitleName, elementTitleAlias);
      const elementBody = await umbracoApi.documentTypes.createDefaultElementType(elementBodyName, elementBodyAlias);

      const dataTypeBlockGrid = new BlockGridDataTypeBuilder()
        .withName(blockGridName)
        .addBlock()
          .withContentElementTypeKey(elementBody['key'])
        .done()
        .addBlock()
          .withContentElementTypeKey(element['key'])
          .addArea()
            .withMinAllowed(2)
            .withAlias(titleArea)
          .done()
        .done()
        .build();
      const dataType = await umbracoApi.dataTypes.save(dataTypeBlockGrid);

      await createContentWithABlockInAnotherBlock(umbracoApi, element, elementBody, dataType);

      await umbracoUi.navigateToContent(blockGridName);

      // Checks if a validation error is visible
      await expect(page.locator('[key="validation_entriesShort"]')).toBeVisible();
      // Tries to publish
      await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.saveAndPublish));
      // Checks if an error has been thrown with the property not being valid
      await expect(page.locator('[data-element="property-' + blockGridAlias + '"]', {hasText: 'This property is invalid'})).toBeVisible();
      // Adds an element to an area
      await page.locator('[data-content-element-type-key="' + element['key'] + '"]').locator('[data-area-alias="' + titleArea + '"]').getByRole('button', {name: 'Add content'}).click();
      await page.locator('[name="infiniteEditorForm"]').locator('[data-element="editor-container"]').getByRole('button', {name: elementBodyName}).click();
      await page.locator('[id="sub-view-0"]').locator('[id="title"]').fill('Test');
      await page.locator('[label="Create"]').click();
      // Tries to publish
      await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.saveAndPublish));

      // Assert
      await umbracoUi.isSuccessNotificationVisible();
      // Checks if there are two blocks in the area
      await expect(page.locator('[data-content-element-type-key="' + element['key'] + '"] >> umb-block-grid-entry')).toHaveCount(2);
    });

    test('can add a maximum number of blocks for a block grid editor', async ({page, umbracoApi, umbracoUi}) => {
      const bodyTextTwo = 'AnotherBody';

      const element = await umbracoApi.documentTypes.createDefaultElementType(elementTitleName, elementTitleAlias);
      const elementBody = await umbracoApi.documentTypes.createDefaultElementType(elementBodyName, elementBodyAlias);

      const dataTypeBlockGrid = new BlockGridDataTypeBuilder()
        .withName(blockGridName)
        .addBlock()
          .withContentElementTypeKey(elementBody['key'])
        .done()
        .addBlock()
          .withContentElementTypeKey(element['key'])
          .addArea()
            .withMaxAllowed(1)
            .withAlias(titleArea)
          .done()
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
                .appendContentProperties(elementBody.groups[0].properties[0].alias, bodyText)
                .withContentTypeKey(elementBody['key'])
              .done()
              .addBlockGridEntry()
                .appendContentProperties(elementBody.groups[0].properties[0].alias, bodyTextTwo)
                .withContentTypeKey(elementBody['key'])
              .done()
              .addBlockGridEntry()
                .appendContentProperties(element.groups[0].properties[0].alias, titleText)
                .withContentTypeKey(element['key'])
              .done()
              .addLayout()
                .withContentUdi(element['key'])
                .addAreas()
                  .withKey(dataType.preValues[0].value[1].areas[0].key)
                  .addItems()
                    .withContentUdi(elementBody['key'], 0)
                  .done()
                  .addItems()
                    .withContentUdi(elementBody['key'], 1)
                  .done()
                .done()
              .done()
            .done()
          .done()
        .done()
        .build();
      await umbracoApi.content.save(rootContentNode);

      await umbracoUi.navigateToContent(blockGridName);

      // Checks if a validation error is visible
      await expect(page.locator('[key="validation_entriesExceed"]')).toBeVisible();
      // Tries to publish
      await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.saveAndPublish));
      // Checks if an error has been thrown with the property not being valid
      await expect(page.locator('[data-element="property-' + blockGridAlias + '"]', {hasText: 'This property is invalid'})).toBeVisible();
      // Removes an element from a area
      const BodyTwoUdi = rootContentNode.variants[0].properties[0].value.contentData[1].udi;
      await page.locator('[data-content-element-type-key="' + element['key'] + '"] >> [data-element-udi="' + BodyTwoUdi + '"]').getByTitle("Delete").click();
      await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey('actions_delete'));
      // Tries to publish
      await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.saveAndPublish));

      // Assert
      await umbracoUi.isSuccessNotificationVisible();
      // Checks if there only is one block in the area
      await expect(page.locator('[data-content-element-type-key="' + element['key'] + '"] >> umb-block-grid-entry')).toHaveCount(1);
    });
  });

  test.describe('Allowed Block Types', () => {
    test('can add allowed block types for an area to a block grid editor', async ({page, umbracoApi, umbracoUi}) => {
      const element = await umbracoApi.documentTypes.createDefaultElementType(elementTitleName, elementTitleAlias);
      const elementBody = await umbracoApi.documentTypes.createDefaultElementType(elementBodyName, elementBodyAlias);

      const dataTypeBlockGrid = new BlockGridDataTypeBuilder()
        .withName(blockGridName)
        .addBlock()
          .withContentElementTypeKey(elementBody['key'])
        .done()
        .addBlock()
          .withContentElementTypeKey(element['key'])
          .addArea()
            .withAlias(titleArea)
            .addSpecifiedAllowance()
              .withElementTypeKey(elementBody['key'])
            .done()
          .done()
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
                .appendContentProperties(elementBody.groups[0].properties[0].alias, bodyText)
                .withContentTypeKey(elementBody['key'])
              .done()
              .addBlockGridEntry()
                .appendContentProperties(element.groups[0].properties[0].alias, titleText)
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

      // Adds a ElementBody
      await page.locator('[data-content-element-type-key="' + element['key'] + '"]').locator('[data-area-alias="' + titleArea + '"]').click();
      // Since the ElementBody is added as the only Specified Allowance for the area of the Element, then we should be instantly directed to it, instead of having to pick it.
      await page.locator('[id="sub-view-0"]').locator('[id="title"]').fill(bodyText);
      await page.locator('[label="Create"]').click();
      await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.saveAndPublish));

      // Assert
      await umbracoUi.isSuccessNotificationVisible();
    });

    test('can add allowed block types with a min amount in a area for a block grid editor', async ({page, umbracoApi, umbracoUi}) => {
      const element = await umbracoApi.documentTypes.createDefaultElementType(elementTitleName, elementTitleAlias);
      const elementBody = await umbracoApi.documentTypes.createDefaultElementType(elementBodyName, elementBodyAlias);

      const dataTypeBlockGrid = new BlockGridDataTypeBuilder()
        .withName(blockGridName)
        .addBlock()
          .withContentElementTypeKey(elementBody['key'])
        .done()
        .addBlock()
          .withContentElementTypeKey(element['key'])
          .addArea()
            .withAlias(titleArea)
            .addSpecifiedAllowance()
              .withElementTypeKey(elementBody['key'])
              .withMinAllowed(2)
            .done()
          .done()
        .done()
        .build();
      const dataType = await umbracoApi.dataTypes.save(dataTypeBlockGrid);

      await createContentWithABlockInAnotherBlock(umbracoApi, element, elementBody, dataType, null);

      await umbracoUi.navigateToContent(blockGridName);

      // Checks if a validation error is visible
      await expect(page.locator('[key="blockEditor_areaValidationEntriesShort"]')).toBeVisible();
      // Tries to publish
      await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.saveAndPublish));
      // Checks if an error has been thrown with the property not being valid
      await expect(page.locator('[data-element="property-' + blockGridAlias + '"]', {hasText: 'This property is invalid'})).toBeVisible();
      // Adds an element to an area
      await page.locator('[data-content-element-type-key="' + element['key'] + '"]').locator('[data-area-alias="' + titleArea + '"]').locator('.umb-block-grid__block--last-inline-create-button').click();
      await page.locator('[id="sub-view-0"]').locator('[id="title"]').fill('Test');
      await page.locator('[label="Create"]').click();
      // Tries to publish
      await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.saveAndPublish));

      // Assert
      await umbracoUi.isSuccessNotificationVisible();
      // Checks if there are two blocks in the area
      await expect(page.locator('[data-content-element-type-key="' + element['key'] + '"] >> umb-block-grid-entry')).toHaveCount(2);
    });

    test('can add allowed block types with a max amount in a area for a block grid editor', async ({page, umbracoApi, umbracoUi}) => {
      const element = await umbracoApi.documentTypes.createDefaultElementType(elementTitleName, elementTitleAlias);
      const elementBody = await umbracoApi.documentTypes.createDefaultElementType(elementBodyName, elementBodyAlias);

      const dataTypeBlockGrid = new BlockGridDataTypeBuilder()
        .withName(blockGridName)
        .addBlock()
          .withContentElementTypeKey(elementBody['key'])
        .done()
        .addBlock()
          .withContentElementTypeKey(element['key'])
          .addArea()
            .withAlias(titleArea)
            .addSpecifiedAllowance()
              .withElementTypeKey(elementBody['key'])
              .withMaxAllowed(2)
            .done()
          .done()
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
                .appendContentProperties(elementBody.groups[0].properties[0].alias, bodyText)
                .withContentTypeKey(elementBody['key'])
              .done()
              .addBlockGridEntry()
                .appendContentProperties(elementBody.groups[0].properties[0].alias, "BodyTwo")
                .withContentTypeKey(elementBody['key'])
              .done()
              .addBlockGridEntry()
                .appendContentProperties(elementBody.groups[0].properties[0].alias, "BodyThree")
                .withContentTypeKey(elementBody['key'])
              .done()
              .addBlockGridEntry()
                .appendContentProperties(element.groups[0].properties[0].alias, titleText)
                .withContentTypeKey(element['key'])
              .done()
              .addLayout()
                .withContentUdi(element['key'])
                .addAreas()
                  .withKey(dataType.preValues[0].value[1].areas[0].key)
                  .addItems()
                    .withContentUdi(elementBody['key'], 0)
                  .done()
                  .addItems()
                    .withContentUdi(elementBody['key'], 1)
                  .done()
                  .addItems()
                    .withContentUdi(elementBody['key'], 2)
                  .done()
                .done()
              .done()
            .done()
          .done()
        .done()
        .build();
      await umbracoApi.content.save(rootContentNode);

      await umbracoUi.navigateToContent(blockGridName);

      // Checks if a validation error is visible
      await expect(page.locator('[key="blockEditor_areaValidationEntriesExceed"]')).toBeVisible();
      // Tries to publish
      await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.saveAndPublish));
      // Checks if an error has been thrown with the property not being valid
      await expect(page.locator('[data-element="property-' + blockGridAlias + '"]', {hasText: 'This property is invalid'})).toBeVisible();
      // Removes an element from a area
      const BodyThreeUdi = rootContentNode.variants[0].properties[0].value.contentData[2].udi;
      await page.locator('[data-content-element-type-key="' + element['key'] + '"] >> [data-element-udi="' + BodyThreeUdi + '"]').getByTitle("Delete").click();
      await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey('actions_delete'));
      // Tries to publish
      await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.saveAndPublish));

      // Assert
      await umbracoUi.isSuccessNotificationVisible();
      // Checks if there are two blocks in the area
      await expect(page.locator('[data-content-element-type-key="' + element['key'] + '"] >> umb-block-grid-entry')).toHaveCount(2);
    });

    test('can add two different blocks with different min and max', async ({page, umbracoApi, umbracoUi}) => {
      const elementFooterName = 'ElementFooter';
      const elementFooterAlias = AliasHelper.toAlias(elementFooterName);

      await umbracoApi.documentTypes.ensureNameNotExists(elementFooterName);

      const element = await umbracoApi.documentTypes.createDefaultElementType(elementTitleName, elementTitleAlias);
      const elementBody = await umbracoApi.documentTypes.createDefaultElementType(elementBodyName, elementBodyAlias);
      const elementTitle = await umbracoApi.documentTypes.createDefaultElementType(elementFooterName, elementFooterAlias);

      const dataTypeBlockGrid = new BlockGridDataTypeBuilder()
        .withName(blockGridName)
        .addBlock()
          .withContentElementTypeKey(elementTitle['key'])
        .done()
        .addBlock()
          .withContentElementTypeKey(elementBody['key'])
        .done()
        .addBlock()
          .withContentElementTypeKey(element['key'])
          .addArea()
            .withAlias(titleArea)
            .addSpecifiedAllowance()
              .withElementTypeKey(elementBody['key'])
              .withMinAllowed(1)
              .withMaxAllowed(1)
            .done()
            .addSpecifiedAllowance()
              .withElementTypeKey(elementTitle['key'])
              .withMinAllowed(1)
              .withMaxAllowed(1)
            .done()
          .done()
        .done()
        .build();
      const dataType = await umbracoApi.dataTypes.save(dataTypeBlockGrid);

      await umbracoApi.documentTypes.createDefaultDocumentWithBlockGridEditor( element, dataType);

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
                .appendContentProperties(elementBody.groups[0].properties[0].alias, bodyText)
                .withContentTypeKey(elementBody['key'])
              .done()
              .addBlockGridEntry()
                .appendContentProperties(elementBody.groups[0].properties[0].alias, "AnotherBodyText")
                .withContentTypeKey(elementBody['key'])
              .done()
              .addBlockGridEntry()
                .appendContentProperties(element.groups[0].properties[0].alias, titleText)
                .withContentTypeKey(element['key'])
              .done()
              .addLayout()
                .withContentUdi(element['key'])
                .addAreas()
                  .withKey(dataType.preValues[0].value[2].areas[0].key)
                  .addItems()
                    .withContentUdi(elementBody['key'], 0)
                  .done()
                  .addItems()
                    .withContentUdi(elementBody['key'], 1)
                  .done()
                .done()
              .done()
            .done()
          .done()
        .done()
        .build();
      await umbracoApi.content.save(rootContentNode);

      await umbracoUi.navigateToContent(blockGridName);

      // Checks if there is validation error for both blocks
      await expect(page.locator('[key="blockEditor_areaValidationEntriesExceed"]', {hasText: elementBodyName})).toBeVisible();
      await expect(page.locator('[key="blockEditor_areaValidationEntriesShort"]', {hasText: elementFooterName})).toBeVisible();
      // Tries to publish
      await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.saveAndPublish));
      // Checks if an error has been thrown with the property not being valid
      await expect(page.locator('[data-element="property-' + blockGridAlias + '"]', {hasText: 'This property is invalid'})).toBeVisible();
      // Removes an ElementBody from a area
      const BodyTwoUdi = rootContentNode.variants[0].properties[0].value.contentData[1].udi;
      await page.locator('[data-content-element-type-key="' + element['key'] + '"] >> [data-element-udi="' + BodyTwoUdi + '"]').getByTitle("Delete").click();
      await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey('actions_delete'));
      // Checks if the validation error for ElementBody is removed
      await expect(page.locator('[key="blockEditor_areaValidationEntriesExceed"]', {hasText: elementBodyName})).not.toBeVisible();
      // Adds an ElementFooter to the area
      await page.locator('[data-content-element-type-key="' + element['key'] + '"]').locator('[data-area-alias="' + titleArea + '"]').locator('.umb-block-grid__block--last-inline-create-button').click();
      await page.locator('[name="infiniteEditorForm"]').locator('[data-element="editor-container"]').getByRole('button', {name: elementFooterName}).click();
      await page.locator('[id="sub-view-0"]').locator('[id="title"]').fill('TitleTest');
      await page.locator('[label="Create"]').click();
      // Checks if the validation error for ElementTitle is removed
      await expect(page.locator('[key="blockEditor_areaValidationEntriesShort"]', {hasText: elementFooterName})).not.toBeVisible();
      // Tries to publish
      await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.saveAndPublish));

      // Assert
      await umbracoUi.isSuccessNotificationVisible();
      // Checks if there are two blocks in the area
      await expect(page.locator('[data-content-element-type-key="' + element['key'] + '"]').locator('[data-content-element-type-alias="' + elementBodyAlias + '"]')).toHaveCount(1);
      await expect(page.locator('[data-content-element-type-key="' + element['key'] + '"]').locator('[data-content-element-type-alias="' + elementFooterAlias + '"]')).toHaveCount(1);

      // Clean
      await umbracoApi.documentTypes.ensureNameNotExists(elementFooterName);
    });
  });

  test.describe('Moving Areas', () => {
    test('can move an element out of an area', async ({page, umbracoApi, umbracoUi}) => {
      const element = await umbracoApi.documentTypes.createDefaultElementType(elementTitleName, elementTitleAlias);
      const elementBody = await umbracoApi.documentTypes.createDefaultElementType(elementBodyName, elementBodyAlias);

      const dataType = await umbracoApi.dataTypes.createBlockGridDataTypeWithArea(element, elementBody, blockGridName, titleArea);

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
                .appendContentProperties(elementBody.groups[0].properties[0].alias, bodyText)
                .withContentTypeKey(elementBody['key'])
              .done()
              .addBlockGridEntry()
                .appendContentProperties(element.groups[0].properties[0].alias, titleText)
                .withContentTypeKey(element['key'])
              .done()
              .addLayout()
                .withContentUdi(element['key'], 0)
                .addAreas()
                  .withKey(dataType.preValues[0].value[1].areas[0].key)
                  .addItems()
                    .withContentUdi(elementBody['key'], 0)
                  .done()
                .done()
              .done()
            .done()
          .done()
        .done()
        .build();
      await umbracoApi.content.save(rootContentNode);

      await umbracoUi.navigateToContent(blockGridName);

      // Drags and drops the ElementBody to another element
      const dragFrom = await page.locator('[data-content-element-type-key="' + element['key'] + '"]').locator('[data-content-element-type-alias="' + elementBodyAlias + '"]');
      const dragTo = await page.locator('[key="blockEditor_addBlock"]');
      await umbracoUi.dragAndDrop(dragFrom, dragTo, 0, 0, 10);

      // Assert
      await expect(page.locator('[data-element="property-' + blockGridAlias + '"]').locator('umb-block-grid-entry')).toHaveCount(2);
    });

    test('can move an element from one area to another', async ({page, umbracoApi, umbracoUi}) => {
      const element = await umbracoApi.documentTypes.createDefaultElementType(elementTitleName, elementTitleAlias);
      const elementBody = await umbracoApi.documentTypes.createDefaultElementType(elementBodyName, elementBodyAlias);

      const dataType = await umbracoApi.dataTypes.createBlockGridDataTypeWithArea(element, elementBody, blockGridName, titleArea);

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
                .appendContentProperties(elementBody.groups[0].properties[0].alias, bodyText)
                .withContentTypeKey(elementBody['key'])
              .done()
              .addBlockGridEntry()
                .appendContentProperties(element.groups[0].properties[0].alias, titleText)
                .withContentTypeKey(element['key'])
              .done()
              .addBlockGridEntry()
                .appendContentProperties(element.groups[0].properties[0].alias, "AnotherOne")
                .withContentTypeKey(element['key'])
              .done()
              .addLayout()
                .withContentUdi(element['key'], 0)
              .done()
              .addLayout()
                .withContentUdi(element['key'], 1)
                .addAreas()
                  .withKey(dataType.preValues[0].value[1].areas[0].key)
                  .addItems()
                    .withContentUdi(elementBody['key'], 0)
                  .done()
                .done()
              .done()
            .done()
          .done()
        .done()
        .build();
      await umbracoApi.content.save(rootContentNode);

      const ElementMoveFromUdi = rootContentNode.variants[0].properties[0].value.contentData[2].udi;
      const ElementMoveToUdi = rootContentNode.variants[0].properties[0].value.contentData[1].udi;

      await umbracoUi.navigateToContent(blockGridName);

      // Drags and drops the ElementBody to another element
      const dragFrom = await page.locator('[data-element-udi="' + ElementMoveFromUdi + '"]').locator('[data-content-element-type-alias="' + elementBodyAlias + '"]');
      const dragTo = await page.locator('[data-element-udi="' + ElementMoveToUdi + '"]').locator('[data-area-alias="' + titleArea + '"]').getByRole('button', {name: 'Add content'});
      await umbracoUi.dragAndDrop(dragFrom, dragTo, -15, 0, 15);

      // Assert
      // Expects the element MoveFrom to have 0 entries
      await expect(page.locator('[data-element-udi="' + ElementMoveFromUdi + '"]').locator('umb-block-grid-entry')).toHaveCount(0);
      // Expects the element MoveTo to have 1 entry
      await expect(page.locator('[data-element-udi="' + ElementMoveToUdi + '"]').locator('umb-block-grid-entry')).toHaveCount(1);
    });
  });

  test.describe('Copy and Paste Blocks', () => {
    test('can copy and paste an element in a area', async ({page, umbracoApi, umbracoUi}) => {
      const element = await umbracoApi.documentTypes.createDefaultElementType(elementTitleName, elementTitleAlias);
      const elementBody = await umbracoApi.documentTypes.createDefaultElementType(elementBodyName, elementBodyAlias);

      const dataType = await umbracoApi.dataTypes.createBlockGridDataTypeWithArea(element, elementBody, blockGridName, titleArea);

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
                .appendContentProperties(elementBody.groups[0].properties[0].alias, bodyText)
                .withContentTypeKey(elementBody['key'])
              .done()
              .addBlockGridEntry()
                .appendContentProperties(element.groups[0].properties[0].alias, titleText)
                .withContentTypeKey(element['key'])
              .done()
              .addBlockGridEntry()
                .appendContentProperties(element.groups[0].properties[0].alias, "AnotherOne")
                .withContentTypeKey(element['key'])
              .done()
              .addLayout()
                .withContentUdi(element['key'], 0)
              .done()
              .addLayout()
                .withContentUdi(element['key'], 1)
                .addAreas()
                  .withKey(dataType.preValues[0].value[1].areas[0].key)
                  .addItems()
                    .withContentUdi(elementBody['key'], 0)
                  .done()
                .done()
              .done()
            .done()
          .done()
        .done()
        .build();
      await umbracoApi.content.save(rootContentNode);

      const blockParentUdi = rootContentNode.variants[0].properties[0].value.contentData[2].udi;
      const blockToCopyTooUdi = rootContentNode.variants[0].properties[0].value.contentData[1].udi;

      await umbracoUi.navigateToContent(blockGridName);

      // Copies and pastes the block into another block
      await page.locator('[data-element-udi="' + blockParentUdi + '"]').locator('[data-content-element-type-alias="' + elementBodyAlias + '"]').getByRole('button', {name: 'Copy'}).click();
      await page.locator('[data-element-udi="' + blockToCopyTooUdi + '"]').locator('[data-area-alias="' + titleArea + '"]').click();
      await umbracoUi.clickDataElementByElementName('sub-view-clipboard');
      await page.locator('[data-element="editor-container"]').locator('umb-block-card', {hasText: elementBodyName}).click();

      // Assert
      // Checks if both blocks exist with a block inside of each
      await expect(page.locator('[data-element-udi="' + blockParentUdi + '"]').locator('[data-content-element-type-alias="' + elementBodyAlias + '"]')).toBeVisible();
      await expect(page.locator('[data-element-udi="' + blockToCopyTooUdi + '"]').locator('[data-content-element-type-alias="' + elementBodyAlias + '"]')).toBeVisible();
      // Checks if the correct text is inside of the pasted block
      await page.locator('[data-element-udi="' + blockToCopyTooUdi + '"]').locator('[data-content-element-type-alias="' + elementBodyAlias + '"]').click();
      await expect(page.locator('[id="sub-view-0"]').locator('[id="title"]')).toHaveValue(bodyText);
    });

    test('can copy and paste a block with another block inside of it', async ({page, umbracoApi, umbracoUi}) => {
      const element = await umbracoApi.documentTypes.createDefaultElementType(elementTitleName, elementTitleAlias);
      const elementBody = await umbracoApi.documentTypes.createDefaultElementType(elementBodyName, elementBodyAlias);

      const dataType = await umbracoApi.dataTypes.createBlockGridDataTypeWithArea(element, elementBody, blockGridName, titleArea);

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
                .appendContentProperties(elementBody.groups[0].properties[0].alias, bodyText)
                .withContentTypeKey(elementBody['key'])
              .done()
              .addBlockGridEntry()
                .appendContentProperties(element.groups[0].properties[0].alias, titleText)
                .withContentTypeKey(element['key'])
              .done()
              .addLayout()
                .withContentUdi(element['key'], 0)
                .addAreas()
                  .withKey(dataType.preValues[0].value[1].areas[0].key)
                  .addItems()
                    .withContentUdi(elementBody['key'], 0)
                  .done()
                .done()
              .done()
            .done()
          .done()
        .done()
        .build();
      await umbracoApi.content.save(rootContentNode);

      const blockParentUdi = rootContentNode.variants[0].properties[0].value.contentData[1].udi;

      await umbracoUi.navigateToContent(blockGridName);

      // Copies and pastes the block into another block
      await page.locator('[data-element-udi="' + blockParentUdi + '"]').getByRole('button', {name: 'Copy'}).nth(1).click();
      await page.locator('[data-element="property-' + blockGridAlias + '"]').getByRole('button', {name: 'Clipboard'}).click();
      await page.locator('[data-element="editor-container"]').locator('umb-block-card', {hasText: elementTitleName}).click();

      // Checks if the blocks were copied
      await expect(page.locator('[data-content-element-type-alias="' + elementTitleAlias + '"]')).toHaveCount(2);
    });
  });
});
