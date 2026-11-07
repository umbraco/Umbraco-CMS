import {ConstantHelper, NotificationConstantHelper, test} from '@umbraco/acceptance-test-helpers';
import {expect} from "@playwright/test";

// Content
const contentName = 'TestSegmentMandatoryContent';
// DocumentType
const documentTypeName = 'TestSegmentMandatoryDocType';
let documentTypeId = '';
// DataType
const dataTypeName = 'Textstring';
const editorAlias = 'Umbraco.TextBox';
// Segments
const vipMemberSegmentAlias = 'vip-members';
// Test Values
const defaultSegmentValue = 'Default segment value';
const vipSegmentValue = 'VIP segment value';

test.beforeEach(async ({umbracoApi, umbracoUi}) => {
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeData.id, 'TestGroup', false, false, true, true, true);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
});

test('cannot publish segment-only content when the mandatory default segment value is empty', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const documentId = await umbracoApi.document.createDocumentWithMultipleSegmentValues(contentName, documentTypeId, dataTypeName, editorAlias, [
    {value: vipSegmentValue, segment: vipMemberSegmentAlias},
  ]);
  await umbracoUi.content.goToContentWithName(contentName);

  // Act
  await umbracoUi.content.clickSaveAndPublishButton();

  // Assert
  await umbracoUi.content.isValidationMessageVisible(ConstantHelper.validationMessages.emptyValue);
  await umbracoUi.content.doesErrorNotificationHaveText(NotificationConstantHelper.error.documentCouldNotBePublished);
  expect(await umbracoApi.document.isDocumentPublished(documentId)).toBeFalsy();
});

test('can publish segment-only content with a mandatory property without a VIP segment value', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const documentId = await umbracoApi.document.createDocumentWithTextContent(contentName, documentTypeId, defaultSegmentValue, dataTypeName);
  await umbracoUi.content.goToContentWithName(contentName);

  // Act
  await umbracoUi.content.clickSaveAndPublishButtonAndWaitForContentToBePublished();

  // Assert
  expect(await umbracoApi.document.isDocumentPublished(documentId)).toBeTruthy();
  const [publishedDefault, publishedVip] = await umbracoApi.document.getPublishedValuesByCultureAndSegmentForDocument(documentId, [
    {culture: null, segment: null},
    {culture: null, segment: vipMemberSegmentAlias},
  ]);
  expect(publishedDefault).toBeTruthy();
  expect(publishedDefault.value).toBe(defaultSegmentValue);
  expect(publishedVip).toBeFalsy();
});
