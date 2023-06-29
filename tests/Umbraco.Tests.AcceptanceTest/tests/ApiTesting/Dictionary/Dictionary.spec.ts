import {test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

test.describe('Dictionary tests', () => {
  let dictionaryId = "";
  const dictionaryName = 'Word'

  test.beforeEach(async ({page, umbracoApi}) => {
    await umbracoApi.dictionary.ensureNameNotExists(dictionaryName);
  });

  test.afterEach(async ({page, umbracoApi}) => {
    await umbracoApi.dictionary.delete(dictionaryId);
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

    dictionaryId = await umbracoApi.dictionary.create(dictionaryName, translationData);

    // Assert
    await expect(umbracoApi.dictionary.exists(dictionaryId)).toBeTruthy();
  });

  test('can update a dictionary', async ({page, umbracoApi, umbracoUi}) => {
    const oldDictionaryName = 'OldWord';

    dictionaryId = await umbracoApi.dictionary.create(oldDictionaryName);

    const dictionary = await umbracoApi.dictionary.get(dictionaryId);

    // Updates the dictionary name
    dictionary.name = dictionaryName;
    await umbracoApi.dictionary.update(dictionaryId, dictionary);

    // Assert
    // Checks if the dictionary was updated
    const newDictionary = await umbracoApi.dictionary.get(dictionaryId);
    await expect(newDictionary.name).toEqual(dictionaryName);

    await expect(umbracoApi.dictionary.exists(dictionaryId)).toBeTruthy();
  });

  test('can delete a dictionary', async ({page, umbracoApi, umbracoUi}) => {
    dictionaryId = await umbracoApi.dictionary.create(dictionaryName);

    await expect(umbracoApi.dictionary.exists(dictionaryId)).toBeTruthy();

    await umbracoApi.dictionary.delete(dictionaryId);

    // Assert
    await expect(await umbracoApi.dictionary.exists(dictionaryId)).toBeFalsy();
  });

  test('can create a dictionary item in a dictionary', async ({page, umbracoApi, umbracoUi}) => {
    const parentDictionaryName = 'Book';

    await umbracoApi.dictionary.ensureNameNotExists(parentDictionaryName);

    dictionaryId = await umbracoApi.dictionary.create(parentDictionaryName);

    await umbracoApi.dictionary.create(dictionaryName, [], dictionaryId);

    const parentDictionaryChildren = await umbracoApi.dictionary.getChildren(dictionaryId);

    // Assert
    // Checks if the parent dictionary contains the child dictionary
    await expect(parentDictionaryChildren.items[0].name).toEqual(dictionaryName);
  });
});
