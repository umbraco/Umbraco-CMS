import { UmbDataTypeFolderServerDataSource } from './data-type-folder.server.data-source.js';
import { UMB_DATA_TYPE_FOLDER_STORE_CONTEXT } from './data-type-folder.store.context-token.js';
import { UmbDetailRepositoryBase } from '@umbraco-cms/backoffice/repository';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbFolderModel } from '@umbraco-cms/backoffice/tree';

export class UmbDataTypeFolderRepository extends UmbDetailRepositoryBase<
	UmbFolderModel,
	UmbDataTypeFolderServerDataSource
> {
	constructor(host: UmbControllerHost) {
		super(host, UmbDataTypeFolderServerDataSource, UMB_DATA_TYPE_FOLDER_STORE_CONTEXT);
	}
}

export { UmbDataTypeFolderRepository as api };
