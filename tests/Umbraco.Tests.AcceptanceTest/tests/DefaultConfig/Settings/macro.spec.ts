import {ConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {PartialViewMacroBuilder} from "@umbraco/json-models-builders";

test.describe('Macros', () => {

  test.beforeEach(async ({ page, umbracoApi }) => {
    await umbracoApi.login();
  });

  test('Create macro', async ({ page, umbracoApi, umbracoUi }) => {
    const name = "Test macro";
    const partialViewName = "Test partialView";

    await umbracoApi.macros.ensureNameNotExists(name);
    await umbracoApi.partialViews.ensureMacroFileNameNotExists(partialViewName);
    
    const partialViewMacro = new PartialViewMacroBuilder()
      .withName(partialViewName)
      .withContent("@inherits Umbraco.Web.Macros.PartialViewMacroPage")
      .build();
    await umbracoApi.partialViews.save(partialViewMacro);
    
    await umbracoUi.goToSection(ConstantHelper.sections.settings);
    await umbracoUi.clickElement(umbracoUi.getTreeItem("settings", ["Macros"]), {button: "right"});

    // Creates macro
    await umbracoUi.clickElement(umbracoUi.getContextMenuAction(ConstantHelper.actions.create));
    let form = await page.locator('form[name="createMacroForm"]');
    await form.locator('input[name="itemKey"]').type(name);
    await form.locator(".btn-primary").click();
    
    // Adds partial view to macro
    await page.locator('[label="Macro partial view"]').click();
    await page.locator('[data-element="tree-item-' + partialViewName + '.cshtml"]').click();
    
    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.save));
    
    // Assert
    await umbracoUi.isSuccessNotificationVisible();
    
    // Clean up
    await umbracoApi.macros.ensureNameNotExists(name);
    await umbracoApi.partialViews.ensureMacroFileNameNotExists(partialViewName);
  });
});