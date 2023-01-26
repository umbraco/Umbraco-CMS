import { v4 as uuid } from 'uuid';
import { UmbEntityData } from './entity.data';
import { createEntityTreeItem } from './utils';
import {
	EntityTreeItem,
	PagedEntityTreeItem,
	Template,
	TemplateCreateModel,
	TemplateScaffold,
} from '@umbraco-cms/backend-api';

type TemplateDBItem = Template & EntityTreeItem;

const createTemplate = (dbItem: TemplateDBItem): Template => {
	return {
		key: dbItem.key,
		name: dbItem.name,
		alias: dbItem.alias,
		content: dbItem.content,
	};
};

export const data: Array<TemplateDBItem> = [
	{
		key: '2bf464b6-3aca-4388-b043-4eb439cc2643',
		isContainer: false,
		parentKey: null,
		name: 'Doc 1',
		type: 'template',
		icon: 'icon-layout',
		hasChildren: false,
		alias: 'Doc1',
		content:
			'@using Umbraco.Cms.Web.Common.PublishedModels;\n@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage<ContentModels.Doc1>\r\n@using ContentModels = Umbraco.Cms.Web.Common.PublishedModels;\r\n@{\r\n\tLayout = null;\r\n}',
	},
	{
		key: '9a84c0b3-03b4-4dd4-84ac-706740ac0f71',
		isContainer: false,
		parentKey: null,
		name: 'Test',
		type: 'template',
		icon: 'icon-layout',
		hasChildren: false,
		alias: 'Test',
		content:
			'@using Umbraco.Cms.Web.Common.PublishedModels;\n@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage<ContentModels.Test>\r\n@using ContentModels = Umbraco.Cms.Web.Common.PublishedModels;\r\n@{\r\n\tLayout = null;\r\n}',
	},
];

export const createTemplateScaffold = (masterTemplateAlias: string) => {
	return `Template Scaffold Mock for master template: ${masterTemplateAlias}`;
};

// Temp mocked database
// TODO: all properties are optional in the server schema. I don't think this is correct.
// eslint-disable-next-line @typescript-eslint/ban-ts-comment
// @ts-ignore
class UmbTemplateData extends UmbEntityData<TemplateDBItem> {
	constructor() {
		super(data);
	}

	getByKey(key: string): Template | undefined {
		const item = this.data.find((item) => item.key === key);
		return item ? createTemplate(item) : undefined;
	}

	getScaffold(masterTemplateAlias: string): TemplateScaffold {
		return {
			content: `Template Scaffold Mock: Layout = ${masterTemplateAlias || null};`,
		};
	}

	create(templateData: TemplateCreateModel) {
		const template = {
			key: uuid(),
			...templateData,
		};
		this.data.push(template);
		return template;
	}

	update(template: Template) {
		this.updateData(template);
		return template;
	}

	getTreeRoot(): PagedEntityTreeItem {
		const items = this.data.filter((item) => item.parentKey === null);
		const treeItems = items.map((item) => createEntityTreeItem(item));
		const total = items.length;
		return { items: treeItems, total };
	}

	getTreeItemChildren(key: string): PagedEntityTreeItem {
		const items = this.data.filter((item) => item.parentKey === key);
		const treeItems = items.map((item) => createEntityTreeItem(item));
		const total = items.length;
		return { items: treeItems, total };
	}

	getTreeItem(keys: Array<string>): Array<EntityTreeItem> {
		const items = this.data.filter((item) => keys.includes(item.key ?? ''));
		return items.map((item) => createEntityTreeItem(item));
	}
}

export const umbTemplateData = new UmbTemplateData();
