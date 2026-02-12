import { UMB_DICTIONARY_ROOT_ENTITY_TYPE } from '../entity.js';
import { UmbDictionaryTreeServerDataSource } from './dictionary-tree.server.data-source.js';
import type { UmbDictionaryTreeItemModel, UmbDictionaryTreeRootModel } from './types.js';
import { UmbTreeRepositoryBase } from '@umbraco-cms/backoffice/tree';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export class UmbDictionaryTreeRepository
	extends UmbTreeRepositoryBase<UmbDictionaryTreeItemModel, UmbDictionaryTreeRootModel>
	implements UmbApi
{
	constructor(host: UmbControllerHost) {
		super(host, UmbDictionaryTreeServerDataSource);
	}

	async requestTreeRoot() {
		const { data: treeRootData } = await this._treeSource.getRootItems({ paging: { skip: 0, take: 0 } });
		const hasChildren = treeRootData ? treeRootData.total > 0 : false;

		const data: UmbDictionaryTreeRootModel = {
			unique: null,
			entityType: UMB_DICTIONARY_ROOT_ENTITY_TYPE,
			name: '#treeHeaders_dictionary',
			hasChildren,
			isFolder: true,
		};

		return { data };
	}
}

export { UmbDictionaryTreeRepository as api };
