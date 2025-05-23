import type { UmbDictionaryDetailModel } from '../../types.js';
import { UMB_DICTIONARY_DETAIL_STORE_CONTEXT } from './dictionary-detail.store.context-token.js';
import { UmbDetailStoreBase } from '@umbraco-cms/backoffice/store';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

/**
 * @class UmbDictionaryDetailStore
 * @augments {UmbStoreBase}
 * @description - Data Store for Dictionary Details
 */
export class UmbDictionaryDetailStore extends UmbDetailStoreBase<UmbDictionaryDetailModel> {
	/**
	 * Creates an instance of UmbDictionaryDetailStore.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbDictionaryDetailStore
	 */
	constructor(host: UmbControllerHost) {
		super(host, UMB_DICTIONARY_DETAIL_STORE_CONTEXT.toString());
	}
}

export { UmbDictionaryDetailStore as api };
