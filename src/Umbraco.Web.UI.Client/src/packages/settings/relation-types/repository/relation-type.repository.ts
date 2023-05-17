import { UmbRelationTypeTreeStore, UMB_RELATION_TYPE_TREE_STORE_CONTEXT_TOKEN } from './relation-type.tree.store';
import { UmbRelationTypeServerDataSource } from './sources/relation-type.server.data';
import { UmbRelationTypeStore, UMB_RELATION_TYPE_STORE_CONTEXT_TOKEN } from './relation-type.store';
import { UmbRelationTypeTreeServerDataSource } from './sources/relation-type.tree.server.data';
import { UmbRelationTypeTreeDataSource } from './sources';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextConsumerController } from '@umbraco-cms/backoffice/context-api';
import {
	CreateRelationTypeRequestModel,
	RelationTypeResponseModel,
	UpdateRelationTypeRequestModel,
} from '@umbraco-cms/backoffice/backend-api';
import { UmbDetailRepository, UmbTreeRepository } from '@umbraco-cms/backoffice/repository';
import { UmbNotificationContext, UMB_NOTIFICATION_CONTEXT_TOKEN } from '@umbraco-cms/backoffice/notification';

export class UmbRelationTypeRepository
	implements
		UmbTreeRepository<any>,
		UmbDetailRepository<CreateRelationTypeRequestModel, any, UpdateRelationTypeRequestModel, RelationTypeResponseModel>
{
	#init!: Promise<unknown>;

	#host: UmbControllerHostElement;

	#treeSource: UmbRelationTypeTreeDataSource;
	#treeStore?: UmbRelationTypeTreeStore;

	#detailDataSource: UmbRelationTypeServerDataSource;
	#detailStore?: UmbRelationTypeStore;

	#notificationContext?: UmbNotificationContext;

	constructor(host: UmbControllerHostElement) {
		this.#host = host;

		// TODO: figure out how spin up get the correct data source
		this.#treeSource = new UmbRelationTypeTreeServerDataSource(this.#host);
		this.#detailDataSource = new UmbRelationTypeServerDataSource(this.#host);

		this.#init = Promise.all([
			new UmbContextConsumerController(this.#host, UMB_RELATION_TYPE_TREE_STORE_CONTEXT_TOKEN, (instance) => {
				this.#treeStore = instance;
			}),

			new UmbContextConsumerController(this.#host, UMB_RELATION_TYPE_STORE_CONTEXT_TOKEN, (instance) => {
				this.#detailStore = instance;
			}),

			new UmbContextConsumerController(this.#host, UMB_NOTIFICATION_CONTEXT_TOKEN, (instance) => {
				this.#notificationContext = instance;
			}),
		]);
	}

	// TODO: Trash
	// TODO: Move

	// TREE:
	async requestTreeRoot() {
		await this.#init;

		const data = {
			id: null,
			type: 'relation-type-root',
			name: 'Relation Types',
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

	//TODO RelationTypes can't have children. But this method is required by the tree interface.
	async requestTreeItemsOf(parentId: string | null) {
		return { data: undefined, error: { title: 'Not implemented', message: 'Not implemented' } };
	}

	async requestItemsLegacy(ids: Array<string>) {
		if (!ids) throw new Error('Ids are missing');
		await this.#init;

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

		const { data, error } = await this.#detailDataSource.get(id);

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

		const { error } = await this.#detailDataSource.insert(template);

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
			// TODO: would be nice to align the stores on methods/methodNames.
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
			// TODO: would be nice to align the stores on methods/methodNames.

			this.#detailStore?.remove([id]);
			this.#treeStore?.removeItem(id);

			const notification = { data: { message: `Relation Type deleted` } };
			this.#notificationContext?.peek('positive', notification);
		}

		return { error };
	}
}
