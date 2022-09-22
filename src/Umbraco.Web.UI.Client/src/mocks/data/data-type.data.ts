import { Entity } from './entities';
import { UmbEntityData } from './entity.data';

export interface DataTypeEntity extends Entity {
	type: 'dataType';
}

export interface DataTypeDetails extends DataTypeEntity {
	propertyEditorAlias: string | null;
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
		propertyEditorAlias: 'Umbraco.TextBox',
		propertyEditorUIAlias: 'Umb.PropertyEditorUI.Text',
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
		propertyEditorAlias: 'Umbraco.TextArea',
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
		propertyEditorAlias: 'Umbraco.Custom',
		propertyEditorUIAlias: 'My.PropertyEditorUI.Custom',
		data: [],
	},
	{
		key: 'dt-4',
		name: 'Context Example',
		type: 'dataType',
		parentKey: '29d78e6c-c1bf-4c15-b820-d511c237ffae',
		isTrashed: false,
		hasChildren: false,
		icon: 'umb:autofill',
		propertyEditorAlias: 'Umbraco.Custom',
		propertyEditorUIAlias: 'Umb.PropertyEditorUI.ContextExample',
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
		propertyEditorAlias: 'Umbraco.ContentPicker',
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
		propertyEditorAlias: '',
		propertyEditorUIAlias: '',
		data: [],
	},
];

// Temp mocked database
class UmbDataTypeData extends UmbEntityData<DataTypeDetails> {
	constructor() {
		super(data);
	}
}

export const umbDataTypeData = new UmbDataTypeData();
