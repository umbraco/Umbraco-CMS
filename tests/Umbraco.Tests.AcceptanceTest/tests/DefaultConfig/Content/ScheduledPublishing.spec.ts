import {ConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

let documentTypeId = '';
let contentId = '';
const contentName = 'TestContent';
const documentTypeName = 'TestDocumentTypeForContent';
const dataTypeName = 'Textstring';
const contentText = 'This is test content text';

test.beforeEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
});

test('can schedule publishing an invariant unpublish content', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeData.id);
  contentId = await umbracoApi.document.createDocumentWithTextContent(contentName, documentTypeId, contentText, dataTypeName);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickViewMoreOptionsButton();
  await umbracoUi.content.clickScheduleButton();

  // Assert

});

test('can schedule publishing an invariant unsaved content', async ({umbracoApi, umbracoUi}) => {

});

test('can schedule publishing a variant unpublish content', async ({umbracoApi, umbracoUi}) => {

});

test('can schedule publishing a variant unsaved content', async ({umbracoApi, umbracoUi}) => {

});

test('can schedule publishing an invariant unpublish child content', async ({umbracoApi, umbracoUi}) => {

});

test('can schedule publishing a variant unpublish child content', async ({umbracoApi, umbracoUi}) => {

});

test('can schedule publishing an invariant unsaved child content', async ({umbracoApi, umbracoUi}) => {

});

test('can schedule publishing a variant unsaved child content', async ({umbracoApi, umbracoUi}) => {

});

test('can schedule publishing a content with multiple culture variants', async ({umbracoApi, umbracoUi}) => {

});

test('publish time cannot be in the past', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeData.id);
  contentId = await umbracoApi.document.createDocumentWithTextContent(contentName, documentTypeId, contentText, dataTypeName);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickViewMoreOptionsButton();
  await umbracoUi.content.clickScheduleButton();
  await umbracoUi.content.enterUnpublishTime('03/05/2025 10:46 AM');
  await umbracoUi.content.clickScheduleModalButton();

  // Assert
});

test('unpublish time cannot be in the past', async ({umbracoApi, umbracoUi}) => {

});

test('unpublish time cannot be before publish time', async ({umbracoApi, umbracoUi}) => {

});