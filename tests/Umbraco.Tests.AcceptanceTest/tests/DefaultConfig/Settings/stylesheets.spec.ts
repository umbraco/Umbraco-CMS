import {expect} from '@playwright/test';
import {ConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {StylesheetBuilder} from "@umbraco/json-models-builders";

test.describe('Stylesheets', () => {

  const name = "TestStylesheet";
  const fileName = name + ".css";

  test.beforeEach(async ({ page, umbracoApi }, testInfo) => {
    await umbracoApi.report.report(testInfo);
    await umbracoApi.login();
  });

  test.afterEach(async ({page, umbracoApi}) => {
    await umbracoApi.stylesheets.ensureNameNotExists(name);
  });

  test('Create new style sheet file', async ({page, umbracoApi, umbracoUi}) => {

    await umbracoUi.goToSection(ConstantHelper.sections.settings);
    await umbracoUi.waitForTreeLoad(ConstantHelper.sections.settings);

    await umbracoUi.clickElement(umbracoUi.getTreeItem("settings", ["Stylesheets"]), {button: "right"});

    await umbracoUi.clickElement(umbracoUi.getContextMenuAction(ConstantHelper.actions.create));
    await page.locator('.menu-label').first().click(); // TODO: Would be better to use something like cy.umbracoContextMenuAction("action-mediaType").click();
    // We have to wait here till everything is loaded, or worker will throw error
    await page.waitForResponse('**/umbraco/lib/ace-builds/src-min-noconflict/worker-css.js');

    // Type name
    await umbracoUi.setEditorHeaderName(name);

    // Save
    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.save));

    // Assert
    await umbracoUi.isSuccessNotificationVisible();
  });
});
