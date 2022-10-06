import { expect, Page } from '@playwright/test';
import {test, ApiHelpers, UiHelpers, AliasHelper, ConstantHelper} from '@umbraco/playwright-testhelpers';
import { DocumentTypeBuilder } from "@umbraco/json-models-builders";

const tabsDocTypeName = 'Tabs Test Document';
const tabsDocTypeAlias = AliasHelper.toAlias(tabsDocTypeName);

test.describe('Tabs', () => {
  
  test.beforeEach(async ({ umbracoApi, page }) => {
    // TODO: REMOVE THIS WHEN SQLITE IS FIXED
    // Wait so we don't bombard the API
    await page.waitForTimeout(1000);
    await umbracoApi.login();
  });

  test.afterEach(async ({page, umbracoApi}) => {
    await umbracoApi.documentTypes.ensureNameNotExists(tabsDocTypeName);
    await umbracoApi.templates.ensureNameNotExists(tabsDocTypeName)
  });

  async function openDocTypeFolder(umbracoUi: UiHelpers, page: Page) {
    await umbracoUi.goToSection('settings');
    await umbracoUi.waitForTreeLoad('settings');
    await page.locator('.umb-tree-item__inner > .umb-tree-item__arrow').first().click();
    await page.locator(`a:has-text("${tabsDocTypeName}")`).click();
  }

  async function createDocTypeWithTabsAndNavigate(umbracoUi: UiHelpers, umbracoApi: ApiHelpers, page: Page){
    await umbracoApi.documentTypes.ensureNameNotExists(tabsDocTypeName);
    await umbracoApi.content.deleteAllContent();
    const tabsDocType = new DocumentTypeBuilder()
        .withName(tabsDocTypeName)
        .withAlias(tabsDocTypeAlias)
        .withAllowAsRoot(true)
        .withDefaultTemplate(tabsDocTypeAlias)
        .addTab()
          .withName('Tab 1')
          .addGroup()
            .withName('Tab group')
            .addUrlPickerProperty()
              .withAlias("urlPicker")
            .done()
          .done()
        .done()
        .build();
    await umbracoApi.documentTypes.save(tabsDocType);
    await openDocTypeFolder(umbracoUi, page);
  }

  test('Create tab', async ({umbracoUi, umbracoApi, page}) => {
    await umbracoApi.documentTypes.ensureNameNotExists(tabsDocTypeName);
    await umbracoApi.content.deleteAllContent();
    const tabsDocType = new DocumentTypeBuilder()
        .withName(tabsDocTypeName)
        .withAlias(tabsDocTypeAlias)
        .withAllowAsRoot(true)
        .withDefaultTemplate(tabsDocTypeAlias)
        .addGroup()
          .withName('Tabs1Group')
          .addUrlPickerProperty()
            .withAlias('picker')
          .done()
        .done()
        .build();
    await umbracoApi.documentTypes.save(tabsDocType);

    await umbracoUi.goToSection('settings');
    await umbracoUi.waitForTreeLoad('settings');

    await umbracoUi.clickElement(umbracoUi.getTreeItem("settings", ["Document Types", tabsDocTypeName]))
    // Create a tab 
    await page.locator('.umb-group-builder__tabs__add-tab').click();
    await page.locator('ng-form.ng-invalid > .umb-group-builder__group-title-input').fill('Tab 1');
    // Create a 2nd tab manually
    await page.locator('.umb-group-builder__tabs__add-tab').click();
    await page.locator('ng-form.ng-invalid > .umb-group-builder__group-title-input').fill('Tab 2');
    // Create a textstring property
    await page.locator('[aria-hidden="false"] > .umb-box-content > .umb-group-builder__group-add-property').click();
    await page.locator('.editor-label').fill('property name');
    await page.locator('[data-element="editor-add"]').click();

    // Search for textstring
    await page.locator('#datatype-search').fill('Textstring');

    // Choose first item
    await page.locator('[title="Textstring"]').first().click();

    // Save property
    await page.locator('.btn-success').last().click();
    await (await umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.save)).click();
    //Assert
    await umbracoUi.isSuccessNotificationVisible();
    await expect(page.locator('[title="tab1"]').first()).toBeVisible();
    await expect(page.locator('[title="tab2"]').first()).toBeVisible();
  });

  test('Delete tabs', async ({umbracoUi, umbracoApi, page}) => {
    await createDocTypeWithTabsAndNavigate(umbracoUi, umbracoApi, page);

    // Check if tab is there, else if it wasn't created, this test would always pass
    let tab = await page.locator('[title="aTab 1"]');
    await expect(tab.first()).toBeVisible();

    // Delete a tab
    await page.locator('.btn-reset > [icon="icon-trash"]').first().click();
    await page.locator('.umb-button > .btn').last().click();
    await (await umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.save)).click();
    // Assert
    await umbracoUi.isSuccessNotificationVisible();
    let deletedTab = await page.locator('[title="aTab 1"]');
    await expect(deletedTab.first()).not.toBeVisible();
    // Clean
    await umbracoApi.documentTypes.ensureNameNotExists(tabsDocTypeName);
  });

  test('Delete property in a tab', async ({umbracoUi, umbracoApi, page}) => {
    await umbracoApi.documentTypes.ensureNameNotExists(tabsDocTypeName);
    const tabsDocType = new DocumentTypeBuilder()
        .withName(tabsDocTypeName)
        .withAlias(tabsDocTypeAlias)
        .withAllowAsRoot(true)
        .withDefaultTemplate(tabsDocTypeAlias)
        .addTab()
          .withName('Tab 1')
          .addGroup()
            .withName('Tab group')
              .addUrlPickerProperty()
                .withAlias("urlPicker")
              .done()
              .addContentPickerProperty()
                .withAlias('picker')
              .done()
            .done()
          .done()
        .build();
    await umbracoApi.documentTypes.save(tabsDocType);
    await openDocTypeFolder(umbracoUi, page);
    await page.locator('[aria-label="Delete property"]').last().click();
    await (await umbracoUi.getButtonByLabelKey('actions_delete')).click();
    await (await umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.save)).click()
    // Assert
    await umbracoUi.isSuccessNotificationVisible();
    await expect(await page.locator('[title=urlPicker]')).toBeVisible();
    await expect(await page.locator('[title=picker]')).toHaveCount(0);
  });

  test('Delete group in tab', async ({umbracoUi, umbracoApi, page}) => {
    await umbracoApi.documentTypes.ensureNameNotExists(tabsDocTypeName);
    const tabsDocType = new DocumentTypeBuilder()
        .withName(tabsDocTypeName)
        .withAlias(tabsDocTypeAlias)
        .withAllowAsRoot(true)
        .withDefaultTemplate(tabsDocTypeAlias)
        .addTab()
          .withName('Tab 1')
          .addGroup()
            .withName('Tab group')
            .addUrlPickerProperty()
              .withAlias("urlPicker")
            .done()
          .done()
          .addGroup()
            .withName('Content Picker Group')
            .addContentPickerProperty()
              .withAlias('picker')
            .done()
          .done()
        .done()
        .build();
    await umbracoApi.documentTypes.save(tabsDocType);
    await openDocTypeFolder(umbracoUi, page);
    // Delete group
    await page.locator(':nth-match(.umb-group-builder__group-remove > [icon="icon-trash"], 2)').click();
    await (await umbracoUi.getButtonByLabelKey('actions_delete')).click();
    await (await umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.save)).click()
    // Assert
    await umbracoUi.isSuccessNotificationVisible();
    await expect(await page.locator('[title=picker]')).toBeVisible();
    await expect(await page.locator('[title=urlPicker]')).toHaveCount(0);
  });

  test('Reorders tab', async ({umbracoUi, umbracoApi, page}) => { 
    await umbracoApi.documentTypes.ensureNameNotExists(tabsDocTypeName);

    const tabsDocType = new DocumentTypeBuilder()
        .withName(tabsDocTypeName)
        .withAlias(tabsDocTypeAlias)
        .withAllowAsRoot(true)
        .withDefaultTemplate(tabsDocTypeAlias)
        .addTab()
          .withName('Tab 1')
          .addGroup()
            .withName('Tab group 1')
            .addUrlPickerProperty()
              .withLabel('Url picker 1')
              .withAlias("urlPicker")
            .done()
          .done()
        .done()
        .addTab()
          .withName('Tab 2')
          .addGroup()
            .withName('Tab group 2')
            .addUrlPickerProperty()
              .withLabel('Url picker 2')
              .withAlias("pickerTab 2")
            .done()
          .done()
        .done()
        .addTab()
          .withName('Tab 3')
          .addGroup()
            .withName('Tab group')
            .addUrlPickerProperty()
              .withLabel('Url picker 3')
              .withAlias('pickerTab3')
            .done()
          .done()
        .done()
        .build();

      await umbracoApi.documentTypes.save(tabsDocType);
      await openDocTypeFolder(umbracoUi, page);
      // Check if there are any tabs
      await page.locator('[alias="reorder"]').click();
      // Type order in
      await page.locator('.umb-group-builder__tab-sort-order > .umb-property-editor-tiny').first().fill('3');
      await page.locator('[alias="reorder"]').click();
      // Assert
      await expect(await page.locator('[ui-sortable="sortableOptionsTab"]').locator("xpath=/*[1]")).toHaveAttribute('data-tab-alias', 'aTab 2');
      await expect(await page.locator('[ui-sortable="sortableOptionsTab"]').locator("xpath=/*[2]")).toHaveAttribute('data-tab-alias', 'aTab 3');
      await expect(await page.locator('[ui-sortable="sortableOptionsTab"]').locator("xpath=/*[3]")).toHaveAttribute('data-tab-alias', 'aTab 1');
  });

  test('Reorders groups in a tab', async ({umbracoUi, umbracoApi, page}) => {
    await umbracoApi.documentTypes.ensureNameNotExists(tabsDocTypeName);
    const tabsDocType = new DocumentTypeBuilder()
      .withName(tabsDocTypeName)
      .withAlias(tabsDocTypeAlias)
      .withAllowAsRoot(true)
      .withDefaultTemplate(tabsDocTypeAlias)
      .addTab()
        .withName('Tab 1')
        .addGroup()
          .withName('Tab group 1')
          .addUrlPickerProperty()
            .withLabel('Url picker 1')
            .withAlias("urlPicker")
          .done()
        .done()
        .addGroup()
          .withName('Tab group 2')
          .addUrlPickerProperty()
            .withLabel('Url picker 2')
            .withAlias('urlPickerTwo')
          .done()
        .done()
      .done()
      .build();
    await umbracoApi.documentTypes.save(tabsDocType);
    await openDocTypeFolder(umbracoUi, page);
    await page.locator('[alias="reorder"]').click();
    await page.locator('.umb-property-editor-tiny >> nth=2').fill('1');

    await page.locator('[alias="reorder"]').click();
    await (await umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.save)).click();
    // Assert
    await umbracoUi.isSuccessNotificationVisible();
    await expect(await page.locator('.umb-group-builder__group-title-input >> nth=2')).toHaveAttribute('title', 'aTab 1/aTab group 2');
  });

  test('Reorders properties in a tab', async ({umbracoUi, umbracoApi, page}) => {
    await umbracoApi.documentTypes.ensureNameNotExists(tabsDocTypeName);
    const tabsDocType = new DocumentTypeBuilder()
      .withName(tabsDocTypeName)
      .withAlias(tabsDocTypeAlias)
      .withAllowAsRoot(true)
      .withDefaultTemplate(tabsDocTypeAlias)
      .addTab()
        .withName('Tab 1')
        .addGroup()
          .withName('Tab group')
          .addUrlPickerProperty()
            .withLabel('PickerOne')
            .withAlias("urlPicker")
          .done()
          .addUrlPickerProperty()
            .withLabel('PickerTwo')
            .withAlias('urlPickerTwo')
          .done()
        .done()
      .done()
      .build();
    await umbracoApi.documentTypes.save(tabsDocType);
    await openDocTypeFolder(umbracoUi, page);
    // Reorder
    await page.locator('[alias="reorder"]').click();
    await page.locator('.umb-group-builder__group-sort-value').first().fill('2');
    await page.locator('[alias="reorder"]').click();
    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.save));
    // Assert
    await umbracoUi.isSuccessNotificationVisible();
    await expect(await page.locator('.umb-locked-field__input').last()).toHaveAttribute('title', 'urlPicker');
  });

  test('Tab name cannot be empty', async ({umbracoUi, umbracoApi, page}) => {
    await createDocTypeWithTabsAndNavigate(umbracoUi, umbracoApi, page);
    await page.locator('.umb-group-builder__group-title-input').first().fill("");
    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.save));
    //Assert
    await umbracoUi.isErrorNotificationVisible();
  });

  test('Two tabs cannot have the same name', async ({umbracoUi, umbracoApi, page}) => {
    await umbracoApi.documentTypes.ensureNameNotExists(tabsDocTypeName);
    const tabsDocType = new DocumentTypeBuilder()
      .withName(tabsDocTypeName)
      .withAlias(tabsDocTypeAlias)
      .withAllowAsRoot(true)
      .withDefaultTemplate(tabsDocTypeAlias)
      .addTab()
        .withName('Tab 1')
        .addGroup()
          .withName('Tab group')
          .addUrlPickerProperty()
            .withAlias("urlPicker")
          .done()
        .done()
      .done()
      .build();
    await umbracoApi.documentTypes.save(tabsDocType);
    await openDocTypeFolder(umbracoUi, page);
    // Create a 2nd tab manually
    await page.locator('.umb-group-builder__tabs__add-tab').click();
    await page.locator('ng-form.ng-invalid > .umb-group-builder__group-title-input').fill('Tab 1');
    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.save));
    // Assert
    await umbracoUi.isErrorNotificationVisible();
  });

  test('Group name cannot be empty', async ({umbracoUi, umbracoApi, page}) => {
    await createDocTypeWithTabsAndNavigate(umbracoUi, umbracoApi, page);
    await page.locator('.clearfix > .-placeholder').click();
    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.save));
    // Assert
    await umbracoUi.isErrorNotificationVisible();
  });

  test('Group name cannot have the same name', async ({umbracoUi, umbracoApi, page}) => {
    await createDocTypeWithTabsAndNavigate(umbracoUi, umbracoApi, page);
    await page.locator('.clearfix > .-placeholder').click();
    await page.locator('.umb-group-builder__group-title-input').last().type('Tab group');
    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.save));
    // Assert
    await umbracoUi.isErrorNotificationVisible();
  });

  test('Drag a group into another tab', async ({umbracoUi, umbracoApi, page}) => {
    await umbracoApi.documentTypes.ensureNameNotExists(tabsDocTypeName);
    const tabsDocType = new DocumentTypeBuilder()
      .withName(tabsDocTypeName)
      .withAlias(tabsDocTypeAlias)
      .withAllowAsRoot(true)
      .withDefaultTemplate(tabsDocTypeAlias)
      .addTab()
        .withName('Tab 1')
        .addGroup()
          .withName('Tab group')
          .addUrlPickerProperty()
            .withAlias("urlPicker")
          .done()
        .done()
      .done()
      .addTab()
        .withName('Tab 2')
        .addGroup()
          .withName('Tab group tab 2')
          .addUrlPickerProperty()
            .withAlias('urlPickerTabTwo')
          .done()
        .done()
        .addGroup()
          .withName('Tab group 2')
          .addUrlPickerProperty()
            .withAlias('urlPickerTwo')
          .done()
        .done()
      .done()
      .build();
    await umbracoApi.documentTypes.save(tabsDocType);
    await openDocTypeFolder(umbracoUi, page);
    await page.locator('[alias="reorder"]').click();
    await page.locator('.umb-group-builder__tab').last().click();
    await page.locator('.umb-group-builder__group-title-icon').last().hover();
    await page.mouse.down();
    await page.locator('.umb-group-builder__tab >> nth=1').hover({force: true});
    await page.waitForTimeout(2000);
    await page.mouse.up();
    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.save));
    // Assert
    await umbracoUi.isSuccessNotificationVisible();
    await expect(await page.locator('[title="aTab 1/aTab group 2"]')).toBeVisible();
  });

  test('Drag and drop reorders a tab', async ({umbracoUi, umbracoApi, page}) => {
    await umbracoApi.documentTypes.ensureNameNotExists(tabsDocTypeName);
    const tabsDocType = new DocumentTypeBuilder()
      .withName(tabsDocTypeName)
      .withAlias(tabsDocTypeAlias)
      .withAllowAsRoot(true)
      .withDefaultTemplate(tabsDocTypeAlias)
      .addTab()
        .withName('Tab 1')
        .addGroup()
          .withName('Tab group')
          .addUrlPickerProperty()
            .withAlias("urlPicker")
          .done()
        .done()
      .done()
      .addTab()
        .withName('Tab 2')
        .addGroup()
          .withName('Tab group tab 2')
          .addUrlPickerProperty()
            .withAlias('urlPickerTabTwo')
          .done()
        .done()
        .addGroup()
          .withName('Tab group 2')
          .addUrlPickerProperty()
            .withAlias('urlPickerTwo')
          .done()
        .done()
      .done()
      .build();
    await umbracoApi.documentTypes.save(tabsDocType);
    await openDocTypeFolder(umbracoUi, page);
    await page.locator('[alias="reorder"]').click();

    await page.locator('.umb-group-builder__tab-title-icon >> nth=1').last().hover();
    await page.mouse.down();
    await page.locator('.umb-group-builder__tab >> nth=1').hover({force: true, position: {x: 0, y:10}});
    await page.waitForTimeout(2000);
    await page.mouse.up();
    await page.locator('[alias="reorder"]').click();
    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.save));
    //Assert
    await umbracoUi.isSuccessNotificationVisible();
    await expect(await page.locator('[title="aTab 2"]').first()).toBeVisible();
  });

  test('Drags and drops a property in a tab', async ({umbracoUi, umbracoApi, page}) => {
    await umbracoApi.documentTypes.ensureNameNotExists(tabsDocTypeName);
    const tabsDocType = new DocumentTypeBuilder()
      .withName(tabsDocTypeName)
      .withAlias(tabsDocTypeAlias)
      .withAllowAsRoot(true)
      .withDefaultTemplate(tabsDocTypeAlias)
      .addTab()
        .withName('Tab 1')
        .addGroup()
          .withName('Tab group')
          .addUrlPickerProperty()
            .withAlias("urlPicker")
            .withLabel('UrlPickerOne')
          .done()
        .done()
      .done()
      .addTab()
        .withName('Tab 2')
        .addGroup()
          .withName('Tab group tab 2')
          .addUrlPickerProperty()
            .withAlias('urlPickerTabTwo')
            .withLabel('UrlPickerTabTwo')
          .done()
          .addUrlPickerProperty()
            .withAlias('urlPickerTwo')
            .withLabel('UrlPickerTwo')
          .done()
        .done()
      .done()
      .build();
    await umbracoApi.documentTypes.save(tabsDocType);
    await openDocTypeFolder(umbracoUi, page);
    await page.locator('[alias="reorder"]').click();
    await page.locator('.umb-group-builder__tab').last().click();
    
    // Drag and drop property from tab 2 into tab 1
    await page.locator('.umb-group-builder__property-meta > .flex > .icon >> nth=1').last().hover();
    await page.mouse.down();
    await page.locator('.umb-group-builder__tab >> nth=1').hover({force:true});
    await page.waitForTimeout(500);
    await page.locator('[data-element="group-Tab group"]').hover({force:true});
    await page.mouse.up();
    
    // Stop reordering and save
    await page.locator('[alias="reorder"]').click();
    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.save));
    
    // Assert
    await umbracoUi.isSuccessNotificationVisible();
    await expect(await page.locator('[title="urlPickerTabTwo"]')).toBeVisible();
  });
  
  test('Drags and drops a group and converts to tab', async ({umbracoUi, umbracoApi, page}) => {
    await umbracoApi.documentTypes.ensureNameNotExists(tabsDocTypeName);
    const tabsDocType = new DocumentTypeBuilder()
      .withName(tabsDocTypeName)
      .withAlias(tabsDocTypeAlias)
      .withAllowAsRoot(true)
      .withDefaultTemplate(tabsDocTypeAlias)
      .addTab()
        .withName('Tab 1')
        .addGroup()
          .withName('Tab group')
          .addUrlPickerProperty()
            .withAlias("urlPicker")
            .withLabel('UrlPickerOne')
          .done()
        .done()
        .addGroup()
          .withName('Tab group 2')
          .addUrlPickerProperty()
            .withAlias('urlPickerTwo')
            .withLabel('UrlPickerTwo')
          .done()
        .done()
      .done()
      .addTab()
        .withName('Tab 2')
        .addGroup()
          .withName('Tab group tab 2')
          .addUrlPickerProperty()
            .withAlias('urlPickerTabTwo')
            .withLabel('UrlPickerTabTwo')
          .done()
        .done()
      .done()
      .build();
    await umbracoApi.documentTypes.save(tabsDocType);
    await openDocTypeFolder(umbracoUi, page);
    await page.locator('[alias="reorder"]').click();

    await page.locator('.umb-group-builder__group-title-icon >> nth=1').last().hover();
    await page.mouse.down();
    await page.locator('.umb-group-builder__convert-dropzone').hover({force: true, position: {x: 0, y:10}});
    await page.waitForTimeout(2000);
    await page.mouse.up();
    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.save));
    // Assert
    await umbracoUi.isSuccessNotificationVisible();
    await expect(await page.locator('[title="tabGroup"]').first()).toBeVisible();
  });
});