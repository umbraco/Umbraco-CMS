import {ConstantHelper, test} from '@umbraco/playwright-testhelpers';

test.describe('Packages', () => {

  test.beforeEach(async ({page, umbracoApi}) => {
    // TODO: REMOVE THIS WHEN SQLITE IS FIXED
    // Wait so we don't bombard the API
    await page.waitForTimeout(1000);
    await umbracoApi.login();
  });

  test('Create member', async ({page, umbracoApi, umbracoUi}) => {
    const name = "Alice Bobson";
    const email = "alice-bobson@acceptancetest.umbraco";
    const password = "$AUlkoF*St0kgPiyyVEk5iU5JWdN*F7&";
    const passwordTimeout = 20000;
    await umbracoApi.members.ensureEmailNotExists(email);
    await umbracoUi.goToSection(ConstantHelper.sections.member);
    await umbracoUi.clickElement(umbracoUi.getTreeItem("member", ["Members"]), { button: "right"});
    await umbracoUi.clickElement(umbracoUi.getContextMenuAction(ConstantHelper.actions.create));
    await umbracoUi.clickElement(page.locator('.menu-label').first());

    await umbracoUi.setEditorHeaderName(name);
    await page.locator('[data-element="sub-view-umbMembership"]').click();
    
    await page.locator('input#_umb_login').type(email);
    await page.locator('input#_umb_email').type(email);
    await page.locator('input#password').type(password, { timeout: passwordTimeout });
    await page.locator('input#confirmPassword').type(password, { timeout: passwordTimeout });
    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.save));
    
    // Assert
    await umbracoUi.isSuccessNotificationVisible();
    
    // Clean up
    await umbracoApi.members.ensureEmailNotExists(email);
  });
});
