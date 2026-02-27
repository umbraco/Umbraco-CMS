import {expect, Locator, Page, Response} from "@playwright/test"
import {ConstantHelper} from "./ConstantHelper";
import {BasePage} from "./BasePage";

export class UiBaseLocators extends BasePage {
  // Core Action Buttons
  public readonly saveBtn: Locator;
  public readonly submitBtn: Locator;
  public readonly confirmBtn: Locator;
  public readonly chooseBtn: Locator;
  public readonly chooseModalBtn: Locator;
  public readonly createBtn: Locator;
  public readonly addBtn: Locator;
  public readonly updateBtn: Locator;
  public readonly changeBtn: Locator;
  public readonly deleteBtn: Locator;
  public readonly deleteExactBtn: Locator;
  public readonly removeExactBtn: Locator;
  public readonly insertBtn: Locator;
  public readonly renameBtn: Locator;
  public readonly reloadBtn: Locator;
  public readonly reloadChildrenBtn: Locator;
  public readonly restoreBtn: Locator;
  public readonly disableBtn: Locator;
  public readonly enableBtn: Locator;
  public readonly actionBtn: Locator;
  public readonly nextBtn: Locator;

  // Confirmation Buttons
  public readonly confirmToDeleteBtn: Locator;
  public readonly confirmCreateFolderBtn: Locator;
  public readonly confirmToRemoveBtn: Locator;
  public readonly confirmToSubmitBtn: Locator;
  public readonly confirmDisableBtn: Locator;
  public readonly confirmEnableBtn: Locator;
  public readonly confirmRenameBtn: Locator;
  public readonly confirmTrashBtn: Locator;

  // Folder Management
  public readonly createFolderBtn: Locator;
  public readonly folderNameTxt: Locator;
  public readonly folderBtn: Locator;
  public readonly newFolderThreeDotsBtn: Locator;
  public readonly renameFolderThreeDotsBtn: Locator;
  public readonly renameFolderBtn: Locator;
  public readonly updateFolderBtn: Locator;
  public readonly deleteFolderThreeDotsBtn: Locator;

  // Navigation & Menu
  public readonly breadcrumbBtn: Locator;
  public readonly leftArrowBtn: Locator;
  public readonly caretBtn: Locator;
  public readonly modalCaretBtn: Locator;
  public readonly backOfficeHeader: Locator;
  public readonly backOfficeMain: Locator;
  public readonly sectionLinks: Locator;
  public readonly sectionSidebar: Locator;
  public readonly menuItem: Locator;
  public readonly actionsMenuContainer: Locator;
  public readonly treeItem: Locator;

  // Three Dots Menu Buttons
  public readonly createThreeDotsBtn: Locator;
  public readonly renameThreeDotsBtn: Locator;
  public readonly deleteThreeDotsBtn: Locator;

  // Modal & Container
  public readonly sidebarModal: Locator;
  public readonly sidebarSaveBtn: Locator;
  public readonly openedModal: Locator;
  public readonly container: Locator;
  public readonly containerChooseBtn: Locator;
  public readonly containerSaveAndPublishBtn: Locator;
  public readonly createModalBtn: Locator;

  // Document Type & Property Editor
  public readonly documentTypeNode: Locator;
  public readonly propertyNameTxt: Locator;
  public readonly selectPropertyEditorBtn: Locator;
  public readonly editorSettingsBtn: Locator;
  public readonly enterPropertyEditorDescriptionTxt: Locator;
  public readonly property: Locator;
  public readonly addPropertyBtn: Locator;
  public readonly labelAboveBtn: Locator;

  // Group & Tab Management
  public readonly addGroupBtn: Locator;
  public readonly groupLabel: Locator;
  public readonly typeGroups: Locator;
  public readonly addTabBtn: Locator;
  public readonly unnamedTabTxt: Locator;
  public readonly structureTabBtn: Locator;

  // Validation & Mandatory
  public readonly mandatoryToggle: Locator;
  public readonly validation: Locator;
  public readonly regexTxt: Locator;
  public readonly regexMessageTxt: Locator;
  public readonly validationMessage: Locator;

  // Composition & Structure
  public readonly compositionsBtn: Locator;
  public readonly allowAtRootBtn: Locator;
  public readonly allowedChildNodesModal: Locator;
  public readonly addCollectionBtn: Locator;

  // Reorder
  public readonly iAmDoneReorderingBtn: Locator;
  public readonly reorderBtn: Locator;

  // Query Builder
  public readonly queryBuilderBtn: Locator;
  public readonly queryBuilderOrderedBy: Locator;
  public readonly queryBuilderCreateDate: Locator;
  public readonly queryBuilderShowCode: Locator;
  public readonly wherePropertyAliasBtn: Locator;
  public readonly whereOperatorBtn: Locator;
  public readonly whereConstrainValueTxt: Locator;
  public readonly orderByPropertyAliasBtn: Locator;
  public readonly ascendingBtn: Locator;
  public readonly chooseRootContentBtn: Locator;
  public readonly returnedItemsCount: Locator;
  public readonly queryResults: Locator;

  // Insert & Template
  public readonly insertValueBtn: Locator;
  public readonly insertPartialViewBtn: Locator;
  public readonly insertDictionaryItemBtn: Locator;
  public readonly chooseFieldDropDown: Locator;
  public readonly systemFieldsOption: Locator;
  public readonly chooseFieldValueDropDown: Locator;
  public readonly breadcrumbsTemplateModal: Locator;

  // Rename
  public readonly newNameTxt: Locator;
  public readonly renameModalBtn: Locator;

  // State & Notification
  public readonly successState: Locator;
  public readonly successStateIcon: Locator;
  public readonly failedStateButton: Locator;
  public readonly successNotification: Locator;
  public readonly errorNotification: Locator;

  // Search & Filter
  public readonly typeToFilterSearchTxt: Locator;
  public readonly filterChooseBtn: Locator;
  public readonly searchTxt: Locator;

  // Text Input
  public readonly textAreaInputArea: Locator;
  public readonly enterAName: Locator;
  public readonly descriptionBtn: Locator;
  public readonly enterDescriptionTxt: Locator;
  public readonly aliasLockBtn: Locator;
  public readonly aliasNameTxt: Locator;

  // Icon
  public readonly iconBtn: Locator;

  // Create Link
  public readonly createLink: Locator;

  // Recycle Bin
  public readonly recycleBinBtn: Locator;
  public readonly recycleBinMenuItem: Locator;
  public readonly recycleBinMenuItemCaretBtn: Locator;

  // View Options
  public readonly gridBtn: Locator;
  public readonly listBtn: Locator;
  public readonly viewBundleBtn: Locator;

  // Media
  public readonly mediaCardItems: Locator;
  public readonly mediaPickerModalSubmitBtn: Locator;
  public readonly mediaCaptionAltTextModalSubmitBtn: Locator;
  public readonly clickToUploadBtn: Locator;
  public readonly inputDropzone: Locator;
  public readonly imageCropperField: Locator;
  public readonly inputUploadField: Locator;
  public readonly chooseMediaInputBtn: Locator;

  // Embedded Media
  public readonly embeddedMediaModal: Locator;
  public readonly embeddedURLTxt: Locator;
  public readonly embeddedRetrieveBtn: Locator;
  public readonly embeddedMediaModalConfirmBtn: Locator;
  public readonly embeddedPreview: Locator;

  // Document & Content
  public readonly chooseDocumentInputBtn: Locator;
  public readonly createDocumentBlueprintBtn: Locator;
  public readonly createDocumentBlueprintModal: Locator;
  public readonly createNewDocumentBlueprintBtn: Locator;

  // User
  public readonly currentUserAvatarBtn: Locator;
  public readonly newPasswordTxt: Locator;
  public readonly confirmPasswordTxt: Locator;
  public readonly currentPasswordTxt: Locator;

  // Collection & Table
  public readonly collectionTreeItemTableRow: Locator;
  public readonly createActionButtonCollection: Locator;
  public readonly createActionBtn: Locator;
  public readonly createOptionActionListModal: Locator;

  // Reference & Entity
  public readonly confirmActionModalEntityReferences: Locator;
  public readonly referenceHeadline: Locator;
  public readonly entityItemRef: Locator;
  public readonly entityItem: Locator;

  // Workspace & Action
  public readonly workspaceAction: Locator;
  public readonly workspaceActionMenuBtn: Locator;
  public readonly entityAction: Locator;
  public readonly openEntityAction: Locator;

  // Pagination
  public readonly firstPaginationBtn: Locator;
  public readonly nextPaginationBtn: Locator;

  // Editor
  public readonly monacoEditor: Locator;

  // Loader
  public readonly uiLoader: Locator;
  
  // Block
  public readonly blockTypeCard: Locator;

  constructor(page: Page) {
    super(page);

    // Core Action Buttons
    this.saveBtn = page.getByLabel('Save', {exact: true});
    this.submitBtn = page.getByLabel('Submit');
    this.confirmBtn = page.getByLabel('Confirm');
    this.chooseBtn = page.getByLabel('Choose', {exact: true});
    this.chooseModalBtn = page.locator('uui-modal-sidebar').locator('[look="primary"]').getByLabel('Choose');
    this.createBtn = page.getByRole('button', {name: /^Create(…)?$/});
    this.addBtn = page.getByRole('button', {name: 'Add', exact: true});
    this.updateBtn = page.getByLabel('Update');
    this.changeBtn = page.getByLabel('Change');
    this.deleteBtn = page.getByRole('button', {name: /^Delete(…)?$/});
    this.deleteExactBtn = page.getByRole('button', {name: 'Delete', exact: true});
    this.removeExactBtn = page.getByLabel('Remove', {exact: true});
    this.insertBtn = page.locator('uui-box uui-button').filter({hasText: 'Insert'});
    this.renameBtn = page.getByRole('button', {name: /^Rename(…)?$/});
    this.reloadBtn = page.getByRole('button', {name: 'Reload', exact: true});
    this.reloadChildrenBtn = page.getByRole('button', {name: 'Reload children'});
    this.restoreBtn = page.getByLabel('Restore', {exact: true});
    this.disableBtn = page.getByLabel('Disable', {exact: true});
    this.enableBtn = page.getByLabel('Enable');
    this.actionBtn = page.getByTestId('workspace:action-menu-button');
    this.nextBtn = page.getByLabel('Next');
  
    // Confirmation Buttons
    this.confirmToDeleteBtn = page.locator('#confirm').getByLabel('Delete');
    this.confirmCreateFolderBtn = page.locator('#confirm').getByLabel('Create Folder');
    this.confirmToRemoveBtn = page.locator('#confirm').getByLabel('Remove');
    this.confirmToSubmitBtn = page.locator('#confirm').getByLabel('Submit');
    this.confirmDisableBtn = page.locator('#confirm').getByLabel('Disable');
    this.confirmEnableBtn = page.locator('#confirm').getByLabel('Enable');
    this.confirmRenameBtn = page.locator('#confirm').getByLabel('Rename');
    this.confirmTrashBtn = page.locator('#confirm').getByLabel('Trash');
  
    // Folder Management
    this.createFolderBtn = page.getByLabel('Create folder');
    this.folderNameTxt = page.getByLabel('Enter a folder name');
    this.folderBtn = page.locator('umb-entity-create-option-action-list-modal').locator('umb-ref-item', {hasText: 'Folder'});
    this.newFolderThreeDotsBtn = page.getByLabel('New Folder…');
    this.renameFolderThreeDotsBtn = page.getByRole('button', {name: 'Rename folder…'});
    this.renameFolderBtn = page.getByLabel('Rename folder');
    this.updateFolderBtn = page.getByLabel('Update folder');
    this.deleteFolderThreeDotsBtn = page.locator('#action-modal').getByLabel('Delete Folder...');
  
    // Navigation & Menu
    this.breadcrumbBtn = page.getByLabel('Breadcrumb');
    this.leftArrowBtn = page.locator('[name="icon-arrow-left"] svg');
    this.caretBtn = page.locator('#caret-button');
    this.modalCaretBtn = page.locator('uui-modal-sidebar').locator('#caret-button');
    this.backOfficeHeader = page.locator('umb-backoffice-header');
    this.backOfficeMain = page.locator('umb-backoffice-main');
    this.sectionLinks = page.getByTestId('section-links');
    this.sectionSidebar = page.locator('umb-section-sidebar');
    this.menuItem = page.locator('uui-menu-item');
    this.actionsMenuContainer = page.locator('uui-scroll-container');
    this.treeItem = page.locator('umb-tree-item');

    // Three Dots Menu Buttons
    this.createThreeDotsBtn = page.getByText('Create…', {exact: true});
    this.renameThreeDotsBtn = page.getByLabel('Rename…', {exact: true});
    this.deleteThreeDotsBtn = page.getByLabel('Delete…');
  
    // Modal & Container
    this.sidebarModal = page.locator('uui-modal-sidebar');
    this.sidebarSaveBtn = this.sidebarModal.getByLabel('Save', {exact: true});
    this.openedModal = page.locator('uui-modal-container[backdrop]');
    this.container = page.locator('#container');
    this.containerChooseBtn = page.locator('#container').getByLabel('Choose');
    this.containerSaveAndPublishBtn = page.locator('#container').getByLabel('Save and Publish');
    this.createModalBtn = page.locator('uui-modal-sidebar').getByLabel('Create', {exact: true});
  
    // Document Type & Property Editor
    this.documentTypeNode = page.locator('uui-ref-node-document-type');
    this.propertyNameTxt = page.getByTestId('input:entity-name').locator('#input').first();
    this.selectPropertyEditorBtn = page.getByLabel('Select Property Editor');
    this.editorSettingsBtn = page.getByLabel('Editor settings');
    this.enterPropertyEditorDescriptionTxt = page.locator('uui-modal-sidebar').getByTestId('input:entity-description').locator('#textarea');
    this.property = page.locator('umb-property');
    this.addPropertyBtn = page.getByLabel('Add property', {exact: true});
    this.labelAboveBtn = page.locator('.appearance-option').filter({hasText: 'Label above'});
  
    // Group & Tab Management
    this.addGroupBtn = page.getByLabel('Add group', {exact: true});
    this.groupLabel = page.getByLabel('Group', {exact: true});
    this.typeGroups = page.locator('umb-content-type-design-editor-group');
    this.addTabBtn = page.getByLabel('Add tab');
    this.unnamedTabTxt = page.getByTestId('tab:').getByTestId('tab:name-input').locator('#input');
    this.structureTabBtn = page.locator('uui-tab').filter({hasText: 'Structure'}).locator('svg');
  
    // Validation & Mandatory
    this.mandatoryToggle = page.locator('#mandatory #toggle');
    this.validation = page.locator('#native');
    this.regexTxt = page.locator('input[name="pattern"]');
    this.regexMessageTxt = page.locator('textarea[name="pattern-message"]');
    this.validationMessage = page.locator('umb-form-validation-message').locator('#messages');
  
    // Composition & Structure
    this.compositionsBtn = page.getByLabel('Compositions');
    this.allowAtRootBtn = page.locator('label').filter({hasText: 'Allow at root'});
    this.allowedChildNodesModal = page.locator('umb-tree-picker-modal');
    this.addCollectionBtn = page.locator('umb-input-content-type-collection-configuration #create-button');
  
    // Reorder
    this.iAmDoneReorderingBtn = page.getByLabel('I am done reordering');
    this.reorderBtn = page.getByLabel('Reorder');
  
    // Query Builder
    this.queryBuilderBtn = page.locator('#query-builder-button');
    this.queryBuilderOrderedBy = page.locator('#property-alias-dropdown').getByLabel('Property alias');
    this.queryBuilderCreateDate = page.locator('#property-alias-dropdown').getByText('CreateDate').locator("..");
    this.queryBuilderShowCode = page.locator('umb-code-block');
    this.wherePropertyAliasBtn = page.locator('#property-alias-dropdown');
    this.whereOperatorBtn = page.locator('#operator-dropdown');
    this.whereConstrainValueTxt = page.getByLabel('constrain value');
    this.orderByPropertyAliasBtn = page.locator('#sort-dropdown');
    this.ascendingBtn = page.locator('[key="template_ascending"]');
    this.chooseRootContentBtn = page.getByLabel('Choose root document');
    this.returnedItemsCount = page.locator('#results-count');
    this.queryResults = page.locator('.query-results');
  
    // Insert & Template
    this.insertValueBtn = page.locator('uui-button').filter({has: page.locator('[key="template_insertPageField"]')});
    this.insertPartialViewBtn = page.locator('uui-button').filter({has: page.locator('[key="template_insertPartialView"]')});
    this.insertDictionaryItemBtn = page.locator('uui-button').filter({has: page.locator('[key="template_insertDictionaryItem"]')});
    this.chooseFieldDropDown = page.locator('#preview #expand-symbol-wrapper');
    this.systemFieldsOption = page.getByText('System fields');
    this.chooseFieldValueDropDown = page.locator('#value #expand-symbol-wrapper');
    this.breadcrumbsTemplateModal = page.locator('uui-modal-sidebar').locator('umb-template-workspace-editor uui-breadcrumbs');
  
    // Rename
    this.newNameTxt = page.getByRole('textbox', {name: 'Enter new name...'});
    this.renameModalBtn = page.locator('umb-rename-modal').getByLabel('Rename');
  
    // State & Notification
    this.successState = page.locator('[state="success"]');
    this.successStateIcon = page.locator('[state="success"]').locator('#state');
    this.failedStateButton = page.locator('uui-button[state="failed"]');
    this.successNotification = page.locator('uui-toast-notification[open][color="positive"]');
    this.errorNotification = page.locator('uui-toast-notification[open][color="danger"]');
  
    // Search & Filter
    this.typeToFilterSearchTxt = page.locator('[type="search"] #input');
    this.filterChooseBtn = page.locator('button').filter({hasText: 'Choose'});
    this.searchTxt = this.page.locator('umb-collection-filter-field').locator('#input');
  
    // Text Input
    this.textAreaInputArea = page.locator('textarea.ime-text-area');
    this.enterAName = page.getByLabel('Enter a name...', {exact: true});
    this.descriptionBtn = page.getByLabel('Description');
    this.enterDescriptionTxt = page.getByLabel('Enter a description...');
    this.aliasLockBtn = page.locator('#name').getByLabel('Unlock input');
    this.aliasNameTxt = page.locator('#name').getByLabel('alias');
  
    // Icon
    this.iconBtn = page.getByLabel('icon');
  
    // Create Link
    this.createLink = page.getByRole('link', {name: 'Create', exact: true});
  
    // Recycle Bin
    this.recycleBinBtn = page.getByLabel('Recycle Bin', {exact: true});
    this.recycleBinMenuItem = page.locator('uui-menu-item[label="Recycle Bin"]');
    this.recycleBinMenuItemCaretBtn = page.locator('uui-menu-item[label="Recycle Bin"]').locator('#caret-button');
  
    // View Options
    this.gridBtn = page.getByLabel('Grid');
    this.listBtn = page.getByLabel('List');
    this.viewBundleBtn = page.locator('umb-collection-view-bundle uui-button svg');
  
    // Media
    this.mediaCardItems = page.locator('uui-card-media');
    this.mediaPickerModalSubmitBtn = page.locator('umb-media-picker-modal').getByLabel('Submit');
    this.mediaCaptionAltTextModalSubmitBtn = page.locator('umb-media-caption-alt-text-modal').getByLabel('Submit');
    this.clickToUploadBtn = page.locator('#splitViews').getByRole('button', {name: 'Click to upload'});
    this.inputDropzone = page.locator('umb-input-dropzone');
    this.imageCropperField = page.locator('umb-image-cropper-field');
    this.inputUploadField = page.locator('umb-input-upload-field').locator('#wrapperInner');
    this.chooseMediaInputBtn = page.locator('umb-input-media').getByLabel('Choose');

    // Embedded Media
    this.embeddedMediaModal = page.locator('umb-embedded-media-modal');
    this.embeddedURLTxt = page.locator('umb-embedded-media-modal').locator('[label="URL"] #input');
    this.embeddedRetrieveBtn = page.locator('umb-embedded-media-modal').locator('[label="Retrieve"]');
    this.embeddedMediaModalConfirmBtn = page.locator('umb-embedded-media-modal').getByLabel('Confirm');
    this.embeddedPreview = page.locator('umb-embedded-media-modal').locator('[label="Preview"]');
  
    // Document & Content
    this.chooseDocumentInputBtn = page.locator('umb-input-document').getByLabel('Choose');
    this.createDocumentBlueprintBtn = page.getByLabel(/^Create Document Blueprint(…)?$/);
    this.createDocumentBlueprintModal = page.locator('umb-document-blueprint-options-create-modal');
    this.createNewDocumentBlueprintBtn = page.locator('umb-document-blueprint-options-create-modal').locator('umb-ref-item', {hasText: 'Document Blueprint for'});
  
    // User
    this.currentUserAvatarBtn = page.getByTestId('header-app:Umb.HeaderApp.CurrentUser').locator('uui-avatar');
    this.currentPasswordTxt = page.locator('input[name="oldPassword"]');
    this.newPasswordTxt = page.locator('input[name="newPassword"]');
    this.confirmPasswordTxt = page.locator('input[name="confirmPassword"]');
  
    // Collection & Table
    this.collectionTreeItemTableRow = page.locator('umb-collection-workspace-view umb-table uui-table-row');
    this.createActionButtonCollection = page.locator('umb-collection-create-action-button');
    this.createActionBtn = page.locator('umb-collection-create-action-button').locator('[label="Create"]');
    this.createOptionActionListModal = page.locator('umb-entity-create-option-action-list-modal');
  
    // Reference & Entity
    this.confirmActionModalEntityReferences = page.locator('umb-confirm-action-modal-entity-references,umb-confirm-bulk-action-modal-entity-references');
    this.referenceHeadline = page.locator('umb-confirm-action-modal-entity-references,umb-confirm-bulk-action-modal-entity-references').locator('#reference-headline').first();
    this.entityItemRef = page.locator('umb-confirm-action-modal-entity-references,umb-confirm-bulk-action-modal-entity-references').locator('uui-ref-list').first().getByTestId('entity-item-ref');
    this.entityItem = page.locator('umb-entity-item-ref');
  
    // Workspace & Action
    this.workspaceAction = page.locator('umb-workspace-action');
    this.workspaceActionMenuBtn = page.getByTestId('workspace:action-menu-button');
    this.entityAction = page.locator('umb-entity-action-list umb-entity-action');
    this.openEntityAction = page.locator('#action-modal[open]').locator(page.locator('umb-entity-action-list umb-entity-action'));
  
    // Pagination
    this.firstPaginationBtn = page.locator('umb-collection-pagination').getByLabel('First');
    this.nextPaginationBtn = page.locator('umb-collection-pagination').getByLabel('Next');
  
    // Editor
    this.monacoEditor = page.locator('.monaco-editor');
  
    // Loader
    this.uiLoader = page.locator('uui-loader');

    // Block
    this.blockTypeCard = page.locator('uui-card-block-type');
  }

  // Helper Methods
  getMenuItemByLabel(name: string): Locator {
    return this.page.locator(`uui-menu-item[label="${name}"]`);
  }

  // Actions Menu Methods
  async clickActionsMenuForNameInSectionSidebar(name: string) {
    await this.sectionSidebar.locator('[label="' + name + '"]').hover();
    await this.click(this.sectionSidebar.locator('[label="' + name + '"] >> [label="Open actions menu"]').first());
  }

  async clickActionsMenuForName(name: string) {
    const menuItem = this.getMenuItemByLabel(name);
    await this.page.waitForTimeout(ConstantHelper.wait.medium);
    const menuItemFirstLocator = menuItem.locator('#menu-item').first();
    const actionModalLocator = menuItem.locator('#action-modal').first();
    await this.hover(menuItemFirstLocator, {force: true});
    await this.click(actionModalLocator, {force: true});
  }

  async isActionsMenuForNameVisible(name: string, isVisible = true) {
    const menuItem = this.getMenuItemByLabel(name);
    await this.click(menuItem);
    await this.isVisible(menuItem.locator('#action-modal').first(), isVisible);
  }

  // Caret Button Methods
  async clickCaretButtonForName(name: string) {
    await this.isCaretButtonWithNameVisible(name);
    await this.click(this.getMenuItemByLabel(name).locator('#caret-button').first());
  }

  async isCaretButtonWithNameVisible(name: string, isVisible = true) {
    const caretButton = this.getMenuItemByLabel(name).locator('#caret-button').first();
    await this.isVisible(caretButton, isVisible);
  }

  async clickCaretButton() {
    await this.click(this.caretBtn);
  }

  async openCaretButtonForName(name: string, isInModal: boolean = false) {
    let menuItem: Locator;
    if (isInModal) {
      menuItem = this.sidebarModal.locator(`uui-menu-item[label="${name}"]`);
    } else {
      menuItem = this.getMenuItemByLabel(name);
    }
    await this.waitForVisible(menuItem, ConstantHelper.timeout.long);
    const isCaretButtonOpen = await menuItem.getAttribute('show-children');
    if (isCaretButtonOpen === null) {
      await this.clickCaretButtonForName(name);
    }
  }

  // Tree Methods
  async reloadTree(treeName: string) {
    await this.isVisible(this.page.getByLabel(treeName, {exact: true}));
    await this.page.waitForTimeout(ConstantHelper.wait.short);
    await this.clickActionsMenuForName(treeName);
    await this.clickReloadChildrenActionMenuOption();
    await this.openCaretButtonForName(treeName);
  }

  async isTreeItemVisible(name: string, isVisible = true) {
    await this.isVisible(this.treeItem.locator('[label="' + name + '"]'), isVisible);
  }

  async doesTreeItemHaveTheCorrectIcon(name: string, icon: string) {
    return await this.isVisible(this.treeItem.filter({hasText: name}).locator('umb-icon').locator('[name="' + icon + '"]'));
  }

  // Core Button Click Methods
  async clickReloadButton() {
    await this.click(this.reloadBtn);
  }

  async clickReloadChildrenButton() {
    await this.click(this.reloadChildrenBtn, {force: true});
  }

  async clickSaveButton() {
    await this.click(this.saveBtn);
    await this.page.waitForTimeout(ConstantHelper.wait.short);
  }

  async clickChooseButton() {
    await this.click(this.chooseBtn);
  }

  async clickChooseContainerButton() {
    await this.click(this.containerChooseBtn);
  }

  async clickSubmitButton() {
    await this.click(this.submitBtn);
  }

  async clickConfirmButton() {
    await this.click(this.confirmBtn);
  }

  async clickCreateButton() {
    await this.click(this.createBtn);
  }

  async clickAddButton() {
    await this.click(this.addBtn);
  }

  async clickUpdateButton() {
    await this.click(this.updateBtn);
  }

  async clickChangeButton() {
    await this.click(this.changeBtn);
  }

  async clickDeleteButton() {
    await this.click(this.deleteBtn);
  }

  async clickDeleteExactButton() {
    await this.click(this.deleteExactBtn);
  }

  async clickRemoveExactButton() {
    await this.click(this.removeExactBtn);
  }

  async clickInsertButton() {
    await this.click(this.insertBtn);
  }

  async clickRenameButton() {
    await this.click(this.renameBtn);
  }

  async clickRestoreButton() {
    await this.click(this.restoreBtn);
  }

  async clickDisableButton() {
    await this.click(this.disableBtn);
  }

  async clickEnableButton() {
    await this.click(this.enableBtn);
  }

  async clickActionButton() {
    // Sometimes this button is clicked before it is visible, resulting in flaky tests
    await this.waitForTimeout(ConstantHelper.wait.short);
    await this.click(this.actionBtn);
  }

  async clickNextButton() {
    await this.click(this.nextBtn);
  }

  async clickBreadcrumbButton() {
    await this.click(this.breadcrumbBtn);
    await this.waitForTimeout(ConstantHelper.wait.short); 
  }

  async clickLeftArrowButton() {
    await this.click(this.leftArrowBtn);
  }

  // Confirmation Button Methods
  async clickConfirmToDeleteButton() {
    await this.click(this.confirmToDeleteBtn);
    await this.page.waitForTimeout(ConstantHelper.wait.short);
  }

  async clickConfirmCreateFolderButton() {
    await this.click(this.confirmCreateFolderBtn);
    await this.page.waitForTimeout(ConstantHelper.wait.short);
  }

  async clickConfirmRemoveButton() {
    await this.click(this.confirmToRemoveBtn);
  }

  async clickConfirmToSubmitButton() {
    await this.click(this.confirmToSubmitBtn);
  }

  async clickConfirmDisableButton() {
    await this.click(this.confirmDisableBtn);
  }

  async clickConfirmEnableButton() {
    await this.click(this.confirmEnableBtn);
  }

  async clickConfirmRenameButton() {
    await this.click(this.confirmRenameBtn);
  }

  async clickConfirmTrashButton() {
    await this.click(this.confirmTrashBtn);
    await this.page.waitForTimeout(ConstantHelper.wait.short);
  }

  async clickDeleteAndConfirmButton() {
    await this.clickDeleteActionMenuOption();
    await this.clickConfirmToDeleteButton();
  }

  // Folder Methods
  async clickCreateFolderButton() {
    await this.click(this.createFolderBtn);
    await this.page.waitForTimeout(ConstantHelper.wait.short);
  }

  async enterFolderName(folderName: string) {
    await this.enterText(this.folderNameTxt, folderName, {verify: true});
  }

  async createFolder(folderName: string) {
    await this.clickCreateActionMenuOption();
    await this.clickNewFolderThreeDotsButton();
    await this.enterFolderName(folderName);
    await this.clickConfirmCreateFolderButton();
  }

  async deleteFolder() {
    await this.clickDeleteActionMenuOption();
    await this.clickConfirmToDeleteButton();
  }

  async clickFolderButton() {
    await this.click(this.folderBtn);
  }

  async clickNewFolderThreeDotsButton() {
    await this.click(this.newFolderThreeDotsBtn);
  }

  async clickRenameFolderThreeDotsButton() {
    await this.click(this.renameFolderThreeDotsBtn);
  }

  async clickRenameFolderButton() {
    await this.clickRenameButton();
  }

  async clickUpdateFolderButton() {
    await this.click(this.updateFolderBtn);
  }

  // Three Dots Menu Methods
  async clickCreateThreeDotsButton() {
    await this.click(this.createThreeDotsBtn);
  }

  async clickFilterChooseButton() {
    await this.click(this.filterChooseBtn);
  }

  // Success State Methods
  async isSuccessStateVisibleForSaveButton(isVisible: boolean = true) {
    const regex = new RegExp(`^workspace-action:.*Save$`);
    const saveButtonLocator = this.page.getByTestId(regex);
    const saveBtn = this.workspaceAction.filter({has: saveButtonLocator});
    await this.isVisible(saveBtn.locator(this.successState), isVisible, ConstantHelper.timeout.long);
  }

  async isSuccessButtonWithTextVisible(text: string) {
    return await this.isVisible(this.successState.filter({hasText: text}));
  }

  async isSuccessStateIconVisible() {
    await this.isVisible(this.successStateIcon);
  }

  async isFailedStateButtonVisible() {
    await this.isVisible(this.failedStateButton);
  }

  // Notification Methods
  async isSuccessNotificationVisible(isVisible: boolean = true) {
    return await this.isVisible(this.successNotification.first(), isVisible, ConstantHelper.timeout.long);
  }

  async doesSuccessNotificationsHaveCount(count: number) {
    await this.hasCount(this.successNotification, count);
  }

  async doesSuccessNotificationHaveText(text: string, isVisible: boolean = true, deleteNotification = false, timeout = 5000) {
    const response = await this.isVisible(this.successNotification.filter({hasText: text}), isVisible, timeout);
    if (deleteNotification) {
      await this.click(this.successNotification.filter({hasText: text}).getByLabel('close'), {force: true});
    }
    return response;
  }

  async isErrorNotificationVisible(isVisible: boolean = true) {
    return await this.isVisible(this.errorNotification.first(), isVisible);
  }

  async doesErrorNotificationHaveText(text: string, isVisible: boolean = true, deleteNotification: boolean = false) {
    const response = await this.isVisible(this.errorNotification.filter({hasText: text}), isVisible);
    if (deleteNotification) {
      await this.click(this.errorNotification.filter({hasText: text}).locator('svg'));
    }
    return response;
  }

  // Modal Methods
  async clickChooseModalButton() {
    await this.click(this.chooseModalBtn);
  }

  async clickCreateModalButton() {
    await this.click(this.createModalBtn);
  }

  async clickModalMenuItemWithName(name: string) {
    await this.click(this.openedModal.locator(`uui-menu-item[label="${name}"]`), {timeout: ConstantHelper.timeout.long});
  }

  async isModalMenuItemWithNameDisabled(name: string) {
    await this.hasAttribute(this.sidebarModal.locator(`uui-menu-item[label="${name}"]`), 'disabled', '');
  }

  async isModalMenuItemWithNameVisible(name: string, isVisible: boolean = true) {
    await this.isVisible(this.sidebarModal.locator(`uui-menu-item[label="${name}"]`), isVisible);
  }

  // Container Methods
  async clickContainerSaveAndPublishButton() {
    await this.click(this.containerSaveAndPublishBtn);
    await this.page.waitForTimeout(ConstantHelper.wait.short);
  }

  // Navigation Methods
  async goToSection(sectionName: string, checkSections = true, skipReload = false) {
    if (checkSections) {
      for (let section in ConstantHelper.sections) {
        await this.isVisible(this.sectionLinks.getByRole('tab', {name: ConstantHelper.sections[section]}), true, ConstantHelper.timeout.navigation);
      }
    }
    const alreadySelected = await this.sectionLinks.locator('[active]').getByText(sectionName).isVisible();
    if (alreadySelected && !skipReload) {
      await this.page.reload();
    } else {
      await this.click(this.backOfficeHeader.getByRole('tab', {name: sectionName}));
    }
  }

  async goToSettingsTreeItem(settingsTreeItemName: string) {
    await this.goToSection(ConstantHelper.sections.settings);
    await this.click(this.page.getByLabel(settingsTreeItemName, {exact: true}));
  }

  async isSectionWithNameVisible(sectionName: string, isVisible: boolean = true) {
    await this.isVisible(this.page.getByRole('tab', {name: sectionName}), isVisible);
  }

  async isBackOfficeMainVisible(isVisible: boolean = true) {
    await this.page.waitForTimeout(ConstantHelper.timeout.medium);
    await this.isVisible(this.backOfficeMain, isVisible);
  }

  // Link & Button Click by Name Methods
  async clickExactLinkWithName(name: string, toForce: boolean = false) {
    const exactLinkWithNameLocator = this.page.getByRole('link', {name: name, exact: true});
    await this.click(exactLinkWithNameLocator, {force: toForce});
  }

  async clickLinkWithName(name: string, isExact: boolean = false) {
    await this.click(this.page.getByRole('link', {name: name, exact: isExact}));
  }

  async clickLabelWithName(name: string, isExact: boolean = true, toForce: boolean = false) {
    await this.click(this.page.getByLabel(name, {exact: isExact}), {force: toForce});
  }

  async clickButtonWithName(name: string, isExact: boolean = false) {
    const exactButtonWithNameLocator = this.page.getByRole('button', {name: name, exact: isExact});
    await this.click(exactButtonWithNameLocator, {force: true});
  }

  async clickTextButtonWithName(name: string) {
    await this.click(this.page.getByText(name, {exact: true}));
  }

  async isButtonWithNameVisible(name: string) {
    await this.isVisible(this.page.getByRole('button', {name: name}));
  }

  async getButtonWithName(name: string) {
    await this.waitForVisible(this.page.getByRole('button', {name: name}));
    return this.page.getByRole('button', {name: name});
  }

  // Remove Button Methods
  async clickRemoveButtonForName(name: string) {
    const removeButtonWithNameLocator = this.page.locator('[name="' + name + '"] [label="Remove"]');
    await this.click(removeButtonWithNameLocator);
  }

  async clickTrashIconButtonForName(name: string) {
    const trashIconButtonWithNameLocator = this.page.locator('[name="' + name + '"] [name="icon-trash"]');
    await this.click(trashIconButtonWithNameLocator);
  }

  async clickRemoveWithName(name: string) {
    const removeLabelWithNameLocator = this.page.locator('[label="Remove ' + name + '"]');
    await this.click(removeLabelWithNameLocator);
  }

  // Alias & Icon Methods
  async enterAliasName(aliasName: string) {
    await this.page.waitForTimeout(ConstantHelper.wait.short);
    await this.click(this.aliasLockBtn, {force: true});
    await this.enterText(this.aliasNameTxt, aliasName);
  }

  async updateIcon(iconName: string) {
    await this.click(this.iconBtn, {force: true});
    await this.searchForTypeToFilterValue(iconName);
    await this.clickLabelWithName(iconName, true, true);
    await this.clickSubmitButton();
  }

  // Property Editor Methods
  async clickSelectPropertyEditorButton() {
    await this.click(this.selectPropertyEditorBtn);
  }

  async enterAPropertyName(name: string) {
    await this.enterText(this.propertyNameTxt, name, {clearFirst: false});
  }

  async clickEditorSettingsButton(index: number = 0) {
    await this.click(this.editorSettingsBtn.nth(index));
  }

  async addPropertyEditor(propertyEditorName: string, index: number = 0) {
    await this.click(this.addPropertyBtn.nth(index));
    await this.enterAPropertyName(propertyEditorName);
    await this.hasValue(this.propertyNameTxt, propertyEditorName);
    await this.clickSelectPropertyEditorButton();
    await this.searchForTypeToFilterValue(propertyEditorName);
    await this.click(this.page.getByText(propertyEditorName, {exact: true}));
    await this.clickSubmitButton();
  }

  async updatePropertyEditor(propertyEditorName: string) {
    await this.clickEditorSettingsButton();
    await this.clickChangeButton();
    await this.searchForTypeToFilterValue(propertyEditorName);
    await this.click(this.page.getByText(propertyEditorName, {exact: true}));
    await this.enterAPropertyName(propertyEditorName);
    await this.clickSubmitButton();
  }

  async deletePropertyEditor(propertyEditorName: string) {
    await this.page.locator('uui-button').filter({hasText: propertyEditorName}).getByLabel('Editor settings').hover();
    await this.click(this.deleteBtn);
  }

  async deletePropertyEditorWithName(name: string) {
    const propertyEditor = this.page.locator('umb-content-type-design-editor-property', {hasText: name});
    await this.hoverAndClick(propertyEditor, propertyEditor.getByLabel('Delete'), {force: true});
    await this.clickConfirmToDeleteButton();
  }

  async enterPropertyEditorDescription(description: string) {
    await this.enterText(this.enterPropertyEditorDescriptionTxt, description);
  }

  async isPropertyEditorUiWithNameReadOnly(name: string) {
    const propertyEditorUiLocator = this.page.locator('umb-property-editor-ui-' + name);
    await this.hasAttribute(propertyEditorUiLocator, 'readonly', '');
  }

  async isPropertyEditorUiWithNameVisible(name: string, isVisible: boolean = true) {
    const propertyEditorUiLocator = this.page.locator('umb-property-editor-ui-' + name);
    await this.isVisible(propertyEditorUiLocator, isVisible);
  }

  async doesPropertyHaveInvalidBadge(propertyName: string) {
    await this.isVisible(this.page.locator('umb-property-layout').filter({hasText: propertyName}).locator('#invalid-badge uui-badge'));
  }

  // Group Methods
  async clickAddGroupButton() {
    await this.click(this.addGroupBtn);
  }

  async enterGroupName(groupName: string, index: number = 0) {
    const groupNameTxt = this.groupLabel.nth(index);
    await this.enterText(groupNameTxt, groupName);
  }

  async isGroupVisible(groupName: string, isVisible = true) {
    await this.isVisible(this.groupLabel.filter({hasText: groupName}), isVisible);
  }

  async doesGroupHaveValue(value: string) {
    await this.waitForVisible(this.groupLabel);
    return await this.hasValue(this.groupLabel, value);
  }

  async deleteGroup(groupName: string) {
    await this.page.waitForTimeout(ConstantHelper.wait.medium);
    const groups = this.page.locator('umb-content-type-design-editor-group').all();
    for (const group of await groups) {
      if (await group.getByLabel('Group', {exact: true}).inputValue() === groupName) {
        const headerActionsDeleteLocator = group.locator('[slot="header-actions"]').getByLabel('Delete');
        await this.click(headerActionsDeleteLocator, {force: true});
        return;
      }
    }
  }

  async reorderTwoGroups(firstGroupName: string, secondGroupName: string) {
    const firstGroup = this.page.getByTestId('group:' + firstGroupName);
    const secondGroup = this.page.getByTestId('group:' + secondGroupName);
    const firstGroupValue = await firstGroup.getByLabel('Group').inputValue();
    const secondGroupValue = await secondGroup.getByLabel('Group').inputValue();
    const dragToLocator = firstGroup.locator('[slot="header"]').first();
    const dragFromLocator = secondGroup.locator('[slot="header"]').first();
    await this.dragAndDrop(dragFromLocator, dragToLocator, 0, 0, 20);
    return {firstGroupValue, secondGroupValue};
  }

  // Tab Methods
  async clickAddTabButton() {
    await this.click(this.addTabBtn);
  }

  async enterTabName(tabName: string) {
    await this.waitForVisible(this.unnamedTabTxt);
    await this.page.waitForTimeout(ConstantHelper.wait.debounce);
    await this.enterText(this.unnamedTabTxt, tabName);
    await this.click(this.page.getByRole('tab', {name: 'Design'}));
    await this.click(this.page.getByTestId('tab:' + tabName));
  }

  async clickRemoveTabWithName(name: string) {
    const tab = this.page.locator('uui-tab').filter({hasText: name});
    await this.hoverAndClick(tab, tab.locator('[label="Remove"]'));
  }

  async clickStructureTab() {
    await this.page.waitForTimeout(ConstantHelper.wait.medium);
    await this.click(this.structureTabBtn);
  }

  getTabLocatorWithName(name: string) {
    return this.page.getByRole('tab', {name: name});
  }

  // Validation Methods
  async clickMandatoryToggle() {
    await this.click(this.mandatoryToggle);
  }

  async selectValidationOption(option: string) {
    await this.selectByValue(this.validation, option);
  }

  async enterRegEx(regEx: string) {
    await this.enterText(this.regexTxt, regEx, {clearFirst: false});
  }

  async enterRegExMessage(regExMessage: string) {
    await this.enterText(this.regexMessageTxt, regExMessage, {clearFirst: false});
  }

  async isValidationMessageVisible(message: string, isVisible: boolean = true) {
    await this.isVisible(this.validationMessage.filter({hasText: message}), isVisible);
  }

  // Composition & Structure Methods
  async clickCompositionsButton() {
    await this.click(this.compositionsBtn);
  }

  async clickAllowAtRootButton() {
    await this.click(this.allowAtRootBtn);
  }

  async clickAllowedChildNodesButton() {
    await this.click(this.allowedChildNodesModal.locator(this.chooseBtn));
  }

  async clickAddCollectionButton() {
    await this.click(this.addCollectionBtn);
  }

  // Reorder Methods
  async clickIAmDoneReorderingButton() {
    await this.click(this.iAmDoneReorderingBtn);
  }

  async clickReorderButton() {
    await this.click(this.reorderBtn);
  }

  async clickLabelAboveButton() {
    await this.click(this.labelAboveBtn);
  }

  // Query Builder Methods
  async clickQueryBuilderButton() {
    await this.click(this.queryBuilderBtn);
    await this.page.waitForTimeout(ConstantHelper.wait.short);
  }

  async addQueryBuilderWithOrderByStatement(propertyAlias: string, isAscending: boolean) {
    await this.click(this.queryBuilderBtn, {timeout: ConstantHelper.timeout.long});
    await this.click(this.orderByPropertyAliasBtn);
    await this.waitAndSelectQueryBuilderDropDownList(propertyAlias);
    await this.click(this.orderByPropertyAliasBtn);
    if (!isAscending) {
      await this.click(this.ascendingBtn);
    }
  }

  async addQueryBuilderWithWhereStatement(propertyAlias: string, operator: string, constrainValue: string) {
    await this.click(this.queryBuilderBtn, {timeout: ConstantHelper.timeout.long});
    await this.click(this.wherePropertyAliasBtn);
    await this.waitAndSelectQueryBuilderDropDownList(propertyAlias);
    await this.click(this.whereOperatorBtn);
    await this.waitAndSelectQueryBuilderDropDownList(operator);
    await this.enterText(this.whereConstrainValueTxt, constrainValue);
    await this.pressKey(this.whereConstrainValueTxt, 'Enter');
  }

  async waitAndSelectQueryBuilderDropDownList(option: string) {
    const ddlOption = this.page.locator('[open]').locator('uui-combobox-list-option').filter({hasText: option}).first();
    await this.click(ddlOption, {timeout: ConstantHelper.timeout.long});
  }

  async chooseRootContentInQueryBuilder(contentName: string) {
    await this.click(this.chooseRootContentBtn);
    await this.clickModalMenuItemWithName(contentName);
    await this.clickChooseButton();
  }

  async isQueryBuilderCodeShown(code: string) {
    await this.click(this.queryBuilderShowCode);
    await this.containsText(this.queryBuilderShowCode, code, ConstantHelper.timeout.long);
  }

  async doesReturnedItemsHaveCount(itemCount: number) {
    await this.containsText(this.returnedItemsCount, itemCount.toString() + ' published items returned');
  }

  async doesQueryResultHaveContentName(contentName: string) {
    await this.containsText(this.queryResults, contentName);
  }

  // Insert Methods
  async insertDictionaryItem(dictionaryName: string) {
    await this.clickInsertButton();
    await this.click(this.insertDictionaryItemBtn);
    await this.click(this.page.getByLabel(dictionaryName));
    await this.click(this.chooseBtn);
  }

  async insertSystemFieldValue(fieldValue: string) {
    await this.clickInsertButton();
    await this.click(this.insertValueBtn);
    await this.click(this.chooseFieldDropDown);
    await this.click(this.systemFieldsOption);
    await this.click(this.chooseFieldValueDropDown);
    await this.click(this.page.getByText(fieldValue));
    await this.clickSubmitButton();
  }

  async insertPartialView(partialViewName: string) {
    await this.clickInsertButton();
    await this.click(this.insertPartialViewBtn);
    await this.click(this.page.getByLabel(partialViewName));
    await this.clickChooseButton();
  }

  // Rename Methods
  async rename(newName: string) {
    await this.clickRenameActionMenuOption();
    await this.click(this.newNameTxt);
    await this.enterText(this.newNameTxt, newName);
    await this.click(this.renameModalBtn);
    await this.page.waitForTimeout(ConstantHelper.wait.short);
  }

  // Search & Filter Methods
  async searchForTypeToFilterValue(searchValue: string) {
    await this.enterText(this.typeToFilterSearchTxt, searchValue, {clearFirst: false});
  }

  // Description Methods
  async enterDescription(description: string) {
    await this.enterText(this.enterDescriptionTxt, description);
  }

  async doesDescriptionHaveValue(value: string, index: number = 0) {
    return await this.hasValue(this.descriptionBtn.nth(index), value);
  }

  // Drag and Drop Methods
  async dragAndDrop(dragFromSelector: Locator, dragToSelector: Locator, verticalOffset: number = 0, horizontalOffset: number = 0, steps: number = 5) {
    await this.waitForVisible(dragFromSelector);
    await this.waitForVisible(dragToSelector);
    const targetLocation = await dragToSelector.boundingBox();
    const elementCenterX = targetLocation!.x + targetLocation!.width / 2;
    const elementCenterY = targetLocation!.y + targetLocation!.height / 2;
    await this.hover(dragFromSelector);
    await this.page.mouse.move(10, 10);
    await this.hover(dragFromSelector);
    await this.page.mouse.down();
    await this.page.waitForTimeout(ConstantHelper.wait.debounce);
    await this.page.mouse.move(elementCenterX + horizontalOffset, elementCenterY + verticalOffset, {steps: steps});
    await this.page.waitForTimeout(ConstantHelper.wait.debounce);
    await this.page.mouse.up();
  }

  // Create Link Methods
  async clickCreateLink() {
    await this.click(this.createLink);
  }

  // Recycle Bin Methods
  async clickRecycleBinButton() {
    await this.click(this.recycleBinBtn);
  }

  async reloadRecycleBin(containsItems = true) {
    await this.waitForVisible(this.recycleBinMenuItem);
    if (!containsItems) {
      await this.clickReloadChildrenActionMenuOption();
      await this.isVisible(this.recycleBinMenuItemCaretBtn, false);
      return;
    }
    await this.clickActionsMenuForName('Recycle Bin');
    await this.clickReloadChildrenActionMenuOption();
    await this.openCaretButtonForName('Recycle Bin');
  }

  async isItemVisibleInRecycleBin(item: string, isVisible: boolean = true, isReload: boolean = true) {
    if (isReload) {
      await this.reloadRecycleBin(isVisible);
    }
    return await this.isVisible(this.page.locator('[label="Recycle Bin"] [label="' + item + '"]'), isVisible);
  }

  // View Methods
  async changeToGridView() {
    await this.click(this.viewBundleBtn);
    await this.click(this.gridBtn);
  }

  async changeToListView() {
    await this.click(this.viewBundleBtn);
    await this.click(this.listBtn);
  }

  async isViewBundleButtonVisible(isVisible: boolean = true) {
    return this.isVisible(this.viewBundleBtn, isVisible);
  }

  // Media Methods
  async clickMediaWithName(name: string) {
    await this.click(this.mediaCardItems.filter({hasText: name}));
  }

  async selectMediaWithName(mediaName: string, isForce: boolean = false) {
    const mediaLocator = this.mediaCardItems.filter({hasText: mediaName});
    await this.waitForVisible(mediaLocator);
    // Using direct click with position option (not supported by BasePage.click)
    await mediaLocator.click({position: {x: 0.5, y: 0.5}, force: isForce});
  }

  async selectMediaWithTestId(mediaKey: string) {
    const locator = this.page.getByTestId('media:' + mediaKey);
    await this.waitForVisible(locator);
    // Using direct click with position option (not supported by BasePage.click)
    await locator.click({position: {x: 0.5, y: 0.5}});
  }

  async clickMediaPickerModalSubmitButton() {
    await this.click(this.mediaPickerModalSubmitBtn);
  }

  async clickMediaCaptionAltTextModalSubmitButton() {
    await this.click(this.mediaCaptionAltTextModalSubmitBtn);
  }

  async clickChooseMediaStartNodeButton() {
    await this.click(this.chooseMediaInputBtn);
  }

  async isMediaCardItemWithNameDisabled(itemName: string) {
    await this.hasAttribute(this.mediaCardItems.filter({hasText: itemName}), 'class', 'not-allowed');
  }

  async isMediaCardItemWithNameVisible(itemName: string, isVisible: boolean = true) {
    await this.isVisible(this.mediaCardItems.filter({hasText: itemName}), isVisible);
  }

  async doesMediaHaveThumbnail(mediaId: string, thumbnailIconName: string, thumbnailImage: string) {
    const mediaThumbnailLocator = this.page.getByTestId('media:' + mediaId);
    if (thumbnailIconName === 'image') {
      const regexImageSrc = new RegExp(`^${thumbnailImage}.*`);
      await this.hasAttribute(mediaThumbnailLocator.locator('umb-imaging-thumbnail img'), 'src', regexImageSrc.toString());
    } else {
      await this.hasAttribute(mediaThumbnailLocator.locator('umb-imaging-thumbnail umb-icon'), 'name', thumbnailIconName);
    }
  }

  async isInputDropzoneVisible(isVisible: boolean = true) {
    await this.isVisible(this.inputDropzone, isVisible);
  }

  async isImageCropperFieldVisible(isVisible: boolean = true) {
    await this.isVisible(this.imageCropperField, isVisible);
  }

  async isInputUploadFieldVisible(isVisible: boolean = true) {
    await this.isVisible(this.inputUploadField, isVisible);
  }

  // Upload Methods
  async clickToUploadButton() {
    await this.click(this.clickToUploadBtn);
  }

  async uploadFile(filePath: string) {
    const [fileChooser] = await Promise.all([
      this.page.waitForEvent('filechooser'),
      await this.clickToUploadButton(),
    ]);
    await fileChooser.setFiles(filePath);
  }

  // Embedded Media Methods
  async enterEmbeddedURL(value: string) {
    await this.enterText(this.embeddedURLTxt, value);
  }

  async clickEmbeddedRetrieveButton() {
    await this.click(this.embeddedRetrieveBtn);
  }

  async clickEmbeddedMediaModalConfirmButton() {
    await this.click(this.embeddedMediaModalConfirmBtn);
  }

  async waitForEmbeddedPreviewVisible() {
    await this.waitForVisible(this.embeddedPreview);
  }

  // Document Methods
  async clickChooseContentStartNodeButton() {
    await this.click(this.chooseDocumentInputBtn);
  }

  // User Methods
  async clickCurrentUserAvatarButton() {
    await this.click(this.currentUserAvatarBtn, {force: true});
  }

  // Collection Methods
  async clickCreateActionButton() {
    await this.click(this.createActionBtn);
  }

  async clickCreateActionWithOptionName(optionName: string) {
    await this.clickCreateActionButton();
    const createOptionLocator = this.createActionButtonCollection.locator('[label="' + optionName + '"]');
    await this.click(createOptionLocator);
  }

  async doesCollectionTreeItemTableRowHaveName(name: string) {
    await this.waitForVisible(this.collectionTreeItemTableRow.first());
    await this.isVisible(this.collectionTreeItemTableRow.locator('[label="' + name + '"]'));
  }

  async doesCollectionTreeItemTableRowHaveIcon(name: string, icon: string) {
    await this.waitForVisible(this.collectionTreeItemTableRow.first());
    await this.isVisible(this.collectionTreeItemTableRow.filter({hasText: name}).locator('umb-icon').locator('[name="' + icon + '"]'));
  }

  // Reference Methods
  async clickReferenceNodeLinkWithName(name: string) {
    await this.click(this.page.locator('[name="' + name + '"] a#open-part'));
  }

  async doesReferenceHeadlineHaveText(text: string) {
    await this.containsText(this.referenceHeadline, text);
  }

  async isReferenceHeadlineVisible(isVisible: boolean) {
    await this.isVisible(this.referenceHeadline, isVisible);
  }

  async doesReferenceItemsHaveCount(count: number) {
    await this.hasCount(this.entityItemRef, count);
  }

  async isReferenceItemNameVisible(itemName: string) {
    await this.isVisible(this.entityItemRef.locator('uui-ref-node[name="' + itemName + '"]'));
  }

  async doesReferencesContainText(text: string) {
    await this.containsText(this.confirmActionModalEntityReferences, text);
  }

  // Entity Action Methods
  async clickEntityActionWithName(name: string) {
    const regex = new RegExp(`^entity-action:.*${name}$`);
    await this.click(this.openEntityAction.getByTestId(regex).filter({has: this.page.locator(':visible')}));
  }

  async clickCreateActionMenuOption() {
    await this.clickEntityActionWithName('Create');
  }

  async clickTrashActionMenuOption() {
    await this.clickEntityActionWithName('Trash');
  }

  async clickMoveToActionMenuOption() {
    await this.clickEntityActionWithName('MoveTo');
  }

  async clickCreateBlueprintActionMenuOption() {
    await this.clickEntityActionWithName('CreateBlueprint');
  }

  async clickDuplicateToActionMenuOption() {
    await this.clickEntityActionWithName('DuplicateTo');
  }

  async clickPublishActionMenuOption() {
    await this.clickEntityActionWithName('Publish');
  }

  async clickUnpublishActionMenuOption() {
    await this.clickEntityActionWithName('Unpublish');
  }

  async clickRollbackActionMenuOption() {
    await this.clickEntityActionWithName('Rollback');
  }

  async clickCultureAndHostnamesActionMenuOption() {
    await this.clickEntityActionWithName('CultureAndHostnames');
  }

  async clickPublicAccessActionMenuOption() {
    await this.clickEntityActionWithName('PublicAccess');
  }

  async clickSortChildrenActionMenuOption() {
    await this.clickEntityActionWithName('SortChildrenOf');
  }

  async clickNotificationsActionMenuOption() {
    await this.clickEntityActionWithName('Notifications');
  }

  async clickReloadChildrenActionMenuOption() {
    await this.clickEntityActionWithName('ReloadChildrenOf');
  }

  async clickDeleteActionMenuOption() {
    await this.clickEntityActionWithName('Delete');
  }

  async clickRestoreActionMenuOption() {
    await this.clickEntityActionWithName('Restore');
  }

  async clickRenameActionMenuOption() {
    await this.clickEntityActionWithName('Rename');
  }

  async clickCreateOptionsActionMenuOption() {
    await this.clickEntityActionWithName('CreateOptions');
  }

  async clickExportActionMenuOption() {
    await this.clickEntityActionWithName('Export');
  }

  async clickImportActionMenuOption() {
    await this.clickEntityActionWithName('Import');
  }

  async clickUpdateActionMenuOption() {
    await this.clickEntityActionWithName('Update');
  }

  async clickLockActionMenuOption() {
    await this.clickEntityActionWithName('Lock');
  }

  // Entity Item Methods
  async clickEntityItemByName(itemName: string) {
    await this.click(this.page.locator('uui-ref-node,umb-ref-item[name="' + itemName + '"]'));
  }

  // Workspace Action Methods
  async clickWorkspaceActionMenuButton() {
    await this.click(this.workspaceActionMenuBtn);
  }

  // Pagination Methods
  async clickNextPaginationButton() {
    await this.click(this.nextPaginationBtn);
  }

  // Editor Methods
  async enterMonacoEditorValue(value: string) {
    await this.click(this.monacoEditor);
    await this.pressKey(this.monacoEditor, 'Control+A');
    await this.pressKey(this.monacoEditor, 'Backspace');
    await this.page.keyboard.insertText(value);
  }

  // Loader Methods
  async waitUntilUiLoaderIsNoLongerVisible() {
    await this.waitForHidden(this.uiLoader, 10000);
  }

  // Dashboard Methods
  async isDashboardTabWithNameVisible(name: string, isVisible: boolean = true) {
    await this.isVisible(this.page.locator('uui-tab[label="' + name + '"]'), isVisible);
  }

  async isWorkspaceViewTabWithAliasVisible(alias: string, isVisible: boolean = true) {
    await this.isVisible(this.page.getByTestId('workspace:view-link:' + alias), isVisible);
  }

  // Submit Button Methods
  async isSubmitButtonDisabled() {
    await this.isVisible(this.submitBtn);
    await this.isDisabled(this.submitBtn);
  }

  // Data Element Methods
  async clickDataElement(elementName: string, options: any = null) {
    await this.click(this.page.locator(`[data-element="${elementName}"]`), options);
  }

  async getDataElement(elementName: string) {
    return this.page.locator(`[data-element="${elementName}"]`);
  }

  getLocatorWithDataMark(dataMark: string) {
    return this.page.getByTestId(dataMark);
  }

  // Text Visibility Methods
  async isTextWithExactNameVisible(name: string, isVisible = true) {
    return await this.isVisible(this.page.getByText(name, {exact: true}), isVisible);
  }

  async isTextWithMessageVisible(message: string, isVisible: boolean = true) {
    return await this.isVisible(this.page.getByText(message), isVisible);
  }

  // Executes a promise (e.g. button click) and waits for a single API response.
  async waitForResponseAfterExecutingPromise(url: string, promise: Promise<void>, statusCode: number) {
    const [response] = await Promise.all([
      this.page.waitForResponse(resp => resp.url().includes(url) && resp.status() === statusCode),
      promise
    ]);

    if (statusCode === 201) {
      return response.headers()['location']?.split("/").pop();
    }
    return response.url().split('?')[0].split("/").pop();
  }

  // Executes a promise (e.g. button click) and waits for multiple API responses.
  // Use when an action triggers multiple API calls (e.g. moving multiple items).
  // Returns an array of IDs extracted from the responses.
  async waitForMultipleResponsesAfterExecutingPromise(url: string, promise: Promise<void>, statusCode: number, expectedCount: number) {
    const responses: Response[] = [];

    // Create a promise that resolves when we've collected enough responses
    const responsePromise = new Promise<void>((resolve) => {
      this.page.on('response', (resp) => {
        if (resp.url().includes(url) && resp.status() === statusCode) {
          responses.push(resp);
          // Resolve once we have all expected responses
          if (responses.length >= expectedCount) {
            resolve();
          }
        }
      });
    });

    // Execute action and wait for responses simultaneously
    await Promise.all([responsePromise, promise]);

    // Extract IDs from responses
    return responses.map(resp => {
      if (statusCode === 201) {
        return resp.headers()['location']?.split("/").pop();
      }
      return resp.url().split('?')[0].split("/").pop();
    });
  }

  // Executes a promise and waits for multiple 201 Created responses matching URL endings.
  // Returns array of IDs extracted from Location headers, in same order as urlEndings.
  async waitForCreatedResponsesAfterExecutingPromise(
    urlEndings: string[],
    promise: Promise<void>
  ): Promise<(string | undefined)[]> {
    const responsePromises = urlEndings.map(ending =>
      this.page.waitForResponse(resp =>
        resp.url().endsWith(ending) && resp.status() === 201
      )
    );

    const [, ...responses] = await Promise.all([promise, ...responsePromises]);

    return responses.map(resp => resp.headers()['location']?.split("/").pop());
  }

  getTextLocatorWithName(name: string) {
    return this.page.getByText(name, {exact: true});
  }

  async doesPropertyWithNameContainValidationMessage(propertyName: string, validationMessage: string, isContained: boolean = true) {
    const validationMessageLocator = this.page.locator('umb-property-layout[label="' + propertyName + '"]').locator(this.validationMessage);
    if (!isContained) {
      await expect(validationMessageLocator).not.toContainText(validationMessage);
    } else {
      await expect(validationMessageLocator).toContainText(validationMessage);
    }
  }

  async removeNotFoundItem(itemName?: string) {
    const hasText = itemName ? itemName : 'Not found';
    const notFoundItemLocator = this.entityItem.filter({hasText: hasText}); 
    const removeButton = notFoundItemLocator.getByLabel('Remove');
    await this.hoverAndClick(notFoundItemLocator, removeButton);
    await this.clickConfirmRemoveButton();
  }

  async searchByKeywordInCollection(keyword: string) {
    await this.enterText(this.searchTxt, keyword);
    await this.pressKey(this.searchTxt, 'Enter');
    await this.page.waitForTimeout(ConstantHelper.wait.medium);
  }
}