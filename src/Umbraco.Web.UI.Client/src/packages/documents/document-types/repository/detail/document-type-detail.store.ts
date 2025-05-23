import type { UmbDocumentTypeDetailModel } from '../../types.js';
import { UMB_DOCUMENT_TYPE_DETAIL_STORE_CONTEXT } from './document-type-detail.store.context-token.js';
import { UmbDetailStoreBase } from '@umbraco-cms/backoffice/store';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

/**
 * @class UmbDocumentTypeStore
 * @augments {UmbStoreBase}
 * @description - Data Store for Document Types
 */
export class UmbDocumentTypeDetailStore extends UmbDetailStoreBase<UmbDocumentTypeDetailModel> {
	/**
	 * Creates an instance of UmbDocumentTypeStore.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbDocumentTypeStore
	 */
	constructor(host: UmbControllerHost) {
		super(host, UMB_DOCUMENT_TYPE_DETAIL_STORE_CONTEXT.toString());
	}
}

export { UmbDocumentTypeDetailStore as api };
