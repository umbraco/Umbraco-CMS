import { UmbData } from './data';
import { DocumentTreeItem, PagedDocumentTreeItem } from '@umbraco-cms/backend-api';
import type { DocumentDetails } from '@umbraco-cms/models';

export const data: Array<DocumentDetails> = [
	{
		name: 'Document 1',
		type: 'document',
		icon: 'document',
		hasChildren: false,
		key: '74e4008a-ea4f-4793-b924-15e02fd380d1',
		isContainer: false,
		parentKey: null,
		noAccess: false,
		isProtected: false,
		isPublished: false,
		isEdited: false,
		isTrashed: false,
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
		name: 'Document 2',
		type: 'document',
		icon: 'favorite',
		hasChildren: false,
		key: '74e4008a-ea4f-4793-b924-15e02fd380d2',
		isContainer: false,
		parentKey: null,
		noAccess: false,
		isProtected: false,
		isPublished: false,
		isEdited: false,
		isTrashed: false,
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
		name: 'Document 3',
		type: 'document',
		icon: 'document',
		hasChildren: false,
		key: 'cdd30288-2d1c-41b4-89a9-61647b4a10d5',
		isContainer: false,
		parentKey: null,
		noAccess: false,
		isProtected: false,
		isPublished: false,
		isEdited: false,
		isTrashed: false,
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
];

// Temp mocked database
class UmbDocumentData extends UmbData<DocumentDetails> {
	constructor() {
		super(data);
	}

	getTreeRoot(): PagedDocumentTreeItem {
		const items = this.data.filter((item) => item.parentKey === null);
		const total = items.length;
		return { items, total };
	}

	getTreeItemChildren(key: string): PagedDocumentTreeItem {
		const items = this.data.filter((item) => item.parentKey === key);
		const total = items.length;
		return { items, total };
	}

	getTreeItem(keys: Array<string>): Array<DocumentTreeItem> {
		return this.data.filter((item) => keys.includes(item.key ?? ''));
	}
}

export const umbDocumentData = new UmbDocumentData();
