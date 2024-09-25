import { UmbDataTypeFolderServerDataSource } from './data-type-folder.server.data-source.js';
import { UmbDetailRepositoryBase } from '@umbraco-cms/backoffice/repository';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbFolderModel } from '@umbraco-cms/backoffice/tree';

export class UmbDataTypeFolderRepository extends UmbDetailRepositoryBase<
	UmbFolderModel,
	UmbDataTypeFolderServerDataSource
> {
	constructor(host: UmbControllerHost) {
		super(host, UmbDataTypeFolderServerDataSource);
	}
}

export { UmbDataTypeFolderRepository as api };
