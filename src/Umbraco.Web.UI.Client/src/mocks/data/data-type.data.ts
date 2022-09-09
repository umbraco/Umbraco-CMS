import { UmbData } from './data';
import { Entity } from './entities';

export interface DataTypeEntity extends Entity {
	propertyEditorUIAlias: string;
	//configUI: any; // this is the prevalues...
}

export const data: Array<DataTypeEntity> = [
	{
		key: 'dt-1',
		name: 'Text',
		type: 'dataType',
		parentKey: '29d78e6c-c1bf-4c15-b820-d511c237ffae',
		isTrashed: false,
		hasChildren: false,
		icon: '',
		propertyEditorUIAlias: 'Umb.PropertyEditorUI.Text',
	},
	{
		key: 'dt-2',
		name: 'Textarea',
		type: 'dataType',
		parentKey: '29d78e6c-c1bf-4c15-b820-d511c237ffae',
		isTrashed: false,
		hasChildren: false,
		icon: '',
		propertyEditorUIAlias: 'Umb.PropertyEditorUI.Textarea',
	},
	{
		key: 'dt-3',
		name: 'My JS Property Editor',
		type: 'dataType',
		parentKey: '29d78e6c-c1bf-4c15-b820-d511c237ffae',
		isTrashed: false,
		hasChildren: false,
		icon: '',
		propertyEditorUIAlias: 'My.PropertyEditorUI.Custom',
	},
	{
		key: 'dt-4',
		name: 'Context Example',
		type: 'dataType',
		parentKey: '29d78e6c-c1bf-4c15-b820-d511c237ffae',
		isTrashed: false,
		hasChildren: false,
		icon: '',
		propertyEditorUIAlias: 'Umb.PropertyEditorUI.ContextExample',
	},
	{
		key: 'dt-5',
		name: 'Content Picker (DataType)',
		type: 'dataType',
		parentKey: '29d78e6c-c1bf-4c15-b820-d511c237ffae',
		isTrashed: false,
		hasChildren: false,
		icon: '',
		propertyEditorUIAlias: 'Umb.PropertyEditorUI.ContentPicker',
	},
];

// Temp mocked database
class UmbDataTypeData extends UmbData<DataTypeEntity> {
	constructor() {
		super(data);
	}
}

export const umbDataTypeData = new UmbDataTypeData();
