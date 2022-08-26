import { UmbData } from './data';

export interface Entity {
	id: number;
	key: string;
	name: string;
	icon?: string; // TODO: Should this be here?
	type: string;
	hasChildren: boolean; // TODO: Should this be here?
	parentKey: string;
}

export const data: Array<Entity> = [
	{
		id: 1,
		key: '865a11f9-d140-4f21-8dfe-2caafc65a971',
		type: 'member',
		parentKey: '24fcd88a-d1bb-423b-b794-8a94dcddcb6a',
		name: 'Member 1',
		hasChildren: false,
	},
	{
		id: 2,
		key: '06c6919c-6fa7-4aa5-8214-0582c721c472',
		type: 'member',
		parentKey: '24fcd88a-d1bb-423b-b794-8a94dcddcb6a',
		name: 'Member 2',
		hasChildren: true,
	},
	{
		id: 3,
		key: '725a26c4-158d-4dc0-8aaa-b64473b11aa8',
		type: 'member',
		parentKey: '06c6919c-6fa7-4aa5-8214-0582c721c472',
		name: 'Member 3',
		hasChildren: false,
	},
	{
		id: 4,
		key: '14be0f66-1472-452a-abde-9da6b4136073',
		parentKey: 'd46d144e-33d8-41e3-bf7a-545287e16e3c',
		type: 'memberGroup',
		name: 'Member Group 1',
		hasChildren: false,
	},
	{
		id: 5,
		key: '8d5cf29a-e73b-4bf5-ad56-8adf6cbf8766',
		parentKey: 'd46d144e-33d8-41e3-bf7a-545287e16e3c',
		type: 'memberGroup',
		name: 'Member Group 2',
		hasChildren: false,
	},
	{
		id: 1245,
		key: 'dt-1',
		parentKey: '3fd3eba5-c893-4d3c-af67-f574e6eded38',
		name: 'Text',
		hasChildren: false,
		type: 'dataType',
	},
	{
		id: 1244,
		key: 'dt-2',
		parentKey: '3fd3eba5-c893-4d3c-af67-f574e6eded38',
		name: 'Textarea',
		hasChildren: false,
		type: 'dataType',
	},
	{
		id: 1246,
		key: 'dt-3',
		parentKey: '3fd3eba5-c893-4d3c-af67-f574e6eded38',
		name: 'My JS Property Editor',
		hasChildren: false,
		type: 'dataType',
	},
	{
		id: 1247,
		key: 'dt-4',
		parentKey: '3fd3eba5-c893-4d3c-af67-f574e6eded38',
		name: 'Context Example',
		hasChildren: false,
		type: 'dataType',
	},
	{
		id: 1248,
		key: 'dt-5',
		parentKey: '3fd3eba5-c893-4d3c-af67-f574e6eded38',
		name: 'Content Picker (DataType)',
		hasChildren: false,
		type: 'dataType',
	},
];

// Temp mocked database
class UmbEntityData extends UmbData<Entity> {
	constructor() {
		super(data);
	}

	getChildren(key: string) {
		return data.filter((item) => item.parentKey === key);
	}
}

export const umbEntityData = new UmbEntityData();
