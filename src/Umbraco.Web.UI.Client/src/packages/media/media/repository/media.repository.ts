import { UMB_MEDIA_TREE_STORE_CONTEXT, type UmbMediaTreeStore } from '../tree/index.js';
import { UMB_MEDIA_STORE_CONTEXT, UmbMediaStore } from './media.store.js';
import { UmbMediaDetailServerDataSource } from './sources/media-detail.server.data-source.js';
import { UmbMediaItemServerDataSource } from './sources/media-item.server.data-source.js';
import { UMB_MEDIA_ITEM_STORE_CONTEXT, UmbMediaItemStore } from './media-item.store.js';
import { type UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbBaseController } from '@umbraco-cms/backoffice/class-api';
import { CreateMediaRequestModel, UpdateMediaRequestModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbNotificationContext, UMB_NOTIFICATION_CONTEXT_TOKEN } from '@umbraco-cms/backoffice/notification';
import { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export class UmbMediaRepository extends UmbBaseController implements UmbApi {
	#init;

	#treeStore?: UmbMediaTreeStore;

	#detailDataSource: UmbMediaDetailServerDataSource;
	#store?: UmbMediaStore;

	#itemSource: UmbMediaItemServerDataSource;
	#itemStore?: UmbMediaItemStore;

	#notificationContext?: UmbNotificationContext;

	constructor(host: UmbControllerHost) {
		super(host);

		// TODO: figure out how spin up get the correct data source
		this.#detailDataSource = new UmbMediaDetailServerDataSource(this);
		this.#itemSource = new UmbMediaItemServerDataSource(this);

		this.#init = Promise.all([
			this.consumeContext(UMB_MEDIA_TREE_STORE_CONTEXT, (instance) => {
				this.#treeStore = instance;
			}).asPromise(),

			this.consumeContext(UMB_MEDIA_STORE_CONTEXT, (instance) => {
				this.#store = instance;
			}).asPromise(),

			this.consumeContext(UMB_MEDIA_ITEM_STORE_CONTEXT, (instance) => {
				this.#itemStore = instance;
			}).asPromise(),

			this.consumeContext(UMB_NOTIFICATION_CONTEXT_TOKEN, (instance) => {
				this.#notificationContext = instance;
			}).asPromise(),
		]);
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

	// eslint-disable-next-line @typescript-eslint/ban-ts-comment
	// @ts-ignore
	async createScaffold(parentId: string | null) {
		if (parentId === undefined) throw new Error('Parent id is missing');
		await this.#init;
		return this.#detailDataSource.createScaffold(parentId);
	}

	async requestById(id: string) {
		if (!id) throw new Error('Id is missing');
		await this.#init;

		const { data, error } = await this.#detailDataSource.read(id);

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

	async create(media: CreateMediaRequestModel) {
		if (!media) throw new Error('Media is missing');

		await this.#init;

		const { error } = await this.#detailDataSource.create(media);

		if (!error) {
			// TODO: we currently don't use the detail store for anything.
			// Consider to look up the data before fetching from the server
			// TODO: Update tree store with the new item? or ask tree to request the new item?
			//this.#store?.append(media);

			const notification = { data: { message: `Media created` } };
			this.#notificationContext?.peek('positive', notification);
		}

		return { error };
	}

	async save(id: string, updatedItem: UpdateMediaRequestModel) {
		if (!id) throw new Error('Id is missing');
		if (!updatedItem) throw new Error('Updated media item is missing');

		await this.#init;

		const { error } = await this.#detailDataSource.update(id, updatedItem);

		if (!error) {
			// TODO: we currently don't use the detail store for anything.
			// Consider to look up the data before fetching from the server
			// Consider notify a workspace if a template is updated in the store while someone is editing it.
			// this.#store?.append(updatedMediaItem);
			// this.#treeStore?.updateItem(id, updatedItem);

			const notification = { data: { message: `Media saved` } };
			this.#notificationContext?.peek('positive', notification);
		}

		return { error };
	}

	// General:
	async delete(id: string) {
		await this.#init;

		if (!id) {
			throw new Error('Document id is missing');
		}

		const { error } = await this.#detailDataSource.delete(id);

		if (!error) {
			const notification = { data: { message: `Document deleted` } };
			this.#notificationContext?.peek('positive', notification);
		}

		// TODO: we currently don't use the detail store for anything.
		// Consider to look up the data before fetching from the server.
		// Consider notify a workspace if a template is deleted from the store while someone is editing it.
		this.#store?.removeItem(id);
		this.#treeStore?.removeItem(id);

		return { error };
	}

	async trash(ids: Array<string>) {
		console.log('media trash: ' + ids);
		alert('implement trash');
	}

	async move(ids: Array<string>, destination: string | null) {
		// TODO: use backend cli when available.
		const res = await fetch('/umbraco/management/api/v1/media/move', {
			method: 'POST',
			body: JSON.stringify({ ids, destination }),
			headers: {
				'Content-Type': 'application/json',
			},
		});
		const data = await res.json();
		this.#treeStore?.appendItems(data);
	}

	async copy(uniques: Array<string>, destination: string) {
		console.log(`copy: ${uniques} to ${destination}`);
		alert('copy');
	}

	async sortChildrenOf() {
		alert('sort');
	}
}
