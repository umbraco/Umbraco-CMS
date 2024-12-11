import { UMB_STYLESHEET_FOLDER_STORE_CONTEXT } from './stylesheet-folder.store.context-token.js';
import { UmbDetailStoreBase } from '@umbraco-cms/backoffice/store';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbFolderModel } from '@umbraco-cms/backoffice/tree';

/**
 * @class UmbStylesheetFolderStore
 * @augments {UmbStoreBase}
 * @description - Data Store for Stylesheet Folders
 */
export class UmbStylesheetFolderStore extends UmbDetailStoreBase<UmbFolderModel> {
	/**
	 * Creates an instance of UmbStylesheetFolderStore.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbStylesheetFolderStore
	 */
	constructor(host: UmbControllerHost) {
		super(host, UMB_STYLESHEET_FOLDER_STORE_CONTEXT.toString());
	}
}

export { UmbStylesheetFolderStore as api };
