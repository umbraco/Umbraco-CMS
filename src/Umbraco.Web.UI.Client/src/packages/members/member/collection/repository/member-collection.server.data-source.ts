import type { UmbMemberDetailModel, UmbMemberValueModel } from '../../types.js';
import { UMB_MEMBER_ENTITY_TYPE } from '../../entity.js';
import type { UmbMemberCollectionFilterModel } from '../types.js';
import { tryExecute } from '@umbraco-cms/backoffice/resources';
import type { UmbCollectionDataSource } from '@umbraco-cms/backoffice/collection';
import type { MemberResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { MemberService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbEntityVariantModel } from '@umbraco-cms/backoffice/variant';

/**
 * A data source that fetches the member collection data from the server.
 * @class UmbMemberCollectionServerDataSource
 * @implements {UmbCollectionDataSource}
 */
export class UmbMemberCollectionServerDataSource implements UmbCollectionDataSource<UmbMemberDetailModel> {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbMemberCollectionServerDataSource.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbMemberCollectionServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Gets the member collection filtered by the given filter.
	 * @param {UmbMemberCollectionFilterModel} filter
	 * @returns {*}
	 * @memberof UmbMemberCollectionServerDataSource
	 */
	async getCollection(filter: UmbMemberCollectionFilterModel) {
		const { data, error } = await tryExecute(this.#host, MemberService.getFilterMember(filter));

		if (error) {
			return { error };
		}

		if (!data) {
			return { data: { items: [], total: 0 } };
		}

		const { items, total } = data;

		const mappedItems: Array<UmbMemberDetailModel> = items.map((item: MemberResponseModel) => {
			const memberDetail: UmbMemberDetailModel = {
				entityType: UMB_MEMBER_ENTITY_TYPE,
				email: item.email,
				variants: item.variants as UmbEntityVariantModel[],
				unique: item.id,
				kind: item.kind,
				lastLoginDate: item.lastLoginDate || null,
				lastLockoutDate: item.lastLockoutDate || null,
				lastPasswordChangeDate: item.lastPasswordChangeDate || null,
				failedPasswordAttempts: item.failedPasswordAttempts,
				isApproved: item.isApproved,
				isLockedOut: item.isLockedOut,
				groups: item.groups,
				isTwoFactorEnabled: item.isTwoFactorEnabled,
				memberType: { unique: item.memberType.id, icon: item.memberType.icon },
				username: item.username,
				values: item.values as UmbMemberValueModel[],
			};

			return memberDetail;
		});

		return { data: { items: mappedItems, total } };
	}
}
