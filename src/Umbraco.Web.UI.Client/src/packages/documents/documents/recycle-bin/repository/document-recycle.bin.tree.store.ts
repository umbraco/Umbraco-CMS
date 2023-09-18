import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbEntityTreeStore } from '@umbraco-cms/backoffice/store';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';

/**
 * @export
 * @class UmbDocumentRecycleBinTreeStore
 * @extends {UmbEntityTreeStore}
 * @description - Tree Data Store for the Document Recycle Bin
 */
export class UmbDocumentRecycleBinTreeStore extends UmbEntityTreeStore {
	/**
	 * Creates an instance of UmbDocumentRecycleBinTreeStore.
	 * @param {UmbControllerHostElement} host
	 * @memberof UmbDocumentRecycleBinTreeStore
	 */
	constructor(host: UmbControllerHostElement) {
		super(host, UMB_DOCUMENT_RECYCLE_BIN_TREE_STORE_CONTEXT.toString());
	}
}

export const UMB_DOCUMENT_RECYCLE_BIN_TREE_STORE_CONTEXT = new UmbContextToken<UmbDocumentRecycleBinTreeStore>(
	'UmbDocumentRecycleBinTreeStore',
);
