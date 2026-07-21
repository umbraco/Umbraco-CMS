import {ConstantHelper, test} from '@umbraco/acceptance-test-helpers';
import {expect} from "@playwright/test";

const dictionaryName = 'TestDictionaryItem';
const parentDictionaryName = 'TestParentDictionary';

test.beforeEach(async ({umbracoUi}) => {
  await umbracoUi.goToBackOffice();
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.dictionary.ensureNameNotExists(dictionaryName);
  await umbracoApi.dictionary.ensureNameNotExists(parentDictionaryName);
});

test('can create a dictionary item', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.dictionary.ensureNameNotExists(dictionaryName);
  await umbracoUi.dictionary.goToSection(ConstantHelper.sections.dictionary);

  // Act
  await umbracoUi.dictionary.clickCreateLink();
  await umbracoUi.dictionary.enterDictionaryName(dictionaryName);
  await umbracoUi.dictionary.clickSaveButtonAndWaitForDictionaryToBeCreated();

  // Assert
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
  await umbracoUi.dictionary.deleteDictionaryAndWaitForDictionaryToBeDeleted();

  // Assert
  // Verify the dictionary item does not display in the tree
  await umbracoUi.dictionary.isDictionaryTreeItemVisible(dictionaryName, false);
  // Verify the dictionary item does not display in the list
  await umbracoUi.reloadPage();
  await umbracoUi.dictionary.doesDictionaryCollectionContainText('No items');
  expect(await umbracoApi.dictionary.doesNameExist(dictionaryName)).toBeFalsy();
});

test('can create a dictionary item in a dictionary', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.dictionary.ensureNameNotExists(parentDictionaryName);
  let parentDictionaryId = await umbracoApi.dictionary.create(parentDictionaryName);
  await umbracoUi.dictionary.goToSection(ConstantHelper.sections.dictionary);

  // Act
  await umbracoUi.dictionary.clickActionsMenuForDictionary(parentDictionaryName);
  await umbracoUi.dictionary.clickCreateActionMenuOption();
  await umbracoUi.dictionary.enterDictionaryName(dictionaryName);
  await umbracoUi.dictionary.clickSaveButtonAndWaitForDictionaryToBeCreated();

  // Assert
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

test('can find a dictionary item in a sibling nested dictionary item', async ({umbracoApi}) => {
  // Arrange
  const firstChildDictionaryName = 'AAFirstChildDictionary';
  const nestedDictionaryName = 'NestedDictionary';
  const secondChildDictionaryName = 'ZZSecondChildDictionary';
  const targetDictionaryName = 'TargetDictionary';
  await umbracoApi.dictionary.ensureNameNotExists(parentDictionaryName);
  const parentDictionaryId = await umbracoApi.dictionary.create(parentDictionaryName);
  const firstChildDictionaryId = await umbracoApi.dictionary.create(firstChildDictionaryName, [], parentDictionaryId);
  await umbracoApi.dictionary.create(nestedDictionaryName, [], firstChildDictionaryId);
  const secondChildDictionaryId = await umbracoApi.dictionary.create(secondChildDictionaryName, [], parentDictionaryId);
  const targetDictionaryId = await umbracoApi.dictionary.create(targetDictionaryName, [], secondChildDictionaryId);

  // Act
  const dictionaryData = await umbracoApi.dictionary.getByName(targetDictionaryName);

  // Assert
  expect(dictionaryData).toBeTruthy();
  expect(dictionaryData.id).toBe(targetDictionaryId);

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
  await umbracoUi.dictionary.clickExportActionMenuOption();
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
  await umbracoUi.dictionary.clickExportActionMenuOption();
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
  await umbracoUi.dictionary.clickImportActionMenuOption();
  await umbracoUi.dictionary.importDictionaryAndWaitForDictionaryToBeImported(udtFilePath);

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
  await umbracoUi.dictionary.clickImportActionMenuOption();
  await umbracoUi.dictionary.importDictionaryAndWaitForDictionaryToBeImported(udtFilePath);

  // Assert
  // Verify the imported dictionary items display in the list
  await umbracoUi.reloadPage();
  expect(await umbracoUi.dictionary.doesDictionaryListHaveText(importParentDictionaryName)).toBeTruthy();
  expect(await umbracoUi.dictionary.doesDictionaryListHaveText(importChildDictionaryName)).toBeTruthy();
  // Verify the imported dictionary items display in the tree
  await umbracoUi.dictionary.reloadTree(dictionaryName);
  await umbracoUi.dictionary.isDictionaryTreeItemVisible(importParentDictionaryName);
  await umbracoUi.dictionary.reloadTree(importParentDictionaryName);
  await umbracoUi.dictionary.isDictionaryTreeItemVisible(importChildDictionaryName);
});

test('can search a dictionary item in list when have results', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.dictionary.ensureNameNotExists(dictionaryName);
  await umbracoApi.dictionary.create(dictionaryName);
  await umbracoUi.dictionary.goToSection(ConstantHelper.sections.dictionary);

  // Act
  await umbracoUi.dictionary.searchByKeywordInCollection(dictionaryName);

  // Assert
  expect(await umbracoUi.dictionary.doesDictionaryListHaveText(dictionaryName)).toBeTruthy();
});

test('can search a dictionary item in list when have no results', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const emptySearchResultMessage = 'No items';
  await umbracoApi.dictionary.ensureNameNotExists(dictionaryName);
  await umbracoApi.dictionary.create(dictionaryName);
  await umbracoUi.dictionary.goToSection(ConstantHelper.sections.dictionary);

  // Act
  await umbracoUi.dictionary.searchByKeywordInCollection('xyz');

  // Assert
  await umbracoUi.dictionary.isSearchResultMessageDisplayEmpty(emptySearchResultMessage);
});
