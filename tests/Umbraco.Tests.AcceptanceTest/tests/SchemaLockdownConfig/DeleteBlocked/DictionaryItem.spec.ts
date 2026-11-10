import {ConstantHelper, test} from '@umbraco/acceptance-test-helpers';
import {expect} from '@playwright/test';

// Seeded by AdditionalSetup/SchemaLockdownComposer.cs
const dictionaryName = 'LockedDictionaryItem';

test('blocks deleting a dictionary item in the backoffice', async ({umbracoUi}) => {
  await umbracoUi.goToBackOffice();
  await umbracoUi.dictionary.goToSection(ConstantHelper.sections.dictionary);

  await test.step('Collection', async () => {
    await umbracoUi.dictionary.isCollectionActionWithNameVisible('Create');
  });

  await test.step('Item actions', async () => {
    await umbracoUi.dictionary.clickActionsMenuForDictionary(dictionaryName);
    await umbracoUi.dictionary.isEntityActionWithNameVisible('MoveTo');
    await umbracoUi.dictionary.isEntityActionWithNameVisible('Delete', false);
    await umbracoUi.page.keyboard.press('Escape');
  });

  await test.step('Workspace', async () => {
    await umbracoUi.dictionary.goToDictionaryItem(dictionaryName);
    await umbracoUi.dictionary.isSaveButtonVisible();
    await umbracoUi.dictionary.isReadOnlyTagVisible(false);
    await umbracoUi.schemaLockdown.isLockedWorkspaceActionVisible(false);
    await umbracoUi.dictionary.isWorkspaceNameEditable();
    await umbracoUi.dictionary.isTranslationEditableForLanguage('da-DK');
  });
});

test('blocks deleting a dictionary item through the Management API', async ({umbracoApi}) => {
  // Arrange
  const dictionaryItem = await umbracoApi.dictionary.getByName(dictionaryName);

  // Act
  const response = await umbracoApi.delete(`${umbracoApi.baseUrl}${ConstantHelper.apiEndpoints.dictionary}/${dictionaryItem.id}`);

  // Assert
  expect(response.status()).toBe(403);
  expect(response.headers()['umb-schema-lockdown']).toBe('dictionary-item:delete');
});
