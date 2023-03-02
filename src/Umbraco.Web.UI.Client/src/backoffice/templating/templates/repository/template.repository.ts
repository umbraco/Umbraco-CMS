import { UmbTemplateDetailServerDataSource } from '../workspace/data/sources/template.detail.server.data';
import { TemplateTreeServerDataSource } from '../tree/data/sources/template.tree.server.data';
import {
	UmbTemplateDetailStore,
	UMB_TEMPLATE_DETAIL_STORE_CONTEXT_TOKEN,
} from '../workspace/data/template.detail.store';
import { UmbTemplateTreeStore, UMB_TEMPLATE_TREE_STORE_CONTEXT_TOKEN } from '../tree/data/template.tree.store';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';
import { UmbNotificationContext, UMB_NOTIFICATION_CONTEXT_TOKEN } from '@umbraco-cms/notification';
import { UmbContextConsumerController } from '@umbraco-cms/context-api';
import { ProblemDetailsModel, TemplateModel } from '@umbraco-cms/backend-api';
import { UmbDetailRepository } from 'libs/repository/detail-repository.interface';
import { UmbTreeRepository } from 'libs/repository/tree-repository.interface';

// Move to documentation / JSdoc
/* We need to create a new instance of the repository from within the element context. We want the notifications to be displayed in the right context. */
// element -> context -> repository -> (store) -> data source
// All methods should be async and return a promise. Some methods might return an observable as part of the promise response.
export class UmbTemplateRepository implements UmbTreeRepository, UmbDetailRepository<TemplateModel> {
	#init;
	#host: UmbControllerHostInterface;

	#treeDataSource: TemplateTreeServerDataSource;
	#detailDataSource: UmbTemplateDetailServerDataSource;

	#treeStore?: UmbTemplateTreeStore;
	#detailStore?: UmbTemplateDetailStore;

	#notificationContext?: UmbNotificationContext;

	constructor(host: UmbControllerHostInterface) {
		this.#host = host;

		// TODO: figure out how spin up get the correct data source
		this.#treeDataSource = new TemplateTreeServerDataSource(this.#host);
		this.#detailDataSource = new UmbTemplateDetailServerDataSource(this.#host);

		this.#init = Promise.all([
			new UmbContextConsumerController(this.#host, UMB_TEMPLATE_TREE_STORE_CONTEXT_TOKEN, (instance) => {
				this.#treeStore = instance;
			}),

			new UmbContextConsumerController(this.#host, UMB_TEMPLATE_DETAIL_STORE_CONTEXT_TOKEN, (instance) => {
				this.#detailStore = instance;
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
		await this.#init;

		if (!parentKey) {
			throw new Error('Parent key is missing');
		}

		// TODO: add parent key to create scaffold
		return this.#detailDataSource.createScaffold();
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
			this.#detailStore?.append(data);
		}

		return { data, error };
	}

	// Could potentially be general methods:

	async create(template: TemplateModel) {
		await this.#init;

		if (!template || !template.key) {
			throw new Error('Template is missing');
		}

		const { error } = await this.#detailDataSource.insert(template);

		if (!error) {
			const notification = { data: { message: `Template created` } };
			this.#notificationContext?.peek('positive', notification);
		}

		// TODO: we currently don't use the detail store for anything.
		// Consider to look up the data before fetching from the server
		this.#detailStore?.append(template);
		// TODO: Update tree store with the new item? or ask tree to request the new item?

		return { error };
	}

	async save(template: TemplateModel) {
		await this.#init;

		if (!template || !template.key) {
			throw new Error('Template is missing');
		}

		const { error } = await this.#detailDataSource.update(template);

		if (!error) {
			const notification = { data: { message: `Template saved` } };
			this.#notificationContext?.peek('positive', notification);
		}

		// TODO: we currently don't use the detail store for anything.
		// Consider to look up the data before fetching from the server
		// Consider notify a workspace if a template is updated in the store while someone is editing it.
		this.#detailStore?.append(template);
		this.#treeStore?.updateItem(template.key, { name: template.name });
		// TODO: would be nice to align the stores on methods/methodNames.

		return { error };
	}

	// General:

	async delete(key: string) {
		await this.#init;

		if (!key) {
			throw new Error('Template key is missing');
		}

		const { error } = await this.#detailDataSource.delete(key);

		if (!error) {
			const notification = { data: { message: `Template deleted` } };
			this.#notificationContext?.peek('positive', notification);
		}

		// TODO: we currently don't use the detail store for anything.
		// Consider to look up the data before fetching from the server.
		// Consider notify a workspace if a template is deleted from the store while someone is editing it.
		this.#detailStore?.remove([key]);
		this.#treeStore?.removeItem(key);
		// TODO: would be nice to align the stores on methods/methodNames.

		return { error };
	}
}
