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
		id: 2,
		key: '725a26c4-158d-4dc0-8aaa-b64473b11aa8',
		type: 'member',
		parentKey: '06c6919c-6fa7-4aa5-8214-0582c721c472',
		name: 'Member 3',
		hasChildren: false,
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
