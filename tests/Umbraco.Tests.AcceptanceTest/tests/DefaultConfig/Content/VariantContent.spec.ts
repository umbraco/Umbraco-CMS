import {ConstantHelper, test, AliasHelper} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

// Content
const englishContentName = 'English Content';
const danishContentName = 'Danish Content';
const contentText = 'This is test content text';
// Document Type
const documentTypeName = 'TestDocumentTypeForContent';
// Data Type
const textStringDataTypeName = 'Textstring';
// Languages
const firstCulture = 'en-US';
const secondCulture = 'da';
// Cultures
const cultures = [
  { isoCode: firstCulture, name: englishContentName },
  { isoCode: secondCulture, name: danishContentName },
];

test.beforeEach(async ({umbracoApi}) => {
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.language.createDanishLanguage();
});

test.afterEach(async ({umbracoApi}) => {
  // await umbracoApi.document.ensureNameNotExists(contentName);
  // await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  // await umbracoApi.language.ensureIsoCodeNotExists(secondCulture);
});

// This is a test for the regression issue #20250
test('cannot edit non-variant property editor value in other languages than default one', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const textStringDataType = await umbracoApi.dataType.getByName(textStringDataTypeName);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, textStringDataTypeName, textStringDataType.id, 'TestGroup', true, false);
  await umbracoApi.document.createDocumentWithMultipleVariantsWithSharedProperty(englishContentName, documentTypeId, AliasHelper.toAlias(textStringDataTypeName), 'Umbraco.Textstring', cultures, '');
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(englishContentName);

  // Assert


});
