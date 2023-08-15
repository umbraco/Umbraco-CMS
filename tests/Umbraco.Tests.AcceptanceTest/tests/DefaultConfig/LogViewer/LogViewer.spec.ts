import {test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";
import base64url from "base64url";



const crypto = require('crypto')


test.describe('Log Viewer tests', () => {

  test.beforeEach(async ({page, umbracoApi}, testInfo) => {
    // await console.log(await umbracoApi.testToken());
  });

  test('can search in log viewer', async ({page, umbracoApi, umbracoUi, request}) => {

    const code_verifier = "1512i5k2fwqwfq282942jf9jefjefijsefjf";

    var hash = crypto.createHash('sha256').update(code_verifier).digest();
    var code_challenge = base64url.encode(hash);


    const reponsetest = await request.get(process.env.URL + '/umbraco/management/api/v1.0/security/back-office/authorize', {
      headers:{
        content_type: "application/x-www-form-urlencoded"
      },

      params:{
        response_type: "code",
        client_id: "umbraco-swagger",
        redirect_uri: "https://localhost:44331/umbraco/swagger/oauth2-redirect.html",
        code_challenge: code_challenge,
        code_challenge_method: "S256",
      }
    });



    await request.post(process.env.URL + '/umbraco/management/api/v1.0/security/back-office/token', {
      headers: {
        'content-type': 'application/x-www-form-urlencoded'

      },
      form:{
        grant_type: 'authorization_code',
        code: "",
        client_id: "umbraco-swagger",
        code_verifier: code_verifier
      }
    });

    //
    //
    // const telemetryLevel = "Minimal";
    // await umbracoApi.telemetry.setLevel(telemetryLevel);
    //
    // await page.goto(umbracoApi.baseUrl + '/umbraco');
    // await umbracoUi.goToSection('settings');
    //
    // // await page.getByLabel('Log Viewer').click();
    // await umbracoUi.clickDataElement('tree-item-logViewer');
    //
    // // await page.getByRole('tab', {name: 'Search'}).click({force: true});
    //
    // await page.pause();
    // await page.getByRole('button', {name: 'All Logs', exact: true }).click();
    //
    //
    // // After TestIDs are added, please for the love of everything, update this locator
    // await page.getByPlaceholder('Search logs...').fill(telemetryLevel);
    //
    // // Assert
    // // Checks if there is a log with the telemetry level to minimal
    // await expect(page.getByRole('group').first()).toContainText('Telemetry level set to "' + telemetryLevel + '"');
  });

  test('can change the search log level', async ({page, umbracoApi, umbracoUi}) => {
    await page.goto(umbracoApi.baseUrl + '/umbraco');
    await umbracoUi.goToSection('Settings');

    await page.getByLabel('Log Viewer').click();
    await page.getByRole('tab', {name: 'Search'}).click({force: true});

    await page.getByRole('button', {name: 'Select log levels'}).click();
    await page.locator('.log-level-menu-item').getByText('Information').click();

    // Assert
    // Checks if the search log level was updated
    await expect(await page.locator('.log-level-button-indicator', {hasText: 'Information'})).toBeVisible();
  });

  test('can add a saved search', async ({page, umbracoApi, umbracoUi}) => {
    const searchName = 'Test';
    const search = 'Acquiring MainDom';

    await umbracoApi.logViewer.deleteSavedSearch(searchName);

    await page.goto(umbracoApi.baseUrl + '/umbraco');
    await umbracoUi.goToSection('Settings');

    await page.getByLabel('Log Viewer').click();
    await page.getByRole('tab', {name: 'Search'}).click({force: true});
    await page.getByPlaceholder('Search logs...').fill(search);
    await page.getByLabel("Save search").click({force: true});

    await page.getByLabel("Search name").fill(searchName);
    await page.locator('uui-dialog-layout').getByLabel("Save search").click();

    // Assert
    // Checks if the saved search is visible in the UI
    await page.getByRole('tab', {name: 'Overview'}).click({force: true});
    await expect(await page.locator('#saved-searches').getByLabel(searchName)).toBeVisible();

    await expect(await umbracoApi.logViewer.doesSavedSearchExist(searchName)).toBeTruthy();

    // Clean
    await umbracoApi.logViewer.deleteSavedSearch(searchName);
  });

  test('can use a saved search', async ({page, umbracoApi, umbracoUi}) => {
    const searchName = 'Test';
    const search = 'Acquiring MainDom';

    await umbracoApi.logViewer.deleteSavedSearch(searchName);

    await umbracoApi.logViewer.createSavedSearch(searchName, search);

    await page.goto(umbracoApi.baseUrl + '/umbraco');
    await page.waitForURL(umbracoApi.baseUrl + '/umbraco');

    await umbracoUi.goToSection('Settings');

    await page.getByRole('tab', {name: 'Settings'}).click();

    await page.getByLabel('Log Viewer').click();

    await page.locator('#saved-searches').getByLabel(searchName).click();

    // Assert
    await expect(await page.getByPlaceholder('Search logs...')).toHaveValue(search);

    // Clean
    await umbracoApi.logViewer.deleteSavedSearch(searchName);
  });

  // Do we want this test?
  test('can update date', async ({page, umbracoApi, umbracoUi}) => {
    const searchName = 'Test';
    const search = 'Acquiring MainDom';

    await umbracoApi.logViewer.deleteSavedSearch(searchName);

    await umbracoApi.logViewer.createSavedSearch(searchName, search);

    await page.goto(umbracoApi.baseUrl + '/umbraco');
    await page.waitForURL(umbracoApi.baseUrl + '/umbraco');

    await umbracoUi.goToSection('Settings');

    await page.getByRole('tab', {name: 'Settings'}).click();

    await page.getByLabel('Log Viewer').click();

    await page.locator('#start-date').fill("2023-07-01");

    await page.pause();
    // await page.locator('#saved-searches').getByLabel(searchName).click();
    //
    // // Assert
    // await expect(await page.getByPlaceholder('Search logs...')).toHaveValue(search);
    //
    // // Clean
    // await umbracoApi.logViewer.deleteSavedSearch(searchName);
  });
});
