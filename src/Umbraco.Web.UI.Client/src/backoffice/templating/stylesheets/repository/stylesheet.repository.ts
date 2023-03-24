import { UmbStylesheetTreeStore, UMB_STYLESHEET_TREE_STORE_CONTEXT_TOKEN } from './stylesheet.tree.store';
import { UmbStylesheetTreeServerDataSource } from './sources/stylesheet.tree.server.data';
import { UmbControllerHostInterface } from '@umbraco-cms/backoffice/controller';
import { UmbNotificationContext, UMB_NOTIFICATION_CONTEXT_TOKEN } from '@umbraco-cms/backoffice/notification';
import { UmbContextConsumerController } from '@umbraco-cms/backoffice/context-api';
import { UmbTreeRepository } from '@umbraco-cms/backoffice/repository';
import {
	FileSystemTreeItemPresentationModel,
	PagedFileSystemTreeItemPresentationModel,
	ProblemDetailsModel,
} from '@umbraco-cms/backoffice/backend-api';

export class UmbStylesheetRepository
	implements UmbTreeRepository<PagedFileSystemTreeItemPresentationModel, FileSystemTreeItemPresentationModel>
{
	#host: UmbControllerHostInterface;
	#treeDataSource: UmbStylesheetTreeServerDataSource;
	#treeStore?: UmbStylesheetTreeStore;
	#notificationContext?: UmbNotificationContext;
	#initResolver?: () => void;
	#initialized = false;

	constructor(host: UmbControllerHostInterface) {
		this.#host = host;
		// TODO: figure out how spin up get the correct data source
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

	async requestTreeItemsOf(path: string | null) {}

	async requestTreeItems(keys: Array<string>) {
		await this.#init;

		if (!keys) {
			const error: ProblemDetailsModel = { title: 'Keys are missing' };
			return { data: undefined, error };
		}

		const { data, error } = await this.#treeDataSource.getItems(keys);

		return { data, error };
	}

	async rootTreeItems() {
		await this.#init;
		return this.#treeStore!.rootItems;
	}

	async treeItemsOf(parentKey: string | null) {
		await this.#init;
		return this.#treeStore!.childrenOf(parentKey);
	}

	async treeItems(keys: Array<string>) {
		await this.#init;
		return this.#treeStore!.items(keys);
	}
}
