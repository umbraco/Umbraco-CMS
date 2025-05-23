import { UmbPartialViewFolderServerDataSource } from './partial-view-folder.server.data-source.js';
import { UMB_PARTIAL_VIEW_FOLDER_STORE_CONTEXT } from './partial-view-folder.store.context-token.js';
import { UmbDetailRepositoryBase } from '@umbraco-cms/backoffice/repository';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbFolderModel } from '@umbraco-cms/backoffice/tree';

export class UmbPartialViewFolderRepository extends UmbDetailRepositoryBase<UmbFolderModel> {
	constructor(host: UmbControllerHost) {
		super(host, UmbPartialViewFolderServerDataSource, UMB_PARTIAL_VIEW_FOLDER_STORE_CONTEXT);
	}
}

export { UmbPartialViewFolderRepository as api };
