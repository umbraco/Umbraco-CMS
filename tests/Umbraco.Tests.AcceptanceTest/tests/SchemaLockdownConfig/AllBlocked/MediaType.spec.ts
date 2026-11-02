import {ConstantHelper, test} from '@umbraco/acceptance-test-helpers';
import {expect} from '@playwright/test';

// Seeded by AdditionalSetup/SchemaLockdownComposer.cs
const mediaTypeName = 'LockedMediaType';
const mediaTypeFolderName = 'LockedMediaTypeFolder';

test('blocks every change to a media type in the backoffice', async ({umbracoUi}) => {
  await umbracoUi.goToBackOffice();
  await umbracoUi.mediaType.goToSection(ConstantHelper.sections.settings);
  const settingsSectionUrl = umbracoUi.page.url();

  await test.step('Tree', async () => {
    await umbracoUi.mediaType.isInlineEntityActionVisibleForName('Media Types', 'ReloadChildrenOf');
    await umbracoUi.mediaType.isSidebarActionsMenuVisibleForName('Media Types', false);
    await umbracoUi.mediaType.clickRootFolderCaretButton();
    await umbracoUi.mediaType.isInlineEntityActionVisibleForName(mediaTypeFolderName, 'ReloadChildrenOf');
    await umbracoUi.mediaType.isSidebarActionsMenuVisibleForName(mediaTypeFolderName, false);
    await umbracoUi.mediaType.clickActionsMenuForMediaType(mediaTypeName);
    await umbracoUi.mediaType.isEntityActionWithNameVisible('Export');
    await umbracoUi.mediaType.isEntityActionWithNameVisible('Create', false);
    await umbracoUi.mediaType.isEntityActionWithNameVisible('DuplicateTo', false);
    await umbracoUi.mediaType.isEntityActionWithNameVisible('MoveTo', false);
    await umbracoUi.mediaType.isEntityActionWithNameVisible('Delete', false);
    await umbracoUi.page.keyboard.press('Escape');
  });

  await test.step('Workspace', async () => {
    await umbracoUi.mediaType.goToMediaType(mediaTypeName);
    await umbracoUi.mediaType.isReadOnlyTagVisible();
    await umbracoUi.schemaLockdown.isLockedWorkspaceActionVisible();
    await umbracoUi.mediaType.isSaveButtonVisible(false);
    await umbracoUi.mediaType.isWorkspaceNameEditable(false);
    await umbracoUi.mediaType.isPropertyTypeEditable('LockedProperty', false);
    await umbracoUi.mediaType.isAddPropertyButtonVisible(false);
  });

  await test.step('Create route', async () => {
    await umbracoUi.page.goto(`${settingsSectionUrl}/workspace/media-type/create/parent/media-type-root/null`);
    await umbracoUi.mediaType.isEntityDetailForbiddenVisible();
  });
});

test('blocks every change to a media type through the Management API', async ({umbracoApi}) => {
  // Arrange
  const mediaType = await umbracoApi.mediaType.getByName(mediaTypeName);
  const url = `${umbracoApi.baseUrl}${ConstantHelper.apiEndpoints.mediaType}`;

  // Act
  const createResponse = await umbracoApi.post(url, {});
  const updateResponse = await umbracoApi.put(`${url}/${mediaType.id}`, mediaType);
  const deleteResponse = await umbracoApi.delete(`${url}/${mediaType.id}`);

  // Assert
  expect(createResponse.status()).toBe(403);
  expect(createResponse.headers()['umb-schema-lockdown']).toBe('media-type:create');
  expect(updateResponse.status()).toBe(403);
  expect(updateResponse.headers()['umb-schema-lockdown']).toBe('media-type:update');
  expect(deleteResponse.status()).toBe(403);
  expect(deleteResponse.headers()['umb-schema-lockdown']).toBe('media-type:delete');
});
