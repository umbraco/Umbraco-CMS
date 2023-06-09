import { UmbTemplateTreeStore, UMB_TEMPLATE_TREE_STORE_CONTEXT_TOKEN } from './template.tree.store.js';
import { UmbTemplateStore, UMB_TEMPLATE_STORE_CONTEXT_TOKEN } from './template.store.js';
import { UmbTemplateTreeServerDataSource } from './sources/template.tree.server.data.js';
import { UmbTemplateDetailServerDataSource } from './sources/template.detail.server.data.js';
import { UMB_TEMPLATE_ITEM_STORE_CONTEXT_TOKEN, UmbTemplateItemStore } from './template-item.store.js';
import { UmbTemplateItemServerDataSource } from './sources/template.item.server.data.js';
import { Observable } from '@umbraco-cms/backoffice/external/rxjs';
import type {
	UmbDetailRepository,
	UmbItemDataSource,
	UmbItemRepository,
	UmbTreeDataSource,
	UmbTreeRepository,
} from '@umbraco-cms/backoffice/repository';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbNotificationContext, UMB_NOTIFICATION_CONTEXT_TOKEN } from '@umbraco-cms/backoffice/notification';
import { UmbContextConsumerController } from '@umbraco-cms/backoffice/context-api';
import type {
	CreateTemplateRequestModel,
	EntityTreeItemResponseModel,
	ItemResponseModelBaseModel,
	TemplateItemResponseModel,
	TemplateResponseModel,
	UpdateTemplateRequestModel,
} from '@umbraco-cms/backoffice/backend-api';

export class UmbTemplateRepository
	implements
		UmbTreeRepository<EntityTreeItemResponseModel>,
		UmbDetailRepository<CreateTemplateRequestModel, string, UpdateTemplateRequestModel, TemplateResponseModel>,
		UmbItemRepository<TemplateItemResponseModel>
{
	#init;
	#host: UmbControllerHostElement;

	#treeDataSource: UmbTreeDataSource<EntityTreeItemResponseModel>;
	#detailDataSource: UmbTemplateDetailServerDataSource;
	#itemSource: UmbItemDataSource<TemplateItemResponseModel>;

	#itemStore?: UmbTemplateItemStore;
	#treeStore?: UmbTemplateTreeStore;
	#store?: UmbTemplateStore;

	#notificationContext?: UmbNotificationContext;

	constructor(host: UmbControllerHostElement) {
		this.#host = host;

		this.#treeDataSource = new UmbTemplateTreeServerDataSource(this.#host);
		this.#detailDataSource = new UmbTemplateDetailServerDataSource(this.#host);
		this.#itemSource = new UmbTemplateItemServerDataSource(this.#host);

		this.#init = Promise.all([
			new UmbContextConsumerController(this.#host, UMB_TEMPLATE_ITEM_STORE_CONTEXT_TOKEN, (instance) => {
				this.#itemStore = instance;
			}),

			new UmbContextConsumerController(this.#host, UMB_TEMPLATE_TREE_STORE_CONTEXT_TOKEN, (instance) => {
				this.#treeStore = instance;
			}),

			new UmbContextConsumerController(this.#host, UMB_TEMPLATE_STORE_CONTEXT_TOKEN, (instance) => {
				this.#store = instance;
			}),

			new UmbContextConsumerController(this.#host, UMB_NOTIFICATION_CONTEXT_TOKEN, (instance) => {
				this.#notificationContext = instance;
			}),
		]);
	}

	//#region TREE:
	async requestTreeRoot() {
		await this.#init;

		const data = {
			id: null,
			type: 'template-root',
			name: 'Templates',
			icon: 'umb:folder',
			hasChildren: true,
		};

		return { data };
	}

	async requestRootTreeItems() {
		await this.#init;

		const { data, error } = await this.#treeDataSource.getRootItems();
		if (data) {
			this.#treeStore?.appendItems(data.items);
		}

		return { data, error, asObservable: () => this.#treeStore!.rootItems };
	}

	async requestTreeItemsOf(parentId: string | null) {
		if (parentId === undefined) throw new Error('Parent id is missing');
		await this.#init;

		const { data, error } = await this.#treeDataSource.getChildrenOf(parentId);

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

		const { data, error } = await this.#treeDataSource.getItems(ids);

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

	async itemsLegacy(ids: Array<string | null>) {
		await this.#init;
		return this.#treeStore!.items(ids);
	}
	//#endregion

	//#region DETAILS:

	async createScaffold() {
		await this.#init;
		return this.#detailDataSource.createScaffold();
	}

	async requestById(id: string) {
		await this.#init;

		// TODO: should we show a notification if the id is missing?
		// Investigate what is best for Acceptance testing, cause in that perspective a thrown error might be the best choice?
		if (!id) {
			throw new Error('Id is missing');
		}
		const { data, error } = await this.#detailDataSource.get(id);

		if (data) {
			this.#store?.append(data);
		}

		return { data, error, asObservable: () => this.#treeStore!.items([id]) };
	}

	// ITEMS:
	async requestItems(ids: Array<string>) {
		if (!ids) throw new Error('Ids are missing');
		await this.#init;

		const { data, error } = await this.#itemSource.getItems(ids);

		if (data) {
			this.#itemStore?.appendItems(data);
		}

		return { data, error, asObservable: () => this.#itemStore!.items(ids) };
	}

	async items(uniques: string[]): Promise<Observable<ItemResponseModelBaseModel[]>> {
		throw new Error('items method is not implemented in UmbTemplateRepository');
	}

	async byId(id: string) {
		if (!id) throw new Error('Key is missing');
		await this.#init;
		return this.#store!.byId(id);
	}

	// Could potentially be general methods:

	async create(template: CreateTemplateRequestModel) {
		await this.#init;

		if (!template) {
			throw new Error('Template is missing');
		}

		const { error } = await this.#detailDataSource.insert(template);

		if (!error) {
			const notification = { data: { message: `Template created` } };
			this.#notificationContext?.peek('positive', notification);
		}

		// TODO: we currently don't use the detail store for anything.
		// Consider to look up the data before fetching from the server
		//this.#store?.append({ ...template, $type: 'EntityTreeItemResponseModel' });
		// TODO: Update tree store with the new item? or ask tree to request the new item?

		return { error };
	}

	async save(id: string, template: UpdateTemplateRequestModel) {
		if (!id) throw new Error('Id is missing');
		if (!template) throw new Error('Template is missing');

		await this.#init;

		const { error } = await this.#detailDataSource.update(id, template);

		if (!error) {
			const notification = { data: { message: `Template saved` } };
			this.#notificationContext?.peek('positive', notification);

			// TODO: we currently don't use the detail store for anything.
			// Consider to look up the data before fetching from the server
			// Consider notify a workspace if a template is updated in the store while someone is editing it.
			// TODO: would be nice to align the stores on methods/methodNames.
			//this.#store?.append(template);
			this.#treeStore?.updateItem(id, template);
		}

		return { error };
	}

	// General:

	async delete(id: string) {
		await this.#init;

		if (!id) {
			throw new Error('Template id is missing');
		}

		const { error } = await this.#detailDataSource.delete(id);

		if (!error) {
			const notification = { data: { message: `Template deleted` } };
			this.#notificationContext?.peek('positive', notification);
		}

		// TODO: we currently don't use the detail store for anything.
		// Consider to look up the data before fetching from the server.
		// Consider notify a workspace if a template is deleted from the store while someone is editing it.
		this.#store?.remove([id]);
		this.#treeStore?.removeItem(id);

		return { error };
	}
}
