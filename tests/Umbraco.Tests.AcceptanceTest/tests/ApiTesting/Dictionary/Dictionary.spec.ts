import {test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";
import * as crypto from 'crypto';

test.describe('Dictionary tests', () => {
  let dictionaryId = '';
  const dictionaryName = 'TestDictionary';

  test.beforeEach(async ({umbracoApi}) => {
    await umbracoApi.dictionary.ensureNameNotExists(dictionaryName);
  });

  test.afterEach(async ({umbracoApi}) => {
    await umbracoApi.dictionary.ensureNameNotExists(dictionaryName);
  });

  test('can create a dictionary', async ({umbracoApi}) => {
    // Act
    dictionaryId = await umbracoApi.dictionary.createDefaultDictionary(dictionaryName);

    // Assert
    expect(await umbracoApi.dictionary.doesExist(dictionaryId)).toBeTruthy();
  });

  test('can update a dictionary', async ({umbracoApi}) => {
    // Arrange
    const wrongDictionaryName = 'WrongTestDictionary';
    dictionaryId = await umbracoApi.dictionary.createDefaultDictionary(wrongDictionaryName);
    expect(await umbracoApi.dictionary.doesNameExist(wrongDictionaryName)).toBeTruthy();
    const dictionary = await umbracoApi.dictionary.get(dictionaryId);

    // Act
    dictionary.name = dictionaryName;
    await umbracoApi.dictionary.update(dictionaryId, dictionary);

    // Assert
    // Checks if the dictionary was updated
    const newDictionary = await umbracoApi.dictionary.get(dictionaryId);
    expect(newDictionary.name).toEqual(dictionaryName);
    expect(await umbracoApi.dictionary.doesExist(dictionaryId)).toBeTruthy();
  });

  test('can delete a dictionary', async ({umbracoApi}) => {
    // Arrange
    dictionaryId = await umbracoApi.dictionary.create(dictionaryName);
    expect(await umbracoApi.dictionary.doesExist(dictionaryId)).toBeTruthy();

    // Act
    await umbracoApi.dictionary.delete(dictionaryId);

    // Assert
    expect(await umbracoApi.dictionary.doesExist(dictionaryId)).toBeFalsy();
  });

  test('can create a dictionary item in a dictionary', async ({umbracoApi}) => {
    // Arrange
    const childDictionaryName = 'ChildDictionary';
    await umbracoApi.dictionary.ensureNameNotExists(dictionaryName);
    dictionaryId = await umbracoApi.dictionary.create(dictionaryName);

    // Act
    await umbracoApi.dictionary.create(childDictionaryName, [], dictionaryId);

    // Assert
    // Checks if the parent dictionary contains the child dictionary
    const dictionaryChildren = await umbracoApi.dictionary.getChildren(dictionaryId);
    expect(dictionaryChildren[0].name).toEqual(childDictionaryName);
  });

  test('can export a dictionary', async ({umbracoApi}) => {
    // Arrange
    dictionaryId = await umbracoApi.dictionary.create(dictionaryName);
    expect(await umbracoApi.dictionary.doesExist(dictionaryId)).toBeTruthy();

    // Act
    const exportResponse = await umbracoApi.dictionary.export(dictionaryId, false);

    // Assert
    // Checks if the .udt file is exported
    expect(exportResponse.headers()['content-disposition']).toContain(".udt");
  });

  test('can import a dictionary', async ({umbracoApi}) => {
    // Arrange
    const temporaryFileId = crypto.randomUUID();
    const fileName = 'TestSingleDictionary.udt';
    const mimeType = 'application/octet-stream';
    const filePath = './fixtures/dictionary/TestSingleDictionary.udt';
    // This variable must not be changed as it is declared in the file TestDictionary.udt
    const importDictionaryName = 'TestImportDictionary';

    // Create temporary file
    await umbracoApi.temporaryFile.create(temporaryFileId, fileName, mimeType, filePath);
    expect(await umbracoApi.temporaryFile.doesExist(temporaryFileId)).toBeTruthy();

    // Act
    const importResponse = await umbracoApi.dictionary.import(temporaryFileId, null);

    // Assert
    expect(importResponse.status).toBeTruthy();
    expect(await umbracoApi.dictionary.doesNameExist(importDictionaryName)).toBeTruthy();

    // Clean
    await umbracoApi.temporaryFile.delete(temporaryFileId);
  });
});
