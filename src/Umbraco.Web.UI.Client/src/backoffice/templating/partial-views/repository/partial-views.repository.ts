import { UmbPartialViewDetailServerDataSource } from './sources/partial-views.detail.server.data';
import { UmbPartialViewsTreeServerDataSource } from './sources/partial-views.tree.server.data';
import { UmbPartialViewsStore, UMB_PARTIAL_VIEWS_STORE_CONTEXT_TOKEN } from './partial-views.store';
import { UmbPartialViewsTreeStore, UMB_PARTIAL_VIEW_TREE_STORE_CONTEXT_TOKEN } from './partial-views.tree.store';
import { UmbControllerHostInterface } from '@umbraco-cms/backoffice/controller';
import { UmbNotificationContext, UMB_NOTIFICATION_CONTEXT_TOKEN } from '@umbraco-cms/backoffice/notification';
import { UmbContextConsumerController } from '@umbraco-cms/backoffice/context-api';
import { ProblemDetailsModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbDetailRepository } from 'libs/repository/detail-repository.interface';
import { UmbTreeRepository } from 'libs/repository/tree-repository.interface';

export class UmbTemplateRepository implements UmbTreeRepository<any>, UmbDetailRepository<any> {
	#init;
	#host: UmbControllerHostInterface;

	#treeDataSource: UmbPartialViewsTreeServerDataSource;
	#detailDataSource: UmbPartialViewDetailServerDataSource;

	#treeStore?: UmbPartialViewsTreeStore;
	#store?: UmbPartialViewsStore;

	#notificationContext?: UmbNotificationContext;

	constructor(host: UmbControllerHostInterface) {
		this.#host = host;

		this.#treeDataSource = new UmbPartialViewsTreeServerDataSource(this.#host);
		this.#detailDataSource = new UmbPartialViewDetailServerDataSource(this.#host);

		this.#init = Promise.all([
			new UmbContextConsumerController(this.#host, UMB_PARTIAL_VIEW_TREE_STORE_CONTEXT_TOKEN, (instance) => {
				this.#treeStore = instance;
			}),

			new UmbContextConsumerController(this.#host, UMB_PARTIAL_VIEWS_STORE_CONTEXT_TOKEN, (instance) => {
				this.#store = instance;
			}),

			new UmbContextConsumerController(this.#host, UMB_NOTIFICATION_CONTEXT_TOKEN, (instance) => {
				this.#notificationContext = instance;
			}),
		]);
	}

	// TREE:

	async requestRootTreeItems() {
		await this.#init;

		const { data, error } = await this.#treeDataSource.getRootItems();

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

		const { data, error } = await this.#treeDataSource.getChildrenOf(parentKey);

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

		const { data, error } = await this.#treeDataSource.getItems(keys);

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
		// await this.#init;

		// if (!parentKey) {
		// 	throw new Error('Parent key is missing');
		// }

		// // TODO: add parent key to create scaffold
		// return this.#detailDataSource.createScaffold();
		return Promise.reject(new Error('Not implemented'));
	}

	async requestByKey(key: string) {
		// await this.#init;

		// // TODO: should we show a notification if the key is missing?
		// // Investigate what is best for Acceptance testing, cause in that perspective a thrown error might be the best choice?
		// if (!key) {
		// 	const error: ProblemDetailsModel = { title: 'Key is missing' };
		// 	return { error };
		// }
		// const { data, error } = await this.#detailDataSource.get(key);

		// if (data) {
		// 	this.#store?.append(data);
		// }

		// return { data, error };
		return Promise.reject(new Error('Not implemented'));
	}

	// Could potentially be general methods:

	async create(patrial: any) {
		// await this.#init;

		// if (!template || !template.key) {
		// 	throw new Error('Template is missing');
		// }

		// const { error } = await this.#detailDataSource.insert(template);

		// if (!error) {
		// 	const notification = { data: { message: `Template created` } };
		// 	this.#notificationContext?.peek('positive', notification);
		// }

		// // TODO: we currently don't use the detail store for anything.
		// // Consider to look up the data before fetching from the server
		// this.#store?.append(template);
		// // TODO: Update tree store with the new item? or ask tree to request the new item?

		// return { error };

		return Promise.reject(new Error('Not implemented'));
	}

	async save(patrial: any) {
		// await this.#init;

		// if (!template || !template.key) {
		// 	throw new Error('Template is missing');
		// }

		// const { error } = await this.#detailDataSource.update(template);

		// if (!error) {
		// 	const notification = { data: { message: `Template saved` } };
		// 	this.#notificationContext?.peek('positive', notification);
		// }

		// // TODO: we currently don't use the detail store for anything.
		// // Consider to look up the data before fetching from the server
		// // Consider notify a workspace if a template is updated in the store while someone is editing it.
		// this.#store?.append(template);
		// this.#treeStore?.updateItem(template.key, { name: template.name });
		// // TODO: would be nice to align the stores on methods/methodNames.

		// return { error };
		return Promise.reject(new Error('Not implemented'));
	}

	// General:

	async delete(key: string) {
		// await this.#init;

		// if (!key) {
		// 	throw new Error('Template key is missing');
		// }

		// const { error } = await this.#detailDataSource.delete(key);

		// if (!error) {
		// 	const notification = { data: { message: `Template deleted` } };
		// 	this.#notificationContext?.peek('positive', notification);
		// }

		// // TODO: we currently don't use the detail store for anything.
		// // Consider to look up the data before fetching from the server.
		// // Consider notify a workspace if a template is deleted from the store while someone is editing it.
		// this.#store?.remove([key]);
		// this.#treeStore?.removeItem(key);
		// // TODO: would be nice to align the stores on methods/methodNames.

		// return { error };
		return Promise.reject(new Error('Not implemented'));
	}
}
