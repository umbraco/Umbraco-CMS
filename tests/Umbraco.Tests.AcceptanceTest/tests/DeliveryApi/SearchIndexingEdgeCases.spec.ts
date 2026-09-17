import {expect} from '@playwright/test';
import {ApiHelpers, ConstantHelper, test} from '@umbraco/acceptance-test-helpers';

// Raised from the 60s default - chained index polls would otherwise hit the test timeout first.
test.describe.configure({timeout: 120000});

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
  const contentIndex = indexes.items.find((index) => index.indexAlias === 'Umb_Content');
  expect(contentIndex, 'the Umb_Content index must exist').toBeTruthy();
  indexAlias = contentIndex!.indexAlias;
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

    // Act
    // SingleBlockPropertyValueHandler recursively indexes the block's inner content under the outer block
    // property's own field name (not the inner "textstring" property alias) - a free-text search for the
    // inner value must still find the document via the ad-hoc search box's query endpoint.
    // Indexing is asynchronous, so poll the search until the document appears rather than waiting a fixed time.
    let searchResult;
    await expect
      .poll(
        async () => {
          searchResult = await umbracoApi.searchManagement.search(indexAlias, singleBlockSearchableValue);
          return searchResult.documents.some((document: {id: string}) => document.id === documentId);
        },
        {timeout: ConstantHelper.timeout.veryLong},
      )
      .toBeTruthy();

    // Assert
    expect(searchResult.total).toBeGreaterThan(0);
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
  // does not break indexing for these previously-unindexed/mishandled editors: the document must still reach the
  // index, leave it healthy, and remain fetchable via the Delivery API.
  //
  // Assert the document is findable rather than that the index count grew: a count delta races the previous
  // test's teardown, whose de-index can cancel out the document added here (+1 -1 = 0).
  async function verifyDateEditorDocumentIsIndexed(umbracoApi: ApiHelpers, documentName: string, createDocument: () => Promise<string>) {
    const documentId = await createDocument();

    await expect
      .poll(
        async () => {
          const searchResult = await umbracoApi.searchManagement.search(indexAlias, documentName);
          return searchResult.documents.some((document: {id: string}) => document.id === documentId);
        },
        {timeout: ConstantHelper.timeout.veryLong},
      )
      .toBeTruthy();

    const index = await umbracoApi.searchManagement.getIndex(indexAlias);
    expect(index.healthStatus).toBe('Healthy');

    const contentItem = await umbracoApi.contentDeliveryApi.getContentItemWithId(documentId);
    expect(contentItem.status()).toBe(200);
  }

  test('a document with a DateOnly property is indexed without error', async ({umbracoApi}) => {
    await verifyDateEditorDocumentIsIndexed(umbracoApi, dateOnlyDocumentName, async () => {
      const dateOnlyDataTypeId = await umbracoApi.dataType.createDefaultDateOnlyPickerDataType(dateOnlyDataTypeName) ?? '';
      const value = {date: '2026-01-01T00:00:00.000Z', timeZone: null};
      return await umbracoApi.document.createPublishedDocumentWithValue(dateOnlyDocumentName, value, dateOnlyDataTypeId, templateId, dateOnlyDataTypeName, dateOnlyDocumentTypeName);
    });
  });

  test('a document with a TimeOnly property is indexed without error', async ({umbracoApi}) => {
    await verifyDateEditorDocumentIsIndexed(umbracoApi, timeOnlyDocumentName, async () => {
      const timeOnlyDataTypeId = await umbracoApi.dataType.createDefaultTimeOnlyPickerDataType(timeOnlyDataTypeName) ?? '';
      const value = {date: '1970-01-01T12:30:00.000Z', timeZone: null};
      return await umbracoApi.document.createPublishedDocumentWithValue(timeOnlyDocumentName, value, timeOnlyDataTypeId, templateId, timeOnlyDataTypeName, timeOnlyDocumentTypeName);
    });
  });

  test('a document with a DateTimeUnspecified property is indexed without error', async ({umbracoApi}) => {
    await verifyDateEditorDocumentIsIndexed(umbracoApi, dateTimeUnspecifiedDocumentName, async () => {
      const dateTimeUnspecifiedDataTypeId = await umbracoApi.dataType.createDefaultDateTimePickerDataType(dateTimeUnspecifiedDataTypeName) ?? '';
      const value = {date: '2026-01-01T12:30:00.000Z', timeZone: null};
      return await umbracoApi.document.createPublishedDocumentWithValue(dateTimeUnspecifiedDocumentName, value, dateTimeUnspecifiedDataTypeId, templateId, dateTimeUnspecifiedDataTypeName, dateTimeUnspecifiedDocumentTypeName);
    });
  });

  test('a document with a DateTimeWithTimeZone property is indexed without error', async ({umbracoApi}) => {
    await verifyDateEditorDocumentIsIndexed(umbracoApi, dateTimeWithTimeZoneDocumentName, async () => {
      const dateTimeWithTimeZoneDataTypeId = await umbracoApi.dataType.createDefaultDateTimeWithTimeZonePickerDataType(dateTimeWithTimeZoneDataTypeName) ?? '';
      const value = {date: '2026-01-01T12:30:00.000Z', timeZone: 'Europe/Copenhagen'};
      return await umbracoApi.document.createPublishedDocumentWithValue(dateTimeWithTimeZoneDocumentName, value, dateTimeWithTimeZoneDataTypeId, templateId, dateTimeWithTimeZoneDataTypeName, dateTimeWithTimeZoneDocumentTypeName);
    });
  });
});
