import {ConstantHelper, test, AliasHelper} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

const contentName = 'TestContent';
const documentTypeName = 'TestDocumentTypeForContent';
const dataTypeName = 'Image Cropper';
const imageFileName = 'Umbraco.png';
const imageFilePath = './fixtures/mediaLibrary/' + imageFileName;
const defaultFocalPoint = {
  left: 0.5,
  top: 0.5,
};

test.beforeEach(async ({umbracoApi, umbracoUi}) => {
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoUi.goToBackOffice();
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
});

test('can create content with the image cropper data type', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const expectedState = 'Draft';
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeData.id);
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.clickActionsMenuAtRoot();
  await umbracoUi.content.clickCreateButton();
  await umbracoUi.content.chooseDocumentType(documentTypeName);
  await umbracoUi.content.enterContentName(contentName);
  await umbracoUi.content.uploadFile(imageFilePath);
  await umbracoUi.content.clickSaveButton();

  // Assert
  await umbracoUi.content.isSuccessNotificationVisible();
  expect(await umbracoApi.document.doesNameExist(contentName)).toBeTruthy();
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.variants[0].state).toBe(expectedState);
  expect(contentData.values[0].alias).toEqual(AliasHelper.toAlias(dataTypeName));
  expect(contentData.values[0].value.src).toContain(AliasHelper.toAlias(imageFileName));
  expect(contentData.values[0].value.crops).toEqual([]);
  expect(contentData.values[0].value.focalPoint).toEqual(defaultFocalPoint);
});

test('can publish content with the image cropper data type', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const expectedState = 'Published';
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeData.id);
  await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  //await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.uploadFile(imageFilePath);
  await umbracoUi.content.clickSaveAndPublishButton();

  // Assert
  await umbracoUi.content.doesSuccessNotificationsHaveCount(2);
  expect(await umbracoApi.document.doesNameExist(contentName)).toBeTruthy();
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.variants[0].state).toBe(expectedState);
  expect(contentData.values[0].alias).toEqual(AliasHelper.toAlias(dataTypeName));
  expect(contentData.values[0].value.src).toContain(AliasHelper.toAlias(imageFileName));
  expect(contentData.values[0].value.crops).toEqual([]);
  expect(contentData.values[0].value.focalPoint).toEqual(defaultFocalPoint);
});

test('can create content with the custom image cropper data type', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const customDataTypeName = 'CustomImageCropper';
  const cropAlias = 'TestCropLabel';
  const cropWidth = 100;
  const cropHeight = 50;
  const customDataTypeId = await umbracoApi.dataType.createImageCropperDataTypeWithOneCrop(customDataTypeName, AliasHelper.toAlias(cropAlias), cropWidth, cropHeight);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, customDataTypeName, customDataTypeId);
  await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.uploadFile(imageFilePath);
  await umbracoUi.content.clickSaveAndPublishButton();

  // Assert
  await umbracoUi.content.doesSuccessNotificationsHaveCount(2);
  expect(await umbracoApi.document.doesNameExist(contentName)).toBeTruthy();
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.values[0].alias).toEqual(AliasHelper.toAlias(customDataTypeName));
  expect(contentData.values[0].value.src).toContain(AliasHelper.toAlias(imageFileName));
  expect(contentData.values[0].value.focalPoint).toEqual(defaultFocalPoint);
  expect(contentData.values[0].value.crops[0].alias).toEqual(AliasHelper.toAlias(cropAlias));
  expect(contentData.values[0].value.crops[0].width).toEqual(cropWidth);
  expect(contentData.values[0].value.crops[0].height).toEqual(cropHeight);

  // Clean
  await umbracoApi.dataType.ensureNameNotExists(customDataTypeName);
});

