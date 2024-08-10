import { ConstantHelper, test, AliasHelper } from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

const dataTypeName = 'Multiple Media Picker';
const contentName = 'TestContent';
const documentTypeName = 'TestDocumentTypeForContent';
const firstMediaFileName = 'TestFirstMedia';
const firstMediaTypeName = 'File';
const secondMediaFileName = 'TestSecondMedia';
const secondMediaTypeName = 'Image';
let firstMediaFileId = '';
let secondMediaFileId = '';

test.beforeEach(async ({umbracoApi, umbracoUi}) => {
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.media.ensureNameNotExists(firstMediaFileName);
  firstMediaFileId = await umbracoApi.media.createDefaultMedia(firstMediaFileName, firstMediaTypeName);
  await umbracoApi.media.ensureNameNotExists(secondMediaFileName);
  secondMediaFileId = await umbracoApi.media.createDefaultMedia(secondMediaFileName, secondMediaTypeName);
  await umbracoUi.goToBackOffice();
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.media.ensureNameNotExists(firstMediaFileName);
  await umbracoApi.media.ensureNameNotExists(secondMediaFileName);
  await umbracoApi.document.ensureNameNotExists(contentName); 
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
});

test('can create content with multiple media picker data type', async ({umbracoApi, umbracoUi}) => {
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
  await umbracoUi.content.selectMediaByName(firstMediaFileName);
  await umbracoUi.content.selectMediaByName(secondMediaFileName);
  await umbracoUi.content.clickSubmitButton();
  await umbracoUi.content.clickSaveButton();

  // Assert
  await umbracoUi.content.doesSuccessNotificationsHaveCount(1);
  expect(await umbracoApi.document.doesNameExist(contentName)).toBeTruthy();
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.values[0].alias).toEqual(AliasHelper.toAlias(dataTypeName));
  expect(contentData.values[0].value.length).toBe(2);
  expect(contentData.values[0].value[0].mediaKey).toEqual(firstMediaFileId);
  expect(contentData.values[0].value[0].mediaTypeAlias).toEqual(firstMediaTypeName);
  expect(contentData.values[0].value[1].mediaKey).toEqual(secondMediaFileId);
  expect(contentData.values[0].value[1].mediaTypeAlias).toEqual(secondMediaTypeName);
});

test('can publish content with multiple media picker data type', async ({umbracoApi, umbracoUi}) => {
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
  await umbracoUi.content.selectMediaByName(firstMediaFileName);
  await umbracoUi.content.selectMediaByName(secondMediaFileName);
  await umbracoUi.content.clickSubmitButton();
  await umbracoUi.content.clickSaveAndPublishButton();

  // Assert
  await umbracoUi.content.doesSuccessNotificationsHaveCount(2);
  expect(await umbracoApi.document.doesNameExist(contentName)).toBeTruthy();
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.values[0].alias).toEqual(AliasHelper.toAlias(dataTypeName));
  expect(contentData.values[0].value.length).toBe(2);
  expect(contentData.values[0].value[0].mediaKey).toEqual(firstMediaFileId);
  expect(contentData.values[0].value[0].mediaTypeAlias).toEqual(firstMediaTypeName);
  expect(contentData.values[0].value[1].mediaKey).toEqual(secondMediaFileId);
  expect(contentData.values[0].value[1].mediaTypeAlias).toEqual(secondMediaTypeName);
});

test('can remove a media picker in the content', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeData.id);
  await umbracoApi.document.createDocumentWithTwoMediaPicker(contentName, documentTypeId, firstMediaFileId, secondMediaFileId);
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.openContent(contentName);
  await umbracoUi.content.removeMediaPickerByName(firstMediaFileName);
  await umbracoUi.content.clickSaveButton();

  // Assert
  await umbracoUi.content.doesSuccessNotificationsHaveCount(1);
  expect(await umbracoApi.document.doesNameExist(contentName)).toBeTruthy();
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.values[0].alias).toEqual(AliasHelper.toAlias(dataTypeName));
  expect(contentData.values[0].value.length).toBe(1);
  expect(contentData.values[0].value[0].mediaKey).toEqual(secondMediaFileId);
  expect(contentData.values[0].value[0].mediaTypeAlias).toEqual(secondMediaTypeName);
});

