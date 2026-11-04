import {ConstantHelper, test} from '@umbraco/acceptance-test-helpers';
import {expect} from '@playwright/test';

// Seeded by AdditionalSetup/SchemaLockdownComposer.cs
const dictionaryName = 'LockedDictionaryItem';

test('blocks every change to a dictionary item in the backoffice', async ({umbracoUi}) => {
  await umbracoUi.goToBackOffice();
  await umbracoUi.dictionary.goToSection(ConstantHelper.sections.dictionary);
  const sectionUrl = umbracoUi.page.url();

  await test.step('Root', async () => {
    await umbracoUi.dictionary.isDictionaryRootEntityActionVisible('ReloadChildrenOf');
    await umbracoUi.dictionary.isDictionaryRootActionsMenuVisible(false);
  });

  await test.step('Collection', async () => {
    await umbracoUi.dictionary.doesDictionaryCollectionContainText(dictionaryName);
    await umbracoUi.dictionary.isCollectionActionWithNameVisible('Create', false);
  });

  await test.step('Item actions', async () => {
    await umbracoUi.dictionary.clickActionsMenuForDictionary(dictionaryName);
    await umbracoUi.dictionary.isEntityActionWithNameVisible('Export');
    await umbracoUi.dictionary.isEntityActionWithNameVisible('Create', false);
    await umbracoUi.dictionary.isEntityActionWithNameVisible('Import', false);
    await umbracoUi.dictionary.isEntityActionWithNameVisible('MoveTo', false);
    await umbracoUi.dictionary.isEntityActionWithNameVisible('Delete', false);
    await umbracoUi.page.keyboard.press('Escape');
  });

  await test.step('Workspace', async () => {
    await umbracoUi.dictionary.goToDictionaryItem(dictionaryName);
    await umbracoUi.dictionary.isReadOnlyTagVisible();
    await umbracoUi.schemaLockdown.isLockedWorkspaceActionVisible();
    await umbracoUi.dictionary.isSaveButtonVisible(false);
    await umbracoUi.dictionary.isWorkspaceNameEditable(false);
    await umbracoUi.dictionary.isTranslationEditableForLanguage('da-DK', false);
  });

  await test.step('Create route', async () => {
    await umbracoUi.page.goto(`${sectionUrl}/workspace/dictionary/create/parent/dictionary-root/null`);
    await umbracoUi.dictionary.isEntityDetailForbiddenVisible();
  });
});

test('blocks every change to a dictionary item through the Management API', async ({umbracoApi}) => {
  // Arrange
  const dictionaryItem = await umbracoApi.dictionary.getByName(dictionaryName);
  const url = `${umbracoApi.baseUrl}${ConstantHelper.apiEndpoints.dictionary}`;

  // Act
  const createResponse = await umbracoApi.post(url, {});
  const updateResponse = await umbracoApi.put(`${url}/${dictionaryItem.id}`, dictionaryItem);
  const deleteResponse = await umbracoApi.delete(`${url}/${dictionaryItem.id}`);

  // Assert
  expect(createResponse.status()).toBe(403);
  expect(createResponse.headers()['umb-schema-lockdown']).toBe('dictionary-item:create');
  expect(updateResponse.status()).toBe(403);
  expect(updateResponse.headers()['umb-schema-lockdown']).toBe('dictionary-item:update');
  expect(deleteResponse.status()).toBe(403);
  expect(deleteResponse.headers()['umb-schema-lockdown']).toBe('dictionary-item:delete');
});
