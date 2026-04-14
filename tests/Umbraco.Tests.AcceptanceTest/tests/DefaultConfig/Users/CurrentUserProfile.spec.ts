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
    await umbracoApi.documentType.ensureNameNotExists(docTypeName);
    await umbracoApi.document.ensureNameNotExists(childName);
    await umbracoApi.document.ensureNameNotExists(parentName);
    await umbracoApi.document.ensureNameNotExists(rootName);

    // Create a self-referencing doc type (allows itself as child + root)
    const docTypeId = await umbracoApi.documentType.createDefaultDocumentTypeWithAllowAsRoot(docTypeName);
    // Update to allow itself as a child document type
    const docType = await umbracoApi.documentType.get(docTypeId);
    docType.allowedDocumentTypes = [{id: docTypeId, sortOrder: 0}];
    await umbracoApi.documentType.update(docTypeId, docType);

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

  test('shows readable label and breadcrumb after visiting a nested document', async ({umbracoUi}) => {
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

    // Assert
    await umbracoUi.currentUserProfile.isHistoryEntryVisible(childName);
    const detail = await umbracoUi.currentUserProfile.getHistoryEntryDetail(childName);
    expect(detail).toContain(rootName);
    expect(detail).toContain(parentName);
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

    // Assert — each entry has the correct breadcrumb depth
    const rootDetail = await umbracoUi.currentUserProfile.getHistoryEntryDetail(rootName);
    expect(rootDetail).toContain('Content');

    const parentDetail = await umbracoUi.currentUserProfile.getHistoryEntryDetail(parentName);
    expect(parentDetail).toContain(rootName);

    const childDetail = await umbracoUi.currentUserProfile.getHistoryEntryDetail(childName);
    expect(childDetail).toContain(rootName);
    expect(childDetail).toContain(parentName);
  });

  test('tab switching does not create duplicate history entries', async ({umbracoUi}) => {
    // Arrange
    await umbracoUi.goToBackOffice();
    await umbracoUi.content.goToSection(ConstantHelper.sections.content);
    await umbracoUi.content.clickCaretButtonForContentName(rootName);
    await umbracoUi.content.clickCaretButtonForContentName(parentName);
    await umbracoUi.content.goToContentWithName(childName);
    await umbracoUi.waitForTimeout(ConstantHelper.wait.medium);

    // Act — switch to the Info tab
    await umbracoUi.content.clickInfoTab();
    await umbracoUi.waitForTimeout(ConstantHelper.wait.short);

    // Open profile modal
    await umbracoUi.currentUserProfile.clickCurrentUserAvatarButton();

    // Assert — only one entry for the child document
    const count = await umbracoUi.currentUserProfile.countHistoryEntriesWithName(childName);
    expect(count).toBe(1);
  });
});
