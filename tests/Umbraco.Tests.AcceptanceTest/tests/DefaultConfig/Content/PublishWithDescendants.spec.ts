import {ConstantHelper, NotificationConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

let documentTypeId = '';
let childDocumentTypeId = '';
let contentId = '';
let dataTypeId = '';
const contentName = 'TestContent';
const childContentName = 'ChildContent';
const documentTypeName = 'DocumentTypeForContent';
const childDocumentTypeName = 'ChildDocumentType';
const dataTypeName = 'Textstring';
const contentText = 'This is test content text';
const defaultLanguage = 'English (United States)';
const danishLanguage = 'Danish';

test.beforeEach(async ({umbracoApi}) => {
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.documentType.ensureNameNotExists(childDocumentTypeName);
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  dataTypeId = dataTypeData.id;
  await umbracoApi.language.ensureIsoCodeNotExists('da');
  await umbracoApi.language.createDanishLanguage();
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.language.ensureIsoCodeNotExists('da');
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.documentType.ensureNameNotExists(childDocumentTypeName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
});

test('can publish invariant content with descendants', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  childDocumentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(childDocumentTypeName, dataTypeName, dataTypeId);
  documentTypeId = await umbracoApi.documentType.createDocumentTypeWithAllowedChildNodeAndDataType(documentTypeName, childDocumentTypeId, dataTypeName, dataTypeId);
  contentId = await umbracoApi.document.createDocumentWithTextContent(contentName, documentTypeId, contentText, dataTypeName);
  await umbracoApi.document.createDefaultDocumentWithParent(childContentName, childDocumentTypeId, contentId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickViewMoreOptionsButton();
  await umbracoUi.content.clickPublishWithDescendantsButton();
  // verify variant language
  await umbracoUi.content.doesDocumentVariantLanguageItemHaveCount(1);
  await umbracoUi.content.doesDocumentVariantLanguageItemHaveName(defaultLanguage);
  await umbracoUi.content.clickPublishWithDescendantsModalButton();

  // Assert
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.publishWithDescendants);
  await umbracoUi.content.isErrorNotificationVisible(false);
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.variants[0].state).toBe('Published');
  expect(contentData.values[0].value).toBe(contentText);
  const childContentData = await umbracoApi.document.getByName(childContentName);
  expect(childContentData.variants[0].state).toBe('Draft');
});

test('can publish invariant content with descendants and include unpublished content items.', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  childDocumentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(childDocumentTypeName, dataTypeName, dataTypeId);
  documentTypeId = await umbracoApi.documentType.createDocumentTypeWithAllowedChildNodeAndDataType(documentTypeName, childDocumentTypeId, dataTypeName, dataTypeId);
  contentId = await umbracoApi.document.createDocumentWithTextContent(contentName, documentTypeId, contentText, dataTypeName);
  await umbracoApi.document.createDefaultDocumentWithParent(childContentName, childDocumentTypeId, contentId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickViewMoreOptionsButton();
  await umbracoUi.content.clickPublishWithDescendantsButton();
  // verify variant language
  await umbracoUi.content.doesDocumentVariantLanguageItemHaveCount(1);
  await umbracoUi.content.doesDocumentVariantLanguageItemHaveName(defaultLanguage);
  await umbracoUi.content.clickIncludeUnpublishedDescendantsToggle();
  await umbracoUi.content.clickPublishWithDescendantsModalButton();

  // Assert
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.publishWithDescendants);
  await umbracoUi.content.isErrorNotificationVisible(false);
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.variants[0].state).toBe('Published');
  expect(contentData.values[0].value).toBe(contentText);
  const childContentData = await umbracoApi.document.getByName(childContentName);
  expect(childContentData.variants[0].state).toBe('Published');
});

test('can cancel to publish invariant content with descendants', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  childDocumentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(childDocumentTypeName, dataTypeName, dataTypeId);
  documentTypeId = await umbracoApi.documentType.createDocumentTypeWithAllowedChildNodeAndDataType(documentTypeName, childDocumentTypeId, dataTypeName, dataTypeId);
  contentId = await umbracoApi.document.createDocumentWithTextContent(contentName, documentTypeId, contentText, dataTypeName);
  await umbracoApi.document.createDefaultDocumentWithParent(childContentName, childDocumentTypeId, contentId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickViewMoreOptionsButton();
  await umbracoUi.content.clickPublishWithDescendantsButton();
  // verify variant language
  await umbracoUi.content.doesDocumentVariantLanguageItemHaveCount(1);
  await umbracoUi.content.doesDocumentVariantLanguageItemHaveName(defaultLanguage);
  await umbracoUi.content.clickCloseButton();

  // Assert
  await umbracoUi.content.isErrorNotificationVisible(false);
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.variants[0].state).toBe('Draft');
  expect(contentData.values[0].value).toBe(contentText);
  const childContentData = await umbracoApi.document.getByName(childContentName);
  expect(childContentData.variants[0].state).toBe('Draft');
});

test('can publish variant content with descendants', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  childDocumentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(childDocumentTypeName, dataTypeName, dataTypeId);
  documentTypeId = await umbracoApi.documentType.createVariantDocumentTypeWithAllowedChildNodeAndInvariantPropertyEditor(documentTypeName, childDocumentTypeId, dataTypeName, dataTypeId);
  contentId = await umbracoApi.document.createDocumentWithEnglishCultureAndTextContent(contentName, documentTypeId, contentText, dataTypeName);
  await umbracoApi.document.createDefaultDocumentWithParent(childContentName, childDocumentTypeId, contentId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickViewMoreOptionsButton();
  await umbracoUi.content.clickPublishWithDescendantsButton();
  // verify variant language
  await umbracoUi.content.doesDocumentVariantLanguageItemHaveCount(2);
  await umbracoUi.content.doesDocumentVariantLanguageItemHaveName(defaultLanguage);
  await umbracoUi.content.doesDocumentVariantLanguageItemHaveName(danishLanguage);
  await umbracoUi.content.clickPublishWithDescendantsModalButton();

  // Assert
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.publishWithDescendants);
  await umbracoUi.content.isErrorNotificationVisible(false);
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.variants[0].state).toBe('Published');
  expect(contentData.values[0].value).toBe(contentText);
  const childContentData = await umbracoApi.document.getByName(childContentName);
  expect(childContentData.variants[0].state).toBe('Draft');
});

test('can publish variant content with descendants and include unpublished content items.', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  childDocumentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(childDocumentTypeName, dataTypeName, dataTypeId);
  documentTypeId = await umbracoApi.documentType.createVariantDocumentTypeWithAllowedChildNodeAndInvariantPropertyEditor(documentTypeName, childDocumentTypeId, dataTypeName, dataTypeId);
  contentId = await umbracoApi.document.createDocumentWithEnglishCultureAndTextContent(contentName, documentTypeId, contentText, dataTypeName);
  await umbracoApi.document.createDefaultDocumentWithParent(childContentName, childDocumentTypeId, contentId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickViewMoreOptionsButton();
  await umbracoUi.content.clickPublishWithDescendantsButton();
  // verify variant language
  await umbracoUi.content.doesDocumentVariantLanguageItemHaveCount(2);
  await umbracoUi.content.doesDocumentVariantLanguageItemHaveName(defaultLanguage);
  await umbracoUi.content.doesDocumentVariantLanguageItemHaveName(danishLanguage);
  await umbracoUi.content.clickIncludeUnpublishedDescendantsToggle();
  await umbracoUi.content.clickPublishWithDescendantsModalButton();

  // Assert
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.publishWithDescendants);
  await umbracoUi.content.isErrorNotificationVisible(false);
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.variants[0].state).toBe('Published');
  expect(contentData.values[0].value).toBe(contentText);
  const childContentData = await umbracoApi.document.getByName(childContentName);
  expect(childContentData.variants[0].state).toBe('Published');
});

test('can cancel to publish variant content with descendants', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  childDocumentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(childDocumentTypeName, dataTypeName, dataTypeId);
  documentTypeId = await umbracoApi.documentType.createVariantDocumentTypeWithAllowedChildNodeAndInvariantPropertyEditor(documentTypeName, childDocumentTypeId, dataTypeName, dataTypeId);
  contentId = await umbracoApi.document.createDocumentWithEnglishCultureAndTextContent(contentName, documentTypeId, contentText, dataTypeName);
  await umbracoApi.document.createDefaultDocumentWithParent(childContentName, childDocumentTypeId, contentId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickViewMoreOptionsButton();
  await umbracoUi.content.clickPublishWithDescendantsButton();
  // verify variant language
  await umbracoUi.content.doesDocumentVariantLanguageItemHaveCount(2);
  await umbracoUi.content.doesDocumentVariantLanguageItemHaveName(defaultLanguage);
  await umbracoUi.content.doesDocumentVariantLanguageItemHaveName(danishLanguage);
  await umbracoUi.content.clickCloseButton();

  // Assert
  await umbracoUi.content.isErrorNotificationVisible(false);
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.variants[0].state).toBe('Draft');
  expect(contentData.values[0].value).toBe(contentText);
  const childContentData = await umbracoApi.document.getByName(childContentName);
  expect(childContentData.variants[0].state).toBe('Draft');
});