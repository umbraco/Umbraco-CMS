import {ConstantHelper, test} from '@umbraco/acceptance-test-helpers';
import {expect} from "@playwright/test";

// Content
const contentName = 'TestSegmentBlueprintContent';
// Document Blueprint
const documentBlueprintName = 'TestSegmentBlueprint';
// DocumentType
const documentTypeName = 'TestSegmentBlueprintDocType';
let documentTypeId = '';
// DataType
const dataTypeName = 'Textstring';
const editorAlias = 'Umbraco.TextBox';
// Segments
const vipMemberSegmentAlias = 'vip-members';
// Test Values
const defaultSegmentValue = 'Default segment value';
const vipSegmentValue = 'VIP segment value';

test.beforeEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.document.ensureNameNotExists(documentBlueprintName);
  await umbracoApi.documentBlueprint.ensureNameNotExists(contentName);
  await umbracoApi.documentBlueprint.ensureNameNotExists(documentBlueprintName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeData.id, 'TestGroup', false, false, false, true, true);
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.document.ensureNameNotExists(documentBlueprintName);
  await umbracoApi.documentBlueprint.ensureNameNotExists(contentName);
  await umbracoApi.documentBlueprint.ensureNameNotExists(documentBlueprintName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
});

// Skip this test due to this issue: https://github.com/umbraco/Umbraco-CMS/issues/24479
test.skip('can create a document blueprint from segment-only content with a VIP segment value', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.document.createDocumentWithMultipleSegmentValues(contentName, documentTypeId, dataTypeName, editorAlias, [
    {value: defaultSegmentValue, segment: null},
    {value: vipSegmentValue, segment: vipMemberSegmentAlias},
  ]);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.clickActionsMenuForContent(contentName);
  await umbracoUi.content.clickCreateBlueprintActionMenuOption();
  await umbracoUi.content.clickModalMenuItemWithName('Document Blueprints');
  await umbracoUi.content.clickSaveModalButtonAndWaitForDocumentBlueprintToBeCreated();

  // Assert
  expect(await umbracoApi.documentBlueprint.doesNameExist(contentName)).toBeTruthy();
  const blueprintData = await umbracoApi.documentBlueprint.getByName(contentName);
  const defaultValue = blueprintData.values.find(value => value.culture === null && value.segment === null);
  const vipValue = blueprintData.values.find(value => value.culture === null && value.segment === vipMemberSegmentAlias);
  expect(defaultValue).toBeTruthy();
  expect(defaultValue.value).toBe(defaultSegmentValue);
  expect(vipValue).toBeTruthy();
  expect(vipValue.value).toBe(vipSegmentValue);
});

test('can create segment-only content from a document blueprint with a VIP segment value', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.documentBlueprint.createDocumentBlueprintWithMultipleSegmentValues(documentBlueprintName, documentTypeId, dataTypeName, editorAlias, [
    {value: defaultSegmentValue, segment: null},
    {value: vipSegmentValue, segment: vipMemberSegmentAlias},
  ]);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.clickActionsMenuAtRoot();
  await umbracoUi.content.clickCreateActionMenuOption();
  await umbracoUi.content.chooseDocumentType(documentTypeName);
  await umbracoUi.content.selectDocumentBlueprintWithName(documentBlueprintName);
  await umbracoUi.content.clickSaveButtonAndWaitForContentToBeCreated();

  // Assert
  expect(await umbracoApi.document.doesNameExist(documentBlueprintName)).toBeTruthy();
  const [defaultValue, vipValue] = await umbracoApi.document.getValuesByCultureAndSegmentForDocument(documentBlueprintName, [
    {culture: null, segment: null},
    {culture: null, segment: vipMemberSegmentAlias},
  ]);
  expect(defaultValue).toBeTruthy();
  expect(defaultValue.value).toBe(defaultSegmentValue);
  expect(vipValue).toBeTruthy();
  expect(vipValue.value).toBe(vipSegmentValue);
});
