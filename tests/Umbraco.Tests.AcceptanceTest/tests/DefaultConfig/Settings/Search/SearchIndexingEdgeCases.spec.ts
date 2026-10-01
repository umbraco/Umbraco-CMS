import {expect} from '@playwright/test';
import {test} from '@umbraco/acceptance-test-helpers';

// Indexes
const contentIndexAlias = 'Umb_Content';
const publishedIndexAlias = 'Umb_PublishedContent';
const mediaIndexAlias = 'Umb_Media';
const memberIndexAlias = 'Umb_Members';

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
const publishedDocumentTypeName = 'SearchIndexingEdgeCasesPublishedDocumentType';
const draftOnlyDocumentName = 'SearchIndexingEdgeCasesDraftOnlyDocument';
const publishedDocumentName = 'SearchIndexingEdgeCasesPublishedDocument';

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(singleBlockDocumentName);
  await umbracoApi.documentType.ensureNameNotExists(singleBlockDocumentTypeName);
  await umbracoApi.documentType.ensureNameNotExists(singleBlockElementTypeName);
  await umbracoApi.dataType.ensureNameNotExists(singleBlockDataTypeName);
  await umbracoApi.document.ensureNameNotExists(cultureDocumentName);
  await umbracoApi.documentType.ensureNameNotExists(cultureDocumentTypeName);
  await umbracoApi.language.ensureIsoCodeNotExists(danishIsoCode);
  await umbracoApi.media.ensureNameNotExists(mediaFileName);
  await umbracoApi.member.ensureNameNotExists(memberName);
  await umbracoApi.memberType.ensureNameNotExists(memberTypeName);
  await umbracoApi.document.ensureNameNotExists(draftOnlyDocumentName);
  await umbracoApi.document.ensureNameNotExists(publishedDocumentName);
  await umbracoApi.documentType.ensureNameNotExists(publishedDocumentTypeName);
});

test('can find a text value inside a SingleBlock property', async ({umbracoApi}) => {
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
  await umbracoApi.searchManagement.waitUntilDocumentIsFound(contentIndexAlias, singleBlockSearchableValue, documentId);
});

test('can only find a culture\'s value when searching in that culture', async ({umbracoApi}) => {
  test.slow();

  // Arrange
  await umbracoApi.language.createDanishLanguage();
  const textstringDataType = await umbracoApi.dataType.getByName(textstringDataTypeName);
  const cultureDocumentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(cultureDocumentTypeName, textstringDataTypeName, textstringDataType.id, cultureGroupName, true, true) ?? '';
  const documentId = await umbracoApi.document.createDocumentWithTwoCultureSpecificValues(cultureDocumentName, cultureDocumentTypeId, textstringDataTypeName, englishIsoCode, englishSearchableValue, danishIsoCode, danishSearchableValue) ?? '';
  await umbracoApi.document.publishDocumentWithCultures(documentId, [englishIsoCode, danishIsoCode]);
  await umbracoApi.searchManagement.waitUntilDocumentIsFound(contentIndexAlias, englishSearchableValue, documentId, englishIsoCode);
  await umbracoApi.searchManagement.waitUntilDocumentIsFound(contentIndexAlias, danishSearchableValue, documentId, danishIsoCode);

  // Assert
  expect(await umbracoApi.searchManagement.isDocumentFound(contentIndexAlias, danishSearchableValue, documentId, englishIsoCode)).toBe(false);
  expect(await umbracoApi.searchManagement.isDocumentFound(contentIndexAlias, englishSearchableValue, documentId, danishIsoCode)).toBe(false);
});

test('can find a media item by name', async ({umbracoApi}) => {
  // Arrange
  const mediaId = await umbracoApi.media.createDefaultMediaFile(mediaFileName) ?? '';

  // Assert
  await umbracoApi.searchManagement.waitUntilDocumentIsFound(mediaIndexAlias, mediaFileName, mediaId);
});

test('can find a member by name', async ({umbracoApi}) => {
  // Arrange
  const memberTypeId = await umbracoApi.memberType.createDefaultMemberType(memberTypeName) ?? '';
  const memberId = await umbracoApi.member.createDefaultMember(memberName, memberTypeId, memberEmail, memberUsername, memberPassword) ?? '';

  // Assert
  await umbracoApi.searchManagement.waitUntilDocumentIsFound(memberIndexAlias, memberName, memberId);
});

test('can find a never-published document in the draft index but not in the published one', async ({umbracoApi}) => {
  test.slow();

  // Arrange
  const documentTypeId = await umbracoApi.documentType.createDefaultDocumentTypeWithAllowAsRoot(publishedDocumentTypeName) ?? '';
  const draftOnlyDocumentId = await umbracoApi.document.createDefaultDocument(draftOnlyDocumentName, documentTypeId) ?? '';
  const publishedDocumentId = await umbracoApi.document.createPublishedDefaultDocument(publishedDocumentName, documentTypeId);
  await umbracoApi.searchManagement.waitUntilDocumentIsFound(publishedIndexAlias, publishedDocumentName, publishedDocumentId);
  await umbracoApi.searchManagement.waitUntilDocumentIsFound(contentIndexAlias, draftOnlyDocumentName, draftOnlyDocumentId);

  // Assert
  expect(await umbracoApi.searchManagement.isDocumentFound(publishedIndexAlias, draftOnlyDocumentName, draftOnlyDocumentId)).toBe(false);
});
