import { ConstantHelper, test, AliasHelper } from '@umbraco/acceptance-test-helpers';
import {expect} from "@playwright/test";

const dataTypeName = 'Multi URL Picker';
const contentName = 'TestContent';
const documentTypeName = 'TestDocumentTypeForContent';
const link = 'https://docs.umbraco.com';
const linkTitle = 'Umbraco Documentation';
const manualPropertyName = 'Manual';
const anchorOrQuerystringPropertyName = 'Anchor or querystring';
const anchorValue = '#anchor';
const querystringValue = '?param=value';
let dataTypeData: any;
let documentTypeId: any;

test.beforeEach(async ({umbracoApi}) => {
  dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeData.id);
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
});

test('cannot create content with empty manual url and anchor', {tag: '@release'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickAddMultiURLPickerButton();
  await umbracoUi.content.clickManualLinkButton();
  await umbracoUi.content.enterLink('');
  await umbracoUi.content.enterAnchorOrQuerystring('');
  await umbracoUi.content.clickLinkPickerAddButton();

  // Assert
  await umbracoUi.content.doesPropertyWithNameContainValidationMessage(manualPropertyName, ConstantHelper.validationMessages.emptyManualLinkPicker);
  await umbracoUi.content.doesPropertyWithNameContainValidationMessage(anchorOrQuerystringPropertyName, ConstantHelper.validationMessages.emptyManualLinkPicker);
});

test('can see validation message disappear when url is entered', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickAddMultiURLPickerButton();
  await umbracoUi.content.clickManualLinkButton();
  // Leave both fields empty first to trigger validation
  await umbracoUi.content.enterLink('');
  await umbracoUi.content.enterAnchorOrQuerystring('');
  await umbracoUi.content.clickLinkPickerAddButton();
  // Verify validation messages appear
  await umbracoUi.content.doesPropertyWithNameContainValidationMessage(manualPropertyName, ConstantHelper.validationMessages.emptyManualLinkPicker);
  await umbracoUi.content.doesPropertyWithNameContainValidationMessage(anchorOrQuerystringPropertyName, ConstantHelper.validationMessages.emptyManualLinkPicker);
  // Enter URL to clear validation
  await umbracoUi.content.enterLink(link);

  // Assert
  await umbracoUi.content.doesPropertyWithNameContainValidationMessage(manualPropertyName, ConstantHelper.validationMessages.emptyManualLinkPicker, false);
  await umbracoUi.content.doesPropertyWithNameContainValidationMessage(anchorOrQuerystringPropertyName, ConstantHelper.validationMessages.emptyManualLinkPicker, false);
});

test('can see validation message disappear when anchor is entered', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickAddMultiURLPickerButton();
  await umbracoUi.content.clickManualLinkButton();
  // Leave both fields empty first to trigger validation
  await umbracoUi.content.enterLink('');
  await umbracoUi.content.enterAnchorOrQuerystring('');
  await umbracoUi.content.clickLinkPickerAddButton();
  // Verify validation messages appear
  await umbracoUi.content.doesPropertyWithNameContainValidationMessage(manualPropertyName, ConstantHelper.validationMessages.emptyManualLinkPicker);
  await umbracoUi.content.doesPropertyWithNameContainValidationMessage(anchorOrQuerystringPropertyName, ConstantHelper.validationMessages.emptyManualLinkPicker);
  // Enter Anchor to clear validation
  await umbracoUi.content.enterAnchorOrQuerystring(anchorValue);

  // Assert
  await umbracoUi.content.doesPropertyWithNameContainValidationMessage(manualPropertyName, ConstantHelper.validationMessages.emptyManualLinkPicker, false);
  await umbracoUi.content.doesPropertyWithNameContainValidationMessage(anchorOrQuerystringPropertyName, ConstantHelper.validationMessages.emptyManualLinkPicker, false);
});

test('can create content with manual url only', {tag: '@release'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickAddMultiURLPickerButton();
  await umbracoUi.content.clickManualLinkButton();
  await umbracoUi.content.enterLink(link);
  await umbracoUi.content.clickLinkPickerAddButton();
  await umbracoUi.content.clickSaveButtonAndWaitForContentToBeUpdated();

  // Assert
  expect(await umbracoApi.document.doesNameExist(contentName)).toBeTruthy();
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.values[0].alias).toEqual(AliasHelper.toAlias(dataTypeName));
  expect(contentData.values[0].value.length).toBe(1);
  expect(contentData.values[0].value[0].type).toEqual('external');
  expect(contentData.values[0].value[0].url).toEqual(link);
});

test('can create content with manual anchor only', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickAddMultiURLPickerButton();
  await umbracoUi.content.clickManualLinkButton();
  await umbracoUi.content.enterAnchorOrQuerystring(anchorValue);
  await umbracoUi.content.clickLinkPickerAddButton();
  await umbracoUi.content.clickSaveButtonAndWaitForContentToBeUpdated();

  // Assert
  expect(await umbracoApi.document.doesNameExist(contentName)).toBeTruthy();
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.values[0].alias).toEqual(AliasHelper.toAlias(dataTypeName));
  expect(contentData.values[0].value.length).toBe(1);
  expect(contentData.values[0].value[0].queryString).toEqual(anchorValue);
});

test('can create content with manual url and anchor', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickAddMultiURLPickerButton();
  await umbracoUi.content.clickManualLinkButton();
  await umbracoUi.content.enterLink(link);
  await umbracoUi.content.enterAnchorOrQuerystring(querystringValue);
  await umbracoUi.content.clickLinkPickerAddButton();
  await umbracoUi.content.clickSaveButtonAndWaitForContentToBeUpdated();

  // Assert
  expect(await umbracoApi.document.doesNameExist(contentName)).toBeTruthy();
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.values[0].alias).toEqual(AliasHelper.toAlias(dataTypeName));
  expect(contentData.values[0].value.length).toBe(1);
  expect(contentData.values[0].value[0].type).toEqual('external');
  expect(contentData.values[0].value[0].url).toEqual(link);
  expect(contentData.values[0].value[0].queryString).toEqual(querystringValue);
});

test('cannot update content with empty manual url and anchor', {tag: '@release'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.document.createDocumentWithExternalLinkURLPicker(contentName, documentTypeId, dataTypeName, link, linkTitle);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickLinkWithName(linkTitle);
  await umbracoUi.content.enterLink('');
  await umbracoUi.content.enterAnchorOrQuerystring('');
  await umbracoUi.content.clickUpdateButton();

  // Assert
  await umbracoUi.content.doesPropertyWithNameContainValidationMessage(manualPropertyName, ConstantHelper.validationMessages.emptyManualLinkPicker);
  await umbracoUi.content.doesPropertyWithNameContainValidationMessage(anchorOrQuerystringPropertyName, ConstantHelper.validationMessages.emptyManualLinkPicker);
});
