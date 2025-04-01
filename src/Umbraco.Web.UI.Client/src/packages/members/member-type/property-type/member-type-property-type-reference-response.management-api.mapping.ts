import type { UmbMemberTypePropertyTypeReferenceModel } from './types.js';
import { UMB_MEMBER_TYPE_PROPERTY_TYPE_ENTITY_TYPE } from './entity.js';
import type { MemberTypePropertyReferenceResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbDataSourceDataMapping } from '@umbraco-cms/backoffice/repository';

export class UmbMemberTypePropertyTypeReferenceResponseManagementApiDataMapping
	extends UmbControllerBase
	implements UmbDataSourceDataMapping<MemberTypePropertyReferenceResponseModel, UmbMemberTypePropertyTypeReferenceModel>
{
	async map(data: MemberTypePropertyReferenceResponseModel): Promise<UmbMemberTypePropertyTypeReferenceModel> {
		return {
			alias: data.alias!,
			memberType: {
				alias: data.memberType.alias!,
				icon: data.memberType.icon!,
				name: data.memberType.name!,
			},
			entityType: UMB_MEMBER_TYPE_PROPERTY_TYPE_ENTITY_TYPE,
			name: data.name!,
			unique: data.id,
		};
	}
}

export { UmbMemberTypePropertyTypeReferenceResponseManagementApiDataMapping as api };
