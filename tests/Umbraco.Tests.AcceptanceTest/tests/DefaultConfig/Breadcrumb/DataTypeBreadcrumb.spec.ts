import {ConstantHelper, test} from '@umbraco/acceptance-test-helpers';

const parentFolderName = 'BreadcrumbDataTypeFolder';
const childFolderName = 'BreadcrumbDataTypeChildFolder';
const dataTypeName = 'BreadcrumbDataTypeInFolder';
const rootName = 'Data Types';

test.beforeEach(async ({umbracoApi, umbracoUi}) => {
  await umbracoApi.dataType.ensureNameNotExists(dataTypeName);
  await umbracoApi.dataType.ensureNameNotExists(parentFolderName);
  await umbracoUi.goToBackOffice();
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.dataType.ensureNameNotExists(dataTypeName);
  await umbracoApi.dataType.ensureNameNotExists(parentFolderName);
});

async function createDataTypeInNestedFolders(umbracoApi) {
  const parentFolderId = await umbracoApi.dataType.createFolder(parentFolderName);
  const childFolderId = await umbracoApi.dataType.createFolder(childFolderName, parentFolderId);
  const dataTypeId = await umbracoApi.dataType.createTextstringDataType(dataTypeName);
  await umbracoApi.dataType.moveToFolder(dataTypeId, childFolderId);
  return childFolderId;
}

async function openDataTypeInNestedFolders(umbracoUi) {
  await umbracoUi.dataType.goToSection(ConstantHelper.sections.settings);
  await umbracoUi.dataType.clickRootFolderCaretButton();
  await umbracoUi.dataType.openCaretButtonForName(parentFolderName);
  await umbracoUi.dataType.openCaretButtonForName(childFolderName);
  await umbracoUi.dataType.clickTreeItemWithName(dataTypeName);
}

test('can see the folder path in the breadcrumb of a data type in nested folders', {tag: '@release'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await createDataTypeInNestedFolders(umbracoApi);

  // Act
  await openDataTypeInNestedFolders(umbracoUi);

  // Assert
  await umbracoUi.dataType.doesWorkspaceBreadcrumbHaveItems([rootName, parentFolderName, childFolderName, dataTypeName]);
});

test('can navigate to a parent folder by clicking it in the breadcrumb', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const childFolderId = await createDataTypeInNestedFolders(umbracoApi);
  await openDataTypeInNestedFolders(umbracoUi);

  // Act
  await umbracoUi.dataType.clickWorkspaceBreadcrumbItemWithName(childFolderName);

  // Assert
  await umbracoUi.dataType.isWorkspaceOpenForEntity('data-type-folder', childFolderId);
  await umbracoUi.dataType.doesWorkspaceBreadcrumbHaveItems([rootName, parentFolderName, childFolderName]);
});
