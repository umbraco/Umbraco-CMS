import {ConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

// Content
const contentName = 'TestContent';
const secondContentName = 'Test indhold';
const textContent = 'Some text content';
// DocumentType
const documentTypeName = 'TestDocumentTypeForContent';
// DataType
const dataTypeName = 'Textstring';
const textAreaDataTypeName = 'Textarea';
// Initial Preset Value
const initialPresetValue = 'Preset value';
const initialVariantPresetValue = 'Preset value varies by culture:';
const varyBySegmentText = ' varies by segment:';
const initialAreaPresetValue = 'Preset value for property editor schema alias';
// Second language
const secondLanguageName = 'Danish';
// Segments
const vipMemberSegment = 'VIP members';
const vipMemberSegmentAlias = 'vip-members';

test.afterEach(async ({ umbracoApi }) => {
    await umbracoApi.document.ensureNameNotExists(contentName);
    await umbracoApi.document.ensureNameNotExists(secondContentName);
    await umbracoApi.document.ensureNameNotExists(vipMemberSegment);
    await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
    await umbracoApi.language.ensureNameNotExists(secondLanguageName);
});

test('can insert specific preset values into text box and text area properties', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  const areaDataType = await umbracoApi.dataType.getByName(textAreaDataTypeName);
  await umbracoApi.documentType.createDocumentTypeWithTwoPropertyEditors(documentTypeName, dataTypeName, dataTypeData.id, textAreaDataTypeName, areaDataType.id, 'test group');
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.clickActionsMenuAtRoot();
  await umbracoUi.content.clickCreateActionMenuOption();
  await umbracoUi.content.chooseDocumentType(documentTypeName);
  await umbracoUi.content.enterContentName(contentName);

  // Assert
  await umbracoUi.content.doesTextStringHaveExpectedValue(initialPresetValue);
  await umbracoUi.content.doesTextAreaHaveExpectedValue(initialAreaPresetValue);
  //await umbracoUi.content.clickSaveButton();
  await umbracoUi.content.clickSaveButtonAndWaitForContentToBeCreated();
  await umbracoUi.waitForTimeout(3000);
  expect(await umbracoApi.document.doesNameExist(contentName)).toBeTruthy();
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.values.find(item => item.editorAlias === dataTypeData.editorAlias).value).toBe(initialPresetValue);
  expect(contentData.values.find(item => item.editorAlias === areaDataType.editorAlias).value).toBe(initialAreaPresetValue);
});

test('can insert preset value into textstring property that vary by culture in second language', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const languageId = await umbracoApi.language.createDanishLanguage();
  const languageData = await umbracoApi.language.get(languageId);
  const variantPresetValue = initialVariantPresetValue + ' ' + languageData.isoCode;
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeData.id, 'Test group', true, true);
  await umbracoApi.document.createDocumentWithEnglishCultureAndTextContent(contentName, documentTypeId, textContent, dataTypeName, true);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickSelectVariantButton();
  await umbracoUi.content.clickVariantAddModeButtonForLanguageName(secondLanguageName);
  await umbracoUi.content.enterContentName(secondContentName);
  
  // Assert
  await umbracoUi.content.doesTextStringHaveExpectedValue(variantPresetValue);
  await umbracoUi.content.clickSaveButtonForContent();
  //await umbracoUi.content.clickSaveButton();
  await umbracoUi.content.clickSaveModalButtonAndWaitForContentToBeCreated();
  await umbracoUi.waitForTimeout(3000);
  expect(await umbracoApi.document.doesNameExist(secondContentName)).toBeTruthy();
  const contentData = await umbracoApi.document.getByName(secondContentName);
  expect(contentData.values.find(item=>item.culture === languageData.isoCode).value).toBe(variantPresetValue);
});

test('can insert preset value into textstring property that shared across segments in content that is enabled vary by segment', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  const documentTypeVaryByCulture = false;
  const propertyVaryByCulture = false;
  const documentTypeVaryBySegment = true;
  const propertyVaryBySegment = false;
  await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeData.id, 'Test group', documentTypeVaryByCulture, propertyVaryByCulture, false, documentTypeVaryBySegment, propertyVaryBySegment);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.clickActionsMenuAtRoot();
  await umbracoUi.content.clickCreateActionMenuOption();
  await umbracoUi.content.chooseDocumentType(documentTypeName);
  await umbracoUi.content.enterContentName(contentName);
  await umbracoUi.content.clickSaveButtonAndWaitForContentToBeCreated();
  await umbracoUi.waitForTimeout(3000);
  await umbracoUi.content.clickSelectVariantButton();
  await umbracoUi.content.clickVariantAddModeButtonForLanguageName(vipMemberSegment);
  await umbracoUi.content.clickSaveButtonAndWaitForContentToBeUpdated();
  await umbracoUi.waitForTimeout(3000);

  // Assert
  await umbracoUi.content.doesTextStringHaveExpectedValue(initialPresetValue);
});

test('can insert preset value in textstring property that vary by segment in content that is enabled vary by segment', async ({umbracoApi, umbracoUi}) => {  
  // Arrange
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  const documentTypeVaryByCulture = false;
  const propertyVaryByCulture = false;
  const documentTypeVaryBySegment = true;
  const propertyVaryBySegment = true;
  const expectedString = initialPresetValue + varyBySegmentText + ' ' + vipMemberSegmentAlias;
  await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeData.id, 'Test group', documentTypeVaryByCulture, propertyVaryByCulture, false, documentTypeVaryBySegment, propertyVaryBySegment);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.clickActionsMenuAtRoot();
  await umbracoUi.content.clickCreateActionMenuOption();
  await umbracoUi.content.chooseDocumentType(documentTypeName);
  await umbracoUi.content.enterContentName(contentName);
  //await umbracoUi.content.clickSaveButton();
  await umbracoUi.content.clickSaveButtonAndWaitForContentToBeCreated();
  await umbracoUi.waitForTimeout(3000);
  await umbracoUi.content.clickSelectVariantButton();
  await umbracoUi.content.clickVariantAddModeButtonForLanguageName(vipMemberSegment);
  // Assert
  await umbracoUi.content.doesTextStringHaveExpectedValue(expectedString);
  //await umbracoUi.content.clickSaveButton();
  await umbracoUi.content.clickSaveButtonAndWaitForContentToBeUpdated();
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.values.find(item => item.segment === vipMemberSegmentAlias).value).toBe(expectedString);
});
