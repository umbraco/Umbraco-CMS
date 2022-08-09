import { UmbData } from './data';

export interface NodeEntity {
	id: number;
	key: string;
	name: string;
	icon: string; // TODO: should come from the doc type?
	type: string;
	properties: Array<NodeProperty>;
	data: Array<NodePropertyData>;
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
		id: 1,
		key: '74e4008a-ea4f-4793-b924-15e02fd380d1',
		name: 'Document 1',
		type: 'document',
		icon: 'document',
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
		id: 2,
		key: '74e4008a-ea4f-4793-b924-15e02fd380d2',
		name: 'Document 2',
		type: 'document',
		icon: 'favorite',
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
		],
	},
	{
		id: 3,
		key: 'cdd30288-2d1c-41b4-89a9-61647b4a10d5',
		name: 'Document 3',
		type: 'document',
		icon: 'document',
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
	},
	{
		id: 2001,
		key: 'f2f81a40-c989-4b6b-84e2-057cecd3adc1',
		name: 'Media 1',
		type: 'media',
		icon: 'picture',
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
	},
	{
		id: 2002,
		key: '69431027-8867-45bf-a93b-72bbdabfb177',
		type: 'media',
		name: 'Media 2',
		icon: 'picture',
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
	},
];

// Temp mocked database
class UmbNodeData extends UmbData<NodeEntity> {
	constructor() {
		super(data);
	}
}

export const umbNodeData = new UmbNodeData();
