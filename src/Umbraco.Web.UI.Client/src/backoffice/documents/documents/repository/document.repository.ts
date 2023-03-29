import { UmbDocumentServerDataSource } from './sources/document.server.data';
import { UmbDocumentStore, UMB_DOCUMENT_STORE_CONTEXT_TOKEN } from './document.store';
import { UmbDocumentTreeStore, UMB_DOCUMENT_TREE_STORE_CONTEXT_TOKEN } from './document.tree.store';
import { DocumentTreeServerDataSource } from './sources/document.tree.server.data';
import type { UmbTreeDataSource, UmbTreeRepository, UmbDetailRepository } from '@umbraco-cms/backoffice/repository';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller';
import { UmbContextConsumerController } from '@umbraco-cms/backoffice/context-api';
import { ProblemDetailsModel, DocumentResponseModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbNotificationContext, UMB_NOTIFICATION_CONTEXT_TOKEN } from '@umbraco-cms/backoffice/notification';

type ItemType = DocumentResponseModel;

export class UmbDocumentRepository implements UmbTreeRepository<ItemType>, UmbDetailRepository<ItemType> {
	#init!: Promise<unknown>;

	#host: UmbControllerHostElement;

	#treeSource: UmbTreeDataSource;
	#treeStore?: UmbDocumentTreeStore;

	#detailDataSource: UmbDocumentServerDataSource;
	#store?: UmbDocumentStore;

	#notificationContext?: UmbNotificationContext;

	constructor(host: UmbControllerHostElement) {
		this.#host = host;

		// TODO: figure out how spin up get the correct data source
		this.#treeSource = new DocumentTreeServerDataSource(this.#host);
		this.#detailDataSource = new UmbDocumentServerDataSource(this.#host);

		this.#init = Promise.all([
			new UmbContextConsumerController(this.#host, UMB_DOCUMENT_TREE_STORE_CONTEXT_TOKEN, (instance) => {
				this.#treeStore = instance;
			}),

			new UmbContextConsumerController(this.#host, UMB_DOCUMENT_STORE_CONTEXT_TOKEN, (instance) => {
				this.#store = instance;
			}),

			new UmbContextConsumerController(this.#host, UMB_NOTIFICATION_CONTEXT_TOKEN, (instance) => {
				this.#notificationContext = instance;
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

	async createScaffold(documentTypeKey: string) {
		if (!documentTypeKey) throw new Error('Document type key is missing');
		await this.#init;
		return this.#detailDataSource.createScaffold(documentTypeKey);
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
			this.#store?.append(data);
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
			this.#notificationContext?.peek('positive', notification);
		}

		// TODO: we currently don't use the detail store for anything.
		// Consider to look up the data before fetching from the server
		this.#store?.append(item);
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
			this.#notificationContext?.peek('positive', notification);
		}

		// TODO: we currently don't use the detail store for anything.
		// Consider to look up the data before fetching from the server
		// Consider notify a workspace if a document is updated in the store while someone is editing it.
		this.#store?.append(item);
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
			this.#notificationContext?.peek('positive', notification);
		}

		// TODO: we currently don't use the detail store for anything.
		// Consider to look up the data before fetching from the server.
		// Consider notify a workspace if a document is deleted from the store while someone is editing it.
		this.#store?.remove([key]);
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
