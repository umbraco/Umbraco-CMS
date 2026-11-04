import {ConstantHelper, test} from '@umbraco/acceptance-test-helpers';
import {expect} from '@playwright/test';

const documentTypeName = 'BreadcrumbDocumentType';
const rootContentName = 'BreadcrumbRoot';
const childContentName = 'BreadcrumbChild';
const grandchildContentName = 'BreadcrumbGrandchild';
const otherRootContentName = 'BreadcrumbOtherRoot';
const contentSectionName = 'Content';
let documentTypeId = '';

test.beforeEach(async ({umbracoApi, umbracoUi}) => {
  await umbracoApi.document.ensureNameNotExists(rootContentName);
  await umbracoApi.document.ensureNameNotExists(otherRootContentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  documentTypeId = await umbracoApi.documentType.createDocumentTypeWithTextstringAndAllowAsRootAndAllowSelfAsChild(documentTypeName);
  await umbracoUi.goToBackOffice();
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(rootContentName);
  await umbracoApi.document.ensureNameNotExists(otherRootContentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
});

test('can see the full ancestor path in the breadcrumb of nested content', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.document.createPublishedInvariantChain(documentTypeId, [rootContentName, childContentName, grandchildContentName]);
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.openContentCaretButtonForName(rootContentName);
  await umbracoUi.content.openContentCaretButtonForName(childContentName);
  await umbracoUi.content.goToContentWithName(grandchildContentName);

  // Assert
  await umbracoUi.content.doesWorkspaceBreadcrumbHaveItems([contentSectionName, rootContentName, childContentName, grandchildContentName]);
});

test('can navigate to an ancestor by clicking it in the breadcrumb', {tag: '@release'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const [, childContentId] = await umbracoApi.document.createPublishedInvariantChain(documentTypeId, [rootContentName, childContentName, grandchildContentName]);
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.content.openContentCaretButtonForName(rootContentName);
  await umbracoUi.content.openContentCaretButtonForName(childContentName);
  await umbracoUi.content.goToContentWithName(grandchildContentName);

  // Act
  await umbracoUi.content.clickWorkspaceBreadcrumbItemWithName(childContentName);

  // Assert
  await umbracoUi.content.isWorkspaceOpenForEntity('document', childContentId);
  await umbracoUi.content.doesWorkspaceBreadcrumbHaveItems([contentSectionName, rootContentName, childContentName]);
});

test('can navigate to the content section by clicking the root of the breadcrumb', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.document.createPublishedInvariantChain(documentTypeId, [rootContentName, childContentName]);
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.content.openContentCaretButtonForName(rootContentName);
  await umbracoUi.content.goToContentWithName(childContentName);
  await umbracoUi.content.isWorkspaceBreadcrumbVisible();

  // Act
  await umbracoUi.content.clickWorkspaceBreadcrumbItemWithName(contentSectionName);

  // Assert
  await umbracoUi.content.isSectionOpenWithoutWorkspace('content');
  await umbracoUi.content.isWorkspaceBreadcrumbVisible(false);
});

test('can see the parent path and the entered name in the breadcrumb when creating content under a parent', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.document.createPublishedInvariantChain(documentTypeId, [rootContentName, childContentName]);
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.content.openContentCaretButtonForName(rootContentName);

  // Act
  await umbracoUi.content.clickActionsMenuForContent(childContentName);
  await umbracoUi.content.clickCreateActionMenuOption();
  await umbracoUi.content.chooseDocumentType(documentTypeName);
  await umbracoUi.content.enterContentName(grandchildContentName);

  // Assert
  await umbracoUi.content.doesWorkspaceBreadcrumbHaveItems([contentSectionName, rootContentName, childContentName, grandchildContentName]);
});

test('can see the breadcrumb follow the content name while it is being changed', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const updatedChildContentName = 'BreadcrumbChildUpdated';
  await umbracoApi.document.createPublishedInvariantChain(documentTypeId, [rootContentName, childContentName]);
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.content.openContentCaretButtonForName(rootContentName);
  await umbracoUi.content.goToContentWithName(childContentName);
  await umbracoUi.content.doesWorkspaceBreadcrumbHaveItems([contentSectionName, rootContentName, childContentName]);

  // Act
  await umbracoUi.content.enterContentName(updatedChildContentName);

  // Assert
  await umbracoUi.content.doesWorkspaceBreadcrumbHaveItems([contentSectionName, rootContentName, updatedChildContentName]);
});

test('can see the breadcrumb update after moving the open content to another parent', {tag: '@release'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.document.createPublishedInvariantChain(documentTypeId, [rootContentName, childContentName]);
  await umbracoApi.document.createPublishedInvariantChain(documentTypeId, [otherRootContentName]);
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.content.openContentCaretButtonForName(rootContentName);
  await umbracoUi.content.goToContentWithName(childContentName);
  await umbracoUi.content.doesWorkspaceBreadcrumbHaveItems([contentSectionName, rootContentName, childContentName]);

  // Act
  await umbracoUi.content.clickWorkspaceActionMenuButton();
  await umbracoUi.content.clickMoveToActionMenuOption();
  await umbracoUi.content.moveToContentWithName([], otherRootContentName);

  // Assert
  await umbracoUi.content.doesWorkspaceBreadcrumbHaveItems([contentSectionName, otherRootContentName, childContentName]);
});

test('can stay on the content with its unsaved changes when leaving through the breadcrumb', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const updatedChildContentName = 'BreadcrumbChildUpdated';
  const [, childContentId] = await umbracoApi.document.createPublishedInvariantChain(documentTypeId, [rootContentName, childContentName]);
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.content.openContentCaretButtonForName(rootContentName);
  await umbracoUi.content.goToContentWithName(childContentName);
  await umbracoUi.content.enterContentName(updatedChildContentName);

  // Act
  await umbracoUi.content.clickWorkspaceBreadcrumbItemWithName(rootContentName);
  await umbracoUi.content.isDiscardChangesModalVisible();
  await umbracoUi.content.clickStayOnPageButton();

  // Assert
  await umbracoUi.content.isWorkspaceOpenForEntity('document', childContentId);
  await umbracoUi.content.doesWorkspaceBreadcrumbHaveItems([contentSectionName, rootContentName, updatedChildContentName]);
});

test('can discard unsaved changes when leaving the content through the breadcrumb', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const updatedChildContentName = 'BreadcrumbChildUpdated';
  const [rootContentId] = await umbracoApi.document.createPublishedInvariantChain(documentTypeId, [rootContentName, childContentName]);
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.content.openContentCaretButtonForName(rootContentName);
  await umbracoUi.content.goToContentWithName(childContentName);
  await umbracoUi.content.enterContentName(updatedChildContentName);

  // Act
  await umbracoUi.content.clickWorkspaceBreadcrumbItemWithName(rootContentName);
  await umbracoUi.content.isDiscardChangesModalVisible();
  await umbracoUi.content.clickDiscardChangesButton();

  // Assert
  await umbracoUi.content.isWorkspaceOpenForEntity('document', rootContentId);
  await umbracoUi.content.doesWorkspaceBreadcrumbHaveItems([contentSectionName, rootContentName]);
  expect(await umbracoApi.document.doesNameExist(childContentName)).toBeTruthy();
  expect(await umbracoApi.document.doesNameExist(updatedChildContentName)).toBeFalsy();
});
