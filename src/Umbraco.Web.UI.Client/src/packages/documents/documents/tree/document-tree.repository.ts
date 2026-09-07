import { UMB_DOCUMENT_ENTITY_TYPE, UMB_DOCUMENT_ROOT_ENTITY_TYPE } from '../entity.js';
import { UmbDocumentTreeItemDataResolver } from './document-tree-item-data-resolver.js';
import { UmbDocumentTreeServerDataSource } from './server-data-source/document-tree.server.data-source.js';
import type { UmbDocumentTreeItemModel, UmbDocumentTreeRootModel } from './types.js';
import { UmbTreeRepositoryBase } from '@umbraco-cms/backoffice/tree';
import type {
	UmbCreateTreeItemDataResolverArgs,
	UmbTreeItemDataResolver,
	UmbTreeRepository,
} from '@umbraco-cms/backoffice/tree';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbDocumentTreeRepository
	extends UmbTreeRepositoryBase<UmbDocumentTreeItemModel, UmbDocumentTreeRootModel>
	implements UmbApi, UmbTreeRepository<UmbDocumentTreeItemModel, UmbDocumentTreeRootModel>
{
	constructor(host: UmbControllerHost) {
		super(host, UmbDocumentTreeServerDataSource);
	}

	async requestTreeRoot() {
		const { data: treeRootData } = await this._treeSource.getRootItems({ paging: { skip: 0, take: 0 } });
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
