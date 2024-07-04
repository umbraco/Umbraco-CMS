import { ConstantHelper, test, AliasHelper } from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

const contentName = 'TestContent';
const documentTypeName = 'TestDocumentTypeForContent';
const dataTypeName = 'Content Picker';
const contentPickerName = 'TestContentPicker';

test.beforeEach(async ({umbracoApi, umbracoUi}) => {
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoUi.goToBackOffice();
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(contentPickerName);
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
});

test('can create content with the content picker datatype', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeData.id);
  const contentPickerId = await umbracoApi.document.createDefaultDocument(contentPickerName, documentTypeId);
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.clickActionsMenuAtRoot();
  await umbracoUi.content.clickCreateButton();
  await umbracoUi.content.chooseDocumentType(documentTypeName);
  await umbracoUi.content.enterContentName(contentName);
  await umbracoUi.content.addContentPicker(contentPickerName);
  await umbracoUi.content.clickSaveButton();

  // Assert
  await umbracoUi.content.doesSuccessNotificationsHaveCount(1);
  expect(await umbracoApi.document.doesNameExist(contentName)).toBeTruthy();
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.values[0].value).toEqual(contentPickerId);
});

test('can publish content with the content picker data type', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeData.id);
  const contentPickerId = await umbracoApi.document.createDefaultDocument(contentPickerName, documentTypeId);
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.clickActionsMenuAtRoot();
  await umbracoUi.content.clickCreateButton();
  await umbracoUi.content.chooseDocumentType(documentTypeName);
  await umbracoUi.content.enterContentName(contentName);
  await umbracoUi.content.addContentPicker(contentPickerName);
  await umbracoUi.content.clickSaveAndPublishButton();

  // Assert
  await umbracoUi.content.doesSuccessNotificationsHaveCount(2);
  expect(await umbracoApi.document.doesNameExist(contentName)).toBeTruthy();
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.values[0].value).toEqual(contentPickerId);
});

test('can create content with the custom content picker data type', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const customDataTypeName = 'CustomContentPicker';
  const optionValues = ['testOption1', 'testOption2'];
  const customDataTypeId = await umbracoApi.dataType.createCheckboxListDataType(customDataTypeName, optionValues);
  await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, customDataTypeName, customDataTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.clickActionsMenuAtRoot();
  await umbracoUi.content.clickCreateButton();
  await umbracoUi.content.chooseDocumentType(documentTypeName);
  await umbracoUi.content.enterContentName(contentName);
  await umbracoUi.content.chooseCheckboxListOption(optionValues[0]);
  await umbracoUi.content.clickSaveAndPublishButton();

  // Assert
  await umbracoUi.content.doesSuccessNotificationsHaveCount(2);
  expect(await umbracoApi.document.doesNameExist(contentName)).toBeTruthy();
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.values[0].alias).toEqual(AliasHelper.toAlias(customDataTypeName));
  expect(contentData.values[0].value).toEqual([optionValues[0]]);

  // Clean
  await umbracoApi.dataType.ensureNameNotExists(customDataTypeName);
});

