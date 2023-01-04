import { UmbEntityData } from './entity.data';
import { createFolderTreeItem } from './utils';
import { FolderTreeItem } from '@umbraco-cms/backend-api';
import type { DataTypeDetails } from '@umbraco-cms/models';

export const data: Array<DataTypeDetails> = [
	{
		name: 'Text',
		type: 'data-type',
		icon: 'umb:autofill',
		hasChildren: false,
		key: 'dt-1',
		isContainer: false,
		parentKey: null,
		isFolder: false,
		propertyEditorModelAlias: 'Umbraco.TextBox',
		propertyEditorUIAlias: 'Umb.PropertyEditorUI.TextBox',
		data: [
			{
				alias: 'maxChars',
				value: 10,
			},
		],
	},
	{
		name: 'Textarea',
		type: 'data-type',
		icon: 'umb:autofill',
		hasChildren: false,
		key: 'dt-2',
		isContainer: false,
		parentKey: null,
		isFolder: false,
		propertyEditorModelAlias: 'Umbraco.TextArea',
		propertyEditorUIAlias: 'Umb.PropertyEditorUI.Textarea',
		data: [
			{
				alias: 'maxChars',
				value: 500,
			},
			{
				alias: 'rows',
				value: 25,
			},
		],
	},
	{
		name: 'My JS Property Editor',
		type: 'data-type',
		icon: 'umb:autofill',
		hasChildren: false,
		key: 'dt-3',
		isContainer: false,
		parentKey: null,
		isFolder: false,
		propertyEditorModelAlias: 'Umbraco.JSON',
		propertyEditorUIAlias: 'My.PropertyEditorUI.Custom',
		data: [],
	},
	{
		name: 'Content Picker (DataType)',
		type: 'data-type',
		icon: 'umb:autofill',
		hasChildren: false,
		key: 'dt-5',
		isContainer: false,
		parentKey: null,
		isFolder: false,
		propertyEditorModelAlias: 'Umbraco.ContentPicker',
		propertyEditorUIAlias: 'Umb.PropertyEditorUI.ContentPicker',
		data: [],
	},
	{
		name: 'Empty',
		type: 'data-type',
		icon: 'umb:autofill',
		hasChildren: false,
		key: 'dt-6',
		isContainer: false,
		parentKey: null,
		isFolder: false,
		propertyEditorModelAlias: '',
		propertyEditorUIAlias: '',
		data: [],
	},
	{
		name: 'Color Picker',
		type: 'data-type',
		icon: 'umb:autofill',
		hasChildren: false,
		key: 'dt-7',
		isContainer: false,
		parentKey: null,
		isFolder: false,
		propertyEditorModelAlias: 'Umbraco.ColorPicker',
		propertyEditorUIAlias: 'Umb.PropertyEditorUI.ColorPicker',
		data: [],
	},
	{
		name: 'Content Picker',
		type: 'data-type',
		icon: 'umb:autofill',
		hasChildren: false,
		key: 'dt-8',
		isContainer: false,
		parentKey: null,
		isFolder: false,
		propertyEditorModelAlias: 'Umbraco.ContentPicker',
		propertyEditorUIAlias: 'Umb.PropertyEditorUI.ContentPicker',
		data: [],
	},
	{
		name: 'Eye Dropper',
		type: 'data-type',
		icon: 'umb:autofill',
		hasChildren: false,
		key: 'dt-9',
		isContainer: false,
		parentKey: null,
		isFolder: false,
		propertyEditorModelAlias: 'Umbraco.ColorPicker.EyeDropper',
		propertyEditorUIAlias: 'Umb.PropertyEditorUI.EyeDropper',
		data: [],
	},
	{
		name: 'Multi URL Picker',
		type: 'data-type',
		icon: 'umb:autofill',
		hasChildren: false,
		key: 'dt-10',
		isContainer: false,
		parentKey: null,
		isFolder: false,
		propertyEditorModelAlias: 'Umbraco.MultiUrlPicker',
		propertyEditorUIAlias: 'Umb.PropertyEditorUI.MultiUrlPicker',
		data: [],
	},
	{
		name: 'Multi Node Tree Picker',
		type: 'data-type',
		icon: 'umb:autofill',
		hasChildren: false,
		key: 'dt-11',
		isContainer: false,
		parentKey: null,
		isFolder: false,
		propertyEditorModelAlias: 'Umbraco.MultiNodeTreePicker',
		propertyEditorUIAlias: 'Umb.PropertyEditorUI.TreePicker',
		data: [],
	},
	{
		name: 'Date Picker',
		type: 'data-type',
		icon: 'umb:autofill',
		hasChildren: false,
		key: 'dt-12',
		isContainer: false,
		parentKey: null,
		isFolder: false,
		propertyEditorModelAlias: 'Umbraco.DateTime',
		propertyEditorUIAlias: 'Umb.PropertyEditorUI.DatePicker',
		data: [],
	},
	{
		name: 'Email',
		type: 'data-type',
		icon: 'umb:autofill',
		hasChildren: false,
		key: 'dt-13',
		isContainer: false,
		parentKey: null,
		isFolder: false,
		propertyEditorModelAlias: 'Umbraco.EmailAddress',
		propertyEditorUIAlias: 'Umb.PropertyEditorUI.Email',
		data: [],
	},
	{
		name: 'Text Box',
		type: 'data-type',
		icon: 'umb:autofill',
		hasChildren: false,
		key: 'dt-14',
		isContainer: false,
		parentKey: null,
		isFolder: false,
		propertyEditorModelAlias: 'Umbraco.TextBox',
		propertyEditorUIAlias: 'Umb.PropertyEditorUI.TextBox',
		data: [],
	},
	{
		name: 'Multiple Text String',
		type: 'data-type',
		icon: 'umb:autofill',
		hasChildren: false,
		key: 'dt-15',
		isContainer: false,
		parentKey: null,
		isFolder: false,
		propertyEditorModelAlias: 'Umbraco.MultipleTextString',
		propertyEditorUIAlias: 'Umb.PropertyEditorUI.MultipleTextString',
		data: [],
	},
	{
		name: 'Dropdown',
		type: 'data-type',
		icon: 'umb:autofill',
		hasChildren: false,
		key: 'dt-16',
		isContainer: false,
		parentKey: null,
		isFolder: false,
		propertyEditorModelAlias: 'Umbraco.DropDown.Flexible',
		propertyEditorUIAlias: 'Umb.PropertyEditorUI.Dropdown',
		data: [],
	},
];

// Temp mocked database
// TODO: all properties are optional in the server schema. I don't think this is correct.
// eslint-disable-next-line @typescript-eslint/ban-ts-comment
// @ts-ignore
class UmbDataTypeData extends UmbEntityData<DataTypeDetails> {
	constructor() {
		super(data);
	}

	getTreeRoot(): Array<FolderTreeItem> {
		const rootItems = this.data.filter((item) => item.parentKey === null);
		return rootItems.map((item) => createFolderTreeItem(item));
	}

	getTreeItemChildren(key: string): Array<FolderTreeItem> {
		const childItems = this.data.filter((item) => item.parentKey === key);
		return childItems.map((item) => createFolderTreeItem(item));
	}

	getTreeItem(keys: Array<string>): Array<FolderTreeItem> {
		const items = this.data.filter((item) => keys.includes(item.key ?? ''));
		return items.map((item) => createFolderTreeItem(item));
	}
}

export const umbDataTypeData = new UmbDataTypeData();
