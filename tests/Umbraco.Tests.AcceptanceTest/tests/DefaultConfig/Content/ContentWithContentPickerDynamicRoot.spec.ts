import {ConstantHelper, test} from '@umbraco/acceptance-test-helpers';
import {expect} from "@playwright/test";

const contentName = 'TestContent';
const documentTypeName = 'TestDocumentTypeForContent';
const customDataTypeName = 'CustomContentPickerDynamicRoot';
const rootDocumentTypeName = 'RootDocumentType';
const firstRootContentName = 'FirstRootContent';
const secondRootContentName = 'SecondRootContent';

test.beforeEach(async ({umbracoUi}) => {
  await umbracoUi.goToBackOffice();
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.document.ensureNameNotExists(firstRootContentName);
  await umbracoApi.document.ensureNameNotExists(secondRootContentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.documentType.ensureNameNotExists(rootDocumentTypeName);
  await umbracoApi.dataType.ensureNameNotExists(customDataTypeName);
});

test('can see all root nodes when using content root dynamic root origin', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const rootDocumentTypeId = await umbracoApi.documentType.createDefaultDocumentTypeWithAllowAsRoot(rootDocumentTypeName);
  await umbracoApi.document.createDefaultDocument(firstRootContentName, rootDocumentTypeId);
  await umbracoApi.document.createDefaultDocument(secondRootContentName, rootDocumentTypeId);
  const customDataTypeId = await umbracoApi.dataType.createContentPickerSourceDataTypeWithDynamicRoot(customDataTypeName, 'ContentRoot');
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, customDataTypeName, customDataTypeId);
  await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickChooseButton();

  // Assert
  await umbracoUi.content.isModalMenuItemWithNameVisible(firstRootContentName);
  await umbracoUi.content.isModalMenuItemWithNameVisible(secondRootContentName);
});

test('can save content from another root node using content root dynamic root', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const rootDocumentTypeId = await umbracoApi.documentType.createDefaultDocumentTypeWithAllowAsRoot(rootDocumentTypeName);
  const firstRootContentId = await umbracoApi.document.createDefaultDocument(firstRootContentName, rootDocumentTypeId);
  await umbracoApi.document.createDefaultDocument(secondRootContentName, rootDocumentTypeId);
  const customDataTypeId = await umbracoApi.dataType.createContentPickerSourceDataTypeWithDynamicRoot(customDataTypeName, 'ContentRoot');
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, customDataTypeName, customDataTypeId);
  await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickChooseButton();
  await umbracoUi.content.selectLinkByName(firstRootContentName);
  await umbracoUi.content.clickChooseModalButton();
  await umbracoUi.content.clickSaveButtonAndWaitForContentToBeUpdated();

  // Assert
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.values[0].value[0]['unique']).toEqual(firstRootContentId);
  expect(contentData.values[0].value[0]['type']).toEqual('document');
});
