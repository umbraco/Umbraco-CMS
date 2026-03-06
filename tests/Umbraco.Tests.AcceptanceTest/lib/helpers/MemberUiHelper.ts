import {Page, Locator, expect} from "@playwright/test";
import {UiBaseLocators} from "./UiBaseLocators";
import {ConstantHelper} from "./ConstantHelper";

export class MemberUiHelper extends UiBaseLocators {
  private readonly membersTab: Locator;
  private readonly memberNameTxt: Locator;
  private readonly commentsTxt: Locator;
  private readonly detailsTab: Locator;
  private readonly usernameTxt: Locator;
  private readonly emailTxt: Locator;
  private readonly passwordTxt: Locator;
  private readonly confirmNewPasswordTxt: Locator;
  private readonly approvedToggle: Locator;
  private readonly lockedOutToggle: Locator;
  private readonly twoFactorAuthenticationToggle: Locator;
  private readonly memberInfoItems: Locator;
  private readonly changePasswordBtn: Locator;
  private readonly membersMenu: Locator;
  private readonly infoTab: Locator;
  private readonly membersCreateBtn: Locator;
  private readonly membersSidebar: Locator;
  private readonly membersSidebarBtn: Locator;
  private readonly memberTableCollectionRow: Locator;

  constructor(page: Page) {
    super(page);
    this.membersTab = page.locator('uui-tab[label="Members"]');
    this.memberNameTxt = page.locator('#name-input #input');
    this.commentsTxt = page.locator('umb-content-workspace-view-edit-tab').locator('umb-property').filter({hasText: 'Comments'}).locator('#textarea');
    this.detailsTab = page.locator('uui-tab').filter({hasText: 'Details'}).locator('svg');
    this.usernameTxt = page.getByLabel('Username', {exact: true});
    this.emailTxt = page.getByLabel('Email', {exact: true});
    this.passwordTxt = page.getByLabel('Enter your new password', {exact: true});
    this.confirmNewPasswordTxt = page.getByLabel('Confirm new password', {exact: true});
    this.approvedToggle = page.locator('[label="Approved"] #toggle');
    this.lockedOutToggle = page.locator('[label="Locked out"] #toggle');
    this.twoFactorAuthenticationToggle = page.locator('[label="Two-Factor authentication"] #toggle');
    this.memberInfoItems = page.locator('umb-stack > div');
    this.changePasswordBtn = page.getByLabel('Change password', {exact: true});
    this.membersMenu = page.locator('umb-menu').getByLabel('Members', {exact: true});
    this.infoTab = page.locator('uui-tab').filter({hasText: 'Info'}).locator('svg');
    this.membersCreateBtn = page.locator('umb-create-member-collection-action').getByLabel('Create', {exact: true});
    this.membersSidebar = page.getByTestId('section-sidebar:Umb.SectionSidebarApp.Menu.MemberManagement');
    this.membersSidebarBtn = this.membersSidebar.locator('uui-menu-item').filter({hasText: 'Members'});
    this.memberTableCollectionRow = page.locator('umb-member-table-collection-view').locator('uui-table-row');
  }

  async clickMembersTab() {
    await this.click(this.membersTab);
  }

  async clickDetailsTab() {
    await this.click(this.detailsTab);
  }

  async clickMemberLinkByName(memberName: string) {
    await this.click(this.page.getByRole('link', {name: memberName}));
  }

  async isMemberWithNameVisible(memberName: string, isVisible: boolean = true) {
    await this.isVisible(this.memberTableCollectionRow.getByText(memberName, {exact: true}), isVisible);
  }

  async clickMembersSidebarButton() {
    await this.click(this.membersSidebarBtn);
  }

  async enterMemberName(name: string) {
    await this.enterText(this.memberNameTxt, name);
  }

  async enterComments(comment: string) {
    await this.enterText(this.commentsTxt, comment);
  }

  async enterUsername(username: string) {
    await this.enterText(this.usernameTxt, username);
  }

  async enterEmail(email: string) {
    await this.enterText(this.emailTxt, email);
  }

  async enterPassword(password: string) {
    await this.enterText(this.passwordTxt, password);
  }

  async enterConfirmPassword(password: string) {
    await this.enterText(this.confirmPasswordTxt, password);
  }

  async enterConfirmNewPassword(password: string) {
    await this.enterText(this.confirmNewPasswordTxt, password);
  }

  async chooseMemberGroup(memberGroupName: string) {
    await this.clickChooseButton();
    await this.click(this.page.getByText(memberGroupName, {exact: true}));
    await this.clickChooseContainerButton();
  }

  async doesMemberInfoHaveValue(infoName: string, value: string) {
    return expect(this.memberInfoItems.filter({hasText: infoName}).locator('span')).toHaveText(value);
  }

  async clickApprovedToggle() {
    await this.click(this.approvedToggle);
  }

  async clickLockedOutToggle() {
    await this.click(this.lockedOutToggle);
  }

  async clickTwoFactorAuthenticationToggle() {
    await this.click(this.twoFactorAuthenticationToggle);
  }

  async clickChangePasswordButton() {
    await this.click(this.changePasswordBtn);
  }

  async clickRemoveMemberGroupByName(memberGroupName: string) {
    await this.click(this.page.locator(`[name="${memberGroupName}"]`).getByLabel('Remove'));
  }

  async enterNewPassword(password: string) {
    await this.enterText(this.newPasswordTxt, password);
  }

  async clickMembersMenu() {
    await this.click(this.membersMenu);
  }
  
  async goToMembers() {
    await this.goToSection(ConstantHelper.sections.members);
    await this.clickMembersMenu();
  }

  async clickInfoTab() {
    await this.click(this.infoTab);
  }

  async clickCreateMembersButton() {
    await this.click(this.membersCreateBtn);
  }

  async clickSaveButtonAndWaitForMemberToBeCreated() {
    return await this.waitForResponseAfterExecutingPromise(ConstantHelper.apiEndpoints.member, this.clickSaveButton(), ConstantHelper.statusCodes.created);
  }

  async clickSaveButtonAndWaitForMemberToBeUpdated() {
    return await this.waitForResponseAfterExecutingPromise(ConstantHelper.apiEndpoints.member, this.clickSaveButton(), ConstantHelper.statusCodes.ok);
  }

  async clickConfirmToDeleteButtonAndWaitForMemberToBeDeleted() {
    return await this.waitForResponseAfterExecutingPromise(ConstantHelper.apiEndpoints.member, this.clickConfirmToDeleteButton(), ConstantHelper.statusCodes.ok);
  }
}
