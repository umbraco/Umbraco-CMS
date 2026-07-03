import {ConstantHelper, test} from '@umbraco/acceptance-test-helpers';
import {expect} from '@playwright/test';

let elementTypeId = '';
const elementName = 'TestElement';
const elementTypeName = 'TestElementTypeForElementValidation';
const dataTypeName = 'Textstring';
const elementText = 'This is test element text';

test.beforeEach(async ({umbracoApi}) => {
  await umbracoApi.element.ensureNameNotExists(elementName);
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  elementTypeId = await umbracoApi.documentType.createElementTypeWithPropertyInTab(elementTypeName, 'TestTab', 'TestGroup', dataTypeName, dataTypeData.id, true);
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.element.ensureNameNotExists(elementName);
  await umbracoApi.documentType.ensureNameNotExists(elementTypeName);
});

test('cannot publish an element with an empty mandatory property', async ({umbracoApi, umbracoUi}) => {
  // Arrange
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
