import {expect} from '@playwright/test';
import {ApiHelpers, test} from '@umbraco/acceptance-test-helpers';

// SingleBlock
const singleBlockDocumentTypeName = 'SearchIndexingEdgeCasesSingleBlockDocumentType';
const singleBlockElementTypeName = 'SearchIndexingEdgeCasesSingleBlockElementType';
const singleBlockElementGroupName = 'SingleBlockElementGroup';
const singleBlockDataTypeName = 'SearchIndexingEdgeCasesSingleBlock';
const singleBlockDocumentName = 'SearchIndexingEdgeCasesSingleBlockDocument';
const singleBlockGroupName = 'SingleBlockGroup';
const textstringDataTypeName = 'Textstring';
const singleBlockInnerPropertyEditorAlias = 'Umbraco.TextBox';
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

// Culture-scoped search
const cultureDocumentTypeName = 'SearchIndexingEdgeCasesCultureDocumentType';
const cultureDocumentName = 'SearchIndexingEdgeCasesCultureDocument';
const cultureGroupName = 'CultureGroup';
const englishIsoCode = 'en-US';
const danishIsoCode = 'da';
const englishSearchableValue = 'CultureScopedSearchEnglishValue1234567890';
const danishSearchableValue = 'CultureScopedSearchDanishValue1234567890';

// Media
const mediaFileName = 'SearchIndexingEdgeCasesMediaFile';

// Member
const memberTypeName = 'SearchIndexingEdgeCasesMemberType';
const memberName = 'SearchIndexingEdgeCasesMember';
const memberEmail = 'searchindexingedgecasesmember@acceptancetest.umbraco.com';
const memberUsername = 'searchindexingedgecasesmember';
const memberPassword = '0123456789';

// Published content index
const publishedIndexAlias = 'Umb_PublishedContent';
const publishedDocumentTypeName = 'SearchIndexingEdgeCasesPublishedDocumentType';
const draftOnlyDocumentName = 'SearchIndexingEdgeCasesDraftOnlyDocument';
const publishedDocumentName = 'SearchIndexingEdgeCasesPublishedDocument';

let indexAlias = '';

test.beforeEach(async ({umbracoApi}) => {
  test.slow();
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

    // Assert
    await umbracoApi.searchManagement.waitUntilDocumentIsFound(indexAlias, singleBlockSearchableValue, documentId);
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

  async function verifyDateEditorDocumentIsIndexed(umbracoApi: ApiHelpers, documentName: string, createDocument: () => Promise<string>) {
    const documentId = await createDocument();

    await umbracoApi.searchManagement.waitUntilDocumentIsFound(indexAlias, documentName, documentId);

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

test.describe('culture-scoped ad-hoc search', () => {
  test.beforeEach(async ({umbracoApi}) => {
    await umbracoApi.language.createDanishLanguage();
  });

  test.afterEach(async ({umbracoApi}) => {
    await umbracoApi.document.ensureNameNotExists(cultureDocumentName);
    await umbracoApi.documentType.ensureNameNotExists(cultureDocumentTypeName);
    await umbracoApi.language.ensureIsoCodeNotExists(danishIsoCode);
  });

  test('a search scoped to a culture only matches that culture\'s variant value', async ({umbracoApi}) => {
    // Arrange
    const textstringDataType = await umbracoApi.dataType.getByName(textstringDataTypeName);
    const cultureDocumentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(cultureDocumentTypeName, textstringDataTypeName, textstringDataType.id, cultureGroupName, true, true) ?? '';
    const documentId = await umbracoApi.document.createDocumentWithTwoCultureSpecificValues(cultureDocumentName, cultureDocumentTypeId, textstringDataTypeName, englishIsoCode, englishSearchableValue, danishIsoCode, danishSearchableValue) ?? '';
    await umbracoApi.document.publishDocumentWithCultures(documentId, [englishIsoCode, danishIsoCode]);

    await umbracoApi.searchManagement.waitUntilDocumentIsFound(indexAlias, englishSearchableValue, documentId, englishIsoCode);
    await umbracoApi.searchManagement.waitUntilDocumentIsFound(indexAlias, danishSearchableValue, documentId, danishIsoCode);

    // Assert
    expect(await umbracoApi.searchManagement.isDocumentFound(indexAlias, danishSearchableValue, documentId, englishIsoCode)).toBe(false);
    expect(await umbracoApi.searchManagement.isDocumentFound(indexAlias, englishSearchableValue, documentId, danishIsoCode)).toBe(false);
  });
});

test.describe('media indexing', () => {
  let mediaIndexAlias = '';

  test.beforeEach(async ({umbracoApi}) => {
    const indexes = await umbracoApi.searchManagement.getAllIndexes();
    const mediaIndex = indexes.items.find((index) => index.indexAlias === 'Umb_Media');
    expect(mediaIndex, 'the Umb_Media index must exist').toBeTruthy();
    mediaIndexAlias = mediaIndex!.indexAlias;
  });

  test.afterEach(async ({umbracoApi}) => {
    await umbracoApi.media.ensureNameNotExists(mediaFileName);
  });

  test('a media item is indexed and findable by name', async ({umbracoApi}) => {
    // Arrange
    const mediaId = await umbracoApi.media.createDefaultMediaFile(mediaFileName) ?? '';

    // Assert
    await umbracoApi.searchManagement.waitUntilDocumentIsFound(mediaIndexAlias, mediaFileName, mediaId);
  });
});

test.describe('member indexing', () => {
  let memberIndexAlias = '';

  test.beforeEach(async ({umbracoApi}) => {
    const indexes = await umbracoApi.searchManagement.getAllIndexes();
    const memberIndex = indexes.items.find((index) => index.indexAlias === 'Umb_Members');
    expect(memberIndex, 'the Umb_Members index must exist').toBeTruthy();
    memberIndexAlias = memberIndex!.indexAlias;
  });

  test.afterEach(async ({umbracoApi}) => {
    await umbracoApi.member.ensureNameNotExists(memberName);
    await umbracoApi.memberType.ensureNameNotExists(memberTypeName);
  });

  test('a member is indexed and findable by name', async ({umbracoApi}) => {
    // Arrange
    const memberTypeId = await umbracoApi.memberType.createDefaultMemberType(memberTypeName) ?? '';
    const memberId = await umbracoApi.member.createDefaultMember(memberName, memberTypeId, memberEmail, memberUsername, memberPassword) ?? '';

    // Assert
    await umbracoApi.searchManagement.waitUntilDocumentIsFound(memberIndexAlias, memberName, memberId);
  });
});

test.describe('published content indexing', () => {
  test.beforeEach(async ({umbracoApi}) => {
    const indexes = await umbracoApi.searchManagement.getAllIndexes();
    expect(indexes.items.some((index) => index.indexAlias === publishedIndexAlias), `the ${publishedIndexAlias} index must exist`).toBeTruthy();
  });

  test.afterEach(async ({umbracoApi}) => {
    await umbracoApi.document.ensureNameNotExists(draftOnlyDocumentName);
    await umbracoApi.document.ensureNameNotExists(publishedDocumentName);
    await umbracoApi.documentType.ensureNameNotExists(publishedDocumentTypeName);
  });

  test('a document that was never published is in the draft index but not the published one', async ({umbracoApi}) => {
    // Arrange
    const documentTypeId = await umbracoApi.documentType.createDefaultDocumentTypeWithAllowAsRoot(publishedDocumentTypeName) ?? '';
    const draftOnlyDocumentId = await umbracoApi.document.createDefaultDocument(draftOnlyDocumentName, documentTypeId) ?? '';
    const publishedDocumentId = await umbracoApi.document.createDefaultDocument(publishedDocumentName, documentTypeId) ?? '';
    await umbracoApi.document.publish(publishedDocumentId);

    // Act
    await umbracoApi.searchManagement.waitUntilDocumentIsFound(publishedIndexAlias, publishedDocumentName, publishedDocumentId);
    await umbracoApi.searchManagement.waitUntilDocumentIsFound(indexAlias, draftOnlyDocumentName, draftOnlyDocumentId);

    // Assert
    expect(await umbracoApi.searchManagement.isDocumentFound(publishedIndexAlias, draftOnlyDocumentName, draftOnlyDocumentId)).toBe(false);
  });
});
