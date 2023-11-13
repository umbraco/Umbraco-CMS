import { UmbStylesheetTreeServerDataSource } from './stylesheet-tree.server.data-source.js';
import { UMB_STYLESHEET_TREE_STORE_CONTEXT_TOKEN, UmbStylesheetTreeStore } from './stylesheet-tree.store.js';
import { FileSystemTreeItemPresentationModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbRepositoryBase, UmbTreeRepository } from '@umbraco-cms/backoffice/repository';
import { UmbTreeRootFileSystemModel } from '@umbraco-cms/backoffice/tree';
import { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbStylesheetTreeRepository
	extends UmbRepositoryBase
	implements UmbTreeRepository<FileSystemTreeItemPresentationModel, UmbTreeRootFileSystemModel>
{
	#init;
	#treeDataSource;
	#treeStore?: UmbStylesheetTreeStore;

	constructor(host: UmbControllerHost) {
		super(host);
		this.#treeDataSource = new UmbStylesheetTreeServerDataSource(this);

		this.#init = this.consumeContext(UMB_STYLESHEET_TREE_STORE_CONTEXT_TOKEN, (instance) => {
			this.#treeStore = instance;
		}).asPromise();
	}
	async requestTreeRoot() {
		await this.#init;

		const data = {
			path: null,
			type: 'stylesheet-root',
			name: 'Stylesheets',
			icon: 'icon-folder',
			hasChildren: true,
		};

		return { data };
	}

	async requestRootTreeItems() {
		console.log('stylesheet root');
		await this.#init;

		const { data, error } = await this.#treeDataSource.getRootItems();

		if (data) {
			this.#treeStore?.appendItems(data.items);
		}

		return { data, error, asObservable: () => this.#treeStore!.rootItems };
	}

	async requestTreeItemsOf(path: string | null) {
		if (path === undefined) throw new Error('Cannot request tree item with missing path');
		if (path === null || path === '/' || path === '') {
			return this.requestRootTreeItems();
		}

		await this.#init;

		const { data, error } = await this.#treeDataSource.getChildrenOf(path);

		if (data) {
			this.#treeStore!.appendItems(data.items);
		}

		return { data, error, asObservable: () => this.#treeStore!.childrenOf(path) };
	}

	async rootTreeItems() {
		await this.#init;
		return this.#treeStore!.rootItems;
	}

	async treeItemsOf(parentPath: string | null) {
		if (!parentPath) throw new Error('Parent Path is missing');
		await this.#init;
		return this.#treeStore!.childrenOf(parentPath);
	}
}
