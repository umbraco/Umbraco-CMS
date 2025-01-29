import { UMB_SCRIPT_FOLDER_STORE_CONTEXT } from './script-folder.store.context-token.js';
import { UmbDetailStoreBase } from '@umbraco-cms/backoffice/store';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbFolderModel } from '@umbraco-cms/backoffice/tree';

/**
 * @class UmbScriptFolderStore
 * @augments {UmbStoreBase}
 * @description - Data Store for Script Folders
 */
export class UmbScriptFolderStore extends UmbDetailStoreBase<UmbFolderModel> {
	/**
	 * Creates an instance of UmbScriptFolderStore.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbScriptFolderStore
	 */
	constructor(host: UmbControllerHost) {
		super(host, UMB_SCRIPT_FOLDER_STORE_CONTEXT.toString());
	}
}

export { UmbScriptFolderStore as api };
