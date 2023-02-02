import { UmbTemplateDetailServerDataSource } from '../workspace/data/sources/template.detail.server.data';
import { TemplateTreeServerDataSource } from '../tree/data/sources/template.tree.server.data';
import { UmbTemplateDetailStore, UMB_TEMPLATE_DETAIL_STORE_CONTEXT_TOKEN } from '../workspace/data/template.detail.store';
import { UmbTemplateTreeStore, UMB_TEMPLATE_TREE_STORE_CONTEXT_TOKEN } from './template.tree.store';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';
import { UmbNotificationService, UMB_NOTIFICATION_SERVICE_CONTEXT_TOKEN } from '@umbraco-cms/notification';
import { UmbContextConsumerController } from '@umbraco-cms/context-api';
import { ProblemDetails, Template } from '@umbraco-cms/backend-api';
import { UmbDetailRepository } from 'libs/repositories/detail-repository.interface';
import { UmbTreeRepository } from 'libs/repositories/tree-repository.interface';

// Move to documentation / JSdoc
/* We need to create a new instance of the repository from within the element context. We want the notifications to be displayed in the right context. */
// element -> context -> repository -> (store) -> data source
// All methods should be async and return a promise. Some methods might return an observable as part of the promise response.
export class UmbTemplateRepository implements UmbTreeRepository, UmbDetailRepository<Template> {


	#init;
	#host: UmbControllerHostInterface;

	#treeDataSource: TemplateTreeServerDataSource;
	#detailDataSource: UmbTemplateDetailServerDataSource;

	#treeStore?: UmbTemplateTreeStore;
	#detailStore?: UmbTemplateDetailStore;

	#notificationService?: UmbNotificationService;


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

			new UmbContextConsumerController(this.#host, UMB_NOTIFICATION_SERVICE_CONTEXT_TOKEN, (instance) => {
				this.#notificationService = instance;
			}),
		]);
	}



	// TREE:


	async requestRootItems() {
		await this.#init;

		const { data, error } = await this.#treeDataSource.getRootItems();

		if (data) {
			this.#treeStore?.appendItems(data.items);
		}

		return { data, error };
	}

	async requestChildrenOf(parentKey: string | null) {
		await this.#init;

		if (!parentKey) {
			throw new Error('Parent key is missing');
		}

		const { data, error } = await this.#treeDataSource.getChildrenOf(parentKey);

		if (data) {
			this.#treeStore?.appendItems(data.items);
		}

		return { data, error };
	}

	async requestItems(keys: Array<string>) {
		await this.#init;

		if (!keys) {
			throw new Error('Keys is missing');
		}

		const { data, error } = await this.#treeDataSource.getItems(keys);

		return { data, error };
	}

	async rootItems() {
		await this.#init;
		return this.#treeStore!.rootItems();
	}

	async childrenOf(parentKey: string | null) {
		await this.#init;
		return this.#treeStore!.childrenOf(parentKey);
	}

	async items(keys: Array<string>) {
		await this.#init;
		return this.#treeStore!.items(keys);
	}








	// DETAILS:

	async createDetailsScaffold(parentKey: string | null) {
		await this.#init;

		if (!parentKey) {
			throw new Error('Parent key is missing');
		}

		return this.#detailDataSource.createScaffold(parentKey);
	}

	async requestDetails(key: string) {
		await this.#init;

		// TODO: should we show a notification if the key is missing?
		// Investigate what is best for Acceptance testing, cause in that perspective a thrown error might be the best choice?
		if (!key) {
			const error: ProblemDetails = { title: 'Key is missing' };
			return { error };
		}
		const { data, error } = await this.#detailDataSource.get(key);

		if (data) {
			this.#detailStore?.append(data);
		}

		return { data, error };
	}

	async create(template: Template) {
		await this.#init;

		if (!template || !template.key) {
			throw new Error('Template is missing');
		}

		const { error } = await this.#detailDataSource.insert(template);

		if (!error) {
			const notification = { data: { message: `Template created` } };
			this.#notificationService?.peek('positive', notification);
		}

		// TODO: we currently don't use the detail store for anything.
		// Consider to look up the data before fetching from the server
		this.#detailStore?.append(template);
		// TODO: Update tree store with the new item? or ask tree to request the new item?

		return { error };
	}

	async save(template: Template) {
		await this.#init;

		if (!template || !template.key) {
			throw new Error('Template is missing');
		}

		const { error } = await this.#detailDataSource.update(template);

		if (!error) {
			const notification = { data: { message: `Template saved` } };
			this.#notificationService?.peek('positive', notification);
		}

		// TODO: we currently don't use the detail store for anything.
		// Consider to look up the data before fetching from the server
		// Consider notify a workspace if a template is updated in the store while someone is editing it.
		this.#detailStore?.append(template);
		this.#treeStore?.updateItem(template.key, { name: template.name });
		// TODO: would be nice to align the stores on methods/methodNames.

		return { error };
	}

	async delete(key: string) {
		await this.#init;

		if (!key) {
			throw new Error('Template key is missing');
		}

		const { error } = await this.#detailDataSource.delete(key);

		if (!error) {
			const notification = { data: { message: `Template deleted` } };
			this.#notificationService?.peek('positive', notification);
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
