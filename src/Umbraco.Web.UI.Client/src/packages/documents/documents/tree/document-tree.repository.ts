import { UMB_DOCUMENT_ROOT_ENTITY_TYPE } from '../entity.js';
import { UmbDocumentTreeServerDataSource } from './document-tree.server.data-source.js';
import { UmbDocumentTreeItemModel, UmbDocumentTreeRootModel } from './types.js';
import { UMB_DOCUMENT_TREE_STORE_CONTEXT } from './document-tree.store.js';
import { UmbTreeRepositoryBase } from '@umbraco-cms/backoffice/tree';
import { type UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export class UmbDocumentTreeRepository
	extends UmbTreeRepositoryBase<UmbDocumentTreeItemModel, UmbDocumentTreeRootModel>
	implements UmbApi
{
	constructor(host: UmbControllerHost) {
		super(host, UmbDocumentTreeServerDataSource, UMB_DOCUMENT_TREE_STORE_CONTEXT);
	}

	async requestTreeRoot() {
		const data = {
			id: null,
			entityType: UMB_DOCUMENT_ROOT_ENTITY_TYPE,
			name: 'Documents',
			icon: 'icon-folder',
			hasChildren: true,
			isContainer: false,
			isFolder: false,
		};

		return { data };
	}
}
