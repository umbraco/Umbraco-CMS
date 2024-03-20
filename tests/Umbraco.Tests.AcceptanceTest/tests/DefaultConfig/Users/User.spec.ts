import {ConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {expect} from '@playwright/test';

test.describe('User tests', () => {
  const nameOfTheUser = 'TestUser';
  const userEmail = 'TestUser@EmailTest.test';
  const defaultUserGroupName = 'Writers';

  test.beforeEach(async ({umbracoUi, umbracoApi}) => {
    await umbracoUi.goToBackOffice();
    await umbracoApi.user.ensureNameNotExists(nameOfTheUser);
  });

  test.afterEach(async ({umbracoApi}) => {
    await umbracoApi.user.ensureNameNotExists(nameOfTheUser);
  });

  test('can create a user', async ({umbracoApi, umbracoUi}) => {
    // Act
    await umbracoUi.user.goToSection(ConstantHelper.sections.users);
    await umbracoUi.user.clickCreateButton();
    await umbracoUi.user.enterNameOfTheUser(nameOfTheUser);
    await umbracoUi.user.enterUserEmail(userEmail);
    await umbracoUi.user.clickAddUserGroupsButton();
    await umbracoUi.user.clickButtonWithName(defaultUserGroupName);
    await umbracoUi.user.clickSubmitButton();
    await umbracoUi.user.clickCreateUserButton();

    // Assert
    await umbracoUi.user.isSuccessNotificationVisible();
    expect(await umbracoApi.user.doesNameExist(nameOfTheUser)).toBeTruthy();
  });

  test('can rename a user', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const wrongName = 'WrongName';
    await umbracoApi.user.ensureNameNotExists(wrongName);
    const userGroup = await umbracoApi.userGroup.getByName(defaultUserGroupName);
    await umbracoApi.user.createDefaultUser(wrongName, wrongName + userEmail, userGroup.id);

    // Act
    await umbracoUi.user.goToSection(ConstantHelper.sections.users);
    await umbracoUi.user.clickTextButtonWithName(wrongName);
    await umbracoUi.user.enterUpdatedNameOfUser(nameOfTheUser);
    await umbracoUi.user.clickSaveButton();

    // Assert
    await umbracoUi.user.isSuccessNotificationVisible();
    expect(await umbracoApi.user.doesNameExist(nameOfTheUser)).toBeTruthy();
  });

  test('can delete a user', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const userGroup = await umbracoApi.userGroup.getByName(defaultUserGroupName);
    await umbracoApi.user.createDefaultUser(nameOfTheUser, userEmail, userGroup.id);

    // Act
    await umbracoUi.user.goToSection(ConstantHelper.sections.users);
    await umbracoUi.user.clickUserWithName(nameOfTheUser);
    await umbracoUi.user.clickDeleteThreeDotsButton();
    await umbracoUi.user.clickDeleteExactLabel();

    // Assert
    await umbracoUi.user.isSuccessNotificationVisible();
    expect(await umbracoApi.user.doesNameExist(nameOfTheUser)).toBeFalsy();
    // Checks if the user is deleted from the list
    await umbracoUi.user.clickUsersTabButton();
    await umbracoUi.user.isUserVisible(nameOfTheUser, false);
  });

  test('can add multiple user groups to a user', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const secondUserGroupName = 'Translators';
    const userGroupWriters = await umbracoApi.userGroup.getByName(defaultUserGroupName);
    const userGroupTranslators = await umbracoApi.userGroup.getByName(secondUserGroupName);
    await umbracoApi.user.createDefaultUser(nameOfTheUser, userEmail, userGroupWriters.id);

    // Act
    await umbracoUi.user.goToSection(ConstantHelper.sections.users);
    await umbracoUi.user.clickUserWithName(nameOfTheUser);
    await umbracoUi.user.clickOpenUserGroupsButton();
    await umbracoUi.user.clickButtonWithName(secondUserGroupName);
    await umbracoUi.user.clickSubmitButton();
    await umbracoUi.user.clickSaveButton();

    // Assert
    await umbracoUi.user.isSuccessNotificationVisible();
    const userData = await umbracoApi.user.getByName(nameOfTheUser);
    expect(userData.userGroupIds).toEqual([userGroupWriters.id, userGroupTranslators.id]);
  });

  test('can remove a user group from a user', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const userGroup = await umbracoApi.userGroup.getByName(defaultUserGroupName);
    await umbracoApi.user.createDefaultUser(nameOfTheUser, userEmail, userGroup.id);

    // Act
    await umbracoUi.user.goToSection(ConstantHelper.sections.users);
    await umbracoUi.user.clickTextButtonWithName(nameOfTheUser);
    await umbracoUi.user.clickRemoveWithName(defaultUserGroupName);
    await umbracoUi.user.clickRemoveExactButton();
    await umbracoUi.user.clickSaveButton();

    // Assert
    await umbracoUi.user.isSuccessNotificationVisible();
    const userData = await umbracoApi.user.getByName(nameOfTheUser);
    expect(userData.userGroupIds).toEqual([]);
  });

  // TODO: wait until the frontend is ready, currently there is always just 2 languages in the dropdown.
  test.skip('can update culture for a user', async ({page, umbracoApi, umbracoUi}) => {
    // Arrange
    const danishIsoCode = 'da';
    await umbracoApi.language.ensureIsoCodeNotExists(danishIsoCode);
    const userGroup = await umbracoApi.userGroup.getByName(defaultUserGroupName);
    await umbracoApi.language.createDanishLanguage();
    await umbracoApi.user.createDefaultUser(nameOfTheUser, userEmail, userGroup.id);

    // Act
    await umbracoUi.user.goToSection(ConstantHelper.sections.users);
    await page.getByText(nameOfTheUser, {exact: true}).click();

    await page.locator('[label="UI Culture"]').getByLabel('combobox-input').click()
    await page.locator('uui-combobox-list').getByText('Dansk').click();
    await umbracoUi.user.clickSaveButton();

    // Assert
    await umbracoUi.user.isSuccessNotificationVisible();

    await page.pause();
    const userData = await umbracoApi.user.getByName(nameOfTheUser);
    console.log(userData);
    expect(userData.languageIsoCode).toEqual(danishIsoCode);

    // Clean
    await umbracoApi.language.ensureIsoCodeNotExists(danishIsoCode);
  });

  // TODO: Wait until the builder for document is available.
  test.skip('can add content start nodes for a user', async ({page, umbracoApi, umbracoUi}) => {
    // Arrange
    const userGroup = await umbracoApi.userGroup.getByName(defaultUserGroupName);
    await umbracoApi.user.createDefaultUser(nameOfTheUser, userEmail, userGroup.id);

    // Act
    await umbracoUi.user.goToSection(ConstantHelper.sections.users);
    await page.getByText(nameOfTheUser, {exact: true}).click();
    await umbracoUi.user.clickSaveButton();

    // Assert
    await umbracoUi.user.isSuccessNotificationVisible();
  });

  // TODO: Wait until the builder for document is available.
  test.skip('can add multiple content start nodes for a user', async ({page, umbracoApi, umbracoUi}) => {
    // Arrange
    const userGroup = await umbracoApi.userGroup.getByName(defaultUserGroupName);
    await umbracoApi.user.createDefaultUser(nameOfTheUser, userEmail, userGroup.id);

    // Act
    await umbracoUi.user.goToSection(ConstantHelper.sections.users);
    await page.getByText(nameOfTheUser, {exact: true}).click();
  });

  // TODO: Wait until the builder for document is available.
  test.skip('can remove a content start node from a user', async ({page, umbracoApi, umbracoUi}) => {
    // Arrange
    const userGroup = await umbracoApi.userGroup.getByName(defaultUserGroupName);
    await umbracoApi.user.createDefaultUser(nameOfTheUser, userEmail, userGroup.id);

    // Act
    await umbracoUi.user.goToSection(ConstantHelper.sections.users);
    await page.getByText(nameOfTheUser, {exact: true}).click();
  });

  // TODO: Wait until the builder for media is available.
  test.skip('can add media start nodes for a user', async ({page, umbracoApi, umbracoUi}) => {
    // Arrange
    const userGroup = await umbracoApi.userGroup.getByName(defaultUserGroupName);
    await umbracoApi.user.createDefaultUser(nameOfTheUser, userEmail, userGroup.id);

    // Act
    await umbracoUi.user.goToSection(ConstantHelper.sections.users);
    await page.getByText(nameOfTheUser, {exact: true}).click();
  });

  // TODO: Wait until the builder for media is available.
  test.skip('can add multiple media start nodes for a user', async ({page, umbracoApi, umbracoUi}) => {
    // Arrange
    const userGroup = await umbracoApi.userGroup.getByName(defaultUserGroupName);
    await umbracoApi.user.createDefaultUser(nameOfTheUser, userEmail, userGroup.id);

    // Act
    await umbracoUi.user.goToSection(ConstantHelper.sections.users);
    await page.getByText(nameOfTheUser, {exact: true}).click();
  });

  // TODO: Wait until the builder for media is available.
  test.skip('can remove a media start node from a user', async ({page, umbracoApi, umbracoUi}) => {
    // Arrange
    const userGroup = await umbracoApi.userGroup.getByName(defaultUserGroupName);
    await umbracoApi.user.createDefaultUser(nameOfTheUser, userEmail, userGroup.id);

    // Act
    await umbracoUi.user.goToSection(ConstantHelper.sections.users);
    await page.getByText(nameOfTheUser, {exact: true}).click();
  });

  //TODO: Frontend does not show the correct access nodes.
  test.skip('can see if the user has the correct access based on userGroup', async ({page, umbracoApi, umbracoUi}) => {
    // Arrange
    const userGroupName = 'TestUserGroup';
    await umbracoApi.userGroup.ensureNameNotExists(userGroupName);
    console.log(await umbracoApi.userGroup.createSimpleUserGroup(userGroupName));
    const userGroup = await umbracoApi.userGroup.getByName(userGroupName);
    await umbracoApi.user.createDefaultUser(nameOfTheUser, userEmail, userGroup.id);

    // Act
    await umbracoUi.user.goToSection(ConstantHelper.sections.users);
    await page.getByText(nameOfTheUser, {exact: true}).click();
  });

  test('can change password for a user', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const userPassword = 'TestPassword';
    const userGroup = await umbracoApi.userGroup.getByName(defaultUserGroupName);
    const userId = await umbracoApi.user.createDefaultUser(nameOfTheUser, userEmail, userGroup.id);

    // Act
    await umbracoUi.user.goToSection(ConstantHelper.sections.users);
    await umbracoUi.user.clickTextButtonWithName(nameOfTheUser);
    await umbracoUi.user.clickChangePasswordButton();
    await umbracoUi.user.updatePassword(userPassword);

    // Assert
    await umbracoUi.user.isPasswordUpdatedForUserWithId(userId);
  });

  test('can disable a user', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const disabledStatus = 'Disabled';
    const userGroup = await umbracoApi.userGroup.getByName(defaultUserGroupName);
    await umbracoApi.user.createDefaultUser(nameOfTheUser, userEmail, userGroup.id);

    // Act
    await umbracoUi.user.goToSection(ConstantHelper.sections.users);
    await umbracoUi.user.clickTextButtonWithName(nameOfTheUser);
    await umbracoUi.user.clickDisableButton();
    await umbracoUi.user.clickConfirmDisableButton();

    // Assert
    await umbracoUi.user.isSuccessNotificationVisible();
    await umbracoUi.user.isTextWithExactNameVisible(disabledStatus);
    const userData = await umbracoApi.user.getByName(nameOfTheUser);
    expect(userData.state).toBe(disabledStatus);
  });

  test('can enable a user', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const inactiveStatus = 'Inactive';
    const userGroup = await umbracoApi.userGroup.getByName(defaultUserGroupName);
    const userId = await umbracoApi.user.createDefaultUser(nameOfTheUser, userEmail, userGroup.id);
    await umbracoApi.user.disable([userId]);

    // Act
    await umbracoUi.user.goToSection(ConstantHelper.sections.users);
    await umbracoUi.user.clickTextButtonWithName(nameOfTheUser);
    await umbracoUi.user.clickEnableButton();
    await umbracoUi.user.clickConfirmEnableButton();

    // Assert
    await umbracoUi.user.isSuccessNotificationVisible();
    // The state of the user is not enabled. The reason for this is that the user has not logged in, resulting in the state Inactive.
    const userData = await umbracoApi.user.getByName(nameOfTheUser);
    expect(userData.state).toBe(inactiveStatus);
  });

  test('can add an avatar to a user', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const userGroup = await umbracoApi.userGroup.getByName(defaultUserGroupName);
    await umbracoApi.user.createDefaultUser(nameOfTheUser, userEmail, userGroup.id);
    const filePath = './fixtures/mediaLibrary/Umbraco.png';

    // Act
    await umbracoUi.user.goToSection(ConstantHelper.sections.users);
    await umbracoUi.user.clickTextButtonWithName(nameOfTheUser);
    await umbracoUi.user.changePhotoWithFileChooser(filePath);

    // Assert
    await umbracoUi.user.isSuccessNotificationVisible();
    const userData = await umbracoApi.user.getByName(nameOfTheUser);
    expect(userData.avatarUrls).not.toHaveLength(0);
  });

  test('can remove an avatar from a user', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const userGroup = await umbracoApi.userGroup.getByName(defaultUserGroupName);
    const userId = await umbracoApi.user.createDefaultUser(nameOfTheUser, userEmail, userGroup.id);
    await umbracoApi.user.addDefaultAvatarImageToUser(userId);

    // Act
    await umbracoUi.user.goToSection(ConstantHelper.sections.users);
    await umbracoUi.user.clickTextButtonWithName(nameOfTheUser);
    await umbracoUi.user.clickRemovePhotoButton();

    // Assert
    await umbracoUi.user.isSuccessNotificationVisible();
    const userData = await umbracoApi.user.getByName(nameOfTheUser);
    expect(userData.avatarUrls).toHaveLength(0);
  });

  test('can see if the inactive label is removed from the test user', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const userLabel = 'Active';
    const currentUser = await umbracoApi.user.getCurrentUser();

    // Act
    await umbracoUi.user.goToSection(ConstantHelper.sections.users);
    await umbracoUi.user.clickTextButtonWithName(currentUser.name);

    // Assert
    await umbracoUi.user.isTextWithExactNameVisible(userLabel);
    const userData = await umbracoApi.user.getByName(currentUser.name);
    expect(userData.state).toBe(userLabel);
  });

  test('can search for a user', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const userGroup = await umbracoApi.userGroup.getByName(defaultUserGroupName);
    await umbracoApi.user.createDefaultUser(nameOfTheUser, userEmail, userGroup.id);

    // Act
    await umbracoUi.user.goToSection(ConstantHelper.sections.users);
    await umbracoUi.user.doesUserSectionContainUserAmount(2);
    await umbracoUi.user.searchInUserSection(nameOfTheUser);

    // Assert
    // Wait for filtering to be done
    await umbracoUi.waitForTimeout(200);
    await umbracoUi.user.doesUserSectionContainUserAmount(1);
    await umbracoUi.user.doesUserSectionContainUserWithText(nameOfTheUser);
  });

  test('can filter by status', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const inactiveStatus = 'Inactive';
    const userGroup = await umbracoApi.userGroup.getByName(defaultUserGroupName);
    await umbracoApi.user.createDefaultUser(nameOfTheUser, userEmail, userGroup.id);

    // Act
    await umbracoUi.user.goToSection(ConstantHelper.sections.users);
    await umbracoUi.user.doesUserSectionContainUserAmount(2);
    await umbracoUi.user.filterByStatusName(inactiveStatus);

    // Assert
    // Wait for filtering to be done
    await umbracoUi.waitForTimeout(200);
    await umbracoUi.user.doesUserSectionContainUserAmount(1);
    await umbracoUi.user.doesUserSectionContainUserWithText(nameOfTheUser);
    await umbracoUi.user.doesUserSectionContainUserWithText(inactiveStatus);
  });

  test('can filter by user groups', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const userGroup = await umbracoApi.userGroup.getByName(defaultUserGroupName);
    await umbracoApi.user.createDefaultUser(nameOfTheUser, userEmail, userGroup.id);

    // Act
    await umbracoUi.user.goToSection(ConstantHelper.sections.users);
    await umbracoUi.user.doesUserSectionContainUserAmount(2);
    await umbracoUi.user.filterByGroupName(defaultUserGroupName);

    // Assert
    // Wait for filtering to be done
    await umbracoUi.waitForTimeout(200);
    await umbracoUi.user.doesUserSectionContainUserAmount(1);
    await umbracoUi.user.doesUserSectionContainUserWithText(defaultUserGroupName);
  });

  // TODO: Sometimes the frontend does not switch from grid to table, or table to grid.
  test.skip('can change from grid to table view', async ({page, umbracoApi, umbracoUi}) => {
    // Act
    await umbracoUi.user.goToSection(ConstantHelper.sections.users);
    await page.locator('umb-collection-view-bundle').getByLabel('status').click({force: true});
    await page.getByRole('link', {name: 'Table'}).click({force: true});

    // Assert
    await expect(page.locator('umb-user-table-collection-view')).toBeVisible();
    expect(page.url()).toEqual(umbracoApi.baseUrl + '/umbraco/section/user-management/view/users/collection/table');
  });
});
