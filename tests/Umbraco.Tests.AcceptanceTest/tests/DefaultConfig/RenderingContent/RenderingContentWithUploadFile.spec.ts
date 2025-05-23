import {AliasHelper, test} from '@umbraco/playwright-testhelpers';

const contentName = 'Test Rendering Content';
const documentTypeName = 'TestDocumentTypeForContent';
const templateName = 'TestTemplateForContent';
const propertyName = 'Test Upload File';

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.template.ensureNameNotExists(templateName);
});

const uploadedFileList = [
  {type: 'an uploaded text file', dataTypeName: 'Upload File', uploadedFileName: 'File.txt', mineType: 'text/plain'},
  {type: 'an uploaded image', dataTypeName: 'Upload File', uploadedFileName: 'Umbraco.png', mineType: 'image/png'},
  {type: 'an uploaded video', dataTypeName: 'Upload Video', uploadedFileName: 'Video.mp4', mineType: 'video/mp4'},
  {type: 'an uploaded audio', dataTypeName: 'Upload Audio', uploadedFileName: 'Audio.mp3', mineType: 'audio/mpeg'},
  {type: 'an uploaded article', dataTypeName: 'Upload Article', uploadedFileName: 'Article.pdf', mineType: 'application/pdf'},
  {type: 'an vector graphics', dataTypeName: 'Upload Vector Graphics', uploadedFileName: 'VectorGraphics.svg', mineType: 'image/svg+xml'},
];

for (const uploadedFile of uploadedFileList) {
  test(`can render content with ${uploadedFile.type}`, async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const dataTypeData = await umbracoApi.dataType.getByName(uploadedFile.dataTypeName);
    const templateId = await umbracoApi.template.createTemplateWithDisplayingUploadedFileValue(templateName, AliasHelper.toAlias(propertyName));
    await umbracoApi.document.createPublishedDocumentWithUploadFile(contentName, uploadedFile.uploadedFileName, uploadedFile.mineType, dataTypeData.id, templateId, propertyName, documentTypeName);
    const contentData = await umbracoApi.document.getByName(contentName);
    const contentURL = await umbracoApi.document.getDocumentUrl(contentData.id);
    const uploadFileSrc = contentData.values[0].value.src;

    // Act
    await umbracoUi.contentRender.navigateToRenderedContentPage(contentURL);

    // Assert
    await umbracoUi.contentRender.doesContentRenderValueContainText(uploadFileSrc.split("/").pop());
    await umbracoUi.contentRender.doesContentRenderValueHaveLink(uploadFileSrc);
  });
}
