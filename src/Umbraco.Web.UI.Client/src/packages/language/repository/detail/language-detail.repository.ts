import type { UmbLanguageDetailModel } from '../../types.js';
import { UmbLanguageServerDataSource } from './language-detail.server.data-source.js';
import { UMB_LANGUAGE_DETAIL_STORE_CONTEXT } from './language-detail.store.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbDetailRepositoryBase } from '@umbraco-cms/backoffice/repository';

export class UmbLanguageDetailRepository extends UmbDetailRepositoryBase<UmbLanguageDetailModel> {
	constructor(host: UmbControllerHost) {
		super(host, UmbLanguageServerDataSource, UMB_LANGUAGE_DETAIL_STORE_CONTEXT);
	}

	async create(model: UmbLanguageDetailModel) {
		return super.create(model, null);
	}
}
