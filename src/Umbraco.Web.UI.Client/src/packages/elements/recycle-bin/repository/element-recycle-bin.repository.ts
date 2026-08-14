import { UmbElementRecycleBinServerDataSource } from './element-recycle-bin.server.data-source.js';
import { UmbRecycleBinRepositoryBase } from '@umbraco-cms/backoffice/recycle-bin';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbElementRecycleBinRepository extends UmbRecycleBinRepositoryBase {
	constructor(host: UmbControllerHost) {
		super(host, UmbElementRecycleBinServerDataSource);
	}
}

export { UmbElementRecycleBinRepository as api };
