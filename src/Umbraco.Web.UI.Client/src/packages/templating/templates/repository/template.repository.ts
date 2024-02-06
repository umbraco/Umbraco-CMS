import type { UmbTemplateTreeStore} from '../tree/index.js';
import { UMB_TEMPLATE_TREE_STORE_CONTEXT } from '../tree/index.js';
import type { UmbTemplateStore} from './template.store.js';
import { UMB_TEMPLATE_STORE_CONTEXT } from './template.store.js';
import { UmbTemplateDetailServerDataSource } from './sources/template.detail.server.data.js';
import type { UmbTemplateItemStore } from './template-item.store.js';
import { UMB_TEMPLATE_ITEM_STORE_CONTEXT } from './template-item.store.js';
import { UmbTemplateItemServerDataSource } from './sources/template.item.server.data.js';
import { UmbTemplateQueryBuilderServerDataSource } from './sources/template.query-builder.server.data.js';
import type { UmbItemDataSource, UmbItemRepository } from '@umbraco-cms/backoffice/repository';
import { UmbBaseController } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbNotificationContext} from '@umbraco-cms/backoffice/notification';
import { UMB_NOTIFICATION_CONTEXT } from '@umbraco-cms/backoffice/notification';
import type {
	CreateTemplateRequestModel,
	TemplateItemResponseModel,
	TemplateQueryExecuteModel,
	UpdateTemplateRequestModel,
} from '@umbraco-cms/backoffice/backend-api';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export class UmbTemplateRepository
	extends UmbBaseController
	implements UmbItemRepository<TemplateItemResponseModel>, UmbApi
{
	#init;

	#detailDataSource: UmbTemplateDetailServerDataSource;
	#itemSource: UmbItemDataSource<TemplateItemResponseModel>;

	#itemStore?: UmbTemplateItemStore;
	#treeStore?: UmbTemplateTreeStore;
	#store?: UmbTemplateStore;

	#notificationContext?: UmbNotificationContext;
	#queryBuilderSource: UmbTemplateQueryBuilderServerDataSource;

	constructor(host: UmbControllerHost) {
		super(host);

		this.#detailDataSource = new UmbTemplateDetailServerDataSource(this);
		this.#itemSource = new UmbTemplateItemServerDataSource(this);
		this.#queryBuilderSource = new UmbTemplateQueryBuilderServerDataSource(this);

		this.#init = Promise.all([
			this.consumeContext(UMB_TEMPLATE_ITEM_STORE_CONTEXT, (instance) => {
				this.#itemStore = instance;
			}),

			this.consumeContext(UMB_TEMPLATE_TREE_STORE_CONTEXT, (instance) => {
				this.#treeStore = instance;
			}),

			this.consumeContext(UMB_TEMPLATE_STORE_CONTEXT, (instance) => {
				this.#store = instance;
			}),

			this.consumeContext(UMB_NOTIFICATION_CONTEXT, (instance) => {
				this.#notificationContext = instance;
			}),
		]);
	}

	//#region DETAILS:

	// eslint-disable-next-line @typescript-eslint/ban-ts-comment
	// @ts-ignore
	async createScaffold(parentId: string | null) {
		await this.#init;
		return this.#detailDataSource.createScaffold(parentId);
	}

	async requestById(id: string) {
		await this.#init;

		// TODO: should we show a notification if the id is missing?
		// Investigate what is best for Acceptance testing, cause in that perspective a thrown error might be the best choice?
		if (!id) {
			throw new Error('Id is missing');
		}
		const { data, error } = await this.#detailDataSource.read(id);

		if (data) {
			this.#store?.append(data);
		}

		return { data, error, asObservable: () => this.#treeStore!.items([id]) };
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

		const { error } = await this.#detailDataSource.create(template);

		if (!error) {
			const notification = { data: { message: `Template created` } };
			this.#notificationContext?.peek('positive', notification);
		}

		// TODO: we currently don't use the detail store for anything.
		// Consider to look up the data before fetching from the server
		//this.#store?.append({ ...template });
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

	//#endregion

	//#region TEMPLATE_QUERY:

	async getTemplateQuerySettings() {
		await this.#init;
		return this.#queryBuilderSource.getTemplateQuerySettings();
	}

	async postTemplateQueryExecute({ requestBody }: { requestBody?: TemplateQueryExecuteModel }) {
		await this.#init;
		return this.#queryBuilderSource.postTemplateQueryExecute({ requestBody });
	}

	//#endregion

	//#region ITEMS:

	async requestItems(ids: Array<string>) {
		if (!ids) throw new Error('Ids are missing');
		await this.#init;

		const { data, error } = await this.#itemSource.getItems(ids);

		if (data) {
			this.#itemStore?.appendItems(data);
		}

		return { data, error, asObservable: () => this.#itemStore!.items(ids) };
	}

	// eslint-disable-next-line @typescript-eslint/ban-ts-comment
	// @ts-ignore
	async items(uniques: string[]): any {
		throw new Error('items method is not implemented in UmbTemplateRepository');
	}

	//#endregion
}
