import { UmbData } from './data.js';
import { createFileItemResponseModelBaseModel, createFileSystemTreeItem } from './utils.js';
import {
	FileSystemTreeItemPresentationModel,
	PagedFileSystemTreeItemPresentationModel,
	ScriptItemResponseModel,
	StaticFileItemResponseModel,
} from '@umbraco-cms/backoffice/backend-api';

type StaticFileItem = StaticFileItemResponseModel & FileSystemTreeItemPresentationModel & { icon?: string };

export const data: Array<StaticFileItem> = [
	{
		path: 'some-file.js',
		name: 'some-file',
		icon: 'icon-document',
		type: 'static-file',
		hasChildren: false,
		isFolder: false,
	},
	{
		path: 'another-file.js',
		name: 'another-file',
		icon: 'icon-document',
		type: 'static-file',
		hasChildren: false,
		isFolder: false,
	},
];

class UmbStaticFileData extends UmbData<StaticFileItem> {
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
		const items = this.data.filter((item) => item.path?.startsWith(parentPath));
		const treeItems = items.map((item) => createFileSystemTreeItem(item));
		const total = items.length;
		return { items: treeItems, total };
	}

	getTreeItem(paths: Array<string>): Array<FileSystemTreeItemPresentationModel> {
		const items = this.data.filter((item) => paths.includes(item.path ?? ''));
		return items.map((item) => createFileSystemTreeItem(item));
	}

	getItem(paths: Array<string>): Array<ScriptItemResponseModel> {
		const items = this.data.filter((item) => paths.includes(item.path ?? ''));
		return items.map((item) => createFileItemResponseModelBaseModel(item));
	}
}

export const umbStaticFileData = new UmbStaticFileData();
