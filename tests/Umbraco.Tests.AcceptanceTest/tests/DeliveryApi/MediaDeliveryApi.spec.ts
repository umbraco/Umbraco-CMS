import {expect} from '@playwright/test';
import {AliasHelper} from '@umbraco/json-models-builders';
import {test} from '@umbraco/playwright-testhelpers';

// Media Folder
const rootMediaFolderName = 'RootMediaFolder';
const childMediaFolderName = 'ChildMediaFolder';
// Media Items
const rootImageName = 'RootImage';
const rootArticleName = 'RootArticle';
const rootAudioName = 'RootAudio';
const rootSVGName = 'RootSVG';
const rootCustomMediaName = 'TestCustomMedia'
const fileName = 'TestFile';
const videoName = 'TestVideo';
const imageSizeNamePrefix = 'Test Image Size';
const rootImageSizeNamePrefix = 'Root Test Image Size';
const specialCharacterImageName = ', . ! ? # $ % & * @ é ü ă đ 漢字'
// Media Type
const customMediaTypeName = 'CustomMediaType';
// Data Type
const textStringDataType = 'Textstring';
const textStringValue = 'This is a test textstring';
// Constant
const imageSize = 3;
const rootImageSize = 5;

test.describe('Media Delivery API', () => {
  test.beforeAll(async ({umbracoApi}) => {
    // Create a root level folder
    const rootFolderId = await umbracoApi.media.createDefaultMediaFolder(rootMediaFolderName);
    // Create child folder
    const childFolderId = await umbracoApi.media.createDefaultMediaFolderAndParentId(childMediaFolderName, rootFolderId);
    // Create an image item at root level
    await umbracoApi.media.createDefaultMediaWithImage(rootImageName);
    // Create a article file item at root level
    await umbracoApi.media.createDefaultMediaWithArticle(rootArticleName);
    // Create an audio item at root level
    await umbracoApi.media.createDefaultMediaWithAudio(rootAudioName);
    // Create an vector graphic item at root level
    await umbracoApi.media.createDefaultMediaWithSVG(rootSVGName);
    // Create a file item in the child folder
    await umbracoApi.media.createDefaultMediaFileAndParentId(fileName, childFolderId);
    // Create a video item in the child folder
    await umbracoApi.media.createDefaultMediaWithVideoAndParentId(videoName, childFolderId);
    // Create multiple image items in the child folder, named have prefix imageSizeNamePrefix
    for (let i = 1; i <= imageSize; i++) {
      await umbracoApi.media.createDefaultMediaWithImageAndParentId(imageSizeNamePrefix + i, childFolderId);
    }
    // Create multiple image items in the root folder, named have prefix rootImageSizeNamePrefix
    for (let i = 1; i <= rootImageSize; i++) {
      await umbracoApi.media.createDefaultMediaWithImage(rootImageSizeNamePrefix + i);
    }
    // Create custom media item at root level
    await umbracoApi.media.createDefaultMediaWithTextstring(rootCustomMediaName, customMediaTypeName, textStringValue, textStringDataType);
    // Create an image item at root level and its name has special characters
    await umbracoApi.media.createDefaultMediaWithImage(specialCharacterImageName);
  });

  test.afterAll(async ({umbracoApi}) => {
    await umbracoApi.media.ensureNameNotExists(rootMediaFolderName);
    await umbracoApi.media.ensureNameNotExists(childMediaFolderName);
    await umbracoApi.media.ensureNameNotExists(rootImageName);
    await umbracoApi.media.ensureNameNotExists(rootArticleName);
    await umbracoApi.media.ensureNameNotExists(rootAudioName);
    await umbracoApi.media.ensureNameNotExists(rootSVGName);
    await umbracoApi.media.ensureNameNotExists(fileName);
    await umbracoApi.media.ensureNameNotExists(videoName);
    await umbracoApi.media.ensureNameNotExists(specialCharacterImageName);
    for (let i = 1; i <= imageSize; i++) {
      await umbracoApi.media.ensureNameNotExists(imageSizeNamePrefix + i);
    }
    for (let i = 1; i <= rootImageSize; i++) {
      await umbracoApi.media.ensureNameNotExists(rootImageSizeNamePrefix + i);
    }
    await umbracoApi.media.ensureNameNotExists(rootCustomMediaName);
    await umbracoApi.mediaType.ensureNameNotExists(customMediaTypeName);
  });

  // Gets a media item by id
  test('can fetch an image item by its ID', async ({umbracoApi}) => {
    // Arrange
    const mediaTypeName = 'Image';
    const mediaData = await umbracoApi.media.getByName(rootImageName);
    const mediaPath = '/' + rootImageName.toLowerCase() + '/';

    // Act
    const mediaItem = await umbracoApi.mediaDeliveryApi.getMediaItemWithId(mediaData.id);

    // Assert
    expect(mediaItem.status()).toBe(200);
    const mediaItemJson = await mediaItem.json();
    await umbracoApi.mediaDeliveryApi.verifyDefaultMediaItemJson(rootImageName, mediaItemJson, mediaPath, mediaTypeName);
  });

  test('can fetch an audio item by its ID', async ({umbracoApi}) => {
    // Arrange
    const mediaTypeName = 'Audio';
    const mediaData = await umbracoApi.media.getByName(rootAudioName);
    const mediaPath = '/' + rootAudioName.toLowerCase() + '/';

    // Act
    const mediaItem = await umbracoApi.mediaDeliveryApi.getMediaItemWithId(mediaData.id);

    // Assert
    expect(mediaItem.status()).toBe(200);
    const mediaItemJson = await mediaItem.json();
    await umbracoApi.mediaDeliveryApi.verifyDefaultMediaItemJson(rootAudioName, mediaItemJson, mediaPath, mediaTypeName);
  });

  test('can fetch a vector graphics item by its ID', async ({umbracoApi}) => {
    // Arrange
    const mediaTypeName = 'Vector Graphics (SVG)';
    const mediaData = await umbracoApi.media.getByName(rootSVGName);
    const mediaPath = '/' + rootSVGName.toLowerCase() + '/';

    // Act
    const mediaItem = await umbracoApi.mediaDeliveryApi.getMediaItemWithId(mediaData.id);

    // Assert
    expect(mediaItem.status()).toBe(200);
    const mediaItemJson = await mediaItem.json();
    await umbracoApi.mediaDeliveryApi.verifyDefaultMediaItemJson(rootSVGName, mediaItemJson, mediaPath, mediaTypeName);
  });

  test('can fetch a media folder by its ID', async ({umbracoApi}) => {
    // Arrange
    const mediaTypeName = 'Folder';
    const mediaData = await umbracoApi.media.getByName(rootMediaFolderName);
    const mediaPath = '/' + rootMediaFolderName.toLowerCase() + '/';

    // Act
    const mediaItem = await umbracoApi.mediaDeliveryApi.getMediaItemWithId(mediaData.id);

    // Assert
    expect(mediaItem.status()).toBe(200);
    const mediaItemJson = await mediaItem.json();
    await umbracoApi.mediaDeliveryApi.verifyDefaultMediaItemJson(rootMediaFolderName, mediaItemJson, mediaPath, mediaTypeName);
  });

  test('can fetch a media item in a folder in a folder by its ID', async ({umbracoApi}) => {
    // Arrange
    const mediaTypeName = 'File';
    const mediaData = await umbracoApi.media.getByName(fileName);
    const mediaPath = '/' + rootMediaFolderName.toLowerCase() + '/' + childMediaFolderName.toLowerCase() + '/' + fileName.toLowerCase() + '/';

    // Act
    const mediaItem = await umbracoApi.mediaDeliveryApi.getMediaItemWithId(mediaData.id);

    // Assert
    expect(mediaItem.status()).toBe(200);
    const mediaItemJson = await mediaItem.json();
    await umbracoApi.mediaDeliveryApi.verifyDefaultMediaItemJson(fileName, mediaItemJson, mediaPath, mediaTypeName);
  });

  test('can fetch an custom media item by its ID', async ({umbracoApi}) => {
    // Arrange
    const mediaData = await umbracoApi.media.getByName(rootCustomMediaName);
    const mediaPath = '/' + rootCustomMediaName.toLowerCase() + '/';

    // Act
    const mediaItem = await umbracoApi.mediaDeliveryApi.getMediaItemWithId(mediaData.id);

    // Assert
    expect(mediaItem.status()).toBe(200);
    const mediaItemJson = await mediaItem.json();
    await umbracoApi.mediaDeliveryApi.verifyDefaultMediaItemJson(rootCustomMediaName, mediaItemJson, mediaPath, customMediaTypeName);
    expect(mediaItemJson.properties[AliasHelper.toAlias(textStringDataType)]).toBe(textStringValue);
  });

  test('returns 404 when fetching a non-existent media item', async ({umbracoApi}) => {
    // Arrange
    const nonExistentMediaId = '00000000-0000-0000-0000-000000000000';

    // Act
    const mediaItem = await umbracoApi.mediaDeliveryApi.getMediaItemWithId(nonExistentMediaId);

    // Assert
    expect(mediaItem.status()).toBe(404);
  });

  test('returns 401 when fetching a media item without proper authorization', async ({umbracoApi}) => {
  });

  // Gets a media item by path
  test('can fetch an media item by its path', async ({umbracoApi}) => {
    // Arrange
    const mediaTypeName = 'Article';
    const mediaPath = '/' + rootArticleName.toLowerCase() + '/';

    // Act
    const mediaItem = await umbracoApi.mediaDeliveryApi.getMediaItemWithPath(mediaPath);

    // Assert
    expect(mediaItem.status()).toBe(200);
    const mediaItemJson = await mediaItem.json();
    await umbracoApi.mediaDeliveryApi.verifyDefaultMediaItemJson(rootArticleName, mediaItemJson, mediaPath, mediaTypeName);
  });

  // This test fails because it will return 404 error if the path includes # or ?
  test('can fetch an media item by its path with special character', async ({umbracoApi}) => {
    // Arrange
    const mediaTypeName = 'Image';
    const mediaPath = '/' + specialCharacterImageName.toLowerCase() + '/';

    // Act
    const mediaItem = await umbracoApi.mediaDeliveryApi.getMediaItemWithPath(mediaPath);

    // Assert
    expect(mediaItem.status()).toBe(200);
    const mediaItemJson = await mediaItem.json();
    await umbracoApi.mediaDeliveryApi.verifyDefaultMediaItemJson(specialCharacterImageName, mediaItemJson, mediaPath, mediaTypeName);
  });

  test('can fetch a media folder in a folder by its path', async ({umbracoApi}) => {
    // Arrange
    const mediaTypeName = 'Folder';
    const mediaPath = '/' + rootMediaFolderName.toLowerCase() + '/' + childMediaFolderName.toLowerCase() + '/';

    // Act
    const mediaItem = await umbracoApi.mediaDeliveryApi.getMediaItemWithPath(mediaPath);

    // Assert
    expect(mediaItem.status()).toBe(200);
    const mediaItemJson = await mediaItem.json();
    await umbracoApi.mediaDeliveryApi.verifyDefaultMediaItemJson(childMediaFolderName, mediaItemJson, mediaPath, mediaTypeName);
  });

  test('can fetch a media item in a folder in a folder by its path', async ({umbracoApi}) => {
    // Arrange
    const mediaTypeName = 'Video';
    const mediaPath = '/' + rootMediaFolderName.toLowerCase() + '/' + childMediaFolderName.toLowerCase() + '/' + videoName.toLowerCase() + '/';

    // Act
    const mediaItem = await umbracoApi.mediaDeliveryApi.getMediaItemWithId(mediaPath);

    // Assert
    expect(mediaItem.status()).toBe(200);
    const mediaItemJson = await mediaItem.json();
    await umbracoApi.mediaDeliveryApi.verifyDefaultMediaItemJson(videoName, mediaItemJson, mediaPath, mediaTypeName);
  });

  test('returns 404 when fetching a non-existent media path', async ({umbracoApi}) => {
    // Arrange
    const nonExistentMediaPath = '/non-existent-media-path/';

    // Act
    const mediaItem = await umbracoApi.mediaDeliveryApi.getMediaItemWithPath(nonExistentMediaPath);

    // Assert
    expect(mediaItem.status()).toBe(404);
  });

  test('returns 401 when fetching a media item by path without proper authorization', async ({umbracoApi}) => {
  });

  // Gets media item(s) by id
  test('can fetch multiple media items by their IDs', async ({umbracoApi}) => {
    // Arrange
    const firstMediaName = rootImageSizeNamePrefix + '1';
    const secondMediaName = childMediaFolderName;
    const mediaNames = [firstMediaName, secondMediaName];
    const firstMediaPath = '/' + firstMediaName.toLowerCase() + '/';
    const secondMediaPath = '/' + rootMediaFolderName.toLowerCase() + '/' + childMediaFolderName.toLowerCase() + '/';
    const mediaPaths = [firstMediaPath, secondMediaPath];
    const mediaTypeNames = ['Image', 'Folder'];
    const firstMediaData = await umbracoApi.media.getByName(firstMediaName);
    const secondMediaData = await umbracoApi.media.getByName(secondMediaName);
    const mediaIds = [firstMediaData.id, secondMediaData.id];

    // Act
    const mediaItems = await umbracoApi.mediaDeliveryApi.getMediaItemsWithIds(mediaIds);

    // Assert
    expect(mediaItems.status()).toBe(200);
    const mediaItemsJson = await mediaItems.json();
    await umbracoApi.mediaDeliveryApi.verifyMutipleMediaItemsJson(mediaNames, mediaItemsJson, mediaPaths, mediaTypeNames);
  });

  test('returns only valid media items when some IDs are invalid', async ({umbracoApi}) => {
    // Arrange
    const validMediaName = rootArticleName;
    const validMediaPath = '/' + validMediaName.toLowerCase() + '/';
    const validMediaTypeName = 'Article';
    const validMediaId = await umbracoApi.media.getByName(validMediaName);
    const nonExistentMediaId = '00000000-0000-0000-0000-000000000000';
    const mediaIds = [validMediaId.id, nonExistentMediaId];

    // Act
    const mediaItems = await umbracoApi.mediaDeliveryApi.getMediaItemsWithIds(mediaIds);

    // Assert
    expect(mediaItems.status()).toBe(200);
    const mediaItemsJson = await mediaItems.json();
    await umbracoApi.mediaDeliveryApi.verifyMutipleMediaItemsJson([validMediaName], mediaItemsJson, [validMediaPath], [validMediaTypeName]);
  });

  // Gets media item(s) from a query
  test('can fetch children at root', async ({umbracoApi}) => {
    // Arrange
    const fetch = 'children:/';
    const rootItems = await umbracoApi.media.getAllAtRoot();
    const rootItemsJson = await rootItems.json();

    // Act
    const mediaItems = await umbracoApi.mediaDeliveryApi.getMediaItemsFromAQuery(fetch);

    // Assert
    expect(mediaItems).not.toBeNull();
    if (mediaItems !== null) {
      expect(mediaItems.status()).toBe(200);
      const mediaItemsJson = await mediaItems.json();
      expect(mediaItemsJson.total).toBe(rootItemsJson.total);
    }
  });

  test('can fetch children of a media folder', async ({umbracoApi}) => {
    // Arrange
    const mediaFolderData = await umbracoApi.media.getByName(childMediaFolderName);
    const fetch = 'children:/' + rootMediaFolderName.toLowerCase() + '/' + childMediaFolderName.toLowerCase() + '/';
    const childrenItems = await umbracoApi.media.getChildren(mediaFolderData.id);

    // Act
    const mediaItems = await umbracoApi.mediaDeliveryApi.getMediaItemsFromAQuery(fetch);

    // Assert
    expect(mediaItems).not.toBeNull();
    if (mediaItems !== null) {
      expect(mediaItems.status()).toBe(200);
      const mediaItemsJson = await mediaItems.json();
      expect(mediaItemsJson.total).toBe(childrenItems.length);
    }
  });

  test('can filter media items', async ({umbracoApi}) => {
    // Arrange
    const fetch = 'children:/' + rootMediaFolderName.toLowerCase() + '/' + childMediaFolderName.toLowerCase() + '/';
    const filter = 'name:' + imageSizeNamePrefix;

    // Act
    const mediaItems = await umbracoApi.mediaDeliveryApi.getMediaItemsFromAQuery(fetch, filter);

    // Assert
    expect(mediaItems).not.toBeNull();
    if (mediaItems !== null) {
      expect(mediaItems.status()).toBe(200);
      const mediaItemsJson = await mediaItems.json();
      expect(mediaItemsJson.total).toBe(imageSize);
    }
  });

  test('can sort media items', async ({umbracoApi}) => {
    // Arrange
    const fetch = 'children:/' + rootMediaFolderName.toLowerCase() + '/' + childMediaFolderName.toLowerCase() + '/';
    const filter = 'name:' + imageSizeNamePrefix;
    const sort = 'name:desc';

    // Act
    const mediaItems = await umbracoApi.mediaDeliveryApi.getMediaItemsFromAQuery(fetch, filter, sort);

    // Assert
    expect(mediaItems).not.toBeNull();
    if (mediaItems !== null) {
      expect(mediaItems.status()).toBe(200);
      const mediaItemsJson = await mediaItems.json();
      for (let i = 0; i < mediaItemsJson.items.length; i++) {
        expect(mediaItemsJson.items[i].name).toBe(imageSizeNamePrefix + (imageSize - i)); 
      }
    }
  });

  // This test fails because it still returns all media items and ignores the 'take' parameter.
  test('can paginate media items', async ({umbracoApi}) => {
    // Arrange
    const fetch = 'children:/';
    const filter = 'mediaType:' + 'Image';
    const skip = 0;
    const take = rootImageSize;

    // Act
    const mediaItems = await umbracoApi.mediaDeliveryApi.getMediaItemsFromAQuery(fetch, filter, undefined, skip, take);

    // Assert
    expect(mediaItems).not.toBeNull();
    if (mediaItems !== null) {
      expect(mediaItems.status()).toBe(200);
      const mediaItemsJson = await mediaItems.json();
      expect(mediaItemsJson.total).toBe(take);
    }
  });

  test('returns 400 when using an invalid sort parameter', async ({umbracoApi}) => {
    // Arrange
    const fetch = 'children:/';
    const invalidSort= 'invalidSort';

    // Act
    const mediaItems = await umbracoApi.mediaDeliveryApi.getMediaItemsFromAQuery(fetch, undefined, invalidSort);

    // Assert
    expect(mediaItems).not.toBeNull();
    if (mediaItems !== null) {
      expect(mediaItems.status()).toBe(400);
    }
  });

  test('returns 400 when using an invalid filter parameter', async ({umbracoApi}) => {
    // Arrange
    const fetch = 'children:/';
    const invalidFilter = 'invalidFilter';

    // Act
    const mediaItems = await umbracoApi.mediaDeliveryApi.getMediaItemsFromAQuery(fetch, invalidFilter);

    // Assert
    expect(mediaItems).not.toBeNull();
    if (mediaItems !== null) {
      expect(mediaItems.status()).toBe(400);
    }
  });

  test('returns 400 when using an invalid fetch type', async ({umbracoApi}) => {
      // Arrange
    const fetch = 'invalid:/';

    // Act
    const mediaItems = await umbracoApi.mediaDeliveryApi.getMediaItemsFromAQuery(fetch);

    // Assert
    expect(mediaItems).not.toBeNull();
    if (mediaItems !== null) {
      expect(mediaItems.status()).toBe(400);
    }
  });
});