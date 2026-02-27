import {ConstantHelper, test} from '@umbraco/playwright-testhelpers';

// Element
const targetElementName = 'TestTargetElement';
const referencingElementName = 'TestReferencingElement';
const secondReferencingElementName = 'TestSecondReferencingElement';
let targetElementId = '';
// Content
const referencingContentName = 'TestReferencingContent';
// Element Types
const targetElementTypeName = 'TestTargetElementType';
const referencingElementTypeName = 'TestReferencingElementType';
let targetElementTypeId = '';
let referencingElementTypeId = '';
// Document Type
const referencingDocumentTypeName = 'TestReferencingDocumentType';
let referencingDocumentTypeId = '';
// Data Types
const elementPickerDataTypeName = 'TestElementPickerForInfoTab';
const textStringDataTypeName = 'Textstring';
let elementPickerDataTypeId = '';

test.beforeEach(async ({umbracoApi, umbracoUi}) => {
  // Create Element Picker data type
  elementPickerDataTypeId = await umbracoApi.dataType.createDefaultElementPickerDataType(elementPickerDataTypeName);
  // Create target element type with Textstring property
  const textDataType = await umbracoApi.dataType.getByName(textStringDataTypeName);
  targetElementTypeId = await umbracoApi.documentType.createElementTypeWithPropertyInTab(targetElementTypeName, 'TestTab', 'TestGroup', textStringDataTypeName, textDataType.id);
  // Create referencing element type with Element Picker property
  referencingElementTypeId = await umbracoApi.documentType.createElementTypeWithPropertyInTab(referencingElementTypeName, 'TestTab', 'TestGroup', elementPickerDataTypeName, elementPickerDataTypeId);
  // Create referencing document type with Element Picker property
  referencingDocumentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(referencingDocumentTypeName, elementPickerDataTypeName, elementPickerDataTypeId);
  // Create target element
  targetElementId = await umbracoApi.element.createDefaultElement(targetElementName, targetElementTypeId);
  await umbracoUi.goToBackOffice();
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.element.ensureNameNotExists(targetElementName);
  await umbracoApi.element.ensureNameNotExists(referencingElementName);
  await umbracoApi.element.ensureNameNotExists(secondReferencingElementName);
  await umbracoApi.document.ensureNameNotExists(referencingContentName);
  await umbracoApi.documentType.ensureNameNotExists(targetElementTypeName);
  await umbracoApi.documentType.ensureNameNotExists(referencingElementTypeName);
  await umbracoApi.documentType.ensureNameNotExists(referencingDocumentTypeName);
  await umbracoApi.dataType.ensureNameNotExists(elementPickerDataTypeName);
});

test('can see no references in info tab for an unreferenced element', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoUi.library.goToSection(ConstantHelper.sections.library);

  // Act
  await umbracoUi.library.goToElementWithName(targetElementName);
  await umbracoUi.library.clickInfoTab();

  // Assert
  await umbracoUi.library.isNoReferencesTextVisible();
});

test('can see one reference in info tab when element is referenced by one element', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.element.createElementWithElementPickers(referencingElementName, referencingElementTypeId, elementPickerDataTypeName, [targetElementId]);
  await umbracoUi.library.goToSection(ConstantHelper.sections.library);

  // Act
  await umbracoUi.library.goToElementWithName(targetElementName);
  await umbracoUi.library.clickInfoTab();

  // Assert
  await umbracoUi.library.doesReferencesItemsInInfoTabHaveCount(1);
  await umbracoUi.library.isReferenceItemNameVisible(referencingElementName);
});

test('can see multiple references in info tab when element is referenced by multiple elements', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.element.createElementWithElementPickers(referencingElementName, referencingElementTypeId, elementPickerDataTypeName, [targetElementId]);
  await umbracoApi.element.createElementWithElementPickers(secondReferencingElementName, referencingElementTypeId, elementPickerDataTypeName, [targetElementId]);
  await umbracoUi.library.goToSection(ConstantHelper.sections.library);

  // Act
  await umbracoUi.library.goToElementWithName(targetElementName);
  await umbracoUi.library.clickInfoTab();

  // Assert
  await umbracoUi.library.doesReferencesItemsInInfoTabHaveCount(2);
  await umbracoUi.library.isReferenceItemNameVisible(referencingElementName);
  await umbracoUi.library.isReferenceItemNameVisible(secondReferencingElementName);
});

test('can see reference is removed in info tab after deleting the referencing element', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const referencingElementId = await umbracoApi.element.createElementWithElementPickers(referencingElementName, referencingElementTypeId, elementPickerDataTypeName, [targetElementId]);
  await umbracoApi.element.delete(referencingElementId);
  await umbracoUi.library.goToSection(ConstantHelper.sections.library);

  // Act
  await umbracoUi.library.goToElementWithName(targetElementName);
  await umbracoUi.library.clickInfoTab();

  // Assert
  await umbracoUi.library.isNoReferencesTextVisible();
});

test('can see one reference in info tab when element is referenced by one content', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.document.createDocumentWithElementPickers(referencingContentName, referencingDocumentTypeId, elementPickerDataTypeName, [targetElementId]);
  await umbracoUi.library.goToSection(ConstantHelper.sections.library);

  // Act
  await umbracoUi.library.goToElementWithName(targetElementName);
  await umbracoUi.library.clickInfoTab();

  // Assert
  await umbracoUi.library.doesReferencesItemsInInfoTabHaveCount(1);
  await umbracoUi.library.isReferenceItemNameVisible(referencingContentName);
});

test('can see references from both content and element in info tab', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.element.createElementWithElementPickers(referencingElementName, referencingElementTypeId, elementPickerDataTypeName, [targetElementId]);
  await umbracoApi.document.createDocumentWithElementPickers(referencingContentName, referencingDocumentTypeId, elementPickerDataTypeName, [targetElementId]);
  await umbracoUi.library.goToSection(ConstantHelper.sections.library);

  // Act
  await umbracoUi.library.goToElementWithName(targetElementName);
  await umbracoUi.library.clickInfoTab();

  // Assert
  await umbracoUi.library.doesReferencesItemsInInfoTabHaveCount(2);
  await umbracoUi.library.isReferenceItemNameVisible(referencingElementName);
  await umbracoUi.library.isReferenceItemNameVisible(referencingContentName);
});

test('can see content reference is removed in info tab after trashing the referencing content', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const referencingContentId = await umbracoApi.document.createDocumentWithElementPickers(referencingContentName, referencingDocumentTypeId, elementPickerDataTypeName, [targetElementId]);
  await umbracoApi.document.delete(referencingContentId);
  await umbracoUi.library.goToSection(ConstantHelper.sections.library);

  // Act
  await umbracoUi.library.goToElementWithName(targetElementName);
  await umbracoUi.library.clickInfoTab();

  // Assert
  await umbracoUi.library.isNoReferencesTextVisible();
});
