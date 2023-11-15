import { UMB_DOCUMENT_TYPE_TREE_STORE_CONTEXT, UmbDocumentTypeTreeStore } from '../../tree/document-type.tree.store.js';
import { UMB_DOCUMENT_TYPE_ITEM_STORE_CONTEXT, UmbDocumentTypeItemStore } from '../item/document-type-item.store.js';
import { UmbDocumentTypeServerDataSource } from './document-type.server.data.js';
import { UmbDocumentTypeDetailStore, UMB_DOCUMENT_TYPE_DETAIL_STORE_CONTEXT } from './document-type-detail.store.js';
import { type UmbDetailRepository } from '@umbraco-cms/backoffice/repository';
import { UmbBaseController, type UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import {
	CreateDocumentTypeRequestModel,
	DocumentTypeResponseModel,
	FolderTreeItemResponseModel,
	UpdateDocumentTypeRequestModel,
} from '@umbraco-cms/backoffice/backend-api';
import { UmbNotificationContext, UMB_NOTIFICATION_CONTEXT_TOKEN } from '@umbraco-cms/backoffice/notification';
import { UmbApi } from '@umbraco-cms/backoffice/extension-api';

type ItemType = DocumentTypeResponseModel;

export class UmbDocumentTypeDetailRepository
	extends UmbBaseController
	implements
		UmbDetailRepository<CreateDocumentTypeRequestModel, any, UpdateDocumentTypeRequestModel, DocumentTypeResponseModel>,
		UmbApi
{
	#init!: Promise<unknown>;

	#treeStore?: UmbDocumentTypeTreeStore;

	#detailDataSource: UmbDocumentTypeServerDataSource;
	#detailStore?: UmbDocumentTypeDetailStore;

	#itemStore?: UmbDocumentTypeItemStore;

	#notificationContext?: UmbNotificationContext;

	constructor(host: UmbControllerHostElement) {
		super(host);

		// TODO: figure out how spin up get the correct data source
		this.#detailDataSource = new UmbDocumentTypeServerDataSource(this);

		this.#init = Promise.all([
			this.consumeContext(UMB_DOCUMENT_TYPE_TREE_STORE_CONTEXT, (instance) => {
				this.#treeStore = instance;
			}),

			this.consumeContext(UMB_DOCUMENT_TYPE_DETAIL_STORE_CONTEXT, (instance) => {
				this.#detailStore = instance;
			}),

			this.consumeContext(UMB_DOCUMENT_TYPE_ITEM_STORE_CONTEXT, (instance) => {
				this.#itemStore = instance;
			}),

			this.consumeContext(UMB_NOTIFICATION_CONTEXT_TOKEN, (instance) => {
				this.#notificationContext = instance;
			}),
		]);
	}

	// DETAILS:

	async createScaffold(parentId: string | null) {
		if (parentId === undefined) throw new Error('Parent id is missing');
		await this.#init;

		const { data } = await this.#detailDataSource.createScaffold(parentId);

		if (data) {
			this.#detailStore?.append(data);
		}

		return { data };
	}

	async requestById(id: string) {
		if (!id) throw new Error('Id is missing');
		await this.#init;

		const { data, error } = await this.#detailDataSource.get(id);

		if (data) {
			this.#detailStore?.append(data);
		}

		return { data, error, asObservable: () => this.#detailStore!.byId(id) };
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

	async create(documentType: ItemType) {
		if (!documentType || !documentType.id) throw new Error('Document Type is missing');
		await this.#init;

		const { error } = await this.#detailDataSource.insert(documentType);

		if (!error) {
			this.#detailStore?.append(documentType);
			const treeItem = createTreeItem(documentType);
			this.#treeStore?.appendItems([treeItem]);
		}

		return { error };
	}

	async save(id: string, item: UpdateDocumentTypeRequestModel) {
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
		if (!id) throw new Error('Document Type id is missing');
		await this.#init;

		const { error } = await this.#detailDataSource.delete(id);

		if (!error) {
			const notification = { data: { message: `Document Type deleted` } };
			this.#notificationContext?.peek('positive', notification);

			// TODO: we currently don't use the detail store for anything.
			// Consider to look up the data before fetching from the server.
			// Consider notify a workspace if a template is deleted from the store while someone is editing it.
			// TODO: would be nice to align the stores on methods/methodNames.
			this.#detailStore?.removeItem(id);
			this.#treeStore?.removeItem(id);
			this.#itemStore?.removeItem(id);
		}

		return { error };
	}
}

export const createTreeItem = (item: ItemType): FolderTreeItemResponseModel => {
	if (!item) throw new Error('item is null or undefined');
	if (!item.id) throw new Error('item.id is null or undefined');

	// TODO: needs parentID, this is missing in the current model. Should be good when updated to a createModel.
	return {
		type: 'document-type',
		parentId: null,
		name: item.name,
		id: item.id,
		isFolder: false,
		isContainer: false,
		hasChildren: false,
	};
};
