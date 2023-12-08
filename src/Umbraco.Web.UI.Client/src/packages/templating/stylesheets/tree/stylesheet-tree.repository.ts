import { UmbStylesheetTreeServerDataSource } from './stylesheet-tree.server.data-source.js';
import { UMB_STYLESHEET_TREE_STORE_CONTEXT_TOKEN } from './stylesheet-tree.store.js';
import { UmbTreeRepositoryBase } from '@umbraco-cms/backoffice/tree';
import { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbStylesheetTreeRepository extends UmbTreeRepositoryBase<any, any> {
	constructor(host: UmbControllerHost) {
		super(host, UmbStylesheetTreeServerDataSource, UMB_STYLESHEET_TREE_STORE_CONTEXT_TOKEN);
	}

	async requestTreeRoot() {
		const data = {
			path: null,
			type: 'stylesheet-root',
			name: 'Stylesheets',
			icon: 'icon-folder',
			hasChildren: true,
			isContainer: false,
		};

		return { data };
	}
}
