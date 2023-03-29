import { UmbStylesheetTreeStore, UMB_STYLESHEET_TREE_STORE_CONTEXT_TOKEN } from './stylesheet.tree.store';
import { UmbStylesheetTreeServerDataSource } from './sources/stylesheet.tree.server.data';
import { UmbStylesheetServerDataSource } from './sources/stylesheet.server.data';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller';
import { UmbNotificationContext, UMB_NOTIFICATION_CONTEXT_TOKEN } from '@umbraco-cms/backoffice/notification';
import { UmbContextConsumerController } from '@umbraco-cms/backoffice/context-api';
import { UmbTreeRepository } from '@umbraco-cms/backoffice/repository';
import {
	FileSystemTreeItemPresentationModel,
	PagedFileSystemTreeItemPresentationModel,
} from '@umbraco-cms/backoffice/backend-api';

export class UmbStylesheetRepository
	implements UmbTreeRepository<FileSystemTreeItemPresentationModel, PagedFileSystemTreeItemPresentationModel>
{
	#host;
	#dataSource;
	#treeDataSource;
	#treeStore?: UmbStylesheetTreeStore;
	#notificationContext?: UmbNotificationContext;
	#initResolver?: () => void;
	#initialized = false;

	constructor(host: UmbControllerHostElement) {
		this.#host = host;

		// TODO: figure out how spin up get the correct data source
		this.#dataSource = new UmbStylesheetServerDataSource(this.#host);
		this.#treeDataSource = new UmbStylesheetTreeServerDataSource(this.#host);

		new UmbContextConsumerController(this.#host, UMB_STYLESHEET_TREE_STORE_CONTEXT_TOKEN, (instance) => {
			this.#treeStore = instance;
			this.#checkIfInitialized();
		});

		new UmbContextConsumerController(this.#host, UMB_NOTIFICATION_CONTEXT_TOKEN, (instance) => {
			this.#notificationContext = instance;
			this.#checkIfInitialized();
		});
	}

	#init = new Promise<void>((resolve) => {
		this.#initialized ? resolve() : (this.#initResolver = resolve);
	});

	#checkIfInitialized() {
		if (this.#treeStore && this.#notificationContext) {
			this.#initialized = true;
			this.#initResolver?.();
		}
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
		if (!path) throw new Error('Cannot request tree item with missing path');

		await this.#init;

		const { data, error } = await this.#treeDataSource.getChildrenOf(path);

		if (data) {
			this.#treeStore!.appendItems(data.items);
		}

		return { data, error, asObservable: () => this.#treeStore!.childrenOf(path) };
	}

	async requestTreeItems(paths: Array<string>) {
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

	async treeItems(paths: Array<string>) {
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
