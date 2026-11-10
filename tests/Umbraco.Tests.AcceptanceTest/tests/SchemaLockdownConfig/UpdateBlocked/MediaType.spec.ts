import {ConstantHelper, test} from '@umbraco/acceptance-test-helpers';
import {expect} from '@playwright/test';

// Seeded by AdditionalSetup/SchemaLockdownComposer.cs
const mediaTypeName = 'LockedMediaType';
const mediaTypeFolderName = 'LockedMediaTypeFolder';

test('blocks updating a media type in the backoffice', async ({umbracoUi}) => {
  await umbracoUi.goToBackOffice();
  await umbracoUi.mediaType.goToSection(ConstantHelper.sections.settings);

  await test.step('Tree', async () => {
    await umbracoUi.mediaType.clickActionsMenuAtRoot();
    await umbracoUi.mediaType.isEntityActionWithNameVisible('Create');
    await umbracoUi.page.keyboard.press('Escape');
    await umbracoUi.mediaType.clickRootFolderCaretButton();
    await umbracoUi.mediaType.clickActionsMenuForMediaType(mediaTypeFolderName);
    await umbracoUi.mediaType.isEntityActionWithNameVisible('Delete');
    await umbracoUi.mediaType.isEntityActionWithNameVisible('Folder.Update', false);
    await umbracoUi.page.keyboard.press('Escape');
    await umbracoUi.mediaType.clickActionsMenuForMediaType(mediaTypeName);
    await umbracoUi.mediaType.isEntityActionWithNameVisible('Delete');
    await umbracoUi.mediaType.isEntityActionWithNameVisible('MoveTo', false);
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
    await umbracoUi.schemaLockdown.clickLockedWorkspaceActionButton();
    await umbracoUi.mediaType.doesModalHaveText('Changes to this item are locked in this environment.');
  });
});

test('blocks updating a media type through the Management API', async ({umbracoApi}) => {
  // Arrange
  const mediaType = await umbracoApi.mediaType.getByName(mediaTypeName);

  // Act
  const response = await umbracoApi.put(`${umbracoApi.baseUrl}${ConstantHelper.apiEndpoints.mediaType}/${mediaType.id}`, mediaType);

  // Assert
  expect(response.status()).toBe(403);
  expect(response.headers()['umb-schema-lockdown']).toBe('media-type:update');
});
