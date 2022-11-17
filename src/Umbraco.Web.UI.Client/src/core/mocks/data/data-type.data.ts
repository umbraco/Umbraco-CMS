import { UmbEntityData } from './entity.data';
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
		isTrashed: false,
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
		isTrashed: false,
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
		isTrashed: false,
		isFolder: false,
		propertyEditorModelAlias: 'Umbraco.Custom',
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
		isTrashed: false,
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
		isTrashed: false,
		isFolder: false,
		propertyEditorModelAlias: '',
		propertyEditorUIAlias: '',
		data: [],
	},
];

// Temp mocked database
class UmbDataTypeData extends UmbEntityData<DataTypeDetails> {
	constructor() {
		super(data);
	}

	getTreeRoot(): Array<FolderTreeItem> {
		return this.data.filter((item) => item.parentKey === null);
	}

	getTreeItemChildren(key: string): Array<FolderTreeItem> {
		return this.data.filter((item) => item.parentKey === key);
	}

	getTreeItem(keys: Array<string>): Array<FolderTreeItem> {
		return this.data.filter((item) => keys.includes(item.key ?? ''));
	}
}

export const umbDataTypeData = new UmbDataTypeData();
