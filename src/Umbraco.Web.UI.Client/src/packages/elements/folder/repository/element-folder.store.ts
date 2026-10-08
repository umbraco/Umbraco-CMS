import { UMB_ELEMENT_FOLDER_STORE_CONTEXT } from './element-folder.store.context-token.js';
import { UmbDetailStoreBase } from '@umbraco-cms/backoffice/store';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbFolderModel } from '@umbraco-cms/backoffice/tree';

/**
 * @class UmbElementFolderStore
 * @augments {UmbStoreBase}
 * @description - Data Store for Element Folders
 */
export class UmbElementFolderStore extends UmbDetailStoreBase<UmbFolderModel> {
	/**
	 * Creates an instance of UmbElementFolderStore.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbElementFolderStore
	 */
	constructor(host: UmbControllerHost) {
		super(host, UMB_ELEMENT_FOLDER_STORE_CONTEXT.toString());
	}
}

export { UmbElementFolderStore as api };
