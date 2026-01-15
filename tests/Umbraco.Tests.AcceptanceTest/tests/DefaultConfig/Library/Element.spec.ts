import {ConstantHelper, NotificationConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

let elementTypeId = '';
let elementId = '';
const elementName = 'TestElement';
const elementTypeName = 'TestElementTypeForElement';
const dataTypeName = 'Textstring';
const elementText = 'This is test element text';

test.beforeEach(async ({umbracoApi}) => {
  await umbracoApi.documentType.ensureNameNotExists(elementTypeName);
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(elementName);
  await umbracoApi.documentType.ensureNameNotExists(elementTypeName);
});

test('can create empty content', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const expectedState = 'Draft';
  await umbracoApi.documentType.createDefaultElementType(elementTypeName);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.clickActionsMenuAtRoot();
  await umbracoUi.content.clickCreateActionMenuOption();
  await umbracoUi.content.chooseDocumentType(elementTypeName);
  await umbracoUi.content.enterElementName(elementName);
  await umbracoUi.content.clickSaveButtonAndWaitForContentToBeCreated();

  // Assert
  expect(await umbracoApi.document.doesNameExist(elementName)).toBeTruthy();
  const contentData = await umbracoApi.document.getByName(elementName);
  expect(contentData.variants[0].state).toBe(expectedState);
});

test('can save and publish empty content', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const expectedState = 'Published';
  await umbracoApi.documentType.createDefaultDocumentTypeWithAllowAsRoot(elementTypeName);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.clickActionsMenuAtRoot();
  await umbracoUi.content.clickCreateActionMenuOption();
  await umbracoUi.content.chooseDocumentType(elementTypeName);
  await umbracoUi.content.enterelementName(elementName);
  await umbracoUi.content.clickSaveAndPublishButtonAndWaitForContentToBeCreated();

  // Assert
  expect(await umbracoApi.document.doesNameExist(elementName)).toBeTruthy();
  const contentData = await umbracoApi.document.getByName(elementName);
  expect(contentData.variants[0].state).toBe(expectedState);
});

test('can create content', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(elementTypeName, dataTypeName, dataTypeData.id);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.clickActionsMenuAtRoot();
  await umbracoUi.content.clickCreateActionMenuOption();
  await umbracoUi.content.chooseDocumentType(elementTypeName);
  await umbracoUi.content.enterelementName(elementName);
  await umbracoUi.content.enterTextstring(elementText);
  await umbracoUi.content.clickSaveButtonAndWaitForContentToBeCreated();

  // Assert
  expect(await umbracoApi.document.doesNameExist(elementName)).toBeTruthy();
  const contentData = await umbracoApi.document.getByName(elementName);
  expect(contentData.values[0].value).toBe(elementText);
});

test('can rename content', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const wrongelementName = 'Wrong Content Name';
  elementTypeId = await umbracoApi.documentType.createDefaultDocumentTypeWithAllowAsRoot(elementTypeName);
  elementId = await umbracoApi.document.createDefaultDocument(wrongelementName, elementTypeId);
  expect(await umbracoApi.document.doesNameExist(wrongelementName)).toBeTruthy();
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(wrongelementName);
  await umbracoUi.content.enterelementName(elementName);
  await umbracoUi.content.clickSaveButtonAndWaitForContentToBeUpdated();

  // Assert
  const updatedContentData = await umbracoApi.document.get(elementId);
  expect(updatedContentData.variants[0].name).toEqual(elementName);
});

test('can update content', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const wrongelementText = 'This is wrong test content text';
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  elementTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(elementTypeName, dataTypeName, dataTypeData.id);
  elementId = await umbracoApi.document.createDocumentWithTextContent(elementName, elementTypeId, wrongelementText, dataTypeName);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(elementName);
  await umbracoUi.content.enterTextstring(elementText);
  await umbracoUi.content.clickSaveButtonAndWaitForContentToBeUpdated();

  // Assert
  const updatedContentData = await umbracoApi.document.get(elementId);
  expect(updatedContentData.variants[0].name).toEqual(elementName);
  expect(updatedContentData.values[0].value).toBe(elementText);
});

test('can publish invariant content node', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  elementTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(elementTypeName, dataTypeName, dataTypeData.id);
  elementId = await umbracoApi.document.createDocumentWithTextContent(elementName, elementTypeId, elementText, dataTypeName);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.clickActionsMenuForContent(elementName);
  await umbracoUi.content.clickPublishActionMenuOption();
  await umbracoUi.content.clickConfirmToPublishButton();

  // Assert
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.published);
  const contentData = await umbracoApi.document.getByName(elementName);
  expect(contentData.variants[0].state).toBe('Published');
});

test('can unpublish content', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  elementTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(elementTypeName, dataTypeName, dataTypeData.id);
  elementId = await umbracoApi.document.createDocumentWithTextContent(elementName, elementTypeId, elementText, dataTypeName);
  await umbracoApi.document.publish(elementId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.clickActionsMenuForContent(elementName);
  await umbracoUi.content.clickUnpublishActionMenuOption();
  await umbracoUi.content.clickConfirmToUnpublishButton();

  // Assert
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.unpublished);
  const contentData = await umbracoApi.document.getByName(elementName);
  expect(contentData.variants[0].state).toBe('Draft');
});

test('can publish variant content node', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  elementTypeId = await umbracoApi.documentType.createVariantDocumentTypeWithInvariantPropertyEditor(elementTypeName, dataTypeName, dataTypeData.id);
  await umbracoApi.document.createDocumentWithEnglishCultureAndTextContent(elementName, elementTypeId, elementText, dataTypeName);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.clickActionsMenuForContent(elementName);
  await umbracoUi.content.clickPublishActionMenuOption();
  await umbracoUi.content.clickConfirmToPublishButton();

  // Assert
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.published);
  const contentData = await umbracoApi.document.getByName(elementName);
  expect(contentData.variants[0].state).toBe('Published');
});

test('can duplicate a content node to root', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const duplicatedelementName = elementName + ' (1)';
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  elementTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(elementTypeName, dataTypeName, dataTypeData.id);
  elementId = await umbracoApi.document.createDocumentWithTextContent(elementName, elementTypeId, elementText, dataTypeName);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.clickActionsMenuForContent(elementName);
  // Duplicate to root
  await umbracoUi.content.clickDuplicateToActionMenuOption();
  await umbracoUi.content.clickLabelWithName('Content');
  await umbracoUi.content.clickDuplicateButton();

  // Assert
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.duplicated);
  expect(await umbracoApi.document.doesNameExist(elementName)).toBeTruthy();
  expect(await umbracoApi.document.doesNameExist(duplicatedelementName)).toBeTruthy();
  await umbracoUi.content.isContentInTreeVisible(elementName);
  await umbracoUi.content.isContentInTreeVisible(duplicatedelementName);
  const contentData = await umbracoApi.document.getByName(elementName);
  const duplicatedContentData = await umbracoApi.document.getByName(duplicatedelementName);
  expect(contentData.values[0].value).toEqual(duplicatedContentData.values[0].value);

  // Clean
  await umbracoApi.document.ensureNameNotExists(duplicatedelementName);
});

test('can duplicate a content node to other parent', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const parentelementTypeName = 'ParentDocumentType';
  const parentelementName = 'ParentContent';
  const listViewDataTypeName = 'List View - Content';
  const listViewDataTypeData  = await umbracoApi.dataType.getByName(listViewDataTypeName);
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  elementTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(elementTypeName, dataTypeName, dataTypeData.id);
  elementId = await umbracoApi.document.createDocumentWithTextContent(elementName, elementTypeId, elementText, dataTypeName);
  const parentelementTypeId = await umbracoApi.documentType.createDocumentTypeWithAllowedChildNodeAndCollectionId(parentelementTypeName, elementTypeId, listViewDataTypeData.id);
  await umbracoApi.document.createDefaultDocument(parentelementName, parentelementTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.clickActionsMenuForContent(elementName);
  await umbracoUi.content.clickDuplicateToActionMenuOption();
  await umbracoUi.content.clickModalMenuItemWithName(parentelementName);
  await umbracoUi.content.clickDuplicateButton();

  // Assert
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.duplicated);
  await umbracoUi.content.isContentInTreeVisible(elementName);
  await umbracoUi.content.isContentInTreeVisible(parentelementName);
  await umbracoUi.content.goToContentWithName(parentelementName);
  await umbracoUi.content.isContentWithNameVisibleInList(elementName);

  // Clean
  await umbracoApi.document.ensureNameNotExists(parentelementName);
  await umbracoApi.document.ensureNameNotExists(parentelementTypeName);
});

// This tests for regression issue: https://github.com/umbraco/Umbraco-CMS/issues/20520
test('can restore a content item from the recycle bin', {tag: '@release'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const restoreMessage = 'Restore ' + elementName + ' to Root';
  await umbracoApi.document.emptyRecycleBin();
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  elementTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(elementTypeName, dataTypeName, dataTypeData.id);
  elementId = await umbracoApi.document.createDocumentWithTextContent(elementName, elementTypeId, elementText, dataTypeName);
  await umbracoApi.document.moveToRecycleBin(elementId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.clickCaretButtonForName('Recycle Bin');
  await umbracoUi.content.clickActionsMenuForContent(elementName);
  await umbracoUi.content.clickRestoreActionMenuOption();
  await umbracoUi.content.isTextWithExactNameVisible(restoreMessage);
  await umbracoUi.content.clickRestoreButton();

  // Assert
  await umbracoUi.content.isSuccessNotificationVisible();
  await umbracoUi.content.isItemVisibleInRecycleBin(elementName, false, false);
  expect(await umbracoApi.document.doesNameExist(elementName)).toBeTruthy();
  expect(await umbracoApi.document.doesItemExistInRecycleBin(elementName)).toBeFalsy();
});
