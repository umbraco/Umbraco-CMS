import type { UmbDocumentBlueprintDetailModel } from '../../types.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbDetailStoreBase } from '@umbraco-cms/backoffice/store';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

/**
 * @export
 * @class UmbDocumentBlueprintDetailStore
 * @extends {UmbStoreBase}
 * @description - Data Store for Document Blueprint Details
 */
export class UmbDocumentBlueprintDetailStore extends UmbDetailStoreBase<UmbDocumentBlueprintDetailModel> {
	/**
	 * Creates an instance of UmbDocumentBlueprintDetailStore.
	 * @param {UmbControllerHost} host
	 * @memberof UmbDocumentBlueprintDetailStore
	 */
	constructor(host: UmbControllerHost) {
		super(host, UMB_DOCUMENT_BLUEPRINT_DETAIL_STORE_CONTEXT.toString());
	}
}

export { UmbDocumentBlueprintDetailStore as api };

export const UMB_DOCUMENT_BLUEPRINT_DETAIL_STORE_CONTEXT = new UmbContextToken<UmbDocumentBlueprintDetailStore>(
	'UmbDocumentBlueprintDetailStore',
);
