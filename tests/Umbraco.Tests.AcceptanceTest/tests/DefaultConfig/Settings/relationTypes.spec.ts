import {expect} from '@playwright/test';
import {ConstantHelper, test} from '@umbraco/playwright-testhelpers';

test.describe('Relation Types', () => {

  test.beforeEach(async ({page, umbracoApi}) => {
    // TODO: REMOVE THIS WHEN SQLITE IS FIXED
    // Wait so we don't bombard the API
    await page.waitForTimeout(1000);
    await umbracoApi.login();
  });

  test('Create relation type', async ({page, umbracoApi, umbracoUi}) => {
    const name = "Test relation type";

    await umbracoApi.relationTypes.ensureNameNotExists(name);

    await umbracoUi.goToSection(ConstantHelper.sections.settings);
    await umbracoUi.waitForTreeLoad(ConstantHelper.sections.settings);

    await umbracoUi.clickElement(umbracoUi.getTreeItem("settings", ["Relation Types"]), {button: "right"});

    await umbracoUi.clickElement(umbracoUi.getContextMenuAction(ConstantHelper.actions.create));

    const form = await page.locator('form[name="createRelationTypeForm"]');
    
    await form.locator('input[name="relationTypeName"]').type(name);
    await form.locator('[name="relationType-direction"] input').first().click({force: true});
    await page.selectOption('select[name="relationType-parent"]', {label: "Document"});
    await page.selectOption('select[name="relationType-child"]', {label: "Media"});
    await form.locator('[name="relationType-isdependency"]').last().click({force: true});
    await form.locator('.btn-primary').click();
    
    await page.waitForNavigation();

    expect(page.url()).toContain("#/settings/relationTypes/edit/");

    //Clean up
    await umbracoApi.relationTypes.ensureNameNotExists(name);
  });
});
