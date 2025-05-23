import { UmbEntityMockDbBase } from '../utils/entity/entity-base.js';
import { UmbMockEntityTreeManager } from '../utils/entity/entity-tree.manager.js';
import { UmbMockEntityItemManager } from '../utils/entity/entity-item.manager.js';
import type { UmbMockTemplateModel } from './template.data.js';
import { data } from './template.data.js';
import { UmbMockTemplateDetailManager } from './template-detail.manager.js';
import { UmbMockTemplateQueryManager } from './template-query.manager.js';
import type {
	NamedEntityTreeItemResponseModel,
	TemplateItemResponseModel,
} from '@umbraco-cms/backoffice/external/backend-api';

class UmbTemplateMockDB extends UmbEntityMockDbBase<UmbMockTemplateModel> {
	tree = new UmbMockEntityTreeManager<UmbMockTemplateModel>(this, treeItemMapper);
	item = new UmbMockEntityItemManager<UmbMockTemplateModel>(this, itemMapper);
	detail = new UmbMockTemplateDetailManager(this);
	query = new UmbMockTemplateQueryManager();

	constructor(data: Array<UmbMockTemplateModel>) {
		super(data);
	}
}

const treeItemMapper = (model: UmbMockTemplateModel): NamedEntityTreeItemResponseModel => {
	return {
		name: model.name,
		hasChildren: model.hasChildren,
		id: model.id,
		parent: model.parent,
	};
};

const itemMapper = (item: UmbMockTemplateModel): TemplateItemResponseModel => {
	return {
		id: item.id,
		name: item.name,
		alias: item.alias,
	};
};

export const umbTemplateMockDb = new UmbTemplateMockDB(data);
