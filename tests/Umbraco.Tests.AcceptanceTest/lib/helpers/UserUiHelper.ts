import {expect, Locator, Page} from "@playwright/test"
import {UiBaseLocators} from "./UiBaseLocators";
import {umbracoConfig} from "../umbraco.config";
import {ConstantHelper} from "./ConstantHelper";

export class UserUiHelper extends UiBaseLocators {
  private readonly usersBtn: Locator;
  private readonly createUserBtn: Locator;
  private readonly nameOfTheUserTxt: Locator;
  private readonly userEmailTxt: Locator;
  private readonly addUserGroupsBtn: Locator;
  private readonly openUserGroupsBtn: Locator;
  private readonly updatedNameOfTheUserTxt: Locator;
  private readonly changePasswordBtn: Locator;
  private readonly changePhotoBtn: Locator;
  private readonly removePhotoBtn: Locator;
  private readonly searchInUserSectionTxt: Locator;
  private readonly userSectionCard: Locator;
  private readonly statusBtn: Locator;
  private readonly groupBtn: Locator;
  private readonly chooseUserGroupsBtn: Locator;
  private readonly allowAccessToAllDocumentsToggle: Locator;
  private readonly allowAccessToAllMediaToggle: Locator;
  private readonly mediaInput: Locator;
  private readonly chooseContainerBtn: Locator;
  private readonly languageBtn: Locator;
  private readonly disabledTxt: Locator;
  private readonly activeTxt: Locator;
  private readonly orderByBtn: Locator;
  private readonly orderByNewestBtn: Locator;
  private readonly documentStartNode: Locator;
  private readonly mediaStartNode: Locator;
  private readonly usersMenu: Locator;
  private readonly userBtn: Locator;
  private readonly userGrid: Locator;
  private readonly apiUserBtn: Locator;
  private readonly goToProfileBtn: Locator;
  private readonly nameOfUserInput: Locator;

  constructor(page: Page) {
    super(page);
    this.usersBtn = page.getByLabel('Users');
    this.createUserBtn = page.getByLabel('Create user');
    this.nameOfTheUserTxt = page.getByLabel('name', {exact: true});
    this.userEmailTxt = page.getByLabel('email');
    this.addUserGroupsBtn = page.locator('#userGroups').getByLabel('open', {exact: true});
    this.openUserGroupsBtn = page.locator('[label="Groups"]').getByLabel('open', {exact: true});
    this.chooseUserGroupsBtn = page.locator('umb-user-group-input').getByLabel('Choose');
    this.updatedNameOfTheUserTxt = page.locator('umb-workspace-header-name-editable').locator('input');
    this.changePasswordBtn = page.getByLabel('Change your password');
    this.changePhotoBtn = page.getByLabel('Change photo');
    this.removePhotoBtn = page.getByLabel('Remove photo');
    this.searchInUserSectionTxt = page.locator('umb-collection-filter-field #input');
    this.userSectionCard = page.locator('uui-card-user');
    this.statusBtn = page.locator('uui-button', {hasText: 'Status'});
    this.groupBtn = page.locator('uui-button', {hasText: 'Groups'});
    this.allowAccessToAllDocumentsToggle = page.locator('umb-property-layout').filter({hasText: 'Allow access to all documents'}).locator('#toggle');
    this.allowAccessToAllMediaToggle = page.locator('umb-property-layout').filter({hasText: 'Allow access to all media'}).locator('#toggle');
    this.mediaInput = page.locator('umb-input-media');
    this.chooseContainerBtn = page.locator('#container').getByLabel('Choose');
    this.languageBtn = page.locator('[label="UI Culture"] select');
    this.disabledTxt = page.getByText('Disabled', {exact: true});
    this.activeTxt = page.getByText('Active', {exact: true});
    this.orderByBtn = page.getByLabel('order by');
    this.orderByNewestBtn = page.getByLabel('Newest');
    this.documentStartNode = page.locator('umb-user-document-start-node');
    this.mediaStartNode = page.locator('umb-user-media-start-node');
    this.usersMenu = page.locator('umb-menu').getByLabel('Users', {exact: true});
    this.userBtn = page.locator('#collection-action-menu-popover').getByLabel('User', {exact: true});
    this.userGrid = page.locator('#card-grid');
    this.apiUserBtn = page.locator('#collection-action-menu-popover').getByLabel('API User', {exact: true});
    this.goToProfileBtn = page.getByLabel('Go to profile', {exact: true});
    this.nameOfUserInput = page.getByTestId('input:workspace-name').locator('#input');
  }

  async clickUsersButton() {
    await this.click(this.usersBtn);
  }

  async clickCreateUserButton() {
    await this.click(this.createUserBtn);
    await this.page.waitForTimeout(ConstantHelper.wait.short);
  }

  async enterNameOfTheUser(name: string) {
    await this.enterText(this.nameOfTheUserTxt, name);
  }

  async enterUserEmail(email: string) {
    await this.enterText(this.userEmailTxt, email);
  }

  async clickAddUserGroupsButton() {
    await this.click(this.addUserGroupsBtn);
    // This wait is necessary to avoid the click on the user group button to be ignored
    await this.page.waitForTimeout(ConstantHelper.wait.minimal);
  }

  async clickChooseUserGroupsButton() {
    await this.click(this.chooseUserGroupsBtn);
  }

  async clickOpenUserGroupsButton() {
    await this.click(this.openUserGroupsBtn);
  }

  async enterUpdatedNameOfUser(name: string) {
    await this.enterText(this.updatedNameOfTheUserTxt, name);
  }

  async clickUserWithName(name: string) {
    const userNameLocator = this.page.locator('#open-part').getByText(name, {exact: true});
    await this.click(userNameLocator);
  }

  async clickChangePasswordButton() {
    await this.click(this.changePasswordBtn);
  }

  async updatePassword(newPassword: string) {
    await this.newPasswordTxt.fill(newPassword);
    await this.confirmPasswordTxt.fill(newPassword);
  }

  async isUserVisible(name: string, isVisible = true) {
    return await this.isVisible(this.page.getByText(name, {exact: true}), isVisible);
  }

  async clickChangePhotoButton() {
    await this.click(this.changePhotoBtn);
  }

  async clickRemoveButtonForUserGroupWithName(userGroupName: string) {
    await this.click(this.page.locator('umb-user-group-ref', {hasText: userGroupName}).locator('[label="Remove"]'));
  }

  async clickRemovePhotoButton() {
    await this.click(this.removePhotoBtn);
  }

  async changePhotoWithFileChooser(filePath: string) {
    const fileChooserPromise = this.page.waitForEvent('filechooser');
    await this.clickChangePhotoButton();
    const fileChooser = await fileChooserPromise;
    await fileChooser.setFiles(filePath);
  }

  async searchInUserSection(name: string) {
    await this.searchInUserSectionTxt.fill(name);
  }

  async doesUserSectionContainUserAmount(amount: number) {
    let userCount = 0;

    while (true) {
      await this.page.waitForTimeout(ConstantHelper.wait.medium);
      userCount += await this.userSectionCard.count();

      // Check if pagination exists and next button is enabled
      const nextButton = this.nextPaginationBtn;
      const nextButtonExists = await nextButton.count() > 0;

      if (!nextButtonExists) {
        break; // No pagination at all
      }

      const isNextEnabled = await nextButton.isEnabled();
      if (!isNextEnabled) {
        break;
      }

      await this.clickNextPaginationButton();
    }

    // If we actually navigated through the pagination, we should go back
    if (amount > 50) {
      const firstPage = this.firstPaginationBtn;
      const isFirstPageEnabled = await firstPage.isEnabled();
      if (isFirstPageEnabled) {
        await this.click(firstPage);
      }

      await this.page.waitForTimeout(ConstantHelper.wait.medium);
    }

    return expect(userCount).toBe(amount);
  }

  async doesUserSectionContainUserWithText(name: string) {
    await this.containsText(this.userGrid, name);
  }

  async filterByStatusName(statusName: string) {
    await this.click(this.statusBtn);
    await this.click(this.page.locator('label').filter({hasText: statusName}));
  }

  async filterByGroupName(groupName: string) {
    await this.click(this.groupBtn);
    await this.click(this.page.locator('label').filter({hasText: groupName}));
  }

  async isPasswordUpdatedForUserWithId(userId: string) {
    await Promise.all([
      this.page.waitForResponse(resp => resp.url().includes(umbracoConfig.environment.baseUrl + '/umbraco/management/api/v1/user/' + userId + '/change-password') && resp.status() === 200),
      await this.clickConfirmButton()
    ]);
  }

  async clickChooseContainerButton() {
    await this.click(this.chooseContainerBtn);
  }

  async selectUserLanguage(language: string) {
    await this.languageBtn.selectOption(language, {force: true});
  }

  async clickRemoveButtonForContentNodeWithName(name: string) {
    const entityItemLocator = this.entityItem.filter({has: this.page.locator(`[name="${name}"]`)});
    await this.hoverAndClick(entityItemLocator, entityItemLocator.getByRole('button', {name: 'Remove'}), {force: true});
  }

  async clickRemoveButtonForMediaNodeWithName(name: string) {
    await this.click(this.mediaInput.locator(`[name="${name}"]`).locator('[label="Remove"]'));
  }

  async clickAllowAccessToAllDocumentsToggle() {
    await this.click(this.allowAccessToAllDocumentsToggle);
  }

  async clickAllowAccessToAllMediaToggle() {
    await this.click(this.allowAccessToAllMediaToggle);
  }

  async isUserDisabledTextVisible() {
    await this.waitForVisible(this.disabledTxt);
  }

  async isUserActiveTextVisible() {
    await this.waitForVisible(this.activeTxt);
  }

  async orderByNewestUser() {
    // Force click is needed
    await this.click(this.orderByBtn, {force: true});
    await this.click(this.orderByNewestBtn);
  }

  async isUserWithNameTheFirstUserInList(name: string) {
    await expect(this.userSectionCard.first()).toContainText(name);
  }

  async doesUserHaveAccessToContentNode(name: string) {
    await this.isVisible(this.documentStartNode.locator(`[name="${name}"]`));
  }

  async doesUserHaveAccessToMediaNode(name: string) {
    await this.isVisible(this.mediaStartNode.locator(`[name="${name}"]`));
  }

  async clickUsersMenu() {
    await this.click(this.usersMenu);
  }

  async goToUsers() {
    await this.goToSection(ConstantHelper.sections.users);
    await this.clickUsersMenu();
  }

  async goToUserWithName(name: string) {
    await this.goToSection(ConstantHelper.sections.users);
    await this.clickUsersMenu();
    await this.searchInUserSection(name);
    await this.clickUserWithName(name);
    await this.hasValue(this.nameOfUserInput, name);
  }
  
  async clickUserButton() {
    await this.click(this.userBtn);
  }

  async isGoToProfileButtonVisible(isVisible: boolean = true) {
    await this.isVisible(this.goToProfileBtn, isVisible);
  }

  async clickAPIUserButton() {
    await this.click(this.apiUserBtn);
  }

  async clickSaveButtonAndWaitForUserToBeCreated() {
    return await this.waitForResponseAfterExecutingPromise(ConstantHelper.apiEndpoints.user, this.clickSaveButton(), ConstantHelper.statusCodes.created);
  }

  async clickSaveButtonAndWaitForUserToBeUpdated() {
    return await this.waitForResponseAfterExecutingPromise(ConstantHelper.apiEndpoints.user, this.clickSaveButton(), ConstantHelper.statusCodes.ok);
  }

  async clickConfirmToDeleteButtonAndWaitForUserToBeDeleted() {
    return await this.waitForResponseAfterExecutingPromise(ConstantHelper.apiEndpoints.user, this.clickConfirmToDeleteButton(), ConstantHelper.statusCodes.ok);
  }

  async clickCreateUserButtonAndWaitForUserToBeCreated() {
    return await this.waitForResponseAfterExecutingPromise(ConstantHelper.apiEndpoints.user, this.clickCreateUserButton(), ConstantHelper.statusCodes.created);
  }

  async doesUserGroupPickerHaveDetails(userGroupName: string, details: string) {
    const userGroupRefLocator = this.page.locator('umb-user-group-ref', {hasText: userGroupName});
    const detailsLocator = userGroupRefLocator.locator('#details');
    return await this.containsText(detailsLocator, details);
  }
}