import {ConstantHelper, test} from '@umbraco/playwright-testhelpers';

test.describe('Member Types', () => {
  test.beforeEach(async ({page, umbracoApi}) => {
    // TODO: REMOVE THIS WHEN SQLITE IS FIXED
    // Wait so we don't bombard the API
    await page.waitForTimeout(1000);
    await umbracoApi.login();
  });

  test('Create member type', async ({page, umbracoApi, umbracoUi}) => {
    const name = "Test member type";

    await umbracoApi.memberTypes.ensureNameNotExists(name);

    await umbracoUi.goToSection(ConstantHelper.sections.settings);
    await umbracoUi.waitForTreeLoad(ConstantHelper.sections.settings);

    await umbracoUi.clickElement(umbracoUi.getTreeItem("settings", ["Member Types"]), { button: "right" });

    await umbracoUi.clickElement(umbracoUi.getContextMenuAction("action-create"));

    //Type name
    await umbracoUi.getEditorHeaderName(name);

    await page.locator('[data-element="group-add"]').click();

    await page.locator('.umb-group-builder__group-title-input').type('Group name');
    await page.locator('[data-element="property-add"]').click();
    await page.locator('.editor-label').type('property name');
    await page.locator('[data-element="editor-add"]').click();

    //Search for textstring
    await page.locator('#datatype-search').type('Textstring');

    // Choose first item
    await page.locator('ul.umb-card-grid [title="Textstring"]').click();

    // Save property
    await page.locator('.btn-success').last().click();

    //Save
    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.save));

    //Assert
    await umbracoUi.isSuccessNotificationVisible();

    //Clean up
    await umbracoApi.memberTypes.ensureNameNotExists(name);
  });
});
