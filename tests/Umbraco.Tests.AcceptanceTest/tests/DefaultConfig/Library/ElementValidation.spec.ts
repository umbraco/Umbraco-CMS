import {ConstantHelper, test} from '@umbraco/acceptance-test-helpers';
import {expect} from '@playwright/test';

const elementName = 'TestElement';
const mandatoryElementTypeName = 'TestElementTypeForMandatoryValidation';
const regexElementTypeName = 'TestElementTypeForRegexValidation';
const dataTypeName = 'Textstring';
const elementText = 'This is test element text';
// Only digits are allowed by the regex element type's property.
const numbersOnlyRegex = '^[0-9]+$';
const invalidText = 'abc';
const validText = '12345';

test.beforeEach(async ({umbracoApi}) => {
  await umbracoApi.element.ensureNameNotExists(elementName);
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.element.ensureNameNotExists(elementName);
  await umbracoApi.documentType.ensureNameNotExists(mandatoryElementTypeName);
  await umbracoApi.documentType.ensureNameNotExists(regexElementTypeName);
});

test('cannot publish an element with an empty mandatory property', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  const elementTypeId = await umbracoApi.documentType.createElementTypeWithPropertyInTab(mandatoryElementTypeName, 'TestTab', 'TestGroup', dataTypeName, dataTypeData.id, true);
  await umbracoApi.element.createDefaultElement(elementName, elementTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.library.goToSection(ConstantHelper.sections.library);
  await umbracoUi.library.goToElementWithName(elementName);

  // Act
  await umbracoUi.library.clickSaveAndPublishButton();

  // Assert
  await umbracoUi.library.isValidationMessageVisible(ConstantHelper.validationMessages.nullValue);
  const elementData = await umbracoApi.element.getByName(elementName);
  expect(elementData.variants[0].state).toBe('Draft');
});

test('can publish an element after filling a mandatory property', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  const elementTypeId = await umbracoApi.documentType.createElementTypeWithPropertyInTab(mandatoryElementTypeName, 'TestTab', 'TestGroup', dataTypeName, dataTypeData.id, true);
  await umbracoApi.element.createDefaultElement(elementName, elementTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.library.goToSection(ConstantHelper.sections.library);
  await umbracoUi.library.goToElementWithName(elementName);

  // Act
  await umbracoUi.library.clickSaveAndPublishButton();
  await umbracoUi.library.isValidationMessageVisible(ConstantHelper.validationMessages.nullValue);
  await umbracoUi.library.enterTextstring(elementText);
  await umbracoUi.library.clickSaveAndPublishButtonAndWaitForElementToBePublished();

  // Assert
  const elementData = await umbracoApi.element.getByName(elementName);
  expect(elementData.variants[0].state).toBe('Published');
  expect(elementData.values[0].value).toBe(elementText);
});

test('cannot publish an element with a value that does not match the regex', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  const elementTypeId = await umbracoApi.documentType.createElementTypeWithRegexValidation(regexElementTypeName, 'TestGroup', dataTypeName, dataTypeData.id, numbersOnlyRegex);
  await umbracoApi.element.createDefaultElement(elementName, elementTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.library.goToSection(ConstantHelper.sections.library);
  await umbracoUi.library.goToElementWithName(elementName);

  // Act
  await umbracoUi.library.enterTextstring(invalidText);
  await umbracoUi.library.clickSaveAndPublishButton();

  // Assert
  await umbracoUi.library.isValidationMessageVisible(ConstantHelper.validationMessages.invalidValue);
  const elementData = await umbracoApi.element.getByName(elementName);
  expect(elementData.variants[0].state).toBe('Draft');
});

test('can publish an element with a value that matches the regex', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  const elementTypeId = await umbracoApi.documentType.createElementTypeWithRegexValidation(regexElementTypeName, 'TestGroup', dataTypeName, dataTypeData.id, numbersOnlyRegex);
  await umbracoApi.element.createDefaultElement(elementName, elementTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.library.goToSection(ConstantHelper.sections.library);
  await umbracoUi.library.goToElementWithName(elementName);

  // Act
  await umbracoUi.library.enterTextstring(validText);
  await umbracoUi.library.clickSaveAndPublishButtonAndWaitForElementToBePublished();

  // Assert
  const elementData = await umbracoApi.element.getByName(elementName);
  expect(elementData.variants[0].state).toBe('Published');
  expect(elementData.values[0].value).toBe(validText);
});
