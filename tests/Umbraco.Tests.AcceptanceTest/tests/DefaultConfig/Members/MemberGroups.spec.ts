import {ConstantHelper, NotificationConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

const memberGroupName = 'Test Member Group';

test.beforeEach(async ({umbracoApi, umbracoUi}) => {
  await umbracoApi.memberGroup.ensureNameNotExists(memberGroupName);
  await umbracoUi.goToBackOffice();
  await umbracoUi.memberGroup.goToMemberGroups();
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.memberGroup.ensureNameNotExists(memberGroupName);
});

test('can create a member group', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Act
  await umbracoUi.memberGroup.clickMemberGroupCreateButton();
  await umbracoUi.memberGroup.enterMemberGroupName(memberGroupName);
  await umbracoUi.memberGroup.clickSaveButtonAndWaitForMemberGroupToBeCreated();

  // Assert
  await umbracoUi.memberGroup.clickMemberGroupsSidebarButton();
  await umbracoUi.memberGroup.isMemberGroupNameVisible(memberGroupName);
  expect(await umbracoApi.memberGroup.doesNameExist(memberGroupName)).toBeTruthy();
});

test('cannot create member group with empty name', {tag: '@release'}, async ({umbracoApi, umbracoUi}) => {
  // Act
  await umbracoUi.memberGroup.clickMemberGroupCreateButton();
  await umbracoUi.memberGroup.clickSaveButton();

  // Assert
  await umbracoUi.memberGroup.isFailedStateButtonVisible();
  expect(await umbracoApi.memberGroup.doesNameExist(memberGroupName)).toBeFalsy();
});

test('cannot create member group with duplicate name', {tag: '@release'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.memberGroup.create(memberGroupName);
  expect(await umbracoApi.memberGroup.doesNameExist(memberGroupName)).toBeTruthy();

  // Act
  await umbracoUi.memberGroup.clickMemberGroupCreateButton();
  await umbracoUi.memberGroup.enterMemberGroupName(memberGroupName);
  await umbracoUi.memberGroup.clickSaveButton();

  // Assert
  await umbracoUi.memberGroup.isFailedStateButtonVisible();
  await umbracoUi.memberGroup.doesErrorNotificationHaveText(NotificationConstantHelper.error.duplicateName);
});

test('can delete a member group', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.memberGroup.create(memberGroupName);
  expect(await umbracoApi.memberGroup.doesNameExist(memberGroupName)).toBeTruthy();

  // Act
  await umbracoUi.memberGroup.clickMemberGroupLinkByName(memberGroupName);
  // This wait is currently necessary as the action button is clicked before it is "ready"
  // TODO: Remove the wait and fix the 'clickActionButton'
  await umbracoUi.memberGroup.waitForTimeout(ConstantHelper.wait.medium);
  await umbracoUi.memberGroup.clickActionButton();
  await umbracoUi.memberGroup.clickDeleteButton();
  await umbracoUi.memberGroup.clickConfirmToDeleteButtonAndWaitForMemberGroupToBeDeleted();

  // Assert
  await umbracoUi.memberGroup.clickMemberGroupsSidebarButton();
  await umbracoUi.memberGroup.isMemberGroupNameVisible(memberGroupName, false);
  expect(await umbracoApi.memberGroup.doesNameExist(memberGroupName)).toBeFalsy();
});
