import { UmbEntityData } from './entity.data';
import { createContentTreeItem } from './utils';
import { ContentTreeItem, PagedContentTreeItem } from '@umbraco-cms/backend-api';
import type { MediaDetails } from '@umbraco-cms/models';

export const data: Array<MediaDetails> = [
	{
		name: 'Media 1',
		type: 'media',
		icon: 'picture',
		hasChildren: false,
		key: 'f2f81a40-c989-4b6b-84e2-057cecd3adc1',
		isContainer: false,
		parentKey: null,
		noAccess: false,
		isTrashed: false,
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
		name: 'Media 2',
		type: 'media',
		icon: 'picture',
		hasChildren: false,
		key: '69431027-8867-45bf-a93b-72bbdabfb177',
		isContainer: false,
		parentKey: null,
		noAccess: false,
		isTrashed: false,
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
	{
		name: 'Media Folder 1',
		type: 'media',
		icon: 'folder',
		hasChildren: true,
		key: '69461027-8867-45bf-a93b-72bbdabfb177',
		isContainer: true,
		parentKey: null,
		noAccess: false,
		isTrashed: false,
		properties: [],
		data: [],
		variants: [],
	},
	{
		name: 'Media 3',
		type: 'media',
		icon: 'picture',
		hasChildren: false,
		key: '69431027-8867-45s7-a93b-7uibdabfb177',
		isContainer: false,
		parentKey: '69461027-8867-45bf-a93b-72bbdabfb177',
		noAccess: false,
		isTrashed: false,
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
// TODO: all properties are optional in the server schema. I don't think this is correct.
// eslint-disable-next-line @typescript-eslint/ban-ts-comment
// @ts-ignore
class UmbMediaData extends UmbEntityData<MediaDetails> {
	constructor() {
		super(data);
	}

	getTreeRoot(): PagedContentTreeItem {
		const items = this.data.filter((item) => item.parentKey === null);
		const treeItems = items.map((item) => createContentTreeItem(item));
		const total = items.length;
		return { items: treeItems, total };
	}

	getTreeItemChildren(key: string): PagedContentTreeItem {
		const items = this.data.filter((item) => item.parentKey === key);
		const treeItems = items.map((item) => createContentTreeItem(item));
		const total = items.length;
		return { items: treeItems, total };
	}

	getTreeItem(keys: Array<string>): Array<ContentTreeItem> {
		const items = this.data.filter((item) => keys.includes(item.key));
		return items.map((item) => createContentTreeItem(item));
	}
}

export const umbMediaData = new UmbMediaData();
