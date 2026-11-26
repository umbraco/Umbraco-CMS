import {ConstantHelper, test, AliasHelper} from '@umbraco/acceptance-test-helpers';
import {expect} from "@playwright/test";

const contentName = 'TestContent';
const documentTypeName = 'TestDocumentTypeForContent';
const compositionDocumentTypeName = 'VariantCompositionDocumentType';
const dataTypeName = 'Textstring';
const groupName = 'TestGroup';
const danishIsoCode = 'da';
let compositionDocumentTypeId = null;

test.beforeEach(async ({umbracoApi, umbracoUi}) => {
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.documentType.ensureNameNotExists(compositionDocumentTypeName);
  await umbracoUi.goToBackOffice();
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  compositionDocumentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(compositionDocumentTypeName, dataTypeName, dataTypeData.id, groupName, true, true);
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.documentType.ensureNameNotExists(compositionDocumentTypeName);
  await umbracoApi.language.ensureIsoCodeNotExists(danishIsoCode);
});

test('can create content with an invariant document type that has a variant composition', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const expectedState = 'Draft';
  await umbracoApi.documentType.createDocumentTypeWithACompositionAndAllowAsRoot(documentTypeName, compositionDocumentTypeId);
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.clickActionsMenuAtRoot();
  await umbracoUi.content.clickCreateActionMenuOption();
  await umbracoUi.content.chooseDocumentType(documentTypeName);
  await umbracoUi.content.enterContentName(contentName);
  await umbracoUi.content.clickSaveButtonAndWaitForContentToBeCreated();

  // Assert
  expect(await umbracoApi.document.doesNameExist(contentName)).toBeTruthy();
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.variants[0].state).toBe(expectedState);
});

test('can save property value from variant composition in invariant content', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const text = 'This is a property value from variant composition';
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithACompositionAndAllowAsRoot(documentTypeName, compositionDocumentTypeId);
  await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.enterTextstring(text);
  await umbracoUi.content.clickSaveButtonAndWaitForContentToBeUpdated();

  // Assert
  expect(await umbracoApi.document.doesNameExist(contentName)).toBeTruthy();
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.values[0].alias).toEqual(AliasHelper.toAlias(dataTypeName));
  expect(contentData.values[0].value).toEqual(text);
});

test('can save property values from variant composition in variant content with multiple cultures', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const englishText = 'English value from composition';
  const danishText = 'Danish value from composition';
  await umbracoApi.language.createDanishLanguage();
  const documentTypeId = await umbracoApi.documentType.createVariantDocumentTypeWithACompositionAndAllowAsRoot(documentTypeName, compositionDocumentTypeId);
  await umbracoApi.document.createDocumentWithMultipleVariants(
    contentName, documentTypeId, AliasHelper.toAlias(dataTypeName),
    [
      {isoCode: 'en-US', name: contentName, value: ''},
      {isoCode: danishIsoCode, name: contentName + danishIsoCode, value: ''},
    ]
  );
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.enterTextstring(englishText);
  await umbracoUi.content.clickSaveButtonForContent();
  await umbracoUi.content.clickContainerSaveButtonAndWaitForContentToBeUpdated();
  await umbracoUi.content.switchLanguage('Danish');
  await umbracoUi.content.enterTextstring(danishText);
  await umbracoUi.content.clickSaveButtonForContent();
  await umbracoUi.content.clickContainerSaveButtonAndWaitForContentToBeUpdated();

  // Assert
  const [englishValue, danishValue] = await umbracoApi.document.getValuesByAliasAndCultures(contentName, AliasHelper.toAlias(dataTypeName), ['en-US', danishIsoCode]);
  expect(englishValue.value).toEqual(englishText);
  expect(danishValue.value).toEqual(danishText);
});
