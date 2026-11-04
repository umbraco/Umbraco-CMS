import {ConstantHelper, test} from '@umbraco/acceptance-test-helpers';
import {expect} from '@playwright/test';

// Seeded by AdditionalSetup/SchemaLockdownComposer.cs
const memberGroupName = 'LockedMemberGroup';

test('blocks every change to a member group in the backoffice', async ({umbracoUi}) => {
  await umbracoUi.goToBackOffice();
  await umbracoUi.memberGroup.goToSection(ConstantHelper.sections.members);
  const sectionUrl = umbracoUi.page.url();
  await umbracoUi.memberGroup.clickMemberGroupsMenu();

  await test.step('Collection', async () => {
    await umbracoUi.memberGroup.isTextWithExactNameVisible(memberGroupName);
    await umbracoUi.memberGroup.isCollectionActionWithNameVisible('Create', false);
    await umbracoUi.memberGroup.isEntityActionVisibleInCollectionRowForName(memberGroupName, 'Delete', false);
  });

  await test.step('Root', async () => {
    // The root has no actions left to check first, so a sibling's action shows the sidebar's actions have rendered.
    await umbracoUi.memberGroup.isInlineEntityActionVisibleForName('Members', 'Create');
    await umbracoUi.memberGroup.isInlineEntityActionVisibleForName('Member Groups', 'Create', false);
  });

  await test.step('Workspace', async () => {
    await umbracoUi.memberGroup.clickMemberGroupLinkByName(memberGroupName);
    await umbracoUi.memberGroup.isReadOnlyTagVisible();
    await umbracoUi.schemaLockdown.isLockedWorkspaceActionVisible();
    await umbracoUi.memberGroup.isSaveButtonVisible(false);
    await umbracoUi.memberGroup.isWorkspaceNameEditable(false);
  });

  await test.step('Create route', async () => {
    await umbracoUi.page.goto(`${sectionUrl}/workspace/member-group/create`);
    await umbracoUi.memberGroup.isEntityDetailForbiddenVisible();
  });
});

test('blocks every change to a member group through the Management API', async ({umbracoApi}) => {
  // Arrange
  const memberGroup = await umbracoApi.memberGroup.getByName(memberGroupName);
  const url = `${umbracoApi.baseUrl}${ConstantHelper.apiEndpoints.memberGroup}`;

  // Act
  const createResponse = await umbracoApi.post(url, {});
  const updateResponse = await umbracoApi.put(`${url}/${memberGroup.id}`, memberGroup);
  const deleteResponse = await umbracoApi.delete(`${url}/${memberGroup.id}`);

  // Assert
  expect(createResponse.status()).toBe(403);
  expect(createResponse.headers()['umb-schema-lockdown']).toBe('member-group:create');
  expect(updateResponse.status()).toBe(403);
  expect(updateResponse.headers()['umb-schema-lockdown']).toBe('member-group:update');
  expect(deleteResponse.status()).toBe(403);
  expect(deleteResponse.headers()['umb-schema-lockdown']).toBe('member-group:delete');
});
