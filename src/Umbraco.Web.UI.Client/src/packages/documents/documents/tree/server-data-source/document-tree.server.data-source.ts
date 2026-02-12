import { UMB_DOCUMENT_ENTITY_TYPE, UMB_DOCUMENT_ROOT_ENTITY_TYPE } from '../../entity.js';
import type {
	UmbDocumentTreeChildrenOfRequestArgs,
	UmbDocumentTreeItemModel,
	UmbDocumentTreeRootItemsRequestArgs,
} from '../types.js';
import { UmbManagementApiDocumentTreeDataRequestManager } from './document-tree.server.request-manager.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { DocumentTreeItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbTreeAncestorsOfRequestArgs, UmbTreeDataSource } from '@umbraco-cms/backoffice/tree';

/**
 * A data source for the Document tree that fetches data from the server
 * @class UmbDocumentTreeServerDataSource
 */
export class UmbDocumentTreeServerDataSource
	extends UmbControllerBase
	implements UmbTreeDataSource<UmbDocumentTreeItemModel>
{
	#treeRequestManager = new UmbManagementApiDocumentTreeDataRequestManager(this);

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
}
