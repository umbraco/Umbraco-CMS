import {ConstantHelper, test, AliasHelper} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

const contentName = 'TestContent';
const documentTypeName = 'TestDocumentTypeForContent';
const dataTypeName = 'Upload Video';
const uploadVideoPath = './fixtures/mediaLibrary/';

test.beforeEach(async ({umbracoApi}) => {
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.document.ensureNameNotExists(contentName);
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
});

test('can create content with the upload video data type', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const expectedState = 'Draft';
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeData.id);
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
  expect(contentData.values).toEqual([]);
});

test('can publish content with the upload video data type', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const expectedState = 'Published';
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeData.id);
  await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickSaveAndPublishButton();

  // Assert
  await umbracoUi.content.doesSuccessNotificationsHaveCount(2);
  expect(await umbracoApi.document.doesNameExist(contentName)).toBeTruthy();
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.variants[0].state).toBe(expectedState);
  expect(contentData.values).toEqual([]);
});

const uploadVideos = [
  {fileExtension: 'mp4', fileName: 'Video.mp4'},
  {fileExtension: 'webm', fileName: 'Webm.webm'},
  {fileExtension: 'ogv', fileName: 'Ogv.ogv'}
];
for (const uploadVideo of uploadVideos) {
  test(`can upload a video with the ${uploadVideo.fileExtension} extension in the content`, async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
    const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeData.id);
    await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
    await umbracoUi.goToBackOffice();
    await umbracoUi.content.goToSection(ConstantHelper.sections.content);

    // Act
    await umbracoUi.content.goToContentWithName(contentName);
    await umbracoUi.content.uploadFile(uploadVideoPath + uploadVideo.fileName);
    await umbracoUi.content.clickSaveButton();

    // Assert
    await umbracoUi.content.isSuccessNotificationVisible();
    expect(await umbracoApi.document.doesNameExist(contentName)).toBeTruthy();
    const contentData = await umbracoApi.document.getByName(contentName);
    expect(contentData.values[0].alias).toEqual(AliasHelper.toAlias(dataTypeName));
    expect(contentData.values[0].value.src).toContain(AliasHelper.toAlias(uploadVideo.fileName));
  });
}

// TODO: Remove skip when the front-end is ready. Currently the uploaded video still displays after removing.
test.skip('can remove a mp4 file in the content', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const uploadFileName = 'Video.mp4';
  const mineType = 'video/mp4';
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeData.id);
  await umbracoApi.document.createDocumentWithUploadFile(contentName, documentTypeId, dataTypeName, uploadFileName, mineType);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickRemoveFilesButton();
  await umbracoUi.content.clickSaveButton();

  // Assert
  await umbracoUi.content.isSuccessNotificationVisible();
  expect(await umbracoApi.document.doesNameExist(contentName)).toBeTruthy();
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.values).toEqual([]);
});
