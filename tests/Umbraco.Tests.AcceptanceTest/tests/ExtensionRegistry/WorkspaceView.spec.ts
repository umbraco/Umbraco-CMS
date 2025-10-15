import {ConstantHelper, test} from '@umbraco/playwright-testhelpers';

// Content
const contentName = 'TestContent';
// DocumentType
const documentTypeName = 'TestDocumentTypeForContent';
// DataType
const dataTypeName = 'Textstring';
// Media
const mediaName = 'TestMedia';

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.media.ensureNameNotExists(mediaName);
});

test('can see the custom workspace view in the content section', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeData.id);
  await umbracoApi.document.createDocumentWithTextContent(contentName, documentTypeId, 'Test content', dataTypeName);

  // Act
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.content.goToContentWithName(contentName);

  // Assert
  await umbracoUi.content.isWorkspaceViewTabWithAliasVisible('My.WorkspaceView', true);
});

test('cannot see the custom workspace view in the media section', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.media.createDefaultMediaWithImage(mediaName);

  // Act
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.media);
  await umbracoUi.media.goToMediaWithName(mediaName);

  // Assert
  await umbracoUi.media.isWorkspaceViewTabWithAliasVisible('My.WorkspaceView', false);
});