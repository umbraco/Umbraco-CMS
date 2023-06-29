import {test} from "@umbraco/playwright-testhelpers";
import {expect} from "@playwright/test";

test.describe('Temporary File tests', () => {
  const temporaryFileId = crypto.randomUUID();
  const fileName = 'Umbraco.png';
  const mimeType = 'image/png';
  const filePath = './fixtures/mediaLibrary/Umbraco.png';

  test.beforeEach(async ({page, umbracoApi}) => {
    await umbracoApi.temporaryFile.delete(temporaryFileId);
  });

  test.afterEach(async ({page, umbracoApi}) => {
    await umbracoApi.temporaryFile.delete(temporaryFileId);
  });

  test('can create temporary file', async ({page, umbracoApi, umbracoUi}) => {
    await umbracoApi.temporaryFile.create(temporaryFileId, fileName, mimeType, filePath);

    // Assert
    await expect(await umbracoApi.temporaryFile.exists(temporaryFileId)).toBeTruthy();
  });

  test('can delete temporary file', async ({page, umbracoApi, umbracoUi}) => {
    await umbracoApi.temporaryFile.create(temporaryFileId, fileName, mimeType, filePath);

    await expect(await umbracoApi.temporaryFile.get(temporaryFileId)).toBeTruthy();

    await umbracoApi.temporaryFile.delete(temporaryFileId);

    // Assert
    await expect(await umbracoApi.temporaryFile.exists(temporaryFileId)).toBeFalsy();
  });
});
