import {ConstantHelper, test, AliasHelper, NotificationConstantHelper} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

// Data Type
const elementPickerDataTypeName = 'Custom Element Picker';
// Element
const elementName = 'TestElement';
const elementPickerName = 'TestElementPicker';
// Element Type
const elementTypeName = 'TestElementTypeForElement';
const elementPickerTypeName = 'TestElementTypeForElementPicker';

test.beforeEach(async ({umbracoApi}) => {
  await umbracoApi.documentType.ensureNameNotExists(elementTypeName);
  await umbracoApi.documentType.ensureNameNotExists(elementPickerTypeName);
  await umbracoApi.element.ensureNameNotExists(elementName);
  await umbracoApi.element.ensureNameNotExists(elementPickerName);
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.documentType.ensureNameNotExists(elementTypeName);
  await umbracoApi.documentType.ensureNameNotExists(elementPickerTypeName);
  await umbracoApi.element.ensureNameNotExists(elementName);
  await umbracoApi.element.ensureNameNotExists(elementPickerName);
  await umbracoApi.dataType.ensureNameNotExists(elementPickerDataTypeName);
});

test('can create element with the element picker data type', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const expectedState = 'Draft';
  const elementPickerDataTypeId = await umbracoApi.dataType.createDefaultElementPickerDataType(elementPickerDataTypeName);
  await umbracoApi.documentType.createElementTypeWithPropertyInTab(elementTypeName, 'TestTab', 'TestGroup', elementPickerDataTypeName, elementPickerDataTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.library.goToSection(ConstantHelper.sections.library);

  // Act
  await umbracoUi.library.clickActionsMenuAtRoot();
  await umbracoUi.library.clickCreateActionMenuOption();
  await umbracoUi.library.clickElementButton();
  await umbracoUi.library.clickModalMenuItemWithName(elementTypeName);
  await umbracoUi.library.clickChooseModalButton();
  await umbracoUi.library.enterElementName(elementName);
  await umbracoUi.library.clickSaveButtonAndWaitForElementToBeCreated();

  // Assert
  expect(await umbracoApi.element.doesNameExist(elementName)).toBeTruthy();
  const elementData = await umbracoApi.element.getByName(elementName);
  expect(elementData.variants[0].state).toBe(expectedState);
  expect(elementData.values).toEqual([]);
});

test('can publish element with the element picker data type', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const expectedState = 'Published';
  const elementPickerDataTypeId = await umbracoApi.dataType.createDefaultElementPickerDataType(elementPickerDataTypeName);
  const elementTypeId = await umbracoApi.documentType.createElementTypeWithPropertyInTab(elementTypeName, 'TestTab', 'TestGroup', elementPickerDataTypeName, elementPickerDataTypeId);
  await umbracoApi.element.createDefaultElement(elementName, elementTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.library.goToSection(ConstantHelper.sections.library);

  // Act
  await umbracoUi.library.goToElementWithName(elementName);
  await umbracoUi.library.clickSaveAndPublishButtonAndWaitForElementToBePublished();

  // Assert
  const elementData = await umbracoApi.element.getByName(elementName);
  expect(elementData.variants[0].state).toBe(expectedState);
  expect(elementData.values).toEqual([]);
});

test('can publish element with an element selected', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const elementPickerDataTypeId = await umbracoApi.dataType.createDefaultElementPickerDataType(elementPickerDataTypeName);
  const elementTypeId = await umbracoApi.documentType.createElementTypeWithPropertyInTab(elementTypeName, 'TestTab', 'TestGroup', elementPickerDataTypeName, elementPickerDataTypeId);
  await umbracoApi.element.createDefaultElement(elementName, elementTypeId);
  // Create element picker type and element picker
  const elementPickerTypeId = await umbracoApi.documentType.createEmptyElementType(elementPickerTypeName);
  const elementPickerId = await umbracoApi.element.createDefaultElement(elementPickerName, elementPickerTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.library.goToSection(ConstantHelper.sections.library);

  // Act
  await umbracoUi.library.goToElementWithName(elementName);
  await umbracoUi.library.addElementPicker(elementPickerName);
  await umbracoUi.library.clickSaveAndPublishButtonAndWaitForElementToBeUpdated();

  // Assert
  expect(await umbracoApi.element.doesNameExist(elementName)).toBeTruthy();
  const elementData = await umbracoApi.element.getByName(elementName);
  expect(elementData.values[0].alias).toEqual(AliasHelper.toAlias(elementPickerDataTypeName));
  expect(elementData.values[0].value).toContain(elementPickerId);
});

test('can select multiple elements in the element picker', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const secondElementPickerName = 'SecondElementPicker';
  // Arrange
  const elementPickerDataTypeId = await umbracoApi.dataType.createDefaultElementPickerDataType(elementPickerDataTypeName);
  const elementTypeId = await umbracoApi.documentType.createElementTypeWithPropertyInTab(elementTypeName, 'TestTab', 'TestGroup', elementPickerDataTypeName, elementPickerDataTypeId);
  await umbracoApi.element.createDefaultElement(elementName, elementTypeId);
  // Create element picker type and element pickers
  const elementPickerTypeId = await umbracoApi.documentType.createEmptyElementType(elementPickerTypeName);
  const firstElementPickerId = await umbracoApi.element.createDefaultElement(elementPickerName, elementPickerTypeId);
  const secondElementPickerId = await umbracoApi.element.createDefaultElement(secondElementPickerName, elementPickerTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.library.goToSection(ConstantHelper.sections.library);

  // Act
  await umbracoUi.library.goToElementWithName(elementName);
  await umbracoUi.library.addElementPicker(elementPickerName);
  await umbracoUi.waitForTimeout(ConstantHelper.wait.short); // Wait for element to be added
  await umbracoUi.library.addElementPicker(secondElementPickerName);
  await umbracoUi.library.clickSaveAndPublishButtonAndWaitForElementToBePublished();

  // Assert
  const elementData = await umbracoApi.element.getByName(elementName);
  expect(elementData.values[0].alias).toEqual(AliasHelper.toAlias(elementPickerDataTypeName));
  expect(elementData.values[0].value).toContain(firstElementPickerId);
  expect(elementData.values[0].value).toContain(secondElementPickerId);

  // Clean
  await umbracoApi.element.ensureNameNotExists(secondElementPickerName);
});

// Currently, there is no validation message shown when publishing a mandatory element picker with an empty value
test.skip('can not publish a mandatory element picker with an empty value', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const elementPickerDataTypeId = await umbracoApi.dataType.createDefaultElementPickerDataType(elementPickerDataTypeName);
  const elementTypeId = await umbracoApi.documentType.createElementTypeWithPropertyInTab(elementTypeName, 'TestTab', 'TestGroup', elementPickerDataTypeName, elementPickerDataTypeId, true);
  await umbracoApi.element.createDefaultElement(elementName, elementTypeId);
  // Create element picker type and element picker
  const elementPickerTypeId = await umbracoApi.documentType.createEmptyElementType(elementPickerTypeName);
  const elementPickerId = await umbracoApi.element.createDefaultElement(elementPickerName, elementPickerTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.library.goToSection(ConstantHelper.sections.library);

  // Act
  await umbracoUi.library.goToElementWithName(elementName);
  // Do not pick any element and the validation error appears
  await umbracoUi.library.clickSaveAndPublishButton();
  await umbracoUi.library.isValidationMessageVisible(ConstantHelper.validationMessages.nullValue);
  await umbracoUi.library.doesErrorNotificationHaveText(NotificationConstantHelper.error.documentCouldNotBePublished);
  // Pick an element and the validation error disappears
  await umbracoUi.library.addElementPicker(elementPickerName);
  await umbracoUi.library.isValidationMessageVisible(ConstantHelper.validationMessages.emptyValue, false);
  await umbracoUi.library.clickSaveAndPublishButtonAndWaitForElementToBeUpdated();

  // Assert
  const elementData = await umbracoApi.element.getByName(elementName);
  expect(elementData.values[0].alias).toEqual(AliasHelper.toAlias(elementPickerDataTypeName));
  expect(elementData.values[0].value).toContain(elementPickerId);
});

test('can validate minimum amount in element picker', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const minAmount = 2;
  const elementPickerDataTypeId = await umbracoApi.dataType.createDefaultElementPickerWithValidationLimit(elementPickerDataTypeName, minAmount);
  const elementTypeId = await umbracoApi.documentType.createElementTypeWithPropertyInTab(elementTypeName, 'TestTab', 'TestGroup', elementPickerDataTypeName, elementPickerDataTypeId, true);
  await umbracoApi.element.createDefaultElement(elementName, elementTypeId);
  // Create element picker type and element picker
  const elementPickerTypeId = await umbracoApi.documentType.createEmptyElementType(elementPickerTypeName);
  await umbracoApi.element.createDefaultElement(elementPickerName, elementPickerTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.library.goToSection(ConstantHelper.sections.library);

  // Act
  await umbracoUi.library.goToElementWithName(elementName);
  // Pick only one element when minimum is 2
  await umbracoUi.library.addElementPicker(elementPickerName);
  await umbracoUi.library.clickSaveAndPublishButton();

  // Assert
  await umbracoUi.library.isValidationMessageVisible('This field need more items');
  await umbracoUi.library.doesErrorNotificationHaveText(NotificationConstantHelper.error.documentCouldNotBePublished);
});

test('can validate maximum amount in element picker', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const maxAmount = 1;
  const secondElementPickerName = 'SecondElementPicker';
  const elementPickerDataTypeId = await umbracoApi.dataType.createDefaultElementPickerWithValidationLimit(elementPickerDataTypeName, undefined,maxAmount);
  const elementTypeId = await umbracoApi.documentType.createElementTypeWithPropertyInTab(elementTypeName, 'TestTab', 'TestGroup', elementPickerDataTypeName, elementPickerDataTypeId);
  await umbracoApi.element.createDefaultElement(elementName, elementTypeId);
  // Create element picker type and element pickers
  const elementPickerTypeId = await umbracoApi.documentType.createEmptyElementType(elementPickerTypeName);
  await umbracoApi.element.createDefaultElement(elementPickerName, elementPickerTypeId);
  await umbracoApi.element.createDefaultElement(secondElementPickerName, elementPickerTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.library.goToSection(ConstantHelper.sections.library);

  // Act
  await umbracoUi.library.goToElementWithName(elementName);
  await umbracoUi.library.addElementPicker(elementPickerName);
  await umbracoUi.waitForTimeout(ConstantHelper.wait.short); // Wait for element to be added

  // Assert
  await umbracoUi.library.isChooseButtonVisible(false);

  // Clean
  await umbracoApi.element.ensureNameNotExists(secondElementPickerName);
});

test('can remove an element from the element picker in the element', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const elementPickerDataTypeId = await umbracoApi.dataType.createDefaultElementPickerDataType(elementPickerDataTypeName);
  const elementTypeId = await umbracoApi.documentType.createElementTypeWithPropertyInTab(elementTypeName, 'TestTab', 'TestGroup', elementPickerDataTypeName, elementPickerDataTypeId, true);
  await umbracoApi.element.createDefaultElement(elementName, elementTypeId);
  // Create element picker type and element picker
  const elementPickerTypeId = await umbracoApi.documentType.createEmptyElementType(elementPickerTypeName);
  const elementPickerId = await umbracoApi.element.createDefaultElement(elementPickerName, elementPickerTypeId);
  await umbracoApi.element.createElementWithElementPickers(elementName, elementTypeId, elementPickerDataTypeName, [elementPickerId]);
  await umbracoUi.goToBackOffice();
  await umbracoUi.library.goToSection(ConstantHelper.sections.library);

  // Act
  await umbracoUi.library.goToElementWithName(elementName);
  await umbracoUi.library.removeElementPicker(elementPickerName);
  await umbracoUi.library.clickSaveButtonAndWaitForElementToBeUpdated();

  // Assert
  expect(await umbracoApi.element.doesNameExist(elementName)).toBeTruthy();
  const elementData = await umbracoApi.element.getByName(elementName);
  expect(elementData.values).toEqual([]);
});

test('can remove a not-found element from the element picker in the element', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const elementPickerDataTypeId = await umbracoApi.dataType.createDefaultElementPickerDataType(elementPickerDataTypeName);
  const elementTypeId = await umbracoApi.documentType.createElementTypeWithPropertyInTab(elementTypeName, 'TestTab', 'TestGroup', elementPickerDataTypeName, elementPickerDataTypeId, true);
  await umbracoApi.element.createDefaultElement(elementName, elementTypeId);
  // Create element picker type and element picker
  const elementPickerTypeId = await umbracoApi.documentType.createEmptyElementType(elementPickerTypeName);
  const elementPickerId = await umbracoApi.element.createDefaultElement(elementPickerName, elementPickerTypeId);
  await umbracoApi.element.createElementWithElementPickers(elementName, elementTypeId, elementPickerDataTypeName, [elementPickerId]);
  await umbracoApi.element.ensureNameNotExists(elementPickerName); // Delete the element to make it not-found
  await umbracoUi.goToBackOffice();
  await umbracoUi.library.goToSection(ConstantHelper.sections.library);

  // Act
  await umbracoUi.library.goToElementWithName(elementName);
  await umbracoUi.library.removeNotFoundItem();
  await umbracoUi.library.clickSaveButtonAndWaitForElementToBeUpdated();

  // Assert
  expect(await umbracoApi.element.doesNameExist(elementName)).toBeTruthy();
  const elementData = await umbracoApi.element.getByName(elementName);
  expect(elementData.values).toEqual([]);
});
