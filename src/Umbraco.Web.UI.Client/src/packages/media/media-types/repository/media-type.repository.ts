import type { MediaTypeDetails } from '../types';
import { UmbMediaTypeTreeStore, UMB_MEDIA_TYPE_TREE_STORE_CONTEXT_TOKEN } from './media-type.tree.store';
import { UmbMediaTypeDetailServerDataSource } from './sources/media-type.detail.server.data';
import { UmbMediaTypeStore, UMB_MEDIA_TYPE_STORE_CONTEXT_TOKEN } from './media-type.detail.store';
import { UmbMediaTypeTreeServerDataSource } from './sources/media-type.tree.server.data';
import { UmbContextConsumerController } from '@umbraco-cms/backoffice/context-api';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbNotificationContext, UMB_NOTIFICATION_CONTEXT_TOKEN } from 'src/packages/core/notification';
import { UmbTreeRepository, UmbTreeDataSource } from '@umbraco-cms/backoffice/repository';
import { EntityTreeItemResponseModel } from '@umbraco-cms/backoffice/backend-api';

export class UmbMediaTypeRepository implements UmbTreeRepository<EntityTreeItemResponseModel> {
	#init!: Promise<unknown>;

	#host: UmbControllerHostElement;

	#treeSource: UmbTreeDataSource;
	#treeStore?: UmbMediaTypeTreeStore;

	#detailSource: UmbMediaTypeDetailServerDataSource;
	#store?: UmbMediaTypeStore;

	#notificationContext?: UmbNotificationContext;

	constructor(host: UmbControllerHostElement) {
		this.#host = host;

		// TODO: figure out how spin up get the correct data source
		this.#treeSource = new UmbMediaTypeTreeServerDataSource(this.#host);
		this.#detailSource = new UmbMediaTypeDetailServerDataSource(this.#host);

		this.#init = Promise.all([
			new UmbContextConsumerController(this.#host, UMB_MEDIA_TYPE_STORE_CONTEXT_TOKEN, (instance) => {
				this.#store = instance;
			}),

			new UmbContextConsumerController(this.#host, UMB_MEDIA_TYPE_TREE_STORE_CONTEXT_TOKEN, (instance) => {
				this.#treeStore = instance;
			}),

			new UmbContextConsumerController(this.#host, UMB_NOTIFICATION_CONTEXT_TOKEN, (instance) => {
				this.#notificationContext = instance;
			}),
		]);
	}

	// TREE:
	async requestTreeRoot() {
		await this.#init;

		const data = {
			id: null,
			type: 'media-type-root',
			name: 'Media Types',
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

	// DETAILS

	async createScaffold() {
		await this.#init;
		return this.#detailSource.createScaffold();
	}

	async requestDetails(id: string) {
		await this.#init;

		// TODO: should we show a notification if the id is missing?
		// Investigate what is best for Acceptance testing, cause in that perspective a thrown error might be the best choice?
		if (!id) {
			throw new Error('Id is missing');
		}
		const { data, error } = await this.#detailSource.get(id);

		if (data) {
			this.#store?.append(data);
		}
		return { data, error };
	}

	async delete(id: string) {
		await this.#init;
		return this.#detailSource.delete(id);
	}

	async save(mediaType: MediaTypeDetails) {
		await this.#init;

		// TODO: should we show a notification if the media type is missing?
		// Investigate what is best for Acceptance testing, cause in that perspective a thrown error might be the best choice?
		if (!mediaType || !mediaType.id) {
			throw new Error('Media type is missing');
		}

		const { error } = await this.#detailSource.update(mediaType);

		if (!error) {
			const notification = { data: { message: `Media type '${mediaType.name}' saved` } };
			this.#notificationContext?.peek('positive', notification);
		}

		// TODO: we currently don't use the detail store for anything.
		// Consider to look up the data before fetching from the server
		// Consider notify a workspace if a media type is updated in the store while someone is editing it.
		this.#store?.append(mediaType);
		this.#treeStore?.updateItem(mediaType.id, { name: mediaType.name });
		// TODO: would be nice to align the stores on methods/methodNames.

		return { error };
	}

	async create(mediaType: MediaTypeDetails) {
		await this.#init;

		if (!mediaType.name) {
			throw new Error('Name is missing');
		}

		const { data, error } = await this.#detailSource.insert(mediaType);

		if (!error) {
			const notification = { data: { message: `Media type '${mediaType.name}' created` } };
			this.#notificationContext?.peek('positive', notification);
		}

		return { data, error };
	}

	async move() {
		alert('move me!');
	}

	async copy() {
		alert('copy me');
	}
}
