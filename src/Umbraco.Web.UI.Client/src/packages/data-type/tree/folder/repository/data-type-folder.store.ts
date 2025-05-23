import { UMB_DATA_TYPE_FOLDER_STORE_CONTEXT } from './data-type-folder.store.context-token.js';
import { UmbDetailStoreBase } from '@umbraco-cms/backoffice/store';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbFolderModel } from '@umbraco-cms/backoffice/tree';

/**
 * @class UmbDataTypeStore
 * @augments {UmbStoreBase}
 * @description - Data Store for Data Types
 */
export class UmbDataTypeFolderStore extends UmbDetailStoreBase<UmbFolderModel> {
	/**
	 * Creates an instance of UmbDataTypeStore.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbDataTypeStore
	 */
	constructor(host: UmbControllerHost) {
		super(host, UMB_DATA_TYPE_FOLDER_STORE_CONTEXT.toString());
	}
}

export { UmbDataTypeFolderStore as api };
