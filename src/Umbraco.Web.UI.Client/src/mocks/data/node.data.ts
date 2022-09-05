import { UmbData } from './data';

export interface NodeEntity {
	key: string;
	name: string;
	icon: string; // TODO: should come from the doc type?
	type: string;
	isTrashed: boolean;
	hasChildren: boolean;
	parentKey: string;
	properties: Array<NodeProperty>;
	data: Array<NodePropertyData>;
	variants: Array<any>; // TODO: define variant data
	//layout?: any; // TODO: define layout type - make it non-optional
}

export interface NodeProperty {
	alias: string;
	label: string;
	description: string;
	dataTypeKey: string;
}

export interface NodePropertyData {
	alias: string;
	value: any;
}

/* TODO:
Consider splitting data into smaller thunks that matches our different stores.
example: we need an entity store for things in the tree, so we dont load the full nodes for everything in the tree.
We would like the tree items to stay up to date, without requesting the server again.

If we split entityData into its own object, then that could go in the entityStore and be merged with the nodeStore (we would have a subscription on both).
*/
export const data: Array<NodeEntity> = [
	{
		key: '74e4008a-ea4f-4793-b924-15e02fd380d1',
		isTrashed: false,
		name: 'Document 1',
		type: 'document',
		icon: 'document',
		parentKey: 'ba23245c-d8c0-46f7-a2bc-7623743d6eba',
		hasChildren: false,
		properties: [
			{
				alias: 'myHeadline',
				label: 'Headline',
				description: 'Text string property',
				dataTypeKey: 'dt-1',
			},
			{
				alias: 'myDescription',
				label: 'Description',
				description: 'Textarea property',
				dataTypeKey: 'dt-2',
			},
		],
		data: [
			{
				alias: 'myHeadline',
				value: 'The daily life at Umbraco HQ',
			},
			{
				alias: 'myDescription',
				value: 'Every day, a rabbit in a military costume greets me at the front door',
			},
		],
		variants: [{ name: 'fake data' }],
		/*
    // Concept for node layout, separation of design from config and data.
    layout: [
      {
        type: 'group',
        children: [
          {
            type: 'property',
            alias: 'myHeadline'
          },
          {
            type: 'property',
            alias: 'myDescription'
          }
        ]
      }
    ],
    */
	},
	{
		key: '74e4008a-ea4f-4793-b924-15e02fd380d2',
		isTrashed: false,
		name: 'Document 2',
		type: 'document',
		icon: 'favorite',
		parentKey: 'ba23245c-d8c0-46f7-a2bc-7623743d6eba',
		hasChildren: false,
		properties: [
			{
				alias: 'myHeadline',
				label: 'Text string label',
				description: 'this is a text string property',
				dataTypeKey: 'dt-1',
			},
			{
				alias: 'myDescription',
				label: 'Textarea label',
				description: 'This is the a textarea property',
				dataTypeKey: 'dt-2',
			},
			{
				alias: 'myExternalEditor',
				label: 'My JS Property Editor',
				description: 'This is the a external property',
				dataTypeKey: 'dt-3',
			},
			{
				alias: 'myContextExampleEditor',
				label: 'Context example label',
				description: 'This is the a example property',
				dataTypeKey: 'dt-4',
			},
			{
				alias: 'myContentPicker',
				label: 'Content Picker',
				description: 'This is a content picker',
				dataTypeKey: 'dt-5',
			},
		],
		data: [
			{
				alias: 'myHeadline',
				value: 'Is it all just fun and curling and scary rabbits?',
			},
			{
				alias: 'myDescription',
				value:
					"So no, there's not confetti every day. And no, there's not champagne every week or a crazy rabbit running around 🐰",
			},
			{
				alias: 'myExternalEditor',
				value: 'Tex lkasdfkljdfsa 1',
			},
			{
				alias: 'myContextExampleEditor',
				value: '',
			},
			{
				alias: 'myContentPicker',
				value: '',
			},
		],
		variants: [{ name: 'Variant 1' }],
	},
	{
		key: 'cdd30288-2d1c-41b4-89a9-61647b4a10d5',
		isTrashed: false,
		name: 'Document 3',
		type: 'document',
		icon: 'document',
		parentKey: 'ba23245c-d8c0-46f7-a2bc-7623743d6eba',
		hasChildren: false,
		properties: [
			{
				alias: 'myDescription',
				label: 'Description',
				description: 'Textarea property',
				dataTypeKey: 'dt-2',
			},
		],
		data: [
			{
				alias: 'myDescription',
				value: 'Every day, a rabbit in a military costume greets me at the front door',
			},
		],
		variants: [{ name: 'Variant 1' }],
	},
	{
		key: 'f2f81a40-c989-4b6b-84e2-057cecd3adc1',
		isTrashed: false,
		name: 'Media 1',
		type: 'media',
		icon: 'picture',
		parentKey: 'c0858d71-52be-4bb2-822f-42fa0c9a1ea5',
		hasChildren: false,
		properties: [
			{
				alias: 'myMediaHeadline',
				label: 'Media Headline',
				description: 'Text string property',
				dataTypeKey: 'dt-1',
			},
		],
		data: [
			{
				alias: 'myMediaHeadline',
				value: 'The daily life at Umbraco HQ',
			},
		],
		variants: [],
	},
	{
		key: '69431027-8867-45bf-a93b-72bbdabfb177',
		isTrashed: false,
		type: 'media',
		name: 'Media 2',
		icon: 'picture',
		parentKey: 'c0858d71-52be-4bb2-822f-42fa0c9a1ea5',
		hasChildren: false,
		properties: [
			{
				alias: 'myMediaDescription',
				label: 'Description',
				description: 'Textarea property',
				dataTypeKey: 'dt-2',
			},
		],
		data: [
			{
				alias: 'myMediaDescription',
				value: 'Every day, a rabbit in a military costume greets me at the front door',
			},
		],
		variants: [],
	},
];

// Temp mocked database
class UmbNodeData extends UmbData<NodeEntity> {
	constructor() {
		super(data);
	}
}

export const umbNodeData = new UmbNodeData();
