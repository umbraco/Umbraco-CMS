import {ConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

test.describe('Languages', () => {

  test.beforeEach(async ({page, umbracoApi}) => {
    // TODO: REMOVE THIS WHEN SQLITE IS FIXED
    // Wait so we don't bombard the API
    await page.waitForTimeout(1000);
    await umbracoApi.login();
  });

  test('Creates a language', async ({page, umbracoApi, umbracoUi}) => {
    const language = 'Danish';
    const culture = 'da';

    await umbracoApi.languages.ensureCultureNotExists(culture);

    await umbracoUi.goToSection(ConstantHelper.sections.settings);
    await umbracoUi.waitForTreeLoad(ConstantHelper.sections.settings);

    await umbracoUi.clickElement(umbracoUi.getTreeItem('settings', ['languages']));
    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey('languages_addLanguage'));
    await page.locator('select[name="newLang"]').selectOption({label: language});

    // Save and assert success
    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.save));
    await umbracoUi.isSuccessNotificationVisible();

    // Cleanup
    await umbracoApi.languages.ensureCultureNotExists(culture);
  });

  test('Deletes a language', async ({page, umbracoApi, umbracoUi}) => {
    // Setup
    const language1 = 'da';
    const language2 = 'en-GB';
    await umbracoApi.languages.ensureCultureNotExists(language1);
    await umbracoApi.languages.ensureCultureNotExists(language2);

    await umbracoApi.languages.createLanguage(language1, true, 1);
    await umbracoApi.languages.createLanguage(language2, true, 1);

    // Enter settings section and wait for everything to load
    await umbracoUi.goToSection(ConstantHelper.sections.settings);
    await umbracoUi.waitForTreeLoad(ConstantHelper.sections.settings);

    // Enter language tree and select the language we just created
    await umbracoUi.clickElement(umbracoUi.getTreeItem('settings', ['languages']));
    
    // Assert there are 3 languages
    await expect(await page.locator('tbody > tr')).toHaveCount(3);

    // Delete UK Language
    await page.locator('umb-button[label-key="general_delete"]').last().click();
    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey('contentTypeEditor_yesDelete'));

    // Assert there is only 2 languages
    await expect(await page.locator('tbody > tr')).toHaveCount(2);

    // Cleanup
    await umbracoApi.languages.ensureCultureNotExists(language1);
    await umbracoApi.languages.ensureCultureNotExists(language2);
  });
});
