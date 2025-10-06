import {expect} from '@playwright/test';
import {test, AliasHelper} from '@umbraco/playwright-testhelpers';

// Media Folders
const rootMediaFolderName = 'RootMediaFolder';
const childMediaFolderName = 'ChildMediaFolder';
let rootFolderId = '';
let childFolderId = '';
// Media Items
const rootImageName = 'RootImage';
const rootArticleName = 'RootArticle';
const rootAudioName = 'RootAudio';
const rootSVGName = 'RootSVG';
const rootCustomMediaName = 'TestCustomMedia';
const fileName = 'TestFile';
const videoName = 'TestVideo';
const imageSizeNamePrefix = 'Test Image Size';
const rootImageSizeNamePrefix = 'Root Test Image Size';
const specialCharacterImageName = ', . ! ? # $ % & * @ é ü ă đ 漢字';
let rootImageId = '';
// Media Type
const customMediaTypeName = 'CustomMediaType';
// Data Type
const textStringDataType = 'Textstring';
const textStringValue = 'This is a test textstring';
// Constants
const imageSize = 3;
const rootImageSize = 5;

test.beforeEach(async ({umbracoApi}) => {
  // Create a folder at root level
  rootFolderId = await umbracoApi.media.createDefaultMediaFolder(rootMediaFolderName);
  // Create child folder
  childFolderId = await umbracoApi.media.createDefaultMediaFolderAndParentId(childMediaFolderName, rootFolderId);
  // Create an image item at root level
  rootImageId = await umbracoApi.media.createDefaultMediaWithImage(rootImageName);
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.media.ensureNameNotExists(rootMediaFolderName);
  await umbracoApi.media.ensureNameNotExists(childMediaFolderName);
  await umbracoApi.media.ensureNameNotExists(rootImageName);
});

test('can fetch an image item by its ID', async ({umbracoApi}) => {
  // Arrange
  const mediaTypeName = 'Image';
  const mediaPath = '/' + rootImageName.toLowerCase() + '/';

  // Act
  const mediaItem = await umbracoApi.mediaDeliveryApi.getMediaItemWithId(rootImageId);

  // Assert
  expect(mediaItem.status()).toBe(200);
  const mediaItemJson = await mediaItem.json();
  await umbracoApi.mediaDeliveryApi.verifyDefaultMediaItemJson(rootImageName, mediaItemJson, mediaPath, mediaTypeName);

  // Clean
  await umbracoApi.media.ensureNameNotExists(rootImageName);
});

test('can fetch an audio item by its ID', async ({umbracoApi}) => {
  // Arrange
  const mediaTypeName = 'Audio';
  // Create an audio item at root level
  const mediaId = await umbracoApi.media.createDefaultMediaWithAudio(rootAudioName);
  const mediaPath = '/' + rootAudioName.toLowerCase() + '/';

  // Act
  const mediaItem = await umbracoApi.mediaDeliveryApi.getMediaItemWithId(mediaId);

  // Assert
  expect(mediaItem.status()).toBe(200);
  const mediaItemJson = await mediaItem.json();
  await umbracoApi.mediaDeliveryApi.verifyDefaultMediaItemJson(rootAudioName, mediaItemJson, mediaPath, mediaTypeName);

  // Clean
  await umbracoApi.media.ensureNameNotExists(rootAudioName);
});

test('can fetch a vector graphics item by its ID', async ({umbracoApi}) => {
  // Arrange
  const mediaTypeName = 'Vector Graphics (SVG)';
  // Create an vector graphic item at root level
  const mediaId = await umbracoApi.media.createDefaultMediaWithSVG(rootSVGName);
  const mediaPath = '/' + rootSVGName.toLowerCase() + '/';

  // Act
  const mediaItem = await umbracoApi.mediaDeliveryApi.getMediaItemWithId(mediaId);

  // Assert
  expect(mediaItem.status()).toBe(200);
  const mediaItemJson = await mediaItem.json();
  await umbracoApi.mediaDeliveryApi.verifyDefaultMediaItemJson(rootSVGName, mediaItemJson, mediaPath, mediaTypeName);

  // Clean
  await umbracoApi.media.ensureNameNotExists(rootSVGName);
});

test('can fetch a media folder by its ID', async ({umbracoApi}) => {
  // Arrange
  const mediaTypeName = 'Folder';
  const mediaPath = '/' + rootMediaFolderName.toLowerCase() + '/';

  // Act
  const mediaItem = await umbracoApi.mediaDeliveryApi.getMediaItemWithId(rootFolderId);

  // Assert
  expect(mediaItem.status()).toBe(200);
  const mediaItemJson = await mediaItem.json();
  await umbracoApi.mediaDeliveryApi.verifyDefaultMediaItemJson(rootMediaFolderName, mediaItemJson, mediaPath, mediaTypeName);

  // Clean
  await umbracoApi.media.ensureNameNotExists(rootMediaFolderName);
});

test('can fetch a media item in a folder in a folder by its ID', async ({umbracoApi}) => {
  // Arrange
  const mediaTypeName = 'File';
  // Create a file item in the child folder
  const mediaId = await umbracoApi.media.createDefaultMediaFileAndParentId(fileName, childFolderId);
  const mediaPath = '/' + rootMediaFolderName.toLowerCase() + '/' + childMediaFolderName.toLowerCase() + '/' + fileName.toLowerCase() + '/';

  // Act
  const mediaItem = await umbracoApi.mediaDeliveryApi.getMediaItemWithId(mediaId);

  // Assert
  expect(mediaItem.status()).toBe(200);
  const mediaItemJson = await mediaItem.json();
  await umbracoApi.mediaDeliveryApi.verifyDefaultMediaItemJson(fileName, mediaItemJson, mediaPath, mediaTypeName);

  // Clean
  await umbracoApi.media.ensureNameNotExists(fileName);  
});

test('can fetch a custom media item by its ID', async ({umbracoApi}) => {
  // Arrange
  // Create custom media item at root level
  const mediaId = await umbracoApi.media.createDefaultMediaWithTextstring(rootCustomMediaName, customMediaTypeName, textStringValue, textStringDataType);
  const mediaPath = '/' + rootCustomMediaName.toLowerCase() + '/';

  // Act
  const mediaItem = await umbracoApi.mediaDeliveryApi.getMediaItemWithId(mediaId);

  // Assert
  expect(mediaItem.status()).toBe(200);
  const mediaItemJson = await mediaItem.json();
  await umbracoApi.mediaDeliveryApi.verifyDefaultMediaItemJson(rootCustomMediaName, mediaItemJson, mediaPath, customMediaTypeName);
  expect(mediaItemJson.properties[AliasHelper.toAlias(textStringDataType)]).toBe(textStringValue);

  // Clean
  await umbracoApi.media.ensureNameNotExists(rootCustomMediaName);
});

test('returns 404 when fetching a non-existent media item', async ({umbracoApi}) => {
  // Arrange
  const nonExistentMediaId = '00000000-0000-0000-0000-000000000000';

  // Act
  const mediaItem = await umbracoApi.mediaDeliveryApi.getMediaItemWithId(nonExistentMediaId);

  // Assert
  expect(mediaItem.status()).toBe(404);
});

test('can fetch a media item by its path', async ({umbracoApi}) => {
  // Arrange
  const mediaTypeName = 'Article';
  // Create a article file item at root level
  await umbracoApi.media.createDefaultMediaWithArticle(rootArticleName);
  const mediaPath = '/' + rootArticleName.toLowerCase() + '/';

  // Act
  const mediaItem = await umbracoApi.mediaDeliveryApi.getMediaItemWithPath(mediaPath);

  // Assert
  expect(mediaItem.status()).toBe(200);
  const mediaItemJson = await mediaItem.json();
  await umbracoApi.mediaDeliveryApi.verifyDefaultMediaItemJson(rootArticleName, mediaItemJson, mediaPath, mediaTypeName);
  
  // Clean
  await umbracoApi.media.ensureNameNotExists(rootArticleName);  
});

// Skip this because it will return 404 error if the path includes # or ?
// Issue link: https://github.com/umbraco/Umbraco-CMS/issues/20024
test.skip('can fetch a media item by its path with special characters', async ({umbracoApi}) => {
  // Arrange
  const mediaTypeName = 'Image';
  // Create an image item at root level and its name has special characters
  await umbracoApi.media.createDefaultMediaWithImage(specialCharacterImageName);
  const mediaPath = '/' + specialCharacterImageName.toLowerCase() + '/';

  // Act
  const mediaItem = await umbracoApi.mediaDeliveryApi.getMediaItemWithPath(mediaPath);

  // Assert
  expect(mediaItem.status()).toBe(200);
  const mediaItemJson = await mediaItem.json();
  await umbracoApi.mediaDeliveryApi.verifyDefaultMediaItemJson(specialCharacterImageName, mediaItemJson, mediaPath, mediaTypeName);

  // Clean
  await umbracoApi.media.ensureNameNotExists(specialCharacterImageName);  
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
  // Create a video item in the child folder
  await umbracoApi.media.createDefaultMediaWithVideoAndParentId(videoName, childFolderId);
  const mediaPath = '/' + rootMediaFolderName.toLowerCase() + '/' + childMediaFolderName.toLowerCase() + '/' + videoName.toLowerCase() + '/';

  // Act
  const mediaItem = await umbracoApi.mediaDeliveryApi.getMediaItemWithId(mediaPath);

  // Assert
  expect(mediaItem.status()).toBe(200);
  const mediaItemJson = await mediaItem.json();
  await umbracoApi.mediaDeliveryApi.verifyDefaultMediaItemJson(videoName, mediaItemJson, mediaPath, mediaTypeName);

  // Clean
  await umbracoApi.media.ensureNameNotExists(videoName);
});

test('returns 404 when fetching a non-existent media path', async ({umbracoApi}) => {
  // Arrange
  const nonExistentMediaPath = '/non-existent-media-path/';

  // Act
  const mediaItem = await umbracoApi.mediaDeliveryApi.getMediaItemWithPath(nonExistentMediaPath);

  // Assert
  expect(mediaItem.status()).toBe(404);
});

test('can fetch multiple media items by their IDs', async ({umbracoApi}) => {
  // Arrange
  const firstMediaName = rootImageName;
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
  await umbracoApi.mediaDeliveryApi.verifyMultipleMediaItemsJson(mediaNames, mediaItemsJson, mediaPaths, mediaTypeNames);
});

test('returns only valid media items when some IDs are invalid', async ({umbracoApi}) => {
  // Arrange
  const validMediaName = rootImageName;
  const validMediaPath = '/' + validMediaName.toLowerCase() + '/';
  const validMediaTypeName = 'Image';
  const validMediaId = await umbracoApi.media.getByName(validMediaName);
  const nonExistentMediaId = '00000000-0000-0000-0000-000000000000';
  const mediaIds = [validMediaId.id, nonExistentMediaId];

  // Act
  const mediaItems = await umbracoApi.mediaDeliveryApi.getMediaItemsWithIds(mediaIds);

  // Assert
  expect(mediaItems.status()).toBe(200);
  const mediaItemsJson = await mediaItems.json();
  await umbracoApi.mediaDeliveryApi.verifyMultipleMediaItemsJson([validMediaName], mediaItemsJson, [validMediaPath], [validMediaTypeName]);
});

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
  // Create multiple image items in the child folder, named have prefix imageSizeNamePrefix
  for (let i = 1; i <= imageSize; i++) {
    await umbracoApi.media.createDefaultMediaWithImageAndParentId(imageSizeNamePrefix + i, childFolderId);
  }
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
  
  // Clean
  for (let i = 1; i <= imageSize; i++) {
    await umbracoApi.media.ensureNameNotExists(imageSizeNamePrefix + i);
  }  
});

test('can sort media items', async ({umbracoApi}) => {
  // Arrange
  // Create multiple image items in the child folder, named have prefix imageSizeNamePrefix
  for (let i = 1; i <= imageSize; i++) {
    await umbracoApi.media.createDefaultMediaWithImageAndParentId(imageSizeNamePrefix + i, childFolderId);
  }
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

  // Clean
  for (let i = 1; i <= imageSize; i++) {
    await umbracoApi.media.ensureNameNotExists(imageSizeNamePrefix + i);
  }  
});

test('can paginate media items', async ({umbracoApi}) => {
  // Arrange
  // Create multiple image items in the root folder, named have prefix rootImageSizeNamePrefix
  for (let i = 1; i <= rootImageSize; i++) {
    await umbracoApi.media.createDefaultMediaWithImage(rootImageSizeNamePrefix + i);
  }
  const fetch = 'children:/';
  const filter = 'name:' + rootImageSizeNamePrefix;
  const skip = 0;
  const take = rootImageSize - 2;

  // Act
  const mediaItems = await umbracoApi.mediaDeliveryApi.getMediaItemsFromAQuery(fetch, filter, undefined, skip, take);

  // Assert
  expect(mediaItems).not.toBeNull();
  if (mediaItems !== null) {
    expect(mediaItems.status()).toBe(200);
    const mediaItemsJson = await mediaItems.json();
    expect(mediaItemsJson.total).toBe(rootImageSize);
    expect(mediaItemsJson.items.length).toBe(take);
  }

  // Clean
  for (let i = 1; i <= rootImageSize; i++) {
    await umbracoApi.media.ensureNameNotExists(rootImageSizeNamePrefix + i);
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