import { UmbMediaTypeTreeStore, UMB_MEDIA_TYPE_TREE_STORE_CONTEXT_TOKEN } from './media-type.tree.store.js';
import { UmbMediaTypeDetailServerDataSource } from './sources/media-type.detail.server.data.js';
import { UmbMediaTypeStore, UMB_MEDIA_TYPE_STORE_CONTEXT_TOKEN } from './media-type.detail.store.js';
import { UmbMediaTypeTreeServerDataSource } from './sources/media-type.tree.server.data.js';
import { UMB_MEDIA_TYPE_ITEM_STORE_CONTEXT_TOKEN, UmbMediaTypeItemStore } from './media-type-item.store.js';
import { UmbMediaTypeItemServerDataSource } from './sources/media-type-item.server.data.js';
import { UmbBaseController, UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbNotificationContext, UMB_NOTIFICATION_CONTEXT_TOKEN } from '@umbraco-cms/backoffice/notification';
import {
	UmbTreeRepository,
	UmbTreeDataSource,
	UmbDataSource,
	UmbItemRepository,
	UmbDetailRepository,
	UmbItemDataSource,
} from '@umbraco-cms/backoffice/repository';
import {
	CreateMediaTypeRequestModel,
	FolderTreeItemResponseModel,
	MediaTypeItemResponseModel,
	MediaTypeResponseModel,
	UpdateMediaTypeRequestModel,
} from '@umbraco-cms/backoffice/backend-api';
import { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export class UmbMediaTypeRepository extends UmbBaseController
	implements
		UmbItemRepository<MediaTypeItemResponseModel>,
		UmbTreeRepository<FolderTreeItemResponseModel>,
		UmbDetailRepository<CreateMediaTypeRequestModel, any, UpdateMediaTypeRequestModel, MediaTypeResponseModel>,
		UmbApi
{
	#init!: Promise<unknown>;

	#treeSource: UmbTreeDataSource;
	#treeStore?: UmbMediaTypeTreeStore;

	#detailSource: UmbDataSource<CreateMediaTypeRequestModel, any, UpdateMediaTypeRequestModel, MediaTypeResponseModel>;
	#detailStore?: UmbMediaTypeStore;

	#itemSource: UmbItemDataSource<MediaTypeItemResponseModel>;
	#itemStore?: UmbMediaTypeItemStore;

	#notificationContext?: UmbNotificationContext;

	constructor(host: UmbControllerHostElement) {
		super(host);

		// TODO: figure out how spin up get the correct data source
		this.#treeSource = new UmbMediaTypeTreeServerDataSource(this);
		this.#detailSource = new UmbMediaTypeDetailServerDataSource(this);
		this.#itemSource = new UmbMediaTypeItemServerDataSource(this);

		this.#init = Promise.all([
			this.consumeContext(UMB_MEDIA_TYPE_STORE_CONTEXT_TOKEN, (instance) => {
				this.#detailStore = instance;
			}),

			this.consumeContext(UMB_MEDIA_TYPE_TREE_STORE_CONTEXT_TOKEN, (instance) => {
				this.#treeStore = instance;
			}),

			this.consumeContext(UMB_MEDIA_TYPE_ITEM_STORE_CONTEXT_TOKEN, (instance) => {
				this.#itemStore = instance;
			}),

			this.consumeContext(UMB_NOTIFICATION_CONTEXT_TOKEN, (instance) => {
				this.#notificationContext = instance;
			}),
		]);
	}

	async requestTreeRoot() {
		await this.#init;

		const data = {
			id: null,
			type: 'media-type-root',
			name: 'Media Types',
			icon: 'icon-folder',
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

	async byId(id: string) {
		if (!id) throw new Error('Key is missing');
		await this.#init;
		return this.#detailStore!.byId(id);
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

	// DETAILS

	async createScaffold(parentId: string | null) {
		if (parentId === undefined) throw new Error('Parent id is missing');
		await this.#init;
		return this.#detailSource.createScaffold(parentId);
	}

	async requestById(id: string) {
		await this.#init;
		if (!id) {
			throw new Error('Id is missing');
		}
		const { data, error } = await this.#detailSource.get(id);

		if (data) {
			this.#detailStore?.append(data);
		}
		return { data, error };
	}

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

	async delete(id: string) {
		await this.#init;
		return this.#detailSource.delete(id);
	}

	async save(id: string, item: UpdateMediaTypeRequestModel) {
		if (!id) throw new Error('Data Type id is missing');
		if (!item) throw new Error('Media Type is missing');
		await this.#init;

		const { error } = await this.#detailSource.update(id, item);

		if (!error) {
			this.#detailStore?.append(item);
			this.#treeStore?.updateItem(id, item);

			const notification = { data: { message: `Media type '${item.name}' saved` } };
			this.#notificationContext?.peek('positive', notification);
		}

		return { error };
	}

	async create(mediaType: CreateMediaTypeRequestModel) {
		if (!mediaType || !mediaType.id) throw new Error('Document Type is missing');
		await this.#init;

		const { error } = await this.#detailSource.insert(mediaType);

		if (!error) {
			//TODO: Model mismatch. FIX
			this.#detailStore?.append(mediaType as unknown as MediaTypeResponseModel);

			const treeItem = {
				type: 'media-type',
				parentId: null,
				name: mediaType.name,
				id: mediaType.id,
				isFolder: false,
				isContainer: false,
				hasChildren: false,
			};
			this.#treeStore?.appendItems([treeItem]);
		}

		return { error };
	}

	async move() {
		alert('move me!');
	}

	async copy() {
		alert('copy me');
	}
}
