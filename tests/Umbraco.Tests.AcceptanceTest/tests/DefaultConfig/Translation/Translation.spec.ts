import {ConstantHelper, test} from '@umbraco/playwright-testhelpers';
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
  await umbracoUi.translation.goToSection(ConstantHelper.sections.translation);

  // Act
  await umbracoUi.translation.clickCreateLink();
  await umbracoUi.translation.enterDictionaryName(dictionaryName);
  await umbracoUi.translation.clickSaveButton();

  // Assert
  expect(await umbracoApi.dictionary.doesNameExist(dictionaryName)).toBeTruthy();
  await umbracoUi.translation.isSuccessNotificationVisible();
  await umbracoUi.translation.clickLeftArrowButton();
  // Verify the dictionary item displays in the tree and in the list
  await umbracoUi.translation.isDictionaryTreeItemVisible(dictionaryName);
  expect(await umbracoUi.translation.doesDictionaryListHaveText(dictionaryName)).toBeTruthy();
});

test('can delete a dictionary item', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.dictionary.ensureNameNotExists(dictionaryName);
  await umbracoApi.dictionary.create(dictionaryName);
  await umbracoUi.translation.goToSection(ConstantHelper.sections.translation);

  // Act
  await umbracoUi.translation.clickActionsMenuForDictionary(dictionaryName);
  await umbracoUi.translation.deleteDictionary();

  // Assert
  await umbracoUi.translation.isSuccessNotificationVisible();
  expect(await umbracoApi.dictionary.doesNameExist(dictionaryName)).toBeFalsy();
  // Verify the dictionary item does not display in the tree
  await umbracoUi.translation.isDictionaryTreeItemVisible(dictionaryName, false);
  // TODO: Uncomment this when the front-end is ready. Currently the dictionary list is not updated immediately.
  // Verify the dictionary item does not display in the list
  //expect(await umbracoUi.translation.doesDictionaryListHaveText(dictionaryName)).toBeFalsy();
});

test('can create a dictionary item in a dictionary', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.dictionary.ensureNameNotExists(parentDictionaryName);
  let parentDictionaryId = await umbracoApi.dictionary.create(parentDictionaryName);
  await umbracoUi.translation.goToSection(ConstantHelper.sections.translation);

  // Act
  await umbracoUi.translation.clickActionsMenuForDictionary(parentDictionaryName);
  await umbracoUi.translation.clickCreateDictionaryItemButton();
  await umbracoUi.translation.enterDictionaryName(dictionaryName);
  await umbracoUi.translation.clickSaveButton();

  // Assert
  await umbracoUi.translation.isSuccessNotificationVisible();
  const dictionaryChildren = await umbracoApi.dictionary.getChildren(parentDictionaryId);
  expect(dictionaryChildren[0].name).toEqual(dictionaryName);
  await umbracoUi.translation.clickLeftArrowButton();
  // Verify the new dictionary item displays in the list
  expect(await umbracoUi.translation.doesDictionaryListHaveText(dictionaryName)).toBeTruthy();
  // Verify the new dictionary item displays in the tree
  await umbracoUi.translation.reloadTree(parentDictionaryName);
  await umbracoUi.translation.isDictionaryTreeItemVisible(dictionaryName);

  // Clean
  await umbracoApi.dictionary.ensureNameNotExists(parentDictionaryName);
});

test('can export a dictionary item', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.dictionary.ensureNameNotExists(dictionaryName);
  const dictionaryId = await umbracoApi.dictionary.create(dictionaryName);
  await umbracoUi.translation.goToSection(ConstantHelper.sections.translation);

  // Act
  await umbracoUi.translation.clickActionsMenuForDictionary(dictionaryName);
  await umbracoUi.translation.clickExportMenu();
  const exportData = await umbracoUi.translation.exportDictionary(false);

  // Assert
  expect(exportData).toEqual(dictionaryId + '.udt');
});

test('can export a dictionary item with descendants', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.dictionary.ensureNameNotExists(parentDictionaryName);
  let parentDictionaryId = await umbracoApi.dictionary.create(parentDictionaryName);
  await umbracoApi.dictionary.create(dictionaryName, [], parentDictionaryId);
  await umbracoUi.translation.goToSection(ConstantHelper.sections.translation);

  // Act
  await umbracoUi.translation.clickActionsMenuForDictionary(parentDictionaryName);
  await umbracoUi.translation.clickExportMenu();
  const exportData = await umbracoUi.translation.exportDictionary(true);

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
  await umbracoUi.translation.goToSection(ConstantHelper.sections.translation);

  // Act
  await umbracoUi.translation.clickActionsMenuForDictionary(dictionaryName);
  await umbracoUi.translation.clickImportMenu();
  await umbracoUi.translation.importDictionary(udtFilePath);

  // Assert
  // Verify the imported dictionary item displays in the tree
  await umbracoUi.translation.reloadTree(dictionaryName);
  await umbracoUi.translation.isDictionaryTreeItemVisible(importDictionaryName);
  // TODO: Uncomment this when the front-end is ready. Currently the dictionary list is not updated immediately.
  // Verify the imported dictionary item displays in the list
  //expect(await umbracoUi.translation.doesDictionaryListHaveText(importDictionaryName)).toBeTruthy();
});

test('can import a dictionary item with descendants', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const udtFilePath = './fixtures/dictionary/TestDictionaryWithDescendants.udt';
  // This variable must not be changed as it is declared in the file TestDictionaryWithDescendants.udt
  const importParentDictionaryName = 'TestImportParent';
  const importChildDictionaryName = 'TestImportChild';
  await umbracoApi.dictionary.ensureNameNotExists(dictionaryName);
  await umbracoApi.dictionary.create(dictionaryName);
  await umbracoUi.translation.goToSection(ConstantHelper.sections.translation);

  // Act
  await umbracoUi.translation.clickActionsMenuForDictionary(dictionaryName);
  await umbracoUi.translation.clickImportMenu();
  await umbracoUi.translation.importDictionary(udtFilePath);

  // Assert
  // Verify the imported dictionary items display in the tree
  await umbracoUi.translation.reloadTree(dictionaryName);
  await umbracoUi.translation.isDictionaryTreeItemVisible(importParentDictionaryName);
  await umbracoUi.translation.reloadTree(importParentDictionaryName);
  await umbracoUi.translation.isDictionaryTreeItemVisible(importChildDictionaryName);
  // TODO: Uncomment this when the front-end is ready. Currently the dictionary list is not updated immediately.
  // Verify the imported dictionary items display in the list
  //expect(await umbracoUi.translation.doesDictionaryListHaveText(importParentDictionaryName)).toBeTruthy();
  //expect(await umbracoUi.translation.doesDictionaryListHaveText(importChildDictionaryName)).toBeTruthy();
});

// Skip this test as the search function is removed
test.skip('can search a dictionary item in list when have results', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.dictionary.ensureNameNotExists(dictionaryName);
  await umbracoApi.dictionary.create(dictionaryName);
  await umbracoUi.translation.goToSection(ConstantHelper.sections.translation);

  // Act
  await umbracoUi.translation.enterSearchKeywordAndPressEnter(dictionaryName);

  // Assert
  expect(await umbracoUi.translation.doesDictionaryListHaveText(dictionaryName)).toBeTruthy();
});

// Skip this test as the search function is removed
test.skip('can search a dictionary item in list when have no results', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const emptySearchResultMessage = 'No Dictionary items to choose from';
  await umbracoApi.dictionary.ensureNameNotExists(dictionaryName);
  await umbracoApi.dictionary.create(dictionaryName);
  await umbracoUi.translation.goToSection(ConstantHelper.sections.translation);

  // Act
  await umbracoUi.translation.enterSearchKeywordAndPressEnter('xyz');

  // Assert
  await umbracoUi.translation.isSearchResultMessageDisplayEmpty(emptySearchResultMessage);
});
