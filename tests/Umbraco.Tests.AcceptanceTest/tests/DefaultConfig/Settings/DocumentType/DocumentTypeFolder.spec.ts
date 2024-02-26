import {ConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {expect} from '@playwright/test';

test.describe('Document Type Folder tests', () => {
  const documentName = 'TestDocumentTypeFolder';

  test.beforeEach(async ({umbracoUi, umbracoApi}) => {
    await umbracoUi.goToBackOffice();

  });

  test.afterEach(async ({umbracoApi}) => {
  });

  test('can create a empty document type folder', async ({umbracoApi, umbracoUi}) => {

  });
});
