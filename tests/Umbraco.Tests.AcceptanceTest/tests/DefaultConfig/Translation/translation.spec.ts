import { expect } from '@playwright/test';
import {ConstantHelper, test} from '@umbraco/playwright-testhelpers';

test.describe('Translation', () => {

  test.beforeEach(async ({ page, umbracoApi }, testInfo) => {
    await umbracoApi.report.report(testInfo);
    await umbracoApi.login();
  });

  const dictionaryKey = "Test Key";
  const dictionaryKey2 = "Test Key 2";

  test('Creates a dictionary key', async ({umbracoUi, umbracoApi, page}) => {
    
    await umbracoApi.users.ensureUserBelongsToGroup("translator");
    await umbracoUi.goToSection(ConstantHelper.sections.translation);
    await umbracoApi.translation.ensureKeyNotExists(dictionaryKey);
    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey('dictionary_createNew'));

    let form = await page.locator('form[name="createDictionaryForm"]');
    await form.locator('input[name="itemKey"]').fill(dictionaryKey);
    await form.locator('.umb-button > .btn > .umb-button__content', { hasText: "Create" }).click();
    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.save))

    // Assert
    await umbracoUi.isSuccessNotificationVisible();

  });
  test('Create a 2nd dictionary key and filter the keys', async ({umbracoUi, umbracoApi, page}) => {

    await umbracoApi.users.ensureUserBelongsToGroup("translator");
    await umbracoUi.goToSection(ConstantHelper.sections.translation);
    await umbracoApi.translation.ensureKeyNotExists(dictionaryKey2);
    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey('dictionary_createNew'));

    let form = await page.locator('form[name="createDictionaryForm"]');
    await form.locator('input[name="itemKey"]').fill(dictionaryKey2);
    await form.locator('.umb-button > .btn > .umb-button__content', { hasText: "Create" }).click();
    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.save))

    // Assert
    await umbracoUi.isSuccessNotificationVisible();
    await page.goBack();

     // Action
    await page.getByPlaceholder('Type to filter...').fill('2');

    // Assert
    await expect(page.locator(`//button[text()='${dictionaryKey2}']`)).toBeVisible();
    // expect the first button not to be visible.
    await expect(page.locator(`//button[text()='${dictionaryKey}']`)).not.toBeVisible();
  
    // Clean up
    await umbracoApi.translation.ensureKeyNotExists(dictionaryKey2);
  });

  test('Expect icon to update when translation is added', async ({umbracoUi, umbracoApi, page}) => {

    await umbracoApi.users.ensureUserBelongsToGroup("translator");
    await umbracoUi.goToSection(ConstantHelper.sections.translation);
    await page.locator(`//button[text()='${dictionaryKey}']`).click();

    // Action
    await page.locator('[id="en-US"]').fill('Hello World');
    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.save));
    await umbracoUi.isSuccessNotificationVisible();
    await page.goBack();

     //Assert
    await expect(page.locator('button:has-text("Has translation")')).toBeVisible();

    // Clean up
    await umbracoApi.translation.ensureKeyNotExists(dictionaryKey);
    await umbracoApi.translation.ensureKeyNotExists(dictionaryKey2);
    await page.close();
  });
});