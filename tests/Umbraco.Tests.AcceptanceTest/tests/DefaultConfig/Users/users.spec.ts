import { expect, Page } from '@playwright/test';
import { test, ApiHelpers, UiHelpers, AliasHelper, ConstantHelper } from '@umbraco/playwright-testhelpers';

test.describe('Users', () => {
  
  const name = "Alice Bobson";
  const email = "alice-bobson@acceptancetest.umbraco";
  const startContentIds = [];
  const startMediaIds = [];
  const userGroups = ["admin"];

  const userData =
    {
      "id": -1,
      "parentId": -1,
      "name": name,
      "username": email,
      "culture": "en-US",
      "email": email,  
      "startContentIds": startContentIds,
      "startMediaIds": startMediaIds,
      "userGroups": userGroups,
      "message": ""
    };
  
  test.beforeEach(async ({ umbracoApi, page }) => {
    // TODO: REMOVE THIS WHEN SQLITE IS FIXED
    // Wait so we don't bombard the API
    await page.waitForTimeout(1000);
    await umbracoApi.login();
  });

  test.afterEach(async({umbracoApi}) => {
    await umbracoApi.users.ensureEmailNotExits(email);
  });
  
  async function createUser(umbracoApi : ApiHelpers){
    let url = process.env.URL + "/umbraco/backoffice/umbracoapi/users/PostCreateUser";
    await umbracoApi.post(url, userData);
  }
  
  test('Create user', async ({umbracoUi, umbracoApi, page}) => {
    const name = "Alice Bobson";
    const email = "alice-bobson@acceptancetest.umbraco";

    await umbracoApi.users.ensureEmailNotExits(email);
    await umbracoUi.goToSection('users');
    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey('user_createUser'));


    await page.locator('input[name="name"]').fill(name);
    await page.locator('input[name="email"]').fill(email);

    await page.locator('.umb-node-preview-add').click();
    await page.locator('.umb-user-group-picker-list-item:nth-child(1) > .umb-user-group-picker__action').click();
    await page.locator('.umb-user-group-picker-list-item:nth-child(2) > .umb-user-group-picker__action').click();
    await page.locator('.btn-success').click();

    await page.locator('.umb-button > .btn > .umb-button__content').click();

    // Assert
    await expect(await umbracoUi.getButtonByLabelKey("user_goToProfile")).toBeVisible();
  });

  test('Update user', async ({umbracoUi, umbracoApi, page}) => {
    // Ensure user doesn't exist
    await umbracoApi.users.ensureEmailNotExits(email);

    //Create user through API
    await createUser(umbracoApi);

    // Go to the user and edit their name
    await umbracoUi.goToSection('users');
    await page.locator(`.umb-user-card__name >> text=${name}`).click();
    await page.locator('#headerName').type('{movetoend}son');
    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.save));

    // Assert save succeeds
    await umbracoUi.isSuccessNotificationVisible();
  });

  test('Delete user', async ({umbracoUi, umbracoApi, page}) => {

    // Ensure user doesn't exist
    await umbracoApi.users.ensureEmailNotExits(email);

    // Create user through API
    await createUser(umbracoApi);

    // Go to the user and delete them
    await umbracoUi.goToSection('users');
    await page.locator(`.umb-user-card__name >> text=${name}`).click();
    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey('user_deleteUser'));
    await page.locator('umb-button[label="Yes, delete"]').click();

    // Assert deletion succeeds
    await umbracoUi.isSuccessNotificationVisible();
  });
});