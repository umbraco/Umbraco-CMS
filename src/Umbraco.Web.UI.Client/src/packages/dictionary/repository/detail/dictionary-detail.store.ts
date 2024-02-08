import type { UmbDictionaryDetailModel } from '../../types.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbDetailStoreBase } from '@umbraco-cms/backoffice/store';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';

/**
 * @export
 * @class UmbDictionaryDetailStore
 * @extends {UmbStoreBase}
 * @description - Data Store for Dictionary Details
 */
export class UmbDictionaryDetailStore extends UmbDetailStoreBase<UmbDictionaryDetailModel> {
	/**
	 * Creates an instance of UmbDictionaryDetailStore.
	 * @param {UmbControllerHostElement} host
	 * @memberof UmbDictionaryDetailStore
	 */
	constructor(host: UmbControllerHostElement) {
		super(host, UMB_DICTIONARY_DETAIL_STORE_CONTEXT.toString());
	}
}

export const UMB_DICTIONARY_DETAIL_STORE_CONTEXT = new UmbContextToken<UmbDictionaryDetailStore>(
	'UmbDictionaryDetailStore',
);
