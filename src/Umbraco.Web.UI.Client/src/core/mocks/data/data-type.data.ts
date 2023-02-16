import { UmbEntityData } from './entity.data';
import { createFolderTreeItem } from './utils';
import type { FolderTreeItemModel, DataTypeModel } from '@umbraco-cms/backend-api';

export const data: Array<DataTypeModel> = [
	{
		key: '0cc0eba1-9960-42c9-bf9b-60e150b429ae',
		parentKey: null,
		name: 'Textstring',
		propertyEditorAlias: 'Umbraco.TextBox',
		propertyEditorUiAlias: 'Umb.PropertyEditorUI.TextBox',
		data: [],
	},
	{
		name: 'Text',
		key: 'dt-textBox',
		parentKey: null,
		propertyEditorAlias: 'Umbraco.TextBox',
		propertyEditorUiAlias: 'Umb.PropertyEditorUI.TextBox',
		data: [
			{
				alias: 'maxChars',
				value: 10,
			},
		],
	},
	{
		name: 'Text Area',
		key: 'dt-textArea',
		parentKey: null,
		propertyEditorAlias: 'Umbraco.TextArea',
		propertyEditorUiAlias: 'Umb.PropertyEditorUI.TextArea',
		data: [],
	},
	{
		name: 'My JS Property Editor',
		key: 'dt-custom',
		parentKey: null,
		propertyEditorAlias: 'Umbraco.JSON',
		propertyEditorUiAlias: 'My.PropertyEditorUI.Custom',
		data: [],
	},
	{
		name: 'Color Picker',
		key: 'dt-colorPicker',
		parentKey: null,
		propertyEditorAlias: 'Umbraco.ColorPicker',
		propertyEditorUiAlias: 'Umb.PropertyEditorUI.ColorPicker',
		data: [
			{
				alias: 'includeLabels',
				value: false,
			},
			{
				alias: 'colors',
				value: ['#000000', '#373737', '#9e9e9e', '#607d8b', '#2196f3', '#03a9f4', '#3f51b5', '#9c27b0', '#673ab7'],
			},
		],
	},
	{
		name: 'Content Picker',
		key: 'dt-contentPicker',
		parentKey: null,
		propertyEditorAlias: 'Umbraco.ContentPicker',
		propertyEditorUiAlias: 'Umb.PropertyEditorUI.DocumentPicker',
		data: [
			{
				alias: 'validationLimit',
				value: { min: 2, max: 4 },
			},
		],
	},
	{
		name: 'Eye Dropper',
		key: 'dt-eyeDropper',
		parentKey: null,
		propertyEditorAlias: 'Umbraco.ColorPicker.EyeDropper',
		propertyEditorUiAlias: 'Umb.PropertyEditorUI.EyeDropper',
		data: [
			{
				alias: 'palette',
				value: [
					'#d0021b',
					'#f5a623',
					'#f8e71c',
					'#8b572a',
					'#7ed321',
					'#417505',
					'#bd10e0',
					'#9013fe',
					'#4a90e2',
					'#50e3c2',
					'#b8e986',
					'#000',
					'#444',
					'#888',
					'#ccc',
					'#fff',
				],
			},
			{
				alias: 'showAlpha',
				value: false,
			},
		],
	},
	{
		name: 'Multi URL Picker',
		key: 'dt-multiUrlPicker',
		parentKey: null,
		propertyEditorAlias: 'Umbraco.MultiUrlPicker',
		propertyEditorUiAlias: 'Umb.PropertyEditorUI.MultiUrlPicker',
		data: [],
	},
	{
		name: 'Multi Node Tree Picker',
		key: 'dt-multiNodeTreePicker',
		parentKey: null,
		propertyEditorAlias: 'Umbraco.MultiNodeTreePicker',
		propertyEditorUiAlias: 'Umb.PropertyEditorUI.TreePicker',
		data: [],
	},
	{
		name: 'Date Picker',
		key: 'dt-datePicker',
		parentKey: null,
		propertyEditorAlias: 'Umbraco.DateTime',
		propertyEditorUiAlias: 'Umb.PropertyEditorUI.DatePicker',
		data: [],
	},
	{
		name: 'Email',
		key: 'dt-email',
		parentKey: null,
		propertyEditorAlias: 'Umbraco.EmailAddress',
		propertyEditorUiAlias: 'Umb.PropertyEditorUI.Email',
		data: [],
	},
	{
		name: 'Multiple Text String',
		key: 'dt-multipleTextString',
		parentKey: null,
		propertyEditorAlias: 'Umbraco.MultipleTextString',
		propertyEditorUiAlias: 'Umb.PropertyEditorUI.MultipleTextString',
		data: [
			{
				alias: 'minNumber',
				value: 2,
			},
			{
				alias: 'maxNumber',
				value: 4,
			},
		],
	},
	{
		name: 'Dropdown',
		key: 'dt-dropdown',
		parentKey: null,
		propertyEditorAlias: 'Umbraco.DropDown.Flexible',
		propertyEditorUiAlias: 'Umb.PropertyEditorUI.Dropdown',
		data: [],
	},
	{
		name: 'Slider',
		key: 'dt-slider',
		parentKey: null,
		propertyEditorAlias: 'Umbraco.Slider',
		propertyEditorUiAlias: 'Umb.PropertyEditorUI.Slider',
		data: [],
	},
	{
		name: 'Toggle',
		key: 'dt-toggle',
		parentKey: null,
		propertyEditorAlias: 'Umbraco.TrueFalse',
		propertyEditorUiAlias: 'Umb.PropertyEditorUI.Toggle',
		data: [],
	},
	{
		name: 'Tags',
		key: 'dt-tags',
		parentKey: null,
		propertyEditorAlias: 'Umbraco.Tags',
		propertyEditorUiAlias: 'Umb.PropertyEditorUI.Tags',
		data: [],
	},
	{
		name: 'Markdown Editor',
		key: 'dt-markdownEditor',
		parentKey: null,
		propertyEditorAlias: 'Umbraco.MarkdownEditor',
		propertyEditorUiAlias: 'Umb.PropertyEditorUI.MarkdownEditor',
		data: [],
	},
	{
		name: 'Radio Button List',
		key: 'dt-radioButtonList',
		parentKey: null,
		propertyEditorAlias: 'Umbraco.RadioButtonList',
		propertyEditorUiAlias: 'Umb.PropertyEditorUI.RadioButtonList',
		data: [
			{
				alias: 'items',
				value: {
					0: { sortOrder: 1, value: 'First Option' },
					1: { sortOrder: 2, value: 'Second Option' },
					2: { sortOrder: 3, value: 'I Am the third Option' },
				},
			},
		],
	},
	{
		name: 'Checkbox List',
		key: 'dt-checkboxList',
		parentKey: null,
		propertyEditorAlias: 'Umbraco.CheckboxList',
		propertyEditorUiAlias: 'Umb.PropertyEditorUI.CheckboxList',
		data: [
			{
				alias: 'items',
				value: {
					0: { sortOrder: 1, value: 'First Option' },
					1: { sortOrder: 2, value: 'Second Option' },
					2: { sortOrder: 3, value: 'I Am the third Option' },
				},
			},
		],
	},
	{
		name: 'Block List',
		key: 'dt-blockList',
		parentKey: null,
		propertyEditorAlias: 'Umbraco.BlockList',
		propertyEditorUiAlias: 'Umb.PropertyEditorUI.BlockList',
		data: [],
	},
	{
		name: 'Media Picker',
		key: 'dt-mediaPicker',
		parentKey: null,
		propertyEditorAlias: 'Umbraco.MediaPicker3',
		propertyEditorUiAlias: 'Umb.PropertyEditorUI.MediaPicker',
		data: [],
	},
	{
		name: 'Image Cropper',
		key: 'dt-imageCropper',
		parentKey: null,
		propertyEditorAlias: 'Umbraco.ImageCropper',
		propertyEditorUiAlias: 'Umb.PropertyEditorUI.ImageCropper',
		data: [],
	},
	{
		name: 'Upload Field',
		key: 'dt-uploadField',
		parentKey: null,
		propertyEditorAlias: 'Umbraco.UploadField',
		propertyEditorUiAlias: 'Umb.PropertyEditorUI.UploadField',
		data: [],
	},
	{
		name: 'Block Grid',
		key: 'dt-blockGrid',
		parentKey: null,
		propertyEditorAlias: 'Umbraco.BlockGrid',
		propertyEditorUiAlias: 'Umb.PropertyEditorUI.BlockGrid',
		data: [],
	},
	{
		name: 'Collection View',
		key: 'dt-collectionView',
		parentKey: null,
		propertyEditorAlias: 'Umbraco.ListView',
		propertyEditorUiAlias: 'Umb.PropertyEditorUI.CollectionView',
		data: [],
	},
	{
		name: 'Icon Picker',
		key: 'dt-iconPicker',
		parentKey: null,
		propertyEditorAlias: 'Umbraco.IconPicker',
		propertyEditorUiAlias: 'Umb.PropertyEditorUI.IconPicker',
		data: [],
	},
	{
		name: 'Number Range',
		key: 'dt-numberRange',
		parentKey: null,
		propertyEditorAlias: 'Umbraco.JSON',
		propertyEditorUiAlias: 'Umb.PropertyEditorUI.NumberRange',
		data: [],
	},
	{
		name: 'Order Direction',
		key: 'dt-orderDirection',
		parentKey: null,
		propertyEditorAlias: 'Umbraco.JSON',
		propertyEditorUiAlias: 'Umb.PropertyEditorUI.OrderDirection',
		data: [],
	},
	{
		name: 'Overlay Size',
		key: 'dt-overlaySize',
		parentKey: null,
		propertyEditorAlias: 'Umbraco.JSON',
		propertyEditorUiAlias: 'Umb.PropertyEditorUI.OverlaySize',
		data: [],
	},
	{
		name: 'Rich Text Editor',
		key: 'dt-richTextEditor',
		parentKey: null,
		propertyEditorAlias: 'Umbraco.TinyMCE',
		propertyEditorUiAlias: 'Umb.PropertyEditorUI.TinyMCE',
		data: [],
	},
	{
		name: 'Label',
		key: 'dt-label',
		parentKey: null,
		propertyEditorAlias: 'Umbraco.Label',
		propertyEditorUiAlias: 'Umb.PropertyEditorUI.Label',
		data: [],
	},
	{
		name: 'Integer',
		key: 'dt-integer',
		parentKey: null,
		propertyEditorAlias: 'Umbraco.Integer',
		propertyEditorUiAlias: 'Umb.PropertyEditorUI.Integer',
		data: [],
	},
	{
		name: 'Decimal',
		key: 'dt-decimal',
		parentKey: null,
		propertyEditorAlias: 'Umbraco.Decimal',
		propertyEditorUiAlias: 'Umb.PropertyEditorUI.Decimal',
		data: [],
	},
	{
		name: 'User Picker',
		key: 'dt-userPicker',
		parentKey: null,
		propertyEditorAlias: 'Umbraco.UserPicker',
		propertyEditorUiAlias: 'Umb.PropertyEditorUI.UserPicker',
		data: [],
	},
	{
		name: 'Member Picker',
		key: 'dt-memberPicker',
		parentKey: null,
		propertyEditorAlias: 'Umbraco.MemberPicker',
		propertyEditorUiAlias: 'Umb.PropertyEditorUI.MemberPicker',
		data: [],
	},
	{
		name: 'Member Group Picker',
		key: 'dt-memberGroupPicker',
		parentKey: null,
		propertyEditorAlias: 'Umbraco.MemberGroupPicker',
		propertyEditorUiAlias: 'Umb.PropertyEditorUI.MemberGroupPicker',
		data: [],
	},
];

// Temp mocked database
// TODO: all properties are optional in the server schema. I don't think this is correct.
// eslint-disable-next-line @typescript-eslint/ban-ts-comment
// @ts-ignore
class UmbDataTypeData extends UmbEntityData<DataTypeModel> {
	constructor() {
		super(data);
	}

	getTreeRoot(): Array<FolderTreeItemModel> {
		const rootItems = this.data.filter((item) => item.parentKey === null);
		return rootItems.map((item) => createFolderTreeItem(item));
	}

	getTreeItemChildren(key: string): Array<FolderTreeItemModel> {
		const childItems = this.data.filter((item) => item.parentKey === key);
		return childItems.map((item) => createFolderTreeItem(item));
	}

	getTreeItem(keys: Array<string>): Array<FolderTreeItemModel> {
		const items = this.data.filter((item) => keys.includes(item.key ?? ''));
		return items.map((item) => createFolderTreeItem(item));
	}
}

export const umbDataTypeData = new UmbDataTypeData();
