import { UmbDocumentTypeStructureServerDataSource } from './document-type-structure.server.data-source.js';
import type { UmbAllowedDocumentTypeModel } from './types.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbContentTypeStructureRepositoryBase } from '@umbraco-cms/backoffice/content-type';

export class UmbDocumentTypeStructureRepository extends UmbContentTypeStructureRepositoryBase<UmbAllowedDocumentTypeModel> {
	constructor(host: UmbControllerHost) {
		super(host, UmbDocumentTypeStructureServerDataSource);
	}
}

export { UmbDocumentTypeStructureRepository as api };
