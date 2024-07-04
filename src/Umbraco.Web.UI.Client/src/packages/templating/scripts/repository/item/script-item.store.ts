import type { UmbScriptItemModel } from '../../types.js';
import { UMB_SCRIPT_ITEM_STORE_CONTEXT } from './script-item.store.context-token.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbItemStoreBase } from '@umbraco-cms/backoffice/store';

/**
 * @export
 * @class UmbScriptItemStore
 * @extends {UmbItemStoreBase}
 * @description - Data Store for Script items
 */

export class UmbScriptItemStore extends UmbItemStoreBase<UmbScriptItemModel> {
	/**
	 * Creates an instance of UmbScriptItemStore.
	 * @param {UmbControllerHost} host
	 * @memberof UmbScriptItemStore
	 */
	constructor(host: UmbControllerHost) {
		super(host, UMB_SCRIPT_ITEM_STORE_CONTEXT.toString());
	}
}

export default UmbScriptItemStore;
