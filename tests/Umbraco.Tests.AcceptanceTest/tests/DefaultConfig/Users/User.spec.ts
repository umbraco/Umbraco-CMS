import {ConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {expect} from '@playwright/test';
import crypto from "crypto";
import * as fs from "fs";
import * as path from "path";

test.describe('User tests', () => {
  const nameOfTheUser = 'TestUser';
  const userEmail = 'TestUser@EmailTest.test';
  const userPassword = 'TestPassword';
  const defaultUserGroupName = 'Writers';

  test.beforeEach(async ({umbracoUi, umbracoApi}) => {
    await umbracoUi.goToBackOffice();
    await umbracoApi.user.ensureNameNotExists(nameOfTheUser);
  });

  test.afterEach(async ({umbracoApi}) => {
    await umbracoApi.user.ensureNameNotExists(nameOfTheUser);
  });

  test('can create a user', async ({page, umbracoApi, umbracoUi}) => {
    // Act
    await umbracoUi.user.goToSection(ConstantHelper.sections.users);
    await umbracoUi.user.clickCreateButton();
    await page.getByLabel('name').fill(nameOfTheUser);
    await page.getByLabel('email').fill(userEmail);
    await page.getByLabel('open', {exact: true}).click();
    await page.getByRole('button', {name: defaultUserGroupName}).click();
    await page.getByLabel('Submit').click();
    await page.getByLabel('Create user').click();

    // Assert
    await umbracoUi.user.isSuccessNotificationVisible();
    expect(await umbracoApi.user.doesNameExist(nameOfTheUser)).toBeTruthy();
  });

  test('can rename a user', async ({page, umbracoApi, umbracoUi}) => {
    // Arrange
    const wrongName = 'WrongName';
    await umbracoApi.user.ensureNameNotExists(wrongName);
    const userGroup = await umbracoApi.userGroup.getByName(defaultUserGroupName);
    await umbracoApi.user.createDefaultUser(wrongName, wrongName + userEmail, userGroup.id);

    // Act
    await umbracoUi.user.goToSection(ConstantHelper.sections.users);
    await page.getByText(wrongName, {exact: true}).click();
    await page.locator('#name').locator('#input').first().fill(nameOfTheUser);
    await umbracoUi.user.clickSaveButton();

    // Assert
    await umbracoUi.user.isSuccessNotificationVisible();
    expect(await umbracoApi.user.doesNameExist(nameOfTheUser)).toBeTruthy();
  });

  test('can delete a user', async ({page, umbracoApi, umbracoUi}) => {
    // Arrange
    const userGroup = await umbracoApi.userGroup.getByName(defaultUserGroupName);
    await umbracoApi.user.createDefaultUser(nameOfTheUser, userEmail, userGroup.id);

    // Act
    await umbracoUi.user.goToSection(ConstantHelper.sections.users);
    await page.getByText(nameOfTheUser, {exact: true}).click();
    await page.getByLabel('Delete...').click();
    await page.getByLabel('Delete', {exact: true}).click();

    // Assert
    await umbracoUi.user.isSuccessNotificationVisible();
    expect(await umbracoApi.user.doesNameExist(nameOfTheUser)).toBeFalsy();
    // Checks if the user is deleted from the list
    await umbracoUi.user.clickUsersTabButton();
    await expect(page.getByText(nameOfTheUser, {exact: true})).not.toBeVisible();
  });

  test('can add multiple user groups to a user', async ({page, umbracoApi, umbracoUi}) => {
    // Arrange
    const secondUserGroupName = 'Translators';
    const userGroupWriters = await umbracoApi.userGroup.getByName(defaultUserGroupName);
    const userGroupTranslators = await umbracoApi.userGroup.getByName(secondUserGroupName);
    await umbracoApi.user.createDefaultUser(nameOfTheUser, userEmail, userGroupWriters.id);

    // Act
    await umbracoUi.user.goToSection(ConstantHelper.sections.users);
    await page.getByText(nameOfTheUser, {exact: true}).click();
    await page.locator('[label="Groups"]').getByLabel('open', {exact: true}).click();
    await page.getByLabel(secondUserGroupName).click();
    await page.getByLabel('Submit').click();
    await page.getByLabel('Save').click();

    // Assert
    await umbracoUi.user.isSuccessNotificationVisible();
    const userData = await umbracoApi.user.getByName(nameOfTheUser);
    expect(userData.userGroupIds).toEqual([userGroupWriters.id, userGroupTranslators.id]);
  });

  test('can remove a user group from a user', async ({page, umbracoApi, umbracoUi}) => {
    // Arrange
    const userGroup = await umbracoApi.userGroup.getByName(defaultUserGroupName);
    await umbracoApi.user.createDefaultUser(nameOfTheUser, userEmail, userGroup.id);

    // Act
    await umbracoUi.user.goToSection(ConstantHelper.sections.users);
    await page.getByText(nameOfTheUser, {exact: true}).click();
    await page.getByLabel('Remove ' + defaultUserGroupName).click();
    await page.getByLabel('Remove', {exact: true}).click();
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
    const userGroup = await umbracoApi.userGroup.getByName('Writers');
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
    const userGroup = await umbracoApi.userGroup.getByName('Writers');
    await umbracoApi.user.createDefaultUser(nameOfTheUser, userEmail, userGroup.id);

    // Act
    await umbracoUi.user.goToSection(ConstantHelper.sections.users);
    await page.getByText(nameOfTheUser, {exact: true}).click();
    await umbracoUi.user.clickSaveButton();

    // Assert
    await umbracoUi.user.isSuccessNotificationVisible();

    await page.pause();
  });

  // TODO: Wait until the builder for document is available.
  test.skip('can add multiple content start nodes for a user', async ({page, umbracoApi, umbracoUi}) => {
    // Arrange
    const userGroup = await umbracoApi.userGroup.getByName('Writers');
    await umbracoApi.user.createDefaultUser(nameOfTheUser, userEmail, userGroup.id);

    // Act
    await umbracoUi.user.goToSection(ConstantHelper.sections.users);
    await page.getByText(nameOfTheUser, {exact: true}).click();
    await page.pause();
  });

  // TODO: Wait until the builder for document is available.
  test.skip('can remove a content start node from a user', async ({page, umbracoApi, umbracoUi}) => {
    // Arrange
    const userGroup = await umbracoApi.userGroup.getByName('Writers');
    await umbracoApi.user.createDefaultUser(nameOfTheUser, userEmail, userGroup.id);

    // Act
    await umbracoUi.user.goToSection(ConstantHelper.sections.users);
    await page.getByText(nameOfTheUser, {exact: true}).click();
    await page.pause();
  });

  // TODO: Wait until the builder for media is available.
  test.skip('can add media start nodes for a user', async ({page, umbracoApi, umbracoUi}) => {
    // Arrange
    const userGroup = await umbracoApi.userGroup.getByName('Writers');
    await umbracoApi.user.createDefaultUser(nameOfTheUser, userEmail, userGroup.id);

    // Act
    await umbracoUi.user.goToSection(ConstantHelper.sections.users);
    await page.getByText(nameOfTheUser, {exact: true}).click();
    await page.pause();
  });

  // TODO: Wait until the builder for media is available.
  test.skip('can add multiple media start nodes for a user', async ({page, umbracoApi, umbracoUi}) => {
    // Arrange
    const userGroup = await umbracoApi.userGroup.getByName('Writers');
    await umbracoApi.user.createDefaultUser(nameOfTheUser, userEmail, userGroup.id);

    // Act
    await umbracoUi.user.goToSection(ConstantHelper.sections.users);
    await page.getByText(nameOfTheUser, {exact: true}).click();
    await page.pause();
  });

  // TODO: Wait until the builder for media is available.
  test.skip('can remove a media start node from a user', async ({page, umbracoApi, umbracoUi}) => {
    // Arrange
    const userGroup = await umbracoApi.userGroup.getByName('Writers');
    await umbracoApi.user.createDefaultUser(nameOfTheUser, userEmail, userGroup.id);

    // Act
    await umbracoUi.user.goToSection(ConstantHelper.sections.users);
    await page.getByText(nameOfTheUser, {exact: true}).click();
    await page.pause();
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
    await page.pause();
  });


  test('can change password for a user', async ({page, umbracoApi, umbracoUi}) => {
    // Arrange
    const userGroup = await umbracoApi.userGroup.getByName('Writers');
    const userId = await umbracoApi.user.createDefaultUser(nameOfTheUser, userEmail, userGroup.id);

    // Act
    await umbracoUi.user.goToSection(ConstantHelper.sections.users);
    await page.getByText(nameOfTheUser, {exact: true}).click();
    await page.getByLabel('Change Password').click();
    await page.locator('input[name="newPassword"]').fill(userPassword);
    await page.locator('input[name="confirmPassword"]').fill(userPassword);

    // Assert
    // Checks if the password was updated successfully.
    await Promise.all([
      page.waitForResponse(resp => resp.url().includes(umbracoApi.baseUrl + '/umbraco/management/api/v1/user/' + userId + '/change-password') && resp.status() === 200),
      await page.getByLabel('Confirm').click()
    ]);
  });

  test('can disable a user', async ({page, umbracoApi, umbracoUi}) => {
    // Arrange
    const userGroup = await umbracoApi.userGroup.getByName('Writers');
    await umbracoApi.user.createDefaultUser(nameOfTheUser, userEmail, userGroup.id);

    // Act
    await umbracoUi.user.goToSection(ConstantHelper.sections.users);
    await page.getByText(nameOfTheUser, {exact: true}).click();

    await page.getByLabel('Disable').click();
    await page.locator('#confirm').getByLabel('Disable').click();

    // Assert
    await umbracoUi.user.isSuccessNotificationVisible();
    await expect(page.getByText('Disabled', {exact: true})).toBeVisible();
    const userData = await umbracoApi.user.getByName(nameOfTheUser);
    expect(userData.state).toBe('Disabled');
  });

  test('can enable a user', async ({page, umbracoApi, umbracoUi}) => {
    // Arrange
    const userGroup = await umbracoApi.userGroup.getByName('Writers');
    const userId = await umbracoApi.user.createDefaultUser(nameOfTheUser, userEmail, userGroup.id);
    await umbracoApi.user.disable([userId]);

    // Act
    await umbracoUi.user.goToSection(ConstantHelper.sections.users);
    await page.getByText(nameOfTheUser, {exact: true}).click();
    await page.getByLabel('Enable').click();
    await page.locator('#confirm').getByLabel('Enable').click();

    // Assert
    await umbracoUi.user.isSuccessNotificationVisible();
    // The state of the user is not enabled. The reason for this is that the user has not logged in, resulting in the state Inactive.
    const userData = await umbracoApi.user.getByName(nameOfTheUser);
    expect(userData.state).toBe('Inactive');
  });

  test('can add an avatar to a user', async ({page, umbracoApi, umbracoUi}) => {
    // Arrange
    const userGroup = await umbracoApi.userGroup.getByName('Writers');
    const userId = await umbracoApi.user.createDefaultUser(nameOfTheUser, userEmail, userGroup.id);

    // Create a temporary file
    const temporaryFileId = crypto.randomUUID();

    const fileName = 'Umbraco.png';
    const mimeType = 'image/png';
    const filePath = './fixtures/mediaLibrary/Umbraco.png';
    console.log(await umbracoApi.temporaryFile.create(temporaryFileId, fileName, mimeType, filePath));

    // Act

    let skippity = fs.readFileSync(filePath);
    console.log(skippity);

    await umbracoUi.user.goToSection(ConstantHelper.sections.users);
    await page.getByText(nameOfTheUser, {exact: true}).click();
    await page.pause();
    await page.getByLabel('Change photo').click();

    await page.locator('#AvatarFileField').setInputFiles({
      name: fileName,
      mimeType: mimeType,
      buffer: fs.readFileSync(filePath)
    })

    // await umbracoApi.user.addAvatar(userId, temporaryFileId);

    await page.pause();


    // Assert
    // Checks if the avatar was updated successfully.
  });

  test('can update an avatar for a user', async ({page, umbracoApi, umbracoUi}) => {

  });

  test('can remove an avatar from a user', async ({page, umbracoApi, umbracoUi}) => {

  });

  test('can see if the inactive label is removed from the test user', async ({page, umbracoApi, umbracoUi}) => {
    const currentUser = await umbracoApi.user.getCurrentUser();

    // Act
    await umbracoUi.user.goToSection(ConstantHelper.sections.users);
    await page.getByText(currentUser.name, {exact: true}).click();
    await expect(page.getByText('Active', {exact: true})).toBeVisible();

    // Assert
    const userData = await umbracoApi.user.getByName(currentUser.name);
    expect(userData.state).toBe('Active');
  });

  test('can search for a user', async ({page, umbracoApi, umbracoUi}) => {
    // Arrange
    const userGroup = await umbracoApi.userGroup.getByName('Writers');
    await umbracoApi.user.createDefaultUser(nameOfTheUser, userEmail, userGroup.id);

    // Act
    await umbracoUi.user.goToSection(ConstantHelper.sections.users);
    await expect(page.locator('uui-card-user')).toHaveCount(2);
    await page.getByLabel('Search the users section').fill('TestUser');

    // Assert
    // Wait for filtering to be done
    await page.waitForTimeout(200);
    await expect(page.locator('uui-card-user')).toHaveCount(1);
    await expect(page.locator('uui-card-user')).toContainText(nameOfTheUser);
  });

  test('can filter by status', async ({page, umbracoApi, umbracoUi}) => {
    // Arrange
    const userGroup = await umbracoApi.userGroup.getByName('Writers');
    await umbracoApi.user.createDefaultUser(nameOfTheUser, userEmail, userGroup.id);

    // Act
    await umbracoUi.user.goToSection(ConstantHelper.sections.users);
    await expect(page.locator('uui-card-user')).toHaveCount(2);
    await page.locator('uui-button').filter({ hasText: 'Status: All' }).getByLabel('status').click({force: true});
    await page.locator('label').filter({hasText: 'Inactive'}).click();

    // Assert
    // Wait for filtering to be done
    await page.waitForTimeout(200);
    await expect(page.locator('uui-card-user')).toHaveCount(1);
    await expect(page.locator('uui-card-user')).toContainText(nameOfTheUser);
    await expect(page.locator('uui-card-user')).toContainText('Inactive');
  });

  test('can filter by user groups', async ({page, umbracoApi, umbracoUi}) => {
    // Arrange
    const userGroup = await umbracoApi.userGroup.getByName('Writers');
    await umbracoApi.user.createDefaultUser(nameOfTheUser, userEmail, userGroup.id);

    // Act
    await umbracoUi.user.goToSection(ConstantHelper.sections.users);

    await expect(page.locator('uui-card-user')).toHaveCount(2);

    await page.pause();
    await page.locator('uui-button').filter({ hasText: 'Groups: All' }).getByLabel('groups').click({force: true});
    await page.locator('label').filter({hasText: 'Writers'}).click();

    // Assert
    // Wait for filtering to be done
    await page.waitForTimeout(200);
    await expect(page.locator('uui-card-user')).toHaveCount(1);
    await expect(page.locator('uui-card-user')).toContainText('Writers');
  });


  test('can change from grid to table view', async ({page, umbracoApi, umbracoUi}) => {
    // Act
    await umbracoUi.user.goToSection(ConstantHelper.sections.users);
  });

  test('can change from table to grid view', async ({page, umbracoApi, umbracoUi}) => {
    // Act
    await umbracoUi.user.goToSection(ConstantHelper.sections.users);
  });


});
