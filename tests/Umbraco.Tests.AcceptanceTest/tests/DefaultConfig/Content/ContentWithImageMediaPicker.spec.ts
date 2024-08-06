import {ConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

const contentName = 'TestContent';
const documentTypeName = 'TestDocumentTypeForContent';
const dataTypeName = 'Image Media Picker';
const groupName = 'TestGroup';

test.beforeEach(async ({umbracoApi, umbracoUi}) => {
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
});

test('can save content with a image media picker', async ({page, umbracoApi, umbracoUi}) => {
  // Arrange
  const expectedState = 'Draft';
  const dataType = await umbracoApi.dataType.getByName(dataTypeName);
  await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataType.id, groupName);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.clickActionsMenuAtRoot();
  await umbracoUi.content.clickCreateButton();
  await umbracoUi.content.chooseDocumentType(documentTypeName);
  await umbracoUi.content.enterContentName(contentName);
  await umbracoUi.content.clickSaveButton();

  // Assert
  await umbracoUi.content.isSuccessNotificationVisible();
  expect(await umbracoApi.document.doesNameExist(contentName)).toBeTruthy();
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.variants[0].state).toBe(expectedState);
});

test('can publish content with a image media picker', async ({page, umbracoApi, umbracoUi}) => {
  // Arrange
  const expectedState = 'Published';
  const dataType = await umbracoApi.dataType.getByName(dataTypeName);
  const documentId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataType.id, groupName);
  await umbracoApi.document.createDefaultDocument(contentName, documentId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickSaveAndPublishButton();

  // Assert
  await umbracoUi.content.isSuccessNotificationVisible();
  expect(await umbracoApi.document.doesNameExist(contentName)).toBeTruthy();
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.variants[0].state).toBe(expectedState);
});

test('can add an image to the image media picker', async ({page, umbracoApi, umbracoUi}) => {
// Arrange
  const expectedState = 'Published';
  const dataType = await umbracoApi.dataType.getByName(dataTypeName);

  await umbracoApi.media.ensureNameNotExists('TestImage');
  await umbracoApi.media.createDefaultMediaWithImage('TestImage');
  const documentId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataType.id, groupName);
  await umbracoApi.document.createDefaultDocument(contentName, documentId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await page.pause();

  await umbracoApi.media.ensureNameNotExists('TestImage');
});

test('can remove an image from the image media picker', async ({page, umbracoApi, umbracoUi}) => {

});

test('image count can not be less that min amount set in an image media picker', async ({page, umbracoApi, umbracoUi}) => {

});

test('image count can not be more that max amount set in an image media picker', async ({page, umbracoApi, umbracoUi}) => {

});

test('can add an image from the image media picker with a start node', async ({page, umbracoApi, umbracoUi}) => {
});

test('can add an image from the image media picker with focal point enabled', async ({page, umbracoApi, umbracoUi}) => {

});





