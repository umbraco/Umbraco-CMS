import type { MediaDetails } from '../../packages/media/media';
import { UmbEntityTreeData } from './entity-tree.data';
import { UmbEntityData } from './entity.data';
import { createContentTreeItem } from './utils';
import { ContentTreeItemResponseModel, PagedContentTreeItemResponseModel } from '@umbraco-cms/backoffice/backend-api';

export const data: Array<MediaDetails> = [
	{
		$type: '',
		name: 'Flipped Car',
		type: 'media',
		icon: 'picture',
		hasChildren: false,
		id: 'f2f81a40-c989-4b6b-84e2-057cecd3adc1',
		isContainer: false,
		parentId: null,
		noAccess: false,
		isTrashed: false,
		properties: [
			{
				alias: 'myMediaHeadline',
				label: 'Media Headline',
				description: 'Text string property',
				dataTypeId: 'dt-textBox',
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
		$type: '',
		name: 'Umbracoffee',
		type: 'media',
		icon: 'picture',
		hasChildren: false,
		id: '69431027-8867-45bf-a93b-72bbdabfb177',
		isContainer: false,
		parentId: null,
		noAccess: false,
		isTrashed: false,
		properties: [
			{
				alias: 'myMediaDescription',
				label: 'Description',
				description: 'Textarea property',
				dataTypeId: 'dt-textArea',
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
		$type: '',
		name: 'People',
		type: 'media',
		icon: 'folder',
		hasChildren: true,
		id: '69461027-8867-45bf-a93b-72bbdabfb177',
		isContainer: true,
		parentId: null,
		noAccess: false,
		isTrashed: false,
		properties: [],
		data: [],
		variants: [],
	},
	{
		$type: '',
		name: 'Products',
		type: 'media',
		icon: 'folder',
		hasChildren: true,
		id: '69461027-8867-45bf-a93b-5224dabfb177',
		isContainer: true,
		parentId: null,
		noAccess: false,
		isTrashed: false,
		properties: [],
		data: [],
		variants: [],
	},
	{
		$type: '',
		name: 'John Smith',
		type: 'media',
		icon: 'picture',
		hasChildren: false,
		id: '69431027-8867-45s7-a93b-7uibdabfb177',
		isContainer: false,
		parentId: '69461027-8867-45bf-a93b-72bbdabfb177',
		noAccess: false,
		isTrashed: false,
		properties: [
			{
				alias: 'myMediaDescription',
				label: 'Description',
				description: 'Textarea property',
				dataTypeId: 'dt-textArea',
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
		$type: '',
		name: 'Jane Doe',
		type: 'media',
		icon: 'picture',
		hasChildren: false,
		id: '69431027-8867-45s7-a93b-7uibdabf2147',
		isContainer: false,
		parentId: '69461027-8867-45bf-a93b-72bbdabfb177',
		noAccess: false,
		isTrashed: false,
		properties: [
			{
				alias: 'myMediaDescription',
				label: 'Description',
				description: 'Textarea property',
				dataTypeId: 'dt-textArea',
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
		$type: '',
		name: 'A very nice hat',
		type: 'media',
		icon: 'picture',
		hasChildren: false,
		id: '694hdj27-8867-45s7-a93b-7uibdabf2147',
		isContainer: false,
		parentId: '69461027-8867-45bf-a93b-5224dabfb177',
		noAccess: false,
		isTrashed: false,
		properties: [
			{
				alias: 'myMediaDescription',
				label: 'Description',
				description: 'Textarea property',
				dataTypeId: 'dt-textArea',
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
		$type: '',
		name: 'Fancy old chair',
		type: 'media',
		icon: 'picture',
		hasChildren: false,
		id: '694hdj27-1237-45s7-a93b-7uibdabfas47',
		isContainer: false,
		parentId: '69461027-8867-45bf-a93b-5224dabfb177',
		noAccess: false,
		isTrashed: false,
		properties: [
			{
				alias: 'myMediaDescription',
				label: 'Description',
				description: 'Textarea property',
				dataTypeId: 'dt-textArea',
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
	#tree = new UmbEntityTreeData<ContentTreeItemResponseModel>(this);

	constructor() {
		super(data);
	}

	getTreeRoot(): PagedContentTreeItemResponseModel {
		const items = this.data.filter((item) => item.parentId === null);
		const treeItems = items.map((item) => createContentTreeItem(item));
		const total = items.length;
		return { items: treeItems, total };
	}

	getTreeItemChildren(id: string): PagedContentTreeItemResponseModel {
		const items = this.data.filter((item) => item.parentId === id);
		const treeItems = items.map((item) => createContentTreeItem(item));
		const total = items.length;
		return { items: treeItems, total };
	}

	getTreeItem(ids: Array<string>): Array<ContentTreeItemResponseModel> {
		const items = this.data.filter((item) => ids.includes(item.id));
		return items.map((item) => createContentTreeItem(item));
	}

	move(ids: Array<string>, destinationKey: string) {
		return this.#tree.move(ids, destinationKey);
	}
}

export const umbMediaData = new UmbMediaData();
