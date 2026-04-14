import {ConstantHelper, NotificationConstantHelper, test} from '@umbraco/acceptance-test-helpers';
import {expect} from "@playwright/test";

const userPassword = '0123456789';

test.afterEach(async ({umbracoApi}) => {
  // Ensure we are logged in to admin
  await umbracoApi.loginToAdminUser();
});

const userGroups = ['Administrators', 'Editors', 'Sensitive data', 'Translators', 'Writers'];
for (const userGroup of userGroups) {
  test(`${userGroup} user can access the user profile and change the password`, async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const crypto = require('crypto');
    const userName = 'TestUser' + crypto.randomUUID();
    const userEmail = userName + '@test.com';
    const newPassword = 'TestNewPassword';
    const userGroupData = await umbracoApi.userGroup.getByName(userGroup);
    const userId = await umbracoApi.user.createDefaultUser(userName, userEmail, [userGroupData.id]);
    await umbracoApi.user.updatePassword(userId, userPassword);
    await umbracoApi.user.loginToUser(userName, userEmail, userPassword);
    await umbracoUi.goToBackOffice();
    await umbracoUi.currentUserProfile.isBackOfficeMainVisible();
    await umbracoUi.waitForTimeout(ConstantHelper.wait.medium); // Wait to ensure the UI is fully loaded

    // Act
    await umbracoUi.currentUserProfile.clickCurrentUserAvatarButton();
    await umbracoUi.currentUserProfile.isErrorNotificationVisible(false);
    await umbracoUi.currentUserProfile.clickChangePasswordButton();
    await umbracoUi.currentUserProfile.changePasswordAndWaitForSuccess(userPassword, newPassword);

    // Assert
    await umbracoUi.currentUserProfile.doesSuccessNotificationHaveText(NotificationConstantHelper.success.passwordChanged);
  });
}

test.describe('Recent History', () => {
  const docTypeName = 'HistoryTestDocType';
  const rootName = 'Root Page';
  const parentName = 'Parent Page';
  const childName = 'Child Page';

  test.beforeEach(async ({umbracoApi}) => {
    await umbracoApi.document.ensureNameNotExists(childName);
    await umbracoApi.document.ensureNameNotExists(parentName);
    await umbracoApi.document.ensureNameNotExists(rootName);
    await umbracoApi.documentType.ensureNameNotExists(docTypeName);

    // Create a doc type that allows as root and allows itself as child (self-referencing)
    const docTypeId = await umbracoApi.documentType.createDefaultDocumentTypeWithAllowAsRoot(docTypeName);
    const docType = await umbracoApi.documentType.get(docTypeId);
    docType.allowedDocumentTypes = [{documentType: {id: docTypeId}, sortOrder: 0}];
    await umbracoApi.put(umbracoApi.baseUrl + '/umbraco/management/api/v1/document-type/' + docTypeId, docType);

    // Create 3-level document tree
    const rootId = await umbracoApi.document.createDefaultDocument(rootName, docTypeId);
    const parentId = await umbracoApi.document.createDefaultDocumentWithParent(parentName, docTypeId, rootId);
    await umbracoApi.document.createDefaultDocumentWithParent(childName, docTypeId, parentId);
  });

  test.afterEach(async ({umbracoApi}) => {
    await umbracoApi.document.ensureNameNotExists(childName);
    await umbracoApi.document.ensureNameNotExists(parentName);
    await umbracoApi.document.ensureNameNotExists(rootName);
    await umbracoApi.documentType.ensureNameNotExists(docTypeName);
  });

  test('shows readable label and breadcrumb after visiting a nested document', {tag: '@smoke'}, async ({umbracoUi}) => {
    // Arrange
    await umbracoUi.goToBackOffice();
    await umbracoUi.content.goToSection(ConstantHelper.sections.content);

    // Act — navigate to the child document
    await umbracoUi.content.clickCaretButtonForContentName(rootName);
    await umbracoUi.content.clickCaretButtonForContentName(parentName);
    await umbracoUi.content.goToContentWithName(childName);
    await umbracoUi.waitForTimeout(ConstantHelper.wait.medium);

    // Open profile modal
    await umbracoUi.currentUserProfile.clickCurrentUserAvatarButton();

    // Assert — the entry's accessible text contains the name and parent breadcrumbs
    await umbracoUi.currentUserProfile.isHistoryEntryVisible(childName);
    const text = await umbracoUi.currentUserProfile.getHistoryEntryText(childName);
    expect(text).toContain(rootName);
    expect(text).toContain(parentName);
  });

  test('shows breadcrumb path at each nesting level', async ({umbracoUi}) => {
    // Arrange
    await umbracoUi.goToBackOffice();
    await umbracoUi.content.goToSection(ConstantHelper.sections.content);

    // Act — visit each level
    await umbracoUi.content.clickCaretButtonForContentName(rootName);
    await umbracoUi.content.goToContentWithName(rootName);
    await umbracoUi.waitForTimeout(ConstantHelper.wait.short);

    await umbracoUi.content.clickCaretButtonForContentName(parentName);
    await umbracoUi.content.goToContentWithName(parentName);
    await umbracoUi.waitForTimeout(ConstantHelper.wait.short);

    await umbracoUi.content.goToContentWithName(childName);
    await umbracoUi.waitForTimeout(ConstantHelper.wait.medium);

    // Open profile modal
    await umbracoUi.currentUserProfile.clickCurrentUserAvatarButton();

    // Assert — wait for entries to render, then check breadcrumb depth
    await umbracoUi.currentUserProfile.isHistoryEntryVisible(childName);

    const rootText = await umbracoUi.currentUserProfile.getHistoryEntryText(rootName);
    expect(rootText).toContain('Content');

    const parentText = await umbracoUi.currentUserProfile.getHistoryEntryText(parentName);
    expect(parentText).toContain(rootName);

    const childText = await umbracoUi.currentUserProfile.getHistoryEntryText(childName);
    expect(childText).toContain(rootName);
    expect(childText).toContain(parentName);
  });

  test('shows document type in history when visited', async ({umbracoUi}) => {
    // Arrange
    await umbracoUi.goToBackOffice();
    await umbracoUi.content.goToSection(ConstantHelper.sections.settings);

    // Act — navigate to the document type
    await umbracoUi.content.clickCaretButtonForContentName('Document Types');
    await umbracoUi.content.goToContentWithName(docTypeName);
    await umbracoUi.waitForTimeout(ConstantHelper.wait.medium);

    // Open profile modal
    await umbracoUi.currentUserProfile.clickCurrentUserAvatarButton();

    // Assert
    await umbracoUi.currentUserProfile.isHistoryEntryVisible(docTypeName);
    const text = await umbracoUi.currentUserProfile.getHistoryEntryText(docTypeName);
    expect(text).toContain('Document Types');
  });

  test('clicking a history entry navigates back to the document', async ({umbracoUi, page}) => {
    // Arrange — visit a document to create a history entry
    await umbracoUi.goToBackOffice();
    await umbracoUi.content.goToSection(ConstantHelper.sections.content);
    await umbracoUi.content.clickCaretButtonForContentName(rootName);
    await umbracoUi.content.goToContentWithName(rootName);
    await umbracoUi.waitForTimeout(ConstantHelper.wait.medium);

    // Navigate away to a different section
    await umbracoUi.content.goToSection(ConstantHelper.sections.settings);
    await umbracoUi.waitForTimeout(ConstantHelper.wait.short);

    // Act — open profile modal and click the history entry
    await umbracoUi.currentUserProfile.clickCurrentUserAvatarButton();
    await umbracoUi.currentUserProfile.isHistoryEntryVisible(rootName);
    const entry = umbracoUi.currentUserProfile.getHistoryEntryByName(rootName);
    await entry.click();
    await umbracoUi.waitForTimeout(ConstantHelper.wait.medium);

    // Assert — we're back on the document workspace
    expect(page.url()).toContain('/workspace/document/edit/');
  });

  test('document entry retains its label after navigating to another section', async ({umbracoUi}) => {
    // Arrange — visit a document
    await umbracoUi.goToBackOffice();
    await umbracoUi.content.goToSection(ConstantHelper.sections.content);
    await umbracoUi.content.clickCaretButtonForContentName(rootName);
    await umbracoUi.content.goToContentWithName(rootName);
    await umbracoUi.waitForTimeout(ConstantHelper.wait.medium);

    // Act — navigate to a different section
    await umbracoUi.content.goToSection(ConstantHelper.sections.settings);
    await umbracoUi.waitForTimeout(ConstantHelper.wait.medium);

    // Open profile modal
    await umbracoUi.currentUserProfile.clickCurrentUserAvatarButton();

    // Assert — the document entry still has its name, not just "Content"
    await umbracoUi.currentUserProfile.isHistoryEntryVisible(rootName);
    const text = await umbracoUi.currentUserProfile.getHistoryEntryText(rootName);
    expect(text).toContain('Content');
  });

  test('revisiting a document does not create duplicate history entries', async ({umbracoUi}) => {
    // Arrange
    await umbracoUi.goToBackOffice();
    await umbracoUi.content.goToSection(ConstantHelper.sections.content);
    await umbracoUi.content.clickCaretButtonForContentName(rootName);
    await umbracoUi.content.clickCaretButtonForContentName(parentName);

    // Act — visit child, then parent, then child again
    await umbracoUi.content.goToContentWithName(childName);
    await umbracoUi.waitForTimeout(ConstantHelper.wait.short);
    await umbracoUi.content.goToContentWithName(parentName);
    await umbracoUi.waitForTimeout(ConstantHelper.wait.short);
    await umbracoUi.content.goToContentWithName(childName);
    await umbracoUi.waitForTimeout(ConstantHelper.wait.medium);

    // Open profile modal
    await umbracoUi.currentUserProfile.clickCurrentUserAvatarButton();

    // Assert — entry exists and is not duplicated
    await umbracoUi.currentUserProfile.isHistoryEntryVisible(childName);
    const count = await umbracoUi.currentUserProfile.countHistoryEntriesWithName(childName);
    expect(count).toBe(1);
  });
});
