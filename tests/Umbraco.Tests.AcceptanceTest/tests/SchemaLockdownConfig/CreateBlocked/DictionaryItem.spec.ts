import {ConstantHelper, test} from '@umbraco/acceptance-test-helpers';
import {expect} from '@playwright/test';

// Seeded by AdditionalSetup/SchemaLockdownComposer.cs
const dictionaryName = 'LockedDictionaryItem';

test('blocks creating a dictionary item in the backoffice', async ({umbracoUi}) => {
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
    await umbracoUi.dictionary.isEntityActionWithNameVisible('Delete');
    await umbracoUi.dictionary.isEntityActionWithNameVisible('Create', false);
    await umbracoUi.dictionary.isEntityActionWithNameVisible('Import', false);
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

  await test.step('Create route', async () => {
    await umbracoUi.page.goto(`${sectionUrl}/workspace/dictionary/create/parent/dictionary-root/null`);
    await umbracoUi.dictionary.isEntityDetailForbiddenVisible();
  });
});

test('blocks creating a dictionary item through the Management API', async ({umbracoApi}) => {
  // Act
  const response = await umbracoApi.post(`${umbracoApi.baseUrl}${ConstantHelper.apiEndpoints.dictionary}`, {});

  // Assert
  expect(response.status()).toBe(403);
  expect(response.headers()['umb-schema-lockdown']).toBe('dictionary-item:create');
});
