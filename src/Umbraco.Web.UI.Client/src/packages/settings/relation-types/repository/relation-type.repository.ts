import { UmbRelationTypeTreeStore, UMB_RELATION_TYPE_TREE_STORE_CONTEXT } from '../tree/index.js';
import { UmbRelationTypeServerDataSource } from './sources/relation-type.server.data.js';
import { UmbRelationTypeStore, UMB_RELATION_TYPE_STORE_CONTEXT_TOKEN } from './relation-type.store.js';
import { type UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbBaseController } from '@umbraco-cms/backoffice/class-api';
import {
	CreateRelationTypeRequestModel,
	RelationTypeResponseModel,
	UpdateRelationTypeRequestModel,
} from '@umbraco-cms/backoffice/backend-api';
import { UmbDetailRepository } from '@umbraco-cms/backoffice/repository';
import { UmbNotificationContext, UMB_NOTIFICATION_CONTEXT_TOKEN } from '@umbraco-cms/backoffice/notification';
import { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export class UmbRelationTypeRepository
	extends UmbBaseController
	implements
		UmbDetailRepository<CreateRelationTypeRequestModel, any, UpdateRelationTypeRequestModel, RelationTypeResponseModel>,
		UmbApi
{
	#init!: Promise<unknown>;

	#treeStore?: UmbRelationTypeTreeStore;

	#detailDataSource: UmbRelationTypeServerDataSource;
	#detailStore?: UmbRelationTypeStore;

	#notificationContext?: UmbNotificationContext;

	constructor(host: UmbControllerHost) {
		super(host);

		// TODO: figure out how spin up get the correct data source
		this.#detailDataSource = new UmbRelationTypeServerDataSource(this._host);

		this.#init = Promise.all([
			this.consumeContext(UMB_RELATION_TYPE_TREE_STORE_CONTEXT, (instance) => {
				this.#treeStore = instance;
			}).asPromise(),

			this.consumeContext(UMB_RELATION_TYPE_STORE_CONTEXT_TOKEN, (instance) => {
				this.#detailStore = instance;
			}).asPromise(),

			this.consumeContext(UMB_NOTIFICATION_CONTEXT_TOKEN, (instance) => {
				this.#notificationContext = instance;
			}).asPromise(),
		]);
	}

	// TODO: Trash
	// TODO: Move

	// DETAILS:

	async createScaffold(parentId: string | null) {
		if (parentId === undefined) throw new Error('Parent id is missing');
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
			this.#detailStore?.append(data);
		}

		return { data, error };
	}

	async byId(id: string) {
		await this.#init;
		return this.#detailStore!.byId(id);
	}

	// Could potentially be general methods:

	async create(template: CreateRelationTypeRequestModel) {
		if (!template) throw new Error('Template is missing');
		if (!template.id) throw new Error('Template id is missing');

		await this.#init;

		const { error } = await this.#detailDataSource.create(template);

		if (!error) {
			// TODO: we currently don't use the detail store for anything.
			// Consider to look up the data before fetching from the server
			// TODO: Update tree store with the new item? or ask tree to request the new item?
			// this.#detailStore?.append(template);

			const notification = { data: { message: `Relation Type created` } };
			this.#notificationContext?.peek('positive', notification);
		}

		return { error };
	}

	async save(id: string, item: UpdateRelationTypeRequestModel) {
		if (!id) throw new Error('Id is missing');
		if (!item) throw new Error('Relation type is missing');

		await this.#init;

		const { error } = await this.#detailDataSource.update(id, item);

		if (!error) {
			// TODO: we currently don't use the detail store for anything.
			// Consider to look up the data before fetching from the server
			// Consider notify a workspace if a template is updated in the store while someone is editing it.
			// eslint-disable-next-line @typescript-eslint/ban-ts-comment
			// @ts-ignore
			this.#detailStore?.append(item);
			this.#treeStore?.updateItem(id, { name: item.name });

			const notification = { data: { message: `Relation Type saved` } };
			this.#notificationContext?.peek('positive', notification);
		}

		return { error };
	}

	// General:

	async delete(id: string) {
		if (!id) throw new Error('Id is missing');

		await this.#init;

		const { error } = await this.#detailDataSource.delete(id);

		if (!error) {
			// TODO: we currently don't use the detail store for anything.
			// Consider to look up the data before fetching from the server.
			// Consider notify a workspace if a template is deleted from the store while someone is editing it.

			this.#detailStore?.removeItem(id);
			this.#treeStore?.removeItem(id);

			const notification = { data: { message: `Relation Type deleted` } };
			this.#notificationContext?.peek('positive', notification);
		}

		return { error };
	}
}
