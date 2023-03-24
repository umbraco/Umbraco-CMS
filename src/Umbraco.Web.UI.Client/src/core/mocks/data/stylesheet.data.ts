import { UmbEntityData } from './entity.data';
import { createFileSystemTreeItem } from './utils';
import {
	FileSystemTreeItemPresentationModel,
	PagedFileSystemTreeItemPresentationModel,
} from '@umbraco-cms/backoffice/backend-api';

type StylesheetDBItem = FileSystemTreeItemPresentationModel & {
	content: string;
};

export const data: Array<StylesheetDBItem> = [
	{
		path: 'Stylesheet File 1.css',
		isFolder: false,
		name: 'Stylesheet File 1.css',
		type: 'stylesheet',
		icon: 'umb:brackets',
		hasChildren: false,
		content: `Stylesheet content 1`,
	},
	{
		path: 'Stylesheet File 2.css',
		isFolder: false,
		name: 'Stylesheet File 2.css',
		type: 'stylesheet',
		icon: 'umb:brackets',
		hasChildren: false,
		content: `Stylesheet content 2`,
	},
	{
		path: 'Folder 1',
		isFolder: true,
		name: 'Folder 1',
		type: 'stylesheet',
		icon: 'umb:folder',
		hasChildren: true,
		content: `Stylesheet content 3`,
	},
	{
		path: 'Folder 1/Stylesheet File 3.css',
		isFolder: false,
		name: 'Stylesheet File 3.css',
		type: 'stylesheet',
		icon: 'umb:brackets',
		hasChildren: false,
		content: `Stylesheet content 3`,
	},
];

// Temp mocked database
// TODO: all properties are optional in the server schema. I don't think this is correct.
// eslint-disable-next-line @typescript-eslint/ban-ts-comment
// @ts-ignore
class UmbStylesheetData extends UmbEntityData<StylesheetDBItem> {
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

export const umbStylesheetData = new UmbStylesheetData();
