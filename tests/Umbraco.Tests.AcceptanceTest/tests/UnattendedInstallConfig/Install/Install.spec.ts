// To be able to test different databases, we need to set an additional UnattendenInstallConfig up because we would have to start from scratch, otherwise we would be using the same database.
import {expect} from '@playwright/test';
import {ConstantHelper, test} from '@umbraco/playwright-testhelpers';


test('test', async ({umbracoApi, umbracoUi}) => {
  // Arrange
});
