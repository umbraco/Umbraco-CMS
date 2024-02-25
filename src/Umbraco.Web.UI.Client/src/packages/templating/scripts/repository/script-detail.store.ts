import type { UmbScriptDetailModel } from '../types.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbDetailStoreBase } from '@umbraco-cms/backoffice/store';

/**
 * @export
 * @class UmbScriptDetailStore
 * @extends {UmbStoreBase}
 * @description - Data Store for scripts
 */
export class UmbScriptDetailStore extends UmbDetailStoreBase<UmbScriptDetailModel> {
	/**
	 * Creates an instance of UmbScriptDetailStore.
	 * @param {UmbControllerHostInterface} host
	 * @memberof UmbScriptDetailStore
	 */
	constructor(host: UmbControllerHostElement) {
		super(host, UMB_SCRIPT_DETAIL_STORE_CONTEXT.toString());
	}
}

export default UmbScriptDetailStore;

export const UMB_SCRIPT_DETAIL_STORE_CONTEXT = new UmbContextToken<UmbScriptDetailStore>('UmbScriptDetailStore');
