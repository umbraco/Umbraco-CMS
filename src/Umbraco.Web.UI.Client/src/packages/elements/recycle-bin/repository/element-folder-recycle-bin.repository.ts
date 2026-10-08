import { UmbElementFolderRecycleBinServerDataSource } from './element-folder-recycle-bin.server.data-source.js';
import { UmbRecycleBinRepositoryBase } from '@umbraco-cms/backoffice/recycle-bin';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbElementFolderRecycleBinRepository extends UmbRecycleBinRepositoryBase {
	constructor(host: UmbControllerHost) {
		super(host, UmbElementFolderRecycleBinServerDataSource);
	}
}

export { UmbElementFolderRecycleBinRepository as api };
