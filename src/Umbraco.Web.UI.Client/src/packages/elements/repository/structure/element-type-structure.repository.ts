import { UmbElementTypeStructureServerDataSource } from './element-type-structure.server.data-source.js';
import type { UmbAllowedElementTypeModel } from './types.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbContentTypeStructureRepositoryBase } from '@umbraco-cms/backoffice/content-type';

export class UmbElementTypeStructureRepository extends UmbContentTypeStructureRepositoryBase<UmbAllowedElementTypeModel> {
	constructor(host: UmbControllerHost) {
		super(host, UmbElementTypeStructureServerDataSource);
	}
}

export { UmbElementTypeStructureRepository as api };
