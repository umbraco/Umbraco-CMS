import {test, ApiHelpers, UiHelpers, ConstantHelper} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

test.describe('Media types', () => {

  test.beforeEach(async ({ page, umbracoApi }) => {
    // TODO: REMOVE THIS WHEN SQLITE IS FIXED
    // Wait so we don't bombard the API
    await page.waitForTimeout(1000);
    await umbracoApi.login();
  });

  test('Create media type', async ({ page, umbracoApi, umbracoUi }) => {

    const name = "Test media type";
    await umbracoApi.mediaTypes.ensureNameNotExists(name);
    
    // Navigate to creation
    await umbracoUi.goToSection(ConstantHelper.sections.settings);
    await umbracoUi.waitForTreeLoad(ConstantHelper.sections.settings);
    await umbracoUi.clickElement(umbracoUi.getTreeItem("settings", ["Media Types"]), {button: "right"})
    await umbracoUi.clickElement(umbracoUi.getContextMenuAction(ConstantHelper.actions.create));
    await page.locator('.menu-label localize[key="content_mediatype"]').click();


    // Fill out data
    await umbracoUi.getEditorHeaderName(name);

    await page.locator('[data-element="group-add"]').click();

    await page.locator('.umb-group-builder__group-title-input').fill('Group name');
    await page.locator('[data-element="property-add"]').click();
    await page.locator('.editor-label').fill('property name');
    await page.locator('[data-element="editor-add"]').click();

    // Search for textstring
    await page.locator('#datatype-search').fill('Textstring');

    // Choose first item
    await page.locator('ul.umb-card-grid [title="Textstring"]').first().click();

    // Save property
    await page.locator('.btn-success').last().click();

    // Save
    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.save))

    // Assert
    await umbracoUi.isSuccessNotificationVisible();

    // Clean up
    await umbracoApi.mediaTypes.ensureNameNotExists(name);
  });
});