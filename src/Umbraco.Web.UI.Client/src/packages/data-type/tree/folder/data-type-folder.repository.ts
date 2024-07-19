import { UmbDataTypeFolderServerDataSource } from './data-type-folder.server.data-source.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbFolderRepositoryBase } from '@umbraco-cms/backoffice/tree';

export class UmbDataTypeFolderRepository extends UmbFolderRepositoryBase {
	constructor(host: UmbControllerHost) {
		super(host, UmbDataTypeFolderServerDataSource);
	}
}

export { UmbDataTypeFolderRepository as api };
