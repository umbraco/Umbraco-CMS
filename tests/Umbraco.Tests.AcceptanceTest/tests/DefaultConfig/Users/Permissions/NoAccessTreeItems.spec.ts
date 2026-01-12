import { expect } from "@playwright/test";
import { test, ConstantHelper } from "@umbraco/playwright-testhelpers";

const testUser = ConstantHelper.testUserCredentials;
let testUserCookieAndToken = { cookie: "", accessToken: "", refreshToken: "" };

const userGroupName = "TestUserGroup";
let userGroupId: string | null = null;

const rootDocumentTypeName = "RootDocumentType";
const childDocumentTypeName = "ChildDocumentType";
const grandchildDocumentTypeName = "GrandchildDocumentType";
let rootDocumentTypeId: string | null = null;
let childDocumentTypeId: string | null = null;
let grandchildDocumentTypeId: string | null = null;

const rootDocumentName = "RootDocument";
const childDocumentName = "ChildDocument";
const grandchildDocumentName = "GrandchildDocument";
let rootDocumentId: string | null = null;
let childDocumentId: string | null = null;
let grandchildDocumentId: string | null = null;

const rootMediaName = "RootMediaFolder";
const childMediaName = "ChildMediaFolder";
let rootMediaId: string | null = null;
let childMediaId: string | null = null;

test.beforeEach(async ({ umbracoApi }) => {
  // Clean up before each test
  await umbracoApi.documentType.ensureNameNotExists(rootDocumentTypeName);
  await umbracoApi.documentType.ensureNameNotExists(childDocumentTypeName);
  await umbracoApi.documentType.ensureNameNotExists(grandchildDocumentTypeName);
  await umbracoApi.media.ensureNameNotExists(rootMediaName);
  await umbracoApi.media.ensureNameNotExists(childMediaName);
  await umbracoApi.user.ensureNameNotExists(testUser.name);
  await umbracoApi.userGroup.ensureNameNotExists(userGroupName);

  // Create document types hierarchy with allowed child nodes
  grandchildDocumentTypeId =
    await umbracoApi.documentType.createDefaultDocumentType(
      grandchildDocumentTypeName
    );
  childDocumentTypeId =
    await umbracoApi.documentType.createDocumentTypeWithAllowedChildNode(
      childDocumentTypeName,
      grandchildDocumentTypeId!
    );
  rootDocumentTypeId =
    await umbracoApi.documentType.createDocumentTypeWithAllowedChildNode(
      rootDocumentTypeName,
      childDocumentTypeId!
    );

  // Create document hierarchy
  rootDocumentId = await umbracoApi.document.createDefaultDocument(
    rootDocumentName,
    rootDocumentTypeId!
  );
  childDocumentId = await umbracoApi.document.createDefaultDocumentWithParent(
    childDocumentName,
    childDocumentTypeId!,
    rootDocumentId!
  );
  grandchildDocumentId =
    await umbracoApi.document.createDefaultDocumentWithParent(
      grandchildDocumentName,
      grandchildDocumentTypeId!,
      childDocumentId!
    );

  // Create media hierarchy using folders (simpler approach without custom media types)
  rootMediaId = await umbracoApi.media.createDefaultMediaFolder(rootMediaName);
  childMediaId = await umbracoApi.media.createDefaultMediaFolderAndParentId(
    childMediaName,
    rootMediaId!
  );
});

test.afterEach(async ({ umbracoApi }) => {
  // Ensure we are logged in to admin for cleanup
  await umbracoApi.loginToAdminUser(
    testUserCookieAndToken.cookie,
    testUserCookieAndToken.accessToken,
    testUserCookieAndToken.refreshToken
  );

  // Clean up
  await umbracoApi.document.ensureNameNotExists(rootDocumentName);
  await umbracoApi.documentType.ensureNameNotExists(rootDocumentTypeName);
  await umbracoApi.documentType.ensureNameNotExists(childDocumentTypeName);
  await umbracoApi.documentType.ensureNameNotExists(grandchildDocumentTypeName);
  await umbracoApi.media.ensureNameNotExists(rootMediaName);
  await umbracoApi.media.ensureNameNotExists(childMediaName);
  await umbracoApi.userGroup.ensureNameNotExists(userGroupName);
});

test.describe("Document Tree NoAccess Property Tests", () => {
  test("should display noAccess ancestor node with reduced opacity and italic styling", async ({
    umbracoApi,
    umbracoUi,
    page,
  }) => {
    // Arrange - Create user with start node at child level (not at root)
    userGroupId =
      await umbracoApi.userGroup.createUserGroupWithDocumentStartNode(
        userGroupName,
        childDocumentId!
      );
    await umbracoApi.user.setUserPermissions(
      testUser.name,
      testUser.email,
      testUser.password,
      userGroupId!
    );
    testUserCookieAndToken = await umbracoApi.user.loginToUser(
      testUser.name,
      testUser.email,
      testUser.password
    );
    await umbracoUi.goToBackOffice();

    // Act - Navigate to content section
    await umbracoUi.userGroup.goToSection(
      ConstantHelper.sections.content,
      false
    );

    // Assert - Root document should be visible but styled as noAccess
    await umbracoUi.content.isContentInTreeVisible(rootDocumentName);

    // Get the tree item element for the root document (noAccess item)
    const rootTreeItem = page
      .locator(`umb-document-tree-item`)
      .filter({ hasText: rootDocumentName });

    // Verify the no-access attribute is present
    await expect(rootTreeItem).toHaveAttribute("no-access", "");

    // Verify cursor style
    await expect(rootTreeItem).toHaveCSS("cursor", "not-allowed");

    // Verify label has reduced opacity and italic style
    const label = rootTreeItem.locator("#label");
    await expect(label).toHaveCSS("opacity", "0.6");
    await expect(label).toHaveCSS("font-style", "italic");

    // Verify icon has reduced opacity (target only the main icon, not state or action icons)
    const icon = rootTreeItem.locator("#icon-container umb-icon").first();
    await expect(icon).toHaveCSS("opacity", "0.6");
  });

  test("should prevent navigation when clicking noAccess ancestor node", async ({
    umbracoApi,
    umbracoUi,
    page,
  }) => {
    // Arrange - Create user with start node at child level (not at root)
    userGroupId =
      await umbracoApi.userGroup.createUserGroupWithDocumentStartNode(
        userGroupName,
        childDocumentId!
      );
    await umbracoApi.user.setUserPermissions(
      testUser.name,
      testUser.email,
      testUser.password,
      userGroupId!
    );
    testUserCookieAndToken = await umbracoApi.user.loginToUser(
      testUser.name,
      testUser.email,
      testUser.password
    );
    await umbracoUi.goToBackOffice();

    // Act - Navigate to content section
    await umbracoUi.userGroup.goToSection(
      ConstantHelper.sections.content,
      false
    );

    // Wait for content tree to load
    await umbracoUi.content.isContentInTreeVisible(rootDocumentName);

    // Get initial URL (should be on content section)
    const initialUrl = page.url();

    // Click on the noAccess root document tree item
    const rootTreeItem = page
      .locator(`umb-document-tree-item`)
      .filter({ hasText: rootDocumentName });
    await rootTreeItem.click();

    // Wait a moment to ensure no navigation occurs
    await page.waitForTimeout(1000);

    // Assert - URL should not have changed (no navigation occurred)
    const currentUrl = page.url();
    expect(currentUrl).toBe(initialUrl);

    // Verify no workspace opened by checking we're still on the content tree view
    await expect(page.locator("umb-body-layout")).toBeVisible();

    // Verify that the document workspace did not open (there should be no editor visible)
    const workspaceEditor = page.locator('[alias="Umb.Workspace.Document"]');
    await expect(workspaceEditor).not.toBeVisible();
  });

  test("should allow expansion of noAccess ancestor node to show children", async ({
    umbracoApi,
    umbracoUi,
    page,
  }) => {
    // Arrange - Create user with start node at grandchild level
    userGroupId =
      await umbracoApi.userGroup.createUserGroupWithDocumentStartNode(
        userGroupName,
        grandchildDocumentId!
      );
    await umbracoApi.user.setUserPermissions(
      testUser.name,
      testUser.email,
      testUser.password,
      userGroupId!
    );
    testUserCookieAndToken = await umbracoApi.user.loginToUser(
      testUser.name,
      testUser.email,
      testUser.password
    );
    await umbracoUi.goToBackOffice();

    // Act - Navigate to content section
    await umbracoUi.userGroup.goToSection(
      ConstantHelper.sections.content,
      false
    );

    // Wait for root document to be visible
    await umbracoUi.content.isContentInTreeVisible(rootDocumentName);

    // Get the root tree item (noAccess)
    const rootTreeItem = page
      .locator(`umb-document-tree-item`)
      .filter({ hasText: rootDocumentName })
      .first();

    // Verify it has the no-access attribute
    await expect(rootTreeItem).toHaveAttribute("no-access", "");

    // Click the expand/caret button on the noAccess root item
    await umbracoUi.content.openContentCaretButtonForName(rootDocumentName);

    // Assert - Child document should now be visible
    await umbracoUi.content.isChildContentInTreeVisible(
      rootDocumentName,
      childDocumentName
    );

    // Verify the child also has noAccess styling (since start node is grandchild)
    const childTreeItem = page
      .locator(`umb-document-tree-item`)
      .filter({ hasText: childDocumentName })
      .first();
    await expect(childTreeItem).toHaveAttribute("no-access", "");

    // Expand the child to show grandchild
    await umbracoUi.content.openContentCaretButtonForName(childDocumentName);

    // Assert - Grandchild should be visible and accessible (NOT noAccess)
    await umbracoUi.content.isChildContentInTreeVisible(
      childDocumentName,
      grandchildDocumentName
    );

    // Verify the grandchild does NOT have noAccess attribute (it's the start node)
    const grandchildTreeItem = page
      .locator(`umb-document-tree-item`)
      .filter({ hasText: grandchildDocumentName })
      .first();
    await expect(grandchildTreeItem).not.toHaveAttribute("no-access", "");
  });

  // TODO: Re-enable once picker modal infrastructure is more stable
  test.skip("should filter noAccess items from document picker", async ({
    umbracoApi,
    umbracoUi,
    page,
  }) => {
    // Arrange - Create a document type with a document picker property
    const pickerDocumentTypeName = "PickerDocumentType";
    const pickerDocumentName = "PickerDocument";
    const propertyName = "DocumentPicker";

    await umbracoApi.documentType.ensureNameNotExists(pickerDocumentTypeName);

    // Get the Content Picker data type
    const contentPickerDataType = await umbracoApi.dataType.getByName(
      "Content Picker"
    );

    // Create document type with content picker
    const pickerDocumentTypeId =
      await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(
        pickerDocumentTypeName,
        propertyName,
        contentPickerDataType!.id
      );

    // Create a document at the child level with picker property
    const pickerDocumentId =
      await umbracoApi.document.createDefaultDocumentWithParent(
        pickerDocumentName,
        pickerDocumentTypeId!,
        childDocumentId!
      );

    // Create user with start node at child level (root will be noAccess)
    userGroupId =
      (await umbracoApi.userGroup.createUserGroupWithDocumentStartNode(
        userGroupName,
        childDocumentId!
      )) ?? null;
    await umbracoApi.user.setUserPermissions(
      testUser.name,
      testUser.email,
      testUser.password,
      userGroupId!
    );
    testUserCookieAndToken = (await umbracoApi.user.loginToUser(
      testUser.name,
      testUser.email,
      testUser.password
    ))!;
    await umbracoUi.goToBackOffice();

    // Act - Navigate to content section and open the picker document
    await umbracoUi.userGroup.goToSection(
      ConstantHelper.sections.content,
      false
    );
    await umbracoUi.content.goToContentWithName(pickerDocumentName);

    // Open the document picker
    const addButton = page.locator(
      'uui-ref-node-document-type[name="' + propertyName + '"] uui-button'
    );
    await addButton.click();

    // Wait for picker modal to open
    const pickerModal = page.locator("umb-document-picker-modal");
    await expect(pickerModal).toBeVisible();

    // Assert - Root document (noAccess) should be visible in the tree but NOT selectable
    const rootInPicker = pickerModal
      .locator(`umb-document-tree-item`)
      .filter({ hasText: rootDocumentName })
      .first();
    await expect(rootInPicker).toBeVisible();

    // Verify the tree item has no-access attribute in picker
    await expect(rootInPicker).toHaveAttribute("no-access", "");

    // Try to click on the noAccess item - it should not become selected
    await rootInPicker.click();

    // Wait a moment
    await page.waitForTimeout(500);

    // The submit button should still be disabled (nothing selected)
    const submitButton = pickerModal.locator(
      'uui-button[type="button"][look="primary"]'
    );
    await expect(submitButton).toBeDisabled();

    // Now expand the root and try to select the accessible child
    const expandButton = rootInPicker.locator("uui-symbol-expand");
    await expandButton.click();

    // Wait for child to be visible
    const childInPicker = pickerModal
      .locator(`umb-document-tree-item`)
      .filter({ hasText: childDocumentName });
    await expect(childInPicker.first()).toBeVisible();

    // Verify child does NOT have no-access attribute (it's the start node)
    await expect(childInPicker.first()).not.toHaveAttribute("no-access", "");

    // Click on the accessible child - it should be selectable
    await childInPicker.first().click();

    // Wait for selection
    await page.waitForTimeout(500);

    // Submit button should now be enabled
    await expect(submitButton).toBeEnabled();

    // Clean up
    await umbracoApi.document.ensureNameNotExists(pickerDocumentName);
    await umbracoApi.documentType.ensureNameNotExists(pickerDocumentTypeName);
  });
});

test.describe("Media Tree NoAccess Property Tests", () => {
  test("should display noAccess ancestor media node with reduced opacity and italic styling", async ({
    umbracoApi,
    umbracoUi,
    page,
  }) => {
    // Arrange - Create user with media start node at child level (not at root)
    userGroupId = (await umbracoApi.userGroup.createUserGroupWithMediaStartNode(
      userGroupName,
      childMediaId!
    )) ?? null;
    await umbracoApi.user.setUserPermissions(
      testUser.name,
      testUser.email,
      testUser.password,
      userGroupId!
    );
    testUserCookieAndToken = (await umbracoApi.user.loginToUser(
      testUser.name,
      testUser.email,
      testUser.password
    ))!;
    await umbracoUi.goToBackOffice();

    // Act - Navigate to media section
    await umbracoUi.userGroup.goToSection(ConstantHelper.sections.media, false);

    // Assert - Root media should be visible but styled as noAccess
    await umbracoUi.media.isMediaTreeItemVisible(rootMediaName);

    // Get the tree item element for the root media (noAccess item)
    const rootMediaTreeItem = page
      .locator(`umb-media-tree-item`)
      .filter({ hasText: rootMediaName });

    // Verify the no-access attribute is present
    await expect(rootMediaTreeItem).toHaveAttribute("no-access", "");

    // Verify cursor style
    await expect(rootMediaTreeItem).toHaveCSS("cursor", "not-allowed");

    // Verify label has reduced opacity and italic style
    const label = rootMediaTreeItem.locator("#label");
    await expect(label).toHaveCSS("opacity", "0.6");
    await expect(label).toHaveCSS("font-style", "italic");

    // Verify icon has reduced opacity (target only the main icon, not state or action icons)
    const icon = rootMediaTreeItem.locator("#icon-container umb-icon").first();
    await expect(icon).toHaveCSS("opacity", "0.6");
  });

  test("should prevent navigation when clicking noAccess ancestor media node", async ({
    umbracoApi,
    umbracoUi,
    page,
  }) => {
    // Arrange - Create user with media start node at child level (not at root)
    userGroupId = (await umbracoApi.userGroup.createUserGroupWithMediaStartNode(
      userGroupName,
      childMediaId!
    )) ?? null;
    await umbracoApi.user.setUserPermissions(
      testUser.name,
      testUser.email,
      testUser.password,
      userGroupId!
    );
    testUserCookieAndToken = (await umbracoApi.user.loginToUser(
      testUser.name,
      testUser.email,
      testUser.password
    ))!;
    await umbracoUi.goToBackOffice();

    // Act - Navigate to media section
    await umbracoUi.userGroup.goToSection(ConstantHelper.sections.media, false);

    // Wait for media tree to load
    await umbracoUi.media.isMediaTreeItemVisible(rootMediaName);

    // Get initial URL (should be on media section)
    const initialUrl = page.url();

    // Click on the noAccess root media tree item
    const rootMediaTreeItem = page
      .locator(`umb-media-tree-item`)
      .filter({ hasText: rootMediaName });
    await rootMediaTreeItem.click();

    // Wait a moment to ensure no navigation occurs
    await page.waitForTimeout(1000);

    // Assert - URL should not have changed (no navigation occurred)
    const currentUrl = page.url();
    expect(currentUrl).toBe(initialUrl);

    // Verify that the media workspace did not open
    const workspaceEditor = page.locator('[alias="Umb.Workspace.Media"]');
    await expect(workspaceEditor).not.toBeVisible();
  });

  test("should allow expansion of noAccess ancestor media node to show children", async ({
    umbracoApi,
    umbracoUi,
    page,
  }) => {
    // Arrange - Create user with media start node at child level
    userGroupId = (await umbracoApi.userGroup.createUserGroupWithMediaStartNode(
      userGroupName,
      childMediaId!
    )) ?? null;
    await umbracoApi.user.setUserPermissions(
      testUser.name,
      testUser.email,
      testUser.password,
      userGroupId!
    );
    testUserCookieAndToken = (await umbracoApi.user.loginToUser(
      testUser.name,
      testUser.email,
      testUser.password
    ))!;
    await umbracoUi.goToBackOffice();

    // Act - Navigate to media section
    await umbracoUi.userGroup.goToSection(ConstantHelper.sections.media, false);

    // Wait for root media to be visible
    await umbracoUi.media.isMediaTreeItemVisible(rootMediaName);

    // Get the root media tree item (noAccess)
    const rootMediaTreeItem = page
      .locator(`umb-media-tree-item`)
      .filter({ hasText: rootMediaName })
      .first();

    // Verify it has the no-access attribute
    await expect(rootMediaTreeItem).toHaveAttribute("no-access", "");

    // Click the expand/caret button on the noAccess root media item
    await umbracoUi.media.openMediaCaretButtonForName(rootMediaName);

    // Assert - Child media should now be visible
    await umbracoUi.media.isChildMediaVisible(
      rootMediaName,
      childMediaName
    );

    // Verify the child does NOT have noAccess attribute (it's the start node)
    const childMediaTreeItem = page
      .locator(`umb-media-tree-item`)
      .filter({ hasText: childMediaName })
      .first();
    await expect(childMediaTreeItem).not.toHaveAttribute("no-access", "");
  });

  // TODO: Re-enable once picker modal infrastructure is more stable
  test.skip("should filter noAccess items from media picker", async ({
    umbracoApi,
    umbracoUi,
    page,
  }) => {
    // Arrange - Create a document type with a media picker property
    const pickerDocumentTypeName = "MediaPickerDocumentType";
    const pickerDocumentName = "MediaPickerDocument";
    const propertyName = "MediaPicker";

    await umbracoApi.documentType.ensureNameNotExists(pickerDocumentTypeName);

    // Get the Media Picker data type
    const mediaPickerDataType = await umbracoApi.dataType.getByName(
      "Media Picker"
    );

    // Create document type with media picker
    const pickerDocumentTypeId =
      await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(
        pickerDocumentTypeName,
        propertyName,
        mediaPickerDataType!.id
      );

    // Create a document with media picker property
    await umbracoApi.document.createDefaultDocument(
      pickerDocumentName,
      pickerDocumentTypeId!
    );

    // Create user with media start node at child level (root will be noAccess)
    userGroupId = (await umbracoApi.userGroup.createUserGroupWithMediaStartNode(
      userGroupName,
      childMediaId!
    )) ?? null;
    await umbracoApi.user.setUserPermissions(
      testUser.name,
      testUser.email,
      testUser.password,
      userGroupId!
    );
    testUserCookieAndToken = (await umbracoApi.user.loginToUser(
      testUser.name,
      testUser.email,
      testUser.password
    ))!;
    await umbracoUi.goToBackOffice();

    // Act - Navigate to content section and open the document with media picker
    await umbracoUi.userGroup.goToSection(
      ConstantHelper.sections.content,
      false
    );
    await umbracoUi.content.goToContentWithName(pickerDocumentName);

    // Open the media picker
    const addButton = page.locator(
      'umb-input-media[alias="' + propertyName + '"] uui-button[label="Add"]'
    );
    await addButton.click();

    // Wait for picker modal to open
    const pickerModal = page.locator("umb-media-picker-modal");
    await expect(pickerModal).toBeVisible();

    // Assert - Root media (noAccess) should be visible in the tree but NOT selectable
    const rootMediaInPicker = pickerModal
      .locator(`umb-media-tree-item`)
      .filter({ hasText: rootMediaName })
      .first();
    await expect(rootMediaInPicker).toBeVisible();

    // Verify the tree item has no-access attribute in picker
    await expect(rootMediaInPicker).toHaveAttribute("no-access", "");

    // Try to click on the noAccess item - it should not become selected
    await rootMediaInPicker.click();

    // Wait a moment
    await page.waitForTimeout(500);

    // The submit button should still be disabled (nothing selected)
    const submitButton = pickerModal.locator(
      'uui-button[type="button"][look="primary"]'
    );
    await expect(submitButton).toBeDisabled();

    // Now expand the root and try to select the accessible child
    const expandButton = rootMediaInPicker.locator("uui-symbol-expand");
    await expandButton.click();

    // Wait for child to be visible
    const childMediaInPicker = pickerModal
      .locator(`umb-media-tree-item`)
      .filter({ hasText: childMediaName });
    await expect(childMediaInPicker.first()).toBeVisible();

    // Verify child does NOT have no-access attribute (it's the start node)
    await expect(childMediaInPicker.first()).not.toHaveAttribute(
      "no-access",
      ""
    );

    // Click on the accessible child - it should be selectable
    await childMediaInPicker.first().click();

    // Wait for selection
    await page.waitForTimeout(500);

    // Submit button should now be enabled
    await expect(submitButton).toBeEnabled();

    // Clean up
    await umbracoApi.document.ensureNameNotExists(pickerDocumentName);
    await umbracoApi.documentType.ensureNameNotExists(pickerDocumentTypeName);
  });
});
