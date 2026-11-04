import {ConstantHelper, test} from '@umbraco/acceptance-test-helpers';
import {expect} from '@playwright/test';

// Seeded by AdditionalSetup/SchemaLockdownComposer.cs
const memberGroupName = 'LockedMemberGroup';

test('blocks updating a member group in the backoffice', async ({umbracoUi}) => {
  await umbracoUi.goToBackOffice();
  await umbracoUi.memberGroup.goToSection(ConstantHelper.sections.members);
  await umbracoUi.memberGroup.clickMemberGroupsMenu();

  await test.step('Collection', async () => {
    await umbracoUi.memberGroup.isCollectionActionWithNameVisible('Create');
    await umbracoUi.memberGroup.isEntityActionVisibleInCollectionRowForName(memberGroupName, 'Delete');
  });

  await test.step('Root', async () => {
    await umbracoUi.memberGroup.isInlineEntityActionVisibleForName('Member Groups', 'Create');
  });

  await test.step('Workspace', async () => {
    await umbracoUi.memberGroup.clickMemberGroupLinkByName(memberGroupName);
    await umbracoUi.memberGroup.isReadOnlyTagVisible();
    await umbracoUi.schemaLockdown.isLockedWorkspaceActionVisible();
    await umbracoUi.memberGroup.isSaveButtonVisible(false);
    await umbracoUi.memberGroup.isWorkspaceNameEditable(false);
    await umbracoUi.schemaLockdown.clickLockedWorkspaceActionButton();
    await umbracoUi.memberGroup.doesModalHaveText('Changes to this item are locked in this environment.');
  });
});

test('blocks updating a member group through the Management API', async ({umbracoApi}) => {
  // Arrange
  const memberGroup = await umbracoApi.memberGroup.getByName(memberGroupName);

  // Act
  const response = await umbracoApi.put(`${umbracoApi.baseUrl}${ConstantHelper.apiEndpoints.memberGroup}/${memberGroup.id}`, memberGroup);

  // Assert
  expect(response.status()).toBe(403);
  expect(response.headers()['umb-schema-lockdown']).toBe('member-group:update');
});
