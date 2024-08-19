import type { UmbDocumentBlueprintDetailModel } from '../../types.js';
import { UMB_DOCUMENT_BLUEPRINT_DETAIL_STORE_CONTEXT } from './document-blueprint-detail.store.context-token.js';
import { UmbDetailStoreBase } from '@umbraco-cms/backoffice/store';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

/**
 * @class UmbDocumentBlueprintDetailStore
 * @augments {UmbStoreBase}
 * @description - Data Store for Document Blueprint Details
 */
export class UmbDocumentBlueprintDetailStore extends UmbDetailStoreBase<UmbDocumentBlueprintDetailModel> {
	/**
	 * Creates an instance of UmbDocumentBlueprintDetailStore.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbDocumentBlueprintDetailStore
	 */
	constructor(host: UmbControllerHost) {
		super(host, UMB_DOCUMENT_BLUEPRINT_DETAIL_STORE_CONTEXT.toString());
	}
}

export { UmbDocumentBlueprintDetailStore as api };
