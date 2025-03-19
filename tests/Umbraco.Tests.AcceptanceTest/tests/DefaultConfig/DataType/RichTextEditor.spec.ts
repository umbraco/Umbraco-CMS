import {ConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

const dataTypeName = 'Richtext editor';
const tipTapPropertyEditorName = 'Rich Text Editor [Tiptap] Property Editor UI';
const tipTapAlias = 'Umbraco.RichText';
const tipTapUiAlias = 'Umb.PropertyEditorUi.Tiptap';
const extensionsDefaultValue = [
  "Umb.Tiptap.Embed",
  "Umb.Tiptap.Link",
  "Umb.Tiptap.Figure",
  "Umb.Tiptap.Image",
  "Umb.Tiptap.Subscript",
  "Umb.Tiptap.Superscript",
  "Umb.Tiptap.Table",
  "Umb.Tiptap.Underline",
  "Umb.Tiptap.TextAlign",
  "Umb.Tiptap.MediaUpload"
];

const toolbarDefaultValue = [
  [
    [
      "Umb.Tiptap.Toolbar.SourceEditor"
    ],
    [
      "Umb.Tiptap.Toolbar.Bold",
      "Umb.Tiptap.Toolbar.Italic",
      "Umb.Tiptap.Toolbar.Underline"
    ],
    [
      "Umb.Tiptap.Toolbar.TextAlignLeft",
      "Umb.Tiptap.Toolbar.TextAlignCenter",
      "Umb.Tiptap.Toolbar.TextAlignRight"
    ],
    [
      "Umb.Tiptap.Toolbar.BulletList",
      "Umb.Tiptap.Toolbar.OrderedList"
    ],
    [
      "Umb.Tiptap.Toolbar.Blockquote",
      "Umb.Tiptap.Toolbar.HorizontalRule"
    ],
    [
      "Umb.Tiptap.Toolbar.Link",
      "Umb.Tiptap.Toolbar.Unlink"
    ],
    [
      "Umb.Tiptap.Toolbar.MediaPicker",
      "Umb.Tiptap.Toolbar.EmbeddedMedia"
    ]
  ]
];
test('tiptap is the default property editor in rich text editor', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoUi.goToBackOffice();
  await umbracoUi.dataType.goToSettingsTreeItem('Data Types');

  // Act
  await umbracoUi.dataType.goToDataType(dataTypeName);

  // Assert  
  await umbracoUi.dataType.doesSettingHaveValue(ConstantHelper.tipTapSettings);
  await umbracoUi.dataType.doesSettingItemsHaveCount(ConstantHelper.tipTapSettings);
  await umbracoUi.dataType.doesPropertyEditorHaveName(tipTapPropertyEditorName);
  await umbracoUi.dataType.doesPropertyEditorHaveAlias(tipTapAlias);
  await umbracoUi.dataType.doesPropertyEditorHaveUiAlias(tipTapUiAlias);
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  expect(dataTypeData.editorAlias).toBe(tipTapAlias);
  expect(dataTypeData.editorUiAlias).toBe(tipTapUiAlias);
  expect(await umbracoApi.dataType.doesDataTypeHaveValue(dataTypeName, 'maxImageSize', 500)).toBeTruthy();
  expect(await umbracoApi.dataType.doesDataTypeHaveValue(dataTypeName, 'overlaySize', 'medium')).toBeTruthy();
  expect(await umbracoApi.dataType.doesTiptapExtensionsItemsMatchCount(dataTypeName, extensionsDefaultValue.length)).toBeTruthy();
  expect(await umbracoApi.dataType.doesTiptapExtensionsHaveItems(dataTypeName, extensionsDefaultValue)).toBeTruthy();
  expect(await umbracoApi.dataType.doesTiptapToolbarHaveItems(dataTypeName, toolbarDefaultValue)).toBeTruthy();
});