import { UMB_DOCUMENT_ENTITY_TYPE } from '../entity.js';
import type { UmbDocumentSearchItemModel, UmbDocumentSearchRequestArgs } from './types.js';
import type { UmbSearchDataSource } from '@umbraco-cms/backoffice/search';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { DocumentService, type DocumentItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';
import type { UmbDocumentItemModel } from '../types.js';

/**
 * A data source for the Rollback that fetches data from the server
 * @class UmbDocumentSearchServerDataSource
 * @implements {RepositoryDetailDataSource}
 */
export class UmbDocumentSearchServerDataSource
	implements UmbSearchDataSource<UmbDocumentSearchItemModel, UmbDocumentSearchRequestArgs>
{
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbDocumentSearchServerDataSource.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbDocumentSearchServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	async #fetchAncestors(ids: Array<string>) {
		if (!ids.length) return { data: new Map() };
		const { data, error } = await tryExecute(
			this.#host,
			DocumentService.getItemDocumentAncestors({ query: { id: ids } }),
		);

		if (error) return { error };

		const ancestorsByItemId = new Map<string, Array<UmbDocumentItemModel>>();
		if (data) {
			for (const entry of data) {
				ancestorsByItemId.set(
					entry.id,
					entry.ancestors.map((ancestor: DocumentItemResponseModel) => ({
						documentType: {
							unique: ancestor.documentType.id,
							icon: ancestor.documentType.icon,
							collection: ancestor.documentType.collection ? { unique: ancestor.documentType.collection.id } : null,
						},
						entityType: UMB_DOCUMENT_ENTITY_TYPE,
						hasChildren: ancestor.hasChildren,
						isProtected: ancestor.isProtected,
						isTrashed: ancestor.isTrashed,
						parent: ancestor.parent ? { unique: ancestor.parent.id } : null,
						unique: ancestor.id,
						variants: ancestor.variants.map((variant) => ({
							name: variant.name,
							culture: variant.culture || null,
							state: variant.state,
							flags: variant.flags,
						})),
						flags: ancestor.flags,
					})),
				);
			}
		}
		return { data: ancestorsByItemId };
	}

	/**
	 * Get a list of versions for a document
	 * @param {UmbDocumentSearchRequestArgs} args - The arguments for the search
	 * @returns {*}
	 * @memberof UmbDocumentSearchServerDataSource
	 */
	async search(args: UmbDocumentSearchRequestArgs) {
		const { data, error } = await tryExecute(
			this.#host,
			DocumentService.getItemDocumentSearch({
				query: {
					allowedDocumentTypes: args.allowedContentTypes?.map((contentType) => contentType.unique),
					culture: args.culture || undefined,
					parentId: args.searchFrom?.unique ?? undefined,
					query: args.query,
					trashed: args.includeTrashed,
					dataTypeId: args.dataTypeUnique,
					skip: args.paging?.skip,
					take: args.paging?.take,
				},
			}),
		);

		if (data) {
			const ids = data.items.map((item) => item.id);
			const { data: ancestorsByItemId, error: ancestorsError } = await this.#fetchAncestors(ids);
			if (ancestorsError) return { error: ancestorsError };

			const mappedItems: Array<UmbDocumentSearchItemModel> = data.items.map((item) => {
				return {
					documentType: {
						collection: item.documentType.collection ? { unique: item.documentType.collection.id } : null,
						icon: item.documentType.icon,
						unique: item.documentType.id,
					},
					entityType: UMB_DOCUMENT_ENTITY_TYPE,
					hasChildren: item.hasChildren,
					href: 'section/content/workspace/document/edit/' + item.id,
					isProtected: item.isProtected,
					isTrashed: item.isTrashed,
					name: item.variants[0]?.name, // TODO: this is not correct. We need to get it from the variants. This is a temp solution.
					parent: item.parent ? { unique: item.parent.id } : null,
					unique: item.id,
					variants: item.variants.map((variant) => {
						return {
							culture: variant.culture || null,
							name: variant.name,
							state: variant.state,
							flags: variant.flags,
						};
					}),
					flags: item.flags,
					ancestors: ancestorsByItemId.get(item.id) ?? [],
				};
			});

			return { data: { items: mappedItems, total: data.total } };
		}

		return { error };
	}
}
