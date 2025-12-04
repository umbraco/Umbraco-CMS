import {ConstantHelper, test, AliasHelper} from '@umbraco/playwright-testhelpers';

// Content
const englishContentName = 'English Content';
const danishContentName = 'Danish Content';
// Document Type
const documentTypeName = 'TestDocumentTypeForContent';
// Data Type
const textStringDataTypeName = 'Textstring';
const textStringEditorAlias = 'Umbraco.Textstring';
// Languages
const firstCulture = 'en-US';
const secondCulture = 'da';
// Cultures
const cultures = [
  {isoCode: firstCulture, name: englishContentName},
  {isoCode: secondCulture, name: danishContentName},
];
// Block List
const blockList1Name = 'BlockList1';
const blockList2Name = 'BlockList2';
// Element Type
const block1ElementTypeName = 'Block1ElementType';
const block2ElementTypeName = 'Block2ElementType';
// Block Properties
const text1Name = 'Text1';
const text2Name = 'Text2';

test.beforeEach(async ({umbracoApi}) => {
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.language.createDanishLanguage();
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(englishContentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.documentType.ensureNameNotExists(block1ElementTypeName);
  await umbracoApi.documentType.ensureNameNotExists(block2ElementTypeName);
  await umbracoApi.dataType.ensureNameNotExists(blockList1Name);
  await umbracoApi.dataType.ensureNameNotExists(blockList2Name);
  await umbracoApi.language.ensureIsoCodeNotExists(secondCulture);
});

test('can edit variant text property in non-default language when AllowEditInvariantFromNonDefault is true', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const textStringDataType = await umbracoApi.dataType.getByName(textStringDataTypeName);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithVariantAndInvariantBlockLists(
    documentTypeName,
    text1Name,
    textStringDataType.id,
    text2Name,
    textStringDataType.id,
    blockList1Name,
    blockList2Name,
    block1ElementTypeName,
    block2ElementTypeName
  );
  await umbracoApi.document.createDocumentWithMultipleVariantsWithSharedProperty(englishContentName, documentTypeId, AliasHelper.toAlias(text2Name), textStringEditorAlias, cultures, '');
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(englishContentName);
  await umbracoUi.content.clickSaveAndPublishButton();
  await umbracoUi.content.clickContainerSaveAndPublishButton();
  await umbracoUi.content.isSuccessNotificationVisible();
  await umbracoUi.content.switchLanguage(secondCulture);

  // Assert
  await umbracoUi.content.isDocumentPropertyEditable(text1Name, true);
});

// Currently failing due to issue: invariant text properties not being editable in non-default languages
test.skip('can edit invariant text property in non-default language when AllowEditInvariantFromNonDefault is true', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const textStringDataType = await umbracoApi.dataType.getByName(textStringDataTypeName);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithVariantAndInvariantBlockLists(
    documentTypeName,
    text1Name,
    textStringDataType.id,
    text2Name,
    textStringDataType.id,
    blockList1Name,
    blockList2Name,
    block1ElementTypeName,
    block2ElementTypeName
  );
  await umbracoApi.document.createDocumentWithMultipleVariantsWithSharedProperty(englishContentName, documentTypeId, AliasHelper.toAlias(text2Name), textStringEditorAlias, cultures, '');
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(englishContentName);
  await umbracoUi.content.clickSaveAndPublishButton();
  await umbracoUi.content.clickContainerSaveAndPublishButton();
  await umbracoUi.content.isSuccessNotificationVisible();
  await umbracoUi.content.switchLanguage(secondCulture);

  // Assert
  await umbracoUi.content.isDocumentPropertyEditable(text2Name, true);
});

test('can edit variant block list in non-default language when AllowEditInvariantFromNonDefault is true', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const textStringDataType = await umbracoApi.dataType.getByName(textStringDataTypeName);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithVariantAndInvariantBlockLists(
    documentTypeName,
    text1Name,
    textStringDataType.id,
    text2Name,
    textStringDataType.id,
    blockList1Name,
    blockList2Name,
    block1ElementTypeName,
    block2ElementTypeName
  );
  await umbracoApi.document.createDocumentWithMultipleVariantsWithSharedProperty(englishContentName, documentTypeId, AliasHelper.toAlias(text2Name), textStringEditorAlias, cultures, '');
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(englishContentName);
  await umbracoUi.content.clickSaveAndPublishButton();
  await umbracoUi.content.clickContainerSaveAndPublishButton();
  await umbracoUi.content.isSuccessNotificationVisible();
  await umbracoUi.content.switchLanguage(secondCulture);

  // Assert
  await umbracoUi.content.isAddBlockListElementWithNameVisible(blockList1Name);
});

// Currently failing due to issue: invariant block lists not being editable in non-default languages
test.skip('can edit invariant block list in non-default language when AllowEditInvariantFromNonDefault is true', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const textStringDataType = await umbracoApi.dataType.getByName(textStringDataTypeName);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithVariantAndInvariantBlockLists(
    documentTypeName,
    text1Name,
    textStringDataType.id,
    text2Name,
    textStringDataType.id,
    blockList1Name,
    blockList2Name,
    block1ElementTypeName,
    block2ElementTypeName
  );
  await umbracoApi.document.createDocumentWithMultipleVariantsWithSharedProperty(englishContentName, documentTypeId, AliasHelper.toAlias(text2Name), textStringEditorAlias, cultures, '');
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(englishContentName);
  await umbracoUi.content.clickSaveAndPublishButton();
  await umbracoUi.content.clickContainerSaveAndPublishButton();
  await umbracoUi.content.isSuccessNotificationVisible();
  await umbracoUi.content.switchLanguage(secondCulture);

  // Assert
  await umbracoUi.content.isAddBlockListElementWithNameVisible(blockList2Name);
});

test('can edit variant text property inside a variant block in non-default language when AllowEditInvariantFromNonDefault is true', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const textStringDataType = await umbracoApi.dataType.getByName(textStringDataTypeName);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithVariantAndInvariantBlockLists(
    documentTypeName,
    text1Name,
    textStringDataType.id,
    text2Name,
    textStringDataType.id,
    blockList1Name,
    blockList2Name,
    block1ElementTypeName,
    block2ElementTypeName
  );
  await umbracoApi.document.createDocumentWithMultipleVariantsWithSharedProperty(englishContentName, documentTypeId, AliasHelper.toAlias(text2Name), textStringEditorAlias, cultures, '');
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(englishContentName);
  await umbracoUi.content.clickAddBlockListElementWithName(blockList1Name);
  await umbracoUi.content.clickBlockElementWithName(block1ElementTypeName);
  await umbracoUi.content.enterBlockPropertyValue(text1Name, 'Variant text in block');
  await umbracoUi.content.enterBlockPropertyValue(text2Name, 'Invariant text in block');
  await umbracoUi.content.clickCreateModalButton();
  await umbracoUi.content.clickSaveAndPublishButton();
  await umbracoUi.content.clickContainerSaveAndPublishButton();
  await umbracoUi.content.isSuccessNotificationVisible();
  await umbracoUi.content.switchLanguage(secondCulture);
  await umbracoUi.content.clickAddBlockListElementWithName(blockList1Name);
  await umbracoUi.content.clickBlockElementWithName(block1ElementTypeName);

  // Assert
  await umbracoUi.content.isBlockPropertyEditable(text1Name, true);
});

test('can edit invariant text property inside a variant block in non-default language when AllowEditInvariantFromNonDefault is true', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const textStringDataType = await umbracoApi.dataType.getByName(textStringDataTypeName);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithVariantAndInvariantBlockLists(
    documentTypeName,
    text1Name,
    textStringDataType.id,
    text2Name,
    textStringDataType.id,
    blockList1Name,
    blockList2Name,
    block1ElementTypeName,
    block2ElementTypeName
  );
  await umbracoApi.document.createDocumentWithMultipleVariantsWithSharedProperty(englishContentName, documentTypeId, AliasHelper.toAlias(text2Name), textStringEditorAlias, cultures, '');
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(englishContentName);
  await umbracoUi.content.clickAddBlockListElementWithName(blockList1Name);
  await umbracoUi.content.clickBlockElementWithName(block1ElementTypeName);
  await umbracoUi.content.enterBlockPropertyValue(text1Name, 'Variant text in block');
  await umbracoUi.content.enterBlockPropertyValue(text2Name, 'Invariant text in block');
  await umbracoUi.content.clickCreateModalButton();
  await umbracoUi.content.clickSaveAndPublishButton();
  await umbracoUi.content.clickContainerSaveAndPublishButton();
  await umbracoUi.content.isSuccessNotificationVisible();
  await umbracoUi.content.switchLanguage(secondCulture);
  await umbracoUi.content.clickAddBlockListElementWithName(blockList1Name);
  await umbracoUi.content.clickBlockElementWithName(block1ElementTypeName);

  // Assert
  await umbracoUi.content.isBlockPropertyEditable(text2Name, true);
});

// Currently failing due to issue: variant text properties inside invariant blocks not being editable in non-default languages
test.skip('can edit variant text property inside invariant block in non-default language when AllowEditInvariantFromNonDefault is true', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const textStringDataType = await umbracoApi.dataType.getByName(textStringDataTypeName);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithVariantAndInvariantBlockLists(
    documentTypeName,
    text1Name,
    textStringDataType.id,
    text2Name,
    textStringDataType.id,
    blockList1Name,
    blockList2Name,
    block1ElementTypeName,
    block2ElementTypeName
  );
  await umbracoApi.document.createDocumentWithMultipleVariantsWithSharedProperty(englishContentName, documentTypeId, AliasHelper.toAlias(text2Name), textStringEditorAlias, cultures, '');
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(englishContentName);
  await umbracoUi.content.clickAddBlockListElementWithName(blockList1Name);
  await umbracoUi.content.clickBlockElementWithName(block2ElementTypeName);
  await umbracoUi.content.enterBlockPropertyValue(text1Name, 'Text1 in invariant block');
  await umbracoUi.content.enterBlockPropertyValue(text2Name, 'Text2 in invariant block');
  await umbracoUi.content.clickCreateModalButton();
  await umbracoUi.content.clickSaveAndPublishButton();
  await umbracoUi.content.clickContainerSaveAndPublishButton();
  await umbracoUi.content.isSuccessNotificationVisible();
  await umbracoUi.content.switchLanguage(secondCulture);
  await umbracoUi.content.clickAddBlockListElementWithName(blockList1Name);
  await umbracoUi.content.clickBlockElementWithName(block2ElementTypeName);

  // Assert
  await umbracoUi.content.isBlockPropertyEditable(text1Name, true);
});

// Currently failing due to issue: invariant text properties inside invariant blocks not being editable in non-default languages
test.skip('can edit invariant text property inside an invariant block in non-default language when AllowEditInvariantFromNonDefault is true', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const textStringDataType = await umbracoApi.dataType.getByName(textStringDataTypeName);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithVariantAndInvariantBlockLists(
    documentTypeName,
    text1Name,
    textStringDataType.id,
    text2Name,
    textStringDataType.id,
    blockList1Name,
    blockList2Name,
    block1ElementTypeName,
    block2ElementTypeName
  );
  await umbracoApi.document.createDocumentWithMultipleVariantsWithSharedProperty(englishContentName, documentTypeId, AliasHelper.toAlias(text2Name), textStringEditorAlias, cultures, '');
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(englishContentName);
  await umbracoUi.content.clickAddBlockListElementWithName(blockList1Name);
  await umbracoUi.content.clickBlockElementWithName(block2ElementTypeName);
  await umbracoUi.content.enterBlockPropertyValue(text1Name, 'Text1 in invariant block');
  await umbracoUi.content.enterBlockPropertyValue(text2Name, 'Text2 in invariant block');
  await umbracoUi.content.clickCreateModalButton();
  await umbracoUi.content.clickSaveAndPublishButton();
  await umbracoUi.content.clickContainerSaveAndPublishButton();
  await umbracoUi.content.isSuccessNotificationVisible();
  await umbracoUi.content.switchLanguage(secondCulture);
  await umbracoUi.content.clickAddBlockListElementWithName(blockList1Name);
  await umbracoUi.content.clickBlockElementWithName(block2ElementTypeName);

  // Assert
  await umbracoUi.content.isBlockPropertyEditable(text2Name, true);
});
