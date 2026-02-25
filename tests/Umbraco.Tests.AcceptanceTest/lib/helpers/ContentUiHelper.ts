import {Page, Locator, expect} from "@playwright/test";
import {UiBaseLocators} from "./UiBaseLocators";
import {ConstantHelper} from "./ConstantHelper";

export class ContentUiHelper extends UiBaseLocators {
  private readonly contentNameTxt: Locator;
  private readonly saveAndPublishBtn: Locator;
  private readonly publishBtn: Locator;
  private readonly unpublishBtn: Locator;
  private readonly actionMenuForContentBtn: Locator;
  private readonly textstringTxt: Locator;
  private readonly infoTab: Locator;
  private readonly linkContent: Locator;
  private readonly historyItems: Locator;
  private readonly generalItem: Locator;
  private readonly documentState: Locator;
  private readonly createdDate: Locator;
  private readonly editDocumentTypeBtn: Locator;
  private readonly addTemplateBtn: Locator;
  private readonly id: Locator;
  private readonly cultureAndHostnamesBtn: Locator;
  private readonly cultureLanguageDropdownBox: Locator;
  private readonly addNewHostnameBtn: Locator;
  private readonly hostnameTxt: Locator;
  private readonly hostnameLanguageDropdownBox: Locator;
  private readonly deleteHostnameBtn: Locator;
  private readonly reloadChildrenThreeDotsBtn: Locator;
  private readonly contentTree: Locator;
  private readonly richTextAreaTxt: Locator;
  private readonly textAreaTxt: Locator;
  private readonly plusIconBtn: Locator;
  private readonly enterTagTxt: Locator;
  private readonly menuItemTree: Locator;
  private readonly hostnameComboBox: Locator;
  private readonly confirmToUnpublishBtn: Locator;
  private readonly saveModalBtn: Locator;
  private readonly dropdown: Locator;
  private readonly setADateTxt: Locator;
  private readonly chooseMediaPickerBtn: Locator;
  private readonly chooseMemberPickerBtn: Locator;
  private readonly numericTxt: Locator;
  private readonly resetFocalPointBtn: Locator;
  private readonly addMultiURLPickerBtn: Locator;
  private readonly linkTxt: Locator;
  private readonly anchorQuerystringTxt: Locator;
  private readonly linkTitleTxt: Locator;
  private readonly tagItems: Locator;
  private readonly removeFilesBtn: Locator;
  private readonly toggleBtn: Locator;
  private readonly toggleInput: Locator;
  private readonly documentTypeWorkspace: Locator;
  private readonly addMultipleTextStringBtn: Locator;
  private readonly multipleTextStringValueTxt: Locator;
  private readonly sliderInput: Locator;
  private readonly tabItems: Locator;
  private readonly documentWorkspace: Locator;
  private readonly selectAVariantBtn: Locator;
  private readonly variantAddModeBtn: Locator;
  private readonly saveAndCloseBtn: Locator;
  private readonly enterNameInContainerTxt: Locator;
  private readonly listView: Locator;
  private readonly nameBtn: Locator;
  private readonly listViewTableRow: Locator;
  private readonly publishSelectedListItems: Locator;
  private readonly unpublishSelectedListItems: Locator;
  private readonly duplicateToSelectedListItems: Locator;
  private readonly moveToSelectedListItems: Locator;
  private readonly trashSelectedListItems: Locator;
  private readonly modalContent: Locator;
  private readonly trashBtn: Locator;
  private readonly documentListView: Locator;
  private readonly documentGridView: Locator;
  private readonly documentTreeItem: Locator;
  private readonly documentLanguageSelect: Locator;
  private readonly documentLanguageSelectPopover: Locator;
  private readonly documentReadOnly: Locator;
  private readonly documentWorkspaceEditor: Locator;
  private readonly documentBlueprintModal: Locator;
  private readonly documentBlueprintModalEnterNameTxt: Locator;
  private readonly documentBlueprintSaveBtn: Locator;
  private readonly documentNotificationsModal: Locator;
  private readonly documentNotificationsSaveBtn: Locator;
  private readonly exactTrashBtn: Locator;
  private readonly emptyRecycleBinBtn: Locator;
  private readonly confirmEmptyRecycleBinBtn: Locator;
  private readonly duplicateToBtn: Locator;
  private readonly moveToBtn: Locator;
  private readonly duplicateBtn: Locator;
  private readonly contentTreeRefreshBtn: Locator;
  private readonly sortChildrenBtn: Locator;
  private readonly rollbackBtn: Locator;
  private readonly rollbackContainerBtn: Locator;
  private readonly publicAccessBtn: Locator;
  private readonly uuiCheckbox: Locator;
  private readonly sortBtn: Locator;
  private readonly containerSaveBtn: Locator
  private readonly groupBasedProtectionBtn: Locator;
  private readonly chooseMemberGroupBtn: Locator;
  private readonly selectLoginPageDocument: Locator;
  private readonly selectErrorPageDocument: Locator;
  private readonly rollbackItem: Locator;
  private readonly actionsMenu: Locator;
  private readonly linkToDocumentBtn: Locator;
  private readonly linkToMediaBtn: Locator;
  private readonly linkToManualBtn: Locator;
  private readonly umbDocumentCollection: Locator;
  private readonly documentTableColumnName: Locator;
  private readonly addBlockElementBtn: Locator;
  private readonly formValidationMessage: Locator;
  private readonly blockName: Locator;
  private readonly addBlockSettingsTabBtn: Locator;
  private readonly editBlockEntryBtn: Locator;
  private readonly copyBlockEntryBtn: Locator;
  private readonly deleteBlockEntryBtn: Locator;
  private readonly blockGridEntry: Locator;
  private readonly blockListEntry: Locator;
  private readonly tipTapPropertyEditor: Locator;
  private readonly tipTapEditor: Locator;
  private readonly uploadedSvgThumbnail: Locator;
  private readonly linkPickerModal: Locator;
  private readonly pasteFromClipboardBtn: Locator;
  private readonly pasteBtn: Locator;
  private readonly closeBtn: Locator;
  private readonly workspaceEditTab: Locator;
  private readonly workspaceEditProperties: Locator;
  private readonly exactCopyBtn: Locator;
  private readonly openActionsMenu: Locator;
  private readonly replaceExactBtn: Locator;
  private readonly clipboardEntryPicker: Locator;
  private readonly blockWorkspaceEditTab: Locator;
  private readonly insertBlockBtn: Locator;
  private readonly blockWorkspace: Locator;
  private readonly saveContentBtn: Locator;
  private readonly splitView: Locator;
  private readonly tiptapInput: Locator;
  private readonly rteBlockInline: Locator;
  private readonly backofficeModalContainer: Locator;
  private readonly modalCreateBtn: Locator;
  private readonly modalUpdateBtn: Locator;
  private readonly rteBlock: Locator;
  private readonly workspaceActionMenu: Locator;
  private readonly workspaceActionMenuItem: Locator;
  private readonly viewMoreOptionsBtn: Locator;
  private readonly schedulePublishBtn: Locator;
  private readonly schedulePublishModalBtn: Locator;
  private readonly documentScheduleModal: Locator;
  private readonly publishAtFormLayout: Locator;
  private readonly unpublishAtFormLayout: Locator;
  private readonly publishAtValidationMessage: Locator;
  private readonly unpublishAtValidationMessage: Locator;
  private readonly lastPublished: Locator;
  private readonly publishAt: Locator;
  private readonly blockGridAreasContainer: Locator;
  private readonly blockGridBlock: Locator;
  private readonly blockGridEntries: Locator;
  private readonly inlineCreateBtn: Locator;
  private readonly removeAt: Locator;
  private readonly selectAllCheckbox: Locator;
  private readonly confirmToPublishBtn: Locator;
  private readonly tiptapStatusbarWordCount: Locator;
  private readonly tiptapStatusbarElementPath: Locator;
  private readonly publishWithDescendantsBtn: Locator;
  private readonly documentPublishWithDescendantsModal: Locator;
  private readonly includeUnpublishedDescendantsToggle: Locator;
  private readonly publishWithDescendantsModalBtn: Locator;
  private readonly documentVariantLanguagePicker: Locator;
  private readonly documentVariantLanguageItem: Locator;
  private readonly styleSelectBtn: Locator;
  private readonly cascadingMenuContainer: Locator;
  private readonly modalFormValidationMessage: Locator;
  private readonly treePickerSearchTxt: Locator;
  private readonly mediaPickerSearchTxt: Locator;
  private readonly memberPickerSearchTxt: Locator;
  private readonly documentCreateOptionsModal: Locator;
  private readonly refListBlock: Locator;
  private readonly propertyActionMenu: Locator;
  private readonly listViewCustomRows: Locator;
  private readonly collectionView: Locator;
  private readonly entityPickerTree: Locator;
  private readonly hostNameItem: Locator;
  private readonly languageToggle: Locator;
  private readonly contentVariantDropdown: Locator;
  private readonly blockProperty: Locator;
  private readonly linkPickerAddBtn: Locator;
  private readonly linkPickerCloseBtn: Locator;
  private readonly linkPickerTargetToggle: Locator;
  private readonly confirmToResetBtn: Locator;
  private readonly saveModal: Locator;
  private readonly expandSegmentBtn: Locator;
  
  constructor(page: Page) {
    super(page);
    this.saveContentBtn = page.getByTestId('workspace-action:Umb.WorkspaceAction.Document.Save');
    this.saveAndPublishBtn = page.getByTestId('workspace-action:Umb.WorkspaceAction.Document.SaveAndPublish');
    this.closeBtn = page.getByRole('button', {name: 'Close', exact: true});
    this.linkPickerModal = page.locator('umb-link-picker-modal');
    this.contentNameTxt = page.locator('#name-input input');
    this.publishBtn = page.getByLabel(/^Publish(…)?$/);
    this.unpublishBtn = page.getByLabel(/^Unpublish(…)?$/);
    this.actionMenuForContentBtn = page.locator('#header').getByTestId('open-dropdown');
    this.textstringTxt = page.locator('umb-property-editor-ui-text-box #input');
    this.reloadChildrenThreeDotsBtn = page.getByRole('button', {name: 'Reload children…'});
    this.contentTree = page.locator('umb-tree[alias="Umb.Tree.Document"]');
    this.richTextAreaTxt = page.frameLocator('iframe[title="Rich Text Area"]').locator('#tinymce');
    this.textAreaTxt = page.locator('umb-property-editor-ui-textarea textarea');
    this.plusIconBtn = page.locator('#icon-add svg');
    this.enterTagTxt = page.getByPlaceholder('Enter tag');
    this.menuItemTree = page.locator('umb-menu-item-tree-default');
    this.confirmToUnpublishBtn = page.locator('umb-document-unpublish-modal').getByLabel('Unpublish');
    this.dropdown = page.locator('select#native');
    this.splitView = page.locator('#splitViews');
    this.setADateTxt = page.getByLabel('Set a date…');
    this.chooseMediaPickerBtn = page.locator('umb-property-editor-ui-media-picker #btn-add');
    this.chooseMemberPickerBtn = page.locator('umb-property-editor-ui-member-picker #btn-add');
    this.numericTxt = page.locator('umb-property-editor-ui-number input');
    this.addMultiURLPickerBtn = page.locator('umb-property-editor-ui-multi-url-picker #btn-add');
    this.linkTxt = page.getByTestId('input:url').locator('#input');
    this.anchorQuerystringTxt = page.getByLabel('#value or ?key=value');
    this.linkTitleTxt = this.linkPickerModal.getByLabel('Title');
    this.tagItems = page.locator('uui-tag');
    this.removeFilesBtn = page.locator('umb-input-upload-field [label="Clear file(s)"]');
    this.toggleBtn = page.locator('umb-property-editor-ui-toggle #toggle');
    this.toggleInput = page.locator('umb-property-editor-ui-toggle span');
    this.documentTypeWorkspace = this.sidebarModal.locator('umb-document-type-workspace-editor');
    this.addMultipleTextStringBtn = page.locator('umb-input-multiple-text-string').getByLabel('Add');
    this.multipleTextStringValueTxt = page.locator('umb-input-multiple-text-string').getByLabel('Value');
    this.sliderInput = page.locator('umb-property-editor-ui-slider #input');
    this.tabItems = page.locator('uui-tab');
    this.documentWorkspace = page.locator('umb-document-workspace-editor');
    this.selectAVariantBtn = page.getByRole('button', {name: 'Open version selector'});
    this.variantAddModeBtn = page.locator('.switch-button.add-mode').locator('.variant-name');
    this.saveAndCloseBtn = page.getByLabel('Save and close');
    this.documentTreeItem = page.locator('umb-document-tree-item');
    this.documentLanguageSelect = page.locator('umb-app-language-select');
    this.documentLanguageSelectPopover = page.locator('umb-popover-layout');
    this.documentReadOnly = this.documentWorkspace.locator('#name-input').getByText('Read-only');
    // Info tab
    this.infoTab = page.getByTestId('workspace:view-link:Umb.WorkspaceView.Document.Info');
    this.linkContent = page.locator('umb-document-links-workspace-info-app');
    this.historyItems = page.locator('umb-history-item');
    this.generalItem = page.locator('.general-item');
    this.documentState = this.generalItem.locator('uui-tag');
    this.createdDate = this.generalItem.filter({hasText: 'Created'}).locator('umb-localize-date');
    this.editDocumentTypeBtn = this.generalItem.filter({hasText: 'Document Type'}).locator('#button');
    this.addTemplateBtn = this.generalItem.filter({hasText: 'Template'}).locator('#button');
    this.id = this.generalItem.filter({hasText: 'Id'}).locator('span');
    this.documentCreateOptionsModal = page.locator('umb-document-create-options-modal');
    // Culture and Hostname
    this.cultureAndHostnamesBtn = page.getByLabel(/^Culture and Hostnames(…)?$/);
    this.hostNameItem = page.locator('.hostname-item');
    this.cultureLanguageDropdownBox = this.page.locator('[label="Culture"]').getByLabel('combobox-input');
    this.hostnameTxt = page.getByLabel('Hostname', {exact: true});
    this.hostnameLanguageDropdownBox = this.hostNameItem.locator('[label="Culture"]').getByLabel('combobox-input');
    this.deleteHostnameBtn = this.hostNameItem.locator('[name="icon-trash"] svg');
    this.hostnameComboBox = this.hostNameItem.locator('[label="Culture"]').locator('uui-combobox-list-option');
    this.saveModal = page.locator('umb-document-save-modal');
    this.saveModalBtn = this.saveModal.getByLabel('Save', {exact: true});
    this.resetFocalPointBtn = page.getByLabel('Reset focal point');
    this.addNewHostnameBtn = page.locator('umb-property-layout[label="Hostnames"]').locator('[label="Add new hostname"]');
    // List View
    this.enterNameInContainerTxt = this.container.getByTestId('input:entity-name').locator('#input');
    this.listView = page.locator('umb-document-table-collection-view');
    this.nameBtn = page.getByRole('button', { name: 'Name', exact: true });
    this.listViewTableRow = this.listView.locator('uui-table-row');
    this.publishSelectedListItems = page.locator('umb-entity-bulk-action').getByText('Publish', {exact: true});
    this.unpublishSelectedListItems = page.locator('umb-entity-bulk-action').getByText('Unpublish', {exact: true});
    this.duplicateToSelectedListItems = page.locator('umb-entity-bulk-action').getByText('Duplicate to', {exact: true});
    this.moveToSelectedListItems = page.locator('umb-entity-bulk-action').getByText('Move to', {exact: true});
    this.trashSelectedListItems = page.locator('umb-entity-bulk-action').getByText('Trash', {exact: true});
    this.modalContent = page.locator('umb-tree-picker-modal');
    this.trashBtn = page.getByLabel(/^Trash(…)?$/);
    this.exactTrashBtn = page.getByRole('button', {name: 'Trash', exact: true});
    this.documentListView = page.locator('umb-document-table-collection-view');
    this.documentGridView = page.locator('umb-card-collection-view');
    this.documentWorkspaceEditor = page.locator('umb-workspace-editor');
    this.documentBlueprintModal = page.locator('umb-create-blueprint-modal');
    this.documentBlueprintModalEnterNameTxt = this.documentBlueprintModal.locator('input');
    this.documentBlueprintSaveBtn = this.documentBlueprintModal.getByLabel('Save');
    this.documentNotificationsModal = page.locator('umb-document-notifications-modal');
    this.documentNotificationsSaveBtn = this.documentNotificationsModal.getByLabel('Save', {exact: true});
    this.emptyRecycleBinBtn = page.getByTestId('entity-action:Umb.EntityAction.Document.RecycleBin.Empty').locator('#button');
    this.confirmEmptyRecycleBinBtn = page.locator('#confirm').getByLabel('Empty recycle bin', {exact: true});
    this.duplicateToBtn = page.getByRole('button', {name: 'Duplicate to'});
    this.moveToBtn = page.getByRole('button', {name: 'Move to'});
    this.duplicateBtn = page.getByLabel('Duplicate', {exact: true});
    this.contentTreeRefreshBtn = page.locator('#header').getByLabel('#actions_refreshNode');
    this.sortChildrenBtn = page.getByRole('button', {name: 'Sort children'});
    this.rollbackBtn = page.getByRole('button', { name: 'Rollback…' });
    this.rollbackContainerBtn = this.container.getByLabel('Rollback');
    this.publicAccessBtn = page.getByRole('button', {name: 'Public Access'});
    this.uuiCheckbox = page.locator('uui-checkbox');
    this.sortBtn = page.getByLabel('Sort', {exact: true});
    this.containerSaveBtn = this.container.getByLabel('Save');
    this.groupBasedProtectionBtn = page.locator('span').filter({hasText: 'Group based protection'});
    this.chooseMemberGroupBtn = page.locator('umb-input-member-group').getByLabel('Choose');
    this.selectLoginPageDocument = page.locator('.select-item').filter({hasText: 'Login Page'}).locator('umb-input-document').locator('#button');
    this.selectErrorPageDocument = page.locator('.select-item').filter({hasText: 'Error Page'}).locator('umb-input-document').locator('#button');
    this.rollbackItem = page.locator('.rollback-item');
    this.actionsMenu = page.locator('uui-scroll-container');
    this.linkToDocumentBtn = this.linkPickerModal.getByTestId('action:document').locator('#button');
    this.linkToMediaBtn = this.linkPickerModal.getByTestId('action:media').locator('#button');
    this.linkToManualBtn = this.linkPickerModal.getByTestId('action:external').locator('#button');
    this.umbDocumentCollection = page.locator('umb-document-collection');
    this.documentTableColumnName = this.listView.locator('umb-document-table-column-name');
    //Block Grid - Block List
    this.addBlockElementBtn = page.locator('uui-button-group > uui-button').first().filter({has: page.locator('#button')});
    this.formValidationMessage = page.locator('#splitViews umb-form-validation-message #messages');
    this.blockName = page.locator('#editor [slot="name"]');
    this.addBlockSettingsTabBtn = page.locator('umb-body-layout').getByRole('tab', {name: 'Settings'});
    this.editBlockEntryBtn = page.locator('[label="edit"] svg');
    this.copyBlockEntryBtn = page.getByLabel('Copy to clipboard');
    this.exactCopyBtn = page.getByRole('button', {name: 'Copy', exact: true});
    this.deleteBlockEntryBtn = page.locator('[label="delete"] svg');
    this.blockGridEntry = page.locator('umb-block-grid-entry');
    this.blockGridBlock = page.locator('umb-block-grid-block');
    this.blockListEntry = page.locator('umb-block-list-entry');
    this.pasteFromClipboardBtn = page.getByLabel('Paste from clipboard');
    this.pasteBtn = page.getByRole('button', {name: 'Paste', exact: true});
    this.workspaceEditTab = page.locator('umb-content-workspace-view-edit-tab');
    this.blockWorkspaceEditTab = page.locator('umb-block-workspace-view-edit-tab');
    this.workspaceEditProperties = page.locator('umb-content-workspace-view-edit-properties');
    this.openActionsMenu = page.locator('#action-menu');
    this.replaceExactBtn = page.getByRole('button', {name: 'Replace', exact: true});
    this.clipboardEntryPicker = page.locator('umb-clipboard-entry-picker');
    this.blockGridAreasContainer = page.locator('umb-block-grid-areas-container');
    this.blockGridEntries = page.locator('umb-block-grid-entries');
    this.inlineCreateBtn = page.locator('uui-button-inline-create');
    this.refListBlock = page.locator('umb-ref-list-block');
    // TipTap
    this.tipTapPropertyEditor = page.locator('umb-property-editor-ui-tiptap');
    this.tipTapEditor = this.tipTapPropertyEditor.locator('#editor .tiptap');
    this.uploadedSvgThumbnail = page.locator('umb-input-upload-field-svg img');
    this.insertBlockBtn = page.getByTestId('action:tiptap-toolbar:Umb.Tiptap.Toolbar.BlockPicker');
    this.blockWorkspace = page.locator('umb-block-workspace-editor');
    this.tiptapInput = page.locator('umb-input-tiptap');
    this.rteBlockInline = page.locator('umb-rte-block-inline');
    this.backofficeModalContainer = page.locator('umb-backoffice-modal-container');
    this.modalCreateBtn = this.backofficeModalContainer.getByLabel('Create', {exact: true});
    this.modalUpdateBtn = this.backofficeModalContainer.getByLabel('Update', {exact: true});
    this.rteBlock = page.locator('umb-rte-block');
    this.tiptapStatusbarWordCount = page.locator('umb-tiptap-statusbar-word-count');
    this.tiptapStatusbarElementPath = page.locator('umb-tiptap-statusbar-element-path');
    // Scheduled Publishing
    this.workspaceActionMenu = page.locator('umb-workspace-action-menu');
    this.workspaceActionMenuItem = page.locator('umb-workspace-action-menu-item');
    this.viewMoreOptionsBtn = this.workspaceActionMenu.locator('#popover-trigger');
    this.schedulePublishBtn = this.workspaceActionMenuItem.getByLabel('Schedule publish', {exact: true});
    this.documentScheduleModal = page.locator('umb-document-schedule-modal');
    this.schedulePublishModalBtn = this.documentScheduleModal.getByLabel('Schedule publish', {exact: true});
    this.publishAtFormLayout = this.documentScheduleModal.locator('uui-form-layout-item').first();
    this.unpublishAtFormLayout = this.documentScheduleModal.locator('uui-form-layout-item').last();
    this.publishAtValidationMessage = this.publishAtFormLayout.locator('#messages');
    this.unpublishAtValidationMessage = this.unpublishAtFormLayout.locator('#messages');
    this.lastPublished = this.generalItem.filter({hasText: 'Last published'}).locator('umb-localize-date');
    this.publishAt = this.generalItem.filter({hasText: 'Publish at'}).locator('umb-localize-date');
    this.removeAt = this.generalItem.filter({hasText: 'Remove at'}).locator('umb-localize-date');
    this.selectAllCheckbox = this.documentScheduleModal.locator('[label="Select all"]');
    this.confirmToPublishBtn = page.locator('umb-document-publish-modal').getByLabel('Publish');
    // Publish with descendants 
    this.documentPublishWithDescendantsModal = page.locator('umb-document-publish-with-descendants-modal');
    this.publishWithDescendantsBtn = this.workspaceActionMenuItem.getByLabel('Publish with descendants', {exact: true});
    this.includeUnpublishedDescendantsToggle = this.documentPublishWithDescendantsModal.locator('#includeUnpublishedDescendants');
    this.publishWithDescendantsModalBtn = this.documentPublishWithDescendantsModal.getByLabel('Publish with descendants', {exact: true});
    this.documentVariantLanguagePicker = page.locator('umb-document-variant-language-picker');
    this.documentVariantLanguageItem = this.documentVariantLanguagePicker.locator('uui-menu-item');
    // Tiptap - Style Select
    this.styleSelectBtn = page.locator('uui-button[label="Style Select"]');
    this.cascadingMenuContainer = page.locator('umb-cascading-menu-popover uui-scroll-container');
    this.modalFormValidationMessage = this.sidebarModal.locator('umb-form-validation-message #messages');
    this.treePickerSearchTxt = this.page.locator('umb-tree-picker-modal #input');
    this.mediaPickerSearchTxt = this.page.locator('umb-media-picker-modal #search #input');
    this.memberPickerSearchTxt = this.page.locator('umb-member-picker-modal #input');
    // Property Actions
    this.propertyActionMenu = page.locator('#property-action-popover umb-popover-layout');
    // List view custom
    this.listViewCustomRows = page.locator('table tbody tr');
    // Entity Data Picker
    this.collectionView = page.locator('umb-ref-collection-view');
    this.entityPickerTree = page.locator('umb-tree[alias="Umb.Tree.EntityDataPicker"]');
    this.languageToggle = page.getByTestId('input:entity-name').locator('#toggle');
    this.contentVariantDropdown = page.locator('umb-document-workspace-split-view-variant-selector uui-popover-container #dropdown');
    this.blockProperty = page.locator('umb-block-workspace-view-edit-property');
    // Multi URL Picker
    this.linkPickerAddBtn = this.linkPickerModal.getByRole('button', {name: 'Add', exact: true});
    this.linkPickerCloseBtn = this.linkPickerModal.getByRole('button', {name: 'Close', exact: true});
    this.linkPickerTargetToggle = this.linkPickerModal.locator('[label="Opens the link in a new window or tab"]').locator('#toggle');
    this.confirmToResetBtn = page.locator('#confirm').getByLabel('Reset', {exact: true});
    // Segment
    this.expandSegmentBtn = page.locator('.expand-area uui-button');
  }

  async enterContentName(name: string) {
    await this.enterText(this.contentNameTxt, name, {verify: true});
  }

  async clickSaveAndPublishButton() {
    await this.click(this.saveAndPublishBtn);
    await this.page.waitForTimeout(ConstantHelper.wait.short);
  }

  async isSuccessStateVisibleForSaveAndPublishButton (isVisible: boolean = true){
    const saveAndPublishBtn = this.workspaceAction.filter({has: this.saveAndPublishBtn});
    await this.isVisible(saveAndPublishBtn.locator(this.successState), isVisible, ConstantHelper.timeout.long);
  }

  async clickPublishButton() {
    await this.click(this.publishBtn);
    await this.page.waitForTimeout(ConstantHelper.wait.short);
  }

  async clickUnpublishButton() {
    await this.click(this.unpublishBtn);
  }

  async clickReloadChildrenThreeDotsButton() {
    await this.click(this.reloadChildrenThreeDotsBtn);
  }

  async clickActionsMenuAtRoot() {
    await this.click(this.actionMenuForContentBtn, {force: true});
  }

  async goToContentWithName(contentName: string) {
    const contentWithNameLocator = this.menuItemTree.getByText(contentName, {exact: true});
    await this.click(contentWithNameLocator, {timeout: ConstantHelper.timeout.long});
  }

  async clickActionsMenuForContent(name: string) {
    await this.clickActionsMenuForName(name);
  }

  async openContentCaretButtonForName(name: string) {
    const menuItem = this.menuItemTree.filter({hasText: name}).last()
    const isCaretButtonOpen = await menuItem.getAttribute('show-children');

    if (isCaretButtonOpen === null) {
      await this.clickCaretButtonForContentName(name);
    }
  }

  async clickCaretButtonForContentName(name: string) {
    await this.click(this.menuItemTree.filter({hasText: name}).last().locator('#caret-button').last());
  }

  async waitForModalVisible() {
    await this.openedModal.waitFor({state: 'attached'});
  }

  async waitForModalHidden() {
    await this.openedModal.waitFor({state: 'hidden'});
  }

  async clickSaveButtonForContent() {
    await this.click(this.saveContentBtn);
    await this.page.waitForTimeout(ConstantHelper.wait.short);
  }

  async enterTextstring(text: string) {
    await this.enterText(this.textstringTxt, text);
  }

  async isTextstringPropertyVisible(isVisible: boolean = true) {
    if (isVisible) {
      await expect(this.textstringTxt).toBeVisible();
    } else {
      await expect(this.textstringTxt).not.toBeVisible();
    }
  }

  async doesContentTreeHaveName(contentName: string) {
    await this.containsText(this.contentTree, contentName);
  }

  async enterRichTextArea(value: string) {
    await this.waitForVisible(this.richTextAreaTxt);
    await this.richTextAreaTxt.fill(value);
  }

  async enterTextArea(value: string) {
    await this.page.waitForTimeout(ConstantHelper.wait.minimal);
    await this.enterText(this.textAreaTxt, value);
  }

  async clickConfirmToUnpublishButton() {
    await this.click(this.confirmToUnpublishBtn);
  }

  async clickCreateDocumentBlueprintButton() {
    await this.click(this.createDocumentBlueprintBtn);
  }

  // Info Tab
  async clickInfoTab() {
    await this.click(this.infoTab);
  }

  async doesDocumentHaveLink(link: string) {
    await this.containsText(this.linkContent, link);
  }

  async doesHistoryHaveText(text: string) {
    await this.hasText(this.historyItems, text);
  }

  async doesDocumentStateHaveText(text: string) {
    await this.hasText(this.documentState, text);
  }

  async doesCreatedDateHaveText(text: string) {
    await this.hasText(this.createdDate, text);
  }

  async doesIdHaveText(text: string) {
    await this.hasText(this.id, text);
  }

  async clickEditDocumentTypeButton() {
    await this.click(this.editDocumentTypeBtn);
  }

  async clickAddTemplateButton() {
    await this.click(this.addTemplateBtn);
  }

  async clickSaveButtonAndWaitForContentToBeCreated(){
    return await this.waitForResponseAfterExecutingPromise(ConstantHelper.apiEndpoints.document, this.clickSaveButtonForContent(), ConstantHelper.statusCodes.created);
  }

  async clickSaveModalButtonAndWaitForContentToBeCreated(){
    return await this.waitForResponseAfterExecutingPromise(ConstantHelper.apiEndpoints.document, this.clickSaveModalButton(), ConstantHelper.statusCodes.created);
  }

  async clickSaveModalButtonAndWaitForContentToBeUpdated(){
    return await this.waitForResponseAfterExecutingPromise(ConstantHelper.apiEndpoints.document, this.clickSaveModalButton(), ConstantHelper.statusCodes.ok);
  }

  async clickSaveAndPublishButtonAndWaitForContentToBeCreated(){
    return await this.waitForResponseAfterExecutingPromise(ConstantHelper.apiEndpoints.document, this.clickSaveAndPublishButton(), ConstantHelper.statusCodes.created);
  }

  async clickConfirmToPublishButtonAndWaitForContentToBeCreated() {
    return await this.waitForResponseAfterExecutingPromise(ConstantHelper.apiEndpoints.document, this.clickConfirmToPublishButton(), ConstantHelper.statusCodes.created);
  }

  async clickSaveButtonAndWaitForContentToBeUpdated() {
    return await this.waitForResponseAfterExecutingPromise(ConstantHelper.apiEndpoints.document, this.clickSaveButtonForContent(), ConstantHelper.statusCodes.ok);
  }

  async clickSaveAndPublishButtonAndWaitForContentToBeUpdated() {
    return await this.waitForResponseAfterExecutingPromise(ConstantHelper.apiEndpoints.document, this.clickSaveAndPublishButton(), ConstantHelper.statusCodes.ok);
  }

  async clickSaveAndPublishButtonAndWaitForContentToBePublished() {
    return await this.waitForResponseAfterExecutingPromise(ConstantHelper.apiEndpoints.document, this.clickSaveAndPublishButton(), ConstantHelper.statusCodes.ok);
  }

  private async clickContainerSaveButton() {
    await this.click(this.containerSaveBtn);
  }

  async clickContainerSaveButtonAndWaitForContentToBeUpdated() {
    return await this.waitForResponseAfterExecutingPromise(ConstantHelper.apiEndpoints.document, this.clickContainerSaveButton(), ConstantHelper.statusCodes.ok);
  }

  async clickContainerSaveAndPublishButtonAndWaitForContentToBePublished() {
    return await this.waitForResponseAfterExecutingPromise(ConstantHelper.apiEndpoints.document, this.clickContainerSaveAndPublishButton(), ConstantHelper.statusCodes.ok);
  }

  async clickDocumentTypeByName(documentTypeName: string) {
    await this.click(this.page.locator(`uui-ref-node-document-type[name="${documentTypeName}"]`));
  }

  async clickTemplateByName(templateName: string) {
    await this.click(this.page.locator(`uui-ref-node[name="${templateName}"]`));
  }

  async isDocumentTypeModalVisible(documentTypeName: string) {
    await this.isVisible(this.documentTypeWorkspace.filter({hasText: documentTypeName}));
  }

  async isTemplateModalVisible(templateName: string) {
    await this.isVisible(this.breadcrumbsTemplateModal.getByText(templateName));
  }

  async clickEditTemplateByName(templateName: string) {
    await this.click(this.page.locator(`uui-ref-node[name="${templateName}"]`).getByLabel('Choose'));
  }

  async changeTemplate(oldTemplate: string, newTemplate: string) {
    await this.clickEditTemplateByName(oldTemplate);
    await this.click(this.sidebarModal.getByLabel(newTemplate));
    await this.clickChooseModalButton();
  }

  async isTemplateNameDisabled(templateName: string) {
    await this.isVisible(this.sidebarModal.getByLabel(templateName));
    await this.isDisabled(this.sidebarModal.getByLabel(templateName));
  }

  // Culture and Hostnames
  async clickCultureAndHostnamesButton() {
    await this.click(this.cultureAndHostnamesBtn);
  }

  async clickAddNewHostnameButton(){
    await this.click(this.addNewHostnameBtn);
  }

  async selectCultureLanguageOption(option: string) {
    await this.click(this.cultureLanguageDropdownBox);
    await this.click(this.page.getByText(option, {exact: true}));
  }

  async selectHostnameLanguageOption(option: string, index: number = 0) {
    await this.click(this.hostnameLanguageDropdownBox.nth(index));
    await this.click(this.hostnameComboBox.getByText(option).nth(index));
  }

  async enterDomain(value: string, index: number = 0) {
    await this.enterText(this.hostnameTxt.nth(index), value, {verify: true});
  }

  async clickDeleteHostnameButton() {
    await this.click(this.deleteHostnameBtn.first());
  }

  async clickSaveModalButton() {
    await this.click(this.saveModalBtn);
    await this.page.waitForTimeout(ConstantHelper.wait.short);
  }

  async chooseDocumentType(documentTypeName: string) {
    await this.click(this.documentTypeNode.filter({hasText: documentTypeName}));
  }

  // Approved Color
  async clickApprovedColorByValue(value: string) {
    await this.click(this.page.locator(`uui-color-swatch[value="#${value}"] #swatch`));
  }

  // Checkbox list
  async chooseCheckboxListOption(optionValue: string) {
    await this.click(this.page.locator(`uui-checkbox[value="${optionValue}"] svg`));
  }

  // Content Picker
  async addContentPicker(contentName: string) {
    await this.clickChooseButton();
    await this.click(this.sidebarModal.getByText(contentName));
    await this.click(this.chooseModalBtn);
  }

  async isOpenButtonVisibleInContentPicker(contentPickerName: string, isVisible: boolean = true) {
    return await this.isVisible(this.page.getByLabel('Open ' + contentPickerName), isVisible);
  }

  async clickContentPickerOpenButton(contentPickerName: string) {
    await this.click(this.page.getByLabel('Open ' + contentPickerName));
  }

  async isNodeOpenForContentPicker(contentPickerName: string) {
    return await this.isVisible(this.openedModal.getByText(contentPickerName));
  }

  async isContentNameVisible(contentName: string, isVisible: boolean = true) {
    return await this.isVisible(this.sidebarModal.getByText(contentName), isVisible);
  }

  async isContentInTreeVisible(name: string, isVisible: boolean = true) {
    await this.isVisible(this.documentTreeItem.getByLabel(name, {exact: true}).first(), isVisible);
  }

  async isChildContentInTreeVisible(parentName: string, childName: string, isVisible: boolean = true) {
    await this.isVisible(this.documentTreeItem.locator('[label="' + parentName + '"]').locator('uui-menu-item[label="' + childName + '"]'), isVisible);
  }

  async removeContentPicker(contentPickerName: string) {
    const contentPickerLocator = this.entityItem.filter({has: this.page.locator(`[name="${contentPickerName}"]`)});
    await this.hoverAndClick(contentPickerLocator, contentPickerLocator.getByLabel('Remove'));
    await this.clickConfirmRemoveButton();
  }

  // Dropdown
  async chooseDropdownOption(optionValues: string[]) {
    await this.selectMultiple(this.dropdown, optionValues);
  }

  // Date Picker
  async enterADate(date: string) {
    await this.setADateTxt.fill(date);
  }

  // Media Picker
  async clickChooseMediaPickerButton() {
    await this.click(this.chooseMediaPickerBtn);
  }

  async clickChooseButtonAndSelectMediaWithName(mediaName: string) {
    await this.clickChooseMediaPickerButton();
    await this.selectMediaWithName(mediaName);
  }

  async clickChooseButtonAndSelectMediaWithKey(mediaKey: string) {
    await this.clickChooseMediaPickerButton();
    await this.selectMediaWithTestId(mediaKey);
  }

  async removeMediaPickerByName(mediaPickerName: string) {
    await this.click(this.page.locator(`[name="${mediaPickerName}"] [label="Remove"] svg`));
    await this.clickConfirmRemoveButton();
  }

  async isMediaNameVisible(mediaName: string, isVisible: boolean = true) {
    return await this.isVisible(this.mediaCardItems.filter({hasText: mediaName}), isVisible);
  }

  async clickResetFocalPointButton() {
    await this.click(this.resetFocalPointBtn);
  }

  async setFocalPoint(widthPercentage: number = 50, heightPercentage: number = 50) {
    await this.page.waitForTimeout(ConstantHelper.wait.medium);
    const element = await this.page.locator('#image').boundingBox();
    if (!element) {
      throw new Error('Element not found');
    }

    const centerX = element.x + element.width / 2;
    const centerY = element.y + element.height / 2;

    const x = element.x + (element.width * widthPercentage) / 100;
    const y = element.y + (element.height * heightPercentage) / 100;

    await this.page.waitForTimeout(ConstantHelper.wait.minimal);
    await this.page.mouse.move(centerX, centerY, {steps: 5});
    await this.page.waitForTimeout(ConstantHelper.wait.minimal);
    await this.page.mouse.down();
    await this.page.waitForTimeout(ConstantHelper.wait.minimal);
    await this.page.mouse.move(x, y);
    await this.page.waitForTimeout(ConstantHelper.wait.minimal);
    await this.page.mouse.up();
  }

  // Member Picker
  async clickChooseMemberPickerButton() {
    await this.click(this.chooseMemberPickerBtn);
  }

  async selectMemberByName(memberName: string) {
    await this.click(this.sidebarModal.getByText(memberName, {exact: true}));
  }

  async removeMemberPickerByName(memberName: string) {
    const mediaPickerLocator = this.entityItem.filter({has: this.page.locator(`[name="${memberName}"]`)});
    await this.hoverAndClick(mediaPickerLocator, mediaPickerLocator.getByLabel('Remove'));
    await this.clickConfirmRemoveButton();
  }

  // Numeric
  async enterNumeric(number: number) {
    await this.enterText(this.numericTxt, number.toString());
  }

  // Radiobox
  async chooseRadioboxOption(optionValue: string) {
    await this.click(this.page.locator(`uui-radio[value="${optionValue}"] #button`));
  }

  // Tags
  async clickPlusIconButton() {
    await this.click(this.plusIconBtn);
  }

  async enterTag(tagName: string) {
    await this.enterText(this.enterTagTxt, tagName);
    await this.pressKey(this.enterTagTxt, 'Enter');
  }

  async removeTagByName(tagName: string) {
    await this.click(this.tagItems.filter({hasText: tagName}).locator('svg'));
  }

  // Multi URL Picker
  async clickAddMultiURLPickerButton() {
    await this.click(this.addMultiURLPickerBtn);
  }

  async selectLinkByName(linkName: string) {
    await this.click(this.sidebarModal.getByText(linkName, {exact: true}));
  }

  async enterLink(value: string, toPress: boolean = false) {
    if (toPress) {
      await this.enterText(this.linkTxt, '');
      await this.pressKey(this.linkTxt, value);
    } else {
      await this.enterText(this.linkTxt, value);
    }
  }

  async enterAnchorOrQuerystring(value: string, toPress: boolean = false) {
    if (toPress) {
      await this.enterText(this.anchorQuerystringTxt, '');
      await this.pressKey(this.anchorQuerystringTxt, value);
    } else {
      await this.enterText(this.anchorQuerystringTxt, value);
    }
  }

  async enterLinkTitle(value: string, toPress: boolean = false) {
    if (toPress) {
      await this.enterText(this.linkTitleTxt, '');
      await this.pressKey(this.linkTitleTxt, value);
    } else {
      await this.enterText(this.linkTitleTxt, value);
    }
  }

  async removeUrlPickerByName(linkName: string) {
    await this.click(this.page.locator(`[name="${linkName}"]`).getByLabel('Remove'));
    await this.clickConfirmRemoveButton();
  }

  async clickEditUrlPickerButtonByName(linkName: string) {
    await this.click(this.page.locator(`[name="${linkName}"]`).getByLabel('Edit'));
  }

  // Upload
  async clickRemoveFilesButton() {
    await this.click(this.removeFilesBtn);
  }

  // True/false
  async clickToggleButton() {
    await this.click(this.toggleBtn, {force: true});
  }

  async doesToggleHaveLabel(label: string) {
    await this.hasText(this.toggleInput, label);
  }

  // Multiple Text String
  async clickAddMultipleTextStringButton() {
    await this.click(this.addMultipleTextStringBtn);
  }

  async enterMultipleTextStringValue(value: string) {
    await this.enterText(this.multipleTextStringValueTxt, value);
  }

  async addMultipleTextStringItem(value: string) {
    await this.clickAddMultipleTextStringButton();
    await this.enterMultipleTextStringValue(value);
  }

  // Code Editor
  async enterCodeEditorValue(value: string) {
    await this.enterMonacoEditorValue(value);
  }

  // Markdown Editor
  async enterMarkdownEditorValue(value: string) {
    await this.enterMonacoEditorValue(value);
  }

  // Slider
  async changeSliderValue(value: string) {
    await this.sliderInput.fill(value);
  }

  async isDocumentTypeNameVisible(contentName: string, isVisible: boolean = true) {
    return await this.isVisible(this.sidebarModal.getByText(contentName), isVisible); 
  }

  async doesModalHaveText(text: string) {
    await this.containsText(this.openedModal, text);
  }

  // Collection tab
  async isTabNameVisible(tabName: string) {
    return await this.isVisible(this.tabItems.filter({hasText: tabName}));
  }

  async clickTabWithName(tabName: string) {
    const tabLocator = this.tabItems.filter({hasText: tabName});
    await this.click(tabLocator);
  }

  async doesDocumentHaveName(name: string) {
    await this.hasValue(this.enterAName, name);
  }

  async doesDocumentTableColumnNameValuesMatch(expectedValues: string[]) {
    await this.waitForVisible(this.documentListView);
    return expectedValues.forEach((text, index) => {
      expect(this.documentTableColumnName.nth(index).getByLabel(text)).toBeVisible();
    });
  }

  async clickSelectVariantButton() {
    await this.click(this.selectAVariantBtn);
  }

  async clickExpandSegmentButton(contentName: string) {
    await this.page.locator('.variant.culture-variant').filter({hasText: contentName}).locator(this.expandSegmentBtn).click();
  }

  async clickSegmentVariantButton(segmentName: string) {
    await this.click(this.page.getByRole('button', {name: segmentName}));
    await this.page.waitForTimeout(ConstantHelper.wait.short);
  }

  async clickVariantAddModeButtonForLanguageName(language: string) {
    await this.click(this.variantAddModeBtn.getByText(language));
    await this.page.waitForTimeout(ConstantHelper.wait.short);
  }

  async clickSaveAndCloseButton() {
    await this.click(this.saveAndCloseBtn);
    await this.page.waitForTimeout(ConstantHelper.wait.short);
  }

  // List View
  async clickCreateContentWithName(name: string) {
    await this.click(this.page.getByLabel(`Create ${name}`));
    await this.waitForTimeout(ConstantHelper.wait.short);
  }

  async enterNameInContainer(name: string) {
    await this.enterText(this.enterNameInContainerTxt, name);
  }

  async goToContentInListViewWithName(contentName: string) {
    await this.click(this.listView.getByLabel(contentName));
  }

  async doesListViewHaveNoItemsInList() {
    await this.isVisible(this.listView.filter({hasText: 'There are no items to show in the list.'}));
  }

  async doesContentListHaveNoItemsInList() {
    await this.isVisible(this.umbDocumentCollection.filter({hasText: 'No items'}));
  }

  async clickNameButtonInListView() {
    await this.click(this.nameBtn);
  }

  async doesFirstItemInListViewHaveName(name: string) {
    await expect(this.listViewTableRow.first()).toContainText(name);
  }

  async doesListViewContainCount(count: number) {
    await this.hasCount(this.listViewTableRow, count);
  }

  async selectContentWithNameInListView(name: string) {
    await this.click(this.listViewTableRow.filter({hasText: name}));
  }

  async clickPublishSelectedListItems() {
    await this.click(this.publishSelectedListItems);
  }

  async clickUnpublishSelectedListItems() {
    await this.click(this.unpublishSelectedListItems);
  }

  async clickDuplicateToSelectedListItems() {
    // Force click is needed
    await this.click(this.duplicateToSelectedListItems, {force: true});
  }

  async clickMoveToSelectedListItems() {
    // Force click is needed
    await this.click(this.moveToSelectedListItems, {force: true});
  }

  async clickTrashSelectedListItems() {
    await this.click(this.trashSelectedListItems);
  }

  async selectDocumentWithNameAtRoot(name: string) {
    await this.openCaretButtonForName('Content');
    await this.click(this.modalContent.getByLabel(name));
    await this.clickChooseButton();
  }

  async clickTrashButton() {
    await this.click(this.trashBtn);
  }

  async clickExactTrashButton() {
    await this.click(this.exactTrashBtn);
  }

  async isDocumentListViewVisible(isVisible: boolean = true) {
    await this.isVisible(this.documentListView, isVisible);
  }

  async isDocumentGridViewVisible(isVisible: boolean = true) {
    await this.isVisible(this.documentGridView, isVisible);
  }

  async changeDocumentSectionLanguage(newLanguageName: string) {
    await this.click(this.documentLanguageSelect);
    // Force click is needed
    await this.click(this.documentLanguageSelectPopover.getByText(newLanguageName), {force: true});
  }

  async doesDocumentSectionHaveLanguageSelected(languageName: string) {
    await this.hasText(this.documentLanguageSelect, languageName);
  }

  async isDocumentReadOnly(isVisible: boolean = true) {
    await this.isVisible(this.documentReadOnly, isVisible);
  }

  async isDocumentNameInputEditable(isEditable: boolean = true) {
    await this.waitForVisible(this.contentNameTxt);
    await expect(this.contentNameTxt).toBeEditable({editable: isEditable});
  }

  async isActionsMenuForRecycleBinVisible(isVisible: boolean = true) {
    await this.isActionsMenuForNameVisible('Recycle Bin', isVisible);
  }

  async isActionsMenuForRootVisible(isVisible: boolean = true) {
    await this.isActionsMenuForNameVisible('Content', isVisible);
  }

  async clickEmptyRecycleBinButton() {
    await this.hover(this.recycleBinMenuItem);
    // Force click is needed
    await this.click(this.emptyRecycleBinBtn, {force: true});
  }

  async clickConfirmEmptyRecycleBinButton() {
    await this.click(this.confirmEmptyRecycleBinBtn);
  }

  async isDocumentPropertyEditable(propertyName: string, isEditable: boolean = true) {
    const propertyLocator = this.documentWorkspace.locator(this.property).filter({hasText: propertyName}).locator('#input');
    await this.waitForVisible(propertyLocator);
    await expect(propertyLocator).toBeEditable({editable: isEditable});
  }

  async doesDocumentPropertyHaveValue(propertyName: string, value: string) {
    const propertyLocator = this.documentWorkspace.locator(this.property).filter({hasText: propertyName}).locator('#input');
    await this.hasValue(propertyLocator, value);
  }

  async clickContentTab() {
    await this.click(this.splitView.getByRole('tab', {name: 'Content'}));
  }

  async isDocumentTreeEmpty() {
    await this.hasCount(this.documentTreeItem, 0);
  }

  async doesDocumentWorkspaceContainName(name: string) {
    await expect(this.documentWorkspaceEditor.locator('#input')).toHaveValue(name);
  }

  async doesDocumentWorkspaceHaveText(text: string) {
    await this.containsText(this.documentWorkspace, text);
  }

  async enterDocumentBlueprintName(name: string) {
    await this.enterText(this.documentBlueprintModalEnterNameTxt, name);
  }
  
  async clickSaveDocumentBlueprintButton() {
    await this.click(this.documentBlueprintSaveBtn);
  }

  async clickDuplicateToButton() {
    await this.click(this.duplicateToBtn);
  }

  async clickDuplicateButton() {
    await this.click(this.duplicateBtn);
  }

  async clickMoveToButton() {
    await this.click(this.moveToBtn);
  }

  async moveToContentWithName(parentNames: string[], moveTo: string) {
    for (const contentName of parentNames) {
      await this.click(this.container.getByLabel(`Expand child items for ${contentName}`));
    }
    await this.click(this.container.getByLabel(moveTo));
    await this.clickChooseContainerButton();
  }

  async isCaretButtonVisibleForContentName(contentName: string, isVisible: boolean = true) {
    await this.isVisible(this.page.locator(`[label="${contentName}"]`).getByLabel('Expand child items for '), isVisible);
  }

  async reloadContentTree() {
    // Force click is needed
    await this.click(this.contentTreeRefreshBtn, {force: true});
  }

  async clickSortChildrenButton() {
    await this.click(this.sortChildrenBtn);
  }

  async clickRollbackButton() {
    await this.click(this.rollbackBtn);
  }

  async clickRollbackContainerButton() {
    await this.click(this.rollbackContainerBtn);
  }

  async clickLatestRollBackItem() {
    await this.click(this.rollbackItem.last());
  }

  async clickPublicAccessButton() {
    await this.click(this.publicAccessBtn);
  }

  async addGroupBasedPublicAccess(memberGroupName: string, documentName: string) {
    await this.click(this.groupBasedProtectionBtn);
    await this.clickNextButton();
    await this.click(this.chooseMemberGroupBtn);
    await this.click(this.page.getByLabel(memberGroupName));
    await this.clickChooseModalButton();
    await this.click(this.selectLoginPageDocument);
    await this.click(this.container.getByLabel(documentName, {exact: true}));
    await this.clickChooseModalButton();
    await this.click(this.selectErrorPageDocument);
    await this.click(this.container.getByLabel(documentName, {exact: true}));
    await this.clickChooseModalButton();
    await this.click(this.containerSaveBtn);
  }

  async sortChildrenDragAndDrop(dragFromSelector: Locator, dragToSelector: Locator, verticalOffset: number = 0, horizontalOffset: number = 0, steps: number = 5) {
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
    // If we do not have this, the drag and drop will not work
    await this.hover(dragToSelector);
    await this.page.mouse.up();
  }

  async clickSortButton() {
    await this.click(this.sortBtn);
  }

  async doesIndexDocumentInTreeContainName(parentName: string, childName: string, index: number) {
    await expect(this.documentTreeItem.locator(`[label="${parentName}"]`).locator('umb-tree-item').nth(index).locator('#label')).toHaveText(childName);
  }

  async selectMemberGroup(memberGroupName: string) {
    await this.click(this.uuiCheckbox.getByLabel(memberGroupName));
  }

  async isPermissionInActionsMenuVisible(permissionName: string, isVisible: boolean = true) {
    await this.isVisible(this.actionsMenu.getByRole('button', {
      name: permissionName,
      exact: true
    }), isVisible);
  }

  async clickDocumentLinkButton() {
    await this.click(this.linkToDocumentBtn);
  }

  async clickMediaLinkButton() {
    await this.click(this.linkToMediaBtn);
  }

  async clickManualLinkButton() {
    await this.click(this.linkToManualBtn);
  }

  // Block Grid - Block List
  async clickAddBlockElementButton() {
    await this.click(this.addBlockElementBtn);
  }

  async clickAddBlockWithNameButton(name: string) {
    await this.click(this.page.getByLabel(`Add ${name}`));
  }

  async clickCreateInModal(headline: string, options?: {waitForClose?: 'target' | 'any'}) {
    const modalLocator = this.page.locator('[headline="' + headline + '"]');
    await this.click(modalLocator.getByLabel('Create'));

    if (options?.waitForClose === 'target') {
      await this.waitForHidden(modalLocator);
    } else if (options?.waitForClose === 'any') {
      await this.waitForHidden(this.openedModal);
    }
  }

  async isAddBlockElementButtonVisible(isVisible: boolean = true) {
    await this.isVisible(this.addBlockElementBtn, isVisible);
  }

  async isAddBlockElementButtonWithLabelVisible(blockName: string, label: string, isVisible: boolean = true) {
    await this.isVisible(this.property.filter({hasText: blockName}).locator(this.addBlockElementBtn).filter({hasText: label}), isVisible);
  }

  async doesFormValidationMessageContainText(text: string) {
    await this.containsText(this.formValidationMessage, text);
  }

  async doesBlockElementHaveName(name: string) {
    await this.containsText(this.blockName, name);
  }

  async clickAddBlockSettingsTabButton() {
    await this.click(this.addBlockSettingsTabBtn);
  }

  async clickEditBlockGridBlockButton() {
    await this.hoverAndClick(this.blockGridEntry, this.editBlockEntryBtn);
  }

  async clickDeleteBlockGridBlockButton() {
    await this.hoverAndClick(this.blockGridEntry, this.deleteBlockEntryBtn);
  }

  async clickEditBlockListBlockButton() {
    await this.hoverAndClick(this.blockListEntry, this.editBlockEntryBtn);
  }

  async clickDeleteBlockListBlockButton() {
    await this.hoverAndClick(this.blockListEntry, this.deleteBlockEntryBtn);
  }

  async clickCopyBlockListBlockButton(groupName: string, propertyName: string, blockName: string, index: number = 0) {
    const blockListBlock = this.workspaceEditTab.filter({hasText: groupName}).locator(this.workspaceEditProperties).filter({hasText: propertyName}).locator(this.blockListEntry).nth(index).filter({hasText: blockName});
    await this.hoverAndClick(blockListBlock, blockListBlock.locator(this.copyBlockEntryBtn), {force: true});
    await this.page.waitForTimeout(ConstantHelper.wait.short);
  }

  async clickCopyBlockGridBlockButton(groupName: string, propertyName: string, blockName: string, index: number = 0) {
    const blockGridBlock = this.workspaceEditTab.filter({hasText: groupName}).locator(this.workspaceEditProperties).filter({hasText: propertyName}).locator(this.blockGridEntry).nth(index).filter({hasText: blockName});
    await this.hoverAndClick(blockGridBlock, blockGridBlock.locator(this.copyBlockEntryBtn), {force: true});
    await this.page.waitForTimeout(ConstantHelper.wait.short);
  }

  async clickPasteFromClipboardButtonForProperty(groupName: string, propertyName: string) {
    await this.page.waitForTimeout(ConstantHelper.wait.short);
    const property = this.workspaceEditTab.filter({hasText: groupName}).locator(this.property).filter({hasText: propertyName});
    await this.click(property.locator(this.pasteFromClipboardBtn), {force: true});
  }

  async clickActionsMenuForProperty(groupName: string, propertyName: string) {
    const property = this.workspaceEditTab.filter({hasText: groupName}).locator(this.workspaceEditProperties).filter({hasText: propertyName});
    await this.hoverAndClick(property, property.locator(this.openActionsMenu), {force: true});
  }

  async clickAddBlockGridElementWithName(elementTypeName: string) {
    await this.click(this.page.getByRole('link', {name: `Add ${elementTypeName}`, exact: true}));
  }

  async clickEditBlockListEntryWithName(blockListElementName: string) {
    await this.click(this.blockListEntry.filter({hasText: blockListElementName}).getByLabel('edit'), {force: true});
  }

  async clickEditBlockGridEntryWithName(blockGridElementName: string) {
    const blockGridElementLocator = this.blockGridEntry.filter({hasText: blockGridElementName});
    const blockGridEditButton = blockGridElementLocator.getByLabel('edit');
    await this.hoverAndClick(blockGridElementLocator, blockGridEditButton, {force: true});
  }

  async goToRTEBlockWithName(groupName: string, propertyName: string, blockName: string, index: number = 0) {
    const rteProperty = this.workspaceEditTab.filter({hasText: groupName}).locator(this.workspaceEditProperties).filter({hasText: propertyName});
    const rteBlockLocator = rteProperty.locator(this.rteBlock).filter({hasText: blockName}).nth(index);
    await this.click(rteBlockLocator);
  }

  async clickSelectBlockElementWithName(elementTypeName: string) {
    await this.click(this.page.getByRole('button', {name: elementTypeName, exact: true}));
  }

  async clickSelectBlockElementInAreaWithName(elementTypeName: string) {
    await this.click(this.container.getByRole('button', {name: elementTypeName, exact: true}));
  }

  async clickBlockElementWithName(elementTypeName: string) {
    await this.click(this.page.getByRole('link', {name: elementTypeName, exact: true}), {force: true});
  }

  async enterPropertyValue(propertyName: string, value: string) {
    const property = this.property.filter({hasText: propertyName});
    await this.enterText(property.locator('input'), value);
  }

  async doesBlockContainBlockInAreaWithName(blockWithAreaName: string, areaName: string, blockInAreaName: string, index: number = 0) {
    const blockWithArea = this.blockGridEntry.locator(this.blockGridBlock.filter({hasText: blockWithAreaName})).nth(index);
    const area = blockWithArea.locator(this.blockGridAreasContainer).locator(`[data-area-alias="${areaName}"]`);
    const blockInArea = area.locator(this.blockGridEntry.filter({hasText: blockInAreaName}));
    await this.waitForVisible(blockInArea);
  }

  async doesBlockContainBlockCountInArea(blockWithAreaName: string, areaName: string, blocksInAreaCount: number, index: number = 0) {
    const blockWithArea = this.blockGridEntry.locator(this.blockGridBlock.filter({hasText: blockWithAreaName})).nth(index);
    const area = blockWithArea.locator(this.blockGridAreasContainer).locator(`[data-area-alias="${areaName}"]`);
    const blocks = area.locator(this.blockGridEntry);
    await this.hasCount(blocks, blocksInAreaCount);
  }

  async doesBlockContainCountOfBlockInArea(blockWithAreaName: string, areaName: string, blockInAreaName: string, count: number, index: number = 0) {
    const blockWithArea = this.blockGridEntry.locator(this.blockGridBlock.filter({hasText: blockWithAreaName})).nth(index);
    const area = blockWithArea.locator(this.blockGridAreasContainer).locator(`[data-area-alias="${areaName}"]`);
    const blockInArea = area.locator(this.blockGridEntry.filter({hasText: blockInAreaName}));
    await this.hasCount(blockInArea, count);
  }

  async getBlockAtRootDataElementKey(blockName: string, index: number = 0) {
    const blockGridEntrySelector = 'umb-block-grid-entry';
    return this.blockGridEntries.locator(`.umb-block-grid__layout-container > ${blockGridEntrySelector}`).filter({hasText: blockName}).nth(index).getAttribute('data-element-key');
  }

  async getBlockAreaKeyFromParentBlockDataElementKey(parentKey: string, index: number = 0) {
    const block = this.page.locator(`[data-element-key="${parentKey}"]`);
    return block.locator(this.blockGridAreasContainer).locator('.umb-block-grid__area-container > umb-block-grid-entries').nth(index).getAttribute('area-key');
  }

  async getBlockDataElementKeyInArea(parentBlockName: string, areaName: string, blockName: string, parentIndex: number = 0, childIndex: number = 0) {
    const parentBlock = this.blockGridEntry.locator(this.blockGridBlock.filter({hasText: parentBlockName})).nth(parentIndex);
    const area = parentBlock.locator(this.blockGridAreasContainer).locator(`[data-area-alias="${areaName}"]`);
    const block = area.locator(this.blockGridEntry.filter({hasText: blockName})).nth(childIndex);
    return block.getAttribute('data-element-key');
  }

  async removeBlockFromArea(parentBlockName: string, areaName: string, blockName: string, parentIndex: number = 0, childIndex: number = 0) {
    const parentBlock = this.blockGridEntry.locator(this.blockGridBlock.filter({hasText: parentBlockName})).nth(parentIndex);
    const area = parentBlock.locator(this.blockGridAreasContainer).locator(`[data-area-alias="${areaName}"]`);
    const block = area.locator(this.blockGridEntry.filter({hasText: blockName})).nth(childIndex);
    await this.hoverAndClick(block, block.getByLabel('delete'), {force: true});
  }

  async doesBlockAreaContainColumnSpan(blockWithAreaName: string, areaName: string, columnSpan: number, index: number = 0) {
    const blockWithArea = this.blockGridEntry.locator(this.blockGridBlock.filter({hasText: blockWithAreaName})).nth(index);
    const area = blockWithArea.locator(this.blockGridAreasContainer).locator(`[data-area-alias="${areaName}"]`);
    await this.hasAttribute(area, 'data-area-col-span', columnSpan.toString());
  }

  async doesBlockAreaContainRowSpan(blockWithAreaName: string, areaName: string, rowSpan: number, index: number = 0) {
    const blockWithArea = this.blockGridEntry.locator(this.blockGridBlock.filter({hasText: blockWithAreaName})).nth(index);
    const area = blockWithArea.locator(this.blockGridAreasContainer).locator(`[data-area-alias="${areaName}"]`);
    await this.hasAttribute(area, 'data-area-row-span', rowSpan.toString());
  }

  async clickInlineAddToAreaButton(parentBlockName: string, areaName: string, parentIndex: number = 0, buttonIndex: number = 1) {
    const parentBlock = this.blockGridEntry.locator(this.blockGridBlock.filter({hasText: parentBlockName})).nth(parentIndex);
    const area = parentBlock.locator(this.blockGridAreasContainer).locator(`[data-area-alias="${areaName}"]`);
    await this.click(area.locator(this.inlineCreateBtn).nth(buttonIndex));
  }

  async addBlockToAreasWithExistingBlock(blockWithAreaName: string, areaName: string, parentIndex: number = 0, addToIndex: number = 0) {
    const blockWithArea = this.blockGridEntry.locator(this.blockGridBlock).filter({hasText: blockWithAreaName}).nth(parentIndex);
    await this.hover(blockWithArea);
    const area = blockWithArea.locator(this.blockGridAreasContainer).locator(`[data-area-alias="${areaName}"]`);
    const addBlockBtn = area.locator(this.inlineCreateBtn).nth(addToIndex);
    await this.hover(addBlockBtn, {force: true});
    await this.click(addBlockBtn, {force: true});
  }

  async doesBlockGridBlockWithAreaContainCreateLabel(blockWithAreaName: string, createLabel: string, index: number = 0) {
    const blockWithArea = this.blockGridEntry.locator(this.blockGridBlock.filter({hasText: blockWithAreaName})).nth(index);
    return await this.isVisible(blockWithArea.locator(this.blockGridAreasContainer).getByLabel(createLabel));
  }

  async doesPropertyContainValue(propertyName: string, value: string) {
    await expect(this.property.filter({hasText: propertyName}).locator('input')).toHaveValue(value);
  }

  async clickCreateButtonForModalWithElementTypeNameAndGroupName(headlineName: string, groupName: string) {
    await this.click(this.blockWorkspace.filter({hasText: `Add ${headlineName}`}).filter({hasText: groupName}).getByLabel('Create'));
  }

  async clickUpdateButtonForModalWithElementTypeNameAndGroupName(headlineName: string, groupName: string) {
    await this.click(this.blockWorkspace.filter({hasText: `Edit ${headlineName}`}).filter({hasText: groupName}).locator(this.updateBtn));
  }

  async clickExactCopyButton() {
    await this.click(this.exactCopyBtn);
  }

  async clickExactReplaceButton() {
    await this.click(this.replaceExactBtn);
  }

  async doesClipboardHaveCopiedBlockWithName(contentName: string, propertyName: string, blockName: string, index: number = 0) {
    await this.isVisible(this.clipboardEntryPicker.getByLabel(`${contentName} - ${propertyName} - ${blockName}`).nth(index));
  }

  async doesClipboardHaveCopiedBlocks(contentName: string, propertyName: string, index: number = 0) {
    await this.isVisible(this.clipboardEntryPicker.getByLabel(`${contentName} - ${propertyName}`).nth(index));
  }

  async doesClipboardContainCopiedBlocksCount(count: number) {
    await this.hasCount(this.clipboardEntryPicker.locator(this.menuItem), count);
  }

  async selectClipboardEntryWithName(contentName: string, propertyName: string, blockName: string, index: number = 0) {
    await this.doesClipboardHaveCopiedBlockWithName(contentName, propertyName, blockName, index);
    await this.click(this.clipboardEntryPicker.getByLabel(`${contentName} - ${propertyName} - ${blockName}`).nth(index));
  }

  async selectClipboardEntriesWithName(contentName: string, propertyName: string, index: number = 0) {
    await this.doesClipboardHaveCopiedBlocks(contentName, propertyName, index);
    await this.click(this.clipboardEntryPicker.getByLabel(`${contentName} - ${propertyName}`).nth(index));
  }

  async goToBlockGridBlockWithName(groupName: string, propertyName: string, blockName: string, index: number = 0) {
    const blockGridBlock = this.workspaceEditTab.filter({hasText: groupName}).locator(this.workspaceEditProperties).filter({hasText: propertyName}).locator(this.blockGridEntry).nth(index).filter({hasText: blockName});
    await this.click(blockGridBlock);
  }

  async goToBlockListBlockWithName(groupName: string, propertyName: string, blockName: string, index: number = 0) {
    const blocklistBlock = this.workspaceEditTab.filter({hasText: groupName}).locator(this.workspaceEditProperties).filter({hasText: propertyName}).locator(this.blockListEntry).nth(index).filter({hasText: blockName});
    await this.click(blocklistBlock);
  }

  async doesBlockEditorBlockWithNameContainValue(groupName: string, propertyName: string, inputType: string = ConstantHelper.inputTypes.general, value) {
    await expect(this.blockWorkspaceEditTab.filter({hasText: groupName}).locator(this.property).filter({hasText: propertyName}).locator(inputType)).toContainText(value);
  }

  async clickCloseButton() {
    await this.click(this.closeBtn);
  }

  async clickPasteButton() {
    await this.click(this.pasteBtn, {force: true});
  }

  async doesBlockListPropertyHaveBlockAmount(groupName: string, propertyName: string, amount: number) {
    await this.hasCount(this.workspaceEditTab.filter({hasText: groupName}).locator(this.workspaceEditProperties).filter({hasText: propertyName}).locator(this.blockListEntry), amount);
  }

  async doesBlockGridPropertyHaveBlockAmount(groupName: string, propertyName: string, amount: number) {
    await this.hasCount(this.workspaceEditTab.filter({hasText: groupName}).locator(this.workspaceEditProperties).filter({hasText: propertyName}).locator(this.blockGridEntry), amount);
  }

  async doesPropertyContainValidationMessage(groupName: string, propertyName: string, message: string) {
    await expect(this.blockWorkspaceEditTab.filter({hasText: groupName}).locator(this.property).filter({hasText: propertyName}).locator(this.validationMessage)).toContainText(message);
  }

  async clickInsertBlockButton() {
    await this.click(this.insertBlockBtn);
  }

  // TipTap
  async enterRTETipTapEditor(value: string) {
    await this.enterText(this.tipTapEditor, value);
  }
  
  async typeRTETipTapEditorValue(value: string, toClearFirst = false) {
    await this.typeText(this.tipTapEditor, value, {clearFirst: toClearFirst});
  }
  
  async clickCreateBlockModalButtonAndWaitForModalToClose() {
    await this.click(this.modalCreateBtn);
    await this.waitForHidden(this.backofficeModalContainer);
  }

  async clickUpdateBlockModalButtonAndWaitForModalToClose() {
    await this.click(this.modalUpdateBtn);
    await this.waitForHidden(this.backofficeModalContainer);
  }
  async enterRTETipTapEditorWithName(name: string , value: string){
    const tipTapEditorLocator = this.page.locator(`[data-mark="property:${name}"]`).locator(this.tipTapEditor);
    await this.enterText(tipTapEditorLocator, value);
  }

  async clickTipTapToolbarIconWithTitle(iconTitle: string) {
    await this.click(this.tipTapPropertyEditor.getByTitle(iconTitle, {exact: true}).locator('svg'));
  }

  async doesUploadedSvgThumbnailHaveSrc(imageSrc: string) {
    await this.hasAttribute(this.uploadedSvgThumbnail, 'src', imageSrc);
  }

  async doesRichTextEditorBlockContainLabel(richTextEditorAlias: string, label: string) {
    await expect(this.page.getByTestId(`property:${richTextEditorAlias}`).locator(this.rteBlock)).toContainText(label);
  }

  async doesBlockEditorModalContainEditorSize(editorSize: string, elementName: string) {
    await this.isVisible(this.backofficeModalContainer.locator(`[size="${editorSize}"]`).locator(`[headline="Add ${elementName}"]`));
  }

  async doesBlockEditorModalContainInline(richTextEditorAlias: string, elementName: string) {
    await this.containsText(this.page.getByTestId(`property:${richTextEditorAlias}`).locator(this.tiptapInput).locator(this.rteBlockInline), elementName);
  }

  async doesBlockHaveBackgroundColor(elementName: string, backgroundColor: string) {
    await this.isVisible(this.page.locator('umb-block-type-card', {hasText: elementName}).locator(`[style="background-color:${backgroundColor};"]`));
  }

  async doesBlockHaveIconColor(elementName: string, backgroundColor: string) {
    await this.isVisible(this.page.locator('umb-block-type-card', {hasText: elementName}).locator(`[color="${backgroundColor}"]`));
  }

  async addDocumentDomain(domainName: string, languageName: string) {
    await this.clickCultureAndHostnamesButton();
    await this.clickAddNewHostnameButton();
    await this.enterDomain(domainName);
    await this.selectHostnameLanguageOption(languageName);
    await this.clickSaveModalButton();
  }

  // Scheduled Publishing
  async clickViewMoreOptionsButton() {
    await this.click(this.viewMoreOptionsBtn);
  }

  async clickSchedulePublishButton() {
    await this.click(this.schedulePublishBtn);
  }

  async clickSchedulePublishModalButton() {
    await this.click(this.schedulePublishModalBtn);
  }

  async enterPublishTime(time: string, index: number = 0) {
    const publishAtTxt = this.documentScheduleModal.locator('.publish-date').nth(index).locator('uui-form-layout-item').first().locator('#input');
    await this.enterText(publishAtTxt, time);
  }

  async enterUnpublishTime(time: string, index: number = 0) {
    const unpublishAtTxt = this.documentScheduleModal.locator('.publish-date').nth(index).locator('uui-form-layout-item').last().locator('#input');
    await this.enterText(unpublishAtTxt, time);
  }

  async doesPublishAtValidationMessageContainText(text: string) {
    await this.containsText(this.publishAtValidationMessage, text);
  }

  async doesUnpublishAtValidationMessageContainText(text: string) {
    await this.containsText(this.unpublishAtValidationMessage, text);
  }

  async doesLastPublishedContainText(text: string) {
    await this.containsText(this.lastPublished, text);
  }

  async doesPublishAtContainText(text: string) {
    await this.containsText(this.publishAt, text);
  }

  async doesRemoveAtContainText(text: string) {
    await this.containsText(this.removeAt, text);
  }

  async clickSelectAllCheckbox() {
    await this.click(this.selectAllCheckbox);
  }

  async doesSchedulePublishModalButtonContainDisabledTag(hasDisabledTag: boolean = false) {
    if (!hasDisabledTag) {
      await expect(this.schedulePublishModalBtn).not.toHaveAttribute('disabled', '');
    } else {
      await this.hasAttribute(this.schedulePublishModalBtn, 'disabled', '');
    }
  }

  async clickInlineBlockCaretButtonForName(blockEditorName: string, index: number = 0) {
    const caretButtonLocator = this.blockListEntry.filter({hasText: blockEditorName}).nth(index).locator('uui-symbol-expand svg');
    await this.click(caretButtonLocator);
  }

  async doesTiptapHaveWordCount(count: number) {
    await this.hasText(this.tiptapStatusbarWordCount, `${count} words`);
  }

  async doesTiptapHaveCharacterCount(count: number) {
    await this.hasText(this.tiptapStatusbarWordCount, `${count} characters`);
  }

  async clickTiptapWordCountButton() {
    await this.click(this.tiptapStatusbarWordCount);
  }

  async doesElementPathHaveText(text: string) {
    await this.hasText(this.tiptapStatusbarElementPath, text);
  }

  async clickConfirmToPublishButton() {
    await this.click(this.confirmToPublishBtn);
    // Extra wait to ensure publish process starts
    await this.waitForTimeout(ConstantHelper.wait.medium);
  }

  async clickPublishWithDescendantsButton() {
    await this.click(this.publishWithDescendantsBtn);
  }

  async clickIncludeUnpublishedDescendantsToggle() {
    await this.click(this.includeUnpublishedDescendantsToggle);
  }

  async clickPublishWithDescendantsModalButton() {
    await this.click(this.publishWithDescendantsModalBtn);
  }

  async doesDocumentVariantLanguageItemHaveCount(count: number) {
    await this.hasCount(this.documentVariantLanguageItem, count);
  }

  async doesDocumentVariantLanguageItemHaveName(name: string) {
    await this.containsText(this.documentVariantLanguagePicker, name);
  }

  async clickSchedulePublishLanguageButton(languageName: string) {
    await this.click(this.page.getByRole('menu').filter({hasText: languageName}));
  }

  async clickBlockCardWithName(name: string, toForce: boolean = false) {
    const blockWithNameLocator = this.page.locator('uui-card-block-type', {hasText: name});
    await this.click(blockWithNameLocator, {force: toForce});
  }

  async clickStyleSelectButton() {
    await this.click(this.styleSelectBtn);
  }

  async clickCascadingMenuItemWithName(name: string) {
    const menuItemLocator = this.cascadingMenuContainer.locator(`uui-menu-item[label="${name}"]`);
    await this.click(menuItemLocator);
  }

  async hoverCascadingMenuItemWithName(name: string) {
    const menuItemLocator = this.cascadingMenuContainer.locator(`uui-menu-item[label="${name}"]`);
    await this.hover(menuItemLocator);
  }

  async selectAllRTETipTapEditorText() {
    await this.click(this.tipTapEditor);
    await this.pressKey(this.tipTapEditor, 'Control+A');
  }

  async clearTipTapEditor() {
    await this.waitForVisible(this.tipTapEditor);
    // We use the middle mouse button click so we don't accidentally open a block in the RTE. This solution avoids that.
    await this.tipTapEditor.click({button: "middle"});
    await this.pressKey(this.tipTapEditor, 'Control+A');
    await this.pressKey(this.tipTapEditor, 'Backspace');
  }

  async clickBlockElementInRTEWithName(elementTypeName: string) {
    const blockElementLocator = this.page.locator('uui-ref-node umb-ufm-render').filter({hasText: elementTypeName});
    await this.click(blockElementLocator, {force: true});
  }

  async doesModalFormValidationMessageContainText(text: string) {
    await this.containsText(this.modalFormValidationMessage, text);
  }

  async enterSearchKeywordInTreePickerModal(keyword: string) {
    await this.enterText(this.treePickerSearchTxt, keyword);
    await this.pressKey(this.treePickerSearchTxt, 'Enter');
  }

  async enterSearchKeywordInMediaPickerModal(keyword: string) {
    await this.enterText(this.mediaPickerSearchTxt, keyword);
    await this.pressKey(this.mediaPickerSearchTxt, 'Enter');
  }

  async enterSearchKeywordInMemberPickerModal(keyword: string) {
    await this.enterText(this.memberPickerSearchTxt, keyword);
    await this.pressKey(this.memberPickerSearchTxt, 'Enter');
  }
  
  async isContentNameReadOnly() {
    await expect(this.contentNameTxt).toHaveAttribute('readonly');
  }

  // Block Custom View
  async isBlockCustomViewVisible(blockCustomViewLocator: string, isVisible: boolean = true) {
    await this.isVisible(this.page.locator(blockCustomViewLocator), isVisible);
  }

  async isSingleBlockElementVisible(isVisible: boolean = true) {
    const count = await this.refListBlock.count();
    if (isVisible) {
      expect(count, `Expected only one element, but found ${count}`).toBe(1);
    } else {
      expect(count, `Expected only one element, but found ${count}`).toBe(0);
    }
    await this.isVisible(this.refListBlock, isVisible);
  }

  async doesBlockCustomViewHaveValue(customBlockViewLocator: string, valueText: string) {
    const locator = this.page.locator(`${customBlockViewLocator} p`);
    await this.waitForVisible(locator);
    await this.hasText(locator, valueText);
  }

  async clickPropertyActionWithName(name: string) {
    const actionLocator = this.propertyActionMenu.locator(`umb-property-action uui-menu-item[label="${name}"]`);
    await this.click(actionLocator);
  }
  
  async isContentWithNameVisibleInList(contentName: string, isVisible: boolean = true) {
    await this.isVisible(this.documentTableColumnName.filter({hasText: contentName}), isVisible);
  }
  
  async selectDocumentBlueprintWithName(blueprintName: string) {
    const blueprintLocator = this.documentCreateOptionsModal.locator('uui-menu-item', {hasText: blueprintName});
    await this.click(blueprintLocator);
  }

  async doesDocumentModalHaveText(text: string) {
    await this.containsText(this.documentCreateOptionsModal, text);
  }

  async doesListViewItemsHaveCount(pageSize: number){
    await this.hasCount(this.listViewCustomRows, pageSize);
  }

  async isListViewItemWithNameVisible(itemName: string, index: number = 0){
    await expect(this.listViewCustomRows.nth(index)).toContainText(itemName);
  }

  async clickPaginationNextButton(){
    await this.click(this.nextPaginationBtn);
  }

  // Entity Data Picker
  async chooseCollectionMenuItemWithName(name: string) {
    await this.clickChooseButton();
    await this.click(this.collectionView.locator('umb-entity-collection-item-ref', {hasText: name}));
    await this.clickChooseContainerButton();
    await this.page.waitForTimeout(500);
  }

  async chooseTreeMenuItemWithName(name: string, parentNames: string[] = []) {
    await this.clickChooseButton();
    for (const itemName of parentNames) {
      await this.click(this.entityPickerTree.locator('umb-tree-item').getByLabel(`Expand child items for ${itemName}`));
    }
    await this.click(this.container.getByLabel(name));
    await this.clickChooseContainerButton();
    await this.page.waitForTimeout(500);
  }
  
  async isChooseButtonVisible(isVisible: boolean = true) {
    await this.isVisible(this.chooseBtn, isVisible);
  }

  async clickDocumentNotificationOptionWithName(name: string) {
    const notificationOptionLocator = this.page.locator(`umb-document-notifications-modal [id$="${name}"]`).locator('#toggle');
    await this.click(notificationOptionLocator);
  }

  async switchLanguage(languageName: string) {
    await this.click(this.languageToggle);
    const languageOptionLocator = this.contentVariantDropdown.locator('.culture-variant').filter({hasText: languageName});
    await this.click(languageOptionLocator);
    await expect(languageOptionLocator).toContainClass('selected');
  }

  async clickAddBlockListElementWithName(blockName: string) {
    const createNewButtonLocator = this.page.getByTestId(`property:${blockName.toLowerCase()}`).getByLabel('Create new');
    await this.click(createNewButtonLocator);
  }

  async isAddBlockListElementWithNameDisabled(blockName: string) {
    const createNewButtonLocator = this.page.getByTestId(`property:${blockName.toLowerCase()}`).locator('uui-button').filter({hasText: 'Create new'});
    await expect(createNewButtonLocator).toHaveAttribute('disabled');
  }

  async isAddBlockListElementWithNameVisible(blockName: string) {
    const createNewButtonLocator = this.page.getByTestId(`property:${blockName.toLowerCase()}`).locator('uui-button').filter({hasText: 'Create new'});
    await this.waitForVisible(createNewButtonLocator);
    await expect(createNewButtonLocator).not.toHaveAttribute('disabled');
  }

  async enterBlockPropertyValue(propertyName: string, value: string) {
    const property = this.blockProperty.filter({hasText: propertyName});
    await this.enterText(property.locator('input'), value);
  }

  async isBlockPropertyEditable(propertyName: string, isEditable: boolean = true) {
    const propertyLocator = this.blockProperty.filter({hasText: propertyName}).locator('#input');
    await this.waitForVisible(propertyLocator);
    await expect(propertyLocator).toBeEditable({editable: isEditable});
  }

  async isInlineBlockPropertyVisible(propertyName: string, isVisible: boolean = true) {
    const propertyLocator = this.blockListEntry.locator(this.blockProperty).filter({hasText: propertyName});
    await this.isVisible(propertyLocator, isVisible);
  }

  async isInlineBlockPropertyVisibleForBlockWithName(blockName: string, propertyName: string, isVisible: boolean = true, index: number = 0) {
    const blockEntryLocator = this.blockListEntry.filter({hasText: blockName}).nth(index);
    const propertyLocator = blockEntryLocator.locator(this.blockProperty).filter({hasText: propertyName});
    await this.isVisible(propertyLocator, isVisible);
  }

  async enterInlineBlockPropertyValue(propertyName: string, value: string, index: number = 0) {
    const propertyLocator = this.blockListEntry.nth(index).locator(this.blockProperty).filter({hasText: propertyName});
    await this.enterText(propertyLocator.locator('input'), value);
  }

  async doesInlineBlockPropertyHaveValue(propertyName: string, value: string, index: number = 0) {
    const propertyLocator = this.blockListEntry.nth(index).locator(this.blockProperty).filter({hasText: propertyName}).locator('input');
    await this.hasValue(propertyLocator, value);
  }

  async clickConfirmTrashButtonAndWaitForContentToBeTrashed() {
    return await this.waitForResponseAfterExecutingPromise(ConstantHelper.apiEndpoints.document, this.clickConfirmTrashButton(), ConstantHelper.statusCodes.ok);
  }

  async clickConfirmEmptyRecycleBinButtonAndWaitForRecycleBinToBeEmptied() {
    return await this.waitForResponseAfterExecutingPromise(ConstantHelper.apiEndpoints.recycleBinDocument, this.clickConfirmEmptyRecycleBinButton(), ConstantHelper.statusCodes.ok);
  }

  async clickConfirmToPublishButtonAndWaitForContentToBePublished() {
    return await this.waitForResponseAfterExecutingPromise(ConstantHelper.apiEndpoints.document, this.clickConfirmToPublishButton(), ConstantHelper.statusCodes.ok);
  }

  async clickSaveModalButtonAndWaitForDomainToBeCreated() {
    return await this.waitForResponseAfterExecutingPromise(ConstantHelper.apiEndpoints.domains, this.click(this.sidebarSaveBtn), ConstantHelper.statusCodes.ok);
  }

  async clickSaveModalButtonAndWaitForDocumentBlueprintToBeCreated() {
    return await this.waitForResponseAfterExecutingPromise(ConstantHelper.apiEndpoints.documentBlueprint, this.documentBlueprintSaveBtn.click(), ConstantHelper.statusCodes.created);
  }

  async clickSaveModalButtonAndWaitForNotificationToBeCreated() {
    return await this.waitForResponseAfterExecutingPromise(ConstantHelper.apiEndpoints.notifications, this.click(this.documentNotificationsSaveBtn), ConstantHelper.statusCodes.ok);
  }

  async isLinkPickerAddButtonEnabled() {
    await expect(this.linkPickerAddBtn).toBeEnabled();
  }

  async isLinkPickerAddButtonDisabled() {
    await expect(this.linkPickerAddBtn).toBeDisabled();
  }

  async clickLinkPickerCloseButton() {
    await this.click(this.linkPickerCloseBtn);
  }

  async clickLinkPickerAddButton() {
    await this.click(this.linkPickerAddBtn);
  }

  async clickLinkPickerTargetToggle() {
    await this.click(this.linkPickerTargetToggle);
  }

  async clickConfirmToResetButton() {
    await this.click(this.confirmToResetBtn);
  }

  async doesTextStringHaveExpectedValue(expectedValue: string) {
    await this.hasValue(this.textstringTxt, expectedValue);
  }

  async doesTextAreaHaveExpectedValue(expectedValue: string) {
    await this.hasValue(this.textAreaTxt, expectedValue);
  }
}