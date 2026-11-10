import {ConstantHelper, test} from '@umbraco/acceptance-test-helpers';
import {expect} from '@playwright/test';

// Seeded by AdditionalSetup/SchemaLockdownComposer.cs
const dictionaryName = 'LockedDictionaryItem';

test('blocks updating a dictionary item in the backoffice', async ({umbracoUi}) => {
  await umbracoUi.goToBackOffice();
  await umbracoUi.dictionary.goToSection(ConstantHelper.sections.dictionary);

  await test.step('Root', async () => {
    await umbracoUi.dictionary.isDictionaryRootActionsMenuVisible();
  });

  await test.step('Collection', async () => {
    await umbracoUi.dictionary.isCollectionActionWithNameVisible('Create');
  });

  await test.step('Item actions', async () => {
    await umbracoUi.dictionary.clickActionsMenuForDictionary(dictionaryName);
    await umbracoUi.dictionary.isEntityActionWithNameVisible('Delete');
    await umbracoUi.dictionary.isEntityActionWithNameVisible('Import', false);
    await umbracoUi.dictionary.isEntityActionWithNameVisible('MoveTo', false);
    await umbracoUi.page.keyboard.press('Escape');
  });

  await test.step('Workspace', async () => {
    await umbracoUi.dictionary.goToDictionaryItem(dictionaryName);
    await umbracoUi.dictionary.isReadOnlyTagVisible();
    await umbracoUi.schemaLockdown.isLockedWorkspaceActionVisible();
    await umbracoUi.dictionary.isSaveButtonVisible(false);
    await umbracoUi.dictionary.isWorkspaceNameEditable(false);
    await umbracoUi.dictionary.isTranslationEditableForLanguage('da-DK', false);
    await umbracoUi.schemaLockdown.clickLockedWorkspaceActionButton();
    await umbracoUi.dictionary.doesModalHaveText('Changes to this item are locked in this environment.');
  });
});

test('blocks updating a dictionary item through the Management API', async ({umbracoApi}) => {
  // Arrange
  const dictionaryItem = await umbracoApi.dictionary.getByName(dictionaryName);
  const url = `${umbracoApi.baseUrl}${ConstantHelper.apiEndpoints.dictionary}`;

  // Act
  const updateResponse = await umbracoApi.put(`${url}/${dictionaryItem.id}`, dictionaryItem);
  const importResponse = await umbracoApi.post(`${url}/import`, {});

  // Assert
  expect(updateResponse.status()).toBe(403);
  expect(updateResponse.headers()['umb-schema-lockdown']).toBe('dictionary-item:update');
  expect(importResponse.status()).toBe(403);
  expect(importResponse.headers()['umb-schema-lockdown']).toBe('dictionary-item:update');
});
