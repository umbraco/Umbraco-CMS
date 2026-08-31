import type { UmbScriptDetailModel } from '../types.js';
import { UMB_SCRIPT_DETAIL_STORE_CONTEXT } from './script-detail.store.context-token.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbDetailStoreBase } from '@umbraco-cms/backoffice/store';

/**
 * @class UmbScriptDetailStore
 * @augments {UmbDetailStoreBase}
 * @description - Data Store for scripts
 */
export class UmbScriptDetailStore extends UmbDetailStoreBase<UmbScriptDetailModel> {
	/**
	 * Creates an instance of UmbScriptDetailStore.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbScriptDetailStore
	 */
	constructor(host: UmbControllerHost) {
		super(host, UMB_SCRIPT_DETAIL_STORE_CONTEXT);
	}
}

export default UmbScriptDetailStore;
