import {ConstantHelper, test} from '@umbraco/acceptance-test-helpers';

const parentFolderName = 'BreadcrumbMediaFolder';
const childFolderName = 'BreadcrumbMediaChildFolder';
const mediaSectionName = 'Media';

test.beforeEach(async ({umbracoApi, umbracoUi}) => {
  await umbracoApi.media.ensureNameNotExists(parentFolderName);
  await umbracoUi.goToBackOffice();
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.media.ensureNameNotExists(parentFolderName);
});

test('can see the folder path in the breadcrumb of media in a folder', {tag: '@release'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const parentFolderId = await umbracoApi.media.createDefaultMediaFolder(parentFolderName);
  await umbracoApi.media.createDefaultMediaFolderAndParentId(childFolderName, parentFolderId);
  await umbracoUi.media.goToSection(ConstantHelper.sections.media);

  // Act
  await umbracoUi.media.openMediaCaretButtonForName(parentFolderName);
  await umbracoUi.media.goToMediaWithName(childFolderName);

  // Assert
  await umbracoUi.media.doesWorkspaceBreadcrumbHaveItems([mediaSectionName, parentFolderName, childFolderName]);
});

test('can navigate to the parent folder by clicking it in the breadcrumb', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const parentFolderId = await umbracoApi.media.createDefaultMediaFolder(parentFolderName);
  await umbracoApi.media.createDefaultMediaFolderAndParentId(childFolderName, parentFolderId);
  await umbracoUi.media.goToSection(ConstantHelper.sections.media);
  await umbracoUi.media.openMediaCaretButtonForName(parentFolderName);
  await umbracoUi.media.goToMediaWithName(childFolderName);

  // Act
  await umbracoUi.media.clickWorkspaceBreadcrumbItemWithName(parentFolderName);

  // Assert
  await umbracoUi.media.isWorkspaceOpenForEntity('media', parentFolderId);
  await umbracoUi.media.doesWorkspaceBreadcrumbHaveItems([mediaSectionName, parentFolderName]);
});
