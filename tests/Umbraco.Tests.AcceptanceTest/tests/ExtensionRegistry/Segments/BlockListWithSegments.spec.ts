import {AliasHelper, ConstantHelper, test} from '@umbraco/acceptance-test-helpers';
import {expect} from "@playwright/test";

// Content
const contentName = 'TestSegmentBlockContent';
// DocumentType
const documentTypeName = 'TestSegmentBlockDocType';
// Element Type
const blockElementTypeName = 'TestSegmentBlockElementType';
// DataTypes
const blockListDataTypeName = 'TestSegmentBlockList';
const textstringDataTypeName = 'Textstring';
const blockListEditorAlias = 'Umbraco.BlockList';
// Segments
const vipMemberSegment = 'VIP members';
const vipMemberSegmentAlias = 'vip-members';
// Test Values
const defaultBlockValue = 'Default block value';
const vipBlockValue = 'VIP block value';

test.describe('block list with segment variations', () => {
  test.beforeEach(async ({umbracoApi}) => {
    await umbracoApi.document.ensureNameNotExists(contentName);
    await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
    await umbracoApi.documentType.ensureNameNotExists(blockElementTypeName);
    await umbracoApi.dataType.ensureNameNotExists(blockListDataTypeName);
  });

  test.afterEach(async ({umbracoApi}) => {
    await umbracoApi.document.ensureNameNotExists(contentName);
    await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
    await umbracoApi.documentType.ensureNameNotExists(blockElementTypeName);
    await umbracoApi.dataType.ensureNameNotExists(blockListDataTypeName);
  });

  test('can add a block to the VIP segment of a block list that varies by segment', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const textstringDataType = await umbracoApi.dataType.getByName(textstringDataTypeName);
    const blockElementTypeId = await umbracoApi.documentType.createDefaultElementType(blockElementTypeName, 'BlockGroup', textstringDataTypeName, textstringDataType.id);
    const blockListDataTypeId = await umbracoApi.dataType.createBlockListDataTypeWithABlock(blockListDataTypeName, blockElementTypeId);
    const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, blockListDataTypeName, blockListDataTypeId, 'TestGroup', false, false, false, true, true);
    await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
    await umbracoUi.goToBackOffice();
    await umbracoUi.content.goToSection(ConstantHelper.sections.content);
    await umbracoUi.content.goToContentWithName(contentName);
    await umbracoUi.content.clickSelectVariantButton();
    await umbracoUi.content.clickSegmentVariantButton(vipMemberSegment);

    // Act
    await umbracoUi.content.clickAddBlockElementButton();
    await umbracoUi.content.clickBlockElementWithName(blockElementTypeName);
    await umbracoUi.content.enterTextstring(vipBlockValue);
    await umbracoUi.content.clickCreateModalButton();
    await umbracoUi.content.clickSaveButtonAndWaitForContentToBeUpdated();

    // Assert
    const contentData = await umbracoApi.document.getByName(contentName);
    const vipBlockListValue = contentData.values.find(value => value.editorAlias === blockListEditorAlias && value.segment === vipMemberSegmentAlias);
    expect(vipBlockListValue).toBeTruthy();
    expect(vipBlockListValue.value.contentData.length).toBe(1);
    expect(vipBlockListValue.value.contentData[0].values[0].value).toBe(vipBlockValue);
    const defaultBlockListValue = contentData.values.find(value => value.editorAlias === blockListEditorAlias && value.segment === null);
    expect(defaultBlockListValue?.value?.contentData ?? []).toHaveLength(0);
  });

  test('can only create a block in the VIP segment of a block list that varies by segment once its mandatory value is filled in', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const textstringDataType = await umbracoApi.dataType.getByName(textstringDataTypeName);
    const blockElementTypeId = await umbracoApi.documentType.createDefaultElementType(blockElementTypeName, 'BlockGroup', textstringDataTypeName, textstringDataType.id, true);
    const blockListDataTypeId = await umbracoApi.dataType.createBlockListDataTypeWithABlock(blockListDataTypeName, blockElementTypeId);
    const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, blockListDataTypeName, blockListDataTypeId, 'TestGroup', false, false, false, true, true);
    const documentId = await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
    await umbracoUi.goToBackOffice();
    await umbracoUi.content.goToSection(ConstantHelper.sections.content);
    await umbracoUi.content.goToContentWithName(contentName);
    await umbracoUi.content.clickSelectVariantButton();
    await umbracoUi.content.clickSegmentVariantButton(vipMemberSegment);

    // Act
    await umbracoUi.content.clickAddBlockElementButton();
    await umbracoUi.content.clickBlockElementWithName(blockElementTypeName);
    await umbracoUi.content.clickCreateModalButton();
    await umbracoUi.content.isValidationMessageVisible(ConstantHelper.validationMessages.emptyValue);
    await umbracoUi.content.enterTextstring(vipBlockValue);
    await umbracoUi.content.isValidationMessageVisible(ConstantHelper.validationMessages.emptyValue, false);
    await umbracoUi.content.clickCreateModalButton();
    await umbracoUi.content.clickSaveAndPublishButtonAndWaitForContentToBePublished();

    // Assert
    expect(await umbracoApi.document.isDocumentPublished(documentId)).toBeTruthy();
    const [publishedDefault, publishedVip] = await umbracoApi.document.getPublishedValuesByCultureAndSegmentForDocument(documentId, [
      {culture: null, segment: null},
      {culture: null, segment: vipMemberSegmentAlias},
    ]);
    expect(publishedDefault?.value?.contentData ?? []).toHaveLength(0);
    expect(publishedVip.value.contentData.length).toBe(1);
    expect(publishedVip.value.contentData[0].values[0].value).toBe(vipBlockValue);
  });

  test('can update the VIP segment value of a block in a block list shared across segments', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const textstringDataType = await umbracoApi.dataType.getByName(textstringDataTypeName);
    const blockElementTypeId = await umbracoApi.documentType.createDefaultElementTypeWithVaryBySegment(blockElementTypeName, 'BlockGroup', textstringDataTypeName, textstringDataType.id);
    const blockListDataTypeId = await umbracoApi.dataType.createBlockListDataTypeWithABlock(blockListDataTypeName, blockElementTypeId);
    const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, blockListDataTypeName, blockListDataTypeId, 'TestGroup', false, false, false, true, false);
    await umbracoApi.document.createDocumentWithBlockListBlocksInSegments(
      contentName,
      documentTypeId,
      AliasHelper.toAlias(blockListDataTypeName),
      blockElementTypeId,
      AliasHelper.toAlias(textstringDataTypeName),
      'Umbraco.TextBox',
      [[{value: defaultBlockValue, segment: null}]]
    );
    await umbracoUi.goToBackOffice();
    await umbracoUi.content.goToSection(ConstantHelper.sections.content);
    await umbracoUi.content.goToContentWithName(contentName);
    await umbracoUi.content.clickSelectVariantButton();
    await umbracoUi.content.clickSegmentVariantButton(vipMemberSegment);

    // Act
    await umbracoUi.content.clickEditBlockListBlockButton();
    await umbracoUi.content.enterTextstring(vipBlockValue);
    await umbracoUi.content.clickUpdateButton();
    await umbracoUi.content.clickSaveButtonAndWaitForContentToBeUpdated();

    // Assert
    const contentData = await umbracoApi.document.getByName(contentName);
    const blockListValue = contentData.values.find(value => value.editorAlias === blockListEditorAlias);
    expect(blockListValue.value.contentData.length).toBe(1);
    const blockValues = blockListValue.value.contentData[0].values;
    expect(blockValues.find(value => value.segment === null)?.value).toBe(defaultBlockValue);
    expect(blockValues.find(value => value.segment === vipMemberSegmentAlias)?.value).toBe(vipBlockValue);
  });
});
