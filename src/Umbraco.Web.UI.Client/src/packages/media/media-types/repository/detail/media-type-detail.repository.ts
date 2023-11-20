import { UMB_MEDIA_TYPE_TREE_STORE_CONTEXT, UmbMediaTypeTreeStore } from '../../tree/media-type-tree.store.js';
import { UMB_MEDIA_TYPE_ITEM_STORE_CONTEXT, UmbMediaTypeItemStore } from '../item/media-type-item.store.js';
import { UmbMediaTypeServerDataSource } from './media-type-detail.server.data-source.js';
import { UmbMediaTypeDetailStore, UMB_MEDIA_TYPE_DETAIL_STORE_CONTEXT } from './media-type-detail.store.js';
import { type UmbDetailRepository } from '@umbraco-cms/backoffice/repository';
import { UmbBaseController, type UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import {
	CreateMediaTypeRequestModel,
	MediaTypeResponseModel,
	FolderTreeItemResponseModel,
	UpdateMediaTypeRequestModel,
} from '@umbraco-cms/backoffice/backend-api';
import { UmbNotificationContext, UMB_NOTIFICATION_CONTEXT_TOKEN } from '@umbraco-cms/backoffice/notification';
import { UmbApi } from '@umbraco-cms/backoffice/extension-api';

type ItemType = MediaTypeResponseModel;

export class UmbMediaTypeDetailRepository
	extends UmbBaseController
	implements
		UmbDetailRepository<CreateMediaTypeRequestModel, any, UpdateMediaTypeRequestModel, MediaTypeResponseModel>,
		UmbApi
{
	#init!: Promise<unknown>;

	#treeStore?: UmbMediaTypeTreeStore;

	#detailDataSource: UmbMediaTypeServerDataSource;
	#detailStore?: UmbMediaTypeDetailStore;

	#itemStore?: UmbMediaTypeItemStore;

	#notificationContext?: UmbNotificationContext;

	constructor(host: UmbControllerHostElement) {
		super(host);

		// TODO: figure out how spin up get the correct data source
		this.#detailDataSource = new UmbMediaTypeServerDataSource(this);

		this.#init = Promise.all([
			this.consumeContext(UMB_MEDIA_TYPE_TREE_STORE_CONTEXT, (instance) => {
				this.#treeStore = instance;
			}),

			this.consumeContext(UMB_MEDIA_TYPE_DETAIL_STORE_CONTEXT, (instance) => {
				this.#detailStore = instance;
			}),

			this.consumeContext(UMB_MEDIA_TYPE_ITEM_STORE_CONTEXT, (instance) => {
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

		const { data, error } = await this.#detailDataSource.read(id);

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

	// Could potentially be general methods:

	async create(mediaType: ItemType) {
		if (!mediaType || !mediaType.id) throw new Error('Media Type is missing');
		await this.#init;

		const { error } = await this.#detailDataSource.create(mediaType);

		if (!error) {
			this.#detailStore?.append(mediaType);
			const treeItem = createTreeItem(mediaType);
			this.#treeStore?.append(treeItem);

			const notification = { data: { message: `Media Type created` } };
			this.#notificationContext?.peek('positive', notification);
		}

		return { error };
	}

	async save(id: string, item: UpdateMediaTypeRequestModel) {
		if (!id) throw new Error('Id is missing');
		if (!item) throw new Error('Item is missing');

		await this.#init;

		const { error } = await this.#detailDataSource.update(id, item);

		if (!error) {
			// TODO: we currently don't use the detail store for anything.
			// Consider to look up the data before fetching from the server
			// Consider notify a workspace if a template is updated in the store while someone is editing it.
			this.#detailStore?.updateItem(id, item);
			this.#treeStore?.updateItem(id, item);
			this.#itemStore?.updateItem(id, item);

			const notification = { data: { message: `Media Type saved` } };
			this.#notificationContext?.peek('positive', notification);
		}

		return { error };
	}

	// General:
	async delete(id: string) {
		if (!id) throw new Error('Media Type id is missing');
		await this.#init;

		const { error } = await this.#detailDataSource.delete(id);

		if (!error) {
			const notification = { data: { message: `Media Type deleted` } };
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
		type: 'media-type',
		parentId: null,
		name: item.name,
		id: item.id,
		isFolder: false,
		isContainer: false,
		hasChildren: false,
	};
};
