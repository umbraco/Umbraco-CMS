import { UmbEntityData } from './entity.data.js';
import { createFileSystemTreeItem, createFileItemResponseModelBaseModel, createTextFileItem } from './utils.js';
import {
	CreateTextFileViewModelBaseModel,
	ExtractRichTextStylesheetRulesRequestModel,
	ExtractRichTextStylesheetRulesResponseModel,
	FileSystemTreeItemPresentationModel,
	InterpolateRichTextStylesheetRequestModel,
	PagedFileSystemTreeItemPresentationModel,
	PagedStylesheetOverviewResponseModel,
	RichTextRuleModel,
	StylesheetResponseModel,
} from '@umbraco-cms/backoffice/backend-api';

//prettier-ignore
// eslint-disable-next-line no-useless-escape

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
		
		/**umb_name:HELLO*/
		h1 {
			color: green;
		}
		
		/**umb_name:SOMETHING*/
		h1 {
			color: green;
		}
		
		/**umb_name:NIOCE*/
		h1 {
			color: green;
		}`,
	},
	{
		path: 'Folder 1',
		name: 'Folder 1',
		isFolder: true,
		icon: 'folder',
		type: 'stylesheet',
		hasChildren: true,
	},
	{
		path: 'Folder 1/Stylesheet File 3.css',
		name: 'Stylesheet File 3.css',
		type: 'stylesheet',
		hasChildren: true,
		isFolder: false,
		content: `		h1 {
			color: pink;
		}
		
		/**umb_name:ONE*/
		h1 {
			color: pink;
		}
		
		/**umb_name:TWO*/
		h1 {
			color: pink;
		}
		
		/**umb_name:THREE*/
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

	getRules(path: string): ExtractRichTextStylesheetRulesResponseModel {
		const regex = /\*\*\s*umb_name:\s*(?<name>[^*\r\n]*?)\s*\*\/\s*(?<selector>[^,{]*?)\s*{\s*(?<styles>.*?)\s*}/gis;
		const item = this.data.find((item) => item.path === path);
		if (!item) throw Error('item not found');

		// eslint-disable-next-line @typescript-eslint/ban-ts-comment
		//@ts-ignore
		// eslint-disable-next-line no-unsafe-optional-chaining
		const rules = [...item.content?.matchAll(regex)].map((match) => match.groups);
		return { rules };
	}

	async extractRules({ requestBody }: { requestBody?: ExtractRichTextStylesheetRulesRequestModel }) {
		const regex = /\*\*\s*umb_name:\s*(?<name>[^*\r\n]*?)\s*\*\/\s*(?<selector>[^,{]*?)\s*{\s*(?<styles>.*?)\s*}/gis;

		if (!requestBody) {
			throw Error('No request body');
		}
		const { content } = await requestBody;
		if (!content) return { rules: [] };
		const rules = [...content.matchAll(regex)].map((match) => match.groups);
		return { rules };
	}

	interpolateRules({ requestBody }: { requestBody?: InterpolateRichTextStylesheetRequestModel }) {
		const regex = /\/\*\*\s*umb_name:\s*(?<name>[^*\r\n]*?)\s*\*\/\s*(?<selector>[^,{]*?)\s*{\s*(?<styles>.*?)\s*}/gis;
		if (!requestBody) {
			throw Error('No request body');
		}
		const { content, rules } = requestBody;
		if (!content && !rules) return { content: '' };

		// eslint-disable-next-line @typescript-eslint/ban-ts-comment
		//@ts-ignore
		const cleanedContent = content?.replaceAll(regex, '');

		const newContent = rules
			?.map(
				(rule) =>
					`/**umb_name:${rule.name}*/
		${rule.selector} {
			${rule.styles}
		}
		${cleanedContent}	
		`,
			)
			.join('');

		return { content: newContent };
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
