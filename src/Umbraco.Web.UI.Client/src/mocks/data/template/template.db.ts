import { UmbEntityMockDbBase } from '../utils/entity/entity-base.js';
import { UmbMockEntityTreeManager } from '../utils/entity/entity-tree.manager.js';
import { folderTreeItemMapper } from '../utils.js';
import { UmbMockEntityItemManager } from '../utils/entity/entity-item.manager.js';
import type { UmbMockTemplateModel } from './template.data.js';
import { data } from './template.data.js';
import { UmbMockTemplateDetailManager } from './template-detail.manager.js';
import { UmbMockTemplateQueryManager } from './template-query.manager.js';
import type { TemplateItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';

class UmbTemplateMockDB extends UmbEntityMockDbBase<UmbMockTemplateModel> {
	tree = new UmbMockEntityTreeManager<UmbMockTemplateModel>(this, folderTreeItemMapper);
	item = new UmbMockEntityItemManager<UmbMockTemplateModel>(this, itemMapper);
	detail = new UmbMockTemplateDetailManager(this);
	query = new UmbMockTemplateQueryManager(this);

	constructor(data: Array<UmbMockTemplateModel>) {
		super(data);
	}
}

const itemMapper = (item: UmbMockTemplateModel): TemplateItemResponseModel => {
	return {
		id: item.id,
		name: item.name,
		alias: item.alias,
	};
};

export const umbTemplateMockDb = new UmbTemplateMockDB(data);
