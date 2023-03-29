import type { MediaDetails } from '../';
import { MediaTreeServerDataSource } from './sources/media.tree.server.data';
import { UmbMediaTreeStore, UMB_MEDIA_TREE_STORE_CONTEXT_TOKEN } from './media.tree.store';
import { UmbMediaStore, UMB_MEDIA_STORE_CONTEXT_TOKEN } from './media.store';
import { UmbMediaDetailServerDataSource } from './sources/media.detail.server.data';
import type { UmbTreeRepository, UmbTreeDataSource } from '@umbraco-cms/backoffice/repository';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller';
import { UmbContextConsumerController } from '@umbraco-cms/backoffice/context-api';
import { ProblemDetailsModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbDetailRepository } from '@umbraco-cms/backoffice/repository';
import { UmbNotificationContext, UMB_NOTIFICATION_CONTEXT_TOKEN } from '@umbraco-cms/backoffice/notification';

type ItemDetailType = MediaDetails;

export class UmbMediaRepository implements UmbTreeRepository, UmbDetailRepository<ItemDetailType> {
	#host: UmbControllerHostElement;

	#treeSource: UmbTreeDataSource;
	#treeStore?: UmbMediaTreeStore;

	#detailDataSource: UmbMediaDetailServerDataSource;
	#store?: UmbMediaStore;

	#notificationContext?: UmbNotificationContext;

	#initResolver?: () => void;
	#initialized = false;

	constructor(host: UmbControllerHostElement) {
		this.#host = host;

		// TODO: figure out how spin up get the correct data source
		this.#treeSource = new MediaTreeServerDataSource(this.#host);
		this.#detailDataSource = new UmbMediaDetailServerDataSource(this.#host);

		new UmbContextConsumerController(this.#host, UMB_MEDIA_TREE_STORE_CONTEXT_TOKEN, (instance) => {
			this.#treeStore = instance;
			this.#checkIfInitialized();
		});

		new UmbContextConsumerController(this.#host, UMB_MEDIA_STORE_CONTEXT_TOKEN, (instance) => {
			this.#store = instance;
			this.#checkIfInitialized();
		});

		new UmbContextConsumerController(this.#host, UMB_NOTIFICATION_CONTEXT_TOKEN, (instance) => {
			this.#notificationContext = instance;
			this.#checkIfInitialized();
		});
	}

	// TODO: make a generic way to wait for initialization
	#init = new Promise<void>((resolve) => {
		this.#initialized ? resolve() : (this.#initResolver = resolve);
	});

	#checkIfInitialized() {
		if (this.#treeStore && this.#store && this.#notificationContext) {
			this.#initialized = true;
			this.#initResolver?.();
		}
	}

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
			this.#store?.append(data);
		}

		return { data, error };
	}

	// Could potentially be general methods:

	async create(template: ItemDetailType) {
		await this.#init;

		if (!template || !template.key) {
			throw new Error('Template is missing');
		}

		const { error } = await this.#detailDataSource.insert(template);

		if (!error) {
			const notification = { data: { message: `Media created` } };
			this.#notificationContext?.peek('positive', notification);
		}

		// TODO: we currently don't use the detail store for anything.
		// Consider to look up the data before fetching from the server
		this.#store?.append(template);
		// TODO: Update tree store with the new item? or ask tree to request the new item?

		return { error };
	}

	async save(document: ItemDetailType) {
		await this.#init;

		if (!document || !document.key) {
			throw new Error('Template is missing');
		}

		const { error } = await this.#detailDataSource.update(document);

		if (!error) {
			const notification = { data: { message: `Document saved` } };
			this.#notificationContext?.peek('positive', notification);
		}

		// TODO: we currently don't use the detail store for anything.
		// Consider to look up the data before fetching from the server
		// Consider notify a workspace if a template is updated in the store while someone is editing it.
		this.#store?.append(document);
		this.#treeStore?.updateItem(document.key, { name: document.name });

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
		// Consider notify a workspace if a template is deleted from the store while someone is editing it.
		this.#store?.remove([key]);
		this.#treeStore?.removeItem(key);
		// TODO: would be nice to align the stores on methods/methodNames.

		return { error };
	}

	async trash(keys: Array<string>) {
		console.log('media trash: ' + keys);
		alert('implement trash');
	}

	async move(keys: Array<string>, destination: string) {
		// TODO: use backend cli when available.
		const res = await fetch('/umbraco/management/api/v1/media/move', {
			method: 'POST',
			body: JSON.stringify({ keys, destination }),
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
