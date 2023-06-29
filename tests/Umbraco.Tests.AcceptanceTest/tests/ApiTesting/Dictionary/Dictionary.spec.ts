import {test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

test.describe('Dictionary tests', () => {
  const dictionaryName = 'Word'

  test.beforeEach(async ({page, umbracoApi}) => {
    await umbracoApi.dictionary.ensureDictionaryNameNotExists(dictionaryName);
  });

  test.afterEach(async ({page, umbracoApi}) => {
    await umbracoApi.dictionary.ensureDictionaryNameNotExists(dictionaryName);
  })

  test('can create a dictionary', async ({page, umbracoApi, umbracoUi}) => {
    const translationData = [
      {
        'isoCode': 'en-US',
        'translation': 'Word'
      },
      {
        'isoCode': 'da-DK',
        'translation': 'Ord'
      }
    ];

    await umbracoApi.dictionary.createDictionary(dictionaryName, translationData);

    // Assert
    await expect(umbracoApi.dictionary.doesDictionaryWithNameExists(dictionaryName)).toBeTruthy();
  });

  test('can update a dictionary', async ({page, umbracoApi, umbracoUi}) => {
    const oldDictionaryName = 'OldWord';

    await umbracoApi.dictionary.createDictionary(oldDictionaryName);

    const dictionary = await umbracoApi.dictionary.getDictionaryByName(oldDictionaryName);

    // Updates the dictionary name
    dictionary.name = dictionaryName;
    await umbracoApi.dictionary.updateDictionaryById(dictionary.id, dictionary);

    // Assert
    // Checks if the dictionary was updated
    const newDictionary = await umbracoApi.dictionary.getDictionaryById(dictionary.id);
    await expect(newDictionary.name == dictionaryName).toBeTruthy();

    await expect(umbracoApi.dictionary.doesDictionaryWithNameExists(dictionaryName)).toBeTruthy();
  });

  test('can delete a dictionary', async ({page, umbracoApi, umbracoUi}) => {
    await umbracoApi.dictionary.createDictionary(dictionaryName);

    await expect(umbracoApi.dictionary.doesDictionaryWithNameExists(dictionaryName)).toBeTruthy();

    await umbracoApi.dictionary.deleteDictionaryByName(dictionaryName);

    // Assert
    await expect(await umbracoApi.dictionary.doesDictionaryWithNameExists(dictionaryName)).toBeFalsy();
  });

  test('can create a dictionary item in a dictionary', async ({page, umbracoApi, umbracoUi}) => {
    const parentDictionaryName = 'Book';

    await umbracoApi.dictionary.ensureDictionaryNameNotExists(parentDictionaryName);

    await umbracoApi.dictionary.createDictionary(parentDictionaryName);

    const parentDictionary = await umbracoApi.dictionary.getDictionaryByName(parentDictionaryName);

    await umbracoApi.dictionary.createDictionary(dictionaryName, [], parentDictionary.id);

    const dictionaryChildren = await umbracoApi.dictionary.getDictionaryChildrenById(parentDictionary.id);

    // Assert
    // Checks if the parent dictionary contains the child dictionary
    await expect(dictionaryChildren.items[0].name == dictionaryName).toBeTruthy();

    // Clean
    await umbracoApi.dictionary.ensureDictionaryNameNotExists(parentDictionaryName);
  });
});
