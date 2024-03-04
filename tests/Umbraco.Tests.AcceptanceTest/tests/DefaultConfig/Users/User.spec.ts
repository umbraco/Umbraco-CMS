import {ConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {expect} from '@playwright/test';

test.describe('User tests', () => {
  const nameOfTheUser = 'TestUser';
  const userEmail = 'TestUser@EmailTest.test';
  const userPassword = 'TestPassword';

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
    await page.getByLabel('open', { exact: true }).click();
    await page.getByRole('button', { name: 'Writers' }).click();
    await page.getByLabel('Submit').click();
    await page.getByLabel('Create user').click();

    // Assert
    await umbracoUi.user.isSuccessNotificationVisible();
    expect(await umbracoApi.user.doesNameExist(nameOfTheUser)).toBeTruthy();
  });

  test('can rename a user', async ({page, umbracoApi, umbracoUi}) => {
    // Arrange
    const userGroup = await umbracoApi.userGroup.getByName('Writers');
    await umbracoApi.user.createDefaultUser(nameOfTheUser, userEmail, userGroup.id);

    // Act
    await umbracoUi.user.goToSection(ConstantHelper.sections.users);

    await page.getByText(nameOfTheUser, {exact: true}).click();

    await page.pause();
  });

  test('can delete a user', async ({page, umbracoApi, umbracoUi}) => {
    // Arrange
    const userGroup = await umbracoApi.userGroup.getByName('Writers');
    await umbracoApi.user.createDefaultUser(nameOfTheUser, userEmail, userGroup.id);

    // Act
    await umbracoUi.user.goToSection(ConstantHelper.sections.users);

    await page.getByText(nameOfTheUser, { exact: true }).click();
    await page.getByLabel('Delete...').click();
    await page.getByLabel('Delete', { exact: true }).click();


    // Assert
    await umbracoUi.user.isSuccessNotificationVisible();
    expect(await umbracoApi.user.doesNameExist(nameOfTheUser)).toBeFalsy();
    // Checks if the user is deleted from the list
    await umbracoUi.user.clickUsersTabButton();
    await expect(page.getByText(nameOfTheUser, { exact: true })).not.toBeVisible();
  });

  test('can add multiple user groups to a user', async ({page, umbracoApi, umbracoUi}) => {
    // Arrange
    const userGroup = await umbracoApi.userGroup.getByName('Writers');
    await umbracoApi.user.createDefaultUser(nameOfTheUser, userEmail, userGroup.id);

    // Act
    await umbracoUi.user.goToSection(ConstantHelper.sections.users);
    await page.getByText(nameOfTheUser, {exact: true}).click();
    await page.pause();
  });
  test('can remove a user group from a user', async ({page, umbracoApi, umbracoUi}) => {
    // Arrange
    const userGroup = await umbracoApi.userGroup.getByName('Writers');
    await umbracoApi.user.createDefaultUser(nameOfTheUser, userEmail, userGroup.id);

    // Act
    await umbracoUi.user.goToSection(ConstantHelper.sections.users);
    await page.getByText(nameOfTheUser, {exact: true}).click();
    await page.pause();
  });

  test('can remove multiple user groups from a user', async ({page, umbracoApi, umbracoUi}) => {
    // Arrange
    const userGroup = await umbracoApi.userGroup.getByName('Writers');
    await umbracoApi.user.createDefaultUser(nameOfTheUser, userEmail, userGroup.id);

    // Act
    await umbracoUi.user.goToSection(ConstantHelper.sections.users);
    await page.getByText(nameOfTheUser, {exact: true}).click();
    await page.pause();
  });

  test('can update culture for a user', async ({page, umbracoApi, umbracoUi}) => {
    // Arrange
    const userGroup = await umbracoApi.userGroup.getByName('Writers');
    await umbracoApi.user.createDefaultUser(nameOfTheUser, userEmail, userGroup.id);

    // Act
    await umbracoUi.user.goToSection(ConstantHelper.sections.users);
    await page.getByText(nameOfTheUser, {exact: true}).click();
    await page.pause();
  });

  test('can add content start nodes for a user', async ({page, umbracoApi, umbracoUi}) => {
    // Arrange
    const userGroup = await umbracoApi.userGroup.getByName('Writers');
    await umbracoApi.user.createDefaultUser(nameOfTheUser, userEmail, userGroup.id);

    // Act
    await umbracoUi.user.goToSection(ConstantHelper.sections.users);
    await page.getByText(nameOfTheUser, {exact: true}).click();
    await page.pause();
  });

  test('can add multiple content start nodes for a user', async ({page, umbracoApi, umbracoUi}) => {
    // Arrange
    const userGroup = await umbracoApi.userGroup.getByName('Writers');
    await umbracoApi.user.createDefaultUser(nameOfTheUser, userEmail, userGroup.id);

    // Act
    await umbracoUi.user.goToSection(ConstantHelper.sections.users);
    await page.getByText(nameOfTheUser, {exact: true}).click();
    await page.pause();
  });

  test('can remove a content start node from a user', async ({page, umbracoApi, umbracoUi}) => {
    // Arrange
    const userGroup = await umbracoApi.userGroup.getByName('Writers');
    await umbracoApi.user.createDefaultUser(nameOfTheUser, userEmail, userGroup.id);

    // Act
    await umbracoUi.user.goToSection(ConstantHelper.sections.users);
    await page.getByText(nameOfTheUser, {exact: true}).click();
    await page.pause();
  });

  test('can add media start nodes for a user', async ({page, umbracoApi, umbracoUi}) => {
    // Arrange
    const userGroup = await umbracoApi.userGroup.getByName('Writers');
    await umbracoApi.user.createDefaultUser(nameOfTheUser, userEmail, userGroup.id);

    // Act
    await umbracoUi.user.goToSection(ConstantHelper.sections.users);
    await page.getByText(nameOfTheUser, {exact: true}).click();
    await page.pause();
  });

  test('can add multiple media start nodes for a user', async ({page, umbracoApi, umbracoUi}) => {
    // Arrange
    const userGroup = await umbracoApi.userGroup.getByName('Writers');
    await umbracoApi.user.createDefaultUser(nameOfTheUser, userEmail, userGroup.id);

    // Act
    await umbracoUi.user.goToSection(ConstantHelper.sections.users);
    await page.getByText(nameOfTheUser, {exact: true}).click();
    await page.pause();
  });

  test('can remove a media start node from a user', async ({page, umbracoApi, umbracoUi}) => {
    // Arrange
    const userGroup = await umbracoApi.userGroup.getByName('Writers');
    await umbracoApi.user.createDefaultUser(nameOfTheUser, userEmail, userGroup.id);

    // Act
    await umbracoUi.user.goToSection(ConstantHelper.sections.users);
    await page.getByText(nameOfTheUser, {exact: true}).click();
    await page.pause();
  });

  test('can see if the user has the correct access based on userGroup (ELABORATE)', async ({page, umbracoApi, umbracoUi}) => {
    // Arrange
    const userGroup = await umbracoApi.userGroup.getByName('Writers');
    await umbracoApi.user.createDefaultUser(nameOfTheUser, userEmail, userGroup.id);

    // Act
    await umbracoUi.user.goToSection(ConstantHelper.sections.users);
    await page.getByText(nameOfTheUser, {exact: true}).click();
    await page.pause();
  });


  test('can change password for a user', async ({page, umbracoApi, umbracoUi}) => {
    // Arrange
    const userGroup = await umbracoApi.userGroup.getByName('Writers');
    await umbracoApi.user.createDefaultUser(nameOfTheUser, userEmail, userGroup.id);

    // Act
    await umbracoUi.user.goToSection(ConstantHelper.sections.users);
    await page.getByText(nameOfTheUser, {exact: true}).click();
    await page.pause();
  });

  test('can disable a user', async ({page, umbracoApi, umbracoUi}) => {
    // Arrange
    const userGroup = await umbracoApi.userGroup.getByName('Writers');
    await umbracoApi.user.createDefaultUser(nameOfTheUser, userEmail, userGroup.id);

    // Act
    await umbracoUi.user.goToSection(ConstantHelper.sections.users);
    await page.getByText(nameOfTheUser, {exact: true}).click();
    await page.pause();
  });

  test('can enable a user', async ({page, umbracoApi, umbracoUi}) => {
    // Arrange
    const userGroup = await umbracoApi.userGroup.getByName('Writers');
    await umbracoApi.user.createDefaultUser(nameOfTheUser, userEmail, userGroup.id);

    // Act
    await umbracoUi.user.goToSection(ConstantHelper.sections.users);
    await page.getByText(nameOfTheUser, {exact: true}).click();
    await page.pause();
  });

  test('can see if the inactive label is removed after logging in', async ({page, umbracoApi, umbracoUi}) => {
    // Arrange
    const userGroup = await umbracoApi.userGroup.getByName('Writers');
    await umbracoApi.user.createDefaultUser(nameOfTheUser, userEmail, userGroup.id);

    // Act
    await umbracoUi.user.goToSection(ConstantHelper.sections.users);
    await page.getByText(nameOfTheUser, {exact: true}).click();
    await page.pause();
  });



});
