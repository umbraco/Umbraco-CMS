import { UmbMediaRecycleBinServerDataSource } from './media-recycle-bin.server.data-source.js';
import { UmbRecycleBinRepositoryBase } from '@umbraco-cms/backoffice/recycle-bin';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbMediaRecycleBinRepository extends UmbRecycleBinRepositoryBase {
	constructor(host: UmbControllerHost) {
		super(host, UmbMediaRecycleBinServerDataSource);
	}
}

export { UmbMediaRecycleBinRepository as api };
