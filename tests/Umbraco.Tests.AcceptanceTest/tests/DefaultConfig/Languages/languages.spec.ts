import {ConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

test.describe('Languages', () => {

  test.beforeEach(async ({ page, umbracoApi }, testInfo) => {
    await umbracoApi.report.report(testInfo);
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
    // await expect(umbracoApi.languages.exists(culture)).toBe(true);

    const doesExistDA = await umbracoApi.languages.exists(culture);
    await expect(doesExistDA).toBe(true);

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

    // Assert that the 2 languages exists
    // DA
    let doesExistDA = await umbracoApi.languages.exists(language1);
    await expect(doesExistDA).toBe(true);
    // EN
    let doesExistEN = await umbracoApi.languages.exists(language2);
    await expect(doesExistEN).toBe(true);

    // Delete UK Language
    await page.getByRole('button', {name: language2}).locator('[label-key=' + ConstantHelper.buttons.delete + ']').click();
    await umbracoUi.clickDataElementByElementName('button-overlaySubmit');

    // Assert the da language still exists and that the uk is deleted
    // DA
    doesExistDA = await umbracoApi.languages.exists(language1);
    await expect(doesExistDA).toBe(true);
    // EN
    await expect(page.getByRole('button', {name: language2})).not.toBeVisible();
    doesExistEN = await umbracoApi.languages.exists(language2);
    await expect(doesExistEN).toBe(false);

    // Cleanup
    await umbracoApi.languages.ensureCultureNotExists(language1);
  });
});
