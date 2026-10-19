import {test as setup} from '@playwright/test';
import {STORAGE_STATE} from '../playwright.config';
import {ApiHelpers, ConstantHelper, UiHelpers} from '@umbraco/acceptance-test-helpers';

// The cookie is the only credential, so an API sign-in seeds storageState just as well as the login
// UI would. The login screen is covered by DefaultConfig/Login/BackOfficeLogin.spec.ts.
setup('authenticate', async ({page}) => {
  const umbracoUi = new UiHelpers(page);
  const umbracoApi = new ApiHelpers(page);

  await umbracoApi.loginToAdminUser();
  // Signing in over the API only seeds the cookie jar - the page itself is still blank, so the
  // back office has to be opened before any of its UI can be asserted on.
  await umbracoUi.goToBackOffice();
  await umbracoUi.login.goToSection(ConstantHelper.sections.settings);
  await page.context().storageState({path: STORAGE_STATE});
});
