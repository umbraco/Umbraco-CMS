import {ConstantHelper, test} from '@umbraco/acceptance-test-helpers';
import {expect} from '@playwright/test';

// Seeded by AdditionalSetup/SchemaLockdownComposer.cs
const languageName = 'Danish (Denmark)';

test('blocks creating a language in the backoffice', async ({umbracoUi}) => {
  await umbracoUi.goToBackOffice();
  await umbracoUi.language.goToSection(ConstantHelper.sections.settings);
  const sectionUrl = umbracoUi.page.url();
  await umbracoUi.language.clickLanguagesMenu();

  await test.step('Collection', async () => {
    await umbracoUi.language.isEntityActionVisibleInCollectionRowForName(languageName, 'Delete');
    await umbracoUi.language.isCollectionActionWithNameVisible('Create', false);
  });

  await test.step('Root', async () => {
    await umbracoUi.language.isInlineEntityActionVisibleForName('Languages', 'Create', false);
  });

  await test.step('Workspace', async () => {
    await umbracoUi.language.clickLanguageByName(languageName);
    await umbracoUi.language.isSaveButtonVisible();
    await umbracoUi.language.isReadOnlyTagVisible(false);
    await umbracoUi.schemaLockdown.isLockedWorkspaceActionVisible(false);
    await umbracoUi.language.isWorkspaceNameEditable();
  });

  await test.step('Create route', async () => {
    await umbracoUi.page.goto(`${sectionUrl}/workspace/language/create`);
    await umbracoUi.language.isEntityDetailForbiddenVisible();
  });
});

test('blocks creating a language through the Management API', async ({umbracoApi}) => {
  // Act
  const response = await umbracoApi.post(`${umbracoApi.baseUrl}${ConstantHelper.apiEndpoints.language}`, {});

  // Assert
  expect(response.status()).toBe(403);
  expect(response.headers()['umb-schema-lockdown']).toBe('language:create');
});
