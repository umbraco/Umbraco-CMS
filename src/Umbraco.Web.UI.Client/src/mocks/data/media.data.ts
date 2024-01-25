import { UmbEntityData } from './entity.data.js';
import { createMediaTreeItem } from './utils.js';
import type {
	ContentTreeItemResponseModel,
	MediaItemResponseModel,
	MediaResponseModel,
	MediaTreeItemResponseModel,
	PagedMediaTreeItemResponseModel,
} from '@umbraco-cms/backoffice/backend-api';

export type UmbMockMediaModelHack = MediaResponseModel & MediaTreeItemResponseModel & MediaItemResponseModel;

export interface UmbMockMediaModel extends Omit<UmbMockMediaModelHack, 'type'> {}

export const data: Array<UmbMockMediaModel> = [
	{
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
		values: [
			{
				alias: 'myMediaHeadline',
				value: 'The daily life at Umbraco HQ',
			},
		],
		variants: [],
		urls: [],
	},
	{
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
		values: [
			{
				alias: 'myMediaDescription',
				value: 'Every day, a rabbit in a military costume greets me at the front door',
			},
		],
		variants: [],
		urls: [],
	},
	{
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
		values: [],
		variants: [],
		urls: [],
	},
	{
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
		values: [],
		variants: [],
		urls: [],
	},
	{
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
		values: [
			{
				alias: 'myMediaDescription',
				value: 'Every day, a rabbit in a military costume greets me at the front door',
			},
		],
		variants: [],
		urls: [],
	},
	{
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
		values: [
			{
				alias: 'myMediaDescription',
				value: 'Every day, a rabbit in a military costume greets me at the front door',
			},
		],
		variants: [],
		urls: [],
	},
	{
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
		values: [
			{
				alias: 'myMediaDescription',
				value: 'Every day, a rabbit in a military costume greets me at the front door',
			},
		],
		variants: [],
		urls: [],
	},
	{
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
		values: [
			{
				alias: 'myMediaDescription',
				value: 'Every day, a rabbit in a military costume greets me at the front door',
			},
		],
		variants: [],
		urls: [],
	},
];

const createMediaItem = (item: UmbMockMediaModel): MediaItemResponseModel => {
	return {
		id: item.id,
		mediaType: item.mediaType,
		isTrashed: false,
		variants: item.variants,
	};
};

class UmbMediaData extends UmbEntityData<UmbMockMediaModel> {
	constructor() {
		super(data);
	}

	getItems(ids: Array<string>): Array<MediaItemResponseModel> {
		const items = this.data.filter((item) => ids.includes(item.id));
		return items.map((item) => createMediaItem(item));
	}

	getTreeRoot(): PagedMediaTreeItemResponseModel {
		const items = this.data.filter((item) => item.parent === null);
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
}

export const umbMediaData = new UmbMediaData();
