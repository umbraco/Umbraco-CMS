import {AliasHelper, ConstantHelper, test} from "@umbraco/playwright-testhelpers";
import {BlockGridDataTypeBuilder} from "@umbraco/json-models-builders/dist/lib/builders/dataTypes";
import {expect} from "@playwright/test";
import {ContentBuilder} from "@umbraco/json-models-builders";

test.describe('BlockGridEditorSettings', () => {
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

  test.describe('General', () => {
    test('can see label in content for a block grid editor', async ({page, umbracoApi, umbracoUi}) => {
      const newLabel = "New Label";

      const element = await umbracoApi.documentTypes.createDefaultElementType(elementName, elementAlias);

      const dataTypeBlockGrid = new BlockGridDataTypeBuilder()
        .withName(blockGridName)
        .addBlock()
          .withContentElementTypeKey(element['key'])
          .withLabel(newLabel + '{{}}')
        .done()
        .build();
      const dataType = await umbracoApi.dataTypes.save(dataTypeBlockGrid);

      await umbracoApi.content.createDefaultContentWithABlockGridEditor(element, dataType, false);

      await umbracoUi.navigateToContent(blockGridName);

      // Assert
      // Checks if the element contains the correct label
      await expect(page.locator('[data-content-element-type-alias="' + elementAlias + '"]')).toContainText(newLabel);
      // Checks if the element is clickable
      await page.locator('[data-content-element-type-alias="' + elementAlias + '"]').click();
    });
  });

  test.describe('Permissions', () => {
    test('can set allow in root to false for a element in a block grid editor', async ({page, umbracoApi, umbracoUi}) => {
      const element = await umbracoApi.documentTypes.createDefaultElementType(elementName, elementAlias);

      const dataTypeBlockGrid = new BlockGridDataTypeBuilder()
        .withName(blockGridName)
        .addBlock()
          .withContentElementTypeKey(element['key'])
          .withAllowAtRoot(false)
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
        .done()
        .build();
      await umbracoApi.content.save(rootContentNode);

      await umbracoUi.navigateToContent(blockGridName);

      // Checks if adding a block is disabled
      await expect(page.locator('[data-element="property-'+blockGridAlias+'"]').locator('umb-block-grid-root').locator('[disabled="disabled"]')).toBeDisabled();
      // Checks if the button is not clickable
      await expect(page.locator('[data-element="property-' + blockGridAlias + '"]').locator('[key="blockEditor_addBlock"]')).not.toBeEnabled();
    });

    test('can set allow in areas to false for an element in a block grid editor', async ({page, umbracoApi, umbracoUi}) => {
      const elementBodyName = 'BodyElement';
      const elementBodyAlias = AliasHelper.toAlias(elementBodyName);

      await umbracoApi.documentTypes.ensureNameNotExists(elementBodyName);

      const element = await umbracoApi.documentTypes.createDefaultElementType(elementName, elementAlias);
      const elementBody = await umbracoApi.documentTypes.createDefaultElementType(elementBodyName, elementBodyAlias);

      const dataTypeBlockGrid = new BlockGridDataTypeBuilder()
        .withName(blockGridName)
        .addBlock()
          .withContentElementTypeKey(elementBody['key'])
          .withAllowInAreas(false)
        .done()
        .addBlock()
          .withContentElementTypeKey(element['key'])
          .addArea()
            .withAlias('titleArea')
          .done()
        .done()
        .build();
      const dataType = await umbracoApi.dataTypes.save(dataTypeBlockGrid);

      await umbracoApi.content.createDefaultContentWithABlockGridEditor(element, dataType, false);

      await umbracoUi.navigateToContent(blockGridName);

      // Assert
      // Checks if the elementTitle is the only element selectable to be in a area
      await expect(page.locator('[data-area-alias="titleArea"]').locator('[key="blockEditor_addThis"]')).toContainText('Add ' + elementName);

      // Clean
      await umbracoApi.documentTypes.ensureNameNotExists(elementBodyName);
    });
  });

  test.describe('Size options', () => {
    test('can resize a block to another column span for an element in a block grid editor', async ({page, umbracoApi, umbracoUi}) => {
      const element = await umbracoApi.documentTypes.createDefaultElementType(elementName, elementAlias);
      const dataTypeBlockGrid = new BlockGridDataTypeBuilder()
        .withName(blockGridName)
        .addBlock()
          .withContentElementTypeKey(element['key'])
          .addColumnSpanOptions(12)
          .addColumnSpanOptions(6)
        .done()
        .build();
      const dataType = await umbracoApi.dataTypes.save(dataTypeBlockGrid);

      await umbracoApi.content.createDefaultContentWithABlockGridEditor(element, dataType, false);

      await umbracoUi.navigateToContent(blockGridName);

      // Drags the blocks from a columnSpan of 12 to 6
      const dragFrom = await page.locator('[data-content-element-type-key="' + element['key'] + '"]', {hasText: elementName}).locator('[title="Drag to scale"]');
      const dragTo = await page.locator('[data-content-element-type-key="' + element['key'] + '"]', {hasText: elementName});
      await umbracoUi.dragAndDrop(dragFrom,dragTo, 0, 0 ,10);
      await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.saveAndPublish));

      // Assert
      await umbracoUi.isSuccessNotificationVisible();
      // Checks if the block is resized to a column span of 6
      await expect(page.locator('[data-content-element-type-key="' + element['key'] + '"], [data-col-span="6"]', {hasText: elementName})).toBeVisible();
    });
  });
});
