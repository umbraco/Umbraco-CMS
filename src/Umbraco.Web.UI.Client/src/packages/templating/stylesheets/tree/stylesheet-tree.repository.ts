import { UMB_STYLESHEET_ROOT_ENTITY_TYPE } from '../entity.js';
import { UmbStylesheetTreeServerDataSource } from './stylesheet-tree.server.data-source.js';
import { UMB_STYLESHEET_TREE_STORE_CONTEXT } from './stylesheet-tree.store.js';
import type { UmbStylesheetTreeItemModel, UmbStylesheetTreeRootModel } from './types.js';
import { UmbTreeRepositoryBase } from '@umbraco-cms/backoffice/tree';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbStylesheetTreeRepository extends UmbTreeRepositoryBase<
	UmbStylesheetTreeItemModel,
	UmbStylesheetTreeRootModel
> {
	constructor(host: UmbControllerHost) {
		super(host, UmbStylesheetTreeServerDataSource, UMB_STYLESHEET_TREE_STORE_CONTEXT);
	}

	async requestTreeRoot() {
		const { data: treeRootData } = await this._treeSource.getRootItems({ skip: 0, take: 1 });
		const hasChildren = treeRootData ? treeRootData.total > 0 : false;

		const data: UmbStylesheetTreeRootModel = {
			unique: null,
			entityType: UMB_STYLESHEET_ROOT_ENTITY_TYPE,
			name: '#treeHeaders_stylesheets',
			hasChildren,
			isFolder: true,
		};

		return { data };
	}
}

export default UmbStylesheetTreeRepository;
