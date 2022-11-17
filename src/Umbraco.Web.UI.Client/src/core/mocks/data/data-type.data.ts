import { UmbEntityData } from './entity.data';
import { FolderTreeItem } from '@umbraco-cms/backend-api';

export interface DataTypeDetails extends FolderTreeItem {
	key: string; // TODO: remove when server is fixed
	isTrashed: boolean; // TODO: remove only temp part of refactor
	propertyEditorModelAlias: string | null;
	propertyEditorUIAlias: string | null;
	data: Array<DataTypePropertyData>;
}

export interface DataTypePropertyData {
	alias: string;
	value: any;
}

export const data: Array<DataTypeDetails> = [
	{
		key: 'dt-1',
		name: 'Text',
		type: 'dataType',
		parentKey: '29d78e6c-c1bf-4c15-b820-d511c237ffae',
		isTrashed: false,
		hasChildren: false,
		icon: 'umb:autofill',
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
		key: 'dt-2',
		name: 'Textarea',
		type: 'dataType',
		parentKey: '29d78e6c-c1bf-4c15-b820-d511c237ffae',
		isTrashed: false,
		hasChildren: false,
		icon: 'umb:autofill',
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
		key: 'dt-3',
		name: 'My JS Property Editor',
		type: 'dataType',
		parentKey: '29d78e6c-c1bf-4c15-b820-d511c237ffae',
		isTrashed: false,
		hasChildren: false,
		icon: 'umb:autofill',
		propertyEditorModelAlias: 'Umbraco.Custom',
		propertyEditorUIAlias: 'My.PropertyEditorUI.Custom',
		data: [],
	},
	{
		key: 'dt-5',
		name: 'Content Picker (DataType)',
		type: 'dataType',
		parentKey: '29d78e6c-c1bf-4c15-b820-d511c237ffae',
		isTrashed: false,
		hasChildren: false,
		icon: 'umb:autofill',
		propertyEditorModelAlias: 'Umbraco.ContentPicker',
		propertyEditorUIAlias: 'Umb.PropertyEditorUI.ContentPicker',
		data: [],
	},
	{
		key: 'dt-6',
		name: 'Empty',
		type: 'dataType',
		parentKey: '29d78e6c-c1bf-4c15-b820-d511c237ffae',
		isTrashed: false,
		hasChildren: false,
		icon: 'umb:autofill',
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
