import { UMB_MEMBER_ENTITY_TYPE } from '../entity.js';
import type { UmbMemberSearchItemModel } from './member.search-provider.js';
import type { UmbSearchDataSource, UmbSearchRequestArgs } from '@umbraco-cms/backoffice/search';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { MemberService } from '@umbraco-cms/backoffice/external/backend-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

/**
 * A data source for the Rollback that fetches data from the server
 * @class UmbMemberSearchServerDataSource
 * @implements {RepositoryDetailDataSource}
 */
export class UmbMemberSearchServerDataSource implements UmbSearchDataSource<UmbMemberSearchItemModel> {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbMemberSearchServerDataSource.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbMemberSearchServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Get a list of versions for a data
	 * @param args
	 * @returns {*}
	 * @memberof UmbMemberSearchServerDataSource
	 */
	async search(args: UmbSearchRequestArgs) {
		const { data, error } = await tryExecuteAndNotify(
			this.#host,
			MemberService.getItemMemberSearch({
				query: args.query,
			}),
		);

		if (data) {
			const mappedItems: Array<UmbMemberSearchItemModel> = data.items.map((item) => {
				return {
					href: '/section/member-management/workspace/member/edit/' + item.id,
					entityType: UMB_MEMBER_ENTITY_TYPE,
					unique: item.id,
					name: item.variants[0].name || '',
					memberType: {
						unique: item.memberType.id,
						icon: item.memberType.icon,
						collection: item.memberType.collection ? { unique: item.memberType.collection.id } : null,
					},
					variants: item.variants.map((variant) => {
						return {
							name: variant.name,
							culture: variant.culture || null,
						};
					}),
				};
			});

			return { data: { items: mappedItems, total: data.total } };
		}

		return { error };
	}
}
