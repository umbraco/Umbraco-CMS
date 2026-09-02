import {ConstantHelper, NotificationConstantHelper, test, AliasHelper} from '@umbraco/acceptance-test-helpers';
import {expect} from "@playwright/test";

const contentName = 'TestContent';
const documentTypeName = 'TestDocumentTypeForContent';
const dataTypeName = 'Numeric';
const customDataTypeName = 'Custom Numeric';
const number = 10;

test.beforeEach(async ({umbracoApi, umbracoUi}) => {
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoUi.goToBackOffice();
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
});

test('can create content with the numeric data type', {tag: '@release'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const expectedState = 'Draft';
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeData.id);
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.clickActionsMenuAtRoot();
  await umbracoUi.content.clickCreateActionMenuOption();
  await umbracoUi.content.chooseDocumentType(documentTypeName);
  await umbracoUi.content.enterContentName(contentName);
  await umbracoUi.content.enterNumeric(number);
  await umbracoUi.content.clickSaveButtonAndWaitForContentToBeCreated();

  // Assert
  expect(await umbracoApi.document.doesNameExist(contentName)).toBeTruthy();
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.variants[0].state).toBe(expectedState);
  expect(contentData.values[0].alias).toEqual(AliasHelper.toAlias(dataTypeName));
  expect(contentData.values[0].value).toEqual(number);
});

test('can publish content with the numeric data type', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const expectedState = 'Published';
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeData.id);
  await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.enterNumeric(number);
  await umbracoUi.content.clickSaveAndPublishButtonAndWaitForContentToBePublished();

  // Assert
  expect(await umbracoApi.document.doesNameExist(contentName)).toBeTruthy();
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.variants[0].state).toBe(expectedState);
  expect(contentData.values[0].alias).toEqual(AliasHelper.toAlias(dataTypeName));
  expect(contentData.values[0].value).toEqual(number);
});

test('cannot publish a numeric value below the configured minimum', {tag: '@release'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const min = 5;
  const max = 100;
  const belowMin = 1;
  const warningMessage = `The value ${belowMin} is less than the allowed minimum value of ${min}`;
  const dataTypeId = await umbracoApi.dataType.createDefaultNumericWithMinMax(customDataTypeName, min, max);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, customDataTypeName, dataTypeId);
  await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.enterNumeric(belowMin);
  await umbracoUi.content.clickSaveAndPublishButton();

  // Assert
  await umbracoUi.content.isFailedStateButtonVisible();
  await umbracoUi.content.isErrorNotificationVisible();
  await umbracoUi.content.isTextWithMessageVisible(warningMessage);

  // Fix the value and the error disappears
  await umbracoUi.content.enterNumeric(min);
  await umbracoUi.content.clickSaveAndPublishButtonAndWaitForContentToBeUpdated();
  await umbracoUi.content.isTextWithMessageVisible(warningMessage, false);

  // Clean
  await umbracoApi.dataType.ensureNameNotExists(customDataTypeName);
});

test('cannot publish a numeric value above the configured maximum', {tag: '@release'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const min = 0;
  const max = 10;
  const aboveMax = 11;
  const warningMessage = `The value ${aboveMax} is greater than the allowed maximum value of ${max}`;
  const dataTypeId = await umbracoApi.dataType.createDefaultNumericWithMinMax(customDataTypeName, min, max);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, customDataTypeName, dataTypeId);
  await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.enterNumeric(aboveMax);
  await umbracoUi.content.clickSaveAndPublishButton();

  // Assert
  await umbracoUi.content.isFailedStateButtonVisible();
  await umbracoUi.content.isErrorNotificationVisible();
  await umbracoUi.content.isTextWithMessageVisible(warningMessage);

  // Fix the value and the error disappears
  await umbracoUi.content.enterNumeric(max);
  await umbracoUi.content.clickSaveAndPublishButtonAndWaitForContentToBeUpdated();
  await umbracoUi.content.isTextWithMessageVisible(warningMessage, false);

  // Clean
  await umbracoApi.dataType.ensureNameNotExists(customDataTypeName);
});

test('can not publish a mandatory numeric with an empty value', {tag: '@release'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeData.id, 'Test Group', false, false, true);
  await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickSaveAndPublishButton();

  // Assert
  await umbracoUi.content.isValidationMessageVisible(ConstantHelper.validationMessages.nullValue);
  await umbracoUi.content.doesErrorNotificationHaveText(NotificationConstantHelper.error.documentCouldNotBePublished);

  // Fill the value and publish succeeds - the mandatory check only re-runs on the next publish attempt,
  // unlike the native range-validity messages above, which clear live as the input changes.
  await umbracoUi.content.enterNumeric(number);
  await umbracoUi.content.clickSaveAndPublishButtonAndWaitForContentToBeUpdated();

  // Assert
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.values[0].value).toEqual(number);
});

