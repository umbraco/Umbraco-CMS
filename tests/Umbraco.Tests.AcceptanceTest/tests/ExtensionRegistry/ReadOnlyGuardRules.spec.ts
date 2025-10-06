import {ConstantHelper, test} from '@umbraco/playwright-testhelpers';

// Content
const contentName = 'TestContent';
const contentText = 'This is test content text';
// DocumentType
const documentTypeName = 'TestDocumentTypeForContent';
// DataType
const dataTypeName = 'Textstring';

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.language.ensureIsoCodeNotExists("da");
});

test('can lock an invariant content node', {tag: '@release'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeData.id);
  await umbracoApi.document.createDocumentWithTextContent(contentName, documentTypeId, contentText, dataTypeName);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.isDocumentReadOnly(false);
  await umbracoUi.content.clickWorkspaceActionMenuButton();
  await umbracoUi.content.clickLockActionMenuOption();

  // Assert
  await umbracoUi.content.isContentNameReadOnly();
  await umbracoUi.content.isDocumentReadOnly(true);
  await umbracoUi.content.isPropertyEditorUiWithNameReadOnly("text-box");
});

test('can lock a variant content node', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.language.createDanishLanguage();
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  const documentTypeId = await umbracoApi.documentType.createVariantDocumentTypeWithInvariantPropertyEditor(documentTypeName, dataTypeName, dataTypeData.id);
  await umbracoApi.document.createDocumentWithEnglishCultureAndTextContent(contentName, documentTypeId, contentText, dataTypeName);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.isDocumentReadOnly(false);
  await umbracoUi.content.clickWorkspaceActionMenuButton();
  await umbracoUi.content.clickLockActionMenuOption();

  // Assert
  await umbracoUi.content.isContentNameReadOnly();
  await umbracoUi.content.isDocumentReadOnly(true);
  await umbracoUi.content.isPropertyEditorUiWithNameReadOnly("text-box");
});
