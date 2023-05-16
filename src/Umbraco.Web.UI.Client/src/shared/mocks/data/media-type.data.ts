import type { MediaTypeDetails } from '../../../packages/media/media-types/types';
import { UmbEntityData } from './entity.data';
import { createFolderTreeItem } from './utils';
import { FolderTreeItemResponseModel, PagedFolderTreeItemResponseModel } from '@umbraco-cms/backoffice/backend-api';

export const data: Array<MediaTypeDetails> = [
	{
		$type: 'media-type',
		name: 'Media Type 1',
		type: 'media-type',
		hasChildren: false,
		id: 'c5159663-eb82-43ee-bd23-e42dc5e71db6',
		isContainer: false,
		parentId: null,
		isFolder: false,
		icon: '',
		alias: 'mediaType1',
		properties: [],
	},
	{
		$type: 'media-type',
		name: 'Media Type 2',
		type: 'media-type',
		hasChildren: false,
		id: '22da1b0b-c310-4730-9912-c30b3eb9802e',
		isContainer: false,
		parentId: null,
		isFolder: false,
		icon: '',
		alias: 'mediaType2',
		properties: [],
	},
];

// Temp mocked database
// TODO: all properties are optional in the server schema. I don't think this is correct.
// eslint-disable-next-line @typescript-eslint/ban-ts-comment
// @ts-ignore
class UmbMediaTypeData extends UmbEntityData<MediaTypeDetails> {
	constructor() {
		super(data);
	}

	getTreeRoot(): PagedFolderTreeItemResponseModel {
		const items = this.data.filter((item) => item.parentId === null);
		const treeItems = items.map((item) => createFolderTreeItem(item));
		const total = items.length;
		return { items: treeItems, total };
	}

	getTreeItemChildren(id: string): PagedFolderTreeItemResponseModel {
		const items = this.data.filter((item) => item.parentId === id);
		const treeItems = items.map((item) => createFolderTreeItem(item));
		const total = items.length;
		return { items: treeItems, total };
	}

	getTreeItem(ids: Array<string>): Array<FolderTreeItemResponseModel> {
		const items = this.data.filter((item) => ids.includes(item.id ?? ''));
		return items.map((item) => createFolderTreeItem(item));
	}
}

export const umbMediaTypeData = new UmbMediaTypeData();
