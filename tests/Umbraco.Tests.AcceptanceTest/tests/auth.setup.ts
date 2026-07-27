import {test as setup} from '@playwright/test';
import {STORAGE_STATE} from '../playwright.config';
import {ApiHelpers} from "@umbraco/acceptance-test-helpers";

// The cookie is the only credential, so an API sign-in seeds storageState just as well as the login
// UI would. The login screen is covered by DefaultConfig/Login/BackOfficeLogin.spec.ts.
setup('authenticate', async ({page}) => {
  const umbracoApi = new ApiHelpers(page);

  await umbracoApi.loginToAdminUser();

  await page.context().storageState({path: STORAGE_STATE});
});
