import { UMB_TEMPLATE_ENTITY_TYPE } from '../entity.js';
import type { UmbTemplateSearchItemModel } from './template.search-provider.js';
import type { UmbSearchDataSource, UmbSearchRequestArgs } from '@umbraco-cms/backoffice/search';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { TemplateService } from '@umbraco-cms/backoffice/external/backend-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';

/**
 * A data source for the Rollback that fetches data from the server
 * @class UmbTemplateSearchServerDataSource
 * @implements {RepositoryDetailDataSource}
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
	 * @param args
	 * @returns {*}
	 * @memberof UmbTemplateSearchServerDataSource
	 */
	async search(args: UmbSearchRequestArgs) {
		const { data, error } = await tryExecute(
			this.#host,
			TemplateService.getItemTemplateSearch({
				query: args.query,
			}),
		);

		if (data) {
			const mappedItems: Array<UmbTemplateSearchItemModel> = data.items.map((item) => {
				return {
					href: '/section/settings/workspace/template/edit/' + item.id,
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
