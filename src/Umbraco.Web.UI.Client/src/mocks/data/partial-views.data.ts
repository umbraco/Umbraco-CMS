import { UmbEntityData } from './entity.data.js';
import { createFileSystemTreeItem } from './utils.js';
import {
	FileSystemTreeItemPresentationModel,
	PagedFileSystemTreeItemPresentationModel,
	SnippetItemResponseModel,
} from '@umbraco-cms/backoffice/backend-api';

export const treeData: Array<FileSystemTreeItemPresentationModel> = [
	{
		path: 'blockgrid',
		isFolder: true,
		name: 'blockgrid',
		type: 'partial-view',
		hasChildren: true,
	},
	{
		path: 'blocklist',
		isFolder: true,
		name: 'blocklist',
		type: 'partial-view',
		hasChildren: true,
	},
	{
		path: 'grid',
		isFolder: true,
		name: 'grid',
		type: 'partial-view',
		hasChildren: true,
	},
	{
		path: 'blockgrid/area.cshtml',
		isFolder: false,
		name: 'area.cshtml',
		type: 'partial-view',
		hasChildren: false,
	},
	{
		path: 'blockgrid/items.cshtml',
		isFolder: false,
		name: 'items.cshtml',
		type: 'partial-view',
		hasChildren: false,
	},
	{
		path: 'blocklist/default.cshtml',
		isFolder: false,
		name: 'default.cshtml',
		type: 'partial-view',
		hasChildren: false,
	},
	{
		path: 'grid/editors',
		isFolder: false,
		name: 'editors',
		type: 'partial-view',
		hasChildren: false,
	},
	{
		path: 'grid/default.cshtml',
		isFolder: false,
		name: 'items.cshtml',
		type: 'partial-view',
		hasChildren: false,
	},
];

class UmbPartialViewsTreeData extends UmbEntityData<FileSystemTreeItemPresentationModel> {
	constructor() {
		super(treeData);
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

export const umbPartialViewsTreeData = new UmbPartialViewsTreeData();

export const snippets: Array<SnippetItemResponseModel> = [
	{
		name: 'Empty',
	},
	{
		name: 'Breadcrumb',
	},
	{
		name: 'EditProfile',
	},
	{
		name: 'ListAncestorsFromCurrentPage',
	},
	{
		name: 'ListChildPagesFromCurrentPage',
	},
	{
		name: 'ListChildPagesOrderedByDate',
	},
	{
		name: 'ListChildPagesOrderedByName',
	},
	{
		name: 'ListChildPagesWithDoctype',
	},
	{
		name: 'ListDescendantsFromCurrentPage',
	},
	{
		name: 'Login',
	},
	{
		name: 'LoginStatus',
	},
	{
		name: 'MultinodeTree-picker',
	},
	{
		name: 'Navigation',
	},
	{
		name: 'RegisterMember',
	},
	{
		name: 'SiteMap',
	},
];
