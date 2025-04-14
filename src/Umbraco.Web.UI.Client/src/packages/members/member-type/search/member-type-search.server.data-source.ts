import { UMB_MEMBER_TYPE_ENTITY_TYPE } from '../entity.js';
import type { UmbMemberTypeSearchItemModel } from './member-type.search-provider.js';
import type { UmbSearchDataSource, UmbSearchRequestArgs } from '@umbraco-cms/backoffice/search';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { MemberTypeService } from '@umbraco-cms/backoffice/external/backend-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';

/**
 * A data source for the Rollback that fetches data from the server
 * @class UmbMemberTypeSearchServerDataSource
 * @implements {RepositoryDetailDataSource}
 */
export class UmbMemberTypeSearchServerDataSource implements UmbSearchDataSource<UmbMemberTypeSearchItemModel> {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbMemberTypeSearchServerDataSource.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbMemberTypeSearchServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Get a list of versions for a data
	 * @param args
	 * @returns {*}
	 * @memberof UmbMemberTypeSearchServerDataSource
	 */
	async search(args: UmbSearchRequestArgs) {
		const { data, error } = await tryExecute(
			this.#host,
			MemberTypeService.getItemMemberTypeSearch({
				query: { query: args.query },
			}),
		);

		if (data) {
			const mappedItems: Array<UmbMemberTypeSearchItemModel> = data.items.map((item) => {
				return {
					href: '/section/settings/workspace/member-type/edit/' + item.id,
					entityType: UMB_MEMBER_TYPE_ENTITY_TYPE,
					unique: item.id,
					name: item.name,
					icon: item.icon || '',
				};
			});

			return { data: { items: mappedItems, total: data.total } };
		}

		return { error };
	}
}
