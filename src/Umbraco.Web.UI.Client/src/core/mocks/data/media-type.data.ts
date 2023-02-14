import { UmbEntityData } from './entity.data';
import { createFolderTreeItem } from './utils';
import { FolderTreeItemModel, PagedFolderTreeItemModel } from '@umbraco-cms/backend-api';
import type { MediaTypeDetails } from '@umbraco-cms/models';

export const data: Array<MediaTypeDetails> = [
	{
		name: 'Media Type 1',
		type: 'media-type',
		hasChildren: false,
		key: 'c5159663-eb82-43ee-bd23-e42dc5e71db6',
		isContainer: false,
		parentKey: null,
		isFolder: false,
		icon: '',
		alias: 'mediaType1',
		properties: [],
	},
	{
		name: 'Media Type 2',
		type: 'media-type',
		hasChildren: false,
		key: '22da1b0b-c310-4730-9912-c30b3eb9802e',
		isContainer: false,
		parentKey: null,
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

	getTreeRoot(): PagedFolderTreeItemModel {
		const items = this.data.filter((item) => item.parentKey === null);
		const treeItems = items.map((item) => createFolderTreeItem(item));
		const total = items.length;
		return { items: treeItems, total };
	}

	getTreeItemChildren(key: string): PagedFolderTreeItemModel {
		const items = this.data.filter((item) => item.parentKey === key);
		const treeItems = items.map((item) => createFolderTreeItem(item));
		const total = items.length;
		return { items: treeItems, total };
	}

	getTreeItem(keys: Array<string>): Array<FolderTreeItemModel> {
		const items = this.data.filter((item) => keys.includes(item.key ?? ''));
		return items.map((item) => createFolderTreeItem(item));
	}
}

export const umbMediaTypeData = new UmbMediaTypeData();
