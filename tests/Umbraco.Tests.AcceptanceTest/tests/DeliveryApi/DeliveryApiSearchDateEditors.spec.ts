import {expect} from '@playwright/test';
import {AliasHelper, test} from '@umbraco/acceptance-test-helpers';

const publishedIndexAlias = 'Umb_PublishedContent';
const dateOnlyDataTypeName = 'DeliveryApiSearchDateEditorsDateOnly';
const timeOnlyDataTypeName = 'DeliveryApiSearchDateEditorsTimeOnly';
const dateTimeUnspecifiedDataTypeName = 'DeliveryApiSearchDateEditorsDateTimeUnspecified';
const dateTimeWithTimeZoneDataTypeName = 'DeliveryApiSearchDateEditorsDateTimeWithTimeZone';
const dateOnlyDocumentName = 'DeliveryApiSearchDateEditorsDateOnlyDocument';
const timeOnlyDocumentName = 'DeliveryApiSearchDateEditorsTimeOnlyDocument';
const dateTimeUnspecifiedDocumentName = 'DeliveryApiSearchDateEditorsDateTimeUnspecifiedDocument';
const dateTimeWithTimeZoneDocumentName = 'DeliveryApiSearchDateEditorsDateTimeWithTimeZoneDocument';
const dateOnlyDocumentTypeName = 'DeliveryApiSearchDateEditorsDateOnlyDocumentType';
const timeOnlyDocumentTypeName = 'DeliveryApiSearchDateEditorsTimeOnlyDocumentType';
const dateTimeUnspecifiedDocumentTypeName = 'DeliveryApiSearchDateEditorsDateTimeUnspecifiedDocumentType';
const dateTimeWithTimeZoneDocumentTypeName = 'DeliveryApiSearchDateEditorsDateTimeWithTimeZoneDocumentType';
const templateName = 'DeliveryApiSearchDateEditorsTemplate';

let templateId = '';

test.beforeEach(async ({umbracoApi}) => {
  templateId = await umbracoApi.template.createDefaultTemplate(templateName) ?? '';
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(dateOnlyDocumentName);
  await umbracoApi.document.ensureNameNotExists(timeOnlyDocumentName);
  await umbracoApi.document.ensureNameNotExists(dateTimeUnspecifiedDocumentName);
  await umbracoApi.document.ensureNameNotExists(dateTimeWithTimeZoneDocumentName);
  await umbracoApi.documentType.ensureNameNotExists(dateOnlyDocumentTypeName);
  await umbracoApi.documentType.ensureNameNotExists(timeOnlyDocumentTypeName);
  await umbracoApi.documentType.ensureNameNotExists(dateTimeUnspecifiedDocumentTypeName);
  await umbracoApi.documentType.ensureNameNotExists(dateTimeWithTimeZoneDocumentTypeName);
  await umbracoApi.dataType.ensureNameNotExists(dateOnlyDataTypeName);
  await umbracoApi.dataType.ensureNameNotExists(timeOnlyDataTypeName);
  await umbracoApi.dataType.ensureNameNotExists(dateTimeUnspecifiedDataTypeName);
  await umbracoApi.dataType.ensureNameNotExists(dateTimeWithTimeZoneDataTypeName);
  await umbracoApi.template.ensureNameNotExists(templateName);
});

test('can index and fetch a document with a DateOnly property', async ({umbracoApi}) => {
  // Arrange
  const dataTypeId = await umbracoApi.dataType.createDefaultDateOnlyPickerDataType(dateOnlyDataTypeName) ?? '';
  const value = {date: '2026-01-01T00:00:00.000Z', timeZone: null};
  const documentId = await umbracoApi.document.createPublishedDocumentWithValue(dateOnlyDocumentName, value, dataTypeId, templateId, dateOnlyDataTypeName, dateOnlyDocumentTypeName);

  // Assert
  await umbracoApi.searchManagement.waitUntilDocumentIsFound(publishedIndexAlias, dateOnlyDocumentName, documentId);
  const documentTypeAlias = (await umbracoApi.documentType.getByName(dateOnlyDocumentTypeName)).alias;
  await umbracoApi.contentDeliveryApi.waitUntilContentQueryReturnsNames('contentType:' + documentTypeAlias, undefined, [dateOnlyDocumentName], 1);
  expect((await umbracoApi.searchManagement.getIndex(publishedIndexAlias)).healthStatus).toBe('Healthy');
  const contentItem = await umbracoApi.contentDeliveryApi.getContentItemWithId(documentId);
  expect(contentItem.status()).toBe(200);
  expect((await contentItem.json()).properties[AliasHelper.toAlias(dateOnlyDataTypeName)]).toBe('2026-01-01');
});

test('can index and fetch a document with a TimeOnly property', async ({umbracoApi}) => {
  // Arrange
  const dataTypeId = await umbracoApi.dataType.createDefaultTimeOnlyPickerDataType(timeOnlyDataTypeName) ?? '';
  const value = {date: '1970-01-01T12:30:00.000Z', timeZone: null};
  const documentId = await umbracoApi.document.createPublishedDocumentWithValue(timeOnlyDocumentName, value, dataTypeId, templateId, timeOnlyDataTypeName, timeOnlyDocumentTypeName);

  // Assert
  await umbracoApi.searchManagement.waitUntilDocumentIsFound(publishedIndexAlias, timeOnlyDocumentName, documentId);
  const documentTypeAlias = (await umbracoApi.documentType.getByName(timeOnlyDocumentTypeName)).alias;
  await umbracoApi.contentDeliveryApi.waitUntilContentQueryReturnsNames('contentType:' + documentTypeAlias, undefined, [timeOnlyDocumentName], 1);
  expect((await umbracoApi.searchManagement.getIndex(publishedIndexAlias)).healthStatus).toBe('Healthy');
  const contentItem = await umbracoApi.contentDeliveryApi.getContentItemWithId(documentId);
  expect(contentItem.status()).toBe(200);
  expect((await contentItem.json()).properties[AliasHelper.toAlias(timeOnlyDataTypeName)]).toBe('12:30:00');
});

test('can index and fetch a document with a DateTimeUnspecified property', async ({umbracoApi}) => {
  // Arrange
  const dataTypeId = await umbracoApi.dataType.createDefaultDateTimePickerDataType(dateTimeUnspecifiedDataTypeName) ?? '';
  const value = {date: '2026-01-01T12:30:00.000Z', timeZone: null};
  const documentId = await umbracoApi.document.createPublishedDocumentWithValue(dateTimeUnspecifiedDocumentName, value, dataTypeId, templateId, dateTimeUnspecifiedDataTypeName, dateTimeUnspecifiedDocumentTypeName);

  // Assert
  await umbracoApi.searchManagement.waitUntilDocumentIsFound(publishedIndexAlias, dateTimeUnspecifiedDocumentName, documentId);
  const documentTypeAlias = (await umbracoApi.documentType.getByName(dateTimeUnspecifiedDocumentTypeName)).alias;
  await umbracoApi.contentDeliveryApi.waitUntilContentQueryReturnsNames('contentType:' + documentTypeAlias, undefined, [dateTimeUnspecifiedDocumentName], 1);
  expect((await umbracoApi.searchManagement.getIndex(publishedIndexAlias)).healthStatus).toBe('Healthy');
  const contentItem = await umbracoApi.contentDeliveryApi.getContentItemWithId(documentId);
  expect(contentItem.status()).toBe(200);
  expect((await contentItem.json()).properties[AliasHelper.toAlias(dateTimeUnspecifiedDataTypeName)]).toBe('2026-01-01T12:30:00');
});

test('can index and fetch a document with a DateTimeWithTimeZone property', async ({umbracoApi}) => {
  // Arrange
  const dataTypeId = await umbracoApi.dataType.createDefaultDateTimeWithTimeZonePickerDataType(dateTimeWithTimeZoneDataTypeName) ?? '';
  const value = {date: '2026-01-01T12:30:00.000Z', timeZone: 'Europe/Copenhagen'};
  const documentId = await umbracoApi.document.createPublishedDocumentWithValue(dateTimeWithTimeZoneDocumentName, value, dataTypeId, templateId, dateTimeWithTimeZoneDataTypeName, dateTimeWithTimeZoneDocumentTypeName);

  // Assert
  await umbracoApi.searchManagement.waitUntilDocumentIsFound(publishedIndexAlias, dateTimeWithTimeZoneDocumentName, documentId);
  const documentTypeAlias = (await umbracoApi.documentType.getByName(dateTimeWithTimeZoneDocumentTypeName)).alias;
  await umbracoApi.contentDeliveryApi.waitUntilContentQueryReturnsNames('contentType:' + documentTypeAlias, undefined, [dateTimeWithTimeZoneDocumentName], 1);
  expect((await umbracoApi.searchManagement.getIndex(publishedIndexAlias)).healthStatus).toBe('Healthy');
  const contentItem = await umbracoApi.contentDeliveryApi.getContentItemWithId(documentId);
  expect(contentItem.status()).toBe(200);
  expect((await contentItem.json()).properties[AliasHelper.toAlias(dateTimeWithTimeZoneDataTypeName)]).toBe('2026-01-01T12:30:00+00:00');
});
