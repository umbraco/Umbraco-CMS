import {ConstantHelper, test} from '@umbraco/acceptance-test-helpers';
import {expect} from "@playwright/test";

// Content
const contentName = 'TestSegmentContent';
// DocumentType
const documentTypeName = 'TestSegmentDocType';
let documentTypeId = '';
// DataType
const dataTypeName = 'Textstring';
const editorAlias = 'Umbraco.TextBox';
// Segments
const vipMemberSegment = 'VIP members';
const vipMemberSegmentAlias = 'vip-members';
// Test Values
const defaultSegmentValue = 'Default segment value';
const vipSegmentValue = 'VIP segment value';
const updatedVipSegmentValue = 'Updated VIP segment value';

test.describe('Content with culture and segment variations', () => {
  test.beforeEach(async ({umbracoApi, umbracoUi}) => {
    await umbracoApi.document.ensureNameNotExists(contentName);
    await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
    await umbracoApi.language.createDanishLanguage();
    const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
    documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeData.id, 'TestGroup', true, true, false, true, true);
    await umbracoUi.goToBackOffice();
    await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  });

  test.afterEach(async ({umbracoApi}) => {
    await umbracoApi.document.ensureNameNotExists(contentName);
    await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
    await umbracoApi.language.ensureNameNotExists('Danish');
  });

  test('can create content with a property value in the default segment', async ({umbracoApi, umbracoUi}) => {
    // Act
    await umbracoUi.content.clickActionsMenuAtRoot();
    await umbracoUi.content.clickCreateActionMenuOption();
    await umbracoUi.content.chooseDocumentType(documentTypeName);
    await umbracoUi.content.enterContentName(contentName);
    await umbracoUi.content.enterTextstring(defaultSegmentValue);
    await umbracoUi.content.clickSaveButtonForContent();
    await umbracoUi.content.clickSaveModalButtonAndWaitForContentToBeCreated();

    // Assert
    const contentData = await umbracoApi.document.getByName(contentName);
    expect(contentData.values[0].culture).toBe('en-US');
    expect(contentData.values[0].segment).toBeNull();
    expect(contentData.values[0].value).toBe(defaultSegmentValue);
  });

  test('can save VIP segment-specific property value', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    await umbracoApi.document.createDocumentWithEnglishCultureAndTextContent(contentName, documentTypeId, defaultSegmentValue, dataTypeName, true);
    await umbracoUi.content.goToContentWithName(contentName);

    // Act
    await umbracoUi.content.clickSelectVariantButton();
    await umbracoUi.content.clickExpandSegmentButton('English');
    await umbracoUi.content.clickVariantAddModeButtonForLanguageName(vipMemberSegment);
    await umbracoUi.content.enterTextstring(vipSegmentValue);
    await umbracoUi.content.clickSaveButtonForContent();
    await umbracoUi.content.clickSaveModalButtonAndWaitForContentToBeUpdated();

    // Assert
    const contentData = await umbracoApi.document.getByName(contentName);
    const defaultValue = contentData.values.find(v => v.culture === 'en-US' && v.segment === null);
    const vipValue = contentData.values.find(v => v.culture === 'en-US' && v.segment === vipMemberSegmentAlias);
    expect(defaultValue).toBeTruthy();
    expect(defaultValue.value).toBe(defaultSegmentValue);
    expect(vipValue).toBeTruthy();
    expect(vipValue.value).toBe(vipSegmentValue);
  });

  test('can update existing VIP segment value', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    await umbracoApi.document.createDocumentWithMultipleCulturesAndSegmentValues(contentName, documentTypeId, dataTypeName, editorAlias, ['en-US'], [{
      value: defaultSegmentValue,
      culture: 'en-US',
      segment: null
    }, {value: vipSegmentValue, culture: 'en-US', segment: vipMemberSegmentAlias},]);
    await umbracoUi.content.goToContentWithName(contentName);
    await umbracoUi.content.clickSelectVariantButton();
    await umbracoUi.content.clickExpandSegmentButton('English');
    await umbracoUi.content.clickSegmentVariantButton(vipMemberSegment);

    // Act
    await umbracoUi.content.enterTextstring(updatedVipSegmentValue);
    await umbracoUi.content.clickSaveButtonForContent();
    await umbracoUi.content.clickSaveModalButtonAndWaitForContentToBeUpdated();

    // Assert
    const contentData = await umbracoApi.document.getByName(contentName);
    const vipValue = contentData.values.find(v => v.culture === 'en-US' && v.segment === vipMemberSegmentAlias);
    expect(vipValue).toBeTruthy();
    expect(vipValue.value).toBe(updatedVipSegmentValue);
  });

  test('can save VIP segment value in second language', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    await umbracoApi.document.createDocumentWithMultipleCulturesAndSegmentValues(
      contentName,
      documentTypeId,
      dataTypeName,
      editorAlias,
      ['en-US', 'da'],
      [
        {value: 'English default', culture: 'en-US', segment: null},
        {value: 'English VIP', culture: 'en-US', segment: vipMemberSegmentAlias},
        {value: 'Danish default', culture: 'da', segment: null},
      ]
    );
    await umbracoUi.content.goToContentWithName(contentName);

    // Act
    await umbracoUi.content.clickSelectVariantButton();
    await umbracoUi.content.clickExpandSegmentButton('Danish');
    await umbracoUi.content.clickSegmentVariantButton(vipMemberSegment);
    await umbracoUi.content.enterTextstring('Danish VIP');
    await umbracoUi.content.clickSaveButtonForContent();
    await umbracoUi.content.clickSaveModalButtonAndWaitForContentToBeUpdated();

    // Assert
    const contentData = await umbracoApi.document.getByName(contentName);
    const enDefault = contentData.values.find(v => v.culture === 'en-US' && v.segment === null);
    const enVip = contentData.values.find(v => v.culture === 'en-US' && v.segment === vipMemberSegmentAlias);
    const daDefault = contentData.values.find(v => v.culture === 'da' && v.segment === null);
    const daVip = contentData.values.find(v => v.culture === 'da' && v.segment === vipMemberSegmentAlias);
    expect(enDefault).toBeTruthy();
    expect(enDefault.value).toBe('English default');
    expect(enVip).toBeTruthy();
    expect(enVip.value).toBe('English VIP');
    expect(daDefault).toBeTruthy();
    expect(daDefault.value).toBe('Danish default');
    expect(daVip).toBeTruthy();
    expect(daVip.value).toBe('Danish VIP');
  });
});

test.describe('Content with segment-only variations', () => {
  test.beforeEach(async ({umbracoApi, umbracoUi}) => {
    await umbracoApi.document.ensureNameNotExists(contentName);
    await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
    const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
    documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeData.id, 'TestGroup', false, false, false, true, true);
    await umbracoUi.goToBackOffice();
    await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  });

  test.afterEach(async ({umbracoApi}) => {
    await umbracoApi.document.ensureNameNotExists(contentName);
    await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  });

  test('can create content with segment-only variation', async ({umbracoApi, umbracoUi}) => {
    // Act
    await umbracoUi.content.clickActionsMenuAtRoot();
    await umbracoUi.content.clickCreateActionMenuOption();
    await umbracoUi.content.chooseDocumentType(documentTypeName);
    await umbracoUi.content.enterContentName(contentName);
    await umbracoUi.content.enterTextstring(defaultSegmentValue);
    await umbracoUi.content.clickSaveButtonAndWaitForContentToBeCreated();

    // Assert
    const contentData = await umbracoApi.document.getByName(contentName);
    expect(contentData.values[0].culture).toBeNull();
    expect(contentData.values[0].segment).toBeNull();
    expect(contentData.values[0].value).toBe(defaultSegmentValue);
  });

  test('can add VIP segment value to segment-only content', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    await umbracoApi.document.createDocumentWithTextContent(contentName, documentTypeId, defaultSegmentValue, dataTypeName);
    await umbracoUi.content.goToContentWithName(contentName);

    // Act
    await umbracoUi.content.clickSelectVariantButton();
    await umbracoUi.content.clickVariantAddModeButtonForLanguageName(vipMemberSegment);
    await umbracoUi.content.enterTextstring(vipSegmentValue);
    await umbracoUi.content.clickSaveButtonAndWaitForContentToBeUpdated();

    // Assert
    const contentData = await umbracoApi.document.getByName(contentName);
    const defaultValue = contentData.values.find(v => v.segment === null);
    const vipValue = contentData.values.find(v => v.segment === vipMemberSegmentAlias);
    expect(defaultValue).toBeTruthy();
    expect(defaultValue.value).toBe(defaultSegmentValue);
    expect(vipValue).toBeTruthy();
    expect(vipValue.value).toBe(vipSegmentValue);
  });

  test('can update VIP segment value on segment-only content', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    await umbracoApi.document.createDocumentWithMultipleSegmentValues(
      contentName,
      documentTypeId,
      dataTypeName,
      editorAlias,
      [
        {value: defaultSegmentValue, segment: null},
        {value: vipSegmentValue, segment: vipMemberSegmentAlias},
      ]
    );
    await umbracoUi.content.goToContentWithName(contentName);
    await umbracoUi.content.clickSelectVariantButton();
    await umbracoUi.content.clickSegmentVariantButton(vipMemberSegment);

    // Act
    await umbracoUi.content.enterTextstring(updatedVipSegmentValue);
    await umbracoUi.content.clickSaveButtonAndWaitForContentToBeUpdated();

    // Assert
    const contentData = await umbracoApi.document.getByName(contentName);
    const vipValue = contentData.values.find(v => v.segment === vipMemberSegmentAlias);
    expect(vipValue).toBeTruthy();
    expect(vipValue.value).toBe(updatedVipSegmentValue);
  });
});
