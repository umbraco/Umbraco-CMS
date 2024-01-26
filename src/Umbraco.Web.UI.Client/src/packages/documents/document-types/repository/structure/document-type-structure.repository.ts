import { UmbDocumentTypeStructureServerDataSource } from './document-type-structure.server.data-source.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbContentTypeStructureRepositoryBase } from '@umbraco-cms/backoffice/content-type';

export class UmbDocumentTypeStructureRepository extends UmbContentTypeStructureRepositoryBase {
	constructor(host: UmbControllerHost) {
		super(host, UmbDocumentTypeStructureServerDataSource);
	}
}
