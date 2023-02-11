import {ConstantHelper, test} from '@umbraco/playwright-testhelpers';

test.describe('Translation', () => {

  test.beforeEach(async ({page, umbracoApi}) => {
    // TODO: REMOVE THIS WHEN SQLITE IS FIXED
    // Wait so we don't bombard the API
    await page.waitForTimeout(1000);
    await umbracoApi.login();
  });

  test('Creates a dictionary key', async ({umbracoUi, umbracoApi, page}) => {
    const dictionaryKey = "Test Key";

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

    // Clean up
    await umbracoApi.translation.ensureKeyNotExists(dictionaryKey);
  });
});
