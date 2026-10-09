import type { UmbMockRelationModel } from '../data/mock-data-set.types.js';
import { umbDocumentMockDb } from './document.db.js';
import { umbMediaMockDb } from './media.db.js';
import { umbMemberMockDb } from './member.db.js';
import { UmbMockEntityDetailManager } from './utils/entity/entity-detail.manager.js';
import { UmbEntityMockDbBase } from './utils/entity/entity-base.js';
import type { RelationResponseModel } from '@umbraco-cms/backoffice/external/backend-api';

class UmbRelationMockDB extends UmbEntityMockDbBase<UmbMockRelationModel> {
	item = new UmbMockEntityDetailManager<UmbMockRelationModel>(this, itemResponseMapper, createDetailMockMapper);

	constructor(data: Array<UmbMockRelationModel>) {
		super('relation', data);
	}

	getByRelationType(relationTypeId: string, { skip = 0, take = 100 }: { skip?: number; take?: number } = {}) {
		const relations = this.data.filter((item) => item.relationType.id === relationTypeId);
		return { items: relations.slice(skip, skip + take).map(itemResponseMapper), total: relations.length };
	}
}

const createDetailMockMapper = (): UmbMockRelationModel => {
	throw new Error('Not possible to create a relation');
};

const relatedEntityReference = (id: string) => {
	const entity = umbDocumentMockDb.read(id) ?? umbMediaMockDb.read(id) ?? umbMemberMockDb.read(id);
	return { id, name: entity?.variants[0]?.name ?? null };
};

const itemResponseMapper = (item: UmbMockRelationModel): RelationResponseModel => {
	return {
		id: item.id,
		child: relatedEntityReference(item.child.id),
		createDate: item.createDate,
		parent: relatedEntityReference(item.parent.id),
		relationType: item.relationType,
		comment: item.comment,
	};
};

export const umbRelationMockDb = new UmbRelationMockDB([]);
