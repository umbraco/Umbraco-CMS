import {ConstantHelper, NotificationConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

const contentName = 'TestContent';
const documentTypeName = 'TestDocumentTypeForContent';
const dataTypeName = 'Content Picker';
const contentPickerDocumentTypeName = 'DocumentTypeForContentPicker';
const contentPickerName = 'TestContentPicker';
let contentPickerDocumentTypeId = '';

test.beforeEach(async ({umbracoApi}) => {
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.document.ensureNameNotExists(contentName);
  contentPickerDocumentTypeId = await umbracoApi.documentType.createDefaultDocumentTypeWithAllowAsRoot(contentPickerDocumentTypeName);
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(contentPickerName);
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.documentType.ensureNameNotExists(contentPickerDocumentTypeName);
});

test('can create content with the content picker datatype', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const expectedState = 'Draft';
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeData.id);
  const contentPickerId = await umbracoApi.document.createDefaultDocument(contentPickerName, contentPickerDocumentTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.clickActionsMenuAtRoot();
  await umbracoUi.content.clickCreateActionMenuOption();
  await umbracoUi.content.chooseDocumentType(documentTypeName);
  await umbracoUi.content.enterContentName(contentName);
  await umbracoUi.content.addContentPicker(contentPickerName);
  await umbracoUi.content.clickSaveButton();

  // Assert
  await umbracoUi.content.waitForContentToBeCreated();
  expect(await umbracoApi.document.doesNameExist(contentName)).toBeTruthy();
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.variants[0].state).toBe(expectedState);
  expect(contentData.values[0].value).toEqual(contentPickerId);
});

test('can publish content with the content picker data type', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const expectedState = 'Published';
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeData.id);
  await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  const contentPickerId = await umbracoApi.document.createDefaultDocument(contentPickerName, contentPickerDocumentTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.addContentPicker(contentPickerName);
  await umbracoUi.content.clickSaveAndPublishButton();

  // Assert
  await umbracoUi.content.isSuccessStateVisibleForSaveAndPublishButton();
  expect(await umbracoApi.document.doesNameExist(contentName)).toBeTruthy();
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.variants[0].state).toBe(expectedState);
  expect(contentData.values[0].value).toEqual(contentPickerId);
});

test('can open content picker in the content', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const customDataTypeName = 'CustomContentPicker';
  const customDataTypeId = await umbracoApi.dataType.createContentPickerDataTypeWithShowOpenButton(customDataTypeName);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, customDataTypeName, customDataTypeId);
  await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  // Create content to pick
  await umbracoApi.document.createDefaultDocument(contentPickerName, contentPickerDocumentTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.addContentPicker(contentPickerName);

  // Assert
  await umbracoUi.content.clickReferenceNodeLinkWithName(contentPickerName);
  await umbracoUi.content.isNodeOpenForContentPicker(contentPickerName);

  // Clean
  await umbracoApi.dataType.ensureNameNotExists(customDataTypeName);
});

test('can create content with content picker without ignore start node', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const customDataTypeName = 'CustomContentPicker';
  const childContentPickerDocumentTypeName = 'ChildDocumentTypeForContentPicker';
  const childContentPickerName = 'TestChildContentPicker';
  await umbracoApi.documentType.ensureNameNotExists(childContentPickerDocumentTypeName);
  const childContentPickerDocumentTypeId = await umbracoApi.documentType.createDefaultDocumentType(childContentPickerDocumentTypeName);
  contentPickerDocumentTypeId = await umbracoApi.documentType.createDocumentTypeWithAllowedChildNode(contentPickerName, childContentPickerDocumentTypeId);
  const contentPickerId = await umbracoApi.document.createDefaultDocument(contentPickerName, contentPickerDocumentTypeId);
  await umbracoApi.document.createDefaultDocumentWithParent(childContentPickerName, childContentPickerDocumentTypeId, contentPickerId);
  // Create a custom content picker with start node
  const customDataTypeId = await umbracoApi.dataType.createContentPickerDataTypeWithStartNode(customDataTypeName, contentPickerId);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, customDataTypeName, customDataTypeId);
  await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.addContentPicker(childContentPickerName);
  await umbracoUi.content.clickSaveAndPublishButton();

  // Assert
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.published);

  // Clean
  await umbracoApi.dataType.ensureNameNotExists(customDataTypeName);
  await umbracoApi.document.ensureNameNotExists(childContentPickerName);
  await umbracoApi.documentType.ensureNameNotExists(childContentPickerDocumentTypeName);
});

test('can create content with content picker with ignore start node', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const customDataTypeName = 'CustomContentPicker';
  const childContentPickerDocumentTypeName = 'ChildDocumentTypeForContentPicker';
  const childContentPickerName = 'TestChildContentPicker';
  await umbracoApi.documentType.ensureNameNotExists(childContentPickerDocumentTypeName);
  const childContentPickerDocumentTypeId = await umbracoApi.documentType.createDefaultDocumentType(childContentPickerDocumentTypeName);
  contentPickerDocumentTypeId = await umbracoApi.documentType.createDocumentTypeWithAllowedChildNode(contentPickerName, childContentPickerDocumentTypeId);
  const contentPickerId = await umbracoApi.document.createDefaultDocument(contentPickerName, contentPickerDocumentTypeId);
  await umbracoApi.document.createDefaultDocumentWithParent(childContentPickerName, childContentPickerDocumentTypeId, contentPickerId);
  // Create a custom content picker with the setting "ignore user start node" is enable
  const customDataTypeId = await umbracoApi.dataType.createContentPickerDataTypeWithIgnoreUserStartNodes(customDataTypeName, contentPickerId);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, customDataTypeName, customDataTypeId);
  await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.addContentPicker(childContentPickerName);
  await umbracoUi.content.clickSaveAndPublishButton();

  // Assert
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.published);

  // Clean
  await umbracoApi.dataType.ensureNameNotExists(customDataTypeName);
  await umbracoApi.document.ensureNameNotExists(childContentPickerName);
  await umbracoApi.documentType.ensureNameNotExists(childContentPickerDocumentTypeName);
});

test('can remove content picker in the content', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeData.id);
  const contentPickerId = await umbracoApi.document.createDefaultDocument(contentPickerName, contentPickerDocumentTypeId);
  await umbracoApi.document.createDocumentWithContentPicker(contentName, documentTypeId, contentPickerId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.removeContentPicker(contentPickerName);
  await umbracoUi.content.clickSaveButton();

  // Assert
  await umbracoUi.content.isSuccessStateVisibleForSaveButton();
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.values).toEqual([]);
});