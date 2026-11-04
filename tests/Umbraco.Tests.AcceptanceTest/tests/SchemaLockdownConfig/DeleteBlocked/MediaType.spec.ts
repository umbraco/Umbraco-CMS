import {ConstantHelper, test} from '@umbraco/acceptance-test-helpers';
import {expect} from '@playwright/test';

// Seeded by AdditionalSetup/SchemaLockdownComposer.cs
const mediaTypeName = 'LockedMediaType';
const mediaTypeFolderName = 'LockedMediaTypeFolder';

test('blocks deleting a media type in the backoffice', async ({umbracoUi}) => {
  await umbracoUi.goToBackOffice();
  await umbracoUi.mediaType.goToSection(ConstantHelper.sections.settings);

  await test.step('Tree', async () => {
    await umbracoUi.mediaType.clickActionsMenuAtRoot();
    await umbracoUi.mediaType.isEntityActionWithNameVisible('Create');
    await umbracoUi.page.keyboard.press('Escape');
    await umbracoUi.mediaType.clickRootFolderCaretButton();
    await umbracoUi.mediaType.clickActionsMenuForMediaType(mediaTypeFolderName);
    await umbracoUi.mediaType.isEntityActionWithNameVisible('Folder.Update');
    await umbracoUi.mediaType.isEntityActionWithNameVisible('Delete', false);
    await umbracoUi.page.keyboard.press('Escape');
    await umbracoUi.mediaType.clickActionsMenuForMediaType(mediaTypeName);
    await umbracoUi.mediaType.isEntityActionWithNameVisible('MoveTo');
    await umbracoUi.mediaType.isEntityActionWithNameVisible('Delete', false);
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
});

test('blocks deleting a media type through the Management API', async ({umbracoApi}) => {
  // Arrange
  const mediaType = await umbracoApi.mediaType.getByName(mediaTypeName);

  // Act
  const response = await umbracoApi.delete(`${umbracoApi.baseUrl}${ConstantHelper.apiEndpoints.mediaType}/${mediaType.id}`);

  // Assert
  expect(response.status()).toBe(403);
  expect(response.headers()['umb-schema-lockdown']).toBe('media-type:delete');
});
