import type { UmbDocumentTypeDetailModel } from '../../types.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbDetailStoreBase } from '@umbraco-cms/backoffice/store';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';

/**
 * @export
 * @class UmbDocumentTypeStore
 * @extends {UmbStoreBase}
 * @description - Data Store for Document Types
 */
export class UmbDocumentTypeDetailStore extends UmbDetailStoreBase<UmbDocumentTypeDetailModel> {
	/**
	 * Creates an instance of UmbDocumentTypeStore.
	 * @param {UmbControllerHostElement} host
	 * @memberof UmbDocumentTypeStore
	 */
	constructor(host: UmbControllerHostElement) {
		super(host, UMB_DOCUMENT_TYPE_DETAIL_STORE_CONTEXT.toString());
	}
}

export const UMB_DOCUMENT_TYPE_DETAIL_STORE_CONTEXT = new UmbContextToken<UmbDocumentTypeDetailStore>(
	'UmbDocumentTypeDetailStore',
);
