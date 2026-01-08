import {AliasHelper, ConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

const contentName = 'TestContent';
const documentTypeName = 'TestDocumentTypeForContent';
const dataTypeName = 'Image Media Picker';
const customDataTypeName = 'Custom Image Media Picker';
const groupName = 'TestGroup';
const mediaName = 'TestImage';

test.beforeEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
});

test('can save content with a image media picker', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const expectedState = 'Draft';
  const dataType = await umbracoApi.dataType.getByName(dataTypeName);
  await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataType.id, groupName);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.clickActionsMenuAtRoot();
  await umbracoUi.content.clickCreateButton();
  await umbracoUi.content.chooseDocumentType(documentTypeName);
  await umbracoUi.content.enterContentName(contentName);
  await umbracoUi.content.clickSaveButton();

  // Assert
  await umbracoUi.content.isSuccessNotificationVisible();
  expect(await umbracoApi.document.doesNameExist(contentName)).toBeTruthy();
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.variants[0].state).toBe(expectedState);
});

test('can publish content with a image media picker', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const expectedState = 'Published';
  const dataType = await umbracoApi.dataType.getByName(dataTypeName);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataType.id, groupName);
  await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickSaveAndPublishButton();

  // Assert
  await umbracoUi.content.doesSuccessNotificationsHaveCount(2);
  expect(await umbracoApi.document.doesNameExist(contentName)).toBeTruthy();
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.variants[0].state).toBe(expectedState);
});

test('can add an image to the image media picker', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const dataType = await umbracoApi.dataType.getByName(dataTypeName);
  await umbracoApi.media.ensureNameNotExists(mediaName);
  const imageId = await umbracoApi.media.createDefaultMediaWithImage(mediaName);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataType.id, groupName);
  await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickChooseButtonAndSelectMediaWithName(mediaName);
  await umbracoUi.content.clickChooseModalButton();
  await umbracoUi.content.clickSaveButton();

  // Assert
  await umbracoUi.content.isSuccessNotificationVisible();
  expect(await umbracoApi.document.doesImageMediaPickerContainImage(contentName, AliasHelper.toAlias(dataTypeName), imageId)).toBeTruthy();

  // Clean
  await umbracoApi.media.ensureNameNotExists(mediaName);
});

test('can remove an image from the image media picker', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const dataType = await umbracoApi.dataType.getByName(dataTypeName);
  await umbracoApi.media.ensureNameNotExists(mediaName);
  const imageId = await umbracoApi.media.createDefaultMediaWithImage(mediaName);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataType.id, groupName);
  await umbracoApi.document.createDocumentWithImageMediaPicker(contentName, documentTypeId, AliasHelper.toAlias(dataTypeName), imageId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickRemoveButtonForName(mediaName);
  await umbracoUi.content.clickConfirmRemoveButton();
  await umbracoUi.content.clickSaveButton();

  // Assert
  await umbracoUi.content.isSuccessNotificationVisible();
  expect(await umbracoApi.document.doesImageMediaPickerContainImage(contentName, AliasHelper.toAlias(dataTypeName), imageId)).toBeFalsy();

  // Clean
  await umbracoApi.media.ensureNameNotExists(mediaName);
});

// TODO: Remove skip when the front-end is ready as there are currently no displayed error notification.
test.skip('image count can not be less than min amount set in image media picker', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.dataType.ensureNameNotExists(customDataTypeName);
  const dataTypeId = await umbracoApi.dataType.createImageMediaPickerDataType(customDataTypeName, 1);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, customDataTypeName, dataTypeId, groupName);
  await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickSaveButton();

  // Assert
  await umbracoUi.content.isErrorNotificationVisible();

  // Clean
  await umbracoApi.dataType.ensureNameNotExists(customDataTypeName);
});

// TODO: Remove skip when the front-end is ready as there are currently no displayed error notification.
test.skip('image count can not be more than max amount set in image media picker', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.dataType.ensureNameNotExists(customDataTypeName);
  const dataTypeId = await umbracoApi.dataType.createImageMediaPickerDataType(customDataTypeName, 0, 0);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, customDataTypeName, dataTypeId, groupName);
  await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickChooseButtonAndSelectMediaWithName(mediaName);
  await umbracoUi.content.clickSubmitButton();
  await umbracoUi.content.clickSaveButton();

  // Assert
  await umbracoUi.content.isErrorNotificationVisible();

  // Clean
  await umbracoApi.dataType.ensureNameNotExists(customDataTypeName);
});

test('can add an image from the image media picker with a start node', async ({umbracoApi, umbracoUi}) => {
  const mediaFolderName = 'TestFolder';
  await umbracoApi.media.ensureNameNotExists(mediaFolderName);
  await umbracoApi.media.ensureNameNotExists(mediaName);
  await umbracoApi.dataType.ensureNameNotExists(customDataTypeName);
  const imageFolderId = await umbracoApi.media.createDefaultMediaFolder(mediaFolderName);
  const imageId = await umbracoApi.media.createDefaultMediaWithImageAndParentId(mediaName, imageFolderId);
  const dataTypeId = await umbracoApi.dataType.createImageMediaPickerDataTypeWithStartNodeId(customDataTypeName, imageFolderId);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, customDataTypeName, dataTypeId, groupName);
  await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickChooseButtonAndSelectMediaWithName(mediaName);
  await umbracoUi.content.clickChooseModalButton();
  await umbracoUi.content.clickSaveButton();

  // Assert
  await umbracoUi.content.isSuccessNotificationVisible();
  expect(await umbracoApi.document.doesImageMediaPickerContainImage(contentName, AliasHelper.toAlias(customDataTypeName), imageId)).toBeTruthy();

  // Clean
  await umbracoApi.media.ensureNameNotExists(mediaFolderName);
  await umbracoApi.media.ensureNameNotExists(mediaName);
  await umbracoApi.dataType.ensureNameNotExists(customDataTypeName);
});

test('can add an image from the image media picker with focal point enabled', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.media.ensureNameNotExists(mediaName);
  await umbracoApi.dataType.ensureNameNotExists(customDataTypeName);
  const imageId = await umbracoApi.media.createDefaultMediaWithImage(mediaName);
  const dataTypeId = await umbracoApi.dataType.createImageMediaPickerDataType(customDataTypeName, 0, 1, true);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, customDataTypeName, dataTypeId, groupName);
  await umbracoApi.document.createDocumentWithImageMediaPicker(contentName, documentTypeId, AliasHelper.toAlias(customDataTypeName), imageId);
  const widthPercentage = 40;
  const heightPercentage = 20;
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickExactLinkWithName(mediaName);
  await umbracoUi.content.setFocalPoint(widthPercentage, heightPercentage);
  await umbracoUi.content.clickSubmitButton();
  await umbracoUi.content.clickSaveButton();

  // Assert
  await umbracoUi.content.isSuccessNotificationVisible();
  expect(await umbracoApi.document.doesImageMediaPickerContainImageWithFocalPoint(contentName, AliasHelper.toAlias(customDataTypeName), imageId, {left: 0.4, top: 0.2})).toBeTruthy();

  // Clean
  await umbracoApi.media.ensureNameNotExists(mediaName);
  await umbracoApi.dataType.ensureNameNotExists(customDataTypeName);
});

test('can reset focal point in a image from the image media picker', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.media.ensureNameNotExists(mediaName);
  await umbracoApi.dataType.ensureNameNotExists(customDataTypeName);
  const imageId = await umbracoApi.media.createDefaultMediaWithImage(mediaName);
  const dataTypeId = await umbracoApi.dataType.createImageMediaPickerDataType(customDataTypeName, 0, 1, true);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, customDataTypeName, dataTypeId, groupName);
  await umbracoApi.document.createDocumentWithImageMediaPicker(contentName, documentTypeId, AliasHelper.toAlias(customDataTypeName), imageId, {left: 0.4, top: 0.2});
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickExactLinkWithName(mediaName);
  await umbracoUi.content.clickResetFocalPointButton();
  await umbracoUi.content.clickSubmitButton();
  await umbracoUi.content.clickSaveButton();

  // Assert
  await umbracoUi.content.isSuccessNotificationVisible();
  expect(await umbracoApi.document.doesImageMediaPickerContainImageWithFocalPoint(contentName, AliasHelper.toAlias(customDataTypeName), imageId, {left: 0, top: 0})).toBeTruthy();

  // Clean
  await umbracoApi.media.ensureNameNotExists(mediaName);
  await umbracoApi.dataType.ensureNameNotExists(customDataTypeName);
});

// TODO: Remove skip when the front-end is ready as currently the crop is not being selected.
test.skip('can add an image from the image media picker with a image crop', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const cropLabel = 'TestCrop';
  const cropWidth = 100;
  const cropHeight = 100;
  await umbracoApi.media.ensureNameNotExists(mediaName);
  await umbracoApi.dataType.ensureNameNotExists(customDataTypeName);
  const imageId = await umbracoApi.media.createDefaultMediaWithImage(mediaName);
  const dataTypeId = await umbracoApi.dataType.createImageMediaPickerDataTypeWithCrop(customDataTypeName, cropLabel, cropWidth, cropHeight);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, customDataTypeName, dataTypeId, groupName);
  await umbracoApi.document.createDocumentWithImageMediaPicker(contentName, documentTypeId, AliasHelper.toAlias(customDataTypeName), imageId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
});
