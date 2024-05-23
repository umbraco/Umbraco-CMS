import {ConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

// Remove smoke tag before merging
test.describe('Redirect Management tests', {tag: '@smoke'}, () => {
  const disableStatus = 'Disabled';
  const enableStatus = 'Enabled';

  test.beforeEach(async ({umbracoApi, umbracoUi}) => {
    await umbracoApi.redirectManagement.setStatus(enableStatus);
    await umbracoUi.goToBackOffice();
  });

  test.afterEach(async ({umbracoApi}) => {
    await umbracoApi.redirectManagement.setStatus(enableStatus);
  });

  test('can disable URL tracker', async ({umbracoApi, umbracoUi}) => {
    // Act
    await umbracoUi.content.goToSection(ConstantHelper.sections.content);
    await umbracoUi.redirectManagement.clickRedirectManagementTab();
    await umbracoUi.redirectManagement.clickDisableURLTrackerButton();
    await umbracoUi.redirectManagement.clickDisableButton();
    await umbracoUi.waitForTimeout(1000);

    // Assert
    const statusData = await umbracoApi.redirectManagement.getStatus();
    expect(statusData.status).toBe(disableStatus);
  });

  test('can re-enable URL tracker', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    await umbracoApi.redirectManagement.setStatus(disableStatus);

    // Act
    await umbracoUi.content.goToSection(ConstantHelper.sections.content);
    await umbracoUi.redirectManagement.clickRedirectManagementTab();
    await umbracoUi.redirectManagement.clickEnableURLTrackerButton();
    await umbracoUi.waitForTimeout(1000);

    // Assert
    const statusData = await umbracoApi.redirectManagement.getStatus();
    expect(statusData.status).toBe(enableStatus);
  });

  test.skip('can search for original URL', async ({umbracoUi}) => {
    // Arrange
    const searchKeyword = '/test-content/';
    // TODO: rename content to add an item in the redirect url management

    // Act
    await umbracoUi.content.goToSection(ConstantHelper.sections.content);
    await umbracoUi.redirectManagement.clickRedirectManagementTab();
    await umbracoUi.redirectManagement.enterOriginalUrl(searchKeyword);
    await umbracoUi.redirectManagement.clickSearchButton();
    
    // Assert
    // TODO: verify the search result
  });

  test.skip('can delete a redirect', async ({umbracoApi, umbracoUi}) => {
    // TODO: implement this tests when front-end is ready
  });
});
