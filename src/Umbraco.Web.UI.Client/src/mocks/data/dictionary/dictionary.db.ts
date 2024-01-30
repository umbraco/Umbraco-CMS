import { UmbMockEntityTreeManager } from '../utils/entity/entity-tree.manager.js';
import { UmbMockEntityItemManager } from '../utils/entity/entity-item.manager.js';
import { UmbMockEntityDetailManager } from '../utils/entity/entity-detail.manager.js';
import { UmbEntityMockDbBase } from '../utils/entity/entity-base.js';
import type { UmbMockDictionaryModel } from './dictionary.data.js';
import { data } from './dictionary.data.js';
import type {
	NamedEntityTreeItemResponseModel,
	CreateDictionaryItemRequestModel,
	DictionaryItemResponseModel,
	DictionaryItemItemResponseModel,
} from '@umbraco-cms/backoffice/backend-api';
import { UmbId } from '@umbraco-cms/backoffice/id';

export class UmbDictionaryMockDB extends UmbEntityMockDbBase<UmbMockDictionaryModel> {
	tree = new UmbMockEntityTreeManager<UmbMockDictionaryModel>(this, treeItemMapper);
	item = new UmbMockEntityItemManager<UmbMockDictionaryModel>(this, itemMapper);
	detail = new UmbMockEntityDetailManager<UmbMockDictionaryModel>(this, createMockMapper, detailResponseMapper);

	constructor(data: Array<UmbMockDictionaryModel>) {
		super(data);
	}
}

const treeItemMapper = (model: UmbMockDictionaryModel): Omit<NamedEntityTreeItemResponseModel, 'type'> => {
	return {
		name: model.name,
		id: model.id,
		parent: model.parent,
		hasChildren: model.hasChildren,
	};
};

const createMockMapper = (request: CreateDictionaryItemRequestModel): UmbMockDictionaryModel => {
	return {
		name: request.name,
		id: request.id ? request.id : UmbId.new(),
		parent: request.parentId ? { id: request.parentId } : null,
		translations: request.translations,
		hasChildren: false,
	};
};

const detailResponseMapper = (model: UmbMockDictionaryModel): DictionaryItemResponseModel => {
	return {
		name: model.name,
		id: model.id,
		translations: model.translations,
	};
};

const itemMapper = (model: UmbMockDictionaryModel): DictionaryItemItemResponseModel => {
	return {
		name: model.name,
		id: model.id,
	};
};

export const umbDictionaryMockDb = new UmbDictionaryMockDB(data);
