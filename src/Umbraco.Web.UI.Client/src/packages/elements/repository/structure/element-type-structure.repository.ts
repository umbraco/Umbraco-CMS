import { UmbElementTypeStructureServerDataSource } from './element-type-structure.server.data-source.js';
import type { UmbAllowedElementTypeModel } from './types.js';
import { UmbContentTypeStructureRepositoryBase } from '@umbraco-cms/backoffice/content-type';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

/**
 * Repository for fetching allowed element type structure, such as which element types are allowed as children.
 * @class UmbElementTypeStructureRepository
 * @augments {UmbContentTypeStructureRepositoryBase}
 */
export class UmbElementTypeStructureRepository extends UmbContentTypeStructureRepositoryBase<UmbAllowedElementTypeModel> {
	constructor(host: UmbControllerHost) {
		super(host, UmbElementTypeStructureServerDataSource);
	}
}

export { UmbElementTypeStructureRepository as api };
