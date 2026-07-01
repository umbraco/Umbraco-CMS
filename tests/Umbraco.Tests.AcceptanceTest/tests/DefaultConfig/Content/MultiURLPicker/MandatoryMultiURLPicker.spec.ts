import { ConstantHelper, test, AliasHelper } from '@umbraco/acceptance-test-helpers';
import {expect} from "@playwright/test";

const mandatoryDataTypeName = 'MandatoryMultiUrlPicker';
const contentName = 'TestContent';
const documentTypeName = 'TestDocumentTypeForContent';
const link = 'https://docs.umbraco.com';
const linkTitle = 'Umbraco Documentation';
const secondLink = 'https://umbraco.com';
const secondLinkTitle = 'Umbraco';
let mandatoryDataTypeId: any;
let mandatoryDocumentTypeId: any;

test.beforeEach(async ({umbracoApi, umbracoUi}) => {
  mandatoryDataTypeId = await umbracoApi.dataType.createDefaultMultiUrlPickerDataType(mandatoryDataTypeName);
  mandatoryDocumentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, mandatoryDataTypeName, mandatoryDataTypeId, 'TestGroup', false, false, true);
  await umbracoUi.goToBackOffice();
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.dataType.ensureNameNotExists(mandatoryDataTypeName);
});

test('can save content with mandatory multi url picker after adding a link', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.document.createDefaultDocument(contentName, mandatoryDocumentTypeId);
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickSaveAndPublishButton();
  await umbracoUi.content.isValidationMessageVisible(ConstantHelper.validationMessages.nullValue);
  // Add a manual link
  await umbracoUi.content.clickAddMultiURLPickerButton();
  await umbracoUi.content.clickManualLinkButton();
  await umbracoUi.content.enterLink(link);
  await umbracoUi.content.enterLinkTitle(linkTitle);
  await umbracoUi.content.clickLinkPickerAddButton();
  await umbracoUi.content.clickSaveButtonAndWaitForContentToBeUpdated();

  // Assert
  await umbracoUi.content.isValidationMessageVisible(ConstantHelper.validationMessages.nullValue, false);
  expect(await umbracoApi.document.doesNameExist(contentName)).toBeTruthy();
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.values[0].alias).toEqual(AliasHelper.toAlias(mandatoryDataTypeName));
  expect(contentData.values[0].value.length).toBe(1);
  expect(contentData.values[0].value[0].type).toEqual('external');
  expect(contentData.values[0].value[0].icon).toEqual('icon-link');
  expect(contentData.values[0].value[0].name).toEqual(linkTitle);
  expect(contentData.values[0].value[0].url).toEqual(link);
});

test('can see validation error after removing all links from mandatory multi url picker', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.document.createDocumentWithExternalLinkURLPicker(contentName, mandatoryDocumentTypeId, mandatoryDataTypeName, link, linkTitle);
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.removeUrlPickerByName(linkTitle);
  await umbracoUi.content.clickSaveAndPublishButton();

  // Assert
  await umbracoUi.content.isValidationMessageVisible(ConstantHelper.validationMessages.emptyValue);
  await umbracoUi.content.isErrorNotificationVisible();
});

test('can see validation error clear when minimum number of links is met', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const minNumberDataTypeName = 'MinNumberMultiUrlPicker';
  const minNumberDataTypeId = await umbracoApi.dataType.createMultiUrlPickerDataTypeWithMinNumberOfItems(minNumberDataTypeName, 2);
  const minNumberDocumentTypeName = 'MinNumberDocumentType';
  const minNumberDocumentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(minNumberDocumentTypeName, minNumberDataTypeName, minNumberDataTypeId, 'TestGroup', false, false, true);
  await umbracoApi.document.createDefaultDocument(contentName, minNumberDocumentTypeId);
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  // Add first link
  await umbracoUi.content.clickAddMultiURLPickerButton();
  await umbracoUi.content.clickManualLinkButton();
  await umbracoUi.content.enterLink(link);
  await umbracoUi.content.enterLinkTitle(linkTitle);
  await umbracoUi.content.clickLinkPickerAddButton();
  // Try to save with only 1 link (min is 2)
  await umbracoUi.content.clickSaveAndPublishButton();
  await umbracoUi.content.isValidationMessageVisible(ConstantHelper.validationMessages.needMoreItems);
  // Add second link
  await umbracoUi.waitForTimeout(ConstantHelper.wait.short);
  await umbracoUi.content.clickAddMultiURLPickerButton();
  await umbracoUi.content.clickManualLinkButton();
  await umbracoUi.content.enterLink(secondLink);
  await umbracoUi.content.enterLinkTitle(secondLinkTitle);
  await umbracoUi.content.clickLinkPickerAddButton();
  // Validation should clear
  await umbracoUi.content.isValidationMessageVisible(ConstantHelper.validationMessages.needMoreItems, false);
  await umbracoUi.content.clickSaveButtonAndWaitForContentToBeUpdated();

  // Assert
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.values[0].alias).toEqual(AliasHelper.toAlias(minNumberDataTypeName));
  expect(contentData.values[0].value.length).toBe(2);
  expect(contentData.values[0].value[0].type).toEqual('external');
  expect(contentData.values[0].value[0].icon).toEqual('icon-link');
  expect(contentData.values[0].value[0].name).toEqual(linkTitle);
  expect(contentData.values[0].value[0].url).toEqual(link);
  expect(contentData.values[0].value[1].type).toEqual('external');
  expect(contentData.values[0].value[1].icon).toEqual('icon-link');
  expect(contentData.values[0].value[1].name).toEqual(secondLinkTitle);
  expect(contentData.values[0].value[1].url).toEqual(secondLink);

  // Clean
  await umbracoApi.documentType.ensureNameNotExists(minNumberDocumentTypeName);
  await umbracoApi.dataType.ensureNameNotExists(minNumberDataTypeName);
});