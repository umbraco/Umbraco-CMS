import { UMB_PARTIAL_VIEW_FOLDER_STORE_CONTEXT } from './partial-view-folder.store.context-token.js';
import { UmbDetailStoreBase } from '@umbraco-cms/backoffice/store';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbFolderModel } from '@umbraco-cms/backoffice/tree';

/**
 * @class UmbPartialViewFolderStore
 * @augments {UmbStoreBase}
 * @description - Data Store for Partial View Folders
 */
export class UmbPartialViewFolderStore extends UmbDetailStoreBase<UmbFolderModel> {
	/**
	 * Creates an instance of UmbPartialViewFolderStore.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbPartialViewFolderStore
	 */
	constructor(host: UmbControllerHost) {
		super(host, UMB_PARTIAL_VIEW_FOLDER_STORE_CONTEXT.toString());
	}
}

export { UmbPartialViewFolderStore as api };
