/* eslint-disable local-rules/no-direct-api-import */
import { memberTypeDetailCache } from './member-type-detail.server.cache.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import {
	MemberTypeService,
	type CreateMemberTypeRequestModel,
	type MemberTypeResponseModel,
	type UpdateMemberTypeRequestModel,
} from '@umbraco-cms/backoffice/external/backend-api';
import {
	UmbManagementApiDetailDataRequestManager,
	UmbManagementApiInFlightRequestCache,
} from '@umbraco-cms/backoffice/management-api';

export class UmbManagementApiMemberTypeDetailDataRequestManager extends UmbManagementApiDetailDataRequestManager<
	MemberTypeResponseModel,
	UpdateMemberTypeRequestModel,
	CreateMemberTypeRequestModel
> {
	static #inflightRequestCache = new UmbManagementApiInFlightRequestCache<MemberTypeResponseModel>();

	constructor(host: UmbControllerHost) {
		super(host, {
			create: (body: CreateMemberTypeRequestModel) => MemberTypeService.postMemberType({ body }),
			read: (id: string) => MemberTypeService.getMemberTypeById({ path: { id } }),
			update: (id: string, body: UpdateMemberTypeRequestModel) =>
				MemberTypeService.putMemberTypeById({ path: { id }, body }),
			delete: (id: string) => MemberTypeService.deleteMemberTypeById({ path: { id } }),
			readMany: (ids: Array<string>) => MemberTypeService.getMemberTypeBatch({ query: { id: ids } }),
			dataCache: memberTypeDetailCache,
			inflightRequestCache: UmbManagementApiMemberTypeDetailDataRequestManager.#inflightRequestCache,
		});
	}
}
