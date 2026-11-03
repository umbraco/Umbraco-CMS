import {ConstantHelper, NotificationConstantHelper, test} from '@umbraco/acceptance-test-helpers';
import {expect} from "@playwright/test";

// Content
const contentName = 'TestSegmentPublishContent';
// DocumentType
const documentTypeName = 'TestSegmentPublishDocType';
let documentTypeId = '';
// DataType
const dataTypeName = 'Textstring';
const editorAlias = 'Umbraco.TextBox';
// Segments
const vipMemberSegment = 'VIP members';
const vipMemberSegmentAlias = 'vip-members';
// Test Values
const defaultSegmentValue = 'Default segment value';
const vipSegmentValue = 'VIP segment value';
const updatedVipSegmentValue = 'Updated VIP segment value';
const englishDefaultValue = 'English default';
const englishVipValue = 'English VIP';
const updatedEnglishVipValue = 'Updated English VIP';
const danishDefaultValue = 'Danish default';

test.describe('publish content with segment-only variations', () => {
  test.beforeEach(async ({umbracoApi, umbracoUi}) => {
    await umbracoApi.document.ensureNameNotExists(contentName);
    await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
    const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
    documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeData.id, 'TestGroup', false, false, false, true, true);
    await umbracoUi.goToBackOffice();
    await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  });

  test.afterEach(async ({umbracoApi}) => {
    await umbracoApi.document.ensureNameNotExists(contentName);
    await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  });

  test('can publish updated VIP segment value on segment-only content', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const documentId = await umbracoApi.document.createDocumentWithMultipleSegmentValues(contentName, documentTypeId, dataTypeName, editorAlias, [
      {value: defaultSegmentValue, segment: null},
      {value: vipSegmentValue, segment: vipMemberSegmentAlias},
    ]);
    await umbracoApi.document.publish(documentId);
    await umbracoUi.content.goToContentWithName(contentName);
    await umbracoUi.content.clickSelectVariantButton();
    await umbracoUi.content.clickSegmentVariantButton(vipMemberSegment);

    // Act
    await umbracoUi.content.enterTextstring(updatedVipSegmentValue);
    await umbracoUi.content.clickSaveAndPublishButtonAndWaitForContentToBePublished();

    // Assert
    await umbracoUi.content.isSuccessStateVisibleForSaveAndPublishButton();
    const [publishedDefault, publishedVip] = await umbracoApi.document.getPublishedValuesByCultureAndSegmentForDocument(documentId, [
      {culture: null, segment: null},
      {culture: null, segment: vipMemberSegmentAlias},
    ]);
    expect(publishedDefault).toBeTruthy();
    expect(publishedDefault.value).toBe(defaultSegmentValue);
    expect(publishedVip).toBeTruthy();
    expect(publishedVip.value).toBe(updatedVipSegmentValue);
  });

  test('can publish the VIP segment value when publishing segment-only content from the default segment', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const documentId = await umbracoApi.document.createDocumentWithMultipleSegmentValues(contentName, documentTypeId, dataTypeName, editorAlias, [
      {value: defaultSegmentValue, segment: null},
      {value: vipSegmentValue, segment: vipMemberSegmentAlias},
    ]);
    await umbracoUi.content.goToContentWithName(contentName);

    // Act
    await umbracoUi.content.clickSaveAndPublishButtonAndWaitForContentToBePublished();

    // Assert
    await umbracoUi.content.isSuccessStateVisibleForSaveAndPublishButton();
    expect(await umbracoApi.document.isDocumentPublished(documentId)).toBeTruthy();
    const [publishedDefault, publishedVip] = await umbracoApi.document.getPublishedValuesByCultureAndSegmentForDocument(documentId, [
      {culture: null, segment: null},
      {culture: null, segment: vipMemberSegmentAlias},
    ]);
    expect(publishedDefault).toBeTruthy();
    expect(publishedDefault.value).toBe(defaultSegmentValue);
    expect(publishedVip).toBeTruthy();
    expect(publishedVip.value).toBe(vipSegmentValue);
  });

  test('can see pending changes after saving only a VIP segment value on published content', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const documentId = await umbracoApi.document.createDocumentWithMultipleSegmentValues(contentName, documentTypeId, dataTypeName, editorAlias, [
      {value: defaultSegmentValue, segment: null},
      {value: vipSegmentValue, segment: vipMemberSegmentAlias},
    ]);
    await umbracoApi.document.publish(documentId);
    await umbracoUi.content.goToContentWithName(contentName);
    await umbracoUi.content.clickSelectVariantButton();
    await umbracoUi.content.clickSegmentVariantButton(vipMemberSegment);

    // Act
    await umbracoUi.content.enterTextstring(updatedVipSegmentValue);
    await umbracoUi.content.clickSaveButtonAndWaitForContentToBeUpdated();

    // Assert
    // On segment-only content, the default segment is the variant labelled with the content name.
    await umbracoUi.content.switchLanguage(contentName);
    await umbracoUi.content.clickInfoTab();
    await umbracoUi.content.doesDocumentStateHaveText('Published (pending changes)');
    const [publishedVip] = await umbracoApi.document.getPublishedValuesByCultureAndSegmentForDocument(documentId, [
      {culture: null, segment: vipMemberSegmentAlias},
    ]);
    expect(publishedVip).toBeTruthy();
    expect(publishedVip.value).toBe(vipSegmentValue);
  });

  test('can unpublish segment-only content and keep the VIP segment value', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const documentId = await umbracoApi.document.createDocumentWithMultipleSegmentValues(contentName, documentTypeId, dataTypeName, editorAlias, [
      {value: defaultSegmentValue, segment: null},
      {value: vipSegmentValue, segment: vipMemberSegmentAlias},
    ]);
    await umbracoApi.document.publish(documentId);

    // Act
    await umbracoUi.content.clickActionsMenuForContent(contentName);
    await umbracoUi.content.clickUnpublishActionMenuOption();
    await umbracoUi.content.clickConfirmToUnpublishButton();

    // Assert
    await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.unpublished);
    expect(await umbracoApi.document.isDocumentPublished(documentId)).toBeFalsy();
    const [defaultValue, vipValue] = await umbracoApi.document.getValuesByCultureAndSegmentForDocument(contentName, [
      {culture: null, segment: null},
      {culture: null, segment: vipMemberSegmentAlias},
    ]);
    expect(defaultValue).toBeTruthy();
    expect(defaultValue.value).toBe(defaultSegmentValue);
    expect(vipValue).toBeTruthy();
    expect(vipValue.value).toBe(vipSegmentValue);
  });
});

test.describe('publish content with culture and segment variations', () => {
  test.beforeEach(async ({umbracoApi, umbracoUi}) => {
    await umbracoApi.document.ensureNameNotExists(contentName);
    await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
    await umbracoApi.language.createDanishLanguage();
    const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
    documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeData.id, 'TestGroup', true, true, false, true, true);
    await umbracoUi.goToBackOffice();
    await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  });

  test.afterEach(async ({umbracoApi}) => {
    await umbracoApi.document.ensureNameNotExists(contentName);
    await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
    await umbracoApi.language.ensureNameNotExists('Danish');
  });

  test('can publish an English VIP segment value without publishing Danish', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const documentId = await umbracoApi.document.createDocumentWithMultipleCulturesAndSegmentValues(contentName, documentTypeId, dataTypeName, editorAlias, ['en-US', 'da'], [
      {value: englishDefaultValue, culture: 'en-US', segment: null},
      {value: englishVipValue, culture: 'en-US', segment: vipMemberSegmentAlias},
      {value: danishDefaultValue, culture: 'da', segment: null},
    ]);
    await umbracoUi.content.goToContentWithName(contentName);
    await umbracoUi.content.clickSelectVariantButton();
    await umbracoUi.content.clickExpandSegmentButton('English');
    await umbracoUi.content.clickSegmentVariantButton(vipMemberSegment);

    // Act
    await umbracoUi.content.enterTextstring(updatedEnglishVipValue);
    await umbracoUi.content.clickSaveAndPublishButton();
    await umbracoUi.content.clickContainerSaveAndPublishButtonAndWaitForContentToBePublished();

    // Assert
    await umbracoUi.content.isSuccessNotificationVisible();
    const contentData = await umbracoApi.document.get(documentId);
    expect(contentData.variants.find(variant => variant.culture === 'en-US').state).toBe('Published');
    expect(contentData.variants.find(variant => variant.culture === 'da').state).toBe('Draft');
    const [publishedEnglishDefault, publishedEnglishVip] = await umbracoApi.document.getPublishedValuesByCultureAndSegmentForDocument(documentId, [
      {culture: 'en-US', segment: null},
      {culture: 'en-US', segment: vipMemberSegmentAlias},
    ]);
    expect(publishedEnglishDefault).toBeTruthy();
    expect(publishedEnglishDefault.value).toBe(englishDefaultValue);
    expect(publishedEnglishVip).toBeTruthy();
    expect(publishedEnglishVip.value).toBe(updatedEnglishVipValue);
  });
});
