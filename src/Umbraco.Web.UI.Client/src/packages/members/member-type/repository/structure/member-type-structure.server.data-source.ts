import { UMB_MEMBER_TYPE_ENTITY_TYPE } from '../../entity.js';
import type { UmbAllowedMemberTypeModel } from './types.js';
import type { AllowedMemberTypeModel } from '@umbraco-cms/backoffice/external/backend-api';
import { MemberTypeService } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbContentTypeStructureServerDataSourceBase } from '@umbraco-cms/backoffice/content-type';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

/**
 * @class UmbMemberTypeStructureServerDataSource
 * @augments {UmbContentTypeStructureServerDataSourceBase}
 */
export class UmbMemberTypeStructureServerDataSource extends UmbContentTypeStructureServerDataSourceBase<
	AllowedMemberTypeModel,
	UmbAllowedMemberTypeModel
> {
	constructor(host: UmbControllerHost) {
		super(host, { getAllowedChildrenOf, mapper });
	}
}

const getAllowedChildrenOf = () => {
	// eslint-disable-next-line local-rules/no-direct-api-import
	return MemberTypeService.getMemberTypeAllowedAtRoot({});
};

const mapper = (item: AllowedMemberTypeModel): UmbAllowedMemberTypeModel => {
	return {
		unique: item.id,
		entityType: UMB_MEMBER_TYPE_ENTITY_TYPE,
		name: item.name,
		description: item.description || null,
		icon: item.icon || null,
	};
};
