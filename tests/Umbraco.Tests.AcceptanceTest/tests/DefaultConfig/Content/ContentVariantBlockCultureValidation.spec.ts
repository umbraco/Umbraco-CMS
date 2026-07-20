import {ConstantHelper, test} from '@umbraco/acceptance-test-helpers';
import {expect} from '@playwright/test';

const contentName = 'TestContent';
const documentTypeName = 'TestDocumentTypeForContent';
const blockElementName = 'TestBlockElement';
const blockListName = 'TestBlockList';
const dataTypeName = 'Textstring';
let dataTypeId = '';

test.beforeEach(async ({umbracoApi}) => {
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.documentType.ensureNameNotExists(blockElementName);
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.dataType.ensureNameNotExists(blockListName);
  await umbracoApi.language.ensureIsoCodeNotExists('da');
  await umbracoApi.language.createDanishLanguage();
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  dataTypeId = dataTypeData.id;
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.documentType.ensureNameNotExists(blockElementName);
  await umbracoApi.dataType.ensureNameNotExists(blockListName);
  await umbracoApi.language.ensureIsoCodeNotExists('da');
});

test('can publish english variant when danish has no blocks and minimum is required', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const elementTypeId = await umbracoApi.documentType.createDefaultElementType(blockElementName, 'BlockContent', dataTypeName, dataTypeId);
  const blockListId = await umbracoApi.dataType.createBlockListWithABlockAndMinAndMaxAmount(blockListName, elementTypeId, 1, 10);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, blockListName, blockListId, 'Content', true, true);
  await umbracoApi.document.createDefaultDocumentWithEnglishCulture(contentName, documentTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  // Add a block and publish english
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickAddBlockElementButton();
  await umbracoUi.content.clickBlockElementWithName(blockElementName);
  await umbracoUi.content.enterBlockPropertyValue(dataTypeName, 'English block text');
  await umbracoUi.content.clickCreateModalButton();
  await umbracoUi.content.clickSaveAndPublishButton();
  await umbracoUi.content.clickContainerSaveAndPublishButtonAndWaitForContentToBePublished();

  // Assert
  await umbracoUi.content.isSuccessNotificationVisible();
  await umbracoUi.content.isErrorNotificationVisible(false);
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.variants[0].state).toBe('Published');
});

test('can publish english variant when block mandatory field is only filled in english and not danish', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const elementTypeId = await umbracoApi.documentType.createDefaultElementType(blockElementName, 'BlockContent', dataTypeName, dataTypeId, true);
  const blockListId = await umbracoApi.dataType.createBlockListDataTypeWithABlock(blockListName, elementTypeId);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, blockListName, blockListId, 'Content', true, true);
  await umbracoApi.document.createDefaultDocumentWithEnglishCulture(contentName, documentTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  // Add a block with mandatory field filled and publish english
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickAddBlockElementButton();
  await umbracoUi.content.clickBlockElementWithName(blockElementName);
  await umbracoUi.content.enterBlockPropertyValue(dataTypeName, 'English block text');
  await umbracoUi.content.clickCreateModalButton();
  await umbracoUi.content.clickSaveAndPublishButton();
  await umbracoUi.content.clickContainerSaveAndPublishButtonAndWaitForContentToBePublished();

  // Assert
  await umbracoUi.content.isSuccessNotificationVisible();
  await umbracoUi.content.isErrorNotificationVisible(false);
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.variants[0].state).toBe('Published');
});

test('can publish english after visiting danish that has block validation errors', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const elementTypeId = await umbracoApi.documentType.createDefaultElementType(blockElementName, 'BlockContent', dataTypeName, dataTypeId);
  const blockListId = await umbracoApi.dataType.createBlockListWithABlockAndMinAndMaxAmount(blockListName, elementTypeId, 1, 10);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, blockListName, blockListId, 'Content', true, true);
  await umbracoApi.document.createDefaultDocumentWithEnglishCulture(contentName, documentTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  // Add a block and publish english
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickAddBlockElementButton();
  await umbracoUi.content.clickBlockElementWithName(blockElementName);
  await umbracoUi.content.enterBlockPropertyValue(dataTypeName, 'English block text');
  await umbracoUi.content.clickCreateModalButton();
  await umbracoUi.content.clickSaveAndPublishButton();
  await umbracoUi.content.clickContainerSaveAndPublishButtonAndWaitForContentToBePublished();
  await umbracoUi.content.isSuccessNotificationVisible();
  // Switch to danish and back to english
  await umbracoUi.content.switchLanguage('Danish');
  await umbracoUi.waitForTimeout(ConstantHelper.timeout.short);
  await umbracoUi.content.switchLanguage('English');
  // Publish english again
  await umbracoUi.content.clickSaveAndPublishButton();
  await umbracoUi.content.clickContainerSaveAndPublishButtonAndWaitForContentToBePublished();

  // Assert
  await umbracoUi.content.isSuccessNotificationVisible();
  await umbracoUi.content.isErrorNotificationVisible(false);
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.variants[0].state).toBe('Published');
});
