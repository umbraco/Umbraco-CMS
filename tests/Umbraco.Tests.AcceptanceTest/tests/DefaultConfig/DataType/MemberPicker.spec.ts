import {test} from '@umbraco/acceptance-test-helpers';
import {expect} from "@playwright/test";

const dataTypeName = 'Member Picker';
const editorAlias = 'Umbraco.MemberPicker';
const editorUiAlias = 'Umb.PropertyEditorUi.MemberPicker';

test('the default configuration is correct', async ({umbracoApi, umbracoUi}) => {
  // Act
  await umbracoUi.goToBackOffice();
  await umbracoUi.dataType.goToSettingsTreeItem('Data Types');
  await umbracoUi.dataType.goToDataType(dataTypeName);

  // Assert
  await umbracoUi.dataType.doesSettingsContainText('There is no configuration for this property editor.');
  await umbracoUi.dataType.doesPropertyEditorHaveAlias(editorAlias);
  await umbracoUi.dataType.doesPropertyEditorHaveUiAlias(editorUiAlias);
  const dataTypeDefaultData = await umbracoApi.dataType.getByName(dataTypeName);
  await umbracoApi.dataType.doesDataTypeHaveEditors(dataTypeDefaultData, editorAlias, editorUiAlias);
  await umbracoApi.dataType.doesHaveValueCount(dataTypeDefaultData, 0);
});
