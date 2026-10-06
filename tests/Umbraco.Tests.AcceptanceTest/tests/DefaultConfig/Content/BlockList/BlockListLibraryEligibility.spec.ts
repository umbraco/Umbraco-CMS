import {ConstantHelper, test} from '@umbraco/acceptance-test-helpers';

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
