import {test} from "@umbraco/playwright-testhelpers";
import {expect} from "@playwright/test";
import * as crypto from 'crypto';

test.describe('Temporary File tests', () => {
  const temporaryFileId = crypto.randomUUID();
  const fileName = 'Umbraco.png';
  const mimeType = 'image/png';
  const filePath = './fixtures/mediaLibrary/Umbraco.png';

  test.beforeEach(async ({umbracoApi}) => {
    await umbracoApi.temporaryFile.delete(temporaryFileId);
  });

  test.afterEach(async ({umbracoApi}) => {
    await umbracoApi.temporaryFile.delete(temporaryFileId);
  });

  test('can create temporary file', async ({umbracoApi}) => {
    // Act
    await umbracoApi.temporaryFile.create(temporaryFileId, fileName, mimeType, filePath);

    // Assert
    expect(await umbracoApi.temporaryFile.doesExist(temporaryFileId)).toBeTruthy();
  });

  test('can delete temporary file', async ({umbracoApi}) => {
    // Arrange
    await umbracoApi.temporaryFile.create(temporaryFileId, fileName, mimeType, filePath);
    expect(await umbracoApi.temporaryFile.get(temporaryFileId)).toBeTruthy();

    // Act
    await umbracoApi.temporaryFile.delete(temporaryFileId);

    // Assert
    expect(await umbracoApi.temporaryFile.doesExist(temporaryFileId)).toBeFalsy();
  });
});
