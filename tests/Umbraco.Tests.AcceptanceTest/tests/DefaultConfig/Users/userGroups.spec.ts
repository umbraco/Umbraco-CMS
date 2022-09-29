import { expect, Page } from '@playwright/test';
import { test, ApiHelpers, UiHelpers, AliasHelper } from '@umbraco/playwright-testhelpers';
import { UserGroupBuilder } from '@umbraco/json-models-builders';

test.describe('User groups', () => {
  
  async function navigateToUserGroups(umbracoUi : UiHelpers, page : Page) {
    await umbracoUi.goToSection('users');
    await page.locator('[data-element="sub-view-userGroups"]').click();
  }

  test.beforeEach(async ({ umbracoApi, page }) => {
    // TODO: REMOVE THIS WHEN SQLITE IS FIXED
    // Wait so we don't bombard the API
    await page.waitForTimeout(1000);
    await umbracoApi.login();
  });

  test('Create user group', async ({umbracoUi, umbracoApi, page}) => {
    const name = "Test Group";

    await umbracoApi.userGroups.ensureNameNotExits(name);

    await navigateToUserGroups(umbracoUi, page);
    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey("actions_createGroup"));

    //Type name
    await umbracoUi.getEditorHeaderName(name);

    // Assign sections
    await page.locator('.umb-box:nth-child(1) .umb-property:nth-child(1) localize').click();

    await umbracoUi.clickMultiple(page.locator('.umb-tree-item__inner'));
    await page.locator('.btn-success').last().click();

    // Save
    await page.locator('.btn-success').click();

    //Assert
    await umbracoUi.isSuccessNotificationVisible();

    //Clean up
    await umbracoApi.userGroups.ensureNameNotExits(name);
  });

  test('Can delete user group', async ({umbracoUi, umbracoApi, page}) => {

    // Create user group
    const groupName = "Delete user group test"
    await umbracoApi.userGroups.ensureNameNotExits(groupName);

    const userGroup = new UserGroupBuilder()
      .withName(groupName)
      .build();

    await umbracoApi.userGroups.save(userGroup);
    await navigateToUserGroups(umbracoUi, page);

    // Delete the user group
    await page.locator('.umb-table-body > :nth-child(2)').click();
    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey("general_delete"));
    await page.locator('umb-button[alias="overlaySubmit"]').click();

    await umbracoUi.isSuccessNotificationVisible();
    await expect(await page.locator('.umb-table-body')).not.toHaveText(groupName);

    // Clean up
    await umbracoApi.userGroups.ensureNameNotExits(groupName);
  });

  test('Cannot delete required groups', async ({umbracoUi, umbracoApi, page}) => {
    
    await navigateToUserGroups(umbracoUi, page);

    // There's not really a good way to be 100% sure we'll get the admin group, it should be first, but who knows
    // so double check that we actually got the correct one
    const administrators = await page.locator('.umb-table-body > :nth-child(1)');
    await expect(administrators).toContainText('Administrators');
    await administrators.click({force: true});

    const sensitive = await page.locator('.umb-table-body > :nth-child(3)');
    await expect(sensitive).toContainText('Sensitive data');
    await sensitive.click({force: true});

    const translators = await page.locator('.umb-table-body > :nth-child(4)');
    await expect(translators).toContainText('Translators');
    await translators.click({force: true});

    // Now that we've clicked all that we shouldn't be able to delete, ensure that the delete button does not show up 
    let header = await page.locator('.umb-editor-sub-header');
    await expect(header).not.toContainText('Delete');
  });
});