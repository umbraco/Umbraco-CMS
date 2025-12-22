import {ConstantHelper, test} from '@umbraco/playwright-testhelpers';

/**
 * Test Scenarios: Block List Variance Combinations
 * Configuration: AllowEditInvariantFromNonDefault = false (default)
 *
 * Key principle:
 * - Variant Block List = each language has its own blocks = properties editable in all languages
 * - Invariant Block List = blocks shared across languages = AllowEditInvariantFromNonDefault applies
 */

const documentTypeName = 'TestDocType';
const variantBlockListName = 'VariantBlockList';
const invariantBlockListName = 'InvariantBlockList';
const invariantBlockElementName = 'InvariantBlockElement';
const invariantTextPropertyName = 'SharedProperty';
const contentName = 'TestContent';
const textStringDataTypeName = 'Textstring';

let textStringDataTypeId = '';

test.beforeEach(async ({umbracoApi}) => {
  await umbracoApi.language.createDanishLanguage();
  const textStringDataType = await umbracoApi.dataType.getByName(textStringDataTypeName);
  textStringDataTypeId = textStringDataType.id;
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.documentType.ensureNameNotExists(invariantBlockElementName);
  await umbracoApi.dataType.ensureNameNotExists(variantBlockListName);
  await umbracoApi.dataType.ensureNameNotExists(invariantBlockListName);
  await umbracoApi.language.ensureIsoCodeNotExists('da');
});

/**
 * VARIANT BLOCK LIST with INVARIANT BLOCK
 * Document Type (variant)
 * └── Block List Property (variant)
 *     └── Block Element (invariant)
 *         └── Property (invariant)
 * Expected: Properties EDITABLE in non-default language (blocks not shared)
 */

test('can save invariant property in invariant block when block list is variant - default language', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const testValue = 'English block content';
  const elementTypeId = await umbracoApi.documentType.createDefaultElementTypeWithVaryByCulture(
    invariantBlockElementName, 'Content', invariantTextPropertyName, textStringDataTypeId, false, false
  );
  const blockListId = await umbracoApi.dataType.createBlockListDataTypeWithABlock(variantBlockListName, elementTypeId);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(
    documentTypeName, variantBlockListName, blockListId, 'Content', true, true
  );
  await umbracoApi.document.createDefaultDocumentWithEnglishCulture(contentName, documentTypeId);

  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickAddBlockElementButton();
  await umbracoUi.content.clickBlockElementWithName(invariantBlockElementName);
  await umbracoUi.content.enterBlockPropertyValue(invariantTextPropertyName, testValue);
  await umbracoUi.content.clickCreateModalButton();
  await umbracoUi.content.clickSaveAndPublishButton();
  await umbracoUi.content.clickContainerSaveAndPublishButton();

  // Assert
  await umbracoUi.content.isSuccessNotificationVisible();
});

test('can save invariant property in invariant block when block list is variant - non-default language', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const testValue = 'Danish block content';
  const elementTypeId = await umbracoApi.documentType.createDefaultElementTypeWithVaryByCulture(
    invariantBlockElementName, 'Content', invariantTextPropertyName, textStringDataTypeId, false, false
  );
  const blockListId = await umbracoApi.dataType.createBlockListDataTypeWithABlock(variantBlockListName, elementTypeId);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(
    documentTypeName, variantBlockListName, blockListId, 'Content', true, true
  );
  await umbracoApi.document.createDefaultDocumentWithEnglishCulture(contentName, documentTypeId);

  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickSaveAndPublishButton();
  await umbracoUi.content.clickContainerSaveAndPublishButton();
  await umbracoUi.content.isSuccessNotificationVisible();

  // Act
  await umbracoUi.content.switchLanguage('da');
  await umbracoUi.content.clickAddBlockElementButton();
  await umbracoUi.content.clickBlockElementWithName(invariantBlockElementName);
  await umbracoUi.content.enterBlockPropertyValue(invariantTextPropertyName, testValue);
  await umbracoUi.content.clickCreateModalButton();
  await umbracoUi.content.clickSaveAndPublishButton();
  await umbracoUi.content.clickContainerSaveAndPublishButton();

  // Assert
  await umbracoUi.content.isSuccessNotificationVisible();
});

/**
 * INVARIANT BLOCK LIST with INVARIANT BLOCK
 * Document Type (variant)
 * └── Block List Property (invariant)
 *     └── Block Element (invariant)
 *         └── Property (invariant)
 * Expected: Properties NOT editable in non-default language (blocks shared)
 */

test('can save invariant property in invariant block when block list is invariant - default language', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const testValue = 'English block content';
  const elementTypeId = await umbracoApi.documentType.createDefaultElementTypeWithVaryByCulture(
    invariantBlockElementName, 'Content', invariantTextPropertyName, textStringDataTypeId, false, false
  );
  const blockListId = await umbracoApi.dataType.createBlockListDataTypeWithABlock(invariantBlockListName, elementTypeId);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(
    documentTypeName, invariantBlockListName, blockListId, 'Content', true, false
  );
  await umbracoApi.document.createDefaultDocumentWithEnglishCulture(contentName, documentTypeId);

  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickAddBlockElementButton();
  await umbracoUi.content.clickBlockElementWithName(invariantBlockElementName);
  await umbracoUi.content.enterBlockPropertyValue(invariantTextPropertyName, testValue);
  await umbracoUi.content.clickCreateModalButton();
  await umbracoUi.content.clickSaveAndPublishButton();
  await umbracoUi.content.clickContainerSaveAndPublishButton();

  // Assert
  await umbracoUi.content.isSuccessNotificationVisible();
});
