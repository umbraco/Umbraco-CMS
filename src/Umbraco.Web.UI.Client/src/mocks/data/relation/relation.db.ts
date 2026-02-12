import { UmbMockEntityDetailManager } from '../utils/entity/entity-detail.manager.js';
import { UmbEntityMockDbBase } from '../utils/entity/entity-base.js';
import type { UmbMockRelationModel } from './relation.data.js';
import { data } from './relation.data.js';
import type { RelationResponseModel } from '@umbraco-cms/backoffice/external/backend-api';

class UmbRelationMockDB extends UmbEntityMockDbBase<UmbMockRelationModel> {
	item = new UmbMockEntityDetailManager<UmbMockRelationModel>(this, itemResponseMapper, createDetailMockMapper);

	constructor(data: Array<UmbMockRelationModel>) {
		super(data);
	}
}

const createDetailMockMapper = (): UmbMockRelationModel => {
	throw new Error('Not possible to create a relation');
};

const itemResponseMapper = (item: UmbMockRelationModel): RelationResponseModel => {
	return {
		id: item.id,
		child: item.child,
		createDate: item.createDate,
		parent: item.parent,
		relationType: item.relationType,
		comment: item.comment,
	};
};

export const umbRelationMockDb = new UmbRelationMockDB(data);
