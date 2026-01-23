import {ConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

// Content
const contentName = 'TestContent';
// DocumentType
const documentTypeName = 'TestDocumentTypeForContent';
// DataType
const dataTypeName = 'Textstring';
// Initial Preset Value
const initialPresetValue = 'Preset value';
const varyByCultureText = ' varies by culture:';
const varyBySegmentText = ' varies by segment:';
// Segments
const vipMemberSegment = 'VIP members';
const vipMemberSegmentAlias = 'vip-members';
// Language
let languageData: any = null;

test.beforeEach(async ({ umbracoApi, umbracoUi }) => {
    const languageId = await umbracoApi.language.createDanishLanguage();
    languageData = await umbracoApi.language.get(languageId);
    const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
    const documentTypeVaryByCulture = true;
    const propertyVaryByCulture = true;
    const documentTypeVaryBySegment = true;
    const propertyVaryBySegment = true;
    await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeData.id, 'Test group', documentTypeVaryByCulture, propertyVaryByCulture, false, documentTypeVaryBySegment, propertyVaryBySegment);
    await umbracoUi.goToBackOffice();
    await umbracoUi.content.goToSection(ConstantHelper.sections.content);
});

test.afterEach(async ({ umbracoApi }) => {
    await umbracoApi.document.ensureNameNotExists(contentName);
    await umbracoApi.document.ensureNameNotExists(vipMemberSegment);
    await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
});

test('can insert preset value into textstring property that vary by culture and segment in default language', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const presetValue = initialPresetValue + varyByCultureText + ' en-US' + varyBySegmentText + ' default';

  // Act
  await umbracoUi.content.clickActionsMenuAtRoot();
  await umbracoUi.content.clickCreateActionMenuOption();
  await umbracoUi.content.chooseDocumentType(documentTypeName);
  await umbracoUi.content.enterContentName(contentName);
  await umbracoUi.content.doesTextStringHaveExpectedValue(presetValue);
  await umbracoUi.content.clickSaveButtonForContent();
  await umbracoUi.content.clickSaveModalButtonAndWaitForContentToBeCreated();

  // Assert
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.values[0].value).toBe(presetValue);
});

test('can insert preset value into textstring property that vary by culture and segment in segment of default language', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const presetValue = initialPresetValue + varyByCultureText + ' en-US' + varyBySegmentText + ' '+ vipMemberSegmentAlias;

  // Act
  await umbracoUi.content.clickActionsMenuAtRoot();
  await umbracoUi.content.clickCreateActionMenuOption();
  await umbracoUi.content.chooseDocumentType(documentTypeName);
  await umbracoUi.content.enterContentName(contentName);
  await umbracoUi.content.clickSaveButtonForContent();
  await umbracoUi.content.clickSaveModalButtonAndWaitForContentToBeCreated();
  await umbracoUi.content.clickSelectVariantButton();
  await umbracoUi.content.clickExpendSegmentButton(contentName);
  await umbracoUi.content.clickVariantAddModeButtonForLanguageName(vipMemberSegment);

  // Assert
  await umbracoUi.content.doesTextStringHaveExpectedValue(presetValue);
  await umbracoUi.content.clickSaveButtonForContent();
  await umbracoUi.content.clickSaveModalButtonAndWaitForContentToBeUpdated();
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.values.find(item => item.culture === 'en-US' && item.segment === vipMemberSegmentAlias).value).toBe(presetValue);
});

test('can insert preset value into textstring property that vary by culture and segment in segment of second language', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const presetValue = initialPresetValue + varyByCultureText + ' '+ languageData.isoCode + varyBySegmentText + ' '+ vipMemberSegmentAlias;

  // Act
  await umbracoUi.content.clickActionsMenuAtRoot();
  await umbracoUi.content.clickCreateActionMenuOption();
  await umbracoUi.content.chooseDocumentType(documentTypeName);
  await umbracoUi.content.clickSelectVariantButton();
  await umbracoUi.content.clickVariantAddModeButtonForLanguageName(languageData.name);
  await umbracoUi.content.enterContentName(contentName);
  await umbracoUi.content.clickSaveButtonForContent();
  await umbracoUi.content.clickSaveModalButtonAndWaitForContentToBeCreated();
  await umbracoUi.content.clickSelectVariantButton();
  await umbracoUi.content.clickExpendSegmentButton(contentName);
  await umbracoUi.content.clickVariantAddModeButtonForLanguageName(vipMemberSegment);

  // Assert
  await umbracoUi.content.doesTextStringHaveExpectedValue(presetValue);
  await umbracoUi.content.clickSaveButtonForContent();
  await umbracoUi.content.clickSaveModalButtonAndWaitForContentToBeUpdated();
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.values.find(item => item.culture === languageData.isoCode && item.segment === vipMemberSegmentAlias).value).toBe(presetValue);
});