import {ConstantHelper, test} from "@umbraco/acceptance-test-helpers";
import {expect} from "@playwright/test";

// There is one label editor per type of value it holds, so the type follows from the editor rather than
// from a configured value type.
const labelTypes = [
  {type: 'Label (bigint)', editorAlias: 'Umbraco.Label.BigInt', editorUiAlias: 'Umb.PropertyEditorUi.Label.BigInt'},
  {type: 'Label (datetime)', editorAlias: 'Umbraco.Label.DateTime', editorUiAlias: 'Umb.PropertyEditorUi.Label.DateTime'},
  {type: 'Label (decimal)', editorAlias: 'Umbraco.Label.Decimal', editorUiAlias: 'Umb.PropertyEditorUi.Label.Decimal'},
  {type: 'Label (integer)', editorAlias: 'Umbraco.Label.Integer', editorUiAlias: 'Umb.PropertyEditorUi.Label.Integer'},
  {type: 'Label (string)', editorAlias: 'Umbraco.Label', editorUiAlias: 'Umb.PropertyEditorUi.Label'},
  {type: 'Label (time)', editorAlias: 'Umbraco.Label.Time', editorUiAlias: 'Umb.PropertyEditorUi.Label.Time'}
];
const customDataTypeName = 'Custom Label';

test.beforeEach(async ({umbracoUi, umbracoApi}) => {
  await umbracoUi.goToBackOffice();
  await umbracoUi.dataType.goToSettingsTreeItem('Data Types');
  await umbracoApi.dataType.ensureNameNotExists(customDataTypeName);
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.dataType.ensureNameNotExists(customDataTypeName);
});

for (const label of labelTypes) {
  test(`the default configuration of ${label.type} is correct`, async ({umbracoApi, umbracoUi}) => {
    // Act
    await umbracoUi.dataType.goToDataType(label.type);

    // Assert
    await umbracoUi.dataType.doesSettingHaveValue(ConstantHelper.labelSettings);
    await umbracoUi.dataType.doesSettingItemsHaveCount(ConstantHelper.labelSettings);
    await umbracoUi.dataType.doesPropertyEditorHaveAlias(label.editorAlias);
    await umbracoUi.dataType.doesPropertyEditorHaveUiAlias(label.editorUiAlias);
    const dataTypeDefaultData = await umbracoApi.dataType.getByName(label.type);
    expect(dataTypeDefaultData.editorAlias).toBe(label.editorAlias);
    expect(dataTypeDefaultData.editorUiAlias).toBe(label.editorUiAlias);
    expect(await umbracoApi.dataType.doesDataTypeHaveValue(label.type, 'umbracoDataValueType')).toBeFalsy();
  });
}
