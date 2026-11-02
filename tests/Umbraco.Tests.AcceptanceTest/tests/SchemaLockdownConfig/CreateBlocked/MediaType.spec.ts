import {ConstantHelper, test} from '@umbraco/acceptance-test-helpers';
import {expect} from '@playwright/test';

// Seeded by AdditionalSetup/SchemaLockdownComposer.cs
const mediaTypeName = 'LockedMediaType';
const mediaTypeFolderName = 'LockedMediaTypeFolder';

test('blocks creating a media type in the backoffice', async ({umbracoUi}) => {
  await umbracoUi.goToBackOffice();
  await umbracoUi.mediaType.goToSection(ConstantHelper.sections.settings);
  const settingsSectionUrl = umbracoUi.page.url();

  await test.step('Tree', async () => {
    await umbracoUi.mediaType.isInlineEntityActionVisibleForName('Media Types', 'ReloadChildrenOf');
    await umbracoUi.mediaType.isSidebarActionsMenuVisibleForName('Media Types', false);
    await umbracoUi.mediaType.clickRootFolderCaretButton();
    await umbracoUi.mediaType.clickActionsMenuForMediaType(mediaTypeFolderName);
    await umbracoUi.mediaType.isEntityActionWithNameVisible('Delete');
    await umbracoUi.mediaType.isEntityActionWithNameVisible('Create', false);
    await umbracoUi.page.keyboard.press('Escape');
    await umbracoUi.mediaType.clickActionsMenuForMediaType(mediaTypeName);
    await umbracoUi.mediaType.isEntityActionWithNameVisible('Delete');
    await umbracoUi.mediaType.isEntityActionWithNameVisible('Create', false);
    await umbracoUi.mediaType.isEntityActionWithNameVisible('DuplicateTo', false);
    await umbracoUi.page.keyboard.press('Escape');
  });

  await test.step('Workspace', async () => {
    await umbracoUi.mediaType.goToMediaType(mediaTypeName);
    await umbracoUi.mediaType.isSaveButtonVisible();
    await umbracoUi.mediaType.isReadOnlyTagVisible(false);
    await umbracoUi.schemaLockdown.isLockedWorkspaceActionVisible(false);
    await umbracoUi.mediaType.isWorkspaceNameEditable();
    await umbracoUi.mediaType.isPropertyTypeEditable('LockedProperty');
  });

  await test.step('Create route', async () => {
    await umbracoUi.page.goto(`${settingsSectionUrl}/workspace/media-type/create/parent/media-type-root/null`);
    await umbracoUi.mediaType.isEntityDetailForbiddenVisible();
  });
});

test('blocks creating a media type through the Management API', async ({umbracoApi}) => {
  // Act
  const response = await umbracoApi.post(`${umbracoApi.baseUrl}${ConstantHelper.apiEndpoints.mediaType}`, {});

  // Assert
  expect(response.status()).toBe(403);
  expect(response.headers()['umb-schema-lockdown']).toBe('media-type:create');
});
