import { UMB_DOCUMENT_ENTITY_TYPE, UMB_DOCUMENT_ROOT_ENTITY_TYPE } from '../entity.js';
import { UmbDocumentTreeItemDataResolver } from './document-tree-item-data-resolver.js';
import { UmbDocumentTreeServerDataSource } from './server-data-source/document-tree.server.data-source.js';
import { UMB_DOCUMENT_TREE_STORE_CONTEXT } from './document-tree.store.context-token.js';
import type {
	UmbDocumentAncestorsOfTreeItemsRequestArgs,
	UmbDocumentTreeItemModel,
	UmbDocumentTreeRootModel,
} from './types.js';
import { UmbTreeRepositoryBase } from '@umbraco-cms/backoffice/tree';
import type {
	UmbCreateTreeItemDataResolverArgs,
	UmbAncestorsOfTreeItemModel,
	UmbTreeItemDataResolver,
	UmbTreeRepository,
} from '@umbraco-cms/backoffice/tree';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbRepositoryResponse } from '@umbraco-cms/backoffice/repository';

export class UmbDocumentTreeRepository
	extends UmbTreeRepositoryBase<UmbDocumentTreeItemModel, UmbDocumentTreeRootModel>
	implements UmbApi, UmbTreeRepository<UmbDocumentTreeItemModel, UmbDocumentTreeRootModel>
{
	#documentTreeSource: UmbDocumentTreeServerDataSource;

	constructor(host: UmbControllerHost) {
		super(host, UmbDocumentTreeServerDataSource, UMB_DOCUMENT_TREE_STORE_CONTEXT);
		this.#documentTreeSource = this._treeSource as UmbDocumentTreeServerDataSource;
	}

	async requestTreeRoot() {
		const { data: treeRootData } = await this._treeSource.getRootItems({ skip: 0, take: 0 });
		const hasChildren = treeRootData ? treeRootData.total > 0 : false;

		const data: UmbDocumentTreeRootModel = {
			unique: null,
			entityType: UMB_DOCUMENT_ROOT_ENTITY_TYPE,
			name: '#treeHeaders_content',
			hasChildren,
			isFolder: true,
		};

		return { data };
	}

	/**
	 * Requests the ancestors of each of the given items, ordered from the root down. Items whose ancestors cannot be
	 * returned are left out.
	 * @param {UmbDocumentAncestorsOfTreeItemsRequestArgs} args - The request arguments
	 * @returns {Promise<UmbRepositoryResponse<Array<UmbAncestorsOfTreeItemModel<UmbDocumentTreeItemModel>>>>} The ancestors of each item
	 * @memberof UmbDocumentTreeRepository
	 */
	async requestAncestorsOfItems(
		args: UmbDocumentAncestorsOfTreeItemsRequestArgs,
	): Promise<UmbRepositoryResponse<Array<UmbAncestorsOfTreeItemModel<UmbDocumentTreeItemModel>>>> {
		return this.#documentTreeSource.getAncestorsOfItems(args);
	}

	createTreeItemDataResolver(
		host: UmbControllerHost,
		args?: UmbCreateTreeItemDataResolverArgs,
	): UmbTreeItemDataResolver<UmbDocumentTreeItemModel> | undefined {
		if (args?.entityType === UMB_DOCUMENT_ENTITY_TYPE) {
			return new UmbDocumentTreeItemDataResolver(host);
		}

		return undefined;
	}
}

export default UmbDocumentTreeRepository;
