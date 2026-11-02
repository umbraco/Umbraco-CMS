import {ConstantHelper, test} from '@umbraco/acceptance-test-helpers';
import {expect} from '@playwright/test';

// Seeded by AdditionalSetup/SchemaLockdownComposer.cs
const memberGroupName = 'LockedMemberGroup';

test('blocks creating a member group in the backoffice', async ({umbracoUi}) => {
  await umbracoUi.goToBackOffice();
  await umbracoUi.memberGroup.goToSection(ConstantHelper.sections.members);
  const sectionUrl = umbracoUi.page.url();
  await umbracoUi.memberGroup.clickMemberGroupsMenu();

  await test.step('Collection', async () => {
    await umbracoUi.memberGroup.isEntityActionVisibleInCollectionRowForName(memberGroupName, 'Delete');
    await umbracoUi.memberGroup.isCollectionActionWithNameVisible('Create', false);
  });

  await test.step('Root', async () => {
    await umbracoUi.memberGroup.isInlineEntityActionVisibleForName('Member Groups', 'Create', false);
  });

  await test.step('Workspace', async () => {
    await umbracoUi.memberGroup.clickMemberGroupLinkByName(memberGroupName);
    await umbracoUi.memberGroup.isSaveButtonVisible();
    await umbracoUi.memberGroup.isReadOnlyTagVisible(false);
    await umbracoUi.schemaLockdown.isLockedWorkspaceActionVisible(false);
    await umbracoUi.memberGroup.isWorkspaceNameEditable();
  });

  await test.step('Create route', async () => {
    await umbracoUi.page.goto(`${sectionUrl}/workspace/member-group/create`);
    await umbracoUi.memberGroup.isEntityDetailForbiddenVisible();
  });
});

test('blocks creating a member group through the Management API', async ({umbracoApi}) => {
  // Act
  const response = await umbracoApi.post(`${umbracoApi.baseUrl}${ConstantHelper.apiEndpoints.memberGroup}`, {});

  // Assert
  expect(response.status()).toBe(403);
  expect(response.headers()['umb-schema-lockdown']).toBe('member-group:create');
});
