export interface Entity {
	id: number;
	key: string;
	name: string;
	icon: string; // TODO: Should this be here?
	type: string;
	hasChildren: boolean; // TODO: Should this be here?
}

export const data: Array<Entity> = [
	{
		id: 1,
		key: '74e4008a-ea4f-4793-b924-15e02fd380d1',
		name: 'Document 1',
		type: 'document',
		icon: 'document',
		hasChildren: false,
	},
	{
		id: 2,
		key: '74e4008a-ea4f-4793-b924-15e02fd380d2',
		name: 'Document 2',
		type: 'document',
		icon: 'favorite',
		hasChildren: false,
	},
	{
		id: 3,
		key: 'cdd30288-2d1c-41b4-89a9-61647b4a10d5',
		name: 'Document 3',
		type: 'document',
		icon: 'document',
		hasChildren: false,
	},
	{
		id: 2001,
		key: 'f2f81a40-c989-4b6b-84e2-057cecd3adc1',
		name: 'Media 1',
		type: 'media',
		icon: 'picture',
		hasChildren: false,
	},
];
