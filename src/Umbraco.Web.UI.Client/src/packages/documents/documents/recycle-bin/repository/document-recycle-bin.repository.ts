import { UmbDocumentRecycleBinServerDataSource } from './document-recycle-bin.server.data-source.js';
import { UmbRecycleBinRepositoryBase } from '@umbraco-cms/backoffice/recycle-bin';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbDocumentRecycleBinRepository extends UmbRecycleBinRepositoryBase {
	constructor(host: UmbControllerHost) {
		super(host, UmbDocumentRecycleBinServerDataSource);
	}
}

export { UmbDocumentRecycleBinRepository as api };
