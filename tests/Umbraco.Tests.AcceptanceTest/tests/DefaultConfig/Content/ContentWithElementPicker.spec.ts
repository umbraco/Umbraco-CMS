import {ConstantHelper, test, AliasHelper, NotificationConstantHelper} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

// Data Type
const elementPickerDataTypeName = 'Custom Element Picker';
// Content
const contentName = 'TestContent';
// Document Type
const documentTypeName = 'TestDocumentTypeForContent';
// Element Type
const elementTypeName = 'TestElementType';
// Element
const elementName = 'TestElement';

test.beforeEach(async ({umbracoApi}) => {
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.documentType.ensureNameNotExists(elementTypeName);
  await umbracoApi.element.ensureNameNotExists(elementName);
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.element.ensureNameNotExists(elementName);
  await umbracoApi.documentType.ensureNameNotExists(elementTypeName);
});

test('can create content with the element picker data type', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const expectedState = 'Draft';
  const elementPickerDataTypeId = await umbracoApi.dataType.createDefaultElementPickerDataType(elementPickerDataTypeName);
  await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, elementPickerDataTypeName, elementPickerDataTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.clickActionsMenuAtRoot();
  await umbracoUi.content.clickCreateActionMenuOption();
  await umbracoUi.content.chooseDocumentType(documentTypeName);
  await umbracoUi.content.enterContentName(contentName);
  await umbracoUi.content.clickSaveButtonAndWaitForContentToBeCreated();

  // Assert
  expect(await umbracoApi.document.doesNameExist(contentName)).toBeTruthy();
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.variants[0].state).toBe(expectedState);
  expect(contentData.values).toEqual([]);
});

test('can publish content with the element picker data type', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const expectedState = 'Published';
  const elementPickerDataTypeId = await umbracoApi.dataType.createDefaultElementPickerDataType(elementPickerDataTypeName);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, elementPickerDataTypeName, elementPickerDataTypeId);
  await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickSaveAndPublishButtonAndWaitForContentToBePublished();

  // Assert
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.variants[0].state).toBe(expectedState);
  expect(contentData.values).toEqual([]);
});

test('can publish content with an element selected', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const elementPickerDataTypeId = await umbracoApi.dataType.createDefaultElementPickerDataType(elementPickerDataTypeName);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, elementPickerDataTypeName, elementPickerDataTypeId);
  await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  // Create element type and element
  const elementTypeId = await umbracoApi.documentType.createEmptyElementType(elementTypeName);
  const elementId = await umbracoApi.element.createDefaultElement(elementName, elementTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.addElementPicker(elementName);
  await umbracoUi.content.clickSaveAndPublishButtonAndWaitForContentToBePublished();

  // Assert
  expect(await umbracoApi.document.doesNameExist(contentName)).toBeTruthy();
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.values[0].alias).toEqual(AliasHelper.toAlias(elementPickerDataTypeName));
  expect(contentData.values[0].value).toContain(elementId);
});

test('can select multiple elements in the element picker', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const secondElementName = 'TestElement2';
  const elementPickerDataTypeId = await umbracoApi.dataType.createDefaultElementPickerDataType(elementPickerDataTypeName);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, elementPickerDataTypeName, elementPickerDataTypeId);
  await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  // Create element type and element
  const elementTypeId = await umbracoApi.documentType.createEmptyElementType(elementTypeName);
  const firstElementId = await umbracoApi.element.createDefaultElement(elementName, elementTypeId);
  const secondElementId = await umbracoApi.element.createDefaultElement(secondElementName, elementTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.addElementPicker(elementName);
  await umbracoUi.waitForTimeout(ConstantHelper.wait.short); // Wait for element to be added
  await umbracoUi.content.addElementPicker(secondElementName);
  await umbracoUi.content.clickSaveAndPublishButtonAndWaitForContentToBePublished();

  // Assert
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.values[0].alias).toEqual(AliasHelper.toAlias(elementPickerDataTypeName));
  expect(contentData.values[0].value).toContain(firstElementId);
  expect(contentData.values[0].value).toContain(secondElementId);

  // Clean
  await umbracoApi.element.ensureNameNotExists(secondElementName);
});

test('can not publish a mandatory element picker with an empty value', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const elementPickerDataTypeId = await umbracoApi.dataType.createDefaultElementPickerDataType(elementPickerDataTypeName);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, elementPickerDataTypeName, elementPickerDataTypeId, 'Test Group', false, false, true);
  await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  // Create element type and element
  const elementTypeId = await umbracoApi.documentType.createEmptyElementType(elementTypeName);
  const elementId = await umbracoApi.element.createDefaultElement(elementName, elementTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  // Do not pick any element and the validation error appears
  await umbracoUi.content.clickSaveAndPublishButton();
  await umbracoUi.content.isValidationMessageVisible(ConstantHelper.validationMessages.nullValue);
  await umbracoUi.content.doesErrorNotificationHaveText(NotificationConstantHelper.error.documentCouldNotBePublished);
  // Pick an element and the validation error disappears
  await umbracoUi.content.addElementPicker(elementName);
  await umbracoUi.content.isValidationMessageVisible(ConstantHelper.validationMessages.emptyValue, false);
  await umbracoUi.content.clickSaveAndPublishButtonAndWaitForContentToBeUpdated();

  // Assert
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.values[0].alias).toEqual(AliasHelper.toAlias(elementPickerDataTypeName));
  expect(contentData.values[0].value).toContain(elementId);
});

test('can validate minimum amount in element picker', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const minAmount = 2;
  const elementPickerDataTypeId = await umbracoApi.dataType.createDefaultElementPickerWithValidationLimit(elementPickerDataTypeName, minAmount);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, elementPickerDataTypeName, elementPickerDataTypeId);
  await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  // Create element type and elements
  const elementTypeId = await umbracoApi.documentType.createEmptyElementType(elementTypeName);
  await umbracoApi.element.createDefaultElement(elementName, elementTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  // Pick only one element when minimum is 2
  await umbracoUi.content.addElementPicker(elementName);
  await umbracoUi.content.clickSaveAndPublishButton();

  // Assert
  await umbracoUi.content.isValidationMessageVisible('This field need more items');
  await umbracoUi.content.doesErrorNotificationHaveText(NotificationConstantHelper.error.documentCouldNotBePublished);
});

test('can validate maximum amount in element picker', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const maxAmount = 1;
  const secondElementName = 'TestElement2';
  const elementPickerDataTypeId = await umbracoApi.dataType.createDefaultElementPickerWithValidationLimit(elementPickerDataTypeName, undefined,maxAmount);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, elementPickerDataTypeName, elementPickerDataTypeId);
  await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  // Create element type and elements
  const elementTypeId = await umbracoApi.documentType.createEmptyElementType(elementTypeName);
  await umbracoApi.element.createDefaultElement(elementName, elementTypeId);
  await umbracoApi.element.createDefaultElement(secondElementName, elementTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.addElementPicker(elementName);
  await umbracoUi.waitForTimeout(ConstantHelper.wait.short); // Wait for element to be added

  // Assert
  await umbracoUi.content.isChooseButtonVisible(false);

  // Clean
  await umbracoApi.element.ensureNameNotExists(secondElementName);
});

test('can remove an element from the element picker in the content', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const elementPickerDataTypeId = await umbracoApi.dataType.createDefaultElementPickerDataType(elementPickerDataTypeName);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, elementPickerDataTypeName, elementPickerDataTypeId);
  // Create element type and element
  const elementTypeId = await umbracoApi.documentType.createEmptyElementType(elementTypeName);
  const elementId = await umbracoApi.element.createDefaultElement(elementName, elementTypeId);
  await umbracoApi.document.createDocumentWithElementPickers(contentName, documentTypeId, elementPickerDataTypeName, [elementId]);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.removeElementPicker(elementName);
  await umbracoUi.content.clickSaveButtonAndWaitForContentToBeUpdated();

  // Assert
  expect(await umbracoApi.document.doesNameExist(contentName)).toBeTruthy();
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.values).toEqual([]);
});

test('can remove a not-found element from the element picker in the content', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const elementPickerDataTypeId = await umbracoApi.dataType.createDefaultElementPickerDataType(elementPickerDataTypeName);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, elementPickerDataTypeName, elementPickerDataTypeId);
  // Create element type and element
  const elementTypeId = await umbracoApi.documentType.createEmptyElementType(elementTypeName);
  const elementId = await umbracoApi.element.createDefaultElement(elementName, elementTypeId);
  await umbracoApi.document.createDocumentWithElementPickers(contentName, documentTypeId, elementPickerDataTypeName, [elementId]);
  await umbracoApi.element.ensureNameNotExists(elementName); // Delete the element to make it not-found
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.removeNotFoundItem();
  await umbracoUi.content.clickSaveButtonAndWaitForContentToBeUpdated();

  // Assert
  expect(await umbracoApi.document.doesNameExist(contentName)).toBeTruthy();
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.values).toEqual([]);
});
