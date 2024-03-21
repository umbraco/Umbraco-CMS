import type { UmbRelationTypeDetailModel } from '../../types.js';
import { UmbRelationTypeServerDataSource } from './relation-type-detail.server.data-source.js';
import { UMB_RELATION_TYPE_DETAIL_STORE_CONTEXT } from './relation-type-detail.store.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbDetailRepositoryBase } from '@umbraco-cms/backoffice/repository';

export class UmbRelationTypeDetailRepository extends UmbDetailRepositoryBase<UmbRelationTypeDetailModel> {
	constructor(host: UmbControllerHost) {
		super(host, UmbRelationTypeServerDataSource, UMB_RELATION_TYPE_DETAIL_STORE_CONTEXT);
	}

	async create(model: UmbRelationTypeDetailModel) {
		return super.create(model, null);
	}
}

export default UmbRelationTypeDetailRepository;
