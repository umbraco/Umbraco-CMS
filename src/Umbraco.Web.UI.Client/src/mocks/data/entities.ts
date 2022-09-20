export interface Entity {
	key: string;
	name: string;
	icon: string;
	type: string;
	hasChildren: boolean;
	parentKey: string;
	isTrashed: boolean;
}

const generateDocumentEntities = (amount: number) => {
	const entities = [];
	for (let i = 0; i < amount; i++) {
		entities.push({
			key: `document-${i}`,
			name: `Document ${i}`,
			type: 'document',
			icon: 'document',
			hasChildren: false,
			isTrashed: false,
			parentKey: 'ba23245c-d8c0-46f7-a2bc-7623743d6eba',
		});
	}
	return entities;
};

const documents = generateDocumentEntities(0);

export const entities: Array<Entity> = [
	{
		key: '865a11f9-d140-4f21-8dfe-2caafc65a971',
		type: 'member',
		parentKey: '8f974b62-392b-4ddd-908c-03c2e03ab1a6',
		name: 'Member 1',
		hasChildren: false,
		isTrashed: false,
		icon: 'umb:user',
	},
	{
		key: '06c6919c-6fa7-4aa5-8214-0582c721c472',
		type: 'member',
		parentKey: '8f974b62-392b-4ddd-908c-03c2e03ab1a6',
		name: 'Member 2',
		hasChildren: false,
		isTrashed: false,
		icon: 'umb:user',
	},
	{
		key: '725a26c4-158d-4dc0-8aaa-b64473b11aa8',
		type: 'member',
		parentKey: '8f974b62-392b-4ddd-908c-03c2e03ab1a6',
		name: 'Member 3',
		hasChildren: false,
		isTrashed: false,
		icon: 'umb:user',
	},
	{
		key: '14be0f66-1472-452a-abde-9da6b4136073',
		parentKey: '575645a5-0f25-4671-b9a0-be515096ad6b',
		type: 'memberGroup',
		name: 'Member Group 1',
		hasChildren: false,
		isTrashed: false,
		icon: 'umb:users-alt',
	},
	{
		key: '8d5cf29a-e73b-4bf5-ad56-8adf6cbf8766',
		parentKey: '575645a5-0f25-4671-b9a0-be515096ad6b',
		type: 'memberGroup',
		name: 'Member Group 2',
		hasChildren: false,
		isTrashed: false,
		icon: 'umb:users-alt',
	},
	{
		key: 'dt-1',
		parentKey: '29d78e6c-c1bf-4c15-b820-d511c237ffae',
		name: 'Text',
		hasChildren: false,
		isTrashed: false,
		type: 'dataType',
		icon: 'umb:autofill',
	},
	{
		key: 'dt-2',
		parentKey: '29d78e6c-c1bf-4c15-b820-d511c237ffae',
		name: 'Textarea',
		hasChildren: false,
		isTrashed: false,
		type: 'dataType',
		icon: 'umb:autofill',
	},
	{
		key: 'dt-3',
		parentKey: '29d78e6c-c1bf-4c15-b820-d511c237ffae',
		name: 'My JS Property Editor',
		hasChildren: false,
		isTrashed: false,
		type: 'dataType',
		icon: 'umb:autofill',
	},
	{
		key: 'dt-4',
		parentKey: '29d78e6c-c1bf-4c15-b820-d511c237ffae',
		name: 'Context Example',
		hasChildren: false,
		isTrashed: false,
		type: 'dataType',
		icon: 'umb:autofill',
	},
	{
		key: 'dt-5',
		parentKey: '29d78e6c-c1bf-4c15-b820-d511c237ffae',
		name: 'Content Picker (DataType)',
		hasChildren: false,
		isTrashed: false,
		type: 'dataType',
		icon: 'umb:autofill',
	},
	{
		key: 'dt-6',
		name: 'Empty',
		type: 'dataType',
		parentKey: '29d78e6c-c1bf-4c15-b820-d511c237ffae',
		isTrashed: false,
		hasChildren: false,
		icon: 'umb:autofill',
	},
	{
		key: 'd81c7957-153c-4b5a-aa6f-b434a4964624',
		name: 'Document Type 1',
		type: 'documentType',
		hasChildren: false,
		isTrashed: false,
		parentKey: 'f50eb86d-3ef2-4011-8c5d-c56c04eec0da',
		icon: 'umb:item-arrangement',
	},
	{
		key: 'a99e4018-3ffc-486b-aa76-eecea9593d17',
		name: 'Document Type 2',
		type: 'documentType',
		hasChildren: false,
		isTrashed: false,
		parentKey: 'f50eb86d-3ef2-4011-8c5d-c56c04eec0da',
		icon: 'umb:item-arrangement',
	},
	{
		key: 'f2f81a40-c989-4b6b-84e2-057cecd3adc1',
		name: 'Media 1',
		type: 'media',
		icon: 'umb:picture',
		hasChildren: false,
		isTrashed: false,
		parentKey: 'c0858d71-52be-4bb2-822f-42fa0c9a1ea5',
	},
	{
		key: '69431027-8867-45bf-a93b-72bbdabfb177',
		type: 'media',
		name: 'Media 2',
		icon: 'umb:picture',
		hasChildren: false,
		isTrashed: false,
		parentKey: 'c0858d71-52be-4bb2-822f-42fa0c9a1ea5',
	},
	{
		key: '74e4008a-ea4f-4793-b924-15e02fd380d1',
		name: 'Document 1',
		type: 'document',
		icon: 'umb:document',
		hasChildren: false,
		isTrashed: false,
		parentKey: 'ba23245c-d8c0-46f7-a2bc-7623743d6eba',
	},
	{
		key: '74e4008a-ea4f-4793-b924-15e02fd380d2',
		name: 'Document 2',
		type: 'document',
		icon: 'umb:document',
		hasChildren: false,
		isTrashed: false,
		parentKey: 'ba23245c-d8c0-46f7-a2bc-7623743d6eba',
	},
	{
		key: 'cdd30288-2d1c-41b4-89a9-61647b4a10d5',
		name: 'Document 3',
		type: 'document',
		icon: 'umb:document',
		hasChildren: false,
		isTrashed: false,
		parentKey: 'ba23245c-d8c0-46f7-a2bc-7623743d6eba',
	},
	...documents,
];
