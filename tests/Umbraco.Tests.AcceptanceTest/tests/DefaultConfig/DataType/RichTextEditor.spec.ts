import {test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

test('tiptap is the default property editor in rich text editor', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const dataTypeName = 'Richtext editor';
  const tipTapPropertyEditorName = 'Rich Text Editor [Tiptap] Property Editor UI';
  const tipTapAlias = 'Umbraco.RichText';
  const tipTapUiAlias = 'Umb.PropertyEditorUi.Tiptap';
  await umbracoUi.goToBackOffice();
  await umbracoUi.dataType.goToSettingsTreeItem('Data Types');

  // Act
  await umbracoUi.dataType.goToDataType(dataTypeName);

  // Assert
  await umbracoUi.dataType.doesPropertyEditorHaveName(tipTapPropertyEditorName);
  await umbracoUi.dataType.doesPropertyEditorHaveSchemaAlias(tipTapAlias);
  await umbracoUi.dataType.doesPropertyEditorHaveAlias(tipTapUiAlias);
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  expect(dataTypeData.editorAlias).toBe(tipTapAlias);
  expect(dataTypeData.editorUiAlias).toBe(tipTapUiAlias);
});