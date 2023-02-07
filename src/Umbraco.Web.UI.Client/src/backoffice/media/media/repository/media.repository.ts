import type { RepositoryTreeDataSource } from '../../../../../libs/repository/repository-tree-data-source.interface';
import { MediaTreeServerDataSource } from './sources/media.tree.server.data';
import { UmbMediaTreeStore, UMB_MEDIA_TREE_STORE_CONTEXT_TOKEN } from './media.tree.store';
import { UmbMediaDetailStore, UMB_MEDIA_DETAIL_STORE_CONTEXT_TOKEN } from './media.detail.store';
import { UmbMediaDetailServerDataSource } from './sources/media.detail.server.data';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';
import { UmbContextConsumerController } from '@umbraco-cms/context-api';
import { ProblemDetails } from '@umbraco-cms/backend-api';
import type { UmbTreeRepository } from 'libs/repository/tree-repository.interface';
import { UmbDetailRepository } from '@umbraco-cms/repository';
import type { MediaDetails } from '@umbraco-cms/models';
import { UmbNotificationService, UMB_NOTIFICATION_SERVICE_CONTEXT_TOKEN } from '@umbraco-cms/notification';

type ItemDetailType = MediaDetails;

// Move to documentation / JSdoc
/* We need to create a new instance of the repository from within the element context. We want the notifications to be displayed in the right context. */
// element -> context -> repository -> (store) -> data source
// All methods should be async and return a promise. Some methods might return an observable as part of the promise response.
export class UmbMediaRepository implements UmbTreeRepository, UmbDetailRepository<ItemDetailType> {
	#init!: Promise<unknown>;

	#host: UmbControllerHostInterface;

	#treeSource: RepositoryTreeDataSource;
	#treeStore?: UmbMediaTreeStore;

	#detailDataSource: UmbMediaDetailServerDataSource;
	#detailStore?: UmbMediaDetailStore;

	#notificationService?: UmbNotificationService;

	constructor(host: UmbControllerHostInterface) {
		this.#host = host;

		// TODO: figure out how spin up get the correct data source
		this.#treeSource = new MediaTreeServerDataSource(this.#host);
		this.#detailDataSource = new UmbMediaDetailServerDataSource(this.#host);

		this.#init = Promise.all([
			new UmbContextConsumerController(this.#host, UMB_MEDIA_TREE_STORE_CONTEXT_TOKEN, (instance) => {
				this.#treeStore = instance;
			}),

			new UmbContextConsumerController(this.#host, UMB_MEDIA_DETAIL_STORE_CONTEXT_TOKEN, (instance) => {
				this.#detailStore = instance;
			}),

			new UmbContextConsumerController(this.#host, UMB_NOTIFICATION_SERVICE_CONTEXT_TOKEN, (instance) => {
				this.#notificationService = instance;
			}),
		]);
	}

	async requestRootTreeItems() {
		await this.#init;

		const { data, error } = await this.#treeSource.getRootItems();

		if (data) {
			this.#treeStore?.appendItems(data.items);
		}

		return { data, error, asObservable: () => this.#treeStore!.rootItems };
	}

	async requestTreeItemsOf(parentKey: string | null) {
		await this.#init;

		if (!parentKey) {
			const error: ProblemDetails = { title: 'Parent key is missing' };
			return { data: undefined, error };
		}

		const { data, error } = await this.#treeSource.getChildrenOf(parentKey);

		if (data) {
			this.#treeStore?.appendItems(data.items);
		}

		return { data, error, asObservable: () => this.#treeStore!.childrenOf(parentKey) };
	}

	async requestTreeItems(keys: Array<string>) {
		await this.#init;

		if (!keys) {
			const error: ProblemDetails = { title: 'Keys are missing' };
			return { data: undefined, error };
		}

		const { data, error } = await this.#treeSource.getItems(keys);

		return { data, error, asObservable: () => this.#treeStore!.items(keys) };
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

	// DETAILS:

	async createDetailsScaffold(parentKey: string | null) {
		await this.#init;

		if (!parentKey) {
			throw new Error('Parent key is missing');
		}

		return this.#detailDataSource.createScaffold(parentKey);
	}

	async requestDetails(key: string) {
		await this.#init;

		// TODO: should we show a notification if the key is missing?
		// Investigate what is best for Acceptance testing, cause in that perspective a thrown error might be the best choice?
		if (!key) {
			const error: ProblemDetails = { title: 'Key is missing' };
			return { error };
		}
		const { data, error } = await this.#detailDataSource.get(key);

		if (data) {
			this.#detailStore?.append(data);
		}

		return { data, error };
	}

	// Could potentially be general methods:

	async createDetail(template: ItemDetailType) {
		await this.#init;

		if (!template || !template.key) {
			throw new Error('Template is missing');
		}

		const { error } = await this.#detailDataSource.insert(template);

		if (!error) {
			const notification = { data: { message: `Media created` } };
			this.#notificationService?.peek('positive', notification);
		}

		// TODO: we currently don't use the detail store for anything.
		// Consider to look up the data before fetching from the server
		this.#detailStore?.append(template);
		// TODO: Update tree store with the new item? or ask tree to request the new item?

		return { error };
	}

	async saveDetail(document: ItemDetailType) {
		await this.#init;

		if (!document || !document.key) {
			throw new Error('Template is missing');
		}

		const { error } = await this.#detailDataSource.update(document);

		if (!error) {
			const notification = { data: { message: `Document saved` } };
			this.#notificationService?.peek('positive', notification);
		}

		// TODO: we currently don't use the detail store for anything.
		// Consider to look up the data before fetching from the server
		// Consider notify a workspace if a template is updated in the store while someone is editing it.
		this.#detailStore?.append(document);
		this.#treeStore?.updateItem(document.key, { name: document.name });

		// TODO: would be nice to align the stores on methods/methodNames.

		return { error };
	}

	// General:
	async delete(key: string) {
		await this.#init;

		if (!key) {
			throw new Error('Document key is missing');
		}

		const { error } = await this.#detailDataSource.delete(key);

		if (!error) {
			const notification = { data: { message: `Document deleted` } };
			this.#notificationService?.peek('positive', notification);
		}

		// TODO: we currently don't use the detail store for anything.
		// Consider to look up the data before fetching from the server.
		// Consider notify a workspace if a template is deleted from the store while someone is editing it.
		this.#detailStore?.remove([key]);
		this.#treeStore?.removeItem(key);
		// TODO: would be nice to align the stores on methods/methodNames.

		return { error };
	}

	async trash() {
		alert('trash');
	}

	async move() {
		alert('move');
	}

	async copy() {
		alert('copy');
	}

	async sortChildrenOf() {
		alert('sort');
	}
}
