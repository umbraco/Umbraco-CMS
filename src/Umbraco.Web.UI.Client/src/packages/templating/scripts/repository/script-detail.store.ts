import type { UmbScriptDetailModel } from '../types.js';
import { UMB_SCRIPT_DETAIL_STORE_CONTEXT } from './script-detail.store.context-token.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbDetailStoreBase } from '@umbraco-cms/backoffice/store';

/**
 * @class UmbScriptDetailStore
 * @augments {UmbStoreBase}
 * @description - Data Store for scripts
 */
export class UmbScriptDetailStore extends UmbDetailStoreBase<UmbScriptDetailModel> {
	/**
	 * Creates an instance of UmbScriptDetailStore.
	 * @param {UmbControllerHostInterface} host
	 * @memberof UmbScriptDetailStore
	 */
	constructor(host: UmbControllerHost) {
		super(host, UMB_SCRIPT_DETAIL_STORE_CONTEXT.toString());
	}
}

export default UmbScriptDetailStore;
