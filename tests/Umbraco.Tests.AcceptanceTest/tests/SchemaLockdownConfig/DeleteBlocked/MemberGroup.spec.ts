import {ConstantHelper, test} from '@umbraco/acceptance-test-helpers';
import {expect} from '@playwright/test';

// Seeded by AdditionalSetup/SchemaLockdownComposer.cs
const memberGroupName = 'LockedMemberGroup';

test('blocks deleting a member group in the backoffice', async ({umbracoUi}) => {
  await umbracoUi.goToBackOffice();
  await umbracoUi.memberGroup.goToSection(ConstantHelper.sections.members);
  await umbracoUi.memberGroup.clickMemberGroupsMenu();

  await test.step('Collection', async () => {
    await umbracoUi.memberGroup.isCollectionActionWithNameVisible('Create');
    await umbracoUi.memberGroup.isEntityActionVisibleInCollectionRowForName(memberGroupName, 'Delete', false);
  });

  await test.step('Workspace', async () => {
    await umbracoUi.memberGroup.clickMemberGroupLinkByName(memberGroupName);
    await umbracoUi.memberGroup.isSaveButtonVisible();
    await umbracoUi.memberGroup.isReadOnlyTagVisible(false);
    await umbracoUi.schemaLockdown.isLockedWorkspaceActionVisible(false);
    await umbracoUi.memberGroup.isWorkspaceNameEditable();
  });
});

test('blocks deleting a member group through the Management API', async ({umbracoApi}) => {
  // Arrange
  const memberGroup = await umbracoApi.memberGroup.getByName(memberGroupName);

  // Act
  const response = await umbracoApi.delete(`${umbracoApi.baseUrl}${ConstantHelper.apiEndpoints.memberGroup}/${memberGroup.id}`);

  // Assert
  expect(response.status()).toBe(403);
  expect(response.headers()['umb-schema-lockdown']).toBe('member-group:delete');
});
