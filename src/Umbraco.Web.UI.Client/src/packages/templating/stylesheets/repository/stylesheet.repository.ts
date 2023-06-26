import { UmbStylesheetTreeStore, UMB_STYLESHEET_TREE_STORE_CONTEXT_TOKEN } from './stylesheet.tree.store.js';
import { UmbStylesheetTreeServerDataSource } from './sources/stylesheet.tree.server.data.js';
import { UmbStylesheetServerDataSource } from './sources/stylesheet.server.data.js';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextConsumerController } from '@umbraco-cms/backoffice/context-api';
import { UmbTreeRepository } from '@umbraco-cms/backoffice/repository';
import { FileSystemTreeItemPresentationModel } from '@umbraco-cms/backoffice/backend-api';
import type { UmbTreeRootFileSystemModel } from '@umbraco-cms/backoffice/tree';

export class UmbStylesheetRepository
	implements UmbTreeRepository<FileSystemTreeItemPresentationModel, UmbTreeRootFileSystemModel>
{
	#host;
	#dataSource;
	#treeDataSource;
	#treeStore?: UmbStylesheetTreeStore;
	#init;

	constructor(host: UmbControllerHostElement) {
		this.#host = host;

		// TODO: figure out how spin up get the correct data source
		this.#dataSource = new UmbStylesheetServerDataSource(this.#host);
		this.#treeDataSource = new UmbStylesheetTreeServerDataSource(this.#host);

		this.#init = new UmbContextConsumerController(this.#host, UMB_STYLESHEET_TREE_STORE_CONTEXT_TOKEN, (instance) => {
			this.#treeStore = instance;
		}).asPromise();
	}

	// TREE:
	async requestTreeRoot() {
		await this.#init;

		const data = {
			path: null,
			type: 'stylesheet-root',
			name: 'Stylesheets',
			icon: 'umb:folder',
			hasChildren: true,
		};

		return { data };
	}

	async requestRootTreeItems() {
		await this.#init;

		const { data, error } = await this.#treeDataSource.getRootItems();

		if (data) {
			this.#treeStore?.appendItems(data.items);
		}

		return { data, error };
	}

	async requestTreeItemsOf(path: string | null) {
		if (path === undefined) throw new Error('Cannot request tree item with missing path');

		await this.#init;

		const { data, error } = await this.#treeDataSource.getChildrenOf(path);

		if (data) {
			this.#treeStore!.appendItems(data.items);
		}

		return { data, error, asObservable: () => this.#treeStore!.childrenOf(path) };
	}

	async requestItemsLegacy(paths: Array<string>) {
		if (!paths) throw new Error('Paths are missing');
		await this.#init;
		const { data, error } = await this.#treeDataSource.getItems(paths);
		return { data, error };
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

	async itemsLegacy(paths: Array<string>) {
		if (!paths) throw new Error('Paths are missing');
		await this.#init;
		return this.#treeStore!.items(paths);
	}

	// DETAILS
	async requestByPath(path: string) {
		if (!path) throw new Error('Path is missing');
		await this.#init;
		const { data, error } = await this.#dataSource.get(path);
		return { data, error };
	}
}
