import { UmbElementFolderServerDataSource } from './element-folder.server.data-source.js';
import { UMB_ELEMENT_FOLDER_STORE_CONTEXT } from './element-folder.store.context-token.js';
import { UmbDetailRepositoryBase } from '@umbraco-cms/backoffice/repository';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbFolderModel } from '@umbraco-cms/backoffice/tree';

export class UmbElementFolderRepository extends UmbDetailRepositoryBase<
	UmbFolderModel,
	UmbElementFolderServerDataSource
> {
	constructor(host: UmbControllerHost) {
		super(host, UmbElementFolderServerDataSource, UMB_ELEMENT_FOLDER_STORE_CONTEXT);
	}
}

export { UmbElementFolderRepository as api };
