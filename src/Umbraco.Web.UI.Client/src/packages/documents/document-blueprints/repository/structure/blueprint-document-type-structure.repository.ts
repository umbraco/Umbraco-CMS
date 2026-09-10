import { UmbBlueprintDocumentTypeStructureServerDataSource } from './blueprint-document-type-structure.server.data-source.js';
import type { UmbAllowedBlueprintDocumentTypeModel } from './types.js';
import { UmbContentTypeStructureRepositoryBase } from '@umbraco-cms/backoffice/content-type';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

/**
 * Repository for fetching the document types a document blueprint can be created for.
 * @class UmbBlueprintDocumentTypeStructureRepository
 * @augments {UmbContentTypeStructureRepositoryBase}
 */
export class UmbBlueprintDocumentTypeStructureRepository extends UmbContentTypeStructureRepositoryBase<UmbAllowedBlueprintDocumentTypeModel> {
	constructor(host: UmbControllerHost) {
		super(host, UmbBlueprintDocumentTypeStructureServerDataSource);
	}
}

export { UmbBlueprintDocumentTypeStructureRepository as api };
