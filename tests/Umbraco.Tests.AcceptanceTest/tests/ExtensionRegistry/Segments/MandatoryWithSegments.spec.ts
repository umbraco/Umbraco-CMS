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
const vipMemberSegment = 'VIP members';
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
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
});

test('cannot publish segment-only content when the mandatory default segment value is empty', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const documentId = await umbracoApi.document.createDocumentWithMultipleSegmentValues(contentName, documentTypeId, dataTypeName, editorAlias, [
    {value: defaultSegmentValue, segment: null},
    {value: vipSegmentValue, segment: vipMemberSegmentAlias},
  ]);
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.content.goToContentWithName(contentName);

  // Act
  await umbracoUi.content.enterTextstring('');
  await umbracoUi.content.clickSaveAndPublishButton();

  // Assert
  await umbracoUi.content.isValidationMessageVisible(ConstantHelper.validationMessages.emptyValue);
  await umbracoUi.content.doesErrorNotificationHaveText(NotificationConstantHelper.error.documentCouldNotBePublished);
  expect(await umbracoApi.document.isDocumentPublished(documentId)).toBeFalsy();
});

test('cannot publish segment-only content when the mandatory VIP segment value is empty', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const documentId = await umbracoApi.document.createDocumentWithMultipleSegmentValues(contentName, documentTypeId, dataTypeName, editorAlias, [
    {value: defaultSegmentValue, segment: null},
    {value: vipSegmentValue, segment: vipMemberSegmentAlias},
  ]);
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickSelectVariantButton();
  await umbracoUi.content.clickSegmentVariantButton(vipMemberSegment);

  // Act
  await umbracoUi.content.enterTextstring('');
  await umbracoUi.content.clickSaveAndPublishButton();

  // Assert
  await umbracoUi.content.isValidationMessageVisible(ConstantHelper.validationMessages.emptyValue);
  await umbracoUi.content.doesErrorNotificationHaveText(NotificationConstantHelper.error.documentCouldNotBePublished);
  expect(await umbracoApi.document.isDocumentPublished(documentId)).toBeFalsy();
});
