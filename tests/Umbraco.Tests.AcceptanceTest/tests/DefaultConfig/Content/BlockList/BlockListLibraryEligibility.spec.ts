import {ConstantHelper, test} from '@umbraco/acceptance-test-helpers';
import {expect} from "@playwright/test";

const contentName = 'TestContentEligibility';
const documentTypeName = 'TestDocumentTypeForEligibility';
const customDataTypeName = 'Custom Block List Eligibility';
const elementTypeName = 'EligibilityElement';

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.documentType.ensureNameNotExists(elementTypeName);
  await umbracoApi.dataType.ensureNameNotExists(customDataTypeName);
});

test('shows the Library tab in the block catalogue when the element type is allowed in the Library', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const elementTypeId = await umbracoApi.documentType.createEmptyElementType(elementTypeName, true);
  await umbracoApi.document.createDefaultDocumentWithAnEmptyBlockListEditor(contentName, elementTypeId, documentTypeName, customDataTypeName);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickAddBlockElementButton();

  // Assert
  await umbracoUi.content.isLibraryTabInBlockCatalogueVisible(true);
});

test('hides the Library tab in the block catalogue when the element type is not allowed in the Library', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const elementTypeId = await umbracoApi.documentType.createEmptyElementType(elementTypeName, false);
  await umbracoApi.document.createDefaultDocumentWithAnEmptyBlockListEditor(contentName, elementTypeId, documentTypeName, customDataTypeName);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickAddBlockElementButton();

  // Assert
  await umbracoUi.content.isLibraryTabInBlockCatalogueVisible(false);
});

test('enabling Allow in Library on an element type sets the allowedInLibrary flag', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.documentType.createEmptyElementType(elementTypeName, false);
  await umbracoUi.goToBackOffice();
  await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);

  // Act
  await umbracoUi.documentType.goToDocumentType(elementTypeName);
  await umbracoUi.documentType.clickStructureTab();
  await umbracoUi.documentType.clickAllowInLibraryButton();
  await umbracoUi.documentType.clickSaveButtonAndWaitForDocumentTypeToBeUpdated();

  // Assert
  const documentTypeData = await umbracoApi.documentType.getByName(elementTypeName);
  expect(documentTypeData.allowedInLibrary).toBeTruthy();
});
