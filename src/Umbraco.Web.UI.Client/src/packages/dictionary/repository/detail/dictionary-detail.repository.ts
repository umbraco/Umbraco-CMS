import type { UmbDictionaryDetailModel } from '../../types.js';
import { UmbDictionaryServerDataSource } from './dictionary-detail.server.data-source.js';
import { UMB_DICTIONARY_DETAIL_STORE_CONTEXT } from './dictionary-detail.store.context-token.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbDetailRepositoryBase } from '@umbraco-cms/backoffice/repository';

export class UmbDictionaryDetailRepository extends UmbDetailRepositoryBase<UmbDictionaryDetailModel> {
	constructor(host: UmbControllerHost) {
		super(host, UmbDictionaryServerDataSource, UMB_DICTIONARY_DETAIL_STORE_CONTEXT);
	}
}

export { UmbDictionaryDetailRepository as api };
