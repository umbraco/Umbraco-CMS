import {ConstantHelper, test} from '@umbraco/acceptance-test-helpers';
import {expect} from '@playwright/test';

// Seeded by AdditionalSetup/SchemaLockdownComposer.cs
const languageIsoCode = 'da-DK';
const languageName = 'Danish (Denmark)';

test('blocks every change to a language in the backoffice', async ({umbracoUi}) => {
  await umbracoUi.goToBackOffice();
  await umbracoUi.language.goToSection(ConstantHelper.sections.settings);
  const sectionUrl = umbracoUi.page.url();
  await umbracoUi.language.clickLanguagesMenu();

  await test.step('Collection', async () => {
    await umbracoUi.language.isLanguageNameVisible(languageName);
    await umbracoUi.language.isCollectionActionWithNameVisible('Create', false);
    await umbracoUi.language.isEntityActionVisibleInCollectionRowForName(languageName, 'Delete', false);
  });

  await test.step('Root', async () => {
    // The root has no actions left to check first, so a sibling's action shows the sidebar's actions have rendered.
    await umbracoUi.language.isInlineEntityActionVisibleForName('Document Types', 'ReloadChildrenOf');
    await umbracoUi.language.isInlineEntityActionVisibleForName('Languages', 'Create', false);
  });

  await test.step('Workspace', async () => {
    await umbracoUi.language.clickLanguageByName(languageName);
    await umbracoUi.language.isReadOnlyTagVisible();
    await umbracoUi.schemaLockdown.isLockedWorkspaceActionVisible();
    await umbracoUi.language.isSaveButtonVisible(false);
    await umbracoUi.language.isWorkspaceNameEditable(false);
  });

  await test.step('Create route', async () => {
    await umbracoUi.page.goto(`${sectionUrl}/workspace/language/create`);
    await umbracoUi.language.isEntityDetailForbiddenVisible();
  });
});

test('blocks every change to a language through the Management API', async ({umbracoApi}) => {
  // Arrange
  const language = await umbracoApi.language.get(languageIsoCode);
  const url = `${umbracoApi.baseUrl}${ConstantHelper.apiEndpoints.language}`;

  // Act
  const createResponse = await umbracoApi.post(url, {});
  const updateResponse = await umbracoApi.put(`${url}/${languageIsoCode}`, language);
  const deleteResponse = await umbracoApi.delete(`${url}/${languageIsoCode}`);

  // Assert
  expect(createResponse.status()).toBe(403);
  expect(createResponse.headers()['umb-schema-lockdown']).toBe('language:create');
  expect(updateResponse.status()).toBe(403);
  expect(updateResponse.headers()['umb-schema-lockdown']).toBe('language:update');
  expect(deleteResponse.status()).toBe(403);
  expect(deleteResponse.headers()['umb-schema-lockdown']).toBe('language:delete');
});
