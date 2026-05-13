import type { UmbElementFolderModel } from '../types.js';
import { UmbElementFolderServerDataSource } from './element-folder.server.data-source.js';
import { UMB_ELEMENT_FOLDER_STORE_CONTEXT } from './element-folder.store.context-token.js';
import { UmbDetailRepositoryBase } from '@umbraco-cms/backoffice/repository';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbElementFolderRepository extends UmbDetailRepositoryBase<
	UmbElementFolderModel,
	UmbElementFolderServerDataSource
> {
	constructor(host: UmbControllerHost) {
		super(host, UmbElementFolderServerDataSource, UMB_ELEMENT_FOLDER_STORE_CONTEXT);
	}
}

export { UmbElementFolderRepository as api };
