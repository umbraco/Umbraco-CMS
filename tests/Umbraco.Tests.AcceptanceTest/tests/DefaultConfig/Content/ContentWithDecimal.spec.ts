import {ConstantHelper, NotificationConstantHelper, test, AliasHelper} from '@umbraco/acceptance-test-helpers';
import {expect} from "@playwright/test";

const contentName = 'TestContent';
const documentTypeName = 'TestDocumentTypeForContent';
const customDataTypeName = 'Custom Decimal';
const number = 5.5;

test.beforeEach(async ({umbracoApi, umbracoUi}) => {
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.dataType.ensureNameNotExists(customDataTypeName);
  await umbracoUi.goToBackOffice();
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.dataType.ensureNameNotExists(customDataTypeName);
});

test('cannot publish a decimal value below the configured minimum', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const min = 5;
  const max = 100;
  const belowMin = 1;
  const warningMessage = `The value ${belowMin} is less than the allowed minimum value of ${min}`;
  const dataTypeId = await umbracoApi.dataType.createDecimalDataTypeWithMinAndMax(customDataTypeName, min, max);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, customDataTypeName, dataTypeId);
  await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.enterDecimal(belowMin);
  await umbracoUi.content.clickSaveAndPublishButton();

  // Assert
  await umbracoUi.content.isFailedStateButtonVisible();
  await umbracoUi.content.isErrorNotificationVisible();
  await umbracoUi.content.isTextWithMessageVisible(warningMessage);
  await umbracoUi.content.enterDecimal(min);
  await umbracoUi.content.clickSaveAndPublishButtonAndWaitForContentToBeUpdated();
  await umbracoUi.content.isTextWithMessageVisible(warningMessage, false);
});

test('cannot publish a decimal value above the configured maximum', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const min = 0;
  const max = 10;
  const aboveMax = 11;
  const warningMessage = `The value ${aboveMax} is greater than the allowed maximum value of ${max}`;
  const dataTypeId = await umbracoApi.dataType.createDecimalDataTypeWithMinAndMax(customDataTypeName, min, max);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, customDataTypeName, dataTypeId);
  await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.enterDecimal(aboveMax);
  await umbracoUi.content.clickSaveAndPublishButton();

  // Assert
  await umbracoUi.content.isFailedStateButtonVisible();
  await umbracoUi.content.isErrorNotificationVisible();
  await umbracoUi.content.isTextWithMessageVisible(warningMessage);
  await umbracoUi.content.enterDecimal(max);
  await umbracoUi.content.clickSaveAndPublishButtonAndWaitForContentToBeUpdated();
  await umbracoUi.content.isTextWithMessageVisible(warningMessage, false);
});

test('can not publish a mandatory decimal with an empty value', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const dataTypeId = await umbracoApi.dataType.createDecimalDataType(customDataTypeName);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, customDataTypeName, dataTypeId, 'Test Group', false, false, true);
  await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickSaveAndPublishButton();

  // Assert
  await umbracoUi.content.isValidationMessageVisible(ConstantHelper.validationMessages.nullValue);
  await umbracoUi.content.doesErrorNotificationHaveText(NotificationConstantHelper.error.documentCouldNotBePublished);

  // The mandatory check only re-runs on the next publish attempt, unlike the range messages above
  await umbracoUi.content.enterDecimal(number);
  await umbracoUi.content.clickSaveAndPublishButtonAndWaitForContentToBeUpdated();

  // Assert
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.values[0].alias).toEqual(AliasHelper.toAlias(customDataTypeName));
  expect(contentData.values[0].value).toEqual(number);
});
