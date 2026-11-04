import {ConstantHelper, test} from '@umbraco/acceptance-test-helpers';

const folderName = 'BreadcrumbDocumentTypeFolder';
const documentTypeName = 'BreadcrumbDocumentTypeInFolder';
const renamedFolderName = 'BreadcrumbDocumentTypeFolderRenamed';
const rootName = 'Document Types';

test.beforeEach(async ({umbracoApi, umbracoUi}) => {
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.documentType.ensureNameNotExists(folderName);
  await umbracoApi.documentType.ensureNameNotExists(renamedFolderName);
  await umbracoUi.goToBackOffice();
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.documentType.ensureNameNotExists(folderName);
  await umbracoApi.documentType.ensureNameNotExists(renamedFolderName);
});

async function openDocumentTypeInFolder(umbracoUi) {
  await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);
  await umbracoUi.documentType.clickRootFolderCaretButton();
  await umbracoUi.documentType.openCaretButtonForName(folderName);
  await umbracoUi.documentType.clickTreeItemWithName(documentTypeName);
}

test('can see the folder path in the breadcrumb of a document type in a folder', {tag: '@release'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const folderId = await umbracoApi.documentType.createFolder(folderName);
  await umbracoApi.documentType.createDefaultDocumentTypeInFolder(documentTypeName, folderId);

  // Act
  await openDocumentTypeInFolder(umbracoUi);

  // Assert
  await umbracoUi.documentType.doesWorkspaceBreadcrumbHaveItems([rootName, folderName, documentTypeName]);
});

test('can navigate to the folder by clicking it in the breadcrumb', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const folderId = await umbracoApi.documentType.createFolder(folderName);
  await umbracoApi.documentType.createDefaultDocumentTypeInFolder(documentTypeName, folderId);
  await openDocumentTypeInFolder(umbracoUi);

  // Act
  await umbracoUi.documentType.clickWorkspaceBreadcrumbItemWithName(folderName);

  // Assert
  await umbracoUi.documentType.isWorkspaceOpenForEntity('document-type-folder', folderId);
  await umbracoUi.documentType.doesWorkspaceBreadcrumbHaveItems([rootName, folderName]);
});

test('can navigate to the document type root by clicking the root of the breadcrumb', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const folderId = await umbracoApi.documentType.createFolder(folderName);
  await umbracoApi.documentType.createDefaultDocumentTypeInFolder(documentTypeName, folderId);
  await openDocumentTypeInFolder(umbracoUi);

  // Act
  await umbracoUi.documentType.clickWorkspaceBreadcrumbItemWithName(rootName);

  // Assert
  await umbracoUi.documentType.isWorkspaceOpenForEntity('document-type-root');
});

test('can see the folder path in the breadcrumb after saving a new document type in a folder', {tag: '@release'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.documentType.createFolder(folderName);
  await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);
  await umbracoUi.documentType.clickRootFolderCaretButton();

  // Act
  await umbracoUi.documentType.clickActionsMenuForName(folderName);
  await umbracoUi.documentType.clickCreateActionMenuOption();
  await umbracoUi.documentType.clickCreateDocumentTypeButton();
  await umbracoUi.documentType.enterDocumentTypeName(documentTypeName);
  const documentTypeId = await umbracoUi.documentType.clickSaveButtonAndWaitForDocumentTypeToBeCreated();

  // Assert
  await umbracoUi.documentType.isWorkspaceOpenForEntity('document-type', documentTypeId);
  await umbracoUi.documentType.doesWorkspaceBreadcrumbHaveItems([rootName, folderName, documentTypeName]);
});

test('can see the breadcrumb update when the folder of the open document type is renamed', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const folderId = await umbracoApi.documentType.createFolder(folderName);
  await umbracoApi.documentType.createDefaultDocumentTypeInFolder(documentTypeName, folderId);
  await openDocumentTypeInFolder(umbracoUi);
  await umbracoUi.documentType.doesWorkspaceBreadcrumbHaveItems([rootName, folderName, documentTypeName]);

  // Act
  await umbracoUi.documentType.clickActionsMenuForName(folderName);
  await umbracoUi.documentType.clickRenameActionMenuOption();
  await umbracoUi.documentType.enterRenameFolderName(renamedFolderName);
  await umbracoUi.documentType.clickConfirmRenameButtonAndWaitForDocumentTypeToBeRenamed();

  // Assert
  await umbracoUi.documentType.doesWorkspaceBreadcrumbHaveItems([rootName, renamedFolderName, documentTypeName]);
});
