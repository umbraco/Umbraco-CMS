import type { UmbStaticFileItemModel } from './types.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbItemStoreBase } from '@umbraco-cms/backoffice/store';

/**
 * @class UmbStaticFileItemStore
 * @augments {UmbStoreBase}
 * @description - Data Store for Static File items
 */

export class UmbStaticFileItemStore extends UmbItemStoreBase<UmbStaticFileItemModel> {
	/**
	 * Creates an instance of UmbStaticFileItemStore.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbStaticFileItemStore
	 */
	constructor(host: UmbControllerHost) {
		super(host, UMB_STATIC_FILE_ITEM_STORE_CONTEXT.toString());
	}
}

export default UmbStaticFileItemStore;

export const UMB_STATIC_FILE_ITEM_STORE_CONTEXT = new UmbContextToken<UmbStaticFileItemStore>('UmbStaticFileItemStore');
