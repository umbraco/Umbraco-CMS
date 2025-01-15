import { UmbMockEntityDetailManager } from '../utils/entity/entity-detail.manager.js';
import { UmbEntityMockDbBase } from '../utils/entity/entity-base.js';
import type { UmbMockRelationTypeItemModel, UmbMockRelationTypeModel } from './relationType.data.js';
import { data } from './relationType.data.js';
import type {
	RelationTypeItemResponseModel,
	RelationTypeResponseModel,
} from '@umbraco-cms/backoffice/external/backend-api';

class UmbRelationTypeMockDB extends UmbEntityMockDbBase<UmbMockRelationTypeModel> {
	item = new UmbMockEntityDetailManager<UmbMockRelationTypeItemModel>(this, itemResponseMapper, createDetailMockMapper);
	detail = new UmbMockEntityDetailManager<UmbMockRelationTypeModel>(this, createDetailMockMapper, detailResponseMapper);

	constructor(data: Array<UmbMockRelationTypeModel>) {
		super(data);
	}
}

const createDetailMockMapper = (): UmbMockRelationTypeModel => {
	throw new Error('Not possible to create a relation type');
};

const detailResponseMapper = (item: UmbMockRelationTypeModel): RelationTypeResponseModel => {
	return {
		id: item.id,
		isBidirectional: item.isBidirectional,
		isDependency: item.isDependency,
		name: item.name,
		alias: item.alias,
		childObject: item.childObject,
		parentObject: item.parentObject,
	};
};

const itemResponseMapper = (item: UmbMockRelationTypeItemModel): RelationTypeItemResponseModel => {
	return {
		id: item.id,
		name: item.name,
		isDeletable: item.isDeletable,
	};
};

export const umbRelationTypeMockDb = new UmbRelationTypeMockDB(data);
