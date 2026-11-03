import {ConstantHelper, NotificationConstantHelper, test} from '@umbraco/acceptance-test-helpers';
import {expect} from "@playwright/test";

// Content
const contentName = 'TestSegmentDuplicateContent';
const duplicatedContentName = contentName + ' (1)';
// DocumentType
const documentTypeName = 'TestSegmentDuplicateDocType';
let documentTypeId = '';
// DataType
const dataTypeName = 'Textstring';
const editorAlias = 'Umbraco.TextBox';
// Segments
const vipMemberSegmentAlias = 'vip-members';
// Test Values
const defaultSegmentValue = 'Default segment value';
const vipSegmentValue = 'VIP segment value';

test.describe('duplicate content with segment-only variations', () => {
  test.beforeEach(async ({umbracoApi}) => {
    await umbracoApi.document.ensureNameNotExists(duplicatedContentName);
    await umbracoApi.document.ensureNameNotExists(contentName);
    await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
    const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
    documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeData.id, 'TestGroup', false, false, false, true, true);
  });

  test.afterEach(async ({umbracoApi}) => {
    await umbracoApi.document.ensureNameNotExists(duplicatedContentName);
    await umbracoApi.document.ensureNameNotExists(contentName);
    await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  });

  test('can duplicate segment-only content with a VIP segment value', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    await umbracoApi.document.createDocumentWithMultipleSegmentValues(contentName, documentTypeId, dataTypeName, editorAlias, [
      {value: defaultSegmentValue, segment: null},
      {value: vipSegmentValue, segment: vipMemberSegmentAlias},
    ]);
    await umbracoUi.goToBackOffice();
    await umbracoUi.content.goToSection(ConstantHelper.sections.content);

    // Act
    await umbracoUi.content.clickActionsMenuForContent(contentName);
    await umbracoUi.content.clickDuplicateToActionMenuOption();
    await umbracoUi.content.clickLabelWithName('Content');
    await umbracoUi.content.clickCopyModalButton();

    // Assert
    await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.duplicated);
    expect(await umbracoApi.document.doesNameExist(duplicatedContentName)).toBeTruthy();
    const [defaultValue, vipValue] = await umbracoApi.document.getValuesByCultureAndSegmentForDocument(duplicatedContentName, [
      {culture: null, segment: null},
      {culture: null, segment: vipMemberSegmentAlias},
    ]);
    expect(defaultValue).toBeTruthy();
    expect(defaultValue.value).toBe(defaultSegmentValue);
    expect(vipValue).toBeTruthy();
    expect(vipValue.value).toBe(vipSegmentValue);
  });
});
