import {expect} from '@playwright/test';
import {test} from '@umbraco/playwright-testhelpers';

test.describe('Tours', () => {
  const timeout = 60000;
  test.beforeEach(async ({page, umbracoApi}) => {
    // TODO: REMOVE THIS WHEN SQLITE IS FIXED
    // Wait so we don't bombard the API
    await page.waitForTimeout(1000);
    await umbracoApi.login();
    await resetTourData(umbracoApi);
  });

  test.afterEach(async ({page, umbracoApi}) => {
    await resetTourData(umbracoApi);
  });

  async function getPercentage(percentage, timeout, page) {
    await expect(await page.locator('[data-element="help-tours"] .umb-progress-circle', {timeout: timeout})).toContainText(percentage + '%');
  }

  async function resetTourData(umbracoApi) {
    const tourStatus =
      {
        "alias": "umbIntroIntroduction",
        "completed": false,
        "disabled": false
      };

    const response = await umbracoApi.post(process.env.URL + "/umbraco/backoffice/UmbracoApi/CurrentUser/PostSetUserTour", tourStatus)
  }

  async function runBackOfficeIntroTour(percentageComplete, buttonText, timeout, page, umbracoUi) {
    await expect(await page.locator('[data-element="help-tours"]')).toBeVisible();
    await umbracoUi.clickElement(page.locator('[data-element="help-tours"]'));
    await expect(await page.locator('.umb-progress-circle', {timeout: timeout})).toContainText(percentageComplete + '%');


    await page.locator('[data-element="help-tours"]').click();
    await expect(await page.locator('[data-element="tour-umbIntroIntroduction"] .umb-button')).toBeVisible();
    await expect(await page.locator('[data-element="tour-umbIntroIntroduction"] .umb-button')).toContainText(buttonText);
    await umbracoUi.clickElement(await page.locator('[data-element="tour-umbIntroIntroduction"] .umb-button'));

    //act
    await expect(await page.locator('.umb-tour-step', {timeout: timeout})).toBeVisible();
    await expect(await page.locator('.umb-tour-step__footer')).toBeVisible();
    await expect(await page.locator('.umb-tour-step__counter')).toBeVisible();

    for (let i = 1; i < 8; i++) {

      if (i == 4) {
        continue
      }

      await expect(await page.locator('.umb-tour-step__counter')).toContainText(i + '/13');
      await expect(await page.locator('.umb-tour-step__footer .umb-button')).toBeVisible();
      await umbracoUi.clickElement(page.locator('.umb-tour-step__footer .umb-button'));
    }
    await umbracoUi.clickElement(await umbracoUi.getGlobalUser());
    await expect(await page.locator('.umb-tour-step__counter', {timeout: timeout})).toContainText('9/13');
    await expect(await page.locator('.umb-tour-step__footer .umb-button')).toBeVisible();
    await umbracoUi.clickElement(page.locator('.umb-tour-step__footer .umb-button'));
    await expect(await page.locator('.umb-tour-step__counter', {timeout: timeout})).toContainText('10/13');
    await expect(await page.locator('[data-element~="overlay-user"] [data-element="button-overlayClose"]')).toBeVisible();
    await umbracoUi.clickElement(page.locator('[data-element~="overlay-user"] [data-element="button-overlayClose"]'));
    await expect(await page.locator('.umb-tour-step__counter', {timeout: timeout})).toContainText('11/13');
    await umbracoUi.clickElement(await umbracoUi.getGlobalHelp());

    for (let i = 12; i < 13; i++) {
      await expect(await page.locator('.umb-tour-step__counter', {timeout: timeout})).toContainText(i + '/13');
      await expect(await page.locator('.umb-tour-step__footer .umb-button')).toBeVisible();
      await umbracoUi.clickElement(page.locator('.umb-tour-step__footer .umb-button'));
    }
    await expect(await page.locator('.umb-tour-step__footer .umb-button')).toBeVisible();
    await umbracoUi.clickElement(page.locator('.umb-tour-step__footer .umb-button'));
    await expect(await umbracoUi.getGlobalHelp()).toBeVisible();
    await umbracoUi.clickElement(page.locator('[label="Complete"]'));

  }

  test('Backoffice introduction tour should run', async ({page, umbracoApi, umbracoUi}) => {
    // We have to reload this page, as we already get a page context after login
    // before we have reset a users tour data
    await expect(await umbracoUi.getGlobalHelp()).toBeVisible();
    await umbracoUi.clickElement(umbracoUi.getGlobalHelp());
    await runBackOfficeIntroTour(0, 'Start', timeout, page, umbracoUi);

    await expect(await page.locator('[data-element="help-tours"]')).toBeVisible();
    await getPercentage(17, timeout, page);
  });

  test('Backoffice introduction tour should run, then rerun', async ({page, umbracoApi, umbracoUi}) => {
    await expect(await umbracoUi.getGlobalHelp()).toBeVisible();
    await umbracoUi.clickElement(umbracoUi.getGlobalHelp());
    await runBackOfficeIntroTour(0, 'Start', timeout, page, umbracoUi);
    await runBackOfficeIntroTour(17, 'Rerun', timeout, page, umbracoUi);

    await expect(await umbracoUi.getGlobalHelp()).toBeVisible();
    await getPercentage(17, timeout, page);
  });
});