import {ConstantHelper, test} from '@umbraco/acceptance-test-helpers';
import {expect} from '@playwright/test';

// Seeded by AdditionalSetup/SchemaLockdownComposer.cs
const languageIsoCode = 'da-DK';
const languageName = 'Danish (Denmark)';

test('blocks updating a language in the backoffice', async ({umbracoUi}) => {
  await umbracoUi.goToBackOffice();
  await umbracoUi.language.goToSection(ConstantHelper.sections.settings);
  await umbracoUi.language.clickLanguagesMenu();

  await test.step('Collection', async () => {
    await umbracoUi.language.isCollectionActionWithNameVisible('Create');
    await umbracoUi.language.isEntityActionVisibleInCollectionRowForName(languageName, 'Delete');
  });

  await test.step('Root', async () => {
    await umbracoUi.language.isInlineEntityActionVisibleForName('Languages', 'Create');
  });

  await test.step('Workspace', async () => {
    await umbracoUi.language.clickLanguageByName(languageName);
    await umbracoUi.language.isReadOnlyTagVisible();
    await umbracoUi.schemaLockdown.isLockedWorkspaceActionVisible();
    await umbracoUi.language.isSaveButtonVisible(false);
    await umbracoUi.language.isWorkspaceNameEditable(false);
    await umbracoUi.schemaLockdown.clickLockedWorkspaceActionButton();
    await umbracoUi.language.doesModalHaveText('Changes to this item are locked in this environment.');
  });
});

test('blocks updating a language through the Management API', async ({umbracoApi}) => {
  // Arrange
  const language = await umbracoApi.language.get(languageIsoCode);

  // Act
  const response = await umbracoApi.put(`${umbracoApi.baseUrl}${ConstantHelper.apiEndpoints.language}/${languageIsoCode}`, language);

  // Assert
  expect(response.status()).toBe(403);
  expect(response.headers()['umb-schema-lockdown']).toBe('language:update');
});
