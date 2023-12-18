import { UmbFileSystemMockDbBase } from '../file-system/file-system-base.js';
import { UmbMockFileSystemDetailManager } from '../file-system/file-system-detail.manager.js';
import { UmbMockFileSystemFolderManager } from '../file-system/file-system-folder.manager.js';
import { UmbMockFileSystemItemManager } from '../file-system/file-system-item.manager.js';
import { UmbMockFileSystemTreeManager } from '../file-system/file-system-tree.manager.js';
import { textFileItemMapper } from '../utils.js';
import { UmbMockStylesheetModel, data } from './stylesheet.data.js';
import {
	CreateStylesheetRequestModel,
	ExtractRichTextStylesheetRulesRequestModel,
	ExtractRichTextStylesheetRulesResponseModel,
	InterpolateRichTextStylesheetRequestModel,
	PagedStylesheetOverviewResponseModel,
	StylesheetResponseModel,
} from '@umbraco-cms/backoffice/backend-api';

class UmbStylesheetData extends UmbFileSystemMockDbBase<UmbMockStylesheetModel> {
	tree = new UmbMockFileSystemTreeManager<UmbMockStylesheetModel>(this);
	item = new UmbMockFileSystemItemManager<UmbMockStylesheetModel>(this);
	folder = new UmbMockFileSystemFolderManager<UmbMockStylesheetModel>(this);
	file;

	constructor(data: Array<UmbMockStylesheetModel>) {
		super(data);

		this.file = new UmbMockFileSystemDetailManager<UmbMockStylesheetModel>(this, {
			createMapper: this.#createStylesheetMockItem,
			readMapper: this.#readStylesheetResponseMapper,
		});
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

	#createStylesheetMockItem = (item: CreateStylesheetRequestModel): UmbMockStylesheetModel => {
		return {
			name: item.name,
			content: item.content,
			path: `${item.parentPath}` ? `${item.parentPath}/${item.name}` : item.name,
			isFolder: false,
			hasChildren: false,
			type: 'stylesheet',
		};
	};

	#readStylesheetResponseMapper = (item: UmbMockStylesheetModel): StylesheetResponseModel => {
		return {
			path: item.path,
			name: item.name,
			content: item.content,
		};
	};
}

export const umbStylesheetData = new UmbStylesheetData(data);
