import {ConstantHelper, NotificationConstantHelper, test} from '@umbraco/acceptance-test-helpers';
import {expect} from "@playwright/test";

const documentBlueprintFolderName = 'TestDocumentBlueprintFolder';
const documentBlueprintTargetFolderName = 'TestDocumentBlueprintTargetFolder';

test.beforeEach(async ({umbracoApi, umbracoUi}) => {
  await umbracoApi.documentBlueprint.ensureNameNotExists(documentBlueprintTargetFolderName);
  await umbracoApi.documentBlueprint.ensureNameNotExists(documentBlueprintFolderName);
  await umbracoUi.goToBackOffice();
  await umbracoUi.documentBlueprint.goToSection(ConstantHelper.sections.settings);
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.documentBlueprint.ensureNameNotExists(documentBlueprintTargetFolderName);
  await umbracoApi.documentBlueprint.ensureNameNotExists(documentBlueprintFolderName);
});

test('can move a document blueprint folder to another document blueprint folder', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const folderId = await umbracoApi.documentBlueprint.createFolder(documentBlueprintFolderName);
  const targetFolderId = await umbracoApi.documentBlueprint.createFolder(documentBlueprintTargetFolderName);

  // Act
  await umbracoUi.documentBlueprint.clickRootFolderCaretButton();
  await umbracoUi.documentBlueprint.clickActionsMenuForDocumentBlueprints(documentBlueprintFolderName);
  await umbracoUi.documentBlueprint.moveToFolder(documentBlueprintTargetFolderName);

  // Assert
  await umbracoUi.documentBlueprint.doesSuccessNotificationHaveText(NotificationConstantHelper.success.moved);
  const targetFolderChildren = await umbracoApi.documentBlueprint.getChildren(targetFolderId);
  expect(targetFolderChildren.some((child) => child.id === folderId)).toBeTruthy();
});
