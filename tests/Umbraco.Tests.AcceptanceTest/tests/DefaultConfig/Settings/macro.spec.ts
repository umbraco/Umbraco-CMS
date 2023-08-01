import {ConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {PartialViewMacroBuilder} from "@umbraco/json-models-builders";

test.describe('Macros', () => {

  test.beforeEach(async ({ page, umbracoApi }, testInfo) => {
    await umbracoApi.report.report(testInfo);
    await umbracoApi.login();
  });

  test('Create macro', async ({ page, umbracoApi, umbracoUi }) => {
    const name = "Test macro";
    const partialViewMacroFileName = "Test_partialView";

    await umbracoApi.macros.ensureNameNotExists(name);
    await umbracoApi.partialViews.ensureMacroFileNameNotExists(partialViewMacroFileName + ".cshtml");
    
    const partialViewMacro = new PartialViewMacroBuilder()
      .withName(partialViewMacroFileName)
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
    await page.locator('[data-element="property-label-macroPartialViewPickerProperty"]').click();
    await page.locator('[data-element="tree-item-' + partialViewMacroFileName + '.cshtml"]').click();
    
    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.save));
    
    // Assert
    await umbracoUi.isSuccessNotificationVisible();
    
    // Clean up
    await umbracoApi.macros.ensureNameNotExists(name);
    await umbracoApi.partialViews.ensureMacroFileNameNotExists(partialViewMacroFileName + ".cshtml");
  });
});