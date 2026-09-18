import {ConstantHelper, test} from '@umbraco/acceptance-test-helpers';
import {expect} from "@playwright/test";

// Content
const contentName = 'TestSegmentRollbackContent';
// DocumentType
const documentTypeName = 'TestSegmentRollbackDocType';
let documentTypeId = '';
// DataType
const dataTypeName = 'Textstring';
// Segments
const vipMemberSegmentAlias = 'vip-members';
// Test Values
const originalValue = 'Original value';
const updatedValue = 'Updated value';
const vipSegmentValue = 'VIP segment value';

test.describe('rollback for content with segments', () => {
  test.beforeEach(async ({umbracoApi}) => {
    await umbracoApi.document.ensureNameNotExists(contentName);
    await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
    const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
    // The document type varies by segment, but not by culture.
    documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeData.id, 'TestGroup', false, false, false, true, true);
  });

  test.afterEach(async ({umbracoApi}) => {
    await umbracoApi.document.ensureNameNotExists(contentName);
    await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  });

  test('can rollback a culture invariant document that has a segment value', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const documentId = await umbracoApi.document.createDocumentWithTextContent(contentName, documentTypeId, originalValue, dataTypeName);
    await umbracoApi.document.publish(documentId);
    const documentData = await umbracoApi.document.get(documentId);
    documentData.values[0].value = updatedValue;
    documentData.values.push({...documentData.values[0], segment: vipMemberSegmentAlias, value: vipSegmentValue});
    await umbracoApi.document.update(documentId, documentData);
    await umbracoApi.document.publish(documentId);
    await umbracoUi.goToBackOffice();
    await umbracoUi.content.goToSection(ConstantHelper.sections.content);

    // Act
    await umbracoUi.content.clickActionsMenuForContent(contentName);
    await umbracoUi.content.clickRollbackActionMenuOption();
    await umbracoUi.content.waitForRollbackItems();
    await umbracoUi.content.clickPreviousRollBackItem();
    await umbracoUi.content.clickRollbackContainerButton();

    // Assert
    await umbracoUi.content.isSuccessNotificationVisible();
    const [defaultValue] = await umbracoApi.document.getValuesByCultureAndSegmentForDocument(contentName, [{culture: null, segment: null}]);
    expect(defaultValue).toBeTruthy();
    expect(defaultValue.value).toBe(originalValue);
  });
});
