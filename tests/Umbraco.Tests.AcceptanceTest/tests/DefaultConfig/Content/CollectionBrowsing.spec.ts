import {ConstantHelper, test} from '@umbraco/acceptance-test-helpers';
import {expect} from '@playwright/test';

const contentName = 'TestContent';
const childContentName = 'ChildContent';
const documentTypeName = 'TestDocumentTypeForContent';
const childDocumentTypeName = 'ChildDocumentTypeForContent';
const dataTypeName = 'List View - Content';

let childDocumentId = '';

test.beforeEach(async ({umbracoApi, umbracoUi}) => {
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.documentType.ensureNameNotExists(childDocumentTypeName);

  const childDocumentTypeId = await umbracoApi.documentType.createDefaultDocumentType(childDocumentTypeName);
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithAllowedChildNodeAndCollectionId(documentTypeName, childDocumentTypeId, dataTypeData.id);
  const documentId = await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  childDocumentId = await umbracoApi.document.createDefaultDocumentWithParent(childContentName, childDocumentTypeId, documentId);

  await umbracoUi.goToBackOffice();
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.documentType.ensureNameNotExists(childDocumentTypeName);
});

test('can open a collection item from the grid view', {tag: '@smoke'}, async ({umbracoUi}) => {
  // Arrange
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.changeToGridView();
  await umbracoUi.content.isDocumentGridViewVisible();
  await umbracoUi.content.isContentWithNameVisibleInGrid(childContentName);

  // Act
  // An early click can be lost while the collection re-renders, leaving the workspace unopened.
  // Retry until the edit route is actually loaded.
  await expect(async () => {
    await umbracoUi.content.clickContentCardWithName(childContentName);
    await umbracoUi.content.waitForWorkspaceEditRoute('document');
  }).toPass({timeout: ConstantHelper.timeout.veryLong});

  // Assert
  // The click must have opened the child's own edit workspace, not just any document route.
  await expect(umbracoUi.content.page).toHaveURL(new RegExp(childDocumentId));
});

test('can open a collection item from the table view', {tag: '@smoke'}, async ({umbracoUi}) => {
  // Arrange
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.changeToListView();
  await umbracoUi.content.isDocumentListViewVisible();
  await umbracoUi.content.doesFirstItemInListViewHaveName(childContentName);

  // Act
  // An early click can be lost while the table re-renders, leaving the workspace unopened.
  // Retry until the edit route is actually loaded.
  await expect(async () => {
    await umbracoUi.content.goToContentInListViewWithName(childContentName);
    await umbracoUi.content.waitForWorkspaceEditRoute('document');
  }).toPass({timeout: ConstantHelper.timeout.veryLong});

  // Assert
  // The click must have opened the child's own edit workspace, not just any document route.
  await expect(umbracoUi.content.page).toHaveURL(new RegExp(childDocumentId));
});
