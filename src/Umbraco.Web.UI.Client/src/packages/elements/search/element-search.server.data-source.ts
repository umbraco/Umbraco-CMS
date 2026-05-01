import { UMB_ELEMENT_ENTITY_TYPE } from '../entity.js';
import { UMB_EDIT_ELEMENT_WORKSPACE_PATH_PATTERN } from '../paths.js';
import type { UmbElementSearchItemModel } from './types.js';
import type { UmbSearchDataSource, UmbSearchRequestArgs } from '@umbraco-cms/backoffice/search';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { ElementService } from '@umbraco-cms/backoffice/external/backend-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';

export class UmbElementSearchServerDataSource implements UmbSearchDataSource<UmbElementSearchItemModel> {
	#host: UmbControllerHost;

	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	async search(args: UmbSearchRequestArgs) {
		const { data, error } = await tryExecute(
			this.#host,
			ElementService.getItemElementSearch({
				query: {
					query: args.query,
					skip: args.paging?.skip,
					take: args.paging?.take,
				},
			}),
		);

		if (data) {
			const mappedItems: Array<UmbElementSearchItemModel> = data.items.map((item) => ({
				entityType: UMB_ELEMENT_ENTITY_TYPE,
				unique: item.id,
				name: item.variants[0]?.name ?? '',
				href: UMB_EDIT_ELEMENT_WORKSPACE_PATH_PATTERN.generateAbsolute({ unique: item.id }),
				documentType: {
					unique: item.documentType.id,
					icon: item.documentType.icon,
					collection: null,
				},
				hasChildren: item.hasChildren,
				isTrashed: false,
				parent: item.parent ? { unique: item.parent.id } : null,
				variants: item.variants.map((variant) => ({
					name: variant.name,
					culture: variant.culture || null,
					state: variant.state,
					flags: variant.flags,
				})),
				flags: item.flags,
			}));

			return { data: { items: mappedItems, total: data.total } };
		}

		return { error };
	}
}
