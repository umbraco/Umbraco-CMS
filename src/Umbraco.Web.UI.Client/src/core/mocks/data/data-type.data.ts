import { UmbEntityData } from './entity.data';
import { createFolderTreeItem } from './utils';
import type { FolderTreeItemResponseModel, DataTypeResponseModel } from '@umbraco-cms/backoffice/backend-api';

// TODO: investigate why we don't get an entity type as part of the DataTypeModel
export const data: Array<DataTypeResponseModel & { type: 'data-type' }> = [
	{
		$type: 'data-type',
		type: 'data-type',
		key: '0cc0eba1-9960-42c9-bf9b-60e150b429ae',
		parentKey: null,
		name: 'Textstring',
		propertyEditorAlias: 'Umbraco.TextBox',
		propertyEditorUiAlias: 'Umb.PropertyEditorUI.TextBox',
		values: [],
	},
	{
		$type: 'data-type',
		type: 'data-type',
		name: 'Text',
		key: 'dt-textBox',
		parentKey: null,
		propertyEditorAlias: 'Umbraco.TextBox',
		propertyEditorUiAlias: 'Umb.PropertyEditorUI.TextBox',
		values: [
			{
				alias: 'maxChars',
				value: 10,
			},
		],
	},
	{
		$type: 'data-type',
		type: 'data-type',
		name: 'Text Area',
		key: 'dt-textArea',
		parentKey: null,
		propertyEditorAlias: 'Umbraco.TextArea',
		propertyEditorUiAlias: 'Umb.PropertyEditorUI.TextArea',
		values: [],
	},
	{
		$type: 'data-type',
		type: 'data-type',
		name: 'My JS Property Editor',
		key: 'dt-custom',
		parentKey: null,
		propertyEditorAlias: 'Umbraco.JSON',
		propertyEditorUiAlias: 'My.PropertyEditorUI.Custom',
		values: [],
	},
	{
		$type: 'data-type',
		type: 'data-type',
		name: 'Color Picker',
		key: 'dt-colorPicker',
		parentKey: null,
		propertyEditorAlias: 'Umbraco.ColorPicker',
		propertyEditorUiAlias: 'Umb.PropertyEditorUI.ColorPicker',
		values: [
			{
				alias: 'useLabel',
				value: true,
			},
			{
				alias: 'items',
				value: [
					{
						value: '#000000',
						label: 'Black',
					},
					{
						value: '#373737',
						label: 'Dark',
					},
					{
						value: '#9e9e9e',
						label: 'Light',
					},
					{
						value: '#607d8b',
						label: 'Sage',
					},
					{
						value: '#2196f3',
						label: 'Sapphire',
					},
					{
						value: '#03a9f4',
						label: 'Sky',
					},
					{
						value: '#3f51b5',
						label: 'Blue',
					},
					{
						value: '#9c27b0',
						label: 'Magenta',
					},
					{
						value: '#673ab7',
						label: 'Purps',
					},
				],
			},
		],
	},
	{
		$type: 'data-type',
		type: 'data-type',
		name: 'Content Picker',
		key: 'dt-contentPicker',
		parentKey: null,
		propertyEditorAlias: 'Umbraco.ContentPicker',
		propertyEditorUiAlias: 'Umb.PropertyEditorUI.DocumentPicker',
		values: [
			{
				alias: 'validationLimit',
				value: { min: 2, max: 4 },
			},
		],
	},
	{
		$type: 'data-type',
		type: 'data-type',
		name: 'Eye Dropper',
		key: 'dt-eyeDropper',
		parentKey: null,
		propertyEditorAlias: 'Umbraco.ColorPicker.EyeDropper',
		propertyEditorUiAlias: 'Umb.PropertyEditorUI.EyeDropper',
		values: [
			{
				//showPalette
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
		$type: 'data-type',
		type: 'data-type',
		name: 'Multi URL Picker',
		key: 'dt-multiUrlPicker',
		parentKey: null,
		propertyEditorAlias: 'Umbraco.MultiUrlPicker',
		propertyEditorUiAlias: 'Umb.PropertyEditorUI.MultiUrlPicker',
		values: [
			{
				alias: 'overlaySize',
				value: 'small',
			},
			{
				alias: 'hideAnchor',
				value: false,
			},
			{
				alias: 'ignoreUserStartNodes',
				value: false,
			},
			{
				alias: 'maxNumber',
				value: 2,
			},
			{
				alias: 'minNumber',
				value: 0,
			},
		],
	},
	{
		$type: 'data-type',
		type: 'data-type',
		name: 'Multi Node Tree Picker',
		key: 'dt-multiNodeTreePicker',
		parentKey: null,
		propertyEditorAlias: 'Umbraco.MultiNodeTreePicker',
		propertyEditorUiAlias: 'Umb.PropertyEditorUI.TreePicker',
		values: [],
	},
	{
		$type: 'data-type',
		type: 'data-type',
		name: 'Date Picker',
		key: 'dt-datePicker',
		parentKey: null,
		propertyEditorAlias: 'Umbraco.DateTime',
		propertyEditorUiAlias: 'Umb.PropertyEditorUI.DatePicker',
		values: [
			{
				alias: 'format',
				value: 'YYYY-MM-DD',
			},
			{
				alias: 'offsetTime',
				value: true,
			},
		],
	},
	{
		$type: 'data-type',
		name: 'Date Picker With Time',
		type: 'data-type',
		key: 'dt-datePicker-time',
		parentKey: null,
		propertyEditorAlias: 'Umbraco.DateTime',
		propertyEditorUiAlias: 'Umb.PropertyEditorUI.DatePicker',
		values: [
			{
				alias: 'format',
				value: 'YYYY-MM-DD HH:mm:ss',
			},
			{
				alias: 'offsetTime',
				value: true,
			},
		],
	},
	{
		$type: 'data-type',
		name: 'Time',
		type: 'data-type',
		key: 'dt-time',
		parentKey: null,
		propertyEditorAlias: 'Umbraco.DateTime',
		propertyEditorUiAlias: 'Umb.PropertyEditorUI.DatePicker',
		values: [
			{
				alias: 'format',
				value: 'HH:mm:ss',
			},
			{
				alias: 'offsetTime',
				value: false,
			},
		],
	},
	{
		$type: 'data-type',
		type: 'data-type',
		name: 'Email',
		key: 'dt-email',
		parentKey: null,
		propertyEditorAlias: 'Umbraco.EmailAddress',
		propertyEditorUiAlias: 'Umb.PropertyEditorUI.Email',
		values: [],
	},
	{
		$type: 'data-type',
		type: 'data-type',
		name: 'Multiple Text String',
		key: 'dt-multipleTextString',
		parentKey: null,
		propertyEditorAlias: 'Umbraco.MultipleTextString',
		propertyEditorUiAlias: 'Umb.PropertyEditorUI.MultipleTextString',
		values: [
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
		$type: 'data-type',
		type: 'data-type',
		name: 'Dropdown',
		key: 'dt-dropdown',
		parentKey: null,
		propertyEditorAlias: 'Umbraco.DropDown.Flexible',
		propertyEditorUiAlias: 'Umb.PropertyEditorUI.Dropdown',
		values: [],
	},
	{
		$type: 'data-type',
		type: 'data-type',
		name: 'Slider',
		key: 'dt-slider',
		parentKey: null,
		propertyEditorAlias: 'Umbraco.Slider',
		propertyEditorUiAlias: 'Umb.PropertyEditorUI.Slider',
		values: [
			{
				alias: 'enableRange',
				value: true,
			},
			{
				alias: 'initVal1',
				value: 10,
			},
			{
				alias: 'initVal2',
				value: 40,
			},
			{
				alias: 'maxVal',
				value: 50,
			},
			{
				alias: 'minVal',
				value: 0,
			},
			{
				alias: 'step',
				value: 10,
			},
		],
	},
	{
		$type: 'data-type',
		type: 'data-type',
		name: 'Toggle',
		key: 'dt-toggle',
		parentKey: null,
		propertyEditorAlias: 'Umbraco.TrueFalse',
		propertyEditorUiAlias: 'Umb.PropertyEditorUI.Toggle',
		values: [
			{
				alias: 'default',
				value: false,
			},
			{
				alias: 'labelOff',
				value: 'Not activated',
			},
			{
				alias: 'labelOn',
				value: 'Activated',
			},
			{
				alias: 'showLabels',
				value: true,
			},
		],
	},
	{
		$type: 'data-type',
		type: 'data-type',
		name: 'Tags',
		key: 'dt-tags',
		parentKey: null,
		propertyEditorAlias: 'Umbraco.Tags',
		propertyEditorUiAlias: 'Umb.PropertyEditorUI.Tags',
		values: [],
	},
	{
		$type: 'data-type',
		type: 'data-type',
		name: 'Markdown Editor',
		key: 'dt-markdownEditor',
		parentKey: null,
		propertyEditorAlias: 'Umbraco.MarkdownEditor',
		propertyEditorUiAlias: 'Umb.PropertyEditorUI.MarkdownEditor',
		values: [],
	},
	{
		$type: 'data-type',
		type: 'data-type',
		name: 'Radio Button List',
		key: 'dt-radioButtonList',
		parentKey: null,
		propertyEditorAlias: 'Umbraco.RadioButtonList',
		propertyEditorUiAlias: 'Umb.PropertyEditorUI.RadioButtonList',
		values: [
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
		$type: 'data-type',
		type: 'data-type',
		name: 'Checkbox List',
		key: 'dt-checkboxList',
		parentKey: null,
		propertyEditorAlias: 'Umbraco.CheckboxList',
		propertyEditorUiAlias: 'Umb.PropertyEditorUI.CheckboxList',
		values: [
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
		$type: 'data-type',
		type: 'data-type',
		name: 'Block List',
		key: 'dt-blockList',
		parentKey: null,
		propertyEditorAlias: 'Umbraco.BlockList',
		propertyEditorUiAlias: 'Umb.PropertyEditorUI.BlockList',
		values: [],
	},
	{
		$type: 'data-type',
		type: 'data-type',
		name: 'Media Picker',
		key: 'dt-mediaPicker',
		parentKey: null,
		propertyEditorAlias: 'Umbraco.MediaPicker3',
		propertyEditorUiAlias: 'Umb.PropertyEditorUI.MediaPicker',
		values: [],
	},
	{
		$type: 'data-type',
		type: 'data-type',
		name: 'Image Cropper',
		key: 'dt-imageCropper',
		parentKey: null,
		propertyEditorAlias: 'Umbraco.ImageCropper',
		propertyEditorUiAlias: 'Umb.PropertyEditorUI.ImageCropper',
		values: [],
	},
	{
		$type: 'data-type',
		type: 'data-type',
		name: 'Upload Field',
		key: 'dt-uploadField',
		parentKey: null,
		propertyEditorAlias: 'Umbraco.UploadField',
		propertyEditorUiAlias: 'Umb.PropertyEditorUI.UploadField',
		values: [],
	},
	{
		$type: 'data-type',
		type: 'data-type',
		name: 'Block Grid',
		key: 'dt-blockGrid',
		parentKey: null,
		propertyEditorAlias: 'Umbraco.BlockGrid',
		propertyEditorUiAlias: 'Umb.PropertyEditorUI.BlockGrid',
		values: [],
	},
	{
		$type: 'data-type',
		type: 'data-type',
		name: 'Collection View',
		key: 'dt-collectionView',
		parentKey: null,
		propertyEditorAlias: 'Umbraco.ListView',
		propertyEditorUiAlias: 'Umb.PropertyEditorUI.CollectionView',
		values: [],
	},
	{
		$type: 'data-type',
		type: 'data-type',
		name: 'Icon Picker',
		key: 'dt-iconPicker',
		parentKey: null,
		propertyEditorAlias: 'Umbraco.IconPicker',
		propertyEditorUiAlias: 'Umb.PropertyEditorUI.IconPicker',
		values: [],
	},
	{
		$type: 'data-type',
		type: 'data-type',
		name: 'Number Range',
		key: 'dt-numberRange',
		parentKey: null,
		propertyEditorAlias: 'Umbraco.JSON',
		propertyEditorUiAlias: 'Umb.PropertyEditorUI.NumberRange',
		values: [],
	},
	{
		$type: 'data-type',
		type: 'data-type',
		name: 'Order Direction',
		key: 'dt-orderDirection',
		parentKey: null,
		propertyEditorAlias: 'Umbraco.JSON',
		propertyEditorUiAlias: 'Umb.PropertyEditorUI.OrderDirection',
		values: [],
	},
	{
		$type: 'data-type',
		type: 'data-type',
		name: 'Overlay Size',
		key: 'dt-overlaySize',
		parentKey: null,
		propertyEditorAlias: 'Umbraco.JSON',
		propertyEditorUiAlias: 'Umb.PropertyEditorUI.OverlaySize',
		values: [],
	},
	{
		$type: 'data-type',
		type: 'data-type',
		name: 'Rich Text Editor',
		key: 'dt-richTextEditor',
		parentKey: null,
		propertyEditorAlias: 'Umbraco.TinyMCE',
		propertyEditorUiAlias: 'Umb.PropertyEditorUI.TinyMCE',
		values: [],
	},
	{
		$type: 'data-type',
		type: 'data-type',
		name: 'Label',
		key: 'dt-label',
		parentKey: null,
		propertyEditorAlias: 'Umbraco.Label',
		propertyEditorUiAlias: 'Umb.PropertyEditorUI.Label',
		values: [],
	},
	{
		$type: 'data-type',
		type: 'data-type',
		name: 'Integer',
		key: 'dt-integer',
		parentKey: null,
		propertyEditorAlias: 'Umbraco.Integer',
		propertyEditorUiAlias: 'Umb.PropertyEditorUI.Integer',
		values: [],
	},
	{
		$type: 'data-type',
		type: 'data-type',
		name: 'Decimal',
		key: 'dt-decimal',
		parentKey: null,
		propertyEditorAlias: 'Umbraco.Decimal',
		propertyEditorUiAlias: 'Umb.PropertyEditorUI.Decimal',
		values: [],
	},
	{
		$type: 'data-type',
		type: 'data-type',
		name: 'User Picker',
		key: 'dt-userPicker',
		parentKey: null,
		propertyEditorAlias: 'Umbraco.UserPicker',
		propertyEditorUiAlias: 'Umb.PropertyEditorUI.UserPicker',
		values: [],
	},
	{
		$type: 'data-type',
		type: 'data-type',
		name: 'Member Picker',
		key: 'dt-memberPicker',
		parentKey: null,
		propertyEditorAlias: 'Umbraco.MemberPicker',
		propertyEditorUiAlias: 'Umb.PropertyEditorUI.MemberPicker',
		values: [],
	},
	{
		$type: 'data-type',
		type: 'data-type',
		name: 'Member Group Picker',
		key: 'dt-memberGroupPicker',
		parentKey: null,
		propertyEditorAlias: 'Umbraco.MemberGroupPicker',
		propertyEditorUiAlias: 'Umb.PropertyEditorUI.MemberGroupPicker',
		values: [],
	},
];

// Temp mocked database
// TODO: all properties are optional in the server schema. I don't think this is correct.
// eslint-disable-next-line @typescript-eslint/ban-ts-comment
// @ts-ignore
class UmbDataTypeData extends UmbEntityData<DataTypeResponseModel> {
	constructor() {
		super(data);
	}

	getTreeRoot(): Array<FolderTreeItemResponseModel> {
		const rootItems = this.data.filter((item) => item.parentKey === null);
		return rootItems.map((item) => createFolderTreeItem(item));
	}

	getTreeItemChildren(key: string): Array<FolderTreeItemResponseModel> {
		const childItems = this.data.filter((item) => item.parentKey === key);
		return childItems.map((item) => createFolderTreeItem(item));
	}

	getTreeItem(keys: Array<string>): Array<FolderTreeItemResponseModel> {
		const items = this.data.filter((item) => keys.includes(item.key ?? ''));
		return items.map((item) => createFolderTreeItem(item));
	}
}

export const umbDataTypeData = new UmbDataTypeData();
