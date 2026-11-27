import {AliasHelper, ConstantHelper, test} from '@umbraco/acceptance-test-helpers';
import {expect} from "@playwright/test";

const dataTypeName = 'Multi URL Picker';
const contentName = 'TestContent';
const danishContentName = 'TestContentDa';
const documentTypeName = 'TestDocumentTypeForContent';
const collectionContentName = 'Link Picker Collection Content';
const collectionDocumentTypeName = 'LinkPickerCollectionDocumentType';
const collectionItemDocumentTypeName = 'LinkPickerCollectionItemDocumentType';
const listViewDataTypeName = 'List View - Content';
const englishIsoCode = 'en-US';
const danishIsoCode = 'da';

test.beforeEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.document.ensureNameNotExists(collectionContentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.documentType.ensureNameNotExists(collectionDocumentTypeName);
  await umbracoApi.documentType.ensureNameNotExists(collectionItemDocumentTypeName);
  await umbracoApi.language.createDanishLanguage();
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.document.ensureNameNotExists(collectionContentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.documentType.ensureNameNotExists(collectionDocumentTypeName);
  await umbracoApi.documentType.ensureNameNotExists(collectionItemDocumentTypeName);
  await umbracoApi.language.ensureIsoCodeNotExists(danishIsoCode);
});

test('can filter a collection in the document link picker by names in the current culture', {tag: '@release'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const firstItemDanishName = 'Dansk Aeble';
  const secondItemDanishName = 'Dansk Paere';
  const collectionItemDocumentTypeId = await umbracoApi.documentType.createDocumentTypeWithAllowVaryByCulture(collectionItemDocumentTypeName);
  const listViewDataTypeData = await umbracoApi.dataType.getByName(listViewDataTypeName);
  const collectionDocumentTypeId = await umbracoApi.documentType.createDocumentTypeWithAllowedChildNodeAndCollectionId(collectionDocumentTypeName, collectionItemDocumentTypeId, listViewDataTypeData.id);
  const collectionContentId = await umbracoApi.document.createDefaultDocument(collectionContentName, collectionDocumentTypeId);
  const firstItemId = await umbracoApi.document.createDocumentWithCultureNames(collectionItemDocumentTypeId, [
    {isoCode: englishIsoCode, name: 'English Apple'},
    {isoCode: danishIsoCode, name: firstItemDanishName},
  ], collectionContentId);
  await umbracoApi.document.createDocumentWithCultureNames(collectionItemDocumentTypeId, [
    {isoCode: englishIsoCode, name: 'English Pear'},
    {isoCode: danishIsoCode, name: secondItemDanishName},
  ], collectionContentId);
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeData.id, 'TestGroup', true, true);
  await umbracoApi.document.createDocumentWithCultureNames(documentTypeId, [
    {isoCode: englishIsoCode, name: contentName},
    {isoCode: danishIsoCode, name: danishContentName},
  ]);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.switchLanguage('Danish');
  await umbracoUi.content.clickAddMultiURLPickerButton();
  await umbracoUi.content.clickDocumentLinkButton();
  // The caret drills into the node (its name would select it instead).
  await umbracoUi.content.openCaretButtonForName(collectionContentName, true);
  await umbracoUi.content.enterFilterKeywordInLinkPickerCollection('Aeble');
  await umbracoUi.content.isCollectionCardInLinkPickerModalVisible(secondItemDanishName, false);
  await umbracoUi.content.clickCollectionCardInLinkPickerModal(firstItemDanishName);
  await umbracoUi.content.clickChooseModalButton();
  await umbracoUi.content.clickLinkPickerAddButton();
  await umbracoUi.content.clickSaveButtonForContent();
  await umbracoUi.content.clickContainerSaveButtonAndWaitForContentToBeUpdated();

  // Assert
  const [danishValue] = await umbracoApi.document.getValuesByAliasAndCultures(contentName, AliasHelper.toAlias(dataTypeName), [danishIsoCode]);
  expect(danishValue.value.length).toBe(1);
  expect(danishValue.value[0].type).toEqual('document');
  expect(danishValue.value[0].unique).toEqual(firstItemId);
});
