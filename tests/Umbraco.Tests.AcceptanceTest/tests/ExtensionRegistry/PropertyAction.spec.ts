import {ConstantHelper, test} from '@umbraco/playwright-testhelpers';
import { expect } from '@playwright/test';

// Content
const contentName = 'TestContent';
// DocumentType
const documentTypeName = 'TestDocumentTypeForContent';
// GroupName
const groupName = 'Content';
// DataType
const dataTypeName = 'Textstring';
// Property actions name
const writeActionName = 'Write text';
const readActionName = 'Read text';
// Test values
const readTextValue = 'Test text value';
const writeTextValue = 'Hello world';

test.afterEach(async ({umbracoApi}) => {
	await umbracoApi.document.ensureNameNotExists(contentName);
	await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
});

test('can read value from textstring editor using read property action', async ({umbracoApi, umbracoUi}) => {
  // Arrange 
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeData.id, groupName);
  await umbracoApi.document.createDocumentWithTextContent(contentName, documentTypeId, readTextValue, dataTypeName);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickActionsMenuForProperty(groupName, dataTypeName);
  await umbracoUi.content.clickPropertyActionWithName(readActionName);

  // Assert
  await umbracoUi.content.doesSuccessNotificationHaveText(readTextValue);
});

test('can write value to textstring editor using write property action', async ({umbracoApi, umbracoUi}) => {
  // Arrange 
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeData.id, groupName);
  const contentId = await umbracoApi.document.createDocumentWithTextContent(contentName, documentTypeId, '', dataTypeName);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickActionsMenuForProperty(groupName, dataTypeName);
  await umbracoUi.content.clickPropertyActionWithName(writeActionName);
  await umbracoUi.content.clickSaveButtonAndWaitForContentToBeUpdated();

  // Assert
  const updatedContentData = await umbracoApi.document.get(contentId);
  expect(updatedContentData.values[0].value).toBe(writeTextValue);
});