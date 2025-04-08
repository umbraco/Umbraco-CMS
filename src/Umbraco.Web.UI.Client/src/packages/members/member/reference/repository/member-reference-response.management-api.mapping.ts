import { UMB_MEMBER_ENTITY_TYPE } from '../../entity.js';
import type { UmbMemberReferenceModel } from './types.js';
import type { MemberReferenceResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbDataSourceDataMapping } from '@umbraco-cms/backoffice/repository';

export class UmbMemberReferenceResponseManagementApiDataMapping
	extends UmbControllerBase
	implements UmbDataSourceDataMapping<MemberReferenceResponseModel, UmbMemberReferenceModel>
{
	async map(data: MemberReferenceResponseModel): Promise<UmbMemberReferenceModel> {
		return {
			entityType: UMB_MEMBER_ENTITY_TYPE,
			memberType: {
				alias: data.memberType.alias!,
				icon: data.memberType.icon!,
				name: data.memberType.name!,
				unique: data.memberType.id,
			},
			name: data.name,
			// TODO: this is a hardcoded array until the server can return the correct variants array
			variants: [
				{
					culture: null,
					name: data.name ?? '',
				},
			],
			unique: data.id,
		};
	}
}

export { UmbMemberReferenceResponseManagementApiDataMapping as api };
