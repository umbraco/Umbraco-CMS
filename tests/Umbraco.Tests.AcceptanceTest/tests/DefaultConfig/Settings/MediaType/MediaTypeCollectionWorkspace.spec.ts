import {expect} from "@playwright/test";
import {ConstantHelper, test} from '@umbraco/acceptance-test-helpers';

const mediaTypeName = 'TestMediaType';
const mediaTypeFolderName = 'TestMediaTypeFolder';

test.beforeEach(async ({umbracoUi, umbracoApi}) => {
  await umbracoApi.mediaType.ensureNameNotExists(mediaTypeName);
  await umbracoApi.mediaType.ensureNameNotExists(mediaTypeFolderName);
  await umbracoUi.goToBackOffice();
  await umbracoUi.mediaType.goToSection(ConstantHelper.sections.settings);
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.mediaType.ensureNameNotExists(mediaTypeName);
  await umbracoApi.mediaType.ensureNameNotExists(mediaTypeFolderName);
});

test('can create a media type using create options', {tag: '@release'}, async ({umbracoApi, umbracoUi}) => {
  // Act
  await umbracoUi.mediaType.clickActionsMenuAtRoot();
  await umbracoUi.mediaType.clickCreateActionMenuOption();
  await umbracoUi.mediaType.clickMediaTypeButton();
  await umbracoUi.mediaType.enterMediaTypeName(mediaTypeName);
  await umbracoUi.mediaType.clickSaveButtonAndWaitForMediaTypeToBeCreated();

  // Assert
  expect(await umbracoApi.mediaType.doesNameExist(mediaTypeName)).toBeTruthy();
  await umbracoUi.mediaType.reloadMediaTypeTree();
  await umbracoUi.mediaType.isMediaTypeTreeItemVisible(mediaTypeName);
  await umbracoUi.mediaType.doesTreeItemHaveTheCorrectIcon(mediaTypeName, 'icon-picture');
});

test('can create a media type folder using create options', {tag: '@release'}, async ({umbracoApi, umbracoUi}) => {
  // Act
  await umbracoUi.mediaType.clickActionsMenuAtRoot();
  await umbracoUi.mediaType.clickCreateActionMenuOption();
  await umbracoUi.mediaType.clickFolderButton();
  await umbracoUi.mediaType.enterFolderName(mediaTypeFolderName);
  await umbracoUi.mediaType.clickConfirmCreateFolderButtonAndWaitForMediaTypeToBeCreated();

  // Assert
  expect(await umbracoApi.mediaType.doesNameExist(mediaTypeFolderName)).toBeTruthy();
  await umbracoUi.mediaType.isMediaTypeTreeItemVisible(mediaTypeFolderName);
  await umbracoUi.mediaType.doesTreeItemHaveTheCorrectIcon(mediaTypeFolderName, 'icon-folder');
});

test('can create a media type in a folder using create options', {tag: '@release'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const mediaTypeFolderId = await umbracoApi.mediaType.createFolder(mediaTypeFolderName);
  await umbracoUi.mediaType.reloadMediaTypeTree();

  // Act
  await umbracoUi.mediaType.clickRootFolderCaretButton();
  await umbracoUi.mediaType.clickActionsMenuForMediaType(mediaTypeFolderName);
  await umbracoUi.mediaType.clickCreateActionMenuOption();
  await umbracoUi.mediaType.clickMediaTypeButton();
  await umbracoUi.mediaType.enterMediaTypeName(mediaTypeName);
  await umbracoUi.mediaType.clickSaveButtonAndWaitForMediaTypeToBeCreated();

  // Assert
  expect(await umbracoApi.mediaType.doesNameExist(mediaTypeName)).toBeTruthy();
  await expect.poll(async () => (await umbracoApi.mediaType.getChildren(mediaTypeFolderId))[0]?.name).toBe(mediaTypeName);
  await umbracoUi.mediaType.goToMediaType(mediaTypeFolderName);
  await umbracoUi.mediaType.doesTreeItemHaveTheCorrectIcon(mediaTypeName, 'icon-picture');
});

test('can create a media type folder in a folder from the tree actions menu', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const mediaTypeFolderId = await umbracoApi.mediaType.createFolder(mediaTypeFolderName);
  await umbracoUi.mediaType.reloadMediaTypeTree();

  // Act
  await umbracoUi.mediaType.clickRootFolderCaretButton();
  await umbracoUi.mediaType.clickActionsMenuForMediaType(mediaTypeFolderName);
  await umbracoUi.mediaType.clickCreateActionMenuOption();
  await umbracoUi.mediaType.clickFolderButton();
  await umbracoUi.mediaType.enterFolderName(childFolderName);
  await umbracoUi.mediaType.clickConfirmCreateFolderButtonAndWaitForMediaTypeToBeCreated();

  // Assert
  expect(await umbracoApi.mediaType.doesNameExist(childFolderName)).toBeTruthy();
  await expect.poll(async () => {
    const [child] = await umbracoApi.mediaType.getChildren(mediaTypeFolderId);
    return {name: child?.name, isFolder: child?.isFolder};
  }).toEqual({name: childFolderName, isFolder: true});
  await umbracoUi.mediaType.openCaretButtonForName(mediaTypeFolderName);
  await umbracoUi.mediaType.doesTreeItemHaveTheCorrectIcon(childFolderName, 'icon-folder');
});
