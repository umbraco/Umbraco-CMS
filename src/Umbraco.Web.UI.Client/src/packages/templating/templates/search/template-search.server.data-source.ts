import { UMB_TEMPLATE_ENTITY_TYPE } from '../entity.js';
import type { UmbTemplateSearchItemModel } from './template.search-provider.js';
import type { UmbSearchDataSource, UmbSearchRequestArgs } from '@umbraco-cms/backoffice/search';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { TemplateService } from '@umbraco-cms/backoffice/external/backend-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';
import type { UmbDataSourceResponse, UmbPagedModel } from '@umbraco-cms/backoffice/repository';

/**
 * A data source for the Rollback that fetches data from the server
 * @class UmbTemplateSearchServerDataSource
 * @implements {UmbSearchDataSource}
 */
export class UmbTemplateSearchServerDataSource implements UmbSearchDataSource<UmbTemplateSearchItemModel> {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbTemplateSearchServerDataSource.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbTemplateSearchServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Get a list of versions for a data
	 * @param {UmbSearchRequestArgs} args - The search arguments
	 * @returns {UmbDataSourceResponse<UmbPagedModel<UmbTemplateSearchItemModel>>} The search results
	 * @memberof UmbTemplateSearchServerDataSource
	 */
	async search(args: UmbSearchRequestArgs): Promise<UmbDataSourceResponse<UmbPagedModel<UmbTemplateSearchItemModel>>> {
		const { data, error } = await tryExecute(
			this.#host,
			TemplateService.getItemTemplateSearch({
				query: {
					query: args.query,
					skip: args.paging?.skip,
					take: args.paging?.take,
				},
			}),
		);

		if (data) {
			const mappedItems: Array<UmbTemplateSearchItemModel> = data.items.map((item) => {
				return {
					href: 'section/settings/workspace/template/edit/' + item.id,
					entityType: UMB_TEMPLATE_ENTITY_TYPE,
					unique: item.id,
					name: item.name,
					alias: item.alias,
				};
			});

			return { data: { items: mappedItems, total: data.total } };
		}

		return { error };
	}
}
