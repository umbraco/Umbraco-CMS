import {ConstantHelper, NotificationConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {expect} from '@playwright/test';

const mediaFileName = 'TestMediaFile';
const folderName = 'TestFolder';
const mediaTypeName = 'File';

test.beforeEach(async ({umbracoUi, umbracoApi}) => {
  await umbracoApi.media.ensureNameNotExists(mediaFileName);
  await umbracoUi.goToBackOffice();
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.media.ensureNameNotExists(mediaFileName);
});

//TODO: Remove skip when the frontend is ready. Currently it is possible to create an empty media file, should not be possible
test.skip('can not create a empty media file', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoUi.media.goToSection(ConstantHelper.sections.media);

  // Act
  await umbracoUi.media.clickCreateMediaItemButton();
  await umbracoUi.media.clickMediaTypeWithNameButton(mediaTypeName);
  await umbracoUi.media.enterMediaItemName(mediaFileName);
  await umbracoUi.media.clickSaveButton();

  // Assert
  await umbracoUi.media.isErrorNotificationVisible();
  await umbracoUi.media.isMediaTreeItemVisible(mediaFileName, false);
  expect(await umbracoApi.media.doesNameExist(mediaFileName)).toBeFalsy();
});

test('can rename a media file', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const wrongMediaFileName = 'NotACorrectName';
  await umbracoApi.media.ensureNameNotExists(wrongMediaFileName);
  await umbracoApi.media.createDefaultMediaFile(wrongMediaFileName);
  await umbracoUi.media.goToSection(ConstantHelper.sections.media);

  // Arrange
  await umbracoUi.waitForTimeout(1000);
  await umbracoUi.media.clickLabelWithName(wrongMediaFileName, true, true);
  await umbracoUi.media.enterMediaItemName(mediaFileName);
  await umbracoUi.media.clickSaveButton();

  // Assert
  await umbracoUi.media.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  await umbracoUi.media.isMediaTreeItemVisible(mediaFileName);
  expect(await umbracoApi.media.doesNameExist(mediaFileName)).toBeTruthy();
});

const mediaFileTypes = [
  {fileName: 'Article', filePath: 'Article.pdf'},
  {fileName: 'Audio', filePath: 'Audio.mp3'},
  {fileName: 'File', filePath: 'File.txt'},
  {fileName: 'Image', filePath: 'Umbraco.png'},
  {fileName: 'Vector Graphics (SVG)', filePath: 'VectorGraphics.svg'},
  {fileName: 'Video', filePath: 'Video.mp4'}
];

for (const mediaFileType of mediaFileTypes) {
  test(`can create a media file with the ${mediaFileType.fileName} type`, async ({umbracoApi, umbracoUi}) => {
    // Arrange
    await umbracoApi.media.ensureNameNotExists(mediaFileType.fileName);
    await umbracoUi.media.goToSection(ConstantHelper.sections.media);

    // Act
    await umbracoUi.waitForTimeout(1000);
    await umbracoUi.media.clickCreateMediaWithType(mediaFileType.fileName);
    await umbracoUi.media.enterMediaItemName(mediaFileType.fileName);
    await umbracoUi.media.uploadFile('./fixtures/mediaLibrary/' + mediaFileType.filePath);
    await umbracoUi.media.clickSaveButton();

    // Assert
    await umbracoUi.media.doesSuccessNotificationHaveText(NotificationConstantHelper.success.created);
    await umbracoUi.media.reloadMediaTree();
    await umbracoUi.media.isMediaTreeItemVisible(mediaFileType.fileName);
    expect(await umbracoApi.media.doesNameExist(mediaFileType.fileName)).toBeTruthy();

    // Clean
    await umbracoApi.media.ensureNameNotExists(mediaFileType.fileName);
  });
}

test('can create a folder', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.media.ensureNameNotExists(folderName);
  await umbracoUi.media.goToSection(ConstantHelper.sections.media);

  // Act
  await umbracoUi.media.clickCreateMediaItemButton();
  await umbracoUi.media.clickMediaTypeWithNameButton('Folder');
  await umbracoUi.media.enterMediaItemName(folderName);
  await umbracoUi.media.clickSaveButton();

  // Assert
  await umbracoUi.media.doesSuccessNotificationHaveText(NotificationConstantHelper.success.created);
  await umbracoUi.media.isMediaTreeItemVisible(folderName);
  expect(await umbracoApi.media.doesNameExist(folderName)).toBeTruthy();

  // Clean
  await umbracoApi.media.ensureNameNotExists(folderName);
});

test('can trash a folder', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.media.ensureNameNotExists(folderName);
  await umbracoApi.media.createDefaultMediaFolder(folderName);
  await umbracoUi.media.goToSection(ConstantHelper.sections.media);
  await umbracoApi.media.doesNameExist(folderName);

  // Act
  await umbracoUi.media.clickActionsMenuForName(folderName);
  await umbracoUi.media.clickTrashButton();
  await umbracoUi.media.clickConfirmTrashButton();

  // Assert
  await umbracoUi.media.doesSuccessNotificationHaveText(NotificationConstantHelper.success.folderDeleted);
  await umbracoUi.media.isTreeItemVisible(folderName, false);
  expect(await umbracoApi.media.doesNameExist(folderName)).toBeFalsy();
});

test('can create a folder in a folder', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const parentFolderName = 'ParentFolder';
  await umbracoApi.media.ensureNameNotExists(parentFolderName);
  await umbracoApi.media.createDefaultMediaFolder(parentFolderName);
  await umbracoUi.media.goToSection(ConstantHelper.sections.media);

  // Act
  await umbracoUi.media.clickActionsMenuForName(parentFolderName);
  await umbracoUi.media.clickCreateModalButton();
  await umbracoUi.media.clickMediaTypeName('Folder');
  await umbracoUi.media.enterMediaItemName(folderName);
  await umbracoUi.media.clickSaveButton();

  // Assert
  await umbracoUi.media.doesSuccessNotificationHaveText(NotificationConstantHelper.success.created);
  await umbracoUi.media.isMediaTreeItemVisible(parentFolderName);
  await umbracoUi.media.isMediaTreeItemVisible(folderName, false);
  await umbracoUi.media.clickMediaCaretButtonForName(parentFolderName);
  await umbracoUi.media.isMediaTreeItemVisible(folderName, true);

  // Clean
  await umbracoApi.media.ensureNameNotExists(parentFolderName);
});

test('can search for a media file', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const secondMediaFile = 'SecondMediaFile';
  await umbracoApi.media.ensureNameNotExists(secondMediaFile);
  await umbracoApi.media.createDefaultMediaFile(mediaFileName);
  await umbracoApi.media.createDefaultMediaFile(secondMediaFile);
  await umbracoUi.media.goToSection(ConstantHelper.sections.media);

  // Act
  await umbracoUi.media.searchForMediaItemByName(mediaFileName);

  // Assert
  await umbracoUi.media.doesMediaCardsContainAmount(1);
  await umbracoUi.media.doesMediaCardContainText(mediaFileName);

  // Clean
  await umbracoApi.media.ensureNameNotExists(secondMediaFile);
});

test('can trash a media item', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.media.emptyRecycleBin();
  await umbracoApi.media.createDefaultMediaFile(mediaFileName);
  await umbracoUi.media.goToSection(ConstantHelper.sections.media);
  await umbracoApi.media.doesNameExist(mediaFileName);

  // Act
  await umbracoUi.media.clickActionsMenuForName(mediaFileName);
  await umbracoUi.media.clickTrashButton();
  await umbracoUi.media.clickConfirmTrashButton();

  // Assert
  await umbracoUi.media.doesSuccessNotificationHaveText(NotificationConstantHelper.success.movedToRecycleBin);
  await umbracoUi.media.isItemVisibleInRecycleBin(mediaFileName);
  expect(await umbracoApi.media.doesNameExist(mediaFileName)).toBeFalsy();
  expect(await umbracoApi.media.doesMediaItemExistInRecycleBin(mediaFileName)).toBeTruthy();

  // Clean
  await umbracoApi.media.emptyRecycleBin();
});

test('can restore a media item from the recycle bin', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.media.emptyRecycleBin();
  await umbracoApi.media.createDefaultMediaFile(mediaFileName);
  await umbracoApi.media.trashMediaItem(mediaFileName);
  await umbracoUi.media.goToSection(ConstantHelper.sections.media);

  // Act
  await umbracoUi.media.reloadRecycleBin();
  await umbracoUi.media.restoreMediaItem(mediaFileName);

  // Assert
  await umbracoUi.media.doesSuccessNotificationHaveText(NotificationConstantHelper.success.restored);
  await umbracoUi.media.isItemVisibleInRecycleBin(mediaFileName, false);
  await umbracoUi.media.reloadMediaTree();
  await umbracoUi.media.isMediaTreeItemVisible(mediaFileName);
  expect(await umbracoApi.media.doesNameExist(mediaFileName)).toBeTruthy();
  expect(await umbracoApi.media.doesMediaItemExistInRecycleBin(mediaFileName)).toBeFalsy();

  // Clean
  await umbracoApi.media.emptyRecycleBin();
});

test('can delete a media item from the recycle bin', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.media.emptyRecycleBin();
  await umbracoApi.media.createDefaultMediaFile(mediaFileName);
  await umbracoApi.media.trashMediaItem(mediaFileName);
  await umbracoUi.media.goToSection(ConstantHelper.sections.media);

  // Act
  await umbracoUi.media.isItemVisibleInRecycleBin(mediaFileName);
  await umbracoUi.media.deleteMediaItem(mediaFileName);

  // Assert
  await umbracoUi.media.doesSuccessNotificationHaveText(NotificationConstantHelper.success.deleted);
  await umbracoUi.media.isItemVisibleInRecycleBin(mediaFileName, false);
  expect(await umbracoApi.media.doesNameExist(mediaFileName)).toBeFalsy();
  expect(await umbracoApi.media.doesMediaItemExistInRecycleBin(mediaFileName)).toBeFalsy();
});

test('can empty the recycle bin', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.media.emptyRecycleBin();
  await umbracoApi.media.createDefaultMediaFile(mediaFileName);
  await umbracoApi.media.trashMediaItem(mediaFileName);
  await umbracoUi.media.goToSection(ConstantHelper.sections.media);

  // Act
  await umbracoUi.media.isItemVisibleInRecycleBin(mediaFileName);
  await umbracoUi.media.clickEmptyRecycleBinButton();
  await umbracoUi.media.clickConfirmEmptyRecycleBinButton();

  // Assert
  await umbracoUi.media.isItemVisibleInRecycleBin(mediaFileName, false);
  await umbracoUi.media.doesSuccessNotificationHaveText(NotificationConstantHelper.success.emptiedRecycleBin);
  expect(await umbracoApi.media.doesNameExist(mediaFileName)).toBeFalsy();
  expect(await umbracoApi.media.doesMediaItemExistInRecycleBin(mediaFileName)).toBeFalsy();
});
