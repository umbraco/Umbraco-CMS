import { UmbEntityData } from './entity.data.js';
import { createFileSystemTreeItem, createFileItemResponseModelBaseModel, createTextFileItem } from './utils.js';
import {
	CreateTextFileViewModelBaseModel,
	FileSystemTreeItemPresentationModel,
	PagedFileSystemTreeItemPresentationModel,
	PagedStylesheetOverviewResponseModel,
	RichTextRuleModel,
	StylesheetResponseModel,
} from '@umbraco-cms/backoffice/backend-api';

type StylesheetDBItem = StylesheetResponseModel & FileSystemTreeItemPresentationModel & { icon?: string };

export const data: Array<StylesheetDBItem> = [
	{
		path: 'Stylesheet File 1.css',
		icon: 'style',
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
		icon: 'style',

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
		icon: 'folder',

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

	getStylesheetItem(path: string): StylesheetDBItem | undefined {
		return createFileItemResponseModelBaseModel(this.data.find((item) => item.path === path));
	}

	getStylesheet(path: string): StylesheetResponseModel | undefined {
		return createTextFileItem(this.data.find((item) => item.path === path));
	}

	getAllStylesheets(): PagedStylesheetOverviewResponseModel {
		return {
			items: this.data.map((item) => createTextFileItem(item)),
			total: this.data.map((item) => !item.isFolder).length,
		};
	}

	getFolder(path: string): StylesheetDBItem | undefined {
		return this.data.find((item) => item.path === path && item.isFolder === true);
	}

	getRules(path: string): Array<RichTextRuleModel> {
		return [
			{
				name: 'bjjh',
				selector: 'h1',
				styles: 'color: blue;',
			},
			{
				name: 'comeone',
				selector: 'h1',
				styles: 'color: blue;',
			},
			{
				name: 'lol',
				selector: 'h1',
				styles: 'color: blue;',
			},
		];
	}

	insertFolder(item: CreateTextFileViewModelBaseModel) {
		const newItem: StylesheetDBItem = {
			...item,
			path: `${item.parentPath}/${item.name}`,
			isFolder: true,
			hasChildren: false,
			type: 'stylesheet',
			icon: 'folder',
		};

		this.insert(newItem);
		return newItem;
	}

	insertStyleSheet(item: CreateTextFileViewModelBaseModel) {
		const newItem: StylesheetDBItem = {
			...item,
			path: `${item.parentPath}/${item.name}.css`,
			isFolder: false,
			hasChildren: false,
			type: 'stylesheet',
			icon: 'style',
		};

		this.insert(newItem);
		return newItem;
	}
}

export const umbStylesheetData = new UmbStylesheetData();
