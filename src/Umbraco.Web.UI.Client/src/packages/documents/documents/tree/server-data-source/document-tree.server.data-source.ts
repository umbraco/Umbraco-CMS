import { UMB_DOCUMENT_ENTITY_TYPE, UMB_DOCUMENT_ROOT_ENTITY_TYPE } from '../../entity.js';
import type {
	UmbDocumentTreeChildrenOfRequestArgs,
	UmbDocumentTreeItemModel,
	UmbDocumentTreeRootItemsRequestArgs,
} from '../types.js';
import { UmbManagementApiDocumentItemDataRequestManager } from '../../item/repository/document-item.server.request-manager.js';
import { UmbManagementApiDocumentTreeDataRequestManager } from './document-tree.server.request-manager.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type {
	DocumentItemResponseModel,
	DocumentTreeItemResponseModel,
} from '@umbraco-cms/backoffice/external/backend-api';
import type {
	UmbTreeAncestorsOfRequestArgs,
	UmbTreeDataSource,
	UmbTreeItemsRequestArgs,
} from '@umbraco-cms/backoffice/tree';

/**
 * A data source for the Document tree that fetches data from the server
 * @class UmbDocumentTreeServerDataSource
 */
export class UmbDocumentTreeServerDataSource
	extends UmbControllerBase
	implements UmbTreeDataSource<UmbDocumentTreeItemModel>
{
	#treeRequestManager = new UmbManagementApiDocumentTreeDataRequestManager(this);
	#itemRequestManager = new UmbManagementApiDocumentItemDataRequestManager(this);

	async getRootItems(args: UmbDocumentTreeRootItemsRequestArgs) {
		const { data, error } = await this.#treeRequestManager.getRootItems(args);

		const mappedData = data
			? {
					...data,
					items: data?.items.map((item) => this.#mapItem(item)),
				}
			: undefined;

		return { data: mappedData, error };
	}

	async getChildrenOf(args: UmbDocumentTreeChildrenOfRequestArgs) {
		const { data, error } = await this.#treeRequestManager.getChildrenOf(args);

		const mappedData = data
			? {
					...data,
					items: data?.items.map((item) => this.#mapItem(item)),
				}
			: undefined;

		return { data: mappedData, error };
	}

	async getAncestorsOf(args: UmbTreeAncestorsOfRequestArgs) {
		const { data, error } = await this.#treeRequestManager.getAncestorsOf(args);

		const mappedData = data?.map((item) => this.#mapItem(item));

		return { data: mappedData, error };
	}

	async getItems(args: UmbTreeItemsRequestArgs) {
		const { data, error } = await this.#itemRequestManager.getItems(args.uniques);

		// The item request manager resolves from the cache, inflight requests and the server, in that order,
		// so the response order does not follow the requested order. Restore it here.
		const itemsByUnique = new Map(data?.map((item) => [item.id, item]));
		const mappedData = data
			? args.uniques
					.map((unique) => itemsByUnique.get(unique))
					.filter((item): item is DocumentItemResponseModel => item !== undefined)
					.map((item) => this.#mapItemModel(item))
			: undefined;

		return { data: mappedData, error };
	}

	#mapItem(item: DocumentTreeItemResponseModel): UmbDocumentTreeItemModel {
		return {
			ancestors: item.ancestors.map((ancestor) => {
				return {
					unique: ancestor.id,
					entityType: UMB_DOCUMENT_ENTITY_TYPE,
				};
			}),
			unique: item.id,
			parent: {
				unique: item.parent ? item.parent.id : null,
				entityType: item.parent ? UMB_DOCUMENT_ENTITY_TYPE : UMB_DOCUMENT_ROOT_ENTITY_TYPE,
			},
			entityType: UMB_DOCUMENT_ENTITY_TYPE,
			noAccess: item.noAccess,
			isTrashed: item.isTrashed,
			hasChildren: item.hasChildren,
			isProtected: item.isProtected,
			flags: item.flags,
			documentType: {
				unique: item.documentType.id,
				icon: item.documentType.icon,
				collection: item.documentType.collection ? { unique: item.documentType.collection.id } : null,
			},
			variants: item.variants.map((variant) => {
				return {
					name: variant.name,
					culture: variant.culture || null,
					segment: null, // TODO: add segment to the backend API?
					state: variant.state,
					flags: variant.flags,
				};
			}),
			name: item.variants[0]?.name, // TODO: this is not correct. We need to get it from the variants. This is a temp solution.
			isFolder: false,
			createDate: item.createDate,
		};
	}

	// The item endpoint carries no ancestors, noAccess or createDate. Items mapped here are only used as the
	// top level of a multi-root tree, where they are not selectable and their children still come from the tree
	// endpoint with the real noAccess value.
	#mapItemModel(item: DocumentItemResponseModel): UmbDocumentTreeItemModel {
		return {
			ancestors: [],
			unique: item.id,
			parent: {
				unique: item.parent ? item.parent.id : null,
				entityType: item.parent ? UMB_DOCUMENT_ENTITY_TYPE : UMB_DOCUMENT_ROOT_ENTITY_TYPE,
			},
			entityType: UMB_DOCUMENT_ENTITY_TYPE,
			noAccess: false,
			isTrashed: item.isTrashed,
			hasChildren: item.hasChildren,
			isProtected: item.isProtected,
			flags: item.flags,
			documentType: {
				unique: item.documentType.id,
				icon: item.documentType.icon,
				collection: item.documentType.collection ? { unique: item.documentType.collection.id } : null,
			},
			variants: item.variants.map((variant) => {
				return {
					name: variant.name,
					culture: variant.culture || null,
					segment: null,
					state: variant.state,
					flags: variant.flags,
				};
			}),
			name: item.variants[0]?.name,
			isFolder: false,
			createDate: '',
		};
	}
}
