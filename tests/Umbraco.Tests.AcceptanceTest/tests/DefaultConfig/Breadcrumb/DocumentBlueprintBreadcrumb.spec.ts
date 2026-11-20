import {ConstantHelper, test} from '@umbraco/acceptance-test-helpers';

const documentTypeName = 'BreadcrumbBlueprintDocumentType';
const folderName = 'BreadcrumbBlueprintFolder';
const documentBlueprintName = 'BreadcrumbBlueprintInFolder';
const rootName = 'Document Blueprints';
let documentTypeId = '';

test.beforeEach(async ({umbracoApi, umbracoUi}) => {
  await umbracoApi.documentBlueprint.ensureNameNotExists(documentBlueprintName);
  await umbracoApi.documentBlueprint.ensureNameNotExists(folderName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  documentTypeId = await umbracoApi.documentType.createDefaultDocumentTypeWithAllowAsRoot(documentTypeName);
  await umbracoUi.goToBackOffice();
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.documentBlueprint.ensureNameNotExists(documentBlueprintName);
  await umbracoApi.documentBlueprint.ensureNameNotExists(folderName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
});

async function openDocumentBlueprintInFolder(umbracoUi) {
  await umbracoUi.documentBlueprint.goToSection(ConstantHelper.sections.library);
  await umbracoUi.documentBlueprint.reloadDocumentBlueprintsTree();
  await umbracoUi.documentBlueprint.openCaretButtonForName(folderName);
  await umbracoUi.documentBlueprint.clickTreeItemWithName(documentBlueprintName);
}

test('can see the folder path in the breadcrumb of a document blueprint in a folder', {tag: '@release'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const folderId = await umbracoApi.documentBlueprint.createFolder(folderName);
  await umbracoApi.documentBlueprint.createDefaultDocumentBlueprintWithParent(documentBlueprintName, documentTypeId, folderId);

  // Act
  await openDocumentBlueprintInFolder(umbracoUi);

  // Assert
  await umbracoUi.documentBlueprint.doesWorkspaceBreadcrumbHaveItems([rootName, folderName, documentBlueprintName]);
});

test('can navigate to the folder by clicking it in the breadcrumb', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const folderId = await umbracoApi.documentBlueprint.createFolder(folderName);
  await umbracoApi.documentBlueprint.createDefaultDocumentBlueprintWithParent(documentBlueprintName, documentTypeId, folderId);
  await openDocumentBlueprintInFolder(umbracoUi);

  // Act
  await umbracoUi.documentBlueprint.clickWorkspaceBreadcrumbItemWithName(folderName);

  // Assert
  await umbracoUi.documentBlueprint.isWorkspaceOpenForEntity('document-blueprint-folder', folderId);
  await umbracoUi.documentBlueprint.doesWorkspaceBreadcrumbHaveItems([rootName, folderName]);
});
