import { UMB_MEDIA_TYPE_FOLDER_STORE_CONTEXT } from './media-type-folder.store.context-token.js';
import { UmbDetailStoreBase } from '@umbraco-cms/backoffice/store';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbFolderModel } from '@umbraco-cms/backoffice/tree';

/**
 * @class UmbMediaTypeStore
 * @augments {UmbStoreBase}
 * @description - Data Store for Media Types
 */
export class UmbMediaTypeFolderStore extends UmbDetailStoreBase<UmbFolderModel> {
	/**
	 * Creates an instance of UmbMediaTypeStore.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbMediaTypeStore
	 */
	constructor(host: UmbControllerHost) {
		super(host, UMB_MEDIA_TYPE_FOLDER_STORE_CONTEXT.toString());
	}
}

export { UmbMediaTypeFolderStore as api };
