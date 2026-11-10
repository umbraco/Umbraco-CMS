import {ConstantHelper, test} from '@umbraco/acceptance-test-helpers';
import {expect} from '@playwright/test';

// Seeded by AdditionalSetup/SchemaLockdownComposer.cs
const languageIsoCode = 'da-DK';
const languageName = 'Danish (Denmark)';

test('blocks deleting a language in the backoffice', async ({umbracoUi}) => {
  await umbracoUi.goToBackOffice();
  await umbracoUi.language.goToSection(ConstantHelper.sections.settings);
  await umbracoUi.language.clickLanguagesMenu();

  await test.step('Collection', async () => {
    await umbracoUi.language.isCollectionActionWithNameVisible('Create');
    await umbracoUi.language.isEntityActionVisibleInCollectionRowForName(languageName, 'Delete', false);
  });

  await test.step('Workspace', async () => {
    await umbracoUi.language.clickLanguageByName(languageName);
    await umbracoUi.language.isSaveButtonVisible();
    await umbracoUi.language.isReadOnlyTagVisible(false);
    await umbracoUi.schemaLockdown.isLockedWorkspaceActionVisible(false);
    await umbracoUi.language.isWorkspaceNameEditable();
  });
});

test('blocks deleting a language through the Management API', async ({umbracoApi}) => {
  // Act
  const response = await umbracoApi.delete(`${umbracoApi.baseUrl}${ConstantHelper.apiEndpoints.language}/${languageIsoCode}`);

  // Assert
  expect(response.status()).toBe(403);
  expect(response.headers()['umb-schema-lockdown']).toBe('language:delete');
});
