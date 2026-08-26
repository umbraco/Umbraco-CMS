import {expect} from '@playwright/test';
import {ConstantHelper, test} from '@umbraco/acceptance-test-helpers';

// SingleBlock
const singleBlockDocumentTypeName = 'SearchIndexingEdgeCasesSingleBlockDocumentType';
const singleBlockElementTypeName = 'SearchIndexingEdgeCasesSingleBlockElementType';
const singleBlockElementGroupName = 'SingleBlockElementGroup';
const singleBlockDataTypeName = 'SearchIndexingEdgeCasesSingleBlock';
const singleBlockDocumentName = 'SearchIndexingEdgeCasesSingleBlockDocument';
const singleBlockGroupName = 'SingleBlockGroup';
const textstringDataTypeName = 'Textstring';
const singleBlockInnerPropertyEditorAlias = 'Umbraco.TextBox';
// A value distinctive enough that it is very unlikely to collide with other content indexed in this environment.
const singleBlockSearchableValue = 'SingleBlockIndexingEdgeCaseSearchableValue1234567890';

// Date/time editors
const dateOnlyDataTypeName = 'SearchIndexingEdgeCasesDateOnly';
const timeOnlyDataTypeName = 'SearchIndexingEdgeCasesTimeOnly';
const dateTimeUnspecifiedDataTypeName = 'SearchIndexingEdgeCasesDateTimeUnspecified';
const dateTimeWithTimeZoneDataTypeName = 'SearchIndexingEdgeCasesDateTimeWithTimeZone';
const dateOnlyDocumentName = 'SearchIndexingEdgeCasesDateOnlyDocument';
const timeOnlyDocumentName = 'SearchIndexingEdgeCasesTimeOnlyDocument';
const dateTimeUnspecifiedDocumentName = 'SearchIndexingEdgeCasesDateTimeUnspecifiedDocument';
const dateTimeWithTimeZoneDocumentName = 'SearchIndexingEdgeCasesDateTimeWithTimeZoneDocument';
const dateOnlyDocumentTypeName = 'SearchIndexingEdgeCasesDateOnlyDocumentType';
const timeOnlyDocumentTypeName = 'SearchIndexingEdgeCasesTimeOnlyDocumentType';
const dateTimeUnspecifiedDocumentTypeName = 'SearchIndexingEdgeCasesDateTimeUnspecifiedDocumentType';
const dateTimeWithTimeZoneDocumentTypeName = 'SearchIndexingEdgeCasesDateTimeWithTimeZoneDocumentType';
const dateEditorsTemplateName = 'SearchIndexingEdgeCasesDateEditorsTemplate';

let indexAlias = '';

test.beforeEach(async ({umbracoApi}) => {
  const indexes = await umbracoApi.searchManagement.getAllIndexes();
  indexAlias = indexes.items.find((index) => index.indexAlias === 'Umb_Content').indexAlias;
});

test.describe('SingleBlock property indexing', () => {
  test.afterEach(async ({umbracoApi}) => {
    await umbracoApi.document.ensureNameNotExists(singleBlockDocumentName);
    await umbracoApi.documentType.ensureNameNotExists(singleBlockDocumentTypeName);
    await umbracoApi.documentType.ensureNameNotExists(singleBlockElementTypeName);
    await umbracoApi.dataType.ensureNameNotExists(singleBlockDataTypeName);
  });

  test('a text value inside a SingleBlock property is indexed and findable', async ({umbracoApi}) => {
    // Arrange
    const textstringDataType = await umbracoApi.dataType.getByName(textstringDataTypeName);
    const elementTypeId = await umbracoApi.documentType.createDefaultElementType(singleBlockElementTypeName, singleBlockElementGroupName, textstringDataTypeName, textstringDataType.id) ?? '';

    const documentId = await umbracoApi.document.createDefaultDocumentWithASingleBlockEditorAndBlockWithValue(
      singleBlockDocumentName,
      singleBlockDocumentTypeName,
      singleBlockDataTypeName,
      elementTypeId,
      'textstring',
      singleBlockSearchableValue,
      singleBlockInnerPropertyEditorAlias,
      singleBlockGroupName,
    );
    await umbracoApi.document.publish(documentId);
    await umbracoApi.page.waitForTimeout(ConstantHelper.wait.medium); // Wait is needed to ensure content is indexed

    // Act
    // SingleBlockPropertyValueHandler recursively indexes the block's inner content under the outer block
    // property's own field name (not the inner "textstring" property alias) - a free-text search for the
    // inner value must still find the document via the ad-hoc search box's query endpoint.
    const searchResult = await umbracoApi.searchManagement.search(indexAlias, singleBlockSearchableValue);

    // Assert
    expect(searchResult.total).toBeGreaterThan(0);
    expect(searchResult.documents.some((document: {id: string}) => document.id === documentId)).toBeTruthy();
  });
});

test.describe('date/time editor indexing', () => {
  let templateId = '';

  test.beforeEach(async ({umbracoApi}) => {
    templateId = await umbracoApi.template.createDefaultTemplate(dateEditorsTemplateName) ?? '';
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
    await umbracoApi.template.ensureNameNotExists(dateEditorsTemplateName);
  });

  // The Delivery API only exposes filter/sort support for a fixed set of system fields (contentType, name,
  // createDate, updateDate, level, sortOrder) - there is no filter handler for arbitrary custom properties, so a
  // custom date property cannot be queried via filter=. Instead, this verifies that DateTimeOffsetPropertyValueHandler
  // does not break indexing for these previously-unindexed/mishandled editors: the document must still make it into
  // the index (document count increases, health stays Healthy) and remain fetchable via the Delivery API.
  // The document count baseline is captured before createDocument() runs, since createDocument() also publishes
  // the document - capturing it any later would already include the new document in "before".
  async function verifyDateEditorDocumentIsIndexed(umbracoApi, createDocument: () => Promise<string>) {
    const indexBefore = await umbracoApi.searchManagement.getIndex(indexAlias);
    const documentId = await createDocument();

    // healthStatus turning Healthy doesn't guarantee documentCount has caught up with the latest write yet,
    // so poll documentCount itself rather than a flat wait followed by a single check.
    await expect
      .poll(async () => (await umbracoApi.searchManagement.getIndex(indexAlias)).documentCount, {timeout: ConstantHelper.timeout.pageLoad})
      .toBeGreaterThan(indexBefore.documentCount);
    const indexAfter = await umbracoApi.searchManagement.getIndex(indexAlias);
    expect(indexAfter.healthStatus).toBe('Healthy');

    const contentItem = await umbracoApi.contentDeliveryApi.getContentItemWithId(documentId);
    expect(contentItem.status()).toBe(200);
  }

  test('a document with a DateOnly property is indexed without error', async ({umbracoApi}) => {
    await verifyDateEditorDocumentIsIndexed(umbracoApi, async () => {
      const dateOnlyDataTypeId = await umbracoApi.dataType.createDefaultDateOnlyPickerDataType(dateOnlyDataTypeName) ?? '';
      const value = {date: '2026-01-01T00:00:00.000Z', timeZone: null};
      return await umbracoApi.document.createPublishedDocumentWithValue(dateOnlyDocumentName, value, dateOnlyDataTypeId, templateId, dateOnlyDataTypeName, dateOnlyDocumentTypeName);
    });
  });

  test('a document with a TimeOnly property is indexed without error', async ({umbracoApi}) => {
    await verifyDateEditorDocumentIsIndexed(umbracoApi, async () => {
      const timeOnlyDataTypeId = await umbracoApi.dataType.createDefaultTimeOnlyPickerDataType(timeOnlyDataTypeName) ?? '';
      const value = {date: '1970-01-01T12:30:00.000Z', timeZone: null};
      return await umbracoApi.document.createPublishedDocumentWithValue(timeOnlyDocumentName, value, timeOnlyDataTypeId, templateId, timeOnlyDataTypeName, timeOnlyDocumentTypeName);
    });
  });

  test('a document with a DateTimeUnspecified property is indexed without error', async ({umbracoApi}) => {
    await verifyDateEditorDocumentIsIndexed(umbracoApi, async () => {
      const dateTimeUnspecifiedDataTypeId = await umbracoApi.dataType.createDefaultDateTimePickerDataType(dateTimeUnspecifiedDataTypeName) ?? '';
      const value = {date: '2026-01-01T12:30:00.000Z', timeZone: null};
      return await umbracoApi.document.createPublishedDocumentWithValue(dateTimeUnspecifiedDocumentName, value, dateTimeUnspecifiedDataTypeId, templateId, dateTimeUnspecifiedDataTypeName, dateTimeUnspecifiedDocumentTypeName);
    });
  });

  test('a document with a DateTimeWithTimeZone property is indexed without error', async ({umbracoApi}) => {
    await verifyDateEditorDocumentIsIndexed(umbracoApi, async () => {
      const dateTimeWithTimeZoneDataTypeId = await umbracoApi.dataType.createDefaultDateTimeWithTimeZonePickerDataType(dateTimeWithTimeZoneDataTypeName) ?? '';
      const value = {date: '2026-01-01T12:30:00.000Z', timeZone: 'Europe/Copenhagen'};
      return await umbracoApi.document.createPublishedDocumentWithValue(dateTimeWithTimeZoneDocumentName, value, dateTimeWithTimeZoneDataTypeId, templateId, dateTimeWithTimeZoneDataTypeName, dateTimeWithTimeZoneDocumentTypeName);
    });
  });
});
