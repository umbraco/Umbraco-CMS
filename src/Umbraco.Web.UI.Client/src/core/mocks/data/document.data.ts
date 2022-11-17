import { UmbData } from './data';
import { DocumentTreeItem, PagedDocumentTreeItem } from '@umbraco-cms/backend-api';
import type { DocumentDetails } from '@umbraco-cms/models';

export const data: Array<DocumentDetails> = [
	{
		key: 'f2f81a40-c989-4b6b-84e2-057cecd3adc1',
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
