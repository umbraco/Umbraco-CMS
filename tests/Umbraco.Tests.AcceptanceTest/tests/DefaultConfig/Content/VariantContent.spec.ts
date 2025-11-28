import {ConstantHelper, test, AliasHelper} from '@umbraco/playwright-testhelpers';
import {expect} from '@playwright/test';

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
  await umbracoApi.document.ensureNameNotExists(englishContentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.language.ensureIsoCodeNotExists(secondCulture);
});

// TODO: Remove .skip when issue #20633 is resolved
// Issue link: https://github.com/umbraco/Umbraco-CMS/issues/20633
test.skip('allow edit invariant property editor value in other languages than default one', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const textStringDataType = await umbracoApi.dataType.getByName(textStringDataTypeName);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, textStringDataTypeName, textStringDataType.id, 'TestGroup', true, false);
  await umbracoApi.document.createDocumentWithMultipleVariantsWithSharedProperty(englishContentName, documentTypeId, AliasHelper.toAlias(textStringDataTypeName), 'Umbraco.Textstring', cultures, '');
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(englishContentName);
  await umbracoUi.content.switchLanguage(secondCulture);
  await umbracoUi.content.enterTextstring(contentText);
  await umbracoUi.content.clickSaveAndPublishButton();
  await umbracoUi.content.clickContainerSaveAndPublishButton();
  await umbracoUi.content.isSuccessNotificationVisible();

  // Assert
  await umbracoUi.content.doesPropertyContainValue(AliasHelper.toAlias(textStringDataTypeName), contentText);
  const contentData = await umbracoApi.document.getByName(englishContentName);
  expect(contentData.variants.length).toBe(2);
  expect(contentData.values.length).toBe(1);
  expect(contentData.values[0].alias).toEqual(AliasHelper.toAlias(textStringDataTypeName));
  expect(contentData.values[0].value).toEqual(contentText);
});
