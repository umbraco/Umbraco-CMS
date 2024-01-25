import type { UmbMediaDetailModel } from '../../packages/media/media/index.js';
import { UmbEntityData } from './entity.data.js';
import { createMediaTreeItem } from './utils.js';
import type {
	ContentTreeItemResponseModel,
	MediaItemResponseModel,
	PagedMediaTreeItemResponseModel,
} from '@umbraco-cms/backoffice/backend-api';

export const data: Array<UmbMediaDetailModel> = [
	{
		type: 'media',
		hasChildren: false,
		id: 'f2f81a40-c989-4b6b-84e2-057cecd3adc1',
		parent: null,
		noAccess: false,
		isTrashed: false,
		mediaType: {
			id: 'media-type-1-id',
			icon: 'icon-bug',
			hasListView: false,
		},
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
		type: 'media',
		hasChildren: false,
		id: '69431027-8867-45bf-a93b-72bbdabfb177',
		parent: null,
		noAccess: false,
		isTrashed: false,
		mediaType: {
			id: 'media-type-1-id',
			icon: 'icon-bug',
			hasListView: false,
		},
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
		type: 'media',
		hasChildren: true,
		id: '69461027-8867-45bf-a93b-72bbdabfb177',
		parent: null,
		noAccess: false,
		isTrashed: false,
		mediaType: {
			id: 'media-type-1-id',
			icon: 'icon-bug',
			hasListView: false,
		},
		properties: [],
		data: [],
		variants: [],
	},
	{
		type: 'media',
		hasChildren: true,
		id: '69461027-8867-45bf-a93b-5224dabfb177',
		parent: null,
		noAccess: false,
		isTrashed: false,
		mediaType: {
			id: 'media-type-1-id',
			icon: 'icon-bug',
			hasListView: false,
		},
		properties: [],
		data: [],
		variants: [],
	},
	{
		type: 'media',
		hasChildren: false,
		id: '69431027-8867-45s7-a93b-7uibdabfb177',
		parent: { id: '69461027-8867-45bf-a93b-72bbdabfb177' },
		noAccess: false,
		isTrashed: false,
		mediaType: {
			id: 'media-type-1-id',
			icon: 'icon-bug',
			hasListView: false,
		},
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
		type: 'media',
		hasChildren: false,
		id: '69431027-8867-45s7-a93b-7uibdabf2147',
		parent: { id: '69461027-8867-45bf-a93b-72bbdabfb177' },
		noAccess: false,
		isTrashed: false,
		mediaType: {
			id: 'media-type-1-id',
			icon: 'icon-bug',
			hasListView: false,
		},
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
		type: 'media',
		hasChildren: false,
		id: '694hdj27-8867-45s7-a93b-7uibdabf2147',
		parent: { id: '69461027-8867-45bf-a93b-5224dabfb177' },
		noAccess: false,
		isTrashed: false,
		mediaType: {
			id: 'media-type-1-id',
			icon: 'icon-bug',
			hasListView: false,
		},
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
		type: 'media',
		hasChildren: false,
		id: '694hdj27-1237-45s7-a93b-7uibdabfas47',
		parent: { id: '69461027-8867-45bf-a93b-5224dabfb177' },
		noAccess: false,
		isTrashed: false,
		mediaType: {
			id: 'media-type-1-id',
			icon: 'icon-bug',
			hasListView: false,
		},
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

const createMediaItem = (item: UmbMediaDetailModel): MediaItemResponseModel => {
	return {
		id: item.id,
		mediaType: item.mediaType,
		isTrashed: false,
		variants: item.variants,
	};
};

// Temp mocked database
// TODO: all properties are optional in the server schema. I don't think this is correct.
// eslint-disable-next-line @typescript-eslint/ban-ts-comment
// @ts-ignore
class UmbMediaData extends UmbEntityData<UmbMediaDetailModel> {
	constructor() {
		super(data);
	}

	getItems(ids: Array<string>): Array<MediaItemResponseModel> {
		const items = this.data.filter((item) => ids.includes(item.id ?? ''));
		return items.map((item) => createMediaItem(item));
	}

	getTreeRoot(): PagedMediaTreeItemResponseModel {
		const items = this.data.filter((item) => item.parent?.id === null);
		const treeItems = items.map((item) => createMediaTreeItem(item));
		const total = items.length;
		return { items: treeItems, total };
	}

	getTreeItemChildren(id: string): PagedMediaTreeItemResponseModel {
		const items = this.data.filter((item) => item.parent?.id === id);
		const treeItems = items.map((item) => createMediaTreeItem(item));
		const total = items.length;
		return { items: treeItems, total };
	}

	getTreeItem(ids: Array<string>): Array<ContentTreeItemResponseModel> {
		const items = this.data.filter((item) => ids.includes(item.id));
		return items.map((item) => createMediaTreeItem(item));
	}

	move(ids: Array<string>, destinationKey: string) {
		alert('change to new tree managers');
	}
}

export const umbMediaData = new UmbMediaData();
