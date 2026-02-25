import {Page, Locator, expect} from "@playwright/test";
import {UiBaseLocators} from "./UiBaseLocators";
import {ConstantHelper} from "./ConstantHelper";

export class DataTypeUiHelper extends UiBaseLocators {
  private readonly moveToBtn: Locator;
  private readonly duplicateToBtn: Locator;
  private readonly newDataTypeBtn: Locator;
  private readonly dataTypeNameTxt: Locator;
  private readonly createDataTypeFolderBtn: Locator;
  private readonly updateDataTypeFolderBtn: Locator;
  private readonly includeLabelsToggle: Locator;
  private readonly addColorBtn: Locator;
  private readonly colorValueTxt: Locator;
  private readonly offsetTimeToggle: Locator;
  private readonly dateFormatTxt: Locator;
  private readonly pageSizeTxt: Locator;
  private readonly ascendingRadioBtn: Locator;
  private readonly descendingRadioBtn: Locator;
  private readonly chooseColumnsDisplayedBtn: Locator;
  private readonly workspaceViewName: Locator;
  private readonly orderByDropDownBox: Locator;
  private readonly showWorkspaceViewFirstToggle: Locator;
  private readonly editInInfiniteEditorToggle: Locator;
  private readonly aliasTxt: Locator;
  private readonly widthTxt: Locator;
  private readonly heightTxt: Locator;
  private readonly minimumTxt: Locator;
  private readonly maximumTxt: Locator;
  private readonly stepSizeTxt: Locator;
  private readonly optionTxt: Locator;
  private readonly addOptionBtn: Locator;
  private readonly maximumAllowedCharsTxt: Locator;
  private readonly numberOfRowsTxt: Locator;
  private readonly minHeightTxt: Locator;
  private readonly maxHeightTxt: Locator;
  private readonly acceptedFileExtensionsTxt: Locator;
  private readonly addAcceptedFileExtensionsBtn: Locator;
  private readonly minimumNumberOfItemsTxt: Locator;
  private readonly maximumNumberOfItemsTxt: Locator;
  private readonly ignoreUserStartNodesToggle: Locator;
  private readonly overlaySizeDropDownBox: Locator;
  private readonly hideAnchorQueryStringInputToggle: Locator;
  private readonly pickMultipleItemsToggle: Locator;
  private readonly enableFocalPointToggle: Locator;
  private readonly amountLowValueTxt: Locator;
  private readonly amountHighValueTxt: Locator;
  private readonly toolbarCheckboxes: Locator;
  private readonly addStylesheetBtn: Locator;
  private readonly dimensionsWidthTxt: Locator;
  private readonly dimensionsHeightTxt: Locator;
  private readonly maxImageSizeTxt: Locator;
  private readonly hideLabelToggle: Locator;
  private readonly defineTagGroupTxt: Locator;
  private readonly showOpenButtonToggle: Locator;
  private readonly enableMultipleChoiceToggle: Locator;
  private readonly addOptionsBtn: Locator;
  private readonly presetValueToggle: Locator;
  private readonly showToggleLabelsToggle: Locator;
  private readonly labelOnTxt: Locator;
  private readonly labelOffTxt: Locator;
  private readonly labelTxt: Locator;
  private readonly chooseAcceptedTypesBtn: Locator;
  private readonly chooseWithPlusBtn: Locator;
  private readonly storageTypeDropDownBox: Locator;
  private readonly allowDecimalsToggle: Locator;
  private readonly chooseLayoutsBtn: Locator;
  private readonly columnsDisplayedItems: Locator;
  private readonly layoutsItems: Locator;
  private readonly inlineRadioBtn: Locator;
  private readonly duplicateBtn: Locator;
  private readonly addWithPlusBtn: Locator;
  private readonly selectAPropertyEditorBtn: Locator;
  private readonly typeToFilterTxt: Locator;
  private readonly chooseStartNodeBtn: Locator;
  private readonly addBlockBtn: Locator;
  private readonly minAmountTxt: Locator;
  private readonly maxAmountTxt: Locator;
  private readonly singleBlockModeBtn: Locator;
  private readonly liveEditingModeBtn: Locator;
  private readonly inlineEditingModeBtn: Locator;
  private readonly propertyEditorWidthTxt: Locator;
  private readonly labelTextTxt: Locator;
  private readonly overlaySizeOption: Locator;
  private readonly chooseContentModelBtn: Locator;
  private readonly chooseSettingsModelBtn: Locator;
  private readonly contentModelNode: Locator;
  private readonly settingsModelNode: Locator;
  private readonly removeExactContentModelNodeBtn: Locator;
  private readonly removeExactSettingsModelNodeBtn: Locator;
  private readonly openBtn: Locator;
  private readonly backgroundColorBtn: Locator;
  private readonly backgroundColorTxt: Locator;
  private readonly chooseCustomStylesheetBtn: Locator;
  private readonly iconColorBtn: Locator;
  private readonly iconColorTxt: Locator;
  private readonly stylesheetRemoveBtn: Locator;
  private readonly hideContentEditorBlockGridBtn: Locator;
  private readonly hideContentEditorBlockListBtn: Locator;
  private readonly customStylesheetLabel: Locator;
  private readonly documentTypeWorkspace: Locator;
  private readonly editorWidthTxt: Locator;
  private readonly createButtonLabelTxt: Locator;
  private readonly gridColumnsTxt: Locator;
  private readonly showResizeOptionsBtn: Locator;
  private readonly columnSpanOptions: Locator;
  private readonly areasTabBtn: Locator;
  private readonly availableRowSpansLowValueTxt: Locator;
  private readonly availableRowSpansHighValueTxt: Locator;
  private readonly areaGridColumnsTxt: Locator;
  private readonly addAreaBtn: Locator;
  private readonly blockAreaConfig: Locator;
  private readonly aliasAliasTxt: Locator;
  private readonly blockGridAreaWorkspaceSubmitBtn: Locator;
  private readonly createLabelTxt: Locator;
  private readonly minAllowedTxt: Locator;
  private readonly maxAllowedTxt: Locator;
  private readonly addSpecifiedAllowanceBtn: Locator;
  private readonly advancedTabBtn: Locator;
  private readonly allowBlockAtRootToggle: Locator;
  private readonly allowInAreasToggle: Locator;
  private readonly chooseThumbnailAlias: Locator;
  private readonly expandChildItemsForMediaBtn: Locator;
  private readonly tiptapToolbarConfiguration: Locator;
  private readonly addGroupToolbarBtn: Locator;
  private readonly addRowToolbarBtn: Locator;
  private readonly tiptapExtensionsConfiguration: Locator;
  private readonly propertyEditor: Locator;
  private readonly selectIconBtn: Locator;
  private readonly newFolderBtn: Locator;
  private readonly dataTypeBtn: Locator;
  private readonly dataTypesMenu: Locator;
  private readonly propertyEditorConfig: Locator;
  private readonly propertyEditorConfigItems: Locator;
  private readonly tiptapStatusbarConfiguration: Locator;
  private readonly dataTypeTreeRoot: Locator;
  private readonly createCropBtn: Locator;
  private readonly editCropBtn: Locator;
  private readonly propertyCrops: Locator;
  private readonly addTimeZoneBtn: Locator;
  private readonly timeZoneDropDown: Locator;
  private readonly dataSourceChooseBtn: Locator;
  private readonly blockThumbnailRemoveBtn: Locator;
  private readonly dynamicRootComponent: Locator;
  private readonly dynamicRootPlaceholderBtn: Locator;
  private readonly dynamicRootOriginPickerModal: Locator;
  private readonly dynamicRootQueryStepPickerModal: Locator;
  private readonly closeDynamicRootOriginPickerModalBtn: Locator;

  constructor(page: Page) {
    super(page);
    this.moveToBtn = this.actionsMenuContainer.getByLabel('Move to');
    this.duplicateToBtn = this.actionsMenuContainer.getByLabel(/^Duplicate to(…)?$/);
    this.newDataTypeBtn = page.getByRole('link', {name: 'Data Type', exact: true});
    this.dataTypeNameTxt = page.locator('umb-data-type-workspace-editor #nameInput #input');
    this.createDataTypeFolderBtn = page.getByLabel('Create folder');
    this.newFolderBtn = page.locator('[name="Folder"]');
    this.updateDataTypeFolderBtn = page.getByLabel('Update folder');
    this.ignoreUserStartNodesToggle = page.getByTestId('property:ignoreUserStartNodes').locator('#toggle');
    this.duplicateBtn = this.sidebarModal.getByLabel('Duplicate', {exact: true});
    this.selectAPropertyEditorBtn = page.getByLabel('Select a property editor');
    this.typeToFilterTxt = page.locator('#filter #input');

    // Approved Color
    this.includeLabelsToggle = page.locator('#toggle');
    this.addColorBtn = page.getByLabel('Add');
    this.colorValueTxt = page.getByPlaceholder('Value').getByRole('textbox');

    // Date Picker
    this.offsetTimeToggle = page.locator('umb-property[label="Offset time"] #toggle');
    this.dateFormatTxt = page.getByTestId('property:format').locator('#input');

    // List View
    this.pageSizeTxt = page.getByTestId('property:pageSize').locator('#input');
    this.ascendingRadioBtn = page.locator('uui-radio[label="Ascending [a-z]"] #button');
    this.descendingRadioBtn = page.locator('uui-radio[label="Descending [z-a]"] #button');
    this.chooseColumnsDisplayedBtn = page.getByTestId('property:includeProperties').getByLabel('Choose');
    this.columnsDisplayedItems = page.getByTestId('property:includeProperties').locator('.layout-item');
    this.workspaceViewName = page.getByTestId('property:tabName').locator('#input');
    this.showWorkspaceViewFirstToggle = page.getByTestId('property:showContentFirst').locator('#toggle');
    this.editInInfiniteEditorToggle = page.locator('umb-property[label="Edit in Infinite Editor"] #toggle');
    this.orderByDropDownBox = page.getByTestId('property:orderBy').locator('select');
    this.chooseLayoutsBtn = page.getByTestId('property:layouts').getByLabel('Choose');
    this.layoutsItems = page.getByTestId('property:layouts').locator('.layout-item');

    // Image Cropper
    this.labelTxt = page.getByLabel('Label', {exact: true});
    this.aliasTxt = page.getByLabel('Alias', {exact: true});
    this.widthTxt = page.getByLabel('Width', {exact: true});
    this.heightTxt = page.getByLabel('Height', {exact: true});
    this.propertyCrops = page.getByTestId('property:crops');
    this.createCropBtn = this.propertyCrops.getByRole('button', {name: 'Create'});
    this.editCropBtn = this.propertyCrops.getByRole('button', {name: 'Edit'});

    // Numeric
    this.minimumTxt = page.getByTestId('property:min').locator('#input');
    this.maximumTxt = page.getByTestId('property:max').locator('#input');
    this.stepSizeTxt = page.getByTestId('property:step').locator('#input');
    this.allowDecimalsToggle = page.locator('umb-property[label="Allow decimals"] #toggle');

    // Radiobox
    this.optionTxt = page.getByTestId('property:items').locator('#input');
    this.addOptionBtn = page.getByTestId('property:items').getByLabel('Add', {exact: true});

    // Textarea - Textstring
    this.maximumAllowedCharsTxt = page.getByTestId('property:maxChars').locator('#input');
    this.numberOfRowsTxt = page.getByTestId('property:rows').locator('#input');
    this.minHeightTxt = page.getByTestId('property:minHeight').locator('#input');
    this.maxHeightTxt = page.getByTestId('property:maxHeight').locator('#input');

    // Upload
    this.acceptedFileExtensionsTxt = page.getByTestId('property:fileExtensions').locator('#input');
    this.addAcceptedFileExtensionsBtn = page.getByTestId('property:fileExtensions').getByLabel('Add', {exact: true});

    // Multi URL Picker
    this.minimumNumberOfItemsTxt = page.getByTestId('property:minNumber').locator('#input');
    this.maximumNumberOfItemsTxt = page.getByTestId('property:maxNumber').locator('#input');
    this.overlaySizeDropDownBox = page.getByTestId('property:overlaySize').locator('select');
    this.hideAnchorQueryStringInputToggle = page.getByTestId('property:hideAnchor').locator('#toggle');

    // Media Picker
    this.pickMultipleItemsToggle = page.getByTestId('property:multiple').locator('#toggle');
    this.enableFocalPointToggle = page.getByTestId('property:enableLocalFocalPoint').locator('#toggle');
    this.amountLowValueTxt = page.getByTestId('property:validationLimit').getByLabel('Low value');
    this.amountHighValueTxt = page.getByTestId('property:validationLimit').getByLabel('High value');
    this.chooseAcceptedTypesBtn = page.getByTestId('property:filter').getByLabel('Choose');
    this.chooseWithPlusBtn = page.locator('#btn-add').filter({hasText: 'Choose'});
    this.chooseStartNodeBtn = page.getByTestId('property:startNodeId').locator('#btn-add');

    // Rich Editor
    this.toolbarCheckboxes = page.getByTestId('property:toolbar').locator('uui-checkbox');
    this.addStylesheetBtn = page.getByTestId('property:stylesheets').getByLabel('Add stylesheet');
    this.dimensionsWidthTxt = page.getByTestId('property:dimensions').getByLabel('Width');
    this.dimensionsHeightTxt = page.getByTestId('property:dimensions').getByLabel('Height');
    this.maxImageSizeTxt = page.getByTestId('property:maxImageSize').locator('#input');
    this.hideLabelToggle = page.getByTestId('property:hideLabel').locator('#toggle');
    this.inlineRadioBtn = page.getByTestId('property:mode').locator('uui-radio[value="Inline"]');
    this.addWithPlusBtn = page.getByTestId('property:blocks').locator('#add-button');

    // Tags
    this.defineTagGroupTxt = page.getByTestId('property:group').locator('#input');
    this.storageTypeDropDownBox = page.locator('#native');

    // Content Picker
    this.showOpenButtonToggle = page.getByTestId('property:showOpenButton').locator('#toggle');

    // Dropdown
    this.enableMultipleChoiceToggle = page.getByTestId('property:multiple').locator('#toggle');
    this.addOptionsBtn = page.getByTestId('property:items').getByLabel('Add', {exact: true});

    // True/false
    this.presetValueToggle = page.getByTestId('property:default').locator('#toggle');
    this.showToggleLabelsToggle = page.getByTestId('property:showLabels').locator('#toggle');
    this.labelOnTxt = page.getByTestId('property:labelOn').locator('#input');
    this.labelOffTxt = page.getByTestId('property:labelOff').locator('#input');

    // Block List Editor and Block Grid Editor
    this.addBlockBtn = page.locator('umb-input-block-type #blocks').getByLabel('open');
    this.minAmountTxt = page.getByLabel('Low value');
    this.maxAmountTxt = page.getByLabel('High value');
    this.singleBlockModeBtn = this.page.locator('umb-property-layout').filter({hasText: 'Single block mode'}).locator('#toggle');
    this.liveEditingModeBtn = this.page.locator('umb-property-layout').filter({hasText: 'Live editing'}).locator('#toggle');
    this.inlineEditingModeBtn = this.page.locator('umb-property-layout').filter({hasText: 'Inline editing'}).locator('#toggle');
    this.propertyEditorWidthTxt = this.page.locator('umb-property-layout').filter({hasText: 'Property editor width'}).locator('#input');
    this.labelTextTxt = this.page.locator('[label="Label"]').locator('#input');
    this.overlaySizeOption = this.page.locator('[label="Overlay editor size"]').locator('#native');
    this.chooseContentModelBtn = this.page.locator('[alias="contentElementTypeKey"]').getByLabel('Choose');
    this.chooseSettingsModelBtn = this.page.locator('[alias="settingsElementTypeKey"]').getByLabel('Choose');
    this.contentModelNode = this.page.locator('[alias="contentElementTypeKey"]').locator('uui-ref-node-document-type');
    this.settingsModelNode = this.page.locator('[alias="settingsElementTypeKey"]').locator('uui-ref-node-document-type')
    this.removeExactContentModelNodeBtn = this.page.locator('[alias="contentElementTypeKey"]').getByLabel('Remove', {exact: true});
    this.removeExactSettingsModelNodeBtn = this.page.locator('[alias="settingsElementTypeKey"]').getByLabel('Remove', {exact: true});
    this.openBtn = this.page.getByLabel('Open', {exact: true});
    this.backgroundColorBtn = this.page.locator('umb-property-layout').filter({hasText: 'Background color'}).getByLabel('Eye dropper');
    this.backgroundColorTxt = this.page.locator('[label="Background color"]').locator('[label="Eye dropper"]').locator('#input');
    this.iconColorBtn = this.page.locator('umb-property-layout').filter({hasText: 'Icon color'}).getByLabel('Eye dropper');
    this.iconColorTxt = this.page.locator('[label="Icon color"]').locator('[label="Eye dropper"]').locator('#input');
    this.stylesheetRemoveBtn = this.page.locator('uui-ref-node').getByLabel('Remove', {exact: true});
    this.hideContentEditorBlockListBtn = this.page.locator('[alias="forceHideContentEditorInOverlay"]').locator('#toggle');
    this.hideContentEditorBlockGridBtn = this.page.locator('[alias="hideContentEditor"]').locator('#toggle');
    this.customStylesheetLabel = this.page.locator('[label="Custom stylesheet"]');
    this.chooseThumbnailAlias = this.page.locator('[alias="thumbnail"]').getByLabel('Choose');
    this.documentTypeWorkspace = this.page.locator('umb-document-type-workspace-editor');
    this.editorWidthTxt = this.page.locator('umb-property-layout').filter({hasText: 'Editor width'}).locator('#input');
    this.createButtonLabelTxt = this.page.locator('umb-property-layout').filter({hasText: 'Create button label'}).locator('#input');
    this.gridColumnsTxt = this.page.locator('umb-property-layout').filter({hasText: 'Grid columns'}).locator('#input');
    this.showResizeOptionsBtn = this.page.getByLabel('Show resize options');
    this.columnSpanOptions = this.page.locator('[alias="columnSpanOptions"]');
    this.areasTabBtn = this.page.getByRole('tab', {name: 'Areas'});
    this.availableRowSpansLowValueTxt = this.page.locator('[label="Available row spans"]').getByLabel('Low value');
    this.availableRowSpansHighValueTxt = this.page.locator('[label="Available row spans"]').getByLabel('High value');
    this.areaGridColumnsTxt = this.page.locator('[alias="areaGridColumns"]').locator('#input');
    this.addAreaBtn = this.page.getByLabel('Add area');
    this.blockAreaConfig = this.page.locator('umb-block-area-config-entry');
    this.aliasAliasTxt = this.page.locator('[alias="alias"]').locator('#input');
    this.blockGridAreaWorkspaceSubmitBtn = this.page.locator('umb-block-grid-area-type-workspace-editor').getByLabel('Submit');
    this.createLabelTxt = this.page.locator('[alias="createLabel"]').locator('#input');
    this.minAllowedTxt = this.page.locator('#container').getByLabel('Low value');
    this.maxAllowedTxt = this.page.locator('#container').getByLabel('High value');
    this.addSpecifiedAllowanceBtn = this.page.locator('[alias="specifiedAllowance"]').getByLabel('Add');
    this.advancedTabBtn = this.page.getByRole('tab', {name: 'Advanced'});
    this.allowBlockAtRootToggle = this.page.getByTestId('property:allowAtRoot').locator('#toggle');
    this.allowInAreasToggle = this.page.getByTestId('property:allowInAreas').locator('#toggle');
    this.expandChildItemsForMediaBtn = this.page.getByLabel('Expand child items for media', {exact: true});
    this.chooseCustomStylesheetBtn = this.page.locator('[label="Custom stylesheet"]').getByLabel('Choose');
    this.blockThumbnailRemoveBtn = this.page.getByTestId('property:thumbnail').getByLabel('Remove', {exact: true});

    // Tiptap
    this.tiptapToolbarConfiguration = this.page.locator('umb-property-editor-ui-tiptap-toolbar-configuration');
    this.addGroupToolbarBtn = this.tiptapToolbarConfiguration.locator('uui-button').filter({hasText: 'Add group'});
    this.addRowToolbarBtn = this.tiptapToolbarConfiguration.locator('uui-button').filter({hasText: 'Add row'});
    this.tiptapExtensionsConfiguration = this.page.locator('umb-property-editor-ui-tiptap-extensions-configuration');
    this.propertyEditor = this.page.locator('umb-ref-property-editor-ui');
    this.selectIconBtn = page.getByLabel('Select icon');
    this.dataTypeBtn = this.createOptionActionListModal.locator('[name="Data Type"]');
    this.dataTypesMenu = page.locator('#menu-item').getByRole('link', {name: 'Data Types'});
    this.tiptapStatusbarConfiguration = this.page.locator('umb-property-editor-ui-tiptap-statusbar-configuration');

    // Settings
    this.propertyEditorConfig = page.locator('umb-property-editor-config');
    this.propertyEditorConfigItems = this.propertyEditorConfig.locator('umb-property');
    this.dataTypeTreeRoot = page.locator('[alias="Umb.TreeItem.DataType"]').locator('uui-menu-item[label="Data Types"]')

    // Date Time with Time Zone Picker
    this.addTimeZoneBtn = page.locator('#add-time-zone [name="icon-add"] svg');
    this.timeZoneDropDown = page.locator('umb-input-time-zone-picker uui-combobox');
    
    // Entity Picker Source
    this.dataSourceChooseBtn = page.locator('[label="Data Source"]').locator(this.chooseBtn);

    // Dynamic Root
    this.dynamicRootComponent = page.locator('umb-input-content-picker-document-root');
    this.dynamicRootPlaceholderBtn = this.dynamicRootComponent.locator('uui-button[look="placeholder"]');
    this.dynamicRootOriginPickerModal = page.locator('umb-dynamic-root-origin-picker-modal');
    this.dynamicRootQueryStepPickerModal = page.locator('umb-dynamic-root-query-step-picker-modal');
    this.closeDynamicRootOriginPickerModalBtn = this.dynamicRootOriginPickerModal.getByLabel('Close');
  }

  async clickActionsMenuForDataType(name: string) {
    await this.clickActionsMenuForName(name);
  }

  async clickActionsMenuAtRoot() {
    await this.clickActionsMenuForDataType('Data Types');
  }

  async clickRootFolderCaretButton() {
    await this.openCaretButtonForName('Data Types');
  }

  async createDataTypeFolder(folderName: string) {
    await this.clickCreateActionMenuOption();
    await this.clickFolderButton();
    await this.enterFolderName(folderName);
    await this.clickConfirmCreateFolderButton();
  }

  async createDataTypeFolderAndWaitForDataTypeToBeCreated(folderName: string) {
    await this.clickCreateActionMenuOption();
    await this.clickFolderButton();
    await this.enterFolderName(folderName);
    return await this.clickConfirmCreateFolderButtonAndWaitForDataTypeToBeCreated();
  }

  async goToDataType(dataTypeName: string) {
    await this.clickRootFolderCaretButton();
    await this.click(this.sectionSidebar.getByLabel(dataTypeName, {exact: true}));
  }

  async clickMoveToButton() {
    await this.click(this.moveToBtn);
  }

  async clickDuplicateToButton() {
    await this.click(this.duplicateToBtn);
  }

  async isDataTypeTreeItemVisible(name: string, isVisible: boolean = true) {
    const hasShowChildren = await this.dataTypeTreeRoot.getAttribute('show-children') !== null;

    if (!hasShowChildren) {
      await this.click(this.dataTypeTreeRoot.locator(this.caretBtn).first());
    }

    await this.isTreeItemVisible(name, isVisible);
  }

  async clickNewDataTypeButton() {
    await this.click(this.newDataTypeBtn);
  }

  async clickNewDataTypeFolderButton() {
    await this.click(this.newFolderBtn);
  }

  async enterDataTypeName(name: string) {
    await this.click(this.dataTypeNameTxt);
    await this.enterText(this.dataTypeNameTxt, name);
  }

  async clickCreateFolderButton() {
    await this.click(this.createDataTypeFolderBtn);
    await this.page.waitForTimeout(ConstantHelper.wait.short);
  }

  async clickUpdateFolderButton() {
    await this.click(this.updateDataTypeFolderBtn);
  }

  async deleteDataType(name: string) {
    await this.clickActionsMenuForDataType(name);
    await this.clickDeleteAndConfirmButton();
  }

  async deleteDataTypeAndWaitForDataTypeToBeDeleted(name: string) {
    await this.clickActionsMenuForDataType(name);
    return await this.clickDeleteAndConfirmButtonAndWaitForDataTypeToBeDeleted();
  }

  async deleteDataTypeFolder(folderName: string) {
    await this.clickActionsMenuForDataType(folderName);
    await this.deleteFolder();
  }

  async deleteDataTypeFolderAndWaitForDataTypeToBeDeleted(folderName: string) {
    await this.clickActionsMenuForDataType(folderName);
    await this.clickDeleteActionMenuOption();
    return await this.clickConfirmToDeleteButtonAndWaitForDataTypeToBeDeleted();
  }

  async moveDataTypeToFolder(folderName: string) {
    await this.clickMoveToActionMenuOption();
    await this.click(this.sidebarModal.getByText(folderName, {exact: true}));
    await this.click(this.chooseModalBtn);
  }

  async duplicateDataTypeToFolder(folderName: string) {
    await this.clickDuplicateToActionMenuOption();
    await this.click(this.sidebarModal.getByText(folderName, {exact: true}));
    await this.click(this.duplicateBtn);
  }

  async addMediaStartNode(mediaName: string) {
    await this.click(this.mediaCardItems.filter({hasText: mediaName}));
    await this.clickChooseModalButton();
  }

  async addContentStartNode(contentName: string) {
    await this.clickTextButtonWithName(contentName);
    await this.click(this.chooseModalBtn);
  }

  async clickSelectAPropertyEditorButton() {
    await this.click(this.selectAPropertyEditorBtn);
  }

  async selectAPropertyEditor(propertyName: string, filterKeyword?: string) {
    await this.typeToFilterTxt.fill(filterKeyword ? filterKeyword : propertyName);
    await this.clickTextButtonWithName(propertyName);
  }

  // Approved Color
  async clickIncludeLabelsToggle() {
    await this.click(this.includeLabelsToggle);
  }

  async removeColorByValue(value: string) {
    await this.click(this.page.locator(`[value="${value}"] uui-button svg`));
    await this.click(this.confirmToDeleteBtn);
  }

  async addColor(value: string) {
    await this.click(this.addColorBtn);
    await this.enterText(this.colorValueTxt, value);
  }

  // Label
  async changeValueType(valueType: string) {
    await this.selectByText(this.page.getByLabel('Select a value type'), valueType);
  }

  // Date Picker
  async clickOffsetTimeToggle() {
    await this.click(this.offsetTimeToggle);
  }

  async enterDateFormatValue(value: string) {
    await this.enterText(this.dateFormatTxt, value);
  }

  // List View
  async enterPageSizeValue(value: string) {
    await this.enterText(this.pageSizeTxt, value);
  }

  async chooseOrderDirection(isAscending: boolean) {
    if (isAscending) {
      await this.click(this.ascendingRadioBtn);
    } else {
      await this.click(this.descendingRadioBtn);
    }
  }

  async addColumnDisplayed(contentType: string, contentName: string, propertyAlias: string) {
    await this.click(this.chooseColumnsDisplayedBtn);
    await this.clickTextButtonWithName(contentType);
    await this.clickTextButtonWithName(contentName);
    await this.clickChooseContainerButton();
    await this.clickTextButtonWithName(propertyAlias);
  }

  async removeColumnDisplayed(propertyAlias: string) {
    await this.click(this.columnsDisplayedItems.filter({has: this.page.getByText(propertyAlias, {exact: true})}).getByText('Remove'));
  }

  async addLayouts(layoutName: string) {
    await this.click(this.chooseLayoutsBtn);
    await this.click(this.page.locator(`[name="${layoutName}"]`));
  }

  async removeLayouts(layoutAlias: string) {
    await this.click(this.layoutsItems.filter({has: this.page.getByText(layoutAlias, {exact: true})}).getByText('Remove'));
  }

  async chooseOrderByValue(value: string) {
    await this.selectByText(this.orderByDropDownBox, value);
  }

  async enterWorkspaceViewName(name: string) {
    await this.enterText(this.workspaceViewName, name);
  }

  async clickShowContentWorkspaceViewFirstToggle() {
    await this.click(this.showWorkspaceViewFirstToggle);
  }

  async clickEditInInfiniteEditorToggle() {
    await this.click(this.editInInfiniteEditorToggle);
  }

  async clickBulkActionPermissionsToggleByValue(value: string) {
    await this.click(this.page.locator(`uui-toggle[label='${value}'] #toggle`));
  }

  async clickSelectIconButton() {
    // Force click is needed
    await this.click(this.selectIconBtn, {force: true});
  }

  async chooseWorkspaceViewIconByValue(value: string) {
    await this.click(this.page.locator(`[label="${value}"] svg`));
    await this.click(this.submitBtn);
  }

  // Image Cropper
  async enterCropValues(label: string, alias: string, width: string, height: string) {
    await this.enterText(this.labelTxt, label);
    await this.enterText(this.aliasTxt, alias);
    await this.enterText(this.widthTxt, width);
    await this.enterText(this.heightTxt, height);
  }

  async clickCreateCropButton() {
    await this.click(this.createCropBtn);
  }

  async clickEditCropButton() {
    await this.click(this.editCropBtn);
  }

  async editCropByAlias(alias: string) {
    await this.click(this.page.locator('.crop').filter({has: this.page.getByText(alias)}).getByText('Edit'));
  }

  async removeCropByAlias(alias: string) {
    await this.click(this.page.locator('.crop').filter({has: this.page.getByText(alias)}).getByText('Remove'));
  }

  // Numeric
  async enterMinimumValue(value: string) {
    await this.enterText(this.minimumTxt, value);
  }

  async enterMaximumValue(value: string) {
    await this.enterText(this.maximumTxt, value);
  }

  async enterStepSizeValue(value: string) {
    await this.enterText(this.stepSizeTxt, value);
  }

  async clickAllowDecimalsToggle() {
    await this.click(this.allowDecimalsToggle);
  }

  // Radiobox
  async removeOptionByName(name: string) {
    await this.click(this.page.locator(`uui-button[label='Remove ${name}'] svg`));
    await this.click(this.confirmToDeleteBtn);
  }

  async enterOptionName(name: string) {
    await this.enterText(this.optionTxt.last(), name);
  }

  async clickAddOptionButton() {
    await this.click(this.addOptionBtn);
  }

  // Textarea - Textstring
  async enterMaximumAllowedCharactersValue(value: string) {
    await this.enterText(this.maximumAllowedCharsTxt, value);
  }

  async enterNumberOfRowsValue(value: string) {
    await this.enterText(this.numberOfRowsTxt, value);
  }

  async enterMaxHeightValue(value: string) {
    await this.enterText(this.maxHeightTxt, value);
  }

  async enterMinHeightValue(value: string) {
    await this.enterText(this.minHeightTxt, value);
  }

  // Upload
  async enterAcceptedFileExtensions(value: string) {
    await this.enterText(this.acceptedFileExtensionsTxt.last(), value);
  }

  async removeAcceptedFileExtensionsByValue(value: string) {
    await this.click(this.page.locator(`uui-button[label='Remove ${value}'] svg`));
    await this.click(this.confirmToDeleteBtn);
  }

  async clickAddAcceptedFileExtensionsButton() {
    await this.click(this.addAcceptedFileExtensionsBtn);
  }

  // Multi URL Picker
  async enterMinimumNumberOfItemsValue(value: string) {
    await this.enterText(this.minimumNumberOfItemsTxt, value);
  }

  async enterMaximumNumberOfItemsValue(value: string) {
    await this.enterText(this.maximumNumberOfItemsTxt, value);
  }

  async clickIgnoreUserStartNodesToggle() {
    await this.click(this.ignoreUserStartNodesToggle);
  }

  async chooseOverlaySizeByValue(value: string) {
    await this.selectByValue(this.overlaySizeDropDownBox, value);
  }

  async clickHideAnchorQueryStringInputToggle() {
    await this.click(this.hideAnchorQueryStringInputToggle);
  }

  // Media Picker
  async clickPickMultipleItemsToggle() {
    await this.click(this.pickMultipleItemsToggle);
  }

  async clickEnableFocalPointToggle() {
    await this.click(this.enableFocalPointToggle);
  }

  async enterAmountValue(lowValue: string, highValue: string) {
    await this.enterText(this.amountLowValueTxt, lowValue);
    await this.enterText(this.amountHighValueTxt, highValue);
  }

  async addAcceptedType(mediaTypeName: string) {
    await this.click(this.chooseAcceptedTypesBtn);
    await this.clickTextButtonWithName(mediaTypeName);
    await this.click(this.chooseModalBtn);
  }

  async removeAcceptedType(mediaTypeName: string) {
    await this.click(this.page.locator(`uui-ref-node-document-type[name="${mediaTypeName}"]`).getByLabel('Remove'));
    await this.click(this.confirmToRemoveBtn);
  }

  async removeMediaStartNode(mediaName: string) {
    await this.click(this.page.locator(`uui-card-media[name="${mediaName}"]`).locator('[label="Remove"]'));
    await this.click(this.confirmToRemoveBtn);
  }

  async clickChooseStartNodeButton() {
    await this.click(this.chooseStartNodeBtn);
  }

  // Richtext Editor
  async clickToolbarOptionByValue(values) {
    for (var index in values) {
      await this.click(this.toolbarCheckboxes.filter({has: this.page.getByLabel(values[index])}).locator('#ticker svg'));
    }
  }

  async addStylesheet(stylesheetName: string) {
    await this.click(this.addStylesheetBtn);
    await this.click(this.page.getByLabel(stylesheetName));
    await this.click(this.chooseModalBtn);
  }

  async enterDimensionsValue(width: string, height: string) {
    await this.enterText(this.dimensionsWidthTxt, width);
    await this.enterText(this.dimensionsHeightTxt, height);
  }

  async enterMaximumSizeForImages(value: string) {
    await this.enterText(this.maxImageSizeTxt, value);
  }

  async clickHideLabelToggle() {
    await this.click(this.hideLabelToggle);
  }

  async clickInlineRadioButton() {
    await this.click(this.inlineRadioBtn);
  }

  async clickChooseWithPlusButton() {
    await this.click(this.chooseWithPlusBtn);
  }

  async addImageUploadFolder(mediaFolderName: string) {
    await this.clickChooseWithPlusButton();
    await this.selectMediaWithName(mediaFolderName);
    await this.clickChooseModalButton();
  }

  async clickAddWithPlusButton() {
    await this.click(this.addWithPlusBtn);
  }

  async addAvailableBlocks(blockName: string) {
    await this.clickAddWithPlusButton();
    await this.clickTextButtonWithName(blockName);
    await this.clickChooseModalButton();
    await this.clickSubmitButton();
  }

  // Tags
  async enterDefineTagGroupValue(value: string) {
    await this.enterText(this.defineTagGroupTxt, value);
  }

  async selectStorageTypeOption(option: string) {
    await this.selectByText(this.storageTypeDropDownBox, option);
  }

  // Content Picker
  async clickShowOpenButtonToggle() {
    await this.click(this.showOpenButtonToggle);
  }

  async removeContentStartNode(contentName: string) {
    const startNodeLocator = this.entityItem.filter({has: this.page.locator(`[name="${contentName}"]`)});
    await this.hoverAndClick(startNodeLocator, startNodeLocator.getByLabel('Remove'));
    await this.clickConfirmRemoveButton();
  }

  // Dropdown
  async clickEnableMultipleChoiceToggle() {
    await this.click(this.enableMultipleChoiceToggle);
  }

  async clickAddOptionsButton() {
    await this.click(this.addOptionsBtn);
  }

  // True/false
  async clickPresetValueToggle() {
    await this.click(this.presetValueToggle);
  }

  async clickShowToggleLabelsToggle() {
    await this.click(this.showToggleLabelsToggle);
  }

  async enterLabelOnValue(value: string) {
    await this.enterText(this.labelOnTxt, value);
  }

  async enterLabelOffValue(value: string) {
    await this.enterText(this.labelOffTxt, value);
  }

  // Block List Editor
  async clickAddBlockButton(index: number = 0) {
    await this.click(this.addBlockBtn.nth(index));
  }

  async clickRemoveBlockWithName(name: string) {
    const blockWithNameLocator = this.page.locator('umb-block-type-card', {hasText: name});
    // The force click is necessary.
    await this.click(blockWithNameLocator.getByLabel('Remove block'), {force: true});
  }

  async enterMinAmount(value: string) {
    await this.enterText(this.minAmountTxt, value);
  }

  async enterMaxAmount(value: string) {
    await this.enterText(this.maxAmountTxt, value);
  }

  async doesAmountContainErrorMessageWithText(errorMessage: string) {
    await this.isVisible(this.page.getByText(errorMessage));
  }

  async clickSingleBlockMode() {
    await this.click(this.singleBlockModeBtn);
  }

  async clickLiveEditingMode() {
    await this.click(this.liveEditingModeBtn);
  }

  async clickInlineEditingMode() {
    await this.click(this.inlineEditingModeBtn);
  }

  async enterPropertyEditorWidth(width: string) {
    await this.enterText(this.propertyEditorWidthTxt, width);
  }

  async goToBlockWithName(name: string) {
    await this.click(this.page.getByRole('link', {name: name}));
  }

  async enterBlockLabelText(label: string) {
    await this.removeBlockLabelText();
    await this.labelTextTxt.fill(label);
  }

  async removeBlockLabelText() {
    await this.waitForVisible(this.labelTextTxt);
    await this.labelTextTxt.clear();
  }

  async clickAllowInRootForBlock() {
    await this.click(this.allowBlockAtRootToggle);
  }

  async clickAllowInAreasForBlock() {
    await this.click(this.allowInAreasToggle);
  }

  async updateBlockOverlaySize(size: string) {
    await this.selectByValue(this.overlaySizeOption, size);
  }

  async addBlockContentModel(elementName: string) {
    await this.click(this.chooseContentModelBtn);
    await this.clickButtonWithName(elementName);
    await this.clickChooseButton();
  }

  async addBlockSettingsModel(elementName: string) {
    await this.click(this.chooseSettingsModelBtn, {timeout: ConstantHelper.timeout.long});
    await this.clickModalMenuItemWithName(elementName);
    await this.clickChooseModalButton();
  }

  async removeBlockContentModel() {
    await this.hoverAndClick(this.contentModelNode, this.removeExactContentModelNodeBtn);
  }

  async removeBlockSettingsModel() {
    await this.hoverAndClick(this.settingsModelNode, this.removeExactSettingsModelNodeBtn);
  }

  async openBlockContentModel() {
    await this.hoverAndClick(this.contentModelNode, this.openBtn);
  }

  async openBlockSettingsModel() {
    await this.hoverAndClick(this.settingsModelNode, this.openBtn);
  }

  async isElementWorkspaceOpenInBlock(elementTypeName: string) {
    await this.isVisible(this.documentTypeWorkspace.filter({hasText: elementTypeName}));
  }

  async selectBlockBackgroundColor(color: string) {
    await this.click(this.backgroundColorBtn);
    await this.enterText(this.backgroundColorTxt, color);
  }

  async selectBlockIconColor(color: string) {
    await this.click(this.iconColorBtn);
    await this.enterText(this.iconColorTxt, color);
  }

  async clickExpandChildItemsForMediaButton() {
    await this.click(this.expandChildItemsForMediaBtn);
  }

  async clickRemoveCustomStylesheetWithName(name: string) {
    await this.click(this.customStylesheetLabel.locator(`[name="${name}"]`));
    await this.click(this.stylesheetRemoveBtn);
    await this.clickConfirmRemoveButton();
  }

  async clickBlockGridHideContentEditorButton() {
    await this.click(this.hideContentEditorBlockGridBtn);
  }

  async chooseBlockCustomStylesheetWithName(name: string) {
    await this.click(this.chooseCustomStylesheetBtn);
    await this.openCaretButtonForName('wwwroot');
    await this.openCaretButtonForName('css');
    await this.clickLabelWithName(name, true);
    await this.clickChooseModalButton();
  }

  async chooseBlockThumbnailWithPath(mediaPath: string) {
    const mediaItems = mediaPath.split('/media/')[1].split('/');
    await this.click(this.chooseThumbnailAlias);
    await this.openCaretButtonForName('wwwroot', true);
    await this.clickExpandChildItemsForMediaButton();
    for (let i = 0; i < mediaItems.length; i++) {
      if (i === mediaItems.length - 1) {
        await this.clickLabelWithName(mediaItems[i], true);
      } else {
        await this.click(this.sidebarModal.locator(`uui-menu-item[label="${mediaItems[i]}"] #caret-button`));
      }
    }
    await this.clickChooseModalButton();
  }

  async clickBlockListHideContentEditorButton() {
    await this.click(this.hideContentEditorBlockListBtn);
  }

  async enterEditorWidth(value: string) {
    await this.enterText(this.editorWidthTxt, value);
  }

  async enterCreateButtonLabel(value: string) {
    await this.enterText(this.createButtonLabelTxt, value);
  }

  async enterGridColumns(value: number) {
    await this.waitForVisible(this.gridColumnsTxt);
    await this.gridColumnsTxt.clear();
    if (value === undefined) {
      return;
    }
    await this.gridColumnsTxt.fill(value.toString());
  }

  async clickShowResizeOptions() {
    await this.click(this.showResizeOptionsBtn);
  }

  async clickAvailableColumnSpans(columnSpans: number[]) {
    for (let index in columnSpans) {
      await this.click(this.columnSpanOptions.getByLabel(columnSpans[index].toString(), {exact: true}));
    }
  }

  async goToBlockAreasTab() {
    await this.click(this.areasTabBtn);
  }

  async enterMinRowSpan(value: number) {
    await this.waitForVisible(this.availableRowSpansLowValueTxt);
    await this.availableRowSpansLowValueTxt.clear();
    if (value === undefined) {
      return;
    }
    await this.availableRowSpansLowValueTxt.fill(value.toString());
  }

  async enterMaxRowSpan(value: number) {
    await this.waitForVisible(this.availableRowSpansHighValueTxt);
    await this.availableRowSpansHighValueTxt.clear();
    if (value === undefined) {
      return;
    }
    await this.availableRowSpansHighValueTxt.fill(value.toString());
  }

  async enterGridColumnsForArea(value: number) {
    await this.waitForVisible(this.areaGridColumnsTxt);
    await this.areaGridColumnsTxt.clear();
    if (value === undefined) {
      return;
    }
    await this.areaGridColumnsTxt.fill(value.toString());
  }

  async addAreaButton() {
    await this.click(this.addAreaBtn);
  }

  async goToAreaByAlias(alias: string) {
    const editAreaWithAliasLocator = this.blockAreaConfig.filter({hasText: alias}).getByLabel('edit');
    // Force click is needed
    await this.click(editAreaWithAliasLocator, {force: true});
  }

  async clickRemoveAreaByAlias(alias: string) {
    const removeAreaWithAliasLocator = this.blockAreaConfig.filter({hasText: alias}).getByLabel('delete');
    // Force click is needed
    await this.click(removeAreaWithAliasLocator, {force: true});
    await this.clickConfirmToDeleteButton();
  }

  async enterAreaAlias(alias: string) {
    await this.enterText(this.aliasAliasTxt, alias);
  }

  async clickAreaSubmitButton() {
    await this.click(this.blockGridAreaWorkspaceSubmitBtn);
    await this.page.waitForTimeout(ConstantHelper.wait.short);
  }

  async enterCreateButtonLabelInArea(value: string) {
    await this.waitForVisible(this.createLabelTxt.nth(1));
    await this.createLabelTxt.nth(1).clear();
    if (value === undefined) {
      return;
    }
    await this.createLabelTxt.nth(1).fill(value);
  }

  async enterMinAllowedInArea(value: number) {
    await this.waitForVisible(this.minAllowedTxt);
    await this.minAllowedTxt.clear();
    if (value === undefined) {
      return;
    }
    await this.minAllowedTxt.fill(value.toString());
  }

  async enterMaxAllowedInArea(value: number) {
    await this.waitForVisible(this.maxAllowedTxt);
    await this.maxAllowedTxt.clear();
    if (value === undefined) {
      return;
    }
    await this.maxAllowedTxt.fill(value.toString());
  }

  async clickAddSpecifiedAllowanceButton() {
    await this.click(this.addSpecifiedAllowanceBtn);
  }

  async goToBlockAdvancedTab() {
    await this.click(this.advancedTabBtn);
  }

  async getLinkWithName(name: string) {
    await this.isVisible(this.page.getByRole('link', {name: name}));
    return this.page.getByRole('link', {name: name});
  }

  async getAddButtonInGroupWithName(name: string) {
    await this.isVisible(this.page.locator('.group').filter({hasText: name}).locator('#add-button'));
    return this.page.locator('.group').filter({hasText: name}).locator('#add-button');
  }

  async clickRemoveStylesheetButton(stylesheetName: string) {
    const removeButton = this.entityItem.filter({hasText: stylesheetName}).getByLabel('Remove');
    await this.click(removeButton);
  }

  // TipTap
  async deleteToolbarGroup(groupIndex: number, rowIndex: number = 0) {
    const groupButton = this.tiptapToolbarConfiguration.locator('.row').nth(rowIndex).locator('.group').nth(groupIndex);
    await this.hover(groupButton);
    const actionsInGroup = groupButton.locator('.items').locator('uui-button');
    const actionsCount = await actionsInGroup.count();
    for (let i = 0; i < actionsCount; i++) {
      await this.click(actionsInGroup.first());
    }
    await this.click(groupButton.locator('[label="Remove group"]'));
  }

  async deleteToolbarRow(rowIndex: number) {
    const rowButton = this.tiptapToolbarConfiguration.locator('.row').nth(rowIndex);
    await this.hoverAndClick(rowButton, rowButton.locator('[label="Remove row"]'));
  }

  async clickAddRowToolbarButton() {
    await this.click(this.addRowToolbarBtn);
  }

  async clickAddGroupToolbarButton() {
    await this.click(this.addGroupToolbarBtn);
  }

  async clickExtensionItemWithName(name: string) {
    await this.click(this.tiptapExtensionsConfiguration.locator(`uui-checkbox[label="${name}"]`));
  }

  async doesPropertyEditorHaveUiAlias(uiAlias: string) {
    await this.hasAttribute(this.propertyEditor, 'alias', uiAlias);
  }

  async doesPropertyEditorHaveName(name: string) {
    await this.hasAttribute(this.propertyEditor, 'name', name);
  }

  async doesPropertyEditorHaveAlias(alias: string) {
    await this.hasAttribute(this.propertyEditor, 'property-editor-schema-alias', alias);
  }

  async clickDataTypeButton() {
    await this.click(this.dataTypeBtn);
  }

  async clickDataTypesMenu() {
    await this.click(this.dataTypesMenu);
  }

  async doesSettingHaveValue(settings) {
    for (let index = 0; index < Object.keys(settings).length; index++) {
      const [label, description] = settings[index];
      await expect(this.propertyEditorConfigItems.nth(index).locator('#headerColumn #label')).toHaveText(label);
      if (description !== '')
        await expect(this.propertyEditorConfigItems.nth(index).locator('#description')).toHaveText(description);
    }
  }

  async doesSettingItemsHaveCount(settings) {
    await this.hasCount(this.propertyEditorConfigItems, Object.keys(settings).length);
  }

  async doesSettingsContainText(text: string) {
    await this.containsText(this.propertyEditorConfig, text);
  }

  async clickStatusbarItemInToolboxWithName(name: string) {
    const statusbarItemLocator = this.tiptapStatusbarConfiguration.locator('#toolbox uui-button').filter({hasText: name});
    await this.click(statusbarItemLocator);
  }

  async clickStatusbarItemWithName(name: string) {
    const statusbarItemLocator = this.tiptapStatusbarConfiguration.locator('#statusbar uui-button').filter({hasText: name});
    await this.click(statusbarItemLocator);
  }

  async isExtensionItemChecked(itemName: string, isChecked: boolean = true) {
    await expect(this.tiptapExtensionsConfiguration.locator(`uui-checkbox[label="${itemName}"] input`)).toBeChecked({checked: isChecked});
  }

  async doesBlockHaveThumbnailImage(blockName: string, thumbnailImageUrl: string) {
    const blockCardLocator = this.blockTypeCard.filter({hasText: blockName});
    await this.hasAttribute(blockCardLocator.locator('img'), 'src', thumbnailImageUrl);
  }

  async addTimeZones(timeZones: string[]) {
    for (let i = 0; i < timeZones.length; i++) {
      await this.click(this.timeZoneDropDown);
      await this.click(this.timeZoneDropDown.getByText(timeZones[i]));
      await this.click(this.addTimeZoneBtn);
    }
  }

  async clickChooseDataSourceButton() {
    await this.click(this.dataSourceChooseBtn);
  }

  async clickChooseThumbnailButton() {
    await this.click(this.chooseThumbnailAlias);
  }

  async clickSaveButtonAndWaitForDataTypeToBeCreated() {
    return await this.waitForResponseAfterExecutingPromise(ConstantHelper.apiEndpoints.dataType, this.clickSaveButton(), ConstantHelper.statusCodes.created);
  }

  async clickSaveButtonAndWaitForDataTypeToBeUpdated() {
    return await this.waitForResponseAfterExecutingPromise(ConstantHelper.apiEndpoints.dataType, this.clickSaveButton(), ConstantHelper.statusCodes.ok);
  }

  async clickConfirmToDeleteButtonAndWaitForDataTypeToBeDeleted() {
    return await this.waitForResponseAfterExecutingPromise(ConstantHelper.apiEndpoints.dataType, this.clickConfirmToDeleteButton(), ConstantHelper.statusCodes.ok);
  }

  async clickDeleteAndConfirmButtonAndWaitForDataTypeToBeDeleted() {
    return await this.waitForResponseAfterExecutingPromise(ConstantHelper.apiEndpoints.dataType, this.clickDeleteAndConfirmButton(), ConstantHelper.statusCodes.ok);
  }

  async clickConfirmCreateFolderButtonAndWaitForDataTypeToBeCreated() {
    return await this.waitForResponseAfterExecutingPromise(ConstantHelper.apiEndpoints.dataTypeFolder, this.clickConfirmCreateFolderButton(), ConstantHelper.statusCodes.created);
  }

  async clickConfirmRenameButtonAndWaitForDataTypeToBeRenamed() {
    return await this.waitForResponseAfterExecutingPromise(ConstantHelper.apiEndpoints.dataTypeFolder, this.clickConfirmRenameButton(), ConstantHelper.statusCodes.ok);
  }

  async removeBlockThumbnail() {
    await this.click(this.blockThumbnailRemoveBtn);
    await this.clickConfirmRemoveButton();
  }

  async doesBlockHaveNoThumbnailImage(blockName: string) {
    const blockCardLocator = this.blockTypeCard.filter({hasText: blockName});
    await expect(blockCardLocator.locator('img')).toHaveCount(0);
  }

  // Dynamic Root
  async clickDefineDynamicRootButton() {
    await this.click(this.dynamicRootPlaceholderBtn);
  }

  async chooseDynamicRootOrigin(originName: string) {
    await expect(this.dynamicRootOriginPickerModal).toBeVisible();
    await this.click(this.dynamicRootOriginPickerModal.locator(`umb-ref-item[name="${originName}"]`));
  }

  async clickEditDynamicRootOriginButton(originName: string) {
    const editButton = this.dynamicRootComponent.locator(`uui-ref-node[name="${originName}"]`).locator('uui-action-bar uui-button');
    await this.click(editButton);
  }

  async clickAddDynamicRootQueryStepButton() {
    await this.click(this.dynamicRootPlaceholderBtn);
  }

  async chooseDynamicRootQueryStep(stepName: string) {
    await expect(this.dynamicRootQueryStepPickerModal).toBeVisible();
    await this.click(this.dynamicRootQueryStepPickerModal.locator(`umb-ref-item[name="${stepName}"]`));
  }

  async isDynamicRootOriginPickerModalVisible() {
    await this.isVisible(this.dynamicRootOriginPickerModal);
  }

  async isDynamicRootOriginInPickerModal(originName: string, detail?: string) {
    const item = this.dynamicRootOriginPickerModal.locator(`umb-ref-item[name="${originName}"]`);
    await this.isVisible(item);
    if (detail) {
      await this.hasAttribute(item, 'detail', detail);
    }
  }

  async closeDynamicRootOriginPickerModal() {
    await this.click(this.closeDynamicRootOriginPickerModalBtn);
  }
}