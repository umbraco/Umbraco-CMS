import type { UmbDocumentBlueprintDetailModel } from '../../types.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbItemStoreBase } from '@umbraco-cms/backoffice/store';

/**
 * @export
 * @class UmbDocumentBlueprintItemStore
 * @extends {UmbStoreBase}
 * @description - Data Store for Document items
 */

export class UmbDocumentBlueprintItemStore extends UmbItemStoreBase<UmbDocumentBlueprintDetailModel> {
	/**
	 * Creates an instance of UmbDocumentBlueprintItemStore.
	 * @param {UmbControllerHost} host
	 * @memberof UmbDocumentBlueprintItemStore
	 */
	constructor(host: UmbControllerHost) {
		super(host, UMB_DOCUMENT_BLUEPRINT_ITEM_STORE_CONTEXT.toString());
	}
}

export { UmbDocumentBlueprintItemStore as api };

export const UMB_DOCUMENT_BLUEPRINT_ITEM_STORE_CONTEXT = new UmbContextToken<UmbDocumentBlueprintItemStore>(
	'UmbDocumentBlueprintItemStore',
);
