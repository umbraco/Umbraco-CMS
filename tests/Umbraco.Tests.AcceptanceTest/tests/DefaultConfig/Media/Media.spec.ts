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
  await umbracoUi.media.goToMediaWithName(wrongMediaFileName);
  await umbracoUi.media.enterMediaItemName(mediaFileName);
  await umbracoUi.media.clickSaveButton();

  // Assert
  await umbracoUi.media.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  await umbracoUi.media.isMediaTreeItemVisible(mediaFileName);
  expect(await umbracoApi.media.doesNameExist(mediaFileName)).toBeTruthy();
});

const mediaFileTypes = [
  {fileName: 'Article', filePath: 'Article.pdf', thumbnail: 'icon-article'},
  {fileName: 'Audio', filePath: 'Audio.mp3', thumbnail: 'icon-audio-lines'},
  {fileName: 'File', filePath: 'File.txt', thumbnail: 'icon-document'},
  {fileName: 'Image', filePath: 'Umbraco.png', thumbnail: 'image'},
  {fileName: 'Vector Graphics (SVG)', filePath: 'VectorGraphics.svg', thumbnail: 'image'},
  {fileName: 'Video', filePath: 'Video.mp4', thumbnail: 'icon-video'}
];

for (const mediaFileType of mediaFileTypes) {
  test(`can create a media file with the ${mediaFileType.fileName} type`, async ({umbracoApi, umbracoUi}) => {
    // Arrange
    await umbracoApi.media.ensureNameNotExists(mediaFileType.fileName);
    await umbracoUi.media.goToSection(ConstantHelper.sections.media);

    // Act
    await umbracoUi.media.clickCreateMediaWithType(mediaFileType.fileName);
    await umbracoUi.media.enterMediaItemName(mediaFileType.fileName);
    await umbracoUi.media.uploadFile('./fixtures/mediaLibrary/' + mediaFileType.filePath);
    await umbracoUi.media.clickSaveButton();

    // Assert
    await umbracoUi.media.doesSuccessNotificationHaveText(NotificationConstantHelper.success.created);
    const mediaData = await umbracoApi.media.getByName(mediaFileType.fileName);
    await umbracoUi.media.doesMediaHaveThumbnail(mediaData.id, mediaFileType.thumbnail, mediaData.urls[0].url);
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
  await umbracoUi.media.doesSuccessNotificationHaveText(NotificationConstantHelper.success.movedToRecycleBin);
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
  await umbracoUi.media.searchForMediaItemByName(secondMediaFile);

  // Assert
  await umbracoUi.media.doesMediaCardsContainAmount(1);
  await umbracoUi.media.doesMediaCardContainText(secondMediaFile);

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
  // Verify the references list not displayed
  await umbracoUi.content.isReferenceHeadlineVisible(false);
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
  await umbracoUi.media.isItemVisibleInRecycleBin(mediaFileName, false, false);
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
  await umbracoUi.media.isItemVisibleInRecycleBin(mediaFileName, false, false);
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
  await umbracoUi.media.isItemVisibleInRecycleBin(mediaFileName, false, false);
  await umbracoUi.media.doesSuccessNotificationHaveText(NotificationConstantHelper.success.emptiedRecycleBin);
  expect(await umbracoApi.media.doesNameExist(mediaFileName)).toBeFalsy();
  expect(await umbracoApi.media.doesMediaItemExistInRecycleBin(mediaFileName)).toBeFalsy();
});

test('can trash a media node with a relation', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const documentPickerName = ['TestPicker', 'DocumentTypeForPicker'];
  await umbracoApi.media.emptyRecycleBin();
  await umbracoApi.media.createDefaultMediaFile(mediaFileName);
  await umbracoApi.media.doesNameExist(mediaFileName);
  // Create a document that have media picker is firstMediaFileName
  await umbracoApi.document.createDefaultDocumentWithOneMediaLink(documentPickerName[0], mediaFileName, documentPickerName[1]);
  await umbracoUi.media.goToSection(ConstantHelper.sections.media);

  // Act
  await umbracoUi.media.clickActionsMenuForName(mediaFileName);
  await umbracoUi.media.clickTrashButton();
  // Verify the references list
  await umbracoUi.media.doesReferenceHeadlineHaveText(ConstantHelper.trashDeleteDialogMessage.referenceHeadline);
  await umbracoUi.media.doesReferenceItemsHaveCount(1);
  await umbracoUi.media.isReferenceItemNameVisible(documentPickerName[0]);
  await umbracoUi.media.clickConfirmTrashButton();

  // Assert
  await umbracoUi.media.doesSuccessNotificationHaveText(NotificationConstantHelper.success.movedToRecycleBin);
  await umbracoUi.media.isItemVisibleInRecycleBin(mediaFileName);
  expect(await umbracoApi.media.doesNameExist(mediaFileName)).toBeFalsy();
  expect(await umbracoApi.media.doesMediaItemExistInRecycleBin(mediaFileName)).toBeTruthy();

  // Clean
  await umbracoApi.media.emptyRecycleBin();
  await umbracoApi.document.ensureNameNotExists(documentPickerName[0]);
  await umbracoApi.documentType.ensureNameNotExists(documentPickerName[1]);
});

test('can bulk trash media nodes with a relation', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const firstMediaFileName = 'FirstMediaFile';
  const secondMediaFileName = 'SecondMediaFile';
  const documentPickerName1 = ['TestPicker1', 'DocumentTypeForPicker1'];
  const documentPickerName2 = ['TestPicker2', 'DocumentTypeForPicker2'];
  await umbracoApi.media.emptyRecycleBin();
  await umbracoApi.media.createDefaultMediaFile(firstMediaFileName);
  await umbracoApi.media.createDefaultMediaFile(secondMediaFileName);
  // Create a document that has a media picker with firstMediaFileName
  await umbracoApi.document.createDefaultDocumentWithOneMediaLink(documentPickerName1[0], firstMediaFileName, documentPickerName1[1]);
  // Create a document that has a media picker with secondMediaFileName
  await umbracoApi.document.createDefaultDocumentWithOneMediaLink(documentPickerName2[0], secondMediaFileName, documentPickerName2[1]);

  // Act
  await umbracoUi.media.goToSection(ConstantHelper.sections.media);
  await umbracoUi.media.selectMediaWithName(firstMediaFileName);
  await umbracoUi.media.selectMediaWithName(secondMediaFileName);
  await umbracoUi.media.clickBulkTrashButton();
  // Verify the references list
  await umbracoUi.media.doesReferenceHeadlineHaveText(ConstantHelper.trashDeleteDialogMessage.bulkReferenceHeadline);
  await umbracoUi.media.doesReferenceItemsHaveCount(2);
  await umbracoUi.media.isReferenceItemNameVisible(firstMediaFileName);
  await umbracoUi.media.isReferenceItemNameVisible(secondMediaFileName);
  await umbracoUi.media.clickConfirmTrashButton();

  // Assert
  await umbracoUi.media.isSuccessNotificationVisible();
  expect(await umbracoApi.media.doesNameExist(firstMediaFileName)).toBeFalsy();
  expect(await umbracoApi.media.doesNameExist(secondMediaFileName)).toBeFalsy();
  expect(await umbracoApi.media.doesMediaItemExistInRecycleBin(firstMediaFileName)).toBeTruthy();
  expect(await umbracoApi.media.doesMediaItemExistInRecycleBin(secondMediaFileName)).toBeTruthy();
  await umbracoUi.media.isItemVisibleInRecycleBin(firstMediaFileName);
  await umbracoUi.media.isItemVisibleInRecycleBin(secondMediaFileName, true, false);

  // Clean
  await umbracoApi.media.ensureNameNotExists(firstMediaFileName);
  await umbracoApi.media.ensureNameNotExists(secondMediaFileName);
  await umbracoApi.document.ensureNameNotExists(documentPickerName1[0]);
  await umbracoApi.documentType.ensureNameNotExists(documentPickerName1[1]);
  await umbracoApi.document.ensureNameNotExists(documentPickerName2[0]);
  await umbracoApi.documentType.ensureNameNotExists(documentPickerName2[1]);
  await umbracoApi.media.emptyRecycleBin();
});