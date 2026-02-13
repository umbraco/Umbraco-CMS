import { UiBaseLocators } from "./UiBaseLocators";
import { Locator, Page } from "@playwright/test";
import { ConstantHelper } from "./ConstantHelper";

export class FormsUiHelper extends UiBaseLocators {
  private readonly quickCreateNewBtn: Locator;
  private readonly createNewFormModalBtn: Locator;
  private readonly saveFormBtn: Locator;
  private readonly formNameTxt: Locator;
  private readonly formPageNametxt: Locator;
  private readonly formGroupNameTxt: Locator;
  private readonly formExpandBtn: Locator;
  private readonly formAddNewPageBtn: Locator;
  private readonly formAddNewGroupBtn: Locator;
  private readonly formPage: Locator;
  private readonly formAddQuestionBtn: Locator;
  private readonly formFieldType: Locator;
  private readonly formFieldNameTxt: Locator;
  private readonly formSubmitButtonModal: Locator;
  private readonly formTree: Locator;
  private readonly formActionModal: Locator;
  private readonly formDeleteThreeDotBtn: Locator;
  private readonly formWorkspaceEditor: Locator;
  private readonly formSettingIcon: Locator;
  private readonly formSettingStoreRecordBtn: Locator;
  private readonly formSettingCaptions: Locator;
  private readonly formSettingStyling: Locator;
  private readonly formSettingValidation: Locator;
  private readonly formSettingAutocomplete: Locator;
  private readonly formSettingModeration: Locator;
  private readonly formSettingFieldsDisplayed: Locator;
  private readonly formSettingDataRetention: Locator;
  private readonly formSettingCaptionsLabel: Locator;
  private readonly formSettingStylingLabel: Locator;
  private readonly formSettingValidationLabel: Locator;
  private readonly formSettingAutocompleteLabel: Locator;
  private readonly formSettingModerationLabel: Locator;
  private readonly formSettingFieldsDisplayedLabel: Locator;
  private readonly formSettingDataRetentionLabel: Locator;
  private readonly formWorkflowConfigureBtn: Locator;
  private readonly formWorkflowOnSubmitStage: Locator;
  private readonly formWorkflowAddButtonModal: Locator;
  private readonly formConfigureWorkflowModal: Locator;
  private readonly formEditWorkflowModal: Locator;
  private readonly formEditFieldModal: Locator;
  private readonly formMenuItemForForm: Locator;
  private readonly formInputTxt: Locator;
  private readonly formInputNumber: Locator;
  private readonly formToggleSlider: Locator;
  private readonly formMenuItemForPrevalueSource: Locator;
  private readonly createNewPrevaluesourceModalBtn: Locator;
  private readonly prevalueSourceExpandBtn: Locator;
  private readonly prevalueSourceTree: Locator;
  private readonly prevalueSourceDeleteBtn: Locator;
  private readonly formFieldMandatory: Locator;
  private readonly formFieldRegex: Locator;
  private readonly formWorkflowNameTxt: Locator;
  private readonly prevalueSourceCacheContainer: Locator;
  private readonly prevalueSourceTypeLabel: Locator;
  private readonly formSettingFieldsDisplayedContainer: Locator;
  private readonly formSettingDataRetentionContainer: Locator;
  private readonly formSettingValidationContainer: Locator;
  private readonly formSettingCaptionsContainer: Locator;
  private readonly formSettingStylingContainer: Locator;
  private readonly formSettingAutocompleteContainer: Locator;
  private readonly formSettingModerationContainer: Locator;
  private readonly prevalueSourceOriginModal: Locator;

  constructor(page: Page) {
    super(page);
    this.quickCreateNewBtn = page.locator('uui-button[label="Create…"]');
    this.createNewFormModalBtn = page.locator('#menu-item').getByLabel('New Form…');
    this.createNewPrevaluesourceModalBtn = page.locator('umb-ref-item');
    this.saveFormBtn = page.getByLabel('Save', {exact: true});
    this.formNameTxt = page.locator('#nameInput input[aria-label="Enter a name…"]');
    this.formPageNametxt = page.locator('input[type = "text"][placeholder = "Untitled page (optional)"]');
    this.formGroupNameTxt = page.locator('input[type = "text"][placeholder = "Untitled group (optional)"]');
    this.formAddNewPageBtn = page.getByLabel('Add new page', {exact: true});
    this.formAddNewGroupBtn = page.getByLabel('Add new group', {exact: true});
    this.formPage = page.locator('forms-form-page');
    this.formAddQuestionBtn = page.getByLabel("Add question", {exact: true});
    this.formEditFieldModal = page.locator("form-edit-field-modal");
    this.formFieldType = page.locator('#btn-item');
    this.formFieldNameTxt = page.locator('#caption input[type="text"][aria-label="caption"]');
    this.formSubmitButtonModal = page.getByLabel('Submit', {exact: true});
    this.formExpandBtn = page.locator('#menu-item button[aria-label="Expand child items for Forms"] svg');
    this.formTree = page.locator('umb-tree[alias="Forms.Tree.Forms"]');
    this.formMenuItemForForm = page.locator('uui-menu-item[label="Forms"]');
    this.formMenuItemForPrevalueSource = page.locator('uui-menu-item[label="Prevalue Sources"]');
    this.formActionModal = page.locator('#action-modal');
    this.formDeleteThreeDotBtn = page.locator('uui-menu-item[label="Delete…"]');
    this.formWorkspaceEditor = page.locator('umb-workspace-editor[alias="Forms.Workspace.Form"]');
    this.formSettingIcon = page.locator('umb-icon[name="settings"]');
    this.formSettingStoreRecordBtn = page.locator('forms-settings-store-records');
    this.formSettingCaptions = page.locator('forms-settings-captions');
    this.formSettingStyling = page.locator('forms-settings-styling');
    this.formSettingValidation = page.locator('forms-settings-validation');
    this.formSettingAutocomplete = page.locator('forms-settings-autocomplete');
    this.formSettingModeration = page.locator('forms-settings-moderation');
    this.formSettingFieldsDisplayed = page.locator('forms-settings-fields-display');
    this.formSettingDataRetention = page.locator('forms-settings-data-retention');
    this.formSettingCaptionsLabel = page.locator('uui-label[title="captions"]');
    this.formSettingStylingLabel = page.locator('uui-label[title="styling"]');
    this.formSettingValidationLabel = page.locator('uui-label[title="validation"]');
    this.formSettingAutocompleteLabel = page.locator('uui-label[title="autocomplete"]');
    this.formSettingModerationLabel = page.locator('uui-label[title="manualApproval"]');
    this.formSettingFieldsDisplayedLabel = page.locator('uui-label[title="fieldsDisplayed"]');
    this.formSettingDataRetentionLabel = page.locator('uui-label[title="dataRetention"]');
    this.formWorkflowConfigureBtn = page.getByLabel('Configure workflow', { exact: true });
    this.formWorkflowOnSubmitStage = page.locator('form-configure-workflow-stage[collectionname="onSubmit"]');
    this.formWorkflowAddButtonModal = page.locator('.stage-block', {hasText: "Add workflow"});
    this.formConfigureWorkflowModal = page.locator('form-configure-workflow-modal');
    this.formEditWorkflowModal = page.locator('form-edit-workflow-modal');
    this.formInputTxt = page.locator('input[type = "text"]');
    this.formInputNumber = page.locator('input[type = "number"]');
    this.formToggleSlider = page.locator('uui-toggle #toggle');
    this.prevalueSourceExpandBtn = page.locator('#menu-item button[aria-label="Expand child items for Prevalue Sources"] svg');
    this.prevalueSourceTree = page.locator('umb-tree[alias="Forms.Tree.PrevalueSources"]');
    this.prevalueSourceDeleteBtn = page.locator('uui-button[label="Delete..."]');
    this.prevalueSourceCacheContainer = this.page.locator('umb-property-layout[alias="_cachePrevalues"]');
    this.prevalueSourceTypeLabel = this.page.locator("[label = 'Type']");
    this.formFieldMandatory = this.page.locator('umb-property-layout[alias="mandatory"]');
    this.formFieldRegex = this.page.locator('umb-property-layout[alias="regex"]');
    this.formWorkflowNameTxt = this.page.locator('umb-property-layout[alias="name"] input[type=text]');
    this.formSettingFieldsDisplayedContainer = this.formSettingFieldsDisplayed.locator(this.formSettingFieldsDisplayedLabel);
    this.formSettingDataRetentionContainer = this.formSettingDataRetention.locator(this.formSettingDataRetentionLabel);
    this.formSettingValidationContainer = this.formSettingValidation.locator(this.formSettingValidationLabel);
    this.formSettingCaptionsContainer = this.formSettingCaptions.locator(this.formSettingCaptionsLabel);
    this.formSettingStylingContainer = this.formSettingStyling.locator(this.formSettingStylingLabel);
    this.formSettingAutocompleteContainer = this.formSettingAutocomplete.locator(this.formSettingAutocompleteLabel);
    this.formSettingModerationContainer = this.formSettingModeration.locator(this.formSettingModerationLabel);
    this.prevalueSourceOriginModal = this.page.locator('umb-body-layout[headline="Pick origin"]');
  }

  /*
   * Methods for Forms.
   */

  async clickQuickCreateFormButton() {
    await this.hoverAndClick(this.formMenuItemForForm, this.formMenuItemForForm.locator(this.quickCreateNewBtn));
  }

  async clickNewFormButton() {
    await this.click(this.createNewFormModalBtn);
  }

  async clickSaveFormButton() {
    await this.click(this.saveFormBtn);
  }

  async fillFormName(name: string) {
    await this.enterText(this.formNameTxt, name);
  }

  async fillFormPageName(position: number, name: string) {
    const nameInput = this.formPageNametxt.nth(position);
    await this.enterText(nameInput, name);
  }

  async fillFormGroupName(position: number, name: string) {
    const groupInput = this.formGroupNameTxt.nth(position);
    await this.enterText(groupInput, name);
  }

  async fillFormFieldName(name: string) {
    await this.formEditFieldModal.locator(this.formFieldNameTxt).fill(name);
  }

  async clickAddNewPageButton() {
    await this.click(this.formAddNewPageBtn);
  }

  async clickAddNewGroupButton() {
    await this.click(this.formAddNewGroupBtn);
  }

  async clickAddQuestionButton(index: number = 0) {
    const button = this.formPage.nth(index).locator(this.formAddQuestionBtn);
    await this.click(button);
  }

  async chooseFormFieldType(type: string) {
    await this.click(this.formFieldType.filter({hasText: type}).nth(0));
  }

  async clickExpandFormsTreeButton() {
    await this.click(this.formExpandBtn);
  }

  async doesFormTreeHaveFormName(name: string) {
    await this.containsText(this.formTree, name);
  }

  async goToFormWithName(name: string) {
    await this.click(this.formTree.getByText(name, {exact: true}));
  }

  async clickFormFieldTypeSubmitModal() {
    await this.click(this.formSubmitButtonModal);
  }

  async clickActionMenuOnFormMenuItem(name: string) {
    await this.click(this.menuItem.locator(`[label="${name}"] uui-button[label="Open actions menu"]`));
  }

  async clickDeleteFormButton() {
    await this.click(this.formActionModal.locator(this.formDeleteThreeDotBtn));
    await this.click(this.deleteExactBtn);
  }

  async goToFormSetting() {
    await this.click(this.formWorkspaceEditor.locator(this.formSettingIcon));
  }

  async setFormStoreRecordsSetting() {
    await this.waitForVisible(this.formSettingStoreRecordBtn);
    const toggle = this.formSettingStoreRecordBtn.locator(this.formToggleSlider);
    await this.waitForVisible(toggle);
    await toggle.check();
  }

  async setFormCaptionsSetting() {
    await this.waitForVisible(this.formSettingCaptionsContainer);
    for (let i = 0; i < 3; i++) {
      const captionInput = this.formSettingCaptions.locator(this.formInputTxt).nth(i);
      await this.enterText(captionInput, `Test Caption ${i + 1}`);
    }
  }

  async setFormStylingSetting() {
    await this.waitForVisible(this.formSettingStylingContainer);
    const cssClassInput = this.formSettingStyling.locator(this.formInputTxt);
    await this.enterText(cssClassInput, "custom-css-class");
    const disableDefaultStylesheetInput = this.formSettingStyling.locator(this.formToggleSlider);
    await this.click(disableDefaultStylesheetInput);
  }

  async setFormValidationSetting() {
    await this.waitForVisible(this.formSettingValidationContainer);
    const requiredErrorMessageInput = this.formSettingValidation.locator(this.formInputTxt).nth(0);
    await this.enterText(requiredErrorMessageInput, "Required error message");
    const invalidErrorMessageInput = this.formSettingValidation.locator(this.formInputTxt).nth(1);
    await this.enterText(invalidErrorMessageInput, "Invalid error message");

    const showValidationSummaryInput = this.formSettingValidation.locator(this.formToggleSlider).nth(0);
    await this.click(showValidationSummaryInput);
    const hideFieldValidationInput = this.formSettingValidation.locator(this.formToggleSlider).nth(1);
    await this.click(hideFieldValidationInput);

    const markMandatoryFieldRadioInput = this.formSettingValidation.locator("uui-radio[value = 'MarkMandatoryFields']");
    await this.click(markMandatoryFieldRadioInput);

    const indicatorInput = this.formSettingValidation.locator(this.formInputTxt).nth(2);
    await this.enterText(indicatorInput, "+");
  }

  async setFormAutocompleteSetting() {
    await this.waitForVisible(this.formSettingAutocompleteContainer);
    const autocompleteAttributeRadioInput = this.formSettingAutocomplete.locator('uui-radio[value = "On"]');
    await this.click(autocompleteAttributeRadioInput);
  }

  async setFormModerationSetting() {
    await this.waitForVisible(this.formSettingModerationContainer);
    const enablePostModerationAttributeToggleInput = this.formSettingModeration.locator(this.formToggleSlider);
    await this.click(enablePostModerationAttributeToggleInput);
  }

  async setFormFieldsDisplayedSetting() {
    await this.waitForVisible(this.formSettingFieldsDisplayedContainer);
    const displayDefaultFieldsToggleInput = this.formSettingFieldsDisplayed.locator(this.formToggleSlider);
    await this.click(displayDefaultFieldsToggleInput);
    await this.page.waitForTimeout(ConstantHelper.wait.minimal); // short pause required here otherwise revealed elements are not found
    const displayFieldsSelect = this.formSettingFieldsDisplayed.locator("select");
    await this.selectByValue(displayFieldsSelect, '_system_state');
    const displayFieldsAddButton = this.formSettingFieldsDisplayed.locator("button[id='button']");
    await this.click(displayFieldsAddButton);
  }

  async setFormDataRetentionSetting(recordNumber: string) {
    await this.waitForVisible(this.formSettingDataRetentionContainer);
    const retainSubmittedRecordsToggleInput = this.formSettingDataRetention.locator(this.formToggleSlider).nth(0);
    await this.click(retainSubmittedRecordsToggleInput);
    await this.page.waitForTimeout(ConstantHelper.wait.minimal); // short pause required here otherwise revealed elements are not found
    const retainSubmittedRecordsNumberInput = this.formSettingDataRetention.locator(this.formInputNumber).nth(0);
    await this.enterText(retainSubmittedRecordsNumberInput, recordNumber);
  }

  async toggleFieldSetting(settingAlias: string) {
    const settingFieldLocator = this.page.locator(`umb-property-layout[alias="${settingAlias}"] #toggle`);
    await this.click(settingFieldLocator);
  }

  async applyFieldSettingViaTextInput(settingAlias: string, settingValue: string) {
    const settingFieldLocator = this.page.locator(`umb-property[alias="${settingAlias}"] input`);
    await settingFieldLocator.fill(settingValue);
  }

  async applyFieldSettingViaDropDown(settingAlias: string, settingValue: string) {
    const settingFieldLocator = this.page.locator(`umb-property[alias="${settingAlias}"] select`);
    await this.selectByValue(settingFieldLocator, settingValue);
  }

  async applyFieldSettingViaSlider(settingAlias: string) {
    const settingFieldLocator = this.page.locator(`umb-property[alias="${settingAlias}"] #toggle`);
    await this.click(settingFieldLocator);
  }

  async applyFieldFileUploadSettings(settingAlias: string, allowedProvidedExtensions: Array<string>, allowedCustomExtensions: Array<string>, allowMultiple: boolean) {
    const settingFieldLocator = this.page.locator(`umb-property-layout[alias="${settingAlias}"]`);
    for (var i = 0; i < allowedProvidedExtensions.length; i++) {
      const checkBoxLocator = settingFieldLocator.locator('uui-toggle', {hasText: allowedProvidedExtensions[i].toUpperCase()}).locator('#toggle');
      await this.click(checkBoxLocator);
    }

    const addNewExtensionLocator = settingFieldLocator.locator('input[placeholder = "Add new allowed file type"]');
    await this.waitForVisible(addNewExtensionLocator);
    const buttonLocator = settingFieldLocator.locator('form svg');
    await this.waitForVisible(buttonLocator);
    for (var i = 0; i < allowedCustomExtensions.length; i++) {
      await addNewExtensionLocator.fill(allowedCustomExtensions[i]);
      await this.click(buttonLocator);
    }

    if (allowMultiple) {
      const alias = "allowMultipleFileUploads";
      const multipleUploadLocator = this.page.locator(`umb-property-layout[alias="${alias}"] #toggle`);
      await this.click(multipleUploadLocator);
    }
  }

  async applyFieldPrevalues(settingAlias: string, prevalues: Array<any>) {
    const settingFieldLocator = this.page.locator(`umb-property-layout[alias="${settingAlias}"]`);
    for (var i = 0; i < prevalues.length; i++) {
      const valueFieldLocator = settingFieldLocator.locator("input[placeholder = 'New value']");
      await this.waitForVisible(valueFieldLocator);
      await valueFieldLocator.fill(prevalues[i].value);

      const captionFieldLocator = settingFieldLocator.locator("input[placeholder = 'New caption']");
      await this.enterText(captionFieldLocator, prevalues[i].caption);

      const buttonLocator = settingFieldLocator.locator('uui-button[label="add"]');
      await this.click(buttonLocator);
    }
  }

  async applyFieldSettingViaTextArea(settingAlias: string, settingValue: string) {
    const settingFieldLocator = this.page.locator(`umb-property[alias="${settingAlias}"] textarea`);
    await settingFieldLocator.fill(settingValue);
  }

  async applyFieldSettingViaRichTextInput(settingAlias: string, settingValue: string) {
    const richTextAreaTxt = this.page.locator(`umb-property[alias="${settingAlias}"] umb-property-editor-ui-tiptap`).locator('#editor .tiptap');
    await this.waitForVisible(richTextAreaTxt);
    await richTextAreaTxt.fill(settingValue);
  }

  async applyFieldSettingViaRange(settingAlias: string, settingValue: string) {
    const settingFieldLocator = this.page.locator(`umb-property[alias="${settingAlias}"]`);
    await settingFieldLocator.locator('input[type="range"]').fill(settingValue);
  }

  async applyFieldSettingViaFieldMappingInput(settingAlias: string, settingValue: Array<any>) {
    const settingFieldLocator = this.page.locator(`umb-property[alias="${settingAlias}"]`);
    await this.waitForVisible(settingFieldLocator);

    for (let i = 0; i < settingValue.length; i++) {
      const buttonLocator = settingFieldLocator.locator('uui-button[label="add"]');
      await this.click(buttonLocator);

      const aliasInputLocator = settingFieldLocator.locator("input[placeholder = 'Alias']").nth(i);
      await this.waitForVisible(aliasInputLocator);
      await aliasInputLocator.fill(settingValue[i].alias);

      const staticValueInputLocator = settingFieldLocator.locator("input[placeholder = 'Static value']").nth(i);
      await this.waitForVisible(staticValueInputLocator);
      await staticValueInputLocator.fill(settingValue[i].staticValue);
    }
  }

  async applyFieldSettingViaDocumentMapper(settingAlias: string, settingValue: any) {
    const settingFieldLocator = this.page.locator(`umb-property[alias="${settingAlias}"]`);
    await this.waitForVisible(settingFieldLocator);

    const selectLocator = settingFieldLocator.locator("forms-document-mapper-property-editor select");
    await this.selectByValue(selectLocator, settingValue.doctype);

    const inputLocator = settingFieldLocator.locator('forms-document-mapper-property-editor input[type = "text"]');
    await this.waitForVisible(inputLocator.first());
    const inputLocatorCount = await inputLocator.count();
    for (let i = 0; i < inputLocatorCount; i++) {
      await this.waitForVisible(inputLocator.nth(i));
      await inputLocator.nth(i).fill(settingValue.nameStaticValue);
    }
  }

  async applyFieldSettingViaEmailTemplatePicker(settingAlias: string, settingValue: string) {
    const settingFieldLocator = this.page.locator(`umb-property[alias="${settingAlias}"]`);
    await this.waitForVisible(settingFieldLocator);

    const buttonLocator = settingFieldLocator.locator("#caret-button");
    await this.click(buttonLocator);

    const templateLocator = this.page.locator("#label-button", {hasText: settingValue});
    await this.click(templateLocator);
  }

  async applyFieldSettingViaStandardFieldMappingInput(settingAlias: string, settingValue: Array<any>) {
    const settingFieldLocator = this.page.locator(`umb-property[alias="${settingAlias}"]`);
    await this.waitForVisible(settingFieldLocator);

    for (let i = 0; i < settingValue.length; i++) {
      if (settingValue[i].include) {
        const includeButtonLocator = settingFieldLocator.locator(`div[data-umb-standard-field-mapping-include="${settingValue[i].alias}"] #toggle`);
        await this.click(includeButtonLocator);
      }

      const keyNameButtonLocator = settingFieldLocator.locator(`div[data-umb-standard-field-mapping-key-name="${settingValue[i].alias}"] input[type="text"]`);
      await keyNameButtonLocator.fill(settingValue[0].keyName);
    }
  }

  async setFieldMandatory(message: string) {
    await this.click(this.formFieldMandatory.locator("#toggle"));
    await this.page.waitForTimeout(ConstantHelper.wait.medium);
    const inputLocator = this.formFieldMandatory.locator(this.formInputTxt);
    await this.enterText(inputLocator, message);
  }

  async setFieldValidation(label: string, message: string) {
    await this.waitForVisible(this.formFieldRegex);
    const selectLocator = this.formFieldRegex.locator("select");
    await this.selectByText(selectLocator, label);
    await this.page.waitForTimeout(ConstantHelper.wait.medium);
    const inputLocator = this.formFieldRegex.locator("input");
    await this.enterText(inputLocator, message);
  }

  async clickFormWorkflowConfigureButton() {
    await this.click(this.formWorkflowConfigureBtn);
  }

  async clickFormWorkflowEditSubmitButton() {
    await this.click(this.formEditWorkflowModal.locator(this.formSubmitButtonModal));
  }

  async clickFormWorkflowConfigureSubmitButton() {
    await this.click(this.formConfigureWorkflowModal.locator(this.formSubmitButtonModal));
  }

  async clickFormWorkflowAddButton() {
    await this.click(this.formWorkflowOnSubmitStage.locator(this.formWorkflowAddButtonModal), {force: true});
  }

  async selectWorkflowType(workflowType: string) {
    await this.click(this.page.locator(`umb-ref-item[title="${workflowType}"]`));
  }

  async fillWorkflowName(workflowName: string) {
    await this.enterText(this.formWorkflowNameTxt, workflowName);
  }

  /*
   * Methods for Prevalue Source.
   */

  async clickQuickCreatePrevalueSourceButton() {
    await this.hoverAndClick(this.formMenuItemForPrevalueSource, this.formMenuItemForPrevalueSource.locator(this.quickCreateNewBtn));
  }

  async clickPrevalueSourceTypeButton(type: string) {
    const button = this.createNewPrevaluesourceModalBtn.locator("#name", {hasText: type});
    await this.click(button);
  }

  async clickExpandPrevalueSourceTreeButton() {
    await this.click(this.prevalueSourceExpandBtn);
  }

  async goToPrevalueSourceWithName(name: string) {
    await this.click(this.prevalueSourceTree.locator(`uui-menu-item[label="${name}"]`));
  }

  async clickDeletePrevalueSourceButton(name: string) {
    const prevalueSource = this.prevalueSourceTree.locator(`uui-menu-item[label="${name}"]`);
    await this.click(prevalueSource.locator(this.prevalueSourceDeleteBtn));
    await this.click(this.deleteExactBtn);
  }

  async applyCacheOptions(option: string, timeValue: number = 0, timeUnit: string = "") {
    await this.waitForVisible(this.prevalueSourceCacheContainer);
    const optionSelect = this.prevalueSourceCacheContainer.locator(`uui-radio[value = "${option}"]`);
    await this.click(optionSelect);

    if (option === "time") {
      const numberInput = this.prevalueSourceCacheContainer.locator("input[type='number']");
      await this.waitForVisible(numberInput);
      await numberInput.fill(timeValue.toString());

      const unitSelect = this.prevalueSourceCacheContainer.locator("select");
      await this.selectByValue(unitSelect, timeUnit);
    }
  }

  async applyPrevalueSourceSettingViaNodeSelector(labelText: string, settingValue: string) {
    const container = this.page.locator(`umb-property[alias="${labelText}"]`);
    await this.waitForVisible(container);
    const rootNode = container.locator('uui-button[label="Specify root node"]');
    await this.click(rootNode);
    await this.waitForVisible(this.prevalueSourceOriginModal);
    const value = this.prevalueSourceOriginModal.locator(`umb-ref-item[name="${settingValue}"]`);
    await this.click(value);
  }

  async checkPrevalueSourceTypeLabel() {
    await this.waitForVisible(this.prevalueSourceTypeLabel);
  }
}