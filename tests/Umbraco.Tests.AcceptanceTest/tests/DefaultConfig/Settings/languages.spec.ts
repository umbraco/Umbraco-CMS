import {test, ApiHelpers, UiHelpers, ConstantHelper} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

test.describe('Languages', () => {
  test.beforeEach(async ({ page, umbracoApi }) => {
    // TODO: REMOVE THIS WHEN SQLITE IS FIXED
    // Wait so we don't bombard the API
    await page.waitForTimeout(1000);
    await umbracoApi.login();
  });

  test('Can add language', async ({ page, umbracoApi, umbracoUi }) => {
    // For some reason the languages to chose from seems to be translated differently than normal, as an example:
    // My system is set to EN (US), but most languages are translated into Danish for some reason
    // Aghem seems untranslated though?
    const name = "Aghem"; // Must be an option in the select box

    await umbracoApi.languages.ensureNameNotExists(name);

    await umbracoUi.goToSection(ConstantHelper.sections.settings);
    await umbracoUi.waitForTreeLoad(ConstantHelper.sections.settings);

    await umbracoUi.clickElement(umbracoUi.getTreeItem("settings", ["Languages"]));
    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey("languages_addLanguage"));

    await page.locator('select[name="newLang"]').selectOption({label: name});
    await page.locator('.btn-success').click();

    await umbracoUi.isSuccessNotificationVisible();

    await umbracoApi.languages.ensureNameNotExists(name);
  });

  test('Deletes language', async ({ page, umbracoApi, umbracoUi }) => {
    
    // Setup
    const language1 = 'da';
    const language2 = 'en-GB';
    await umbracoApi.languages.ensureCultureNotExists(language1);
    await umbracoApi.languages.ensureCultureNotExists(language2);

    await umbracoApi.languages.createLanguage(language1, true, 1);
    await umbracoApi.languages.createLanguage(language2, true, 1);
    
    //Enter settings section and wait for everything to load
    await umbracoUi.goToSection(ConstantHelper.sections.settings);
    await umbracoUi.waitForTreeLoad(ConstantHelper.sections.settings);

    // Enter language tree and select the language we just created
    await umbracoUi.clickElement(umbracoUi.getTreeItem('settings', ['Languages']));

    // Assert there are 3 languages
    let languages = page.locator('tbody > tr');
    await expect(languages).toHaveCount(3);

    // Delete UK Language
    await page.locator('umb-button[label-key="general_delete"]').last().click();
    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey('contentTypeEditor_yesDelete'));

    // Assert there is only 2 languages
    await expect(page.locator('tbody > tr')).toHaveCount(2);

    // Cleanup
    await umbracoApi.languages.ensureCultureNotExists(language1);
    await umbracoApi.languages.ensureCultureNotExists(language2);
  });
});
