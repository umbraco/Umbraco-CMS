import { UmbDocumentServerDataSource } from './sources/document.server.data.js';
import { UmbDocumentStore, UMB_DOCUMENT_STORE_CONTEXT_TOKEN } from './document.store.js';
import { UmbDocumentTreeStore, UMB_DOCUMENT_TREE_STORE_CONTEXT_TOKEN } from './document.tree.store.js';
import { UMB_DOCUMENT_ITEM_STORE_CONTEXT_TOKEN, type UmbDocumentItemStore } from './document-item.store.js';
import { UmbDocumentItemServerDataSource } from './sources/document-item.server.data.js';
import type { UmbDetailRepository } from '@umbraco-cms/backoffice/repository';
import { UmbBaseController, UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import {
	DocumentResponseModel,
	CreateDocumentRequestModel,
	UpdateDocumentRequestModel,
} from '@umbraco-cms/backoffice/backend-api';
import { UmbNotificationContext, UMB_NOTIFICATION_CONTEXT_TOKEN } from '@umbraco-cms/backoffice/notification';
import { UmbApi } from '@umbraco-cms/backoffice/extension-api';
import { UmbVariantId } from '@umbraco-cms/backoffice/variant';

export class UmbDocumentRepository
	extends UmbBaseController
	implements
		UmbDetailRepository<CreateDocumentRequestModel, any, UpdateDocumentRequestModel, DocumentResponseModel>,
		UmbApi
{
	#init!: Promise<unknown>;

	#treeStore?: UmbDocumentTreeStore;

	#detailDataSource: UmbDocumentServerDataSource;
	#store?: UmbDocumentStore;

	#itemSource: UmbDocumentItemServerDataSource;
	#itemStore?: UmbDocumentItemStore;

	#notificationContext?: UmbNotificationContext;

	constructor(host: UmbControllerHostElement) {
		super(host);

		// TODO: figure out how spin up get the correct data source
		this.#detailDataSource = new UmbDocumentServerDataSource(this);
		this.#itemSource = new UmbDocumentItemServerDataSource(this);

		this.#init = Promise.all([
			this.consumeContext(UMB_DOCUMENT_TREE_STORE_CONTEXT_TOKEN, (instance) => {
				this.#treeStore = instance;
			}).asPromise(),

			this.consumeContext(UMB_DOCUMENT_STORE_CONTEXT_TOKEN, (instance) => {
				this.#store = instance;
			}).asPromise(),

			this.consumeContext(UMB_DOCUMENT_ITEM_STORE_CONTEXT_TOKEN, (instance) => {
				this.#itemStore = instance;
			}).asPromise(),

			this.consumeContext(UMB_NOTIFICATION_CONTEXT_TOKEN, (instance) => {
				this.#notificationContext = instance;
			}).asPromise(),
		]);
	}

	// TODO: Move

	// Structure permissions;
	async requestAllowedDocumentTypesOf(id: string | null) {
		if (id === undefined) throw new Error('Id is missing');
		await this.#init;
		return this.#detailDataSource.getAllowedDocumentTypesOf(id);
	}

	// ITEMS:
	async requestItems(ids: Array<string>) {
		if (!ids) throw new Error('Keys are missing');
		await this.#init;

		const { data, error } = await this.#itemSource.getItems(ids);

		if (data) {
			this.#itemStore?.appendItems(data);
		}

		return { data, error, asObservable: () => this.#itemStore!.items(ids) };
	}

	async items(ids: Array<string>) {
		await this.#init;
		return this.#itemStore!.items(ids);
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

		const { data, error } = await this.#detailDataSource.read(id);

		if (data) {
			this.#store?.append(data);
		}

		return { data, error, asObservable: () => this.#store!.byId(id) };
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

		const { error } = await this.#detailDataSource.create(item);

		if (!error) {
			// TODO: we currently don't use the detail store for anything.
			// Consider to look up the data before fetching from the server
			this.#store?.append(item);
			// TODO: Update tree store with the new item? or ask tree to request the new item?

			const notification = { data: { message: `Document created` } };
			this.#notificationContext?.peek('positive', notification);

			// TODO: Revisit this call, as we should be able to update tree on client.
			//await this.requestRootTreeItems();

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

			const notification = { data: { message: `Document saved` } };
			this.#notificationContext?.peek('positive', notification);

			// TODO: Revisit this call, as we should be able to update tree on client.
			//await this.requestRootTreeItems();
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
			this.#store?.removeItem(id);
			this.#treeStore?.removeItem(id);
			this.#itemStore?.removeItem(id);

			const notification = { data: { message: `Document deleted` } };
			this.#notificationContext?.peek('positive', notification);
		}

		return { error };
	}

	async trash(id: string) {
		if (!id) throw new Error('Id is missing');
		await this.#init;

		const { error } = await this.#detailDataSource.trash(id);

		if (!error) {
			// TODO: we currently don't use the detail store for anything.
			// Consider to look up the data before fetching from the server.
			// Consider notify a workspace if a document is deleted from the store while someone is editing it.
			// TODO: Temp hack: would should update a property isTrashed (maybe) on the item instead of removing them.
			this.#store?.removeItem(id);
			this.#treeStore?.removeItem(id);
			this.#itemStore?.removeItem(id);

			// TODO: append to recycle bin store

			const notification = { data: { message: `Document moved to recycle bin` } };
			this.#notificationContext?.peek('positive', notification);
		}

		return { error };
	}

	async saveAndPublish(id: string, item: UpdateDocumentRequestModel, variantIds: Array<UmbVariantId>) {
		if (!id) throw new Error('id is missing');
		if (!variantIds) throw new Error('variant IDs are missing');
		//await this.#init;

		await this.save(id, item);

		const { error } = await this.#detailDataSource.saveAndPublish(id, variantIds);

		if (!error) {
			// TODO: Update other stores based on above effect.

			const notification = { data: { message: `Document saved and published` } };
			this.#notificationContext?.peek('positive', notification);
		}

		return { error };
	}

	async publish(id: string, variantIds: Array<UmbVariantId>) {
		if (!id) throw new Error('id is missing');
		if (!variantIds) throw new Error('variant IDs are missing');
		await this.#init;

		const { error } = await this.#detailDataSource.saveAndPublish(id, variantIds);

		if (!error) {
			// TODO: Update other stores based on above effect.

			const notification = { data: { message: `Document published` } };
			this.#notificationContext?.peek('positive', notification);
		}

		return { error };
	}

	async unpublish(id: string, variantIds: Array<UmbVariantId>) {
		if (!id) throw new Error('id is missing');
		if (!variantIds) throw new Error('variant IDs are missing');
		await this.#init;

		const { error } = await this.#detailDataSource.unpublish(id, variantIds);

		if (!error) {
			// TODO: Update other stores based on above effect.

			const notification = { data: { message: `Document unpublished` } };
			this.#notificationContext?.peek('positive', notification);
		}

		return { error };
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

	async rollback() {
		alert('rollback');
	}
}
