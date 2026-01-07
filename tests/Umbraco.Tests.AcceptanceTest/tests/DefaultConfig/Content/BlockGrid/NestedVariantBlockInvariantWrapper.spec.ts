import {ConstantHelper, test} from "@umbraco/playwright-testhelpers";

/**
 * Verifies variant blocks deep inside invariant wrappers correctly receive culture context.
 *
 * Structure:
 * - Variant Document Type (varies by culture)
 *   - Invariant Block Grid (outer property)
 *     - Invariant Wrapper Block
 *       - Invariant Block Grid (nested inside wrapper)
 *         - Variant Block (with variant and invariant properties)
 */

const documentTypeName = 'VariantDocType';
const documentTypeGroupName = 'DocumentGroup';
const outerBlockGridName = 'OuterBlockGrid';
const wrapperBlockName = 'WrapperBlock';
const wrapperGroupName = 'WrapperGroup';
const innerBlockGridName = 'InnerBlockGrid';
const variantBlockName = 'VariantBlock';
const variantBlockGroupName = 'VariantBlockGroup';
const variantPropertyName = 'CultureSpecific';
const invariantPropertyName = 'SharedProperty';
const textStringDataTypeName = 'Textstring';
const contentName = 'TestContent';
const englishVariantText = 'Hello English Variant';
const invariantText = 'Shared Invariant Text';

test.beforeEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.documentType.ensureNameNotExists(variantBlockName);
  await umbracoApi.documentType.ensureNameNotExists(wrapperBlockName);
  await umbracoApi.dataType.ensureNameNotExists(outerBlockGridName);
  await umbracoApi.dataType.ensureNameNotExists(innerBlockGridName);
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.documentType.ensureNameNotExists(variantBlockName);
  await umbracoApi.documentType.ensureNameNotExists(wrapperBlockName);
  await umbracoApi.dataType.ensureNameNotExists(outerBlockGridName);
  await umbracoApi.dataType.ensureNameNotExists(innerBlockGridName);
});

test('variant block values are readable in UI after page reload', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const textStringDataType = await umbracoApi.dataType.getByName(textStringDataTypeName);
  const textStringDataTypeId = textStringDataType.id;

  const variantBlockId = await umbracoApi.documentType.createVariantElementTypeWithVariantAndInvariantProperty(variantBlockName, variantBlockGroupName, variantPropertyName, invariantPropertyName, textStringDataTypeId);
  const innerBlockGridId = await umbracoApi.dataType.createBlockGridWithABlockAndAllowAtRoot(innerBlockGridName, variantBlockId, true);
  const wrapperBlockId = await umbracoApi.documentType.createDefaultElementType(wrapperBlockName, wrapperGroupName, innerBlockGridName, innerBlockGridId);
  const outerBlockGridId = await umbracoApi.dataType.createBlockGridWithABlockAndAllowAtRoot(outerBlockGridName, wrapperBlockId, true);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, outerBlockGridName, outerBlockGridId, documentTypeGroupName, true, false);
  await umbracoApi.document.createDefaultDocumentWithEnglishCulture(contentName, documentTypeId);

  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.content.goToContentWithName(contentName);

  // Act
  await umbracoUi.content.clickAddBlockElementButton();
  await umbracoUi.content.clickBlockElementWithName(wrapperBlockName);
  await umbracoUi.content.clickAddBlockWithNameButton(variantBlockName);
  await umbracoUi.content.clickBlockElementWithName(variantBlockName);
  await umbracoUi.content.enterPropertyValue(variantPropertyName, englishVariantText);
  await umbracoUi.content.enterPropertyValue(invariantPropertyName, invariantText);
  await umbracoUi.content.clickCreateInModal('Add ' + variantBlockName);
  await umbracoUi.content.clickCreateInModal('Add ' + wrapperBlockName, {waitForClose: 'target'});
  await umbracoUi.content.clickSaveButtonAndWaitForContentToBeUpdated();
  await umbracoUi.reloadPage();

  // Assert
  await umbracoUi.content.goToBlockGridBlockWithName(documentTypeGroupName, outerBlockGridName, wrapperBlockName);
  await umbracoUi.content.clickEditBlockGridEntryWithName(variantBlockName);
  await umbracoUi.content.doesPropertyContainValue(variantPropertyName, englishVariantText);
  await umbracoUi.content.doesPropertyContainValue(invariantPropertyName, invariantText);
});
