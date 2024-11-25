import {expect} from "@playwright/test";
import {AliasHelper, ConstantHelper, NotificationConstantHelper, test} from '@umbraco/playwright-testhelpers';

const mediaTypeName = 'TestMediaType';

test.beforeEach(async ({umbracoUi, umbracoApi}) => {
  await umbracoApi.mediaType.ensureNameNotExists(mediaTypeName);
  await umbracoUi.goToBackOffice();
  await umbracoUi.mediaType.goToSection(ConstantHelper.sections.settings);
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.mediaType.ensureNameNotExists(mediaTypeName);
});

test('can create a media type', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Act
  await umbracoUi.mediaType.clickActionsMenuForName('Media Types');
  await umbracoUi.mediaType.clickCreateButton();
  await umbracoUi.mediaType.clickNewMediaTypeButton();
  await umbracoUi.mediaType.enterMediaTypeName(mediaTypeName);
  await umbracoUi.mediaType.clickSaveButton();

  // Assert
  await umbracoUi.mediaType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.created);
  expect(await umbracoApi.mediaType.doesNameExist(mediaTypeName)).toBeTruthy();
});

test('can rename a media type', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const wrongName = 'NotAMediaTypeName';
  await umbracoApi.mediaType.ensureNameNotExists(wrongName);
  await umbracoApi.mediaType.createDefaultMediaType(wrongName);

  // Act
  await umbracoUi.mediaType.goToMediaType(wrongName);
  await umbracoUi.mediaType.enterMediaTypeName(mediaTypeName);
  await umbracoUi.mediaType.clickSaveButton();

  // Assert
  await umbracoUi.mediaType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  expect(await umbracoApi.mediaType.doesNameExist(mediaTypeName)).toBeTruthy();
});

test('can update the alias for a media type', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const oldAlias = AliasHelper.toAlias(mediaTypeName);
  const updatedAlias = 'TestMediaAlias';
  await umbracoApi.mediaType.createDefaultMediaType(mediaTypeName);
  const mediaTypeDataOld = await umbracoApi.mediaType.getByName(mediaTypeName);
  expect(mediaTypeDataOld.alias).toBe(oldAlias);

  // Act
  await umbracoUi.mediaType.goToMediaType(mediaTypeName);
  await umbracoUi.mediaType.enterAliasName(updatedAlias);
  await umbracoUi.mediaType.clickSaveButton();

  // Assert
  await umbracoUi.mediaType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  const mediaTypeData = await umbracoApi.mediaType.getByName(mediaTypeName);
  expect(mediaTypeData.alias).toBe(updatedAlias);
});

test('can add an icon for a media type', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const bugIcon = 'icon-bug';
  await umbracoApi.mediaType.createDefaultMediaType(mediaTypeName);

  // Act
  await umbracoUi.mediaType.goToMediaType(mediaTypeName);
  await umbracoUi.waitForTimeout(500);
  await umbracoUi.mediaType.updateIcon(bugIcon);
  await umbracoUi.mediaType.clickSaveButton();

  // Assert
  await umbracoUi.mediaType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  const mediaTypeData = await umbracoApi.mediaType.getByName(mediaTypeName);
  expect(mediaTypeData.icon).toBe(bugIcon);
  await umbracoUi.mediaType.isTreeItemVisible(mediaTypeName, true);
});

test('can delete a media type', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.mediaType.createDefaultMediaType(mediaTypeName);

  // Act
  await umbracoUi.mediaType.clickRootFolderCaretButton();
  await umbracoUi.mediaType.clickActionsMenuForName(mediaTypeName);
  await umbracoUi.mediaType.clickDeleteButton();
  await umbracoUi.mediaType.clickConfirmToDeleteButton();

  // Assert
  await umbracoUi.mediaType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.deleted);
  expect(await umbracoApi.mediaType.doesNameExist(mediaTypeName)).toBeFalsy();
});
