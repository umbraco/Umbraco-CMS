import {ConstantHelper, test, AliasHelper} from '@umbraco/playwright-testhelpers';

// Content
const englishContentName = 'English Content';
const danishContentName = 'Danish Content';
// Document Type
const documentTypeName = 'TestDocumentTypeForContent';
// Data Type
const textStringDataTypeName = 'Textstring';
const textStringEditorAlias = 'Umbraco.Textstring';
let textStringDataType: any;
// Languages
const firstCulture = 'en-US';
const secondCulture = 'da';
// Cultures
const cultures = [
  {isoCode: firstCulture, name: englishContentName},
  {isoCode: secondCulture, name: danishContentName},
];
// Block List
const firstBlockListName = 'FirstBlockList';
const secondBlockListName = 'SecondBlockList';
// Element Type
const firstBlockElementTypeName = 'FirstBlockElementType';
const secondBlockElementTypeName = 'SecondBlockElementType';
// Block Properties
const firstTextName = 'FirstText';
const secondTextName = 'SecondText';

test.beforeEach(async ({umbracoApi}) => {
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.language.createDanishLanguage();
  textStringDataType = await umbracoApi.dataType.getByName(textStringDataTypeName);
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(englishContentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.documentType.ensureNameNotExists(firstBlockElementTypeName);
  await umbracoApi.documentType.ensureNameNotExists(secondBlockElementTypeName);
  await umbracoApi.dataType.ensureNameNotExists(firstBlockListName);
  await umbracoApi.dataType.ensureNameNotExists(secondBlockListName);
  await umbracoApi.language.ensureIsoCodeNotExists(secondCulture);
});

test('can edit variant text property in non-default language when AllowEditInvariantFromNonDefault is false', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithVariantAndInvariantBlockLists(
    documentTypeName,
    firstTextName,
    textStringDataType.id,
    secondTextName,
    textStringDataType.id,
    firstBlockListName,
    secondBlockListName,
    firstBlockElementTypeName,
    secondBlockElementTypeName
  );
  await umbracoApi.document.createDocumentWithMultipleVariantsWithSharedProperty(englishContentName, documentTypeId, AliasHelper.toAlias(secondTextName), textStringEditorAlias, cultures, '');
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(englishContentName);
  await umbracoUi.content.clickSaveAndPublishButton();
  await umbracoUi.content.clickContainerSaveAndPublishButton();
  await umbracoUi.content.isSuccessNotificationVisible();
  await umbracoUi.content.switchLanguage(secondCulture);

  // Assert
  await umbracoUi.content.isDocumentPropertyEditable(firstTextName, true);
});

test('cannot edit invariant text property in non-default language when AllowEditInvariantFromNonDefault is false', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithVariantAndInvariantBlockLists(
    documentTypeName,
    firstTextName,
    textStringDataType.id,
    secondTextName,
    textStringDataType.id,
    firstBlockListName,
    secondBlockListName,
    firstBlockElementTypeName,
    secondBlockElementTypeName
  );
  await umbracoApi.document.createDocumentWithMultipleVariantsWithSharedProperty(englishContentName, documentTypeId, AliasHelper.toAlias(secondTextName), textStringEditorAlias, cultures, '');
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(englishContentName);
  await umbracoUi.content.clickSaveAndPublishButton();
  await umbracoUi.content.clickContainerSaveAndPublishButton();
  await umbracoUi.content.isSuccessNotificationVisible();
  await umbracoUi.content.switchLanguage(secondCulture);

  // Assert
  await umbracoUi.content.isDocumentPropertyEditable(secondTextName, false);
});

test('can edit variant block list in non-default language when AllowEditInvariantFromNonDefault is false', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithVariantAndInvariantBlockLists(
    documentTypeName,
    firstTextName,
    textStringDataType.id,
    secondTextName,
    textStringDataType.id,
    firstBlockListName,
    secondBlockListName,
    firstBlockElementTypeName,
    secondBlockElementTypeName
  );
  await umbracoApi.document.createDocumentWithMultipleVariantsWithSharedProperty(englishContentName, documentTypeId, AliasHelper.toAlias(secondTextName), textStringEditorAlias, cultures, '');
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(englishContentName);
  await umbracoUi.content.clickSaveAndPublishButton();
  await umbracoUi.content.clickContainerSaveAndPublishButton();
  await umbracoUi.content.isSuccessNotificationVisible();
  await umbracoUi.content.switchLanguage(secondCulture);

  // Assert
  await umbracoUi.content.isAddBlockListElementWithNameVisible(firstBlockListName);
});

test('cannot create block in invariant block list in non-default language when AllowEditInvariantFromNonDefault is false', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithVariantAndInvariantBlockLists(
    documentTypeName,
    firstTextName,
    textStringDataType.id,
    secondTextName,
    textStringDataType.id,
    firstBlockListName,
    secondBlockListName,
    firstBlockElementTypeName,
    secondBlockElementTypeName
  );
  await umbracoApi.document.createDocumentWithMultipleVariantsWithSharedProperty(englishContentName, documentTypeId, AliasHelper.toAlias(secondTextName), textStringEditorAlias, cultures, '');
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(englishContentName);
  await umbracoUi.content.clickSaveAndPublishButton();
  await umbracoUi.content.clickContainerSaveAndPublishButton();
  await umbracoUi.content.isSuccessNotificationVisible();
  await umbracoUi.content.switchLanguage(secondCulture);

  // Assert
  await umbracoUi.content.isAddBlockListElementWithNameDisabled(secondBlockListName);
});

test('can edit variant text property inside a variant block in non-default language when AllowEditInvariantFromNonDefault is false', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithVariantAndInvariantBlockLists(
    documentTypeName,
    firstTextName,
    textStringDataType.id,
    secondTextName,
    textStringDataType.id,
    firstBlockListName,
    secondBlockListName,
    firstBlockElementTypeName,
    secondBlockElementTypeName
  );
  await umbracoApi.document.createDocumentWithMultipleVariantsWithSharedProperty(englishContentName, documentTypeId, AliasHelper.toAlias(secondTextName), textStringEditorAlias, cultures, '');
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(englishContentName);
  await umbracoUi.content.clickAddBlockListElementWithName(firstBlockListName);
  await umbracoUi.content.clickBlockElementWithName(firstBlockElementTypeName);
  await umbracoUi.content.enterBlockPropertyValue(firstTextName, 'Variant text in block');
  await umbracoUi.content.enterBlockPropertyValue(secondTextName, 'Invariant text in block');
  await umbracoUi.content.clickCreateModalButton();
  await umbracoUi.content.clickSaveAndPublishButton();
  await umbracoUi.content.clickContainerSaveAndPublishButton();
  await umbracoUi.content.isSuccessNotificationVisible();
  await umbracoUi.content.switchLanguage(secondCulture);
  await umbracoUi.content.clickAddBlockListElementWithName(firstBlockListName);
  await umbracoUi.content.clickBlockElementWithName(firstBlockElementTypeName);

  // Assert
  await umbracoUi.content.isBlockPropertyEditable(firstTextName, true);
});

test('can edit invariant text property inside a variant block in non-default language when AllowEditInvariantFromNonDefault is false', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithVariantAndInvariantBlockLists(
    documentTypeName,
    firstTextName,
    textStringDataType.id,
    secondTextName,
    textStringDataType.id,
    firstBlockListName,
    secondBlockListName,
    firstBlockElementTypeName,
    secondBlockElementTypeName
  );
  await umbracoApi.document.createDocumentWithMultipleVariantsWithSharedProperty(englishContentName, documentTypeId, AliasHelper.toAlias(secondTextName), textStringEditorAlias, cultures, '');
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(englishContentName);
  await umbracoUi.content.clickAddBlockListElementWithName(firstBlockListName);
  await umbracoUi.content.clickBlockElementWithName(firstBlockElementTypeName);
  await umbracoUi.content.enterBlockPropertyValue(firstTextName, 'Variant text in block');
  await umbracoUi.content.enterBlockPropertyValue(secondTextName, 'Invariant text in block');
  await umbracoUi.content.clickCreateModalButton();
  await umbracoUi.content.clickSaveAndPublishButton();
  await umbracoUi.content.clickContainerSaveAndPublishButton();
  await umbracoUi.content.isSuccessNotificationVisible();
  await umbracoUi.content.switchLanguage(secondCulture);
  await umbracoUi.content.clickAddBlockListElementWithName(firstBlockListName);
  await umbracoUi.content.clickBlockElementWithName(firstBlockElementTypeName);

  // Assert
  await umbracoUi.content.isBlockPropertyEditable(secondTextName, true);
});

test('can edit variant text property inside invariant block in non-default language when AllowEditInvariantFromNonDefault is false', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithVariantAndInvariantBlockLists(
    documentTypeName,
    firstTextName,
    textStringDataType.id,
    secondTextName,
    textStringDataType.id,
    firstBlockListName,
    secondBlockListName,
    firstBlockElementTypeName,
    secondBlockElementTypeName
  );
  await umbracoApi.document.createDocumentWithMultipleVariantsWithSharedProperty(englishContentName, documentTypeId, AliasHelper.toAlias(secondTextName), textStringEditorAlias, cultures, '');
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(englishContentName);
  await umbracoUi.content.clickAddBlockListElementWithName(secondBlockListName);
  await umbracoUi.content.clickBlockElementWithName(firstBlockElementTypeName);
  await umbracoUi.content.enterBlockPropertyValue(firstTextName, 'Text1 in invariant block');
  await umbracoUi.content.enterBlockPropertyValue(secondTextName, 'Text2 in invariant block');
  await umbracoUi.content.clickCreateModalButton();
  await umbracoUi.content.clickSaveAndPublishButton();
  await umbracoUi.content.clickContainerSaveAndPublishButton();
  await umbracoUi.content.isSuccessNotificationVisible();
  await umbracoUi.content.switchLanguage(secondCulture);
  await umbracoUi.content.clickEditBlockListBlockButton();

  // Assert
  await umbracoUi.content.isBlockPropertyEditable(firstTextName, true);
});

test('cannot edit invariant text property inside an invariant block in non-default language when AllowEditInvariantFromNonDefault is false', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithVariantAndInvariantBlockLists(
    documentTypeName,
    firstTextName,
    textStringDataType.id,
    secondTextName,
    textStringDataType.id,
    firstBlockListName,
    secondBlockListName,
    firstBlockElementTypeName,
    secondBlockElementTypeName
  );
  await umbracoApi.document.createDocumentWithMultipleVariantsWithSharedProperty(englishContentName, documentTypeId, AliasHelper.toAlias(secondTextName), textStringEditorAlias, cultures, '');
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(englishContentName);
  await umbracoUi.content.clickAddBlockListElementWithName(secondBlockListName);
  await umbracoUi.content.clickBlockElementWithName(firstBlockElementTypeName);
  await umbracoUi.content.enterBlockPropertyValue(firstTextName, 'Text1 in invariant block');
  await umbracoUi.content.enterBlockPropertyValue(secondTextName, 'Text2 in invariant block');
  await umbracoUi.content.clickCreateModalButton();
  await umbracoUi.content.clickSaveAndPublishButton();
  await umbracoUi.content.clickContainerSaveAndPublishButton();
  await umbracoUi.content.isSuccessNotificationVisible();
  await umbracoUi.content.switchLanguage(secondCulture);
  await umbracoUi.content.clickEditBlockListBlockButton();

  // Assert
  await umbracoUi.content.isBlockPropertyEditable(secondTextName, false);
});
