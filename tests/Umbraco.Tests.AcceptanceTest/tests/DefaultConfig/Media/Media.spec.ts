import {ConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {expect} from '@playwright/test';

test.describe('Media tests', () => {
  const mediaFileName = 'TestMediaFile';
  const folderName = 'TestFolder';

  test.beforeEach(async ({umbracoUi, umbracoApi}) => {
    await umbracoApi.media.ensureNameNotExists(mediaFileName);
    await umbracoUi.goToBackOffice();
  });

  test.afterEach(async ({umbracoApi}) => {
    await umbracoApi.media.ensureNameNotExists(mediaFileName);
  });

  //TODO: It is currently possible to create an empty mediaFile, should not be possible
  test.skip('can not create a empty media file', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    await umbracoUi.media.goToSection(ConstantHelper.sections.media);

    // Act
    await umbracoUi.media.clickCreateMediaItemButton();
    await umbracoUi.media.clickMediaTypeWithNameButton('File');
    await umbracoUi.media.enterMediaItemName(mediaFileName);
    await umbracoUi.media.clickSaveButton();

    // Assert
    await umbracoUi.media.isErrorNotificationVisible();
    await umbracoUi.media.isTreeItemVisible(mediaFileName, false);
    expect(await umbracoApi.media.doesNameExist(mediaFileName)).toBeFalsy();
  });

  test('can rename a media file', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const wrongMediaFileName = 'NotACorrectName';
    await umbracoApi.media.ensureNameNotExists(wrongMediaFileName);
    const mediaType = await umbracoApi.mediaType.getByName('File');
    await umbracoApi.media.createDefaultMedia(wrongMediaFileName, mediaType.id);
    await umbracoUi.media.goToSection(ConstantHelper.sections.media);

    // Arrange
    await umbracoUi.media.clickLabelWithName(wrongMediaFileName)
    await umbracoUi.media.enterMediaItemName(mediaFileName);
    await umbracoUi.media.clickSaveButton();

    // Assert
    await umbracoUi.media.isSuccessNotificationVisible();
    await umbracoUi.media.isTreeItemVisible(mediaFileName);
    expect(await umbracoApi.media.doesNameExist(mediaFileName)).toBeTruthy();
  });

  // TODO: Uncomment the mediaTypes when they are able to be created.
  const mediaFileTypes = [
    // {fileName: 'Article', filePath: 'Article.pdf'},
    // {fileName: 'Audio', filePath: 'Audio.mp3'},
    {fileName: 'File', filePath: 'File.txt'},
    {fileName: 'Image', filePath: 'Umbraco.png'},
    // {fileName: 'Vector Graphics (SVG)', filePath: 'VectorGraphics.svg'},
    // {fileName: 'Video', filePath: 'Video.mp4'}
  ];

  for (const mediaFileType of mediaFileTypes) {
    test(`can create a media file with the ${mediaFileType.fileName} type`, async ({umbracoApi, umbracoUi}) => {
      // Arrange
      await umbracoApi.media.ensureNameNotExists(mediaFileType.fileName);
      await umbracoUi.media.goToSection(ConstantHelper.sections.media);

      // Act
      await umbracoUi.media.clickCreateMediaItemButton();
      await umbracoUi.media.clickMediaTypeWithNameButton(mediaFileType.fileName);
      await umbracoUi.media.enterMediaItemName(mediaFileType.fileName);
      await umbracoUi.media.changeFileTypeWithFileChooser('./fixtures/mediaLibrary/' + mediaFileType.filePath);
      await umbracoUi.media.clickSaveButton();

      // Assert
      await umbracoUi.media.isSuccessNotificationVisible();
      await umbracoUi.media.isTreeItemVisible(mediaFileType.fileName);
      expect(await umbracoApi.media.doesNameExist(mediaFileType.fileName)).toBeTruthy();

      // Clean
      await umbracoApi.media.ensureNameNotExists(mediaFileType.fileName);
    });
  }

  test('can delete a media file', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const mediaType = await umbracoApi.mediaType.getByName('File');
    await umbracoApi.media.createDefaultMedia(mediaFileName, mediaType.id);
    await umbracoUi.media.goToSection(ConstantHelper.sections.media);
    await umbracoApi.media.doesNameExist(mediaFileName);

    // Act
    await umbracoUi.media.clickActionsMenuForName(mediaFileName);
    await umbracoUi.media.clickDeleteThreeDotsButton();
    await umbracoUi.media.clickDeleteExactLabel();

    // Assert
    await umbracoUi.media.isSuccessNotificationVisible();
    await umbracoUi.media.isTreeItemVisible(mediaFileName, false);
    expect(await umbracoApi.media.doesNameExist(mediaFileName)).toBeFalsy();

  });

  test('can create a folder', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    await umbracoApi.media.ensureNameNotExists(folderName);

    // Act
    await umbracoUi.media.goToSection(ConstantHelper.sections.media);
    await umbracoUi.media.clickCreateMediaItemButton();
    await umbracoUi.media.clickMediaTypeWithNameButton('Folder');
    await umbracoUi.media.enterMediaItemName(folderName);
    await umbracoUi.media.clickSaveButton();

    // Assert
    await umbracoUi.media.isSuccessNotificationVisible();
    await umbracoUi.media.isTreeItemVisible(folderName);
    expect(await umbracoApi.media.doesNameExist(folderName)).toBeTruthy();

    // Clean
    await umbracoApi.media.ensureNameNotExists(folderName);
  });

  test('can delete a folder', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    await umbracoApi.media.ensureNameNotExists(folderName);
    const mediaType = await umbracoApi.mediaType.getByName('Folder');
    await umbracoApi.media.createDefaultMedia(folderName, mediaType.id);
    await umbracoUi.media.goToSection(ConstantHelper.sections.media);
    await umbracoApi.media.doesNameExist(folderName);

    // Act
    await umbracoUi.media.clickActionsMenuForName(folderName);
    await umbracoUi.media.clickDeleteThreeDotsButton();
    await umbracoUi.media.clickDeleteExactLabel();

    // Assert
    await umbracoUi.media.isTreeItemVisible(folderName, false);
    expect(await umbracoApi.media.doesNameExist(folderName)).toBeFalsy();
  });

  test('can create a folder in a folder', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const parentFolderName = 'ParentFolder';
    await umbracoApi.media.ensureNameNotExists(parentFolderName);
    const mediaType = await umbracoApi.mediaType.getByName('Folder');
    await umbracoApi.media.createDefaultMedia(parentFolderName, mediaType.id);
    await umbracoUi.media.goToSection(ConstantHelper.sections.media);

    // Act
    await umbracoUi.media.clickActionsMenuForName(parentFolderName);
    await umbracoUi.media.clickCreateModalButton();
    await umbracoUi.media.clickExactLinkWithName('Folder');
    await umbracoUi.media.enterMediaItemName(folderName);
    await umbracoUi.media.clickSaveButton();

    // Assert
    await umbracoUi.media.isSuccessNotificationVisible();
    await umbracoUi.media.isTreeItemVisible(parentFolderName);
    await umbracoUi.media.clickCaretButtonForName(parentFolderName);
    await umbracoUi.media.isTreeItemVisible(folderName);

    // Clean
    await umbracoApi.media.ensureNameNotExists(parentFolderName);
  });

  test('can search for a media file', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const secondMediaFile = 'SecondMediaFile';
    await umbracoApi.media.ensureNameNotExists(secondMediaFile);
    const mediaType = await umbracoApi.mediaType.getByName('File');
    await umbracoApi.media.createDefaultMedia(mediaFileName, mediaType.id);
    await umbracoApi.media.createDefaultMedia(secondMediaFile, mediaType.id);
    await umbracoUi.media.goToSection(ConstantHelper.sections.media);

    // Act
    await umbracoUi.media.searchForMediaItemByName(mediaFileName);

    // Assert
    await umbracoUi.media.doesMediaCardsContainAmount(1);
    await umbracoUi.media.doesMediaCardContainText(mediaFileName);

    // Clean
    await umbracoApi.media.ensureNameNotExists(secondMediaFile);
  });
});
