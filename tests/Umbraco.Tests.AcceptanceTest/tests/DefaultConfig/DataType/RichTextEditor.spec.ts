import {test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

const dataTypeName = 'Richtext editor';

test('titap is the default plugins in rich text editor', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const tiptapPropertyEditorName = 'Rich Text Editor [Tiptap] Property Editor UI';
  const tiptapAlias = 'Umbraco.RichText';
  const tiptapUiAlias = 'Umb.PropertyEditorUi.Tiptap';
  await umbracoUi.goToBackOffice();
  await umbracoUi.dataType.goToSettingsTreeItem('Data Types');

  // Act
  await umbracoUi.dataType.goToDataType(dataTypeName);

  // Assert
  await umbracoUi.dataType.doesPropertyEditorHaveName(tiptapPropertyEditorName);
  await umbracoUi.dataType.doesPropertyEditorHaveSchemaAlias(tiptapAlias);
  await umbracoUi.dataType.doesPropertyEditorHaveAlias(tiptapUiAlias);
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  expect(dataTypeData.editorAlias).toBe(tiptapAlias);
  expect(dataTypeData.editorUiAlias).toBe(tiptapUiAlias);
});