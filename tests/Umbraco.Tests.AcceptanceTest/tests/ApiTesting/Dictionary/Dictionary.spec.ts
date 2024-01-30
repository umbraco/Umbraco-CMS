import {test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";
import * as crypto from 'crypto';

test.describe('Dictionary tests', () => {
  let dictionaryId = '';
  const dictionaryName = 'Word';

  test.beforeEach(async ({umbracoApi}) => {
    await umbracoApi.dictionary.ensureNameNotExists(dictionaryName);
  });

  test.afterEach(async ({umbracoApi}) => {
    await umbracoApi.dictionary.ensureNameNotExists(dictionaryName);
  });

  test('can create a dictionary', async ({umbracoApi}) => {
    // Arrange
    const translationData = [
      {
        'isoCode': 'en-US',
        'translation': 'Word'
      }
    ];

    // Act
    dictionaryId = await umbracoApi.dictionary.create(dictionaryName, translationData);

    // Assert
    await expect(umbracoApi.dictionary.doesExist(dictionaryId)).toBeTruthy();
  });

  test('can update a dictionary', async ({umbracoApi}) => {
    // Arrange
    const oldDictionaryName = 'OldWord';
    dictionaryId = await umbracoApi.dictionary.create(oldDictionaryName);
    const dictionary = await umbracoApi.dictionary.get(dictionaryId);

    // Act
    dictionary.name = dictionaryName;
    await umbracoApi.dictionary.update(dictionaryId, dictionary);

    // Assert
    // Checks if the dictionary was updated
    const newDictionary = await umbracoApi.dictionary.get(dictionaryId);
    await expect(newDictionary.name).toEqual(dictionaryName);
    await expect(umbracoApi.dictionary.doesExist(dictionaryId)).toBeTruthy();
  });

  test('can delete a dictionary', async ({umbracoApi}) => {
    // Arrange
    dictionaryId = await umbracoApi.dictionary.create(dictionaryName);
    await expect(umbracoApi.dictionary.doesExist(dictionaryId)).toBeTruthy();

    // Act
    await umbracoApi.dictionary.delete(dictionaryId);

    // Assert
    await expect(await umbracoApi.dictionary.doesExist(dictionaryId)).toBeFalsy();
  });

  test('can create a dictionary item in a dictionary', async ({umbracoApi}) => {
    // Arrange
    const childDictionaryName = 'Book';
    await umbracoApi.dictionary.ensureNameNotExists(dictionaryName);
    dictionaryId = await umbracoApi.dictionary.create(dictionaryName);

    // Act
    await umbracoApi.dictionary.create(childDictionaryName, [], dictionaryId);

    // Assert
    // Checks if the parent dictionary contains the child dictionary
    const dictionaryChildren = await umbracoApi.dictionary.getChildren(dictionaryId);
    await expect(dictionaryChildren[0].name).toEqual(childDictionaryName);
  });

  test('can export a dictionary', async ({umbracoApi}) => {
    // Arrange
    dictionaryId = await umbracoApi.dictionary.create(dictionaryName);
    await expect(umbracoApi.dictionary.doesExist(dictionaryId)).toBeTruthy();

    // Act
    const exportResponse = await umbracoApi.dictionary.export(dictionaryId, false);

    // Assert
    // Checks if the .udt file is exported
    await expect(exportResponse.headers()['content-disposition']).toContain(".udt");
  });

  test('can import a dictionary', async ({umbracoApi}) => {
    // Arrange
    const temporaryFileId = crypto.randomUUID();
    const fileName = 'TestSingleDictionary.udt';
    const mimeType = 'application/octet-stream';
    const filePath = './fixtures/dictionary/TestSingleDictionary.udt';
    // This variable must not be changed as it is declared in the file TestDictionary.udt
    const importDictionaryName = 'TestImportDictionary';

    // Create a dictionary
    dictionaryId = await umbracoApi.dictionary.create(dictionaryName);
    await expect(umbracoApi.dictionary.doesExist(dictionaryId)).toBeTruthy();

    // Create temporary file
    await umbracoApi.temporaryFile.create(temporaryFileId, fileName, mimeType, filePath);
    await expect(await umbracoApi.temporaryFile.exists(temporaryFileId)).toBeTruthy();

    // Act
    const importResponse = await umbracoApi.dictionary.import(temporaryFileId, dictionaryId);

    // Assert
    await expect(importResponse.ok()).toBeTruthy();
    // Checks if the parent dictionary contains the import dictionary
    const dictionaryChildren = await umbracoApi.dictionary.getChildren(dictionaryId);
    await expect(dictionaryChildren[0].name).toEqual(importDictionaryName);

    // Clean
    await umbracoApi.temporaryFile.delete(temporaryFileId);
  });
});
