import type { RepositoryTreeDataSource } from '../../../../../libs/repository/repository-tree-data-source.interface';
import { DocumentTreeServerDataSource } from './sources/document.tree.server.data';
import { UmbDocumentTreeStore, UMB_DOCUMENT_TREE_STORE_CONTEXT_TOKEN } from './document.tree.store';
import { UmbDocumentStore, UMB_DOCUMENT_DETAIL_STORE_CONTEXT_TOKEN } from './document.store';
import { UmbDocumentServerDataSource } from './sources/document.server.data';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';
import { UmbContextConsumerController } from '@umbraco-cms/context-api';
import { ProblemDetailsModel, DocumentModel } from '@umbraco-cms/backend-api';
import type { UmbTreeRepository } from 'libs/repository/tree-repository.interface';
import { UmbDetailRepository } from '@umbraco-cms/repository';
import { UmbNotificationContext, UMB_NOTIFICATION_SERVICE_CONTEXT_TOKEN } from '@umbraco-cms/notification';

type ItemType = DocumentModel;

// Move to documentation / JSdoc
/* We need to create a new instance of the repository from within the element context. We want the notifications to be displayed in the right context. */
// element -> context -> repository -> (store) -> data source
// All methods should be async and return a promise. Some methods might return an observable as part of the promise response.
export class UmbDocumentRepository implements UmbTreeRepository, UmbDetailRepository<ItemType> {
	#init!: Promise<unknown>;

	#host: UmbControllerHostInterface;

	#treeSource: RepositoryTreeDataSource;
	#treeStore?: UmbDocumentTreeStore;

	#detailDataSource: UmbDocumentServerDataSource;
	#detailStore?: UmbDocumentStore;

	#notificationService?: UmbNotificationContext;

	constructor(host: UmbControllerHostInterface) {
		this.#host = host;

		// TODO: figure out how spin up get the correct data source
		this.#treeSource = new DocumentTreeServerDataSource(this.#host);
		this.#detailDataSource = new UmbDocumentServerDataSource(this.#host);

		this.#init = Promise.all([
			new UmbContextConsumerController(this.#host, UMB_DOCUMENT_TREE_STORE_CONTEXT_TOKEN, (instance) => {
				this.#treeStore = instance;
			}),

			new UmbContextConsumerController(this.#host, UMB_DOCUMENT_DETAIL_STORE_CONTEXT_TOKEN, (instance) => {
				this.#detailStore = instance;
			}),

			new UmbContextConsumerController(this.#host, UMB_NOTIFICATION_SERVICE_CONTEXT_TOKEN, (instance) => {
				this.#notificationService = instance;
			}),
		]);
	}

	// TODO: Trash
	// TODO: Move

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
			const error: ProblemDetailsModel = { title: 'Parent key is missing' };
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
			const error: ProblemDetailsModel = { title: 'Keys are missing' };
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

	async createScaffold(parentKey: string | null) {
		await this.#init;

		if (!parentKey) {
			throw new Error('Parent key is missing');
		}

		return this.#detailDataSource.createScaffold(parentKey);
	}

	async requestByKey(key: string) {
		await this.#init;

		// TODO: should we show a notification if the key is missing?
		// Investigate what is best for Acceptance testing, cause in that perspective a thrown error might be the best choice?
		if (!key) {
			const error: ProblemDetailsModel = { title: 'Key is missing' };
			return { error };
		}
		const { data, error } = await this.#detailDataSource.get(key);

		if (data) {
			this.#detailStore?.append(data);
		}

		return { data, error };
	}

	// Could potentially be general methods:

	async create(item: ItemType) {
		await this.#init;

		if (!item || !item.key) {
			throw new Error('Document is missing');
		}

		const { error } = await this.#detailDataSource.insert(item);

		if (!error) {
			const notification = { data: { message: `Document created` } };
			this.#notificationService?.peek('positive', notification);
		}

		// TODO: we currently don't use the detail store for anything.
		// Consider to look up the data before fetching from the server
		this.#detailStore?.append(item);
		// TODO: Update tree store with the new item? or ask tree to request the new item?

		return { error };
	}

	async save(item: ItemType) {
		await this.#init;

		if (!item || !item.key) {
			throw new Error('Document is missing');
		}

		const { error } = await this.#detailDataSource.update(item);

		if (!error) {
			const notification = { data: { message: `Document saved` } };
			this.#notificationService?.peek('positive', notification);
		}

		// TODO: we currently don't use the detail store for anything.
		// Consider to look up the data before fetching from the server
		// Consider notify a workspace if a document is updated in the store while someone is editing it.
		this.#detailStore?.append(item);
		//this.#treeStore?.updateItem(item.key, { name: item.name });// Port data to tree store.
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
		// Consider notify a workspace if a document is deleted from the store while someone is editing it.
		this.#detailStore?.remove([key]);
		this.#treeStore?.removeItem(key);
		// TODO: would be nice to align the stores on methods/methodNames.

		return { error };
	}

	// Listing all currently known methods we need to implement:
	// these currently only covers posting data
	// TODO: find a good way to split these
	async trash(keys: Array<string>) {
		console.log('document trash: ' + keys);
		alert('implement trash');
	}

	async saveAndPublish() {
		alert('save and publish');
	}

	async saveAndPreview() {
		alert('save and preview');
	}

	async saveAndSchedule() {
		alert('save and schedule');
	}

	async createBlueprint() {
		alert('create document blueprint');
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

	async setCultureAndHostnames() {
		alert('set culture and hostnames');
	}

	async setPermissions() {
		alert('set permissions');
	}

	async setPublicAccess() {
		alert('set public access');
	}

	async publish() {
		alert('publish');
	}

	async unpublish() {
		alert('unpublish');
	}

	async rollback() {
		alert('rollback');
	}
}
