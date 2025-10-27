import {ConstantHelper, test} from '@umbraco/playwright-testhelpers';

// Content
const contentName = 'TestContent';
// DocumentType
const documentTypeName = 'TestDocumentTypeForContent';
// DataType
const dataTypeName = 'Textstring';
//Media
const mediaName = 'TestMedia';

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.media.ensureNameNotExists(mediaName);
});

test('can retrieve unique id and entity type of document', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeData.id);
  const documentId = await umbracoApi.document.createDocumentWithTextContent(contentName, documentTypeId, 'Test content', dataTypeName);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickWorkspaceActionMenuButton();
  await umbracoUi.content.clickEntityActionWithName('Retrieve');

  // Assert
  await umbracoUi.content.doesSuccessNotificationHaveText('document_' + documentId);
});

test('can retrieve unique id and entity type of media', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const mediaId = await umbracoApi.media.createDefaultMediaWithImage(mediaName);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.media);

  // Act
  await umbracoUi.media.goToMediaWithName(mediaName);
  await umbracoUi.media.clickWorkspaceActionMenuButton();
  await umbracoUi.media.clickEntityActionWithName('Retrieve');

  // Assert
  await umbracoUi.media.doesSuccessNotificationHaveText('media_' + mediaId);
});
