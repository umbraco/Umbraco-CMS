import {ConstantHelper, test} from "@umbraco/playwright-testhelpers";
import {expect} from "@playwright/test";
import {BlockGridDataTypeBuilder} from "@umbraco/json-models-builders/dist/lib/builders/dataTypes";
import {StylesheetBuilder} from "@umbraco/json-models-builders";

test.describe('BlockGridEditorDataTypeConfiguration', () => {
  const blockGridName = 'BlockGridEditorTest';

  test.beforeEach(async ({page, umbracoApi}, testInfo) => {
    await umbracoApi.report.report(testInfo);
    await umbracoApi.login();
    await umbracoApi.dataTypes.ensureNameNotExists(blockGridName);
  });

  test.afterEach(async ({page, umbracoApi, umbracoUi}) => {
    await umbracoApi.dataTypes.ensureNameNotExists(blockGridName);
  });
  
  async function createEmptyBlockGridWithName(umbracoApi) {
    const blockGridType = new BlockGridDataTypeBuilder()
      .withName(blockGridName)
      .build();
    await umbracoApi.dataTypes.save(blockGridType);

    return blockGridType;
  }

  test.describe('Amount tests', () => {

    test('can add a min and max amount to a block grid editor', async ({page, umbracoApi, umbracoUi}) => {
      await createEmptyBlockGridWithName(umbracoApi);

      await umbracoUi.navigateToDataType(blockGridName);

      // Changes the amount in min and max
      await page.locator('[name="numberFieldMin"]').fill('2');
      await page.locator('[name="numberFieldMax"]').fill('4');
      await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.save));

      // Assert
      await umbracoUi.isSuccessNotificationVisible();
      // Checks if min and max were added
      await expect(page.locator('input[name="numberFieldMin"]')).toHaveValue('2');
      await expect(page.locator('input[name="numberFieldMax"]')).toHaveValue('4');
      // Checks if the datatype was created
      await expect(await umbracoApi.dataTypes.exists(blockGridName)).toBe(true);
      await umbracoUi.doesDataTypeExist(blockGridName);
    });

    test('can edit a min and max amount in a block grid editor', async ({page, umbracoApi, umbracoUi}) => {
      const blockGridType = new BlockGridDataTypeBuilder()
        .withName(blockGridName)
        .withMin(1)
        .withMax(2)
        .build();
      await umbracoApi.dataTypes.save(blockGridType);

      await umbracoUi.navigateToDataType(blockGridName);

      // Updates min so it's equal to max
      await page.locator('[name="numberFieldMin"]').fill('2');
      await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.save));

      // Assert
      await umbracoUi.isSuccessNotificationVisible();
      // Checks if min and max were updated
      await expect(page.locator('input[name="numberFieldMin"]')).toHaveValue('2');
      await expect(page.locator('input[name="numberFieldMax"]')).toHaveValue('2');
      // Checks if the datatype was created
      await expect(await umbracoApi.dataTypes.exists(blockGridName)).toBe(true);
      await umbracoUi.doesDataTypeExist(blockGridName);
    });

    test('min amount cant be more than max amount in a block grid editor', async ({page, umbracoApi, umbracoUi}) => {
      await createEmptyBlockGridWithName(umbracoApi);

      await umbracoUi.navigateToDataType(blockGridName);

      // Updates min so it's more than max
      await page.locator('[name="numberFieldMin"]').fill('4');
      await page.locator('[name="numberFieldMax"]').fill('2');
      await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.save));

      // Assert
      // Checks if an error message is visible.
      await expect(page.locator('.alert-error >> "This property is invalid"')).toBeVisible();
      // Checks if the datatype was created
      await expect(await umbracoApi.dataTypes.exists(blockGridName)).toBe(true);
      await umbracoUi.doesDataTypeExist(blockGridName);
    });
  });

  test.describe('Live editing mode tests', () => {

    test('can turn live editing mode on for a block grid editor', async ({page, umbracoApi, umbracoUi}) => {
      await createEmptyBlockGridWithName(umbracoApi);

      await umbracoUi.navigateToDataType(blockGridName);

      // Enables live editing mode
      await page.locator('[id="useLiveEditing"]').click();

      // Assert
      // Checks if live editing mode is true
      await expect(page.locator('.umb-property-editor >> .umb-toggle--checked')).toBeVisible();
      // Checks if the datatype was created
      await expect(await umbracoApi.dataTypes.exists(blockGridName)).toBe(true);
      await umbracoUi.doesDataTypeExist(blockGridName);
    });
  });

  test.describe('Editor width tests', () => {

    test('can add editor width for a block grid editor', async ({page, umbracoApi, umbracoUi}) => {
      const editorWidth = '100%';

      await createEmptyBlockGridWithName(umbracoApi);

      await umbracoUi.navigateToDataType(blockGridName);

      // Adds editor width
      await page.locator('[id="maxPropertyWidth"]').fill(editorWidth);
      await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.save));

      // Assert
      await umbracoUi.isSuccessNotificationVisible();
      // Checks if the editor width was added
      await expect(page.locator('input[id="maxPropertyWidth"]')).toHaveValue(editorWidth);
      // Checks if the datatype was created
      await expect(await umbracoApi.dataTypes.exists(blockGridName)).toBe(true);
      await umbracoUi.doesDataTypeExist(blockGridName);
    });

    test('can edit editor width for a block grid editor', async ({page, umbracoApi, umbracoUi}) => {
      const editorWidth = '50%';

      const blockGridType = new BlockGridDataTypeBuilder()
        .withName(blockGridName)
        .withMaxPropertyWidth('100%')
        .build();
      await umbracoApi.dataTypes.save(blockGridType);

      await umbracoUi.navigateToDataType(blockGridName);

      // Edits editor width
      await page.locator('[id="maxPropertyWidth"]').fill(editorWidth);
      await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.save));

      // Assert
      await umbracoUi.isSuccessNotificationVisible();
      // Checks if the editor width was updated
      await expect(page.locator('input[id="maxPropertyWidth"]')).toHaveValue(editorWidth);
      // Checks if the datatype was created
      await expect(await umbracoApi.dataTypes.exists(blockGridName)).toBe(true);
      await umbracoUi.doesDataTypeExist(blockGridName);
    });
  });

  test.describe('Grid columns tests', () => {

    test('can add grid columns for a block grid editor', async ({page, umbracoApi, umbracoUi}) => {
      const gridColumns = '10';

      await createEmptyBlockGridWithName(umbracoApi);

      await umbracoUi.navigateToDataType(blockGridName);

      // Adds grid columns
      await page.locator('[name="numberField"]').fill(gridColumns);
      await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.save));

      // Assert
      await umbracoUi.isSuccessNotificationVisible();
      // Checks if the grid columns was added
      await expect(page.locator('input[name="numberField"]')).toHaveValue(gridColumns);
      // Checks if the datatype was created
      await expect(await umbracoApi.dataTypes.exists(blockGridName)).toBe(true);
      await umbracoUi.doesDataTypeExist(blockGridName);
    });

    test('can edit grid columns for a block grid editor', async ({page, umbracoApi, umbracoUi}) => {
      const gridColumns = '9';

      const blockGridType = new BlockGridDataTypeBuilder()
        .withName(blockGridName)
        .withGridColumns(8)
        .build();
      await umbracoApi.dataTypes.save(blockGridType);

      await umbracoUi.navigateToDataType(blockGridName);

      // Edits grid columns
      await page.locator('[name="numberField"]').fill(gridColumns);
      await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.save));

      // Assert
      await umbracoUi.isSuccessNotificationVisible();
      // Checks if grid columns were updated
      await expect(page.locator('input[name="numberField"]')).toHaveValue(gridColumns);
      // Checks if the datatype was created
      await expect(await umbracoApi.dataTypes.exists(blockGridName)).toBe(true);
      await umbracoUi.doesDataTypeExist(blockGridName);
    });
  });

  test.describe('Layout stylesheet tests', () => {

    test('can add a layout stylesheet for a block grid editor', async ({page, umbracoApi, umbracoUi}) => {
      const stylesheetName = 'StylesheetTest';

      await umbracoApi.stylesheets.ensureNameNotExists(stylesheetName + '.css');

      await createEmptyBlockGridWithName(umbracoApi);

      const stylesheet = new StylesheetBuilder()
        .withVirtualPath("/css/")
        .withFileType("stylesheets")
        .withName(stylesheetName)
        .build();
      await umbracoApi.stylesheets.save(stylesheet);

      await umbracoUi.navigateToDataType(blockGridName);

      await umbracoUi.clickElement(umbracoUi.getButtonByKey('blockEditor_addCustomStylesheet'));
      await page.locator('[data-element="tree-item-wwwroot"]').locator('[data-element="tree-item-expand"]').click();
      await page.locator('[data-element="tree-item-css"]').locator('[data-element="tree-item-expand"]').click();
      await umbracoUi.clickDataElementByElementName('tree-item-' + stylesheetName + '.css');
      await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.save));

      // Assert
      await umbracoUi.isSuccessNotificationVisible();
      // Checks if the stylesheet was added
      await expect(page.locator('.umb-node-preview__name', {hasText: 'StylesheetTest'})).toBeVisible();
      // Checks if the datatype was created
      await expect(await umbracoApi.dataTypes.exists(blockGridName)).toBe(true);
      await umbracoUi.doesDataTypeExist(blockGridName);

      // Clean
      await umbracoApi.stylesheets.ensureNameNotExists(stylesheetName + '.css');
    });

    test('can remove a layout stylesheet from a block grid editor', async ({page, umbracoApi, umbracoUi}) => {
      const stylesheetName = 'StylesheetTest';
      const path = '/css/';

      await umbracoApi.stylesheets.ensureNameNotExists(stylesheetName + '.css');

      const stylesheet = new StylesheetBuilder()
        .withVirtualPath(path)
        .withFileType("stylesheets")
        .withName(stylesheetName)
        .build();
      await umbracoApi.stylesheets.save(stylesheet);

      const blockGridType = new BlockGridDataTypeBuilder()
        .withName(blockGridName)
        .withLayoutStylesheet('~' + path + stylesheetName + '.css')
        .build();
      await umbracoApi.dataTypes.save(blockGridType);

      await umbracoUi.navigateToDataType(blockGridName);

      // Removes the stylesheet
      await page.locator('.__control-actions >> .btn-reset').click();
      await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.save));

      // Assert
      await umbracoUi.isSuccessNotificationVisible();
      // Checks if the stylesheet is deleted
      await expect(page.locator('.umb-node-preview__name', {hasText: 'StylesheetTest'})).not.toBeVisible();
      // Checks if the datatype was created
      await expect(await umbracoApi.dataTypes.exists(blockGridName)).toBe(true);
      await umbracoUi.doesDataTypeExist(blockGridName);

      // Clean
      await umbracoApi.stylesheets.ensureNameNotExists(stylesheetName + '.css');
    });
  });

  test.describe('Create button label tests', () => {

    test('can add a create button label for a block grid editor', async ({page, umbracoApi, umbracoUi}) => {
      const createButtonLabel = 'testButton';

      await createEmptyBlockGridWithName(umbracoApi);

      await umbracoUi.navigateToDataType(blockGridName);

      // Adds create button label text
      await page.locator('[id="createLabel"]').fill(createButtonLabel);
      await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.save));

      // Assert
      await umbracoUi.isSuccessNotificationVisible();
      // Checks if the label for create button label was added
      await expect(page.locator('input[id="createLabel"]')).toHaveValue(createButtonLabel);
      // Checks if the datatype was created
      await expect(await umbracoApi.dataTypes.exists(blockGridName)).toBe(true);
      await umbracoUi.doesDataTypeExist(blockGridName);
    });

    test('can edit a create button label for a block grid editor', async ({page, umbracoApi, umbracoUi}) => {
      const editButtonLabel = 'NewButtonLabel';

      const blockGridType = new BlockGridDataTypeBuilder()
        .withName(blockGridName)
        .withCreateLabel('OldLabel')
        .build();
      await umbracoApi.dataTypes.save(blockGridType);

      await umbracoUi.navigateToDataType(blockGridName);

      // Edits create button label text
      await page.locator('[id="createLabel"]').fill(editButtonLabel);
      await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.save));

      // Assert
      await umbracoUi.isSuccessNotificationVisible();
      // Checks if the label for create button label was updated
      await expect(page.locator('input[id="createLabel"]')).toHaveValue(editButtonLabel);
      // Checks if the datatype was created
      await expect(await umbracoApi.dataTypes.exists(blockGridName)).toBe(true);
      await umbracoUi.doesDataTypeExist(blockGridName);
    });
  });
});