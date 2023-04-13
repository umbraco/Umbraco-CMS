import { DocumentTypeTreeServerDataSource } from './sources/document-type.tree.server.data';
import { UmbDocumentTypeServerDataSource } from './sources/document-type.server.data';
import { UmbDocumentTypeTreeStore, UMB_DOCUMENT_TYPE_TREE_STORE_CONTEXT_TOKEN } from './document-type.tree.store';
import { UmbDocumentTypeStore, UMB_DOCUMENT_TYPE_STORE_CONTEXT_TOKEN } from './document-type.store';
import type { UmbTreeDataSource, UmbTreeRepository, UmbDetailRepository } from '@umbraco-cms/backoffice/repository';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller';
import { UmbContextConsumerController } from '@umbraco-cms/backoffice/context-api';
import { ProblemDetailsModel, DocumentTypeResponseModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbNotificationContext, UMB_NOTIFICATION_CONTEXT_TOKEN } from '@umbraco-cms/backoffice/notification';

type ItemType = DocumentTypeResponseModel;

export class UmbDocumentTypeRepository implements UmbTreeRepository<ItemType>, UmbDetailRepository<ItemType> {
	#init!: Promise<unknown>;

	#host: UmbControllerHostElement;

	#treeSource: UmbTreeDataSource;
	#treeStore?: UmbDocumentTypeTreeStore;

	#detailDataSource: UmbDocumentTypeServerDataSource;
	#detailStore?: UmbDocumentTypeStore;

	#notificationContext?: UmbNotificationContext;

	constructor(host: UmbControllerHostElement) {
		this.#host = host;

		// TODO: figure out how spin up get the correct data source
		this.#treeSource = new DocumentTypeTreeServerDataSource(this.#host);
		this.#detailDataSource = new UmbDocumentTypeServerDataSource(this.#host);

		this.#init = Promise.all([
			new UmbContextConsumerController(this.#host, UMB_DOCUMENT_TYPE_TREE_STORE_CONTEXT_TOKEN, (instance) => {
				this.#treeStore = instance;
			}),

			new UmbContextConsumerController(this.#host, UMB_DOCUMENT_TYPE_STORE_CONTEXT_TOKEN, (instance) => {
				this.#detailStore = instance;
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

	async requestTreeItemsOf(parentId: string | null) {
		await this.#init;

		if (!parentId) {
			const error: ProblemDetailsModel = { title: 'Parent id is missing' };
			return { data: undefined, error };
		}

		const { data, error } = await this.#treeSource.getChildrenOf(parentId);

		if (data) {
			this.#treeStore?.appendItems(data.items);
		}

		return { data, error, asObservable: () => this.#treeStore!.childrenOf(parentId) };
	}

	async requestTreeItems(ids: Array<string>) {
		await this.#init;

		if (!ids) {
			const error: ProblemDetailsModel = { title: 'Ids are missing' };
			return { data: undefined, error };
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

	async treeItems(ids: Array<string>) {
		await this.#init;
		return this.#treeStore!.items(ids);
	}

	// DETAILS:

	async createScaffold(parentId: string | null) {
		if (!parentId) throw new Error('Parent id is missing');
		await this.#init;
		return this.#detailDataSource.createScaffold(parentId);
	}

	async requestById(id: string) {
		if (!id) throw new Error('Id is missing');
		await this.#init;

		const { data, error } = await this.#detailDataSource.get(id);

		if (data) {
			this.#detailStore?.append(data);
		}

		return { data, error };
	}

	async byId(id: string) {
		if (!id) throw new Error('Id is missing');
		await this.#init;
		return this.#detailStore!.byId(id);
	}

	// TODO: we need to figure out where to put this
	async requestAllowedChildTypesOf(id: string) {
		if (!id) throw new Error('Id is missing');
		await this.#init;
		return this.#detailDataSource.getAllowedChildrenOf(id);
	}

	// Could potentially be general methods:

	async create(template: ItemType) {
		if (!template || !template.id) throw new Error('Template is missing');
		await this.#init;

		const { error } = await this.#detailDataSource.insert(template);

		if (!error) {
			const notification = { data: { message: `Document created` } };
			this.#notificationContext?.peek('positive', notification);

			// TODO: we currently don't use the detail store for anything.
			// Consider to look up the data before fetching from the server
			this.#detailStore?.append(template);
			// TODO: Update tree store with the new item? or ask tree to request the new item?
		}

		return { error };
	}

	async save(id: string, item: any) {
		if (!id) throw new Error('Id is missing');
		if (!item) throw new Error('Item is missing');

		await this.#init;

		const { error } = await this.#detailDataSource.update(id, item);

		if (!error) {
			// TODO: we currently don't use the detail store for anything.
			// Consider to look up the data before fetching from the server
			// Consider notify a workspace if a template is updated in the store while someone is editing it.
			this.#detailStore?.append(item);
			this.#treeStore?.updateItem(id, item);
			// TODO: would be nice to align the stores on methods/methodNames.

			const notification = { data: { message: `Document Type saved` } };
			this.#notificationContext?.peek('positive', notification);
		}

		return { error };
	}

	// General:

	async delete(id: string) {
		if (!id) throw new Error('Document id is missing');
		await this.#init;

		const { error } = await this.#detailDataSource.delete(id);

		if (!error) {
			const notification = { data: { message: `Document deleted` } };
			this.#notificationContext?.peek('positive', notification);

			// TODO: we currently don't use the detail store for anything.
			// Consider to look up the data before fetching from the server.
			// Consider notify a workspace if a template is deleted from the store while someone is editing it.
			this.#detailStore?.remove([id]);
			this.#treeStore?.removeItem(id);
			// TODO: would be nice to align the stores on methods/methodNames.
		}

		return { error };
	}
}
