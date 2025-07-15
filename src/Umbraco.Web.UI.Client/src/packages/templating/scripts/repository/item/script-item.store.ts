import type { UmbScriptItemModel } from '../../types.js';
import { UMB_SCRIPT_ITEM_STORE_CONTEXT } from './script-item.store.context-token.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbItemStoreBase } from '@umbraco-cms/backoffice/store';

/**
 * @class UmbScriptItemStore
 * @augments {UmbItemStoreBase}
 * @description - Data Store for Script items
 */

export class UmbScriptItemStore extends UmbItemStoreBase<UmbScriptItemModel> {
	/**
	 * Creates an instance of UmbScriptItemStore.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbScriptItemStore
	 */
	constructor(host: UmbControllerHost) {
		super(host, UMB_SCRIPT_ITEM_STORE_CONTEXT.toString());
	}
}

export default UmbScriptItemStore;
