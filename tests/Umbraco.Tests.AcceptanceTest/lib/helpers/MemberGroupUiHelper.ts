import {Page, Locator} from "@playwright/test";
import {UiBaseLocators} from "./UiBaseLocators";
import {ConstantHelper} from "./ConstantHelper";

export class MemberGroupUiHelper extends UiBaseLocators {
  private readonly memberGroupsTab: Locator;
  private readonly memberGroupNameTxt: Locator;
  private readonly memberGroupView: Locator;
  private readonly activeMemberGroupsTab: Locator;
  private readonly createMemberGroupBtn: Locator;
  private readonly memberGroupsMenu: Locator;
  private readonly membersSidebar: Locator;
  private readonly memberGroupsSidebarBtn: Locator;

  constructor(page: Page) {
    super(page);
    this.memberGroupsTab = page.locator('uui-tab[label="Member Groups"]');
    this.memberGroupNameTxt = page.locator('input#input');
    this.memberGroupView = page.locator('umb-member-group-table-collection-view');
    this.activeMemberGroupsTab = page.locator('uui-tab[label="Member Groups"][active]');
    this.createMemberGroupBtn = page.getByTestId('collection-action:Umb.CollectionAction.MemberGroup.Create').getByLabel('Create', {exact: true});
    this.memberGroupsMenu = page.locator('umb-menu').getByLabel('Member Groups', {exact: true});
    this.membersSidebar = page.getByTestId('section-sidebar:Umb.SectionSidebarApp.Menu.MemberManagement');
    this.memberGroupsSidebarBtn = this.membersSidebar.locator('uui-menu-item').filter({hasText: 'Member Groups'});
  }

  async clickMemberGroupsTab() {
    await this.waitForVisible(this.memberGroupsTab);
    await this.page.waitForTimeout(ConstantHelper.wait.short);
    await this.click(this.memberGroupsTab);
    await this.waitForVisible(this.activeMemberGroupsTab);
  }

  async clickMemberGroupCreateButton() {
    await this.click(this.createMemberGroupBtn);
  }

  async clickMemberGroupsSidebarButton() {
    await this.click(this.memberGroupsSidebarBtn);
  }

  async enterMemberGroupName(name: string) {
    await this.enterText(this.memberGroupNameTxt, name);
  }

  async clickMemberGroupLinkByName(memberGroupName: string) {
    await this.click(this.page.getByRole('link', {name: memberGroupName}));
  }

  async isMemberGroupNameVisible(memberGroupName: string, isVisible: boolean = true) {
    const memberGroupNameLocator = this.memberGroupView.filter({hasText: memberGroupName});
    return await this.isVisible(memberGroupNameLocator, isVisible, ConstantHelper.wait.short);
  }

  async clickMemberGroupsMenu() {
    await this.click(this.memberGroupsMenu);
  }

  async goToMemberGroups() {
    await this.goToSection(ConstantHelper.sections.members);
    await this.clickMemberGroupsMenu();
  }

  async clickSaveButtonAndWaitForMemberGroupToBeCreated() {
    return await this.waitForResponseAfterExecutingPromise(ConstantHelper.apiEndpoints.memberGroup, this.clickSaveButton(), ConstantHelper.statusCodes.created);
  }

  async clickSaveButtonAndWaitForMemberGroupToBeUpdated() {
    return await this.waitForResponseAfterExecutingPromise(ConstantHelper.apiEndpoints.memberGroup, this.clickSaveButton(), ConstantHelper.statusCodes.ok);
  }

  async clickConfirmToDeleteButtonAndWaitForMemberGroupToBeDeleted() {
    return await this.waitForResponseAfterExecutingPromise(ConstantHelper.apiEndpoints.memberGroup, this.clickConfirmToDeleteButton(), ConstantHelper.statusCodes.ok);
  }
}
