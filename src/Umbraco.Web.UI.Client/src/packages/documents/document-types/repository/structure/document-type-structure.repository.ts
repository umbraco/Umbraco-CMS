import { UmbDocumentTypeStructureServerDataSource } from './document-type-structure.server.data-source.js';
import type { UmbAllowedDocumentTypeModel } from './types.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbContentTypeStructureRepositoryBase } from '@umbraco-cms/backoffice/content-type';

export class UmbDocumentTypeStructureRepository extends UmbContentTypeStructureRepositoryBase<UmbAllowedDocumentTypeModel> {
	constructor(host: UmbControllerHost) {
		super(host, UmbDocumentTypeStructureServerDataSource);
	}

	async requestAllowedParentsOf(unique: string) {
		// We expect the Document Type Structure Data Source to always have the getAllowedParentsOf method,
		//  but it is optional in the interface, so we check just to be safe.
		if (!this._dataSource.getAllowedParentsOf) {
			throw new Error('Data source does not support fetching allowed parents');
		}

		return this._dataSource.getAllowedParentsOf(unique);
	}
}

export { UmbDocumentTypeStructureRepository as api };
