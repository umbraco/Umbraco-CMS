import {ConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

const userGroupName = 'TestUserGroupName';

test.beforeEach(async ({umbracoUi, umbracoApi}) => {
  await umbracoApi.userGroup.ensureNameNotExists(userGroupName);
  await umbracoUi.goToBackOffice();
  await umbracoUi.userGroup.goToSection(ConstantHelper.sections.users);
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.userGroup.ensureNameNotExists(userGroupName);
});

test('the default descriptions for all default user groups are correct', {tag: '@release'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const userGroupDescriptions = ConstantHelper.userGroupDescriptionValues;

  // Act
  await umbracoUi.userGroup.clickUserGroupsButton();

  // Assert
  for (const userGroup of Object.entries(userGroupDescriptions)) {
    const groupName = userGroup[0];
    const expectedDescription = userGroup[1];
    // Verify description in UI
    await umbracoUi.userGroup.doesUserGroupHaveDescription(groupName, expectedDescription);
    // Verify description via API
    const userGroupData = await umbracoApi.userGroup.getByName(groupName);
    expect(userGroupData.description).toBe(expectedDescription);
  }
});

const descriptionTestCases = [
  {
    name: 'short description',
    description: 'Short desc',
    tag: '@release'
  },
  {
    name: 'normal description',
    description: 'This is a test description for user group',
    tag: '@release'
  },
  {
    name: 'long description',
    description: 'This is a very long description that contains multiple sentences to test the maximum length handling of the description field. It should be able to handle large amounts of text without any issues. This description is intentionally verbose to ensure the system can process and store lengthy descriptions properly.',
    tag: undefined
  },
  {
    name: 'description with special characters',
    description: 'Description with special chars: @#$%^&*()_+-=[]{}|;:\'",.<>?/~`!',
    tag: undefined
  },
  {
    name: 'description with unicode characters',
    description: 'Description with unicode: émojis 😀🎉 và tiếng Việt ăn ơ ư đ 日本語 こんにちは カタカナ',
    tag: undefined
  },
  {
    name: 'description with HTML-like content',
    description: '<script>alert("test")</script> & <b>bold</b> text',
    tag: undefined
  },
  {
    name: 'description with quotes and apostrophes',
    description: 'It\'s a "test" description with \'single\' and "double" quotes',
    tag: undefined
  }
];

for (const testCase of descriptionTestCases) {
  test(`can create user group with ${testCase.name}`, testCase.tag ? {tag: testCase.tag} : {}, async ({umbracoApi, umbracoUi}) => {
    // Act
    await umbracoUi.userGroup.clickUserGroupsButton();
    await umbracoUi.userGroup.clickCreateLink();
    await umbracoUi.userGroup.enterUserGroupName(userGroupName);
    if (testCase.description !== '') {
      await umbracoUi.userGroup.enterDescription(testCase.description);
    }
    const userGroupId = await umbracoUi.userGroup.clickSaveButtonAndWaitForUserGroupToBeCreated();

    // Assert
    expect(userGroupId).toBeTruthy();
    expect(await umbracoApi.userGroup.doesExist(userGroupId!)).toBe(true);
    // Checks if the user group was created in the UI as well
    await umbracoUi.userGroup.clickUserGroupsButton();
    await umbracoUi.userGroup.isUserGroupWithNameVisible(userGroupName);
    await umbracoUi.userGroup.doesUserGroupHaveDescription(userGroupName, testCase.description);

    // Verify description was saved correctly via API
    const userGroupData = await umbracoApi.userGroup.getByName(userGroupName);
    expect(userGroupData.description).toBe(testCase.description);
  });
}

test('can update description for a user group', {tag: '@release'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const originalDescription = 'Original description for testing';
  const updatedDescription = 'Updated description after modification';
  await umbracoApi.userGroup.createEmptyUserGroup(userGroupName, originalDescription);
  // Verify original description
  const originalUserGroupData = await umbracoApi.userGroup.getByName(userGroupName);
  expect(originalUserGroupData.description).toBe(originalDescription);

  // Act
  await umbracoUi.userGroup.clickUserGroupsButton();
  await umbracoUi.userGroup.clickUserGroupWithName(userGroupName);
  await umbracoUi.userGroup.enterDescription(updatedDescription);
  await umbracoUi.userGroup.clickSaveButtonAndWaitForUserGroupToBeUpdated();

  // Assert
  await umbracoUi.userGroup.clickUserGroupsButton();
  await umbracoUi.userGroup.doesUserGroupHaveDescription(userGroupName,updatedDescription);
  const updatedUserGroupData = await umbracoApi.userGroup.getByName(userGroupName);
  expect(updatedUserGroupData.description).toBe(updatedDescription);
});

test('can see user group description in the user group picker', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const userGroupHaveNoDescriptionName = 'UserGroupNoDescriptionPickerTest';
  const userGroupDescription = 'This is a test description for user group';
  await umbracoApi.userGroup.createSimpleUserGroupWithContentSection(userGroupName, userGroupDescription);
  await umbracoApi.userGroup.createSimpleUserGroupWithContentSection(userGroupHaveNoDescriptionName, '');
  await umbracoUi.user.goToUsers();

  // Act
  await umbracoUi.user.clickCreateActionWithOptionName('User');
  await umbracoUi.user.clickChooseButton();

  // Assert
  await umbracoUi.user.doesUserGroupPickerHaveDetails(userGroupName, userGroupDescription);
  await umbracoUi.user.doesUserGroupPickerHaveDetails(userGroupHaveNoDescriptionName, 'Sections: Content');

  // Clean
  await umbracoApi.userGroup.ensureNameNotExists(userGroupHaveNoDescriptionName);
});
