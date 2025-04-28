import {ConstantHelper, NotificationConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

const dictionaryName = 'TestDictionaryItem';
const parentDictionaryName = 'TestParentDictionary';

test.beforeEach(async ({umbracoUi}) => {
  await umbracoUi.goToBackOffice();
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.dictionary.ensureNameNotExists(dictionaryName);
});

test('can create a dictionary item', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.dictionary.ensureNameNotExists(dictionaryName);
  await umbracoUi.dictionary.goToSection(ConstantHelper.sections.dictionary);

  // Act
  await umbracoUi.dictionary.clickCreateLink();
  await umbracoUi.dictionary.enterDictionaryName(dictionaryName);
  await umbracoUi.dictionary.clickSaveButton();

  // Assert
  //await umbracoUi.dictionary.doesSuccessNotificationHaveText(NotificationConstantHelper.success.created);
  await umbracoUi.dictionary.isErrorNotificationVisible(false);
  expect(await umbracoApi.dictionary.doesNameExist(dictionaryName)).toBeTruthy();
  await umbracoUi.dictionary.clickLeftArrowButton();
  // Verify the dictionary item displays in the tree and in the list
  await umbracoUi.dictionary.isDictionaryTreeItemVisible(dictionaryName);
  expect(await umbracoUi.dictionary.doesDictionaryListHaveText(dictionaryName)).toBeTruthy();
});

test('can delete a dictionary item', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.dictionary.ensureNameNotExists(dictionaryName);
  await umbracoApi.dictionary.ensureNameNotExists(parentDictionaryName);
  await umbracoApi.dictionary.create(dictionaryName);
  await umbracoUi.dictionary.goToSection(ConstantHelper.sections.dictionary);

  // Act
  await umbracoUi.dictionary.clickActionsMenuForDictionary(dictionaryName);
  await umbracoUi.dictionary.deleteDictionary();

  // Assert
  await umbracoUi.waitForTimeout(500);
  //await umbracoUi.dictionary.doesSuccessNotificationHaveText(NotificationConstantHelper.success.deleted);
  await umbracoUi.dictionary.isErrorNotificationVisible(false);
  expect(await umbracoApi.dictionary.doesNameExist(dictionaryName)).toBeFalsy();
  // Verify the dictionary item does not display in the tree
  await umbracoUi.dictionary.isDictionaryTreeItemVisible(dictionaryName, false);
  // Verify the dictionary item does not display in the list
  await umbracoUi.reloadPage();
  await umbracoUi.dictionary.doesDictionaryCollectionContainText('No items');
});

test('can create a dictionary item in a dictionary', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.dictionary.ensureNameNotExists(parentDictionaryName);
  let parentDictionaryId = await umbracoApi.dictionary.create(parentDictionaryName);
  await umbracoUi.dictionary.goToSection(ConstantHelper.sections.dictionary);

  // Act
  await umbracoUi.dictionary.clickActionsMenuForDictionary(parentDictionaryName);
  await umbracoUi.dictionary.clickCreateButton();
  await umbracoUi.dictionary.enterDictionaryName(dictionaryName);
  await umbracoUi.dictionary.clickSaveButton();

  // Assert
  //await umbracoUi.dictionary.doesSuccessNotificationHaveText(NotificationConstantHelper.success.created);
  await umbracoUi.dictionary.isErrorNotificationVisible(false);
  const dictionaryChildren = await umbracoApi.dictionary.getChildren(parentDictionaryId);
  expect(dictionaryChildren[0].name).toEqual(dictionaryName);
  await umbracoUi.dictionary.clickLeftArrowButton();
  // Verify the new dictionary item displays in the list
  expect(await umbracoUi.dictionary.doesDictionaryListHaveText(dictionaryName)).toBeTruthy();
  // Verify the new dictionary item displays in the tree
  await umbracoUi.dictionary.reloadTree(parentDictionaryName);
  await umbracoUi.dictionary.isDictionaryTreeItemVisible(dictionaryName);

  // Clean
  await umbracoApi.dictionary.ensureNameNotExists(parentDictionaryName);
});

test('can export a dictionary item', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.dictionary.ensureNameNotExists(dictionaryName);
  const dictionaryId = await umbracoApi.dictionary.create(dictionaryName);
  await umbracoUi.dictionary.goToSection(ConstantHelper.sections.dictionary);

  // Act
  await umbracoUi.dictionary.clickActionsMenuForDictionary(dictionaryName);
  await umbracoUi.dictionary.clickExportButton();
  const exportData = await umbracoUi.dictionary.exportDictionary(false);

  // Assert
  expect(exportData).toEqual(dictionaryId + '.udt');
});

test('can export a dictionary item with descendants', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.dictionary.ensureNameNotExists(parentDictionaryName);
  let parentDictionaryId = await umbracoApi.dictionary.create(parentDictionaryName);
  await umbracoApi.dictionary.create(dictionaryName, [], parentDictionaryId);
  await umbracoUi.dictionary.goToSection(ConstantHelper.sections.dictionary);

  // Act
  await umbracoUi.dictionary.clickActionsMenuForDictionary(parentDictionaryName);
  await umbracoUi.dictionary.clickExportButton();
  const exportData = await umbracoUi.dictionary.exportDictionary(true);

  // Assert
  expect(exportData).toEqual(parentDictionaryId + '.udt');

  // Clean
  await umbracoApi.dictionary.ensureNameNotExists(parentDictionaryName);
});

test('can import a dictionary item', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const udtFilePath = './fixtures/dictionary/TestSingleDictionary.udt';
  // This variable must not be changed as it is declared in the file TestDictionary.udt
  const importDictionaryName = 'TestImportDictionary';
  await umbracoApi.dictionary.ensureNameNotExists(dictionaryName);
  await umbracoApi.dictionary.create(dictionaryName);
  await umbracoUi.dictionary.goToSection(ConstantHelper.sections.dictionary);

  // Act
  await umbracoUi.dictionary.clickActionsMenuForDictionary(dictionaryName);
  await umbracoUi.dictionary.clickImportButton();
  await umbracoUi.dictionary.importDictionary(udtFilePath);

  // Assert
  // Verify the imported dictionary item displays in the list
  await umbracoUi.reloadPage();
  expect(await umbracoUi.dictionary.doesDictionaryListHaveText(importDictionaryName)).toBeTruthy();
  // Verify the imported dictionary item displays in the tree
  await umbracoUi.dictionary.reloadTree(dictionaryName);
  await umbracoUi.dictionary.isDictionaryTreeItemVisible(importDictionaryName);
});

test('can import a dictionary item with descendants', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const udtFilePath = './fixtures/dictionary/TestDictionaryWithDescendants.udt';
  // This variable must not be changed as it is declared in the file TestDictionaryWithDescendants.udt
  const importParentDictionaryName = 'TestImportParent';
  const importChildDictionaryName = 'TestImportChild';
  await umbracoApi.dictionary.ensureNameNotExists(dictionaryName);
  await umbracoApi.dictionary.create(dictionaryName);
  await umbracoUi.dictionary.goToSection(ConstantHelper.sections.dictionary);

  // Act
  await umbracoUi.dictionary.clickActionsMenuForDictionary(dictionaryName);
  await umbracoUi.dictionary.clickImportButton();
  await umbracoUi.dictionary.importDictionary(udtFilePath);
  // These timeouts are necessary as this test can fail
  await umbracoUi.waitForTimeout(500);

  // Assert
  // Verify the imported dictionary items display in the list
  await umbracoUi.reloadPage();
  await umbracoUi.waitForTimeout(500);
  expect(await umbracoUi.dictionary.doesDictionaryListHaveText(importParentDictionaryName)).toBeTruthy();
  expect(await umbracoUi.dictionary.doesDictionaryListHaveText(importChildDictionaryName)).toBeTruthy();
  // Verify the imported dictionary items display in the tree
  await umbracoUi.dictionary.reloadTree(dictionaryName);
  await umbracoUi.dictionary.isDictionaryTreeItemVisible(importParentDictionaryName);
  await umbracoUi.dictionary.reloadTree(importParentDictionaryName);
  await umbracoUi.dictionary.isDictionaryTreeItemVisible(importChildDictionaryName);
});

// Skip this test as the search function is removed
test.skip('can search a dictionary item in list when have results', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.dictionary.ensureNameNotExists(dictionaryName);
  await umbracoApi.dictionary.create(dictionaryName);
  await umbracoUi.dictionary.goToSection(ConstantHelper.sections.dictionary);

  // Act
  await umbracoUi.dictionary.enterSearchKeywordAndPressEnter(dictionaryName);

  // Assert
  expect(await umbracoUi.dictionary.doesDictionaryListHaveText(dictionaryName)).toBeTruthy();
});

// Skip this test as the search function is removed
test.skip('can search a dictionary item in list when have no results', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const emptySearchResultMessage = 'No Dictionary items to choose from';
  await umbracoApi.dictionary.ensureNameNotExists(dictionaryName);
  await umbracoApi.dictionary.create(dictionaryName);
  await umbracoUi.dictionary.goToSection(ConstantHelper.sections.dictionary);

  // Act
  await umbracoUi.dictionary.enterSearchKeywordAndPressEnter('xyz');

  // Assert
  await umbracoUi.dictionary.isSearchResultMessageDisplayEmpty(emptySearchResultMessage);
});
