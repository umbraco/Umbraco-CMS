import {ConstantHelper, test} from '@umbraco/acceptance-test-helpers';
import {expect} from "@playwright/test";

// There is one label editor per type of value it holds, so the type a label property yields - and the column
// its value is stored in - follow from the editor rather than from a configured value type.
const typedLabels = [
  {propertyEditorName: 'Label (long string)', editorAlias: 'Umbraco.Label.Text', editorUiAlias: 'Umb.PropertyEditorUi.Label.Text'},
  {propertyEditorName: 'Label (integer)', editorAlias: 'Umbraco.Label.Integer', editorUiAlias: 'Umb.PropertyEditorUi.Label.Integer'},
  {propertyEditorName: 'Label (big integer)', editorAlias: 'Umbraco.Label.BigInt', editorUiAlias: 'Umb.PropertyEditorUi.Label.BigInt'},
  {propertyEditorName: 'Label (decimal)', editorAlias: 'Umbraco.Label.Decimal', editorUiAlias: 'Umb.PropertyEditorUi.Label.Decimal'},
  {propertyEditorName: 'Label (date and time)', editorAlias: 'Umbraco.Label.DateTime', editorUiAlias: 'Umb.PropertyEditorUi.Label.DateTime'},
  {propertyEditorName: 'Label (time)', editorAlias: 'Umbraco.Label.Time', editorUiAlias: 'Umb.PropertyEditorUi.Label.Time'}
];
const customDataTypeName = 'Custom Typed Label';

test.beforeEach(async ({umbracoUi, umbracoApi}) => {
  await umbracoUi.goToBackOffice();
  await umbracoUi.dataType.goToSettingsTreeItem('Data Types');
  await umbracoApi.dataType.ensureNameNotExists(customDataTypeName);
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.dataType.ensureNameNotExists(customDataTypeName);
});

for (const typedLabel of typedLabels) {
  test(`can create a ${typedLabel.propertyEditorName} data type`, async ({umbracoApi, umbracoUi}) => {
    // Act
    await umbracoUi.dataType.clickActionsMenuForName('Data Types');
    await umbracoUi.dataType.clickCreateActionMenuOption();
    await umbracoUi.dataType.clickDataTypeButton();
    await umbracoUi.dataType.enterDataTypeName(customDataTypeName);
    await umbracoUi.dataType.clickSelectAPropertyEditorButton();
    await umbracoUi.dataType.selectAPropertyEditor(typedLabel.propertyEditorName);
    await umbracoUi.dataType.clickSaveButtonAndWaitForDataTypeToBeCreated();

    // Assert
    await umbracoUi.dataType.isDataTypeTreeItemVisible(customDataTypeName);
    const dataTypeData = await umbracoApi.dataType.getByName(customDataTypeName);
    expect(dataTypeData.editorAlias).toBe(typedLabel.editorAlias);
    expect(dataTypeData.editorUiAlias).toBe(typedLabel.editorUiAlias);
    // The value type is no longer configured, so it must not be written by picking the editor.
    expect(await umbracoApi.dataType.doesDataTypeHaveValue(customDataTypeName, 'umbracoDataValueType')).toBeFalsy();
  });

  test(`the settings of a ${typedLabel.propertyEditorName} are correct`, async ({umbracoApi, umbracoUi}) => {
    // Arrange
    await umbracoApi.dataType.createTypedLabelDataType(customDataTypeName, typedLabel.editorAlias, typedLabel.editorUiAlias);

    // Act
    await umbracoUi.dataType.goToDataType(customDataTypeName);

    // Assert
    await umbracoUi.dataType.doesSettingHaveValue(ConstantHelper.labelSettings);
    await umbracoUi.dataType.doesSettingItemsHaveCount(ConstantHelper.labelSettings);
    await umbracoUi.dataType.doesPropertyEditorHaveAlias(typedLabel.editorAlias);
    await umbracoUi.dataType.doesPropertyEditorHaveUiAlias(typedLabel.editorUiAlias);
  });
}
