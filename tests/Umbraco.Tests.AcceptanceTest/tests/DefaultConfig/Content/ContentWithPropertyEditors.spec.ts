import {ConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

const contentName = 'TestContent';
const documentTypeName = 'TestDocumentTypeForContent';

test.beforeEach(async ({umbracoApi}) => {
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.document.ensureNameNotExists(contentName); 
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(contentName); 
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName); 
});

// TODO: Skip this test as TinyMCE is replaced by Tiptap. This test should be updated.
test.skip('can create content with the Rich Text Editor datatype', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const dataTypeName = 'Richtext editor';
  const contentText = 'This is Rich Text Editor content!';
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeData.id);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.clickActionsMenuAtRoot();
  await umbracoUi.content.clickCreateButton();
  await umbracoUi.content.chooseDocumentType(documentTypeName);
  await umbracoUi.content.enterContentName(contentName);
  await umbracoUi.content.enterRichTextArea(contentText);
  await umbracoUi.content.clickSaveAndPublishButton();

  // Assert
  //await umbracoUi.content.doesSuccessNotificationsHaveCount(2);  
  await umbracoUi.content.isErrorNotificationVisible(false);
  expect(await umbracoApi.document.doesNameExist(contentName)).toBeTruthy();
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.values[0].value.markup).toEqual('<p>' + contentText + '</p>');
});

// TODO: Remove skip when the front-end is ready. Currently it returns error when publishing a content
test.skip('can create content with the upload file datatype', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const dataTypeName = 'Upload File';
  const uploadFilePath = 'Umbraco.png';
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeData.id);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.clickActionsMenuAtRoot();
  await umbracoUi.content.clickCreateButton();
  await umbracoUi.content.chooseDocumentType(documentTypeName);
  await umbracoUi.content.enterContentName(contentName);
  await umbracoUi.content.uploadFile('./fixtures/mediaLibrary/' + uploadFilePath);
  await umbracoUi.content.clickSaveAndPublishButton();

  // Assert
  //await umbracoUi.content.doesSuccessNotificationsHaveCount(2);  
  await umbracoUi.content.isErrorNotificationVisible(false);
  expect(await umbracoApi.document.doesNameExist(contentName)).toBeTruthy();
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.values[0].value.src).toContainEqual(uploadFilePath);
});

// TODO: Remove skip and update the test when the front-end is ready. Currently the list of content is not displayed.
test.skip('can create content with the list view - content datatype', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const dataTypeName = 'List View - Content';
  const contentListViewName = 'TestListViewContent';
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeData.id);
  const contentPickerId = await umbracoApi.document.createDefaultDocument(contentListViewName, documentTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  
  // Act
  await umbracoUi.content.clickActionsMenuAtRoot();
  await umbracoUi.content.clickCreateButton();
  await umbracoUi.content.chooseDocumentType(documentTypeName);
  await umbracoUi.content.enterContentName(contentName);
  // TODO: add step to interact with the list
  await umbracoUi.content.clickSaveAndPublishButton();

  // Assert
  //await umbracoUi.content.doesSuccessNotificationsHaveCount(2);  
  await umbracoUi.content.isErrorNotificationVisible(false);
  expect(await umbracoApi.document.doesNameExist(contentName)).toBeTruthy();

  // Clean
  await umbracoApi.document.delete(contentPickerId);
});
