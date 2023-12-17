import { UmbData } from '../data.js';
import { createFileSystemTreeItem, createFileItemResponseModelBaseModel, textFileItemMapper } from '../utils.js';
import { UmbMockStylesheetModel, data } from './stylesheet.data.js';
import {
	CreateTextFileViewModelBaseModel,
	ExtractRichTextStylesheetRulesRequestModel,
	ExtractRichTextStylesheetRulesResponseModel,
	FileSystemTreeItemPresentationModel,
	InterpolateRichTextStylesheetRequestModel,
	PagedFileSystemTreeItemPresentationModel,
	PagedStylesheetOverviewResponseModel,
	StylesheetItemResponseModel,
	StylesheetResponseModel,
	UpdateStylesheetRequestModel,
} from '@umbraco-cms/backoffice/backend-api';

//prettier-ignore
// eslint-disable-next-line no-useless-escape

// Temp mocked database
// TODO: all properties are optional in the server schema. I don't think this is correct.
// eslint-disable-next-line @typescript-eslint/ban-ts-comment
// @ts-ignore
class UmbStylesheetData extends UmbData<UmbMockStylesheetModel> {
	constructor(data: Array<UmbMockStylesheetModel>) {
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

	getItems(paths: Array<string>): Array<StylesheetItemResponseModel> {
		const items = this.data.filter((item) => paths.includes(item.path ?? ''));
		return items.map((item) => createFileItemResponseModelBaseModel(item));
	}

	getStylesheet(path: string): StylesheetResponseModel | undefined {
		return textFileItemMapper(this.data.find((item) => item.path === path));
	}

	getAllStylesheets(): PagedStylesheetOverviewResponseModel {
		return {
			items: this.data.map((item) => textFileItemMapper(item)),
			total: this.data.map((item) => !item.isFolder).length,
		};
	}

	getFolder(path: string): UmbMockStylesheetModel | undefined {
		return this.data.find((item) => item.path === path && item.isFolder === true);
	}

	getRules(path: string): ExtractRichTextStylesheetRulesResponseModel {
		const regex = /\*\*\s*umb_name:\s*(?<name>[^*\r\n]*?)\s*\*\/\s*(?<selector>[^,{]*?)\s*{\s*(?<styles>.*?)\s*}/gis;
		const item = this.data.find((item) => item.path === path);
		if (!item) throw Error('item not found');

		// eslint-disable-next-line @typescript-eslint/ban-ts-comment
		//@ts-ignore
		// eslint-disable-next-line no-unsafe-optional-chaining
		const rules = [...item.content?.matchAll(regex)].map((match) => match.groups) as Array<RichTextRuleModel>;
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

		const newContent = `${cleanedContent.replace(/[\r\n]+$/, '')}

${rules
	?.map(
		(rule) =>
			`/**umb_name:${rule.name}*/
${rule.selector} {
	${rule.styles}
}

`,
	)
	.join('')}`;

		return { content: newContent };
	}

	insertFolder(item: CreateTextFileViewModelBaseModel) {
		const newItem: UmbMockStylesheetModel = {
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
		const parentPath = item.parentPath ? `${item.parentPath}/` : '';
		const newItem: UmbMockStylesheetModel = {
			...item,
			path: `${parentPath}${item.name}`,
			isFolder: false,
			hasChildren: false,
			type: 'stylesheet',
			icon: 'style',
		};

		this.insert(newItem);
		return newItem;
	}

	insert(item: UmbMockStylesheetModel) {
		const exits = this.data.find((i) => i.path === item.path);

		if (exits) {
			throw new Error(`Item with path ${item.path} already exists`);
		}

		this.data.push(item);

		return item;
	}

	updateData(updateItem: UpdateStylesheetRequestModel) {
		const itemIndex = this.data.findIndex((item) => item.path === updateItem.existingPath);
		const item = this.data[itemIndex];
		if (!item) return;

		// TODO: revisit this code, seems like something we can solve smarter/type safer now:
		const itemKeys = Object.keys(item);
		const newItem = { ...item };

		for (const [key] of Object.entries(updateItem)) {
			if (itemKeys.indexOf(key) !== -1) {
				// eslint-disable-next-line @typescript-eslint/ban-ts-comment
				// @ts-ignore
				newItem[key] = updateItem[key];
			}
		}
		// Specific to fileSystem, we need to update path based on name:
		const dirName = updateItem.existingPath?.substring(0, updateItem.existingPath.lastIndexOf('/'));
		newItem.path = `${dirName}${dirName ? '/' : ''}${updateItem.name}`;

		// eslint-disable-next-line @typescript-eslint/ban-ts-comment
		// @ts-ignore
		this.data[itemIndex] = newItem;
	}

	delete(paths: Array<string>) {
		const deletedPaths = this.data
			.filter((item) => {
				if (!item.path) throw new Error('Item has no path');
				paths.includes(item.path);
			})
			.map((item) => item.path);

		this.data = this.data.filter((item) => {
			if (!item.path) throw new Error('Item has no path');
			paths.indexOf(item.path) === -1;
		});

		return deletedPaths;
	}
}

export const umbStylesheetData = new UmbStylesheetData(data);
