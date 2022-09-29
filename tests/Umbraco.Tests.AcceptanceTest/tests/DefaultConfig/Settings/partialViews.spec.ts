import {expect} from '@playwright/test';
import {ConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {PartialViewBuilder} from "@umbraco/json-models-builders";

test.describe('Partial Views', () => {

  test.beforeEach(async ({page, umbracoApi}) => {
    // TODO: REMOVE THIS WHEN SQLITE IS FIXED
    // Wait so we don't bombard the API
    await page.waitForTimeout(1000);
    await umbracoApi.login();
  });

  async function openPartialViewsCreatePanel(page, umbracoUi) {
    await navigateToSettings(page, umbracoUi)
    await umbracoUi.clickElement(umbracoUi.getTreeItem("settings", ["Partial Views"]), {button: "right"});
  }

  async function navigateToSettings(page, umbracoUi) {
    await umbracoUi.goToSection(ConstantHelper.sections.settings);
    await umbracoUi.waitForTreeLoad(ConstantHelper.sections.settings);
  }

  test('Create new empty partial view', async ({page, umbracoApi, umbracoUi}) => {
    const name = "TestPartialView";
    const fileName = name + ".cshtml";

    await umbracoApi.partialViews.ensureNameNotExists(fileName);

    await openPartialViewsCreatePanel(page, umbracoUi);

    await umbracoUi.clickElement(umbracoUi.getContextMenuAction(ConstantHelper.actions.create));
    await page.locator('.menu-label localize[key="create_newEmptyPartialView"]').click();

    //Type name
    await umbracoUi.setEditorHeaderName(name);

    //Save
    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.save));

    //Assert
    await umbracoUi.isSuccessNotificationVisible();

    //Clean up
    await umbracoApi.partialViews.ensureNameNotExists(fileName);
  });  
  
  test('Create partial view from snippet', async ({page, umbracoApi, umbracoUi}) => {
    const name = "TestPartialViewFromSnippet";
    const fileName = name + ".cshtml";

    await umbracoApi.partialViews.ensureNameNotExists(fileName);
    
    await openPartialViewsCreatePanel(page, umbracoUi);

    await umbracoUi.clickElement(umbracoUi.getContextMenuAction("action-create"));
    await page.locator('.menu-label >> nth=1').click();
    // Select snippet
    await page.locator('.menu-label >> nth=2').click();

    // Type name
    await umbracoUi.setEditorHeaderName(name);

    // Save
    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.save));
    
    // Assert
    await umbracoUi.isSuccessNotificationVisible();

    // Clean up
    await umbracoApi.partialViews.ensureNameNotExists(fileName);
  });  
  
  test('Partial view with no name', async ({page, umbracoApi, umbracoUi}) => {
    await openPartialViewsCreatePanel(page, umbracoUi);

    await umbracoUi.clickElement(umbracoUi.getContextMenuAction("action-create"));
    await page.locator('.menu-label localize[key="create_newEmptyPartialView"]').click();

    // The test would fail intermittently, most likely because the editor didn't have time to load
    // This should ensure that the editor is loaded and the test should no longer fail unexpectedly.
    await expect(await page.locator('.ace_content')).toBeDefined();

    // Click save
    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.save));

    // Asserts
    await umbracoUi.isErrorNotificationVisible();
  });  
  
  test('Delete partial view', async ({page, umbracoApi, umbracoUi}) => {
    const name = "TestDeletePartialView";
    const fileName = name + ".cshtml";

    await umbracoApi.partialViews.ensureNameNotExists(fileName);

    // Build and save partial view
    const partialView = new PartialViewBuilder()
      .withName(name)
      .withContent("@inherits UUmbraco.Cms.Web.Common.Views.UmbracoViewPage")
      .build();

    await umbracoApi.partialViews.save(partialView);

    await navigateToSettings(page, umbracoUi);

    // Delete partial view
    await umbracoUi.clickElement(umbracoUi.getTreeItem("settings", ["Partial Views", fileName]), {button: "right"});
    await umbracoUi.clickElement(umbracoUi.getContextMenuAction(ConstantHelper.actions.delete));
    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.ok));

    // Assert
    await expect(await page.locator("body", { hasText: fileName})).not.toBeVisible();

    // Clean
    await umbracoApi.partialViews.ensureNameNotExists(fileName);
  });  
  
  test('Edit partial view', async ({page, umbracoApi, umbracoUi}) => {
    const name = 'EditPartialView';
    const fileName = name + ".cshtml";

    await umbracoApi.partialViews.ensureNameNotExists(fileName);

    const partialView = new PartialViewBuilder()
      .withName(name)
      .withContent("@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage\n")
      .build();

    await umbracoApi.partialViews.save(partialView);

    await navigateToSettings(page, umbracoUi);
    // Open partial view
    await umbracoUi.clickElement(umbracoUi.getTreeItem("settings", ["Partial Views", fileName]));
    // Edit
    await page.locator('.ace_text-input').type("var num = 5;");
    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.save));

    // Assert
    await umbracoUi.isSuccessNotificationVisible();
    // Clean
    await umbracoApi.partialViews.ensureNameNotExists(fileName);
  });
});
