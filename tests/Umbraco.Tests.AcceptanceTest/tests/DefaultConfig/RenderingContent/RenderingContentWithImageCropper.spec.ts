import {AliasHelper, test} from '@umbraco/playwright-testhelpers';

const contentName = 'Test Rendering Content';
const documentTypeName = 'TestDocumentTypeForContent';
const customDataTypeName = 'Custom Image Cropper';
const templateName = 'TestTemplateForContent';
const propertyName = 'Test Image Cropper';
const cropLabel = 'Test Crop';
const cropValue = {label: cropLabel, alias: AliasHelper.toAlias(cropLabel), width: 500, height: 700};
let dataTypeId = null;

test.beforeEach(async ({umbracoApi}) => {
  dataTypeId = await umbracoApi.dataType.createImageCropperDataTypeWithOneCrop(customDataTypeName, cropValue.label, cropValue.width, cropValue.height);
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.template.ensureNameNotExists(templateName);
  await umbracoApi.dataType.ensureNameNotExists(customDataTypeName);
});

test('can render content with an image cropper', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const templateId = await umbracoApi.template.createTemplateWithDisplayingImageCropperValue(templateName, AliasHelper.toAlias(propertyName), AliasHelper.toAlias(cropValue.label));
  await umbracoApi.document.createPublishedDocumentWithImageCropper(contentName, cropValue, dataTypeId, templateId, propertyName, documentTypeName);
  const contentData = await umbracoApi.document.getByName(contentName);
  const contentURL = await umbracoApi.document.getDocumentUrl(contentData.id);
  const imageSrc = contentData.values[0].value.src;

  // Act
  await umbracoUi.contentRender.navigateToRenderedContentPage(contentURL);

  // Assert
  await umbracoUi.contentRender.doesContentRenderValueHaveImage(imageSrc, cropValue.width, cropValue.height);
});
