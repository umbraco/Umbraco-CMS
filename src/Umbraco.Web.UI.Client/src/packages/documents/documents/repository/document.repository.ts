import { UmbDocumentServerDataSource } from './sources/document.server.data.js';
import { UmbDocumentStore, UMB_DOCUMENT_STORE_CONTEXT_TOKEN } from './document.store.js';
import { UmbDocumentTreeStore, UMB_DOCUMENT_TREE_STORE_CONTEXT_TOKEN } from './document.tree.store.js';
import { UmbDocumentTreeServerDataSource } from './sources/document.tree.server.data.js';
import type { UmbTreeDataSource, UmbTreeRepository, UmbDetailRepository } from '@umbraco-cms/backoffice/repository';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextConsumerController } from '@umbraco-cms/backoffice/context-api';
import {
	DocumentResponseModel,
	CreateDocumentRequestModel,
	UpdateDocumentRequestModel,
	DocumentTreeItemResponseModel,
} from '@umbraco-cms/backoffice/backend-api';
import { UmbNotificationContext, UMB_NOTIFICATION_CONTEXT_TOKEN } from '@umbraco-cms/backoffice/notification';

export class UmbDocumentRepository
	implements
		UmbTreeRepository<DocumentTreeItemResponseModel>,
		UmbDetailRepository<CreateDocumentRequestModel, any, UpdateDocumentRequestModel, DocumentResponseModel>
{
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
		this.#treeSource = new UmbDocumentTreeServerDataSource(this.#host);
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

	// TREE:
	async requestTreeRoot() {
		await this.#init;

		const data = {
			id: null,
			type: 'document-root',
			name: 'Documents',
			icon: 'umb:folder',
			hasChildren: true,
		};

		return { data };
	}

	async requestRootTreeItems() {
		await this.#init;

		const { data, error } = await this.#treeSource.getRootItems();

		if (data) {
			this.#treeStore?.appendItems(data.items);
		}

		return { data, error, asObservable: () => this.#treeStore!.rootItems };
	}

	async requestTreeItemsOf(parentId: string | null) {
		await this.#init;
		if (parentId === undefined) throw new Error('Parent id is missing');

		const { data, error } = await this.#treeSource.getChildrenOf(parentId);

		if (data) {
			this.#treeStore?.appendItems(data.items);
		}

		return { data, error, asObservable: () => this.#treeStore!.childrenOf(parentId) };
	}

	async requestItemsLegacy(ids: Array<string>) {
		await this.#init;

		if (!ids) {
			throw new Error('Ids are missing');
		}

		const { data, error } = await this.#treeSource.getItems(ids);

		return { data, error, asObservable: () => this.#treeStore!.items(ids) };
	}

	async rootTreeItems() {
		await this.#init;
		return this.#treeStore!.rootItems;
	}

	async treeItemsOf(parentId: string | null) {
		await this.#init;
		return this.#treeStore!.childrenOf(parentId);
	}

	async itemsLegacy(ids: Array<string>) {
		await this.#init;
		return this.#treeStore!.items(ids);
	}

	// DETAILS:

	async createScaffold(documentTypeKey: string, preset?: Partial<CreateDocumentRequestModel>) {
		if (!documentTypeKey) throw new Error('Document type id is missing');
		await this.#init;
		return this.#detailDataSource.createScaffold(documentTypeKey, preset);
	}

	async requestById(id: string) {
		await this.#init;

		// TODO: should we show a notification if the id is missing?
		// Investigate what is best for Acceptance testing, cause in that perspective a thrown error might be the best choice?
		if (!id) {
			throw new Error('Id is missing');
		}

		const { data, error } = await this.#detailDataSource.get(id);

		if (data) {
			this.#store?.append(data);
		}

		return { data, error };
	}

	async byId(id: string) {
		if (!id) throw new Error('Id is missing');
		await this.#init;
		return this.#store!.byId(id);
	}

	// Could potentially be general methods:
	async create(item: CreateDocumentRequestModel & { id: string }) {
		await this.#init;

		if (!item || !item.id) {
			throw new Error('Document is missing');
		}

		const { error } = await this.#detailDataSource.insert(item);

		if (!error) {
			// TODO: we currently don't use the detail store for anything.
			// Consider to look up the data before fetching from the server
			this.#store?.append(item);
			// TODO: Update tree store with the new item? or ask tree to request the new item?

			// TODO: Revisit this call, as we should be able to update tree on client.
			await this.requestRootTreeItems();

			const notification = { data: { message: `Document created` } };
			this.#notificationContext?.peek('positive', notification);

			return { data: item };
		}

		return { error };
	}

	async save(id: string, item: UpdateDocumentRequestModel) {
		if (!id) throw new Error('Id is missing');
		if (!item) throw new Error('Item is missing');

		await this.#init;

		const { error } = await this.#detailDataSource.update(id, item);

		if (!error) {
			// TODO: we currently don't use the detail store for anything.
			// Consider to look up the data before fetching from the server
			// Consider notify a workspace if a document is updated in the store while someone is editing it.
			this.#store?.append(item);
			//this.#treeStore?.updateItem(item.id, { name: item.name });// Port data to tree store.
			// TODO: would be nice to align the stores on methods/methodNames.

			// TODO: Revisit this call, as we should be able to update tree on client.
			await this.requestRootTreeItems();

			const notification = { data: { message: `Document saved` } };
			this.#notificationContext?.peek('positive', notification);
		}

		return { error };
	}

	// General:

	async delete(id: string) {
		if (!id) throw new Error('Id is missing');
		await this.#init;

		const { error } = await this.#detailDataSource.delete(id);

		if (!error) {
			// TODO: we currently don't use the detail store for anything.
			// Consider to look up the data before fetching from the server.
			// Consider notify a workspace if a document is deleted from the store while someone is editing it.
			// TODO: would be nice to align the stores on methods/methodNames.
			this.#store?.remove([id]);
			this.#treeStore?.removeItem(id);

			const notification = { data: { message: `Document deleted` } };
			this.#notificationContext?.peek('positive', notification);
		}

		return { error };
	}

	// Listing all currently known methods we need to implement:
	// these currently only covers posting data
	// TODO: find a good way to split these
	async trash(ids: Array<string>) {
		console.log('document trash: ' + ids);
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
