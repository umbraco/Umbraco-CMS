import { UmbEntityData } from './entity.data';
import { createFileSystemTreeItem } from './utils';
import {
	FileSystemTreeItemPresentationModel,
	PagedFileSystemTreeItemPresentationModel,
} from '@umbraco-cms/backoffice/backend-api';

export const data: Array<FileSystemTreeItemPresentationModel> = [
	{
		path: 'blockgrid',
		isFolder: true,
		name: 'blockgrid',
		type: 'partial-view',
		icon: 'umb:folder',
		hasChildren: true,
	},
	{
		path: 'blocklist',
		isFolder: true,
		name: 'blocklist',
		type: 'partial-view',
		icon: 'umb:folder',
		hasChildren: true,
	},
	{
		path: 'grid',
		isFolder: true,
		name: 'grid',
		type: 'partial-view',
		icon: 'umb:folder',
		hasChildren: true,
	},
];

// Temp mocked database
// TODO: all properties are optional in the server schema. I don't think this is correct.
// eslint-disable-next-line @typescript-eslint/ban-ts-comment
// @ts-ignore
class UmbPartialViewsData extends UmbEntityData<FileSystemTreeItemPresentationModel> {
	constructor() {
		super(data);
	}

	getTreeRoot(): PagedFileSystemTreeItemPresentationModel {
		const items = this.data.filter((item) => item.path?.includes('/') === false);
		const treeItems = items.map((item) => createFileSystemTreeItem(item));
		const total = items.length;
		return { items: treeItems, total };
	}

	getTreeItemChildren(parentPath: string): PagedFileSystemTreeItemPresentationModel {
		const items = this.data.filter((item) => item.path?.startsWith(parentPath + '/'));
		const treeItems = items.map((item) => createFileSystemTreeItem(item));
		const total = items.length;
		return { items: treeItems, total };
	}

	getTreeItem(paths: Array<string>): Array<FileSystemTreeItemPresentationModel> {
		const items = this.data.filter((item) => paths.includes(item.path ?? ''));
		return items.map((item) => createFileSystemTreeItem(item));
	}
}

export const umbPartialViewsData = new UmbPartialViewsData();
