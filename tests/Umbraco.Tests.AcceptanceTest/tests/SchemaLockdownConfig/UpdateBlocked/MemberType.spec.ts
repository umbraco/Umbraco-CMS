import {ConstantHelper, test} from '@umbraco/acceptance-test-helpers';
import {expect} from '@playwright/test';

// Seeded by AdditionalSetup/SchemaLockdownComposer.cs
const memberTypeName = 'LockedMemberType';
const memberTypeFolderName = 'LockedMemberTypeFolder';

test('blocks updating a member type in the backoffice', async ({umbracoUi}) => {
  await umbracoUi.goToBackOffice();
  await umbracoUi.memberType.goToSection(ConstantHelper.sections.settings);

  await test.step('Tree', async () => {
    await umbracoUi.memberType.clickActionsMenuAtRoot();
    await umbracoUi.memberType.isEntityActionWithNameVisible('Create');
    await umbracoUi.page.keyboard.press('Escape');
    await umbracoUi.memberType.clickRootFolderCaretButton();
    await umbracoUi.memberType.clickActionsMenuForMemberType(memberTypeFolderName);
    await umbracoUi.memberType.isEntityActionWithNameVisible('Delete');
    await umbracoUi.memberType.isEntityActionWithNameVisible('Folder.Update', false);
    await umbracoUi.page.keyboard.press('Escape');
    await umbracoUi.memberType.clickActionsMenuForMemberType(memberTypeName);
    await umbracoUi.memberType.isEntityActionWithNameVisible('Delete');
    await umbracoUi.memberType.isEntityActionWithNameVisible('MoveTo', false);
    await umbracoUi.page.keyboard.press('Escape');
  });

  await test.step('Workspace', async () => {
    await umbracoUi.memberType.goToMemberType(memberTypeName);
    await umbracoUi.memberType.isReadOnlyTagVisible();
    await umbracoUi.schemaLockdown.isLockedWorkspaceActionVisible();
    await umbracoUi.memberType.isSaveButtonVisible(false);
    await umbracoUi.memberType.isWorkspaceNameEditable(false);
    await umbracoUi.memberType.isPropertyTypeEditable('LockedProperty', false);
    await umbracoUi.memberType.isAddPropertyButtonVisible(false);
    await umbracoUi.schemaLockdown.clickLockedWorkspaceActionButton();
    await umbracoUi.memberType.doesModalHaveText('Changes to this item are locked in this environment.');
  });
});

test('blocks updating a member type through the Management API', async ({umbracoApi}) => {
  // Arrange
  const memberType = await umbracoApi.memberType.getByName(memberTypeName);

  // Act
  const response = await umbracoApi.put(`${umbracoApi.baseUrl}${ConstantHelper.apiEndpoints.memberType}/${memberType.id}`, memberType);

  // Assert
  expect(response.status()).toBe(403);
  expect(response.headers()['umb-schema-lockdown']).toBe('member-type:update');
});
