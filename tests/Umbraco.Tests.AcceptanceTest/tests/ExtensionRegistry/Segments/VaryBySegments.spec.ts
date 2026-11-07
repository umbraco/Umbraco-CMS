import {ConstantHelper, test} from '@umbraco/acceptance-test-helpers';
import {expect} from "@playwright/test";

// DocumentType
const documentTypeName = 'TestSegmentVaryDocType';
// Element Type
const elementTypeName = 'TestSegmentVaryElementType';
// DataType
const dataTypeName = 'Textstring';

test.beforeEach(async ({umbracoApi, umbracoUi}) => {
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.documentType.ensureNameNotExists(elementTypeName);
  await umbracoUi.goToBackOffice();
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.documentType.ensureNameNotExists(elementTypeName);
});

test('can allow vary by segments for a document type', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.documentType.createDefaultDocumentType(documentTypeName);
  await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);

  // Act
  await umbracoUi.documentType.goToDocumentType(documentTypeName);
  await umbracoUi.documentType.clickDocumentTypeSettingsTab();
  await umbracoUi.documentType.clickVaryBySegmentsButton();
  await umbracoUi.documentType.clickSaveButtonAndWaitForDocumentTypeToBeUpdated();

  // Assert
  const documentTypeData = await umbracoApi.documentType.getByName(documentTypeName);
  expect(documentTypeData.variesBySegment).toBeTruthy();
  expect(documentTypeData.variesByCulture).toBeFalsy();
});

test('can allow vary by segments for a property in a document type', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeData.id, 'TestGroup', false, false, false, true, false);
  await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);

  // Act
  await umbracoUi.documentType.goToDocumentType(documentTypeName);
  await umbracoUi.documentType.clickEditorSettingsButton();
  await umbracoUi.documentType.clickSharedAcrossSegmentsToggle();
  await umbracoUi.documentType.clickSubmitButton();
  await umbracoUi.documentType.clickSaveButtonAndWaitForDocumentTypeToBeUpdated();

  // Assert
  const documentTypeData = await umbracoApi.documentType.getByName(documentTypeName);
  expect(documentTypeData.properties[0].variesBySegment).toBeTruthy();
  expect(documentTypeData.properties[0].variesByCulture).toBeFalsy();
});

test('can allow vary by segments for a Library element type', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  await umbracoApi.documentType.createDefaultElementType(elementTypeName, 'TestGroup', dataTypeName, dataTypeData.id);
  await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);

  // Act
  await umbracoUi.documentType.goToDocumentType(elementTypeName);
  await umbracoUi.documentType.clickDocumentTypeSettingsTab();
  await umbracoUi.documentType.clickVaryBySegmentsButton();
  await umbracoUi.documentType.clickSaveButtonAndWaitForDocumentTypeToBeUpdated();

  // Assert
  const elementTypeData = await umbracoApi.documentType.getByName(elementTypeName);
  expect(elementTypeData.isElement).toBeTruthy();
  expect(elementTypeData.allowedInLibrary).toBeTruthy();
  expect(elementTypeData.variesBySegment).toBeTruthy();
});
