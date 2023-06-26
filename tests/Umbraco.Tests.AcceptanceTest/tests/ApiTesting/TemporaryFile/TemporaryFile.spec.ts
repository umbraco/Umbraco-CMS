import {test} from "@umbraco/playwright-testhelpers";
import {expect} from "@playwright/test";

test.describe('Temporary File tests', () => {
  const temporaryFileId = crypto.randomUUID();
  const fileName = 'Umbraco.png';
  const mimeType = 'image/png';
  const filePath = './fixtures/mediaLibrary/Umbraco.png';

  test.beforeEach(async ({page, umbracoApi}) => {
    await umbracoApi.temporaryFile.ensureTemporaryFileWithIdNotExists(temporaryFileId);
  });

  test.afterEach(async ({page, umbracoApi}) => {
    await umbracoApi.temporaryFile.ensureTemporaryFileWithIdNotExists(temporaryFileId);
  });

  test('can create temporary file', async ({page, umbracoApi, umbracoUi}) => {
    await umbracoApi.temporaryFile.createTemporaryFile(temporaryFileId, fileName, mimeType, filePath);

    // Assert
    await expect(await umbracoApi.temporaryFile.doesTemporaryFileWithIdExist(temporaryFileId)).toBeTruthy();
  });

  test('can delete temporary file', async ({page, umbracoApi, umbracoUi}) => {
    await umbracoApi.temporaryFile.createTemporaryFile(temporaryFileId, fileName, mimeType, filePath);

    await expect(await umbracoApi.temporaryFile.getTemporaryFileById(temporaryFileId)).toBeTruthy();

    await umbracoApi.temporaryFile.deleteTemporaryFileById(temporaryFileId);

    // Assert
    await expect(await umbracoApi.temporaryFile.doesTemporaryFileWithIdExist(temporaryFileId)).toBeFalsy();
  });
});
