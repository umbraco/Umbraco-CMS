import { ConstantHelper, test, AliasHelper } from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

const dataTypeName = 'Media Picker';
const contentName = 'TestContent';
const documentTypeName = 'TestDocumentTypeForContent';
const mediaFileName = 'TestMediaFileForContent';
const mediaTypeName = 'File';
let mediaFileId = '';

test.beforeEach(async ({umbracoApi, umbracoUi}) => {
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.media.ensureNameNotExists(mediaFileName);
  mediaFileId = await umbracoApi.media.createDefaultMedia(mediaFileName, mediaTypeName);
  await umbracoUi.goToBackOffice();
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.media.ensureNameNotExists(mediaFileName);
  await umbracoApi.document.ensureNameNotExists(contentName); 
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
});

test('can create content with the media picker data type', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeData.id);
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.clickActionsMenuAtRoot();
  await umbracoUi.content.clickCreateButton();
  await umbracoUi.content.chooseDocumentType(documentTypeName);
  await umbracoUi.content.enterContentName(contentName);
  await umbracoUi.content.clickChooseMediaPickerButton();
  await umbracoUi.content.selectMediaByName(mediaFileName);
  await umbracoUi.content.clickSubmitButton();
  await umbracoUi.content.clickSaveButton();

  // Assert
  await umbracoUi.content.isSuccessNotificationVisible();
  expect(await umbracoApi.document.doesNameExist(contentName)).toBeTruthy();
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.values[0].alias).toEqual(AliasHelper.toAlias(dataTypeName));
  expect(contentData.values[0].value[0].mediaKey).toEqual(mediaFileId);
  expect(contentData.values[0].value[0].mediaTypeAlias).toEqual(mediaTypeName);
  expect(contentData.values[0].value[0].focalPoint).toBeNull();
  expect(contentData.values[0].value[0].crops).toEqual([]);
});

test('can publish content with the media picker data type', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeData.id);
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.clickActionsMenuAtRoot();
  await umbracoUi.content.clickCreateButton();
  await umbracoUi.content.chooseDocumentType(documentTypeName);
  await umbracoUi.content.enterContentName(contentName);
  await umbracoUi.content.clickChooseMediaPickerButton();
  await umbracoUi.content.selectMediaByName(mediaFileName);
  await umbracoUi.content.clickSubmitButton();
  await umbracoUi.content.clickSaveAndPublishButton();

  // Assert
  await umbracoUi.content.doesSuccessNotificationsHaveCount(2);
  expect(await umbracoApi.document.doesNameExist(contentName)).toBeTruthy();
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.values[0].alias).toEqual(AliasHelper.toAlias(dataTypeName));
  expect(contentData.values[0].value[0].mediaKey).toEqual(mediaFileId);
  expect(contentData.values[0].value[0].mediaTypeAlias).toEqual(mediaTypeName);
  expect(contentData.values[0].value[0].focalPoint).toBeNull();
  expect(contentData.values[0].value[0].crops).toEqual([]);
});

test('can remove a media picker in the content', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeData.id);
  await umbracoApi.document.createDocumentWithOneMediaPicker(contentName, documentTypeId, mediaFileId);
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.openContent(contentName);
  await umbracoUi.content.removeMediaPickerByName(mediaFileName);
  await umbracoUi.content.clickSaveButton();

  // Assert
  await umbracoUi.content.doesSuccessNotificationsHaveCount(1);
  expect(await umbracoApi.document.doesNameExist(contentName)).toBeTruthy();
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.values).toEqual([]);
});

test('can limit the media picker in the content by setting the start node', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const customDataTypeName = 'CustomMediaPicker';
  const mediaFolderName = 'TestMediaFolder';
  const childMediaName = 'ChildMedia';
  await umbracoApi.media.ensureNameNotExists(mediaFolderName);
  const mediaFolderId = await umbracoApi.media.createDefaultMediaFolder(mediaFolderName);
  await umbracoApi.media.ensureNameNotExists(childMediaName);
  await umbracoApi.media.createDefaultMedia(childMediaName, mediaTypeName, mediaFolderId);
  const customDataTypeId = await umbracoApi.dataType.createMediaPickerDataTypeWithStartNodeId(customDataTypeName, mediaFolderId);
  await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, customDataTypeName, customDataTypeId);
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.clickActionsMenuAtRoot();
  await umbracoUi.content.clickCreateButton();
  await umbracoUi.content.chooseDocumentType(documentTypeName);
  await umbracoUi.content.enterContentName(contentName);
  await umbracoUi.content.clickChooseMediaPickerButton();

  // Assert
  await umbracoUi.content.isMediaNameVisible(mediaFolderName, false);
  await umbracoUi.content.isMediaNameVisible(childMediaName, true);

  // Clean
  await umbracoApi.media.ensureNameNotExists(mediaFolderName);
  await umbracoApi.media.ensureNameNotExists(childMediaName);
  await umbracoApi.dataType.ensureNameNotExists(customDataTypeName);
});

