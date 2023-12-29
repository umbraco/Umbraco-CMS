import {ConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

test.describe('Relation types tests', () => {
  const relationTypeName = 'Test Relation Type';

  test.beforeEach(async ({umbracoUi}) => {
    await umbracoUi.goToBackOffice();
    await umbracoUi.relationType.goToSettingsTreeItem('Relation Types');
  });

  test.afterEach(async ({umbracoApi}) => {
    await umbracoApi.relationType.ensureNameNotExists(relationTypeName);
  });

  test('can create a relation type', async ({umbracoApi, umbracoUi}) => {

  });

  test('can edit a relation type', async ({umbracoApi, umbracoUi}) => {

  });

  test('can show relation', async ({umbracoApi, umbracoUi}) => {

  });


});
