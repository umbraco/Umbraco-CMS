import { UmbMemberTypeStructureServerDataSource } from './member-type-structure.server.data-source.js';
import type { UmbAllowedMemberTypeModel } from './types.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbContentTypeStructureRepositoryBase } from '@umbraco-cms/backoffice/content-type';

export class UmbMemberTypeStructureRepository extends UmbContentTypeStructureRepositoryBase<UmbAllowedMemberTypeModel> {
	constructor(host: UmbControllerHost) {
		super(host, UmbMemberTypeStructureServerDataSource);
	}
}

export { UmbMemberTypeStructureRepository as api };
