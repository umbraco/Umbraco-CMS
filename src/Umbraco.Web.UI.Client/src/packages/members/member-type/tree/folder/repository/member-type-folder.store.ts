import { UMB_MEMBER_TYPE_FOLDER_STORE_CONTEXT } from './member-type-folder.store.context-token.js';
import { UmbDetailStoreBase } from '@umbraco-cms/backoffice/store';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbFolderModel } from '@umbraco-cms/backoffice/tree';

/**
 * @class UmbMemberTypeStore
 * @augments {UmbStoreBase}
 * @description - Data Store for Member Types
 */
export class UmbMemberTypeFolderStore extends UmbDetailStoreBase<UmbFolderModel> {
	/**
	 * Creates an instance of UmbMemberTypeStore.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbMemberTypeStore
	 */
	constructor(host: UmbControllerHost) {
		super(host, UMB_MEMBER_TYPE_FOLDER_STORE_CONTEXT.toString());
	}
}

export { UmbMemberTypeFolderStore as api };
