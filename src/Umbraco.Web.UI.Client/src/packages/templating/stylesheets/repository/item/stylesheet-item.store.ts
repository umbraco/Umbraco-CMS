import type { StylesheetItemResponseModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbFileSystemItemStore } from '@umbraco-cms/backoffice/store';

/**
 * @export
 * @class UmbStylesheetItemStore
 * @extends {UmbFileSystemItemStore}
 * @description - Data Store for Stylesheet items
 */

export class UmbStylesheetItemStore extends UmbFileSystemItemStore<StylesheetItemResponseModel> {
	/**
	 * Creates an instance of UmbStylesheetItemStore.
	 * @param {UmbControllerHostElement} host
	 * @memberof UmbStylesheetItemStore
	 */
	constructor(host: UmbControllerHostElement) {
		super(host, UMB_STYLESHEET_ITEM_STORE_CONTEXT_TOKEN.toString());
	}
}

export const UMB_STYLESHEET_ITEM_STORE_CONTEXT_TOKEN = new UmbContextToken<UmbStylesheetItemStore>(
	'UmbStylesheetItemStore',
);
