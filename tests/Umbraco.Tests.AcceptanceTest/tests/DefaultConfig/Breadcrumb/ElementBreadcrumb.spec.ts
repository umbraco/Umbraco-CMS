import {ConstantHelper, test} from '@umbraco/acceptance-test-helpers';

const elementTypeName = 'BreadcrumbElementType';
const folderName = 'BreadcrumbElementFolder';
const elementName = 'BreadcrumbElementInFolder';
const rootName = 'Elements';
let elementTypeId = '';

test.beforeEach(async ({umbracoApi, umbracoUi}) => {
  await umbracoApi.element.ensureNameNotExists(folderName);
  await umbracoApi.documentType.ensureNameNotExists(elementTypeName);
  elementTypeId = await umbracoApi.documentType.createEmptyElementType(elementTypeName, true);
  await umbracoUi.goToBackOffice();
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.element.ensureNameNotExists(folderName);
  await umbracoApi.documentType.ensureNameNotExists(elementTypeName);
});

async function openElementInFolder(umbracoUi) {
  await umbracoUi.library.goToSection(ConstantHelper.sections.library);
  await umbracoUi.library.openElementCaretButtonForName(folderName);
  await umbracoUi.library.goToElementWithName(elementName);
}

test('can see the folder path in the breadcrumb of an element in a folder', {tag: '@release'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const folderId = await umbracoApi.element.createDefaultElementFolder(folderName);
  await umbracoApi.element.createDefaultElementWithParent(elementName, elementTypeId, folderId);

  // Act
  await openElementInFolder(umbracoUi);

  // Assert
  await umbracoUi.library.doesWorkspaceBreadcrumbHaveItems([rootName, folderName, elementName]);
});

test('can navigate to the folder by clicking it in the breadcrumb', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const folderId = await umbracoApi.element.createDefaultElementFolder(folderName);
  await umbracoApi.element.createDefaultElementWithParent(elementName, elementTypeId, folderId);
  await openElementInFolder(umbracoUi);

  // Act
  await umbracoUi.library.clickWorkspaceBreadcrumbItemWithName(folderName);

  // Assert
  await umbracoUi.library.isWorkspaceOpenForEntity('element-folder', folderId);
  await umbracoUi.library.doesWorkspaceBreadcrumbHaveItems([rootName, folderName]);
});
