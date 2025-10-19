import {ConstantHelper, test, AliasHelper, NotificationConstantHelper} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

const dataTypeName = 'Media Picker';
const contentName = 'TestContent';
const documentTypeName = 'TestDocumentTypeForContent';
const mediaFileName = 'TestMediaFileForContent';
const mediaTypeName = 'File';
let mediaFileId = '';

test.beforeEach(async ({umbracoApi}) => {
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.media.ensureNameNotExists(mediaFileName);
  mediaFileId = await umbracoApi.media.createDefaultMediaFile(mediaFileName);
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.media.ensureNameNotExists(mediaFileName);
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
});

test('can create content with the media picker data type', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const expectedState = 'Draft';
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeData.id);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.clickActionsMenuAtRoot();
  await umbracoUi.content.clickCreateActionMenuOption();
  await umbracoUi.content.chooseDocumentType(documentTypeName);
  await umbracoUi.content.enterContentName(contentName);
  await umbracoUi.content.clickChooseButtonAndSelectMediaWithName(mediaFileName);
  await umbracoUi.content.clickChooseModalButton();
  await umbracoUi.content.clickSaveButton();

  // Assert
  await umbracoUi.content.waitForContentToBeCreated();
  expect(await umbracoApi.document.doesNameExist(contentName)).toBeTruthy();
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.variants[0].state).toBe(expectedState);
  expect(contentData.values[0].alias).toEqual(AliasHelper.toAlias(dataTypeName));
  expect(contentData.values[0].value[0].mediaKey).toEqual(mediaFileId);
  expect(contentData.values[0].value[0].mediaTypeAlias).toEqual(mediaTypeName);
  expect(contentData.values[0].value[0].focalPoint).toBeNull();
  expect(contentData.values[0].value[0].crops).toEqual([]);
});

test('can publish content with the media picker data type', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const expectedState = 'Published';
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeData.id);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.clickActionsMenuAtRoot();
  await umbracoUi.content.clickCreateActionMenuOption();
  await umbracoUi.content.chooseDocumentType(documentTypeName);
  await umbracoUi.content.enterContentName(contentName);
  await umbracoUi.content.clickChooseButtonAndSelectMediaWithName(mediaFileName);
  await umbracoUi.content.clickChooseModalButton();
  await umbracoUi.content.clickSaveAndPublishButton();

  // Assert
  await umbracoUi.content.waitForContentToBeCreated();
  expect(await umbracoApi.document.doesNameExist(contentName)).toBeTruthy();
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.variants[0].state).toBe(expectedState);
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
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.removeMediaPickerByName(mediaFileName);
  await umbracoUi.content.clickSaveButton();

  // Assert
  await umbracoUi.content.isSuccessStateVisibleForSaveButton();
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
  await umbracoApi.media.createDefaultMediaFileAndParentId(childMediaName, mediaFolderId);
  const customDataTypeId = await umbracoApi.dataType.createMediaPickerDataTypeWithStartNodeId(customDataTypeName, mediaFolderId);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, customDataTypeName, customDataTypeId);
  await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickChooseMediaPickerButton();

  // Assert
  await umbracoUi.content.isMediaNameVisible(mediaFolderName, false);
  await umbracoUi.content.isMediaNameVisible(childMediaName, true);

  // Clean
  await umbracoApi.media.ensureNameNotExists(mediaFolderName);
  await umbracoApi.media.ensureNameNotExists(childMediaName);
  await umbracoApi.dataType.ensureNameNotExists(customDataTypeName);
});

test.fixme('can not publish a mandatory media picker with an empty value', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeData.id, 'Test Group', false, false, true);
  await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  // Do not pick any media and the validation error appears
  await umbracoUi.content.clickSaveAndPublishButton();
  await umbracoUi.content.isValidationMessageVisible(ConstantHelper.validationMessages.emptyValue);
  await umbracoUi.content.doesErrorNotificationHaveText(NotificationConstantHelper.error.documentCouldNotBePublished);
  // Pick a media value and the validation error disappears
  // TODO: This should be fixed
  await umbracoUi.content.clickChooseButtonAndSelectMediaWithName(mediaFileName);
  await umbracoUi.content.clickChooseModalButton();
  await umbracoUi.content.isValidationMessageVisible(ConstantHelper.validationMessages.emptyValue, false);
  await umbracoUi.content.clickSaveAndPublishButton();

  // Assert
  await umbracoUi.content.isSuccessStateVisibleForSaveAndPublishButton();
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.values[0].alias).toEqual(AliasHelper.toAlias(dataTypeName));
  expect(contentData.values[0].value[0].mediaKey).toEqual(mediaFileId);
  expect(contentData.values[0].value[0].mediaTypeAlias).toEqual(mediaTypeName);
  expect(contentData.values[0].value[0].focalPoint).toBeNull();
  expect(contentData.values[0].value[0].crops).toEqual([]);
});

// This is a test for the regression issue #10431
test('can add a media image to a media picker in variant content, remove it and then add it again', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeData.id, 'Test Group', true, true, false);
  await umbracoApi.document.createDefaultDocumentWithEnglishCulture(contentName, documentTypeId);
  const mediaFileId = await umbracoApi.media.createDefaultMediaWithImage(mediaFileName);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  // Adds media item to a media picker
  await umbracoUi.content.clickChooseButtonAndSelectMediaWithKey(mediaFileId);
  await umbracoUi.content.clickChooseModalButton();
  await umbracoUi.content.clickSaveAndPublishButton();
  await umbracoUi.content.isSuccessStateVisibleForSaveAndPublishButton();
  // Removes media item from the media picker
  await umbracoUi.content.removeMediaPickerByName(mediaFileName);
  await umbracoUi.content.clickSaveAndPublishButton();
  await umbracoUi.content.isSuccessStateVisibleForSaveAndPublishButton();

  // Adds media item to a media picker again
  await umbracoUi.content.clickChooseButtonAndSelectMediaWithKey(mediaFileId);
  await umbracoUi.content.clickChooseModalButton();

  // Assert
  await umbracoUi.content.isMediaNameVisible(mediaFileName);
});
