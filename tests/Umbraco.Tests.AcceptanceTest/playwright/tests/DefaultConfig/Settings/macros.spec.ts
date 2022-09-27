import {ConstantHelper, test} from '../../../lib';
import {expect} from "@playwright/test";

test.describe('Macros', () => {

  test.beforeEach(async ({ page, umbracoApi }) => {
    await umbracoApi.login();
  });

  test('Create macro', async ({ page, umbracoApi, umbracoUi }) => {

    const name = "Test macro";

    await umbracoApi.macros.ensureNameNotExists(name);

    await umbracoUi.goToSection(ConstantHelper.sections.settings);
    await umbracoUi.waitForTreeLoad(ConstantHelper.sections.settings);
    
    await umbracoUi.clickElement(umbracoUi.getTreeItem("settings", ["Macros"]), {button: "right"});
    
    await umbracoUi.clickElement(umbracoUi.getContextMenuAction(ConstantHelper.actions.create));

    let form = await page.locator('form[name="createMacroForm"]');
    await form.locator('input[name="itemKey"]').type(name);
    await form.locator(".btn-primary").click();
    
    // Wait for navigation, then Assert
    await page.waitForNavigation();
    expect(page.url()).toContain('#/settings/macros/edit/');

    //Clean up
    await umbracoApi.macros.ensureNameNotExists(name);
  });
});