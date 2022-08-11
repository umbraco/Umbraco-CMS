import { UmbData } from './data';

export interface DataTypeEntity {
	id: number;
	key: string;
	name: string;
	//configUI: any; // this is the prevalues...
	propertyEditorUIAlias: string;
}

export const data: Array<DataTypeEntity> = [
	{
		id: 1245,
		key: 'dt-1',
		name: 'Text',
		propertyEditorUIAlias: 'Umb.PropertyEditorUI.Text',
	},
	{
		id: 1244,
		key: 'dt-2',
		name: 'Textarea',
		propertyEditorUIAlias: 'Umb.PropertyEditorUI.Textarea',
	},
	{
		id: 1246,
		key: 'dt-3',
		name: 'My JS Property Editor',
		propertyEditorUIAlias: 'My.PropertyEditorUI.Custom',
	},
	{
		id: 1247,
		key: 'dt-4',
		name: 'Context Example',
		propertyEditorUIAlias: 'Umb.PropertyEditorUI.ContextExample',
	},
	{
		id: 1248,
		key: 'dt-5',
		name: 'Content Picker (DataType)',
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
