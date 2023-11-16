import { UMB_DOCUMENT_RECYCLE_BIN_ROOT_ENTITY_TYPE } from '../entities.js';
import { UmbDocumentRecycleBinTreeServerDataSource } from './document-recycle-bin-tree.server.data-source.js';
import { UmbDocumentRecycleBinTreeItemModel, UmbDocumentRecycleBinTreeRootModel } from './types.js';
import { UMB_DOCUMENT_RECYCLE_BIN_TREE_STORE_CONTEXT } from './document-recycle-bin-tree.store.js';
import { UmbTreeRepositoryBase } from '@umbraco-cms/backoffice/tree';
import { type UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export class UmbDocumentRecycleBinTreeRepository
	extends UmbTreeRepositoryBase<UmbDocumentRecycleBinTreeItemModel, UmbDocumentRecycleBinTreeRootModel>
	implements UmbApi
{
	constructor(host: UmbControllerHost) {
		super(host, UmbDocumentRecycleBinTreeServerDataSource, UMB_DOCUMENT_RECYCLE_BIN_TREE_STORE_CONTEXT);
	}

	async requestTreeRoot() {
		const data = {
			id: null,
			type: UMB_DOCUMENT_RECYCLE_BIN_ROOT_ENTITY_TYPE,
			name: 'Recycle Bin',
			icon: 'icon-trash',
			hasChildren: true,
		};

		return { data };
	}
}
