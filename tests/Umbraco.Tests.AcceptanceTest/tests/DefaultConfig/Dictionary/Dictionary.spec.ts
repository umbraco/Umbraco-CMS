import {ConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

test.describe('Dictionary tests', () => {
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
    await umbracoUi.dictionary.clickSaveButton();

    // Assert
    expect(await umbracoApi.dictionary.doesNameExist(dictionaryName)).toBeTruthy();
    await umbracoUi.dictionary.isSuccessNotificationVisible();
    // TODO: when frontend is ready, verify the new dictionary item displays in tree and in list
    // await umbracoUi.goToSection(ConstantHelper.sections.dictionary);
    // await umbracoUi.dictionary.isDictionaryListHasText(dictionaryName);
  });

  test('can delete a dictionary item', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    await umbracoApi.dictionary.ensureNameNotExists(dictionaryName);
    await umbracoApi.dictionary.create(dictionaryName);
    await umbracoUi.dictionary.goToSection(ConstantHelper.sections.dictionary);

    // Act
    await umbracoUi.dictionary.clickActionsMenuForDictionary(dictionaryName);
    await umbracoUi.dictionary.deleteDictionary();

    // Assert
    expect(await umbracoApi.dictionary.doesNameExist(dictionaryName)).toBeFalsy();
    // TODO: when frontend is ready, verify the new dictionary item is NOT displayed in list view and in tree
  });

  test.skip('can create a dictionary item in a dictionary @smoke', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    await umbracoApi.dictionary.ensureNameNotExists(parentDictionaryName);
    let parentDictionaryId = await umbracoApi.dictionary.create(parentDictionaryName);
    await umbracoUi.dictionary.goToSection(ConstantHelper.sections.dictionary);

    // Act
    await umbracoUi.dictionary.clickActionsMenuForDictionary(parentDictionaryName);
    await umbracoUi.dictionary.clickCreateMenu();
    await umbracoUi.dictionary.enterDictionaryName(dictionaryName);
    await umbracoUi.dictionary.clickSaveButton();

    // Assert
    const dictionaryChildren = await umbracoApi.dictionary.getChildren(parentDictionaryId);
    await expect(dictionaryChildren[0].name).toEqual(dictionaryName);
    // TODO: when frontend is ready, verify the notification displays
  });

  // Remove skip when export function works
  test.skip('can export a dictionary item', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    await umbracoApi.dictionary.ensureNameNotExists(dictionaryName);
    await umbracoApi.dictionary.create(dictionaryName);
    await umbracoUi.dictionary.goToSection(ConstantHelper.sections.dictionary);

    // Act
    await umbracoUi.dictionary.clickActionsMenuForDictionary(dictionaryName);
    await umbracoUi.dictionary.clickExportMenu();
    await umbracoUi.dictionary.exportDictionary(false);

    // Assert
    // TODO: when frontend is ready, verify the notification displays
  });

  // Remove skip when export function works
  test.skip('can export a dictionary item with descendants @smoke', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    await umbracoApi.dictionary.ensureNameNotExists(parentDictionaryName);
    let parentDictionaryId = await umbracoApi.dictionary.create(parentDictionaryName);
    await umbracoApi.dictionary.create(dictionaryName, [], parentDictionaryId);
    await umbracoUi.dictionary.goToSection(ConstantHelper.sections.dictionary);

    // Act
    await umbracoUi.dictionary.clickActionsMenuForDictionary(dictionaryName);
    await umbracoUi.dictionary.clickExportMenu();
    await umbracoUi.dictionary.exportDictionary(true);

    // Assert
    // TODO: when frontend is ready, verify the notification displays
  });

  // Remove skip when import function works
  test.skip('can import a dictionary item', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const udtFilePath = './fixtures/dictionary/TestSingleDictionary.udt';
    await umbracoApi.dictionary.ensureNameNotExists(dictionaryName);
    await umbracoApi.dictionary.create(dictionaryName);
    await umbracoUi.dictionary.goToSection(ConstantHelper.sections.dictionary);

    // Act
    await umbracoUi.dictionary.clickActionsMenuForDictionary(dictionaryName);
    await umbracoUi.dictionary.clickImportMenu();
    await umbracoUi.dictionary.importDictionary(udtFilePath);

    // Assert
    // TODO: when frontend is ready, verify the notification displays
    // TODO: when frontend is ready, verify the imported dictionary items displays
  });

  // Remove skip when import function works
  test.skip('can import a dictionary item with descendants @smoke', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const udtFilePath = './fixtures/dictionary/TestDictionaryWithDescendants.udt';
    await umbracoApi.dictionary.ensureNameNotExists(parentDictionaryName);
    let parentDictionaryId = await umbracoApi.dictionary.create(parentDictionaryName);
    await umbracoApi.dictionary.create(dictionaryName, [], parentDictionaryId);
    await umbracoUi.dictionary.goToSection(ConstantHelper.sections.dictionary);

    // Act
    await umbracoUi.dictionary.clickActionsMenuForDictionary(dictionaryName);
    await umbracoUi.dictionary.clickImportMenu();
    await umbracoUi.dictionary.importDictionary(udtFilePath);

    // Assert
    // TODO: when frontend is ready, verify the notification displays
    // TODO: when frontend is ready, verify the imported dictionary items displays
  });

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
});
