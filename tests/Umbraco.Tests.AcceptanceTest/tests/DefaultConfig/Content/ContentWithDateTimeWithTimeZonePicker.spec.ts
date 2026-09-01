import {ConstantHelper, NotificationConstantHelper, test, AliasHelper} from '@umbraco/acceptance-test-helpers';
import {expect} from "@playwright/test";

const contentName = 'TestContent';
const documentTypeName = 'TestDocumentTypeForContent';
const dataTypeName = 'Custom Date Time With Time Zone Picker';
const dateTimeValue = '2026-09-01T14:30';
// The picker defaults to the client's local time zone, which is the same machine running the test -
// compute it rather than hardcoding a zone/offset that would only be correct on this one machine.
const expectedClientTimeZone = Intl.DateTimeFormat().resolvedOptions().timeZone;

test.beforeEach(async ({umbracoApi, umbracoUi}) => {
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.dataType.ensureNameNotExists(dataTypeName);
  await umbracoUi.goToBackOffice();
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.dataType.ensureNameNotExists(dataTypeName);
});

test('can create content with the date time with time zone picker data type', {tag: '@release'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const expectedState = 'Draft';
  const dataTypeId = await umbracoApi.dataType.createDefaultDateTimeWithTimeZonePickerDataType(dataTypeName);
  await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeId);
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.clickActionsMenuAtRoot();
  await umbracoUi.content.clickCreateActionMenuOption();
  await umbracoUi.content.chooseDocumentType(documentTypeName);
  await umbracoUi.content.enterContentName(contentName);
  await umbracoUi.content.enterDateInputValue(dateTimeValue);
  await umbracoUi.content.clickSaveButtonAndWaitForContentToBeCreated();

  // Assert
  expect(await umbracoApi.document.doesNameExist(contentName)).toBeTruthy();
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.variants[0].state).toBe(expectedState);
  expect(contentData.values[0].alias).toEqual(AliasHelper.toAlias(dataTypeName));
  expect(contentData.values[0].value.date).toContain('2026-09-01T14:30:00');
  expect(contentData.values[0].value.timeZone).toEqual(expectedClientTimeZone);
});

test('can publish content with the date time with time zone picker data type', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const expectedState = 'Published';
  const dataTypeId = await umbracoApi.dataType.createDefaultDateTimeWithTimeZonePickerDataType(dataTypeName);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeId);
  await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.enterDateInputValue(dateTimeValue);
  await umbracoUi.content.clickSaveAndPublishButtonAndWaitForContentToBeUpdated();

  // Assert
  expect(await umbracoApi.document.doesNameExist(contentName)).toBeTruthy();
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.variants[0].state).toBe(expectedState);
  expect(contentData.values[0].alias).toEqual(AliasHelper.toAlias(dataTypeName));
  expect(contentData.values[0].value.date).toContain('2026-09-01T14:30:00');
  expect(contentData.values[0].value.timeZone).toEqual(expectedClientTimeZone);
});

test('cannot publish content with a mandatory date time with time zone picker field left empty', {tag: '@release'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const dataTypeId = await umbracoApi.dataType.createDefaultDateTimeWithTimeZonePickerDataType(dataTypeName);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeId, 'Test Group', false, false, true);
  await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickSaveAndPublishButton();

  // Assert
  await umbracoUi.content.isValidationMessageVisible(ConstantHelper.validationMessages.emptyDate);
  await umbracoUi.content.doesErrorNotificationHaveText(NotificationConstantHelper.error.documentCouldNotBePublished);

  // Set the value and publish succeeds
  await umbracoUi.content.enterDateInputValue(dateTimeValue);
  await umbracoUi.content.clickSaveAndPublishButtonAndWaitForContentToBeUpdated();

  // Assert
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.values[0].alias).toEqual(AliasHelper.toAlias(dataTypeName));
  expect(contentData.values[0].value.date).toContain('2026-09-01T14:30:00');
  expect(contentData.values[0].value.timeZone).toEqual(expectedClientTimeZone);
});
