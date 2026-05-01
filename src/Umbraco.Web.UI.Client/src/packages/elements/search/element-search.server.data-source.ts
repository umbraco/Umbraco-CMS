import { UMB_ELEMENT_ENTITY_TYPE } from '../entity.js';
import { UMB_EDIT_ELEMENT_WORKSPACE_PATH_PATTERN } from '../paths.js';
import type { UmbElementSearchAncestorModel, UmbElementSearchItemModel } from './types.js';
import type { UmbSearchDataSource, UmbSearchRequestArgs } from '@umbraco-cms/backoffice/search';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { ElementService } from '@umbraco-cms/backoffice/external/backend-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';

export class UmbElementSearchServerDataSource implements UmbSearchDataSource<UmbElementSearchItemModel> {
	#host: UmbControllerHost;

	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	async #fetchAncestors(ids: Array<string>) {
		if (!ids.length) return { data: new Map<string, Array<UmbElementSearchAncestorModel>>() };
		const { data, error } = await tryExecute(
			this.#host,
			ElementService.getItemElementAncestors({ query: { id: ids } }),
		);

		if (error) return { error };

		const ancestorsByItemId = new Map<string, Array<UmbElementSearchAncestorModel>>();
		if (data) {
			for (const entry of data) {
				ancestorsByItemId.set(
					entry.id,
					entry.ancestors.map((ancestor) => ({ unique: ancestor.id, name: ancestor.name })),
				);
			}
		}
		return { data: ancestorsByItemId };
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
			const ids = data.items.map((item) => item.id);
			const { data: ancestorsByItemId, error: ancestorsError } = await this.#fetchAncestors(ids);
			if (ancestorsError) return { error: ancestorsError };

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
				isTrashed: item.isTrashed,
				parent: item.parent ? { unique: item.parent.id } : null,
				variants: item.variants.map((variant) => ({
					name: variant.name,
					culture: variant.culture || null,
					state: variant.state,
					flags: variant.flags,
				})),
				flags: item.flags,
				ancestors: ancestorsByItemId.get(item.id) ?? [],
			}));

			return { data: { items: mappedItems, total: data.total } };
		}

		return { error };
	}
}
