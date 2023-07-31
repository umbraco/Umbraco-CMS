import { UmbEntityData } from './entity.data.js';
import { createFileSystemTreeItem, createTextFileItem } from './utils.js';
import {
	CreateTextFileViewModelBaseModel,
	FileSystemTreeItemPresentationModel,
	PagedFileSystemTreeItemPresentationModel,
	StylesheetResponseModel,
} from '@umbraco-cms/backoffice/backend-api';

type StylesheetDBItem = StylesheetResponseModel & FileSystemTreeItemPresentationModel;

export const data: Array<StylesheetDBItem> = [
	{
		path: 'Stylesheet File 1.css',
		isFolder: false,
		name: 'Stylesheet File 1.css',
		type: 'stylesheet',
		hasChildren: false,
		content: `
		h1 {
			color: blue;
		}
		
		/**umb_name:bjjh*/
		h1 {
			color: blue;
		}
		
		/**umb_name:comeone*/
		h1 {
			color: blue;
		}
		
		/**umb_name:lol*/
		h1 {
			color: blue;
		}
		`,
	},
	{
		path: 'Stylesheet File 2.css',
		isFolder: false,
		name: 'Stylesheet File 2.css',
		type: 'stylesheet',
		hasChildren: false,
		content: `		h1 {
			color: green;
		}
		
		/**umb_name:bjjh*/
		h1 {
			color: green;
		}
		
		/**umb_name:comeone*/
		h1 {
			color: green;
		}
		
		/**umb_name:lol*/
		h1 {
			color: green;
		}`,
	},
	{
		path: 'Folder 1',
		isFolder: true,
		name: 'Folder 1',
		type: 'stylesheet',
		hasChildren: true,
		content: `		h1 {
			color: pink;
		}
		
		/**umb_name:bjjh*/
		h1 {
			color: pink;
		}
		
		/**umb_name:comeone*/
		h1 {
			color: pink;
		}
		
		/**umb_name:lol*/
		h1 {
			color: pink;
		}`,
	},
	{
		path: 'Folder 1/Stylesheet File 3.css',
		isFolder: false,
		name: 'Stylesheet File 3.css',
		type: 'stylesheet',
		hasChildren: false,
		content: `		h1 {
			color: red;
		}
		
		/**umb_name:bjjh*/
		h1 {
			color: red;
		}
		
		/**umb_name:comeone*/
		h1 {
			color: red;
		}
		
		/**umb_name:lol*/
		h1 {
			color: red;
		}`,
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

	getStylesheet(path: string): StylesheetDBItem | undefined {
		return createTextFileItem(this.data.find((item) => item.path === path));
	}

	insertStyleSheet(item: CreateTextFileViewModelBaseModel) {
		const newItem: StylesheetDBItem = {
			...item,
			path: `${item.parentPath}/${item.name}.cshtml}`,
			isFolder: false,
			hasChildren: false,
			type: 'partial-view',
		};

		this.insert(newItem);
		return newItem;
	}
}

export const umbStylesheetData = new UmbStylesheetData();
